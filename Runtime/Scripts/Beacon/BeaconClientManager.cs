#region

using System;
using System.IO;
using Beacon.Sdk;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.BeaconClients.Abstract;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

#endregion

namespace TezosSDK.Beacon
{

	public class BeaconClientManager : IDisposable
	{
		private readonly EventDispatcher _eventDispatcher;
		private bool _pairingDone;
		private WalletProviderInfo _walletProviderInfo;

		public BeaconClientManager(EventDispatcher eventDispatcher)
		{
			_eventDispatcher = eventDispatcher;
		}

		public DappBeaconClient BeaconDappClient { get; private set; }

		public void Dispose()
		{
			BeaconDappClient?.Disconnect();
		}

		public async void InitAsync()
		{
			try
			{
				if (BeaconDappClient == null)
				{
					Logger.LogError("BeaconDappClient is null - Call CreateBeaconClient() first!");
					return;
				}

				await BeaconDappClient.InitAsync();
				Logger.LogDebug($"Dapp initialized: {BeaconDappClient.LoggedIn}");
				_eventDispatcher.DispatchHandshakeEvent(BeaconDappClient.GetPairingRequestInfo());
			}
			catch (Exception e)
			{
				Logger.LogError($"Error during dapp initialization: {e.Message}");
			}
		}

		public void ConnectDappClient()
		{
			try
			{
				if (BeaconDappClient == null)
				{
					Logger.LogError("BeaconDappClient is null - Call CreateBeaconClient() first!");
					return;
				}

				var activeAccountPermissions = BeaconDappClient.GetActiveAccount();
				var activePeer = BeaconDappClient.GetActivePeer();

				Logger.LogDebug(
					$"ConnectDappClient - activeAccountPermissions: {activeAccountPermissions}, activePeer: {activePeer}");

				BeaconDappClient.Connect();
				Logger.LogInfo($"ConnectDappClient - Dapp connected: {BeaconDappClient.Connected}");

				activeAccountPermissions = BeaconDappClient.GetActiveAccount();
				activePeer = BeaconDappClient.GetActivePeer();

				Logger.LogDebug(
					$"ConnectDappClient - activeAccountPermissions: {activeAccountPermissions}, activePeer: {activePeer}");
			}
			catch (Exception e)
			{
				Logger.LogError($"Error during dapp connection: {e.Message}");
			}
		}

		private void HandleActiveAccount()
		{
			var activeAccountPermissions = BeaconDappClient.GetActiveAccount();
			var activeAccountPermissions2 = BeaconDappClient.GetActivePeer();

			if (activeAccountPermissions == null)
			{
				Logger.LogWarning("No active account");
				return;
			}

			var permissionsString = string.Join(", ", activeAccountPermissions.Scopes);

			Logger.LogInfo($"We already have active peer with \"{activeAccountPermissions.AppMetadata.Name}\"");
			Logger.LogInfo($"Permissions: {permissionsString}");

			_eventDispatcher.DispatchAccountConnectedEvent(BeaconDappClient);
		}

		public void Create()
		{
			var options = CreateBeaconOptions();

			BeaconDappClient =
				BeaconClientFactory.Create<IDappBeaconClient>(options, new MyLoggerProvider()) as DappBeaconClient;

			if (BeaconDappClient == null)
			{
				throw new Exception("Failed to create Beacon client");
			}

			BeaconDappClient.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;
			BeaconDappClient.OnDisconnected += OnBeaconDappClientDisconnected;
		}

		private void OnBeaconDappClientDisconnected()
		{
			Logger.LogDebug("OnBeaconDappClientDisconnected - Dapp disconnected");
			// _eventDispatcher.DispatchWalletDisconnectedEvent(BeaconDappClient.GetActiveAccount());
		}

		private async void OnBeaconDappClientMessageReceived(object sender, BeaconMessageEventArgs e)
		{
			if (e == null)
			{
				return;
			}

			Logger.LogDebug($"Received message of type: {e.Request?.Type}");

			if (!_pairingDone && e.PairingDone)
			{
				Logger.LogDebug("Pairing done");
				_pairingDone = true;
				_eventDispatcher.DispatchPairingDoneEvent(BeaconDappClient);
				return;
			}

			var message = e.Request;

			switch (message?.Type)
			{
				case BeaconMessageType.permission_response:
				{
					if (message is not PermissionResponse permissionResponse)
					{
						return;
					}

					var permissionsString = string.Join(", ", permissionResponse.Scopes);

					Logger.LogDebug($"\"{BeaconDappClient.AppName}\" received permissions: \"{permissionsString}\", " +
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
				Logger.LogWarning("No active wallet - nothing to disconnect");
				return;
			}

			BeaconDappClient.RemoveActiveAccounts();
			_eventDispatcher.DispatchWalletDisconnectedEvent(activeWallet);
		}

		public string GetActiveAccountAddress()
		{
			return BeaconDappClient?.GetActiveAccount()?.Address ?? string.Empty;
		}

		public void SetWalletProviderInfo(WalletProviderInfo info)
		{
			_walletProviderInfo = info;
		}
	}

}