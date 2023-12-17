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

	public class EventDispatcher
	{
		private readonly WalletEventManager _eventManager;

		public EventDispatcher(WalletEventManager eventManager)
		{
			_eventManager = eventManager;
		}

		public void DispatchAccountDisconnectedEvent(DappBeaconClient beaconDappClient)
		{
			var accountDisconnectedEvent = CreateAccountDisconnectedEvent(beaconDappClient.GetActiveAccount());
			DispatchEvent(accountDisconnectedEvent);
		}

		private UnifiedEvent CreateAccountDisconnectedEvent(PermissionInfo getActiveAccount)
		{
			var accountInfo = CreateAccountInfo(getActiveAccount);

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
			var accountInfo = CreateAccountInfo(activeAccountPermissions);

			return new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeAccountConnected,
				Data = JsonUtility.ToJson(accountInfo)
			};
		}

		private AccountInfo CreateAccountInfo(PermissionInfo activeAccountPermissions)
		{
			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			return new AccountInfo
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
		private DappBeaconClient _beaconDappClient;
		private WalletProviderInfo _walletProviderInfo;

		public BeaconClientManager(EventDispatcher eventDispatcher, WalletProviderInfo walletProviderInfo)
		{
			_eventDispatcher = eventDispatcher;
			_walletProviderInfo = walletProviderInfo;
		}

		public DappBeaconClient BeaconDappClient
		{
			get => _beaconDappClient;
		}

		public void Dispose()
		{
			_beaconDappClient?.Disconnect();
		}

		public void ConnectAccount()
		{
			if (_beaconDappClient != null)
			{
				return;
			}

			_beaconDappClient = CreateAndInitializeBeaconClient();
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
				await _beaconDappClient.InitAsync();
				Logger.LogInfo($"Dapp initialized: {_beaconDappClient.LoggedIn}");
				_beaconDappClient.Connect();
				Logger.LogInfo($"Dapp connected: {_beaconDappClient.Connected}");
			}
			catch (Exception e)
			{
				Logger.LogError($"Error during account connection: {e}");
			}
		}

		private void HandleActiveAccount()
		{
			var activeAccountPermissions = _beaconDappClient.GetActiveAccount();

			if (activeAccountPermissions != null)
			{
				var permissionsString = activeAccountPermissions.Scopes.Aggregate(string.Empty,
					(res, scope) => res + $"{scope}, ") ?? string.Empty;

				Logger.LogInfo($"We have active peer with \"{activeAccountPermissions.AppMetadata.Name}\"");
				Logger.LogInfo($"Permissions: {permissionsString}");

				_eventDispatcher.DispatchAccountConnectedEvent(_beaconDappClient);
			}
			else
			{
				_eventDispatcher.DispatchHandshakeEvent(_beaconDappClient);
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
				_eventDispatcher.DispathPairingDoneEvent(_beaconDappClient);
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
						$"{_beaconDappClient.AppName} received permissions {permissionsString} from {permissionResponse.AppMetadata.Name} with public key {permissionResponse.PublicKey}");

					_eventDispatcher.DispatchAccountConnectedEvent(_beaconDappClient);

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
						await _beaconDappClient.PermissionInfoRepository.TryReadBySenderIdAsync(signPayloadResponse
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

		private void InitializeBeaconClient(BeaconOptions options)
		{
			_beaconDappClient =
				BeaconClientFactory.Create<IDappBeaconClient>(options, new MyLoggerProvider()) as DappBeaconClient;

			_beaconDappClient.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;
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

		public void DisconnectAccount()
		{
			var activeAccount = _beaconDappClient.GetActiveAccount();

			if (activeAccount == null)
			{
				Logger.LogError("No active account");
				return;
			}

			_beaconDappClient.RemoveActiveAccounts();
			_eventDispatcher.DispatchAccountDisconnectedEvent(_beaconDappClient);
		}

		public string GetActiveAccountAddress()
		{
			return _beaconDappClient?.GetActiveAccount()?.Address ?? string.Empty;
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
		private readonly WalletProviderInfo _walletProviderInfo;

		public BeaconConnectorDotNet(
			WalletEventManager eventManager,
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata dAppMetadata)
		{
			_walletProviderInfo = new WalletProviderInfo
			{
				Network = network,
				Rpc = rpc,
				WalletProviderType = walletProviderType,
				Metadata = dAppMetadata
			};

			_eventDispatcher = new EventDispatcher(eventManager);
			_beaconClientManager = new BeaconClientManager(_eventDispatcher, _walletProviderInfo);
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
			_beaconClientManager.DisconnectAccount();
		}

		public async void RequestTezosPermission(string networkName = "", string networkRPC = "")
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
				KeyPairService.CreateGuid(), BeaconDappClient.SenderId, BeaconDappClient.Metadata, network,
				permissionScopes);

			var activePeer = BeaconDappClient.GetActivePeer();

			if (activePeer != null)
			{
				await BeaconDappClient.SendResponseAsync(activePeer.SenderId, permissionRequest);
				Logger.LogInfo("Permission request sent");
			}
			else
			{
				Logger.LogError("No active peer found");
			}
		}

		public async void RequestTezosOperation(
			string destination,
			string entryPoint = "default",
			string arg = null,
			ulong amount = 0,
			string networkName = "",
			string networkRPC = "")
		{
			var activeAccountPermissions = BeaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				Logger.LogError("No active permissions");
				return;
			}

			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			var operationDetails = new List<TezosBaseOperation>();

			var partialTezosTransactionOperation = new PartialTezosTransactionOperation(amount.ToString(), destination,
				new JObject
				{
					["entrypoint"] = entryPoint,
					["value"] = JToken.Parse(arg)
				});

			operationDetails.Add(partialTezosTransactionOperation);

			var operationRequest = new OperationRequest(BeaconMessageType.operation_request, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), BeaconDappClient.SenderId, activeAccountPermissions.Network,
				operationDetails, pubKey.Address);

			Logger.LogDebug("requesting operation: " + operationRequest);
			await BeaconDappClient.SendResponseAsync(activeAccountPermissions.SenderId, operationRequest);
		}

		public async void RequestContractOrigination(string script, string delegateAddress)
		{
			var activeAccountPermissions = BeaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				Logger.LogError("No active permissions");
				return;
			}

			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			var operationDetails = new List<TezosBaseOperation>();

			var partialTezosTransactionOperation = new PartialTezosOriginationOperation("0",
				JObject.Parse(script), delegateAddress);

			operationDetails.Add(partialTezosTransactionOperation);

			var operationRequest = new OperationRequest(BeaconMessageType.operation_request, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), BeaconDappClient.SenderId, activeAccountPermissions.Network,
				operationDetails, pubKey.Address);

			Logger.LogDebug("requesting operation: " + operationRequest);
			await BeaconDappClient.SendResponseAsync(activeAccountPermissions.SenderId, operationRequest);
		}

		public void RequestTezosSignPayload(SignPayloadType signingType, string payload)
		{
			BeaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signingType, payload), signingType);
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

		private void HandleActiveAccount()
		{
			var activeAccountPermissions = BeaconDappClient.GetActiveAccount();

			if (activeAccountPermissions != null)
			{
				var permissionsString = activeAccountPermissions.Scopes.Aggregate(string.Empty,
					(res, scope) => res + $"{scope}, ") ?? string.Empty;

				Logger.LogInfo(
					$"We have active peer with {activeAccountPermissions.AppMetadata.Name} with permissions {permissionsString}");

				_eventDispatcher.DispatchAccountConnectedEvent(BeaconDappClient);
			}
			else
			{
				HandleMissingActiveAccount();
			}
		}

		private void HandleMissingActiveAccount()
		{
			_eventDispatcher.DispatchHandshakeEvent(BeaconDappClient);
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