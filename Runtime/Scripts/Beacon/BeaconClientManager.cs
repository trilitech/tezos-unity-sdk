#region

using System;
using System.IO;
using System.Threading.Tasks;
using Beacon.Sdk;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.BeaconClients.Abstract;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

#endregion

namespace TezosSDK.Beacon
{

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

				_eventDispatcher.DispatchHandshakeEvent(BeaconDappClient);
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

					var permissionsString = string.Join(", ", permissionResponse.Scopes);

					Logger.LogDebug(
						$"\"{BeaconDappClient.AppName}\" received permissions: \"{permissionsString}\", " +
						$"from: \"{permissionResponse.AppMetadata.Name}\", " +
						$"with public key: \"{permissionResponse.PublicKey}\"");

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
				Logger.LogWarning("No active wallet");
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

}