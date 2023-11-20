#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

	public class BeaconConnectorDotNet : IBeaconConnector, IDisposable
	{
		private DAppMetadata dAppMetadata;

		private WalletEventManager eventManager;
		private string network;
		private string rpc;

		private DappBeaconClient BeaconDappClient { get; set; }

		public void Dispose()
		{
			BeaconDappClient.Disconnect();
		}

		private string GetDbPath()
		{
			return Path.Combine(Application.persistentDataPath, "beacon.db");
		}

		private BeaconOptions CreateBeaconOptions(string pathToDb)
		{
			return new BeaconOptions
			{
				AppName = dAppMetadata.Name,
				AppUrl = dAppMetadata.Url,
				IconUrl = dAppMetadata.Icon,
				KnownRelayServers = Constants.KnownRelayServers,
				DatabaseConnectionString = $"Filename={pathToDb};Connection=direct;Upgrade=true"
			};
		}
		
		private void InitializeBeaconClient(BeaconOptions options)
		{
			BeaconDappClient =
				BeaconClientFactory.Create<IDappBeaconClient>(options, new MyLoggerProvider()) as DappBeaconClient;

			BeaconDappClient.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;
		}
		
		private async void ConnectAndHandleAccount()
		{
			try
			{
				await BeaconDappClient.InitAsync();
				Logger.LogInfo($"Dapp initialized: {BeaconDappClient.LoggedIn}");
				BeaconDappClient.Connect();
				Logger.LogInfo($"Dapp connected: {BeaconDappClient.Connected}");

				HandleActiveAccount();
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
				var permissionsString = activeAccountPermissions.Scopes.Aggregate(string.Empty,
					(res, scope) => res + $"{scope}, ") ?? string.Empty;

				Logger.LogInfo(
					$"We have active peer {activeAccountPermissions.AppMetadata.Name} with permissions {permissionsString}");

				var accountConnectedEvent = CreateAccountConnectedEvent(activeAccountPermissions);

				UnityMainThreadDispatcher.Enqueue(() => eventManager.HandleEvent(JsonUtility.ToJson(accountConnectedEvent)));
			}
			else
			{
				HandleMissingActiveAccount();
			}
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
		
		private UnifiedEvent CreateAccountConnectedEvent(PermissionInfo activeAccountPermissions)
		{
			var accountInfo = CreateAccountInfo(activeAccountPermissions);

			return new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeAccountConnected,
				Data = JsonUtility.ToJson(accountInfo)
			};
		}
		
		private void HandleMissingActiveAccount()
		{
			TriggerHandshakeEvent();
		}

		private void TriggerHandshakeEvent()
		{
			var handshakeData = new HandshakeData
			{
				PairingData = BeaconDappClient.GetPairingRequestInfo()
			};

			var handshakeEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeHandshakeReceived,
				Data = JsonUtility.ToJson(handshakeData)
			};

			UnityMainThreadDispatcher.Enqueue(() => eventManager.HandleEvent(JsonUtility.ToJson(handshakeEvent)));
		}

		public void ConnectAccount()
		{
			if (BeaconDappClient != null)
			{
				return;
			}

			var pathToDb = GetDbPath();
			Logger.LogDebug($"DB file stored in {pathToDb}");

			var options = CreateBeaconOptions(pathToDb);
			InitializeBeaconClient(options);
			
			ConnectAndHandleAccount();
		}

		public string GetActiveAccountAddress()
		{
			return BeaconDappClient?.GetActiveAccount()?.Address ?? string.Empty;
		}

		public void DisconnectAccount()
		{
			var activeAccount = BeaconDappClient.GetActiveAccount();

			if (activeAccount == null)
			{
				Logger.LogError("No active account");
				return;
			}

			var pubKey = PubKey.FromBase58(activeAccount.PublicKey);

			var accountInfo = new AccountInfo
			{
				Address = pubKey.Address,
				PublicKey = pubKey.ToString()
			};

			var disconnectEvent = new UnifiedEvent
			{
				EventType = WalletEventManager.EventTypeAccountDisconnected,
				Data = JsonUtility.ToJson(accountInfo)
			};

			BeaconDappClient.RemoveActiveAccounts();
			
			TriggerHandshakeEvent();
			UnityMainThreadDispatcher.Enqueue(() => eventManager.HandleEvent(JsonUtility.ToJson(disconnectEvent)));
		}

		public void InitWalletProvider(
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata dAppMetadata)
		{
			this.network = network;
			this.rpc = rpc;
			this.dAppMetadata = dAppMetadata;
		}

		public void SetWalletMessageReceiver(WalletEventManager event_manager)
		{
			eventManager = event_manager;
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
				Name = this.network,
				RpcUrl = rpc
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

		private async void OnBeaconDappClientMessageReceived(object sender, BeaconMessageEventArgs e)
		{
			if (e.PairingDone)
			{
				var pairingDoneData = new PairingDoneData
				{
					DAppPublicKey = BeaconDappClient.GetActiveAccount()?.PublicKey,
					Timestamp = DateTime.UtcNow.ToString("o") // ISO 8601 format
				};

				var pairingDoneEvent = new UnifiedEvent
				{
					EventType = WalletEventManager.EventTypePairingDone,
					Data = JsonUtility.ToJson(pairingDoneData)
				};

				eventManager.HandleEvent(JsonUtility.ToJson(pairingDoneEvent));
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

					var pubKey = PubKey.FromBase58(permissionResponse.PublicKey);

					var accountInfo = new AccountInfo
					{
						Address = pubKey.Address,
						PublicKey = permissionResponse.PublicKey
					};

					var accountConnectedEvent = new UnifiedEvent
					{
						EventType = WalletEventManager.EventTypeAccountConnected,
						Data = JsonUtility.ToJson(accountInfo)
					};

					UnityMainThreadDispatcher.Enqueue(() =>
						eventManager.HandleEvent(JsonUtility.ToJson(accountConnectedEvent)));

					break;
				}

				case BeaconMessageType.operation_response:
				{
					if (message is not OperationResponse operationResponse)
					{
						return;
					}

					Logger.LogDebug($"Received operation with hash {operationResponse.TransactionHash}");

					var operationResult = new OperationResult
					{
						TransactionHash = operationResponse.TransactionHash
					};

					var contractEvent = new UnifiedEvent
					{
						EventType = WalletEventManager.EventTypeContractCallInjected,
						Data = JsonUtility.ToJson(operationResult)
					};

					UnityMainThreadDispatcher.Enqueue(() =>
						eventManager.HandleEvent(JsonUtility.ToJson(contractEvent)));

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

					var signResult = new SignResult
					{
						Signature = signPayloadResponse.Signature
					};

					var signedEvent = new UnifiedEvent
					{
						EventType = WalletEventManager.EventTypePayloadSigned,
						Data = JsonUtility.ToJson(signResult)
					};

					UnityMainThreadDispatcher.Enqueue(() => eventManager.HandleEvent(JsonUtility.ToJson(signedEvent)));

					break;
				}
			}
		}

		public void OnReady()
		{
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