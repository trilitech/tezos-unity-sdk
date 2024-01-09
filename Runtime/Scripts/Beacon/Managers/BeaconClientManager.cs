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
using TezosSDK.Beacon.Loggers;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Tezos;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Beacon
{

	public class BeaconClientManager : IDisposable
	{
		private readonly EventDispatcher _eventDispatcher;
		private WalletInfo _activeWallet; // Keep track of the active wallet

		public BeaconClientManager(WalletEventManager eventManager)
		{
			_eventDispatcher = new EventDispatcher(eventManager);
			eventManager.WalletConnected += OnWalletConnected;
			eventManager.WalletDisconnected += OnWalletDisconnected;
		}

		public DappBeaconClient BeaconDappClient { get; private set; }

		public void Dispose()
		{
			BeaconDappClient?.Disconnect();
		}

		private void OnWalletDisconnected(WalletInfo obj)
		{
			_activeWallet = null; // Reset active wallet
		}

		private void OnWalletConnected(WalletInfo wallet)
		{
			_activeWallet = wallet; // Set active wallet
		}

		public async void InitAsyncAndConnect()
		{
			try
			{
				if (BeaconDappClient == null)
				{
					Logger.LogError("BeaconDappClient is null - Call CreateBeaconClient() first!");
					return;
				}

				Logger.LogInfo("Initializing BeaconDappClient");

				await BeaconDappClient.InitAsync();
				BeaconDappClient.Connect();

				var activePeer = BeaconDappClient.GetActivePeer();

				Logger.LogInfo($"BeaconDappClient initialized. Logged in: {BeaconDappClient.LoggedIn} - " +
				               $"Connected: {BeaconDappClient.Connected} - " + $"Active peer: {activePeer?.Name}");

				if (HasExistingConnection())
				{
					_eventDispatcher.DispatchWalletConnectedEvent(BeaconDappClient);
				}
				else
				{
					var pairingRequestInfo = BeaconDappClient.GetPairingRequestInfo();
					_eventDispatcher.DispatchHandshakeEvent(pairingRequestInfo);
				}
			}
			catch (Exception e)
			{
				Logger.LogError($"Error during dapp initialization: {e.Message}");
			}
		}

		/// <summary>
		///     Checks if there is an existing connection with a wallet.
		/// </summary>
		/// <returns>Returns true if an active connection exists, otherwise false.</returns>
		private bool HasExistingConnection()
		{
			var activeAccountPermissions = BeaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				Logger.LogInfo("No already active wallet");
				return false;
			}

			Logger.LogInfo("We already have connection with wallet:");
			Logger.LogInfo(activeAccountPermissions.Print());
			return true;
		}

		/// <summary>
		///     Creates and initializes the Beacon client.
		/// </summary>
		public void Create()
		{
			Logger.LogDebug("Creating Beacon client");
			var options = CreateBeaconOptions();

			BeaconDappClient =
				BeaconClientFactory.Create<IDappBeaconClient>(options, new BeaconLoggerProvider()) as DappBeaconClient;

			if (BeaconDappClient == null)
			{
				throw new Exception("Failed to create Beacon client");
			}

			BeaconDappClient.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;
			BeaconDappClient.OnDisconnected += OnBeaconDappClientDisconnected;
			BeaconDappClient.OnConnectedClientsListChanged += OnConnectedClientsListChanged;
		}

		/// <summary>
		///     Handles the event when the list of connected clients changes.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">Null when a client disconnects</param>
		private void OnConnectedClientsListChanged(object sender, ConnectedClientsListChangedEventArgs e)
		{
			if (e != null)
			{
				Logger.LogDebug("OnConnectedClientsListChanged - Connected clients list changed");
				_eventDispatcher.DispatchWalletConnectedEvent(BeaconDappClient);
			}
			else
			{
				Logger.LogDebug("OnConnectedClientsListChanged - Connected clients list is empty");
				_eventDispatcher.DispatchWalletDisconnectedEvent(_activeWallet);
			}
		}

		/// <summary>
		///     Handles the event of the Beacon Dapp client getting disconnected.
		/// </summary>
		private void OnBeaconDappClientDisconnected()
		{
			Logger.LogDebug("OnBeaconDappClientDisconnected - Dapp disconnected");
			_eventDispatcher.DispatchWalletDisconnectedEvent(_activeWallet);
		}

		/// <summary>
		///     Receives and processes Beacon messages asynchronously.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">Event arguments containing the received Beacon message.</param>
		/// <remarks>The event is raised on a background thread and must be marshalled to the UI thread.</remarks>
		private async void OnBeaconDappClientMessageReceived(object sender, BeaconMessageEventArgs e)
		{
			if (e == null)
			{
				return;
			}

			Logger.LogDebug($"Received message of type: {e.Request?.Type}");

			if (e.PairingDone)
			{
				HandlePairingDone();
				return;
			}

			switch (e.Request?.Type)
			{
				case BeaconMessageType.permission_response:
					HandlePermissionResponse(e.Request as PermissionResponse);
					break;
				case BeaconMessageType.operation_response:
					HandleOperationResponse(e.Request as OperationResponse);
					break;
				case BeaconMessageType.sign_payload_response:
					await HandleSignPayloadResponse(e.Request as SignPayloadResponse);
					break;
				case BeaconMessageType.disconnect:
					HandleDisconnect();
					break;
			}
		}

		/// <summary>
		///     Handles a permission response message.
		/// </summary>
		/// <param name="permissionResponse">The permission response message to handle.</param>
		/// <remarks>
		///     Simply logs the permission response message.
		/// </remarks>
		private void HandlePermissionResponse(PermissionResponse permissionResponse)
		{
			if (permissionResponse == null)
			{
				return;
			}

			Logger.LogDebug(permissionResponse.Print());
		}

		/// <summary>
		///     Handles an operation response message.
		/// </summary>
		/// <param name="operationResponse">The operation response message to handle.</param>
		/// <remarks>
		///     Dispatches a contract call injected event on the main (UI) thread.
		/// </remarks>
		private void HandleOperationResponse(OperationResponse operationResponse)
		{
			if (operationResponse == null)
			{
				return;
			}

			Logger.LogDebug($"Received operation response with hash {operationResponse.TransactionHash}");
			_eventDispatcher.DispatchContractCallInjectedEvent(operationResponse);
		}

		/// <summary>
		///     Asynchronously handles a sign payload response message.
		/// </summary>
		/// <param name="signPayloadResponse">The sign payload response message to handle.</param>
		/// <remarks>
		///     Checks sender permissions and dispatches a payload signed event on the main (UI) thread.
		/// </remarks>
		private async Task HandleSignPayloadResponse(SignPayloadResponse signPayloadResponse)
		{
			if (signPayloadResponse == null)
			{
				return;
			}

			var senderPermissions =
				await BeaconDappClient.PermissionInfoRepository.TryReadBySenderIdAsync(signPayloadResponse.SenderId);

			if (senderPermissions == null)
			{
				return;
			}

			_eventDispatcher.DispatchPayloadSignedEvent(signPayloadResponse);
		}

		/// <summary>
		///     Handles a disconnect type Beacon message.
		/// </summary>
		/// <remarks>
		///     Dispatches a wallet disconnected event on the main (UI) thread.
		/// </remarks>
		private void HandleDisconnect()
		{
			_eventDispatcher.DispatchWalletDisconnectedEvent(_activeWallet);
		}

		/// <summary>
		///     Handles the 'Pairing Done' type Beacon message.
		/// </summary>
		/// <remarks>
		///     Checks for active wallet and dispatches pairing done event on the main (UI) thread, if no active wallet is found.
		///     Pairing happens either through QR code scanning or deep linking.
		/// </remarks>
		private void HandlePairingDone()
		{
			Logger.LogDebug("Received message of type Pairing done");

			if (_activeWallet != null)
			{
				Logger.LogDebug("Active wallet already exists, ignoring duplicate pairing done event");
				return;
			}

			TezosManager.Instance.BeaconConnector.RequestWalletConnection();
			_eventDispatcher.DispatchPairingCompletedEvent(BeaconDappClient);
		}

		/// <summary>
		///     Creates a new instance of BeaconOptions for the Beacon client.
		/// </summary>
		/// <returns>A configured instance of BeaconOptions.</returns>
		private BeaconOptions CreateBeaconOptions()
		{
			return new BeaconOptions
			{
				AppName = TezosManager.Instance.DAppMetadata.Name,
				AppUrl = TezosManager.Instance.DAppMetadata.Url,
				IconUrl = TezosManager.Instance.DAppMetadata.Icon,
				KnownRelayServers = Constants.KnownRelayServers,
				DatabaseConnectionString = BuildDatabaseConnectionString()
			};
		}

		/// <summary>
		///     Builds the connection string for the Beacon client's database.
		/// </summary>
		/// <returns>The constructed database connection string.</returns>
		private string BuildDatabaseConnectionString()
		{
			var dbPath = GetDbPath();
			return $"Filename={dbPath};Connection=direct;Upgrade=true";
		}

		/// <summary>
		///     Gets the path to the database used by the Beacon client.
		/// </summary>
		/// <returns>The file path to the database file.</returns>
		private string GetDbPath()
		{
			return Path.Combine(Application.persistentDataPath, "beacon.db");
		}

		/// <summary>
		///     Disconnects the wallet from the Beacon Dapp client.
		/// </summary>
		public void DisconnectWallet()
		{
			if (!BeaconDappClient.Connected)
			{
				Logger.LogWarning("Dapp is not connected - nothing to disconnect");
				return;
			}

			Logger.LogDebug("Disconnecting wallet");
			BeaconDappClient.RemoveActiveAccounts();
			BeaconDappClient.Disconnect();
		}

		/// <summary>
		///     Retrieves the address of the active wallet.
		/// </summary>
		/// <returns>The address of the active wallet if any, otherwise an empty string.</returns>
		public string GetActiveWalletAddress()
		{
			return BeaconDappClient?.GetActiveAccount()?.Address ?? string.Empty;
		}
	}

}