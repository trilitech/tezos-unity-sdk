#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beacon.Sdk;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.BeaconClients.Abstract;
using Beacon.Sdk.Core.Domain.Entities;
using Beacon.Sdk.Core.Domain.Services;
using Microsoft.Extensions.Logging;
using Netezos.Keys;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using BeaconNetwork = Beacon.Sdk.Beacon.Permission.Network;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = TezosSDK.Helpers.Logger;

#endregion

namespace TezosSDK.Beacon
{

	public class OperationRequestHandler
	{
		private readonly WalletProviderInfo _walletProviderInfo;

		public OperationRequestHandler(WalletProviderInfo walletProviderInfo)
		{
			_walletProviderInfo = walletProviderInfo;
		}

		public async Task RequestTezosPermission(
			string networkName,
			string networkRPC,
			DappBeaconClient beaconDappClient)
		{
			if (!Enum.TryParse(networkName, out NetworkType networkType))
			{
				networkType = TezosConfig.Instance.Network;
			}

			var network = new BeaconNetwork
			{
				Type = networkType,
				Name = _walletProviderInfo.Network,
				RpcUrl = _walletProviderInfo.Rpc
			};

			var permissionScopes = new List<PermissionScope>
			{
				PermissionScope.operation_request,
				PermissionScope.sign
			};

			var permissionRequest = new PermissionRequest(BeaconMessageType.permission_request, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), beaconDappClient.SenderId, beaconDappClient.Metadata, network,
				permissionScopes);

			var activePeer = beaconDappClient.GetActivePeer();

			if (activePeer != null)
			{
				await beaconDappClient.SendResponseAsync(activePeer.SenderId, permissionRequest);
				Logger.LogInfo("Permission request sent");
			}
			else
			{
				Logger.LogError("No active peer found");
			}
		}

		public Task RequestTezosOperation(
			string destination,
			string entryPoint,
			string input,
			ulong amount,
			string networkName,
			string networkRPC,
			DappBeaconClient beaconDappClient)
		{
			var activeAccountPermissions = beaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				Logger.LogError("No active permissions");
				return Task.CompletedTask;
			}

			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			var operationDetails = new List<TezosBaseOperation>();

			var partialTezosTransactionOperation = new PartialTezosTransactionOperation(amount.ToString(), destination,
				new JObject
				{
					["entrypoint"] = entryPoint,
					["value"] = JToken.Parse(input)
				});

			operationDetails.Add(partialTezosTransactionOperation);

			var operationRequest = new OperationRequest(BeaconMessageType.operation_request, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), beaconDappClient.SenderId, activeAccountPermissions.Network,
				operationDetails, pubKey.Address);

			Logger.LogDebug("requesting operation: " + operationRequest);
			return beaconDappClient.SendResponseAsync(activeAccountPermissions.SenderId, operationRequest);
		}

		public Task RequestContractOrigination(string script, string delegateAddress, DappBeaconClient beaconDappClient)
		{
			var activeAccountPermissions = beaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				Logger.LogError("No active permissions");
				return Task.CompletedTask;
			}

			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			var operationDetails = new List<TezosBaseOperation>();

			var partialTezosTransactionOperation = new PartialTezosOriginationOperation("0",
				JObject.Parse(script), delegateAddress);

			operationDetails.Add(partialTezosTransactionOperation);

			var operationRequest = new OperationRequest(BeaconMessageType.operation_request, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), beaconDappClient.SenderId, activeAccountPermissions.Network,
				operationDetails, pubKey.Address);

			Logger.LogDebug("requesting operation: " + operationRequest);
			return beaconDappClient.SendResponseAsync(activeAccountPermissions.SenderId, operationRequest);
		}
	}

	public class EventDispatcher
	{
		private readonly WalletEventManager _eventManager;

		public EventDispatcher(WalletEventManager eventManager)
		{
			_eventManager = eventManager;
		}

		public void DispatchWalletDisconnectedEvent(PermissionInfo activeWallet)
		{
			var walletDisconnectedEvent = CreateWalletDisconnectedEvent(activeWallet);
			DispatchEvent(walletDisconnectedEvent);
		}

		private UnifiedEvent CreateWalletDisconnectedEvent(PermissionInfo activeWallet)
		{
			var accountInfo = CreateWalletInfo(activeWallet);

			return new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeAccountDisconnected,
				Data = JsonUtility.ToJson(accountInfo)
			};
		}

		private void DispatchEvent(UnifiedEvent eventData)
		{
			var json = JsonUtility.ToJson(eventData);
			UnityMainThreadDispatcher.Enqueue(() => _eventManager.HandleEvent(json));
		}

		public void DispatchAccountConnectedEvent(DappBeaconClient beaconDappClient)
		{
			var accountConnectedEvent = CreateAccountConnectedEvent(beaconDappClient.GetActiveAccount());
			DispatchEvent(accountConnectedEvent);
		}

		private UnifiedEvent CreateAccountConnectedEvent(PermissionInfo activeAccountPermissions)
		{
			var accountInfo = CreateWalletInfo(activeAccountPermissions);

			return new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeAccountConnected,
				Data = JsonUtility.ToJson(accountInfo)
			};
		}

		private WalletInfo CreateWalletInfo(PermissionInfo activeAccountPermissions)
		{
			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			return new WalletInfo
			{
				Address = pubKey.Address,
				PublicKey = activeAccountPermissions.PublicKey
			};
		}

		public void DispathPairingDoneEvent(DappBeaconClient beaconDappClient)
		{
			var pairingDoneData = new PairingDoneData
			{
				DAppPublicKey = beaconDappClient.GetActiveAccount()?.PublicKey,
				Timestamp = DateTime.UtcNow.ToString("o")
			};

			var pairingDoneEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypePairingDone,
				Data = JsonUtility.ToJson(pairingDoneData)
			};

			DispatchEvent(pairingDoneEvent);
		}

		public void DispatchContractCallInjectedEvent(OperationResponse operationResponse)
		{
			var operationResult = new OperationResult
			{
				TransactionHash = operationResponse.TransactionHash
			};

			var contractEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeContractCallInjected,
				Data = JsonUtility.ToJson(operationResult)
			};

			DispatchEvent(contractEvent);
		}

		public void DispatchPayloadSignedEvent(SignPayloadResponse signPayloadResponse)
		{
			var signResult = new SignResult
			{
				Signature = signPayloadResponse.Signature
			};

			var signedEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypePayloadSigned,
				Data = JsonUtility.ToJson(signResult)
			};

			DispatchEvent(signedEvent);
		}

		public void DispatchHandshakeEvent(DappBeaconClient beaconDappClient)
		{
			var handshakeData = new HandshakeData
			{
				PairingData = beaconDappClient.GetPairingRequestInfo()
			};

			var handshakeEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeHandshakeReceived,
				Data = JsonUtility.ToJson(handshakeData)
			};

			DispatchEvent(handshakeEvent);
		}
	}

	public class BeaconClientManager : IDisposable
	{
		private readonly EventDispatcher _eventDispatcher;
		private WalletProviderInfo _walletProviderInfo;

		public BeaconClientManager(EventDispatcher eventDispatcher, WalletProviderInfo walletProviderInfo)
		{
			_eventDispatcher = eventDispatcher;
			_walletProviderInfo = walletProviderInfo;
		}

		public DappBeaconClient BeaconDappClient { get; private set; }

		public void Dispose()
		{
			BeaconDappClient?.Disconnect();
		}

		public void ConnectAccount()
		{
			if (BeaconDappClient != null)
			{
				return;
			}

			BeaconDappClient = CreateAndInitializeBeaconClient();
			ConnectAndHandleAccount();
		}

		private async void ConnectAndHandleAccount()
		{
			await InitAndConnectDappClientAsync();
			HandleActiveAccount();
		}

		private async Task InitAndConnectDappClientAsync()
		{
			try
			{
				await BeaconDappClient.InitAsync();
				Logger.LogDebug($"Dapp initialized: {BeaconDappClient.LoggedIn}");
				BeaconDappClient.Connect();
				Logger.LogInfo($"Dapp connected: {BeaconDappClient.Connected}");
			}
			catch (Exception e)
			{
				Logger.LogError($"Error during account connection: {e}");
			}
		}

		private void HandleActiveAccount()
		{
			var activeAccountPermissions = BeaconDappClient.GetActiveAccount();

			if (activeAccountPermissions != null)
			{
				var permissionsString = string.Join(", ", activeAccountPermissions.Scopes);

				Logger.LogInfo($"We have active peer with \"{activeAccountPermissions.AppMetadata.Name}\"");
				Logger.LogInfo($"Permissions: {permissionsString}");

				_eventDispatcher.DispatchAccountConnectedEvent(BeaconDappClient);
			}
			else
			{
				_eventDispatcher.DispatchHandshakeEvent(BeaconDappClient);
			}
		}

		private DappBeaconClient CreateAndInitializeBeaconClient()
		{
			var options = CreateBeaconOptions();

			var client =
				BeaconClientFactory.Create<IDappBeaconClient>(options, new MyLoggerProvider()) as DappBeaconClient;

			client.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;
			return client;
		}

		private async void OnBeaconDappClientMessageReceived(object sender, BeaconMessageEventArgs e)
		{
			if (e.PairingDone)
			{
				_eventDispatcher.DispathPairingDoneEvent(BeaconDappClient);
				return;
			}

			var message = e.Request;

			switch (message.Type)
			{
				case BeaconMessageType.permission_response:
				{
					if (message is not PermissionResponse permissionResponse)
					{
						return;
					}

					var permissionsString = permissionResponse.Scopes.Aggregate(string.Empty,
						(res, scope) => res + $"{scope}, ");

					Logger.LogDebug(
						$"{BeaconDappClient.AppName} received permissions {permissionsString} from {permissionResponse.AppMetadata.Name} with public key {permissionResponse.PublicKey}");

					_eventDispatcher.DispatchAccountConnectedEvent(BeaconDappClient);

					break;
				}

				case BeaconMessageType.operation_response:
				{
					if (message is not OperationResponse operationResponse)
					{
						return;
					}

					Logger.LogDebug($"Received operation with hash {operationResponse.TransactionHash}");

					_eventDispatcher.DispatchContractCallInjectedEvent(operationResponse);

					break;
				}

				case BeaconMessageType.sign_payload_response:
				{
					if (message is not SignPayloadResponse signPayloadResponse)
					{
						return;
					}

					var senderPermissions =
						await BeaconDappClient.PermissionInfoRepository.TryReadBySenderIdAsync(signPayloadResponse
							.SenderId);

					if (senderPermissions == null)
					{
						return;
					}

					_eventDispatcher.DispatchPayloadSignedEvent(signPayloadResponse);
					break;
				}
			}
		}

		private BeaconOptions CreateBeaconOptions()
		{
			return new BeaconOptions
			{
				AppName = _walletProviderInfo.Metadata.Name,
				AppUrl = _walletProviderInfo.Metadata.Url,
				IconUrl = _walletProviderInfo.Metadata.Icon,
				KnownRelayServers = Constants.KnownRelayServers,
				DatabaseConnectionString = BuildDatabaseConnectionString()
			};
		}

		private string BuildDatabaseConnectionString()
		{
			var dbPath = GetDbPath();
			return $"Filename={dbPath};Connection=direct;Upgrade=true";
		}

		private string GetDbPath()
		{
			return Path.Combine(Application.persistentDataPath, "beacon.db");
		}

		public void DisconnectWallet()
		{
			var activeWallet = BeaconDappClient.GetActiveAccount();

			if (activeWallet == null)
			{
				Logger.LogError("No active wallet");
				return;
			}

			BeaconDappClient.RemoveActiveAccounts();
			_eventDispatcher.DispatchWalletDisconnectedEvent(activeWallet);
		}

		public string GetActiveAccountAddress()
		{
			return BeaconDappClient?.GetActiveAccount()?.Address ?? string.Empty;
		}

		public void InitWalletProvider(
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata metadata)
		{
			_walletProviderInfo = new WalletProviderInfo
			{
				Network = network,
				Rpc = rpc,
				WalletProviderType = walletProviderType,
				Metadata = metadata
			};
		}
	}

	public class WalletProviderInfo
	{
		public string Network { get; set; }
		public string Rpc { get; set; }
		public WalletProviderType WalletProviderType { get; set; }
		public DAppMetadata Metadata { get; set; }
	}

	public class BeaconConnectorDotNet : IBeaconConnector, IDisposable
	{
		private readonly BeaconClientManager _beaconClientManager;
		private readonly EventDispatcher _eventDispatcher;
		private readonly OperationRequestHandler _operationRequestHandler;

		public BeaconConnectorDotNet(
			WalletEventManager eventManager,
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata dAppMetadata)
		{
			var walletProviderInfo = new WalletProviderInfo
			{
				Network = network,
				Rpc = rpc,
				WalletProviderType = walletProviderType,
				Metadata = dAppMetadata
			};

			_eventDispatcher = new EventDispatcher(eventManager);
			_beaconClientManager = new BeaconClientManager(_eventDispatcher, walletProviderInfo);
			_operationRequestHandler = new OperationRequestHandler(walletProviderInfo);
		}

		private DappBeaconClient BeaconDappClient
		{
			get => _beaconClientManager.BeaconDappClient;
		}

		public void ConnectAccount()
		{
			_beaconClientManager.ConnectAccount();
		}

		public string GetActiveAccountAddress()
		{
			return _beaconClientManager.GetActiveAccountAddress();
		}

		public void DisconnectAccount()
		{
			_beaconClientManager.DisconnectWallet();
		}

		public async void RequestTezosPermission(string networkName = "", string networkRPC = "")
		{
			await _operationRequestHandler.RequestTezosPermission(networkName, networkRPC, BeaconDappClient);
		}

		public async void RequestTezosOperation(
			string destination,
			string entryPoint = "default",
			string input = null,
			ulong amount = 0,
			string networkName = "",
			string networkRPC = "")
		{
			await _operationRequestHandler.RequestTezosOperation(destination, entryPoint, input, amount, networkName,
				networkRPC, BeaconDappClient);
		}

		public async void RequestContractOrigination(string script, string delegateAddress)
		{
			await _operationRequestHandler.RequestContractOrigination(script, delegateAddress, BeaconDappClient);
		}

		public async void RequestTezosSignPayload(SignPayloadType signingType, string payload)
		{
			await BeaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signingType, payload), signingType);
		}

		public void InitWalletProvider(
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata metadata)
		{
			_beaconClientManager.InitWalletProvider(network, rpc, walletProviderType, metadata);
		}

		public void OnReady()
		{
		}

		public void Dispose()
		{
			BeaconDappClient.Disconnect();
		}
	}

	// todo: this logger didn't work inside Beacon, improve this.
	public class MyLoggerProvider : ILoggerProvider
	{
		#region IDisposable Implementation

		public void Dispose()
		{
		}

		#endregion

		#region ILoggerProvider Implementation

		public ILogger CreateLogger(string categoryName)
		{
			return new MyLogger();
		}

		#endregion

		#region Nested Types

		public class MyLogger : ILogger
		{
			#region ILogger Implementation

			public IDisposable BeginScope<TState>(TState state)
			{
				return null;
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return true;
			}

			public void Log<TState>(
				LogLevel logLevel,
				EventId eventId,
				TState state,
				Exception exception,
				Func<TState, Exception, string> formatter)
			{
				if (exception != null)
				{
					Debug.LogException(exception);
				}

				//Debug.Log(state.ToString());
			}

			#endregion
		}

		#endregion
	}

}