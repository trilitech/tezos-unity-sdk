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
using TezosSDK.Helpers.Extensions;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Enums;
using TezosSDK.WalletServices.Helpers;
using TezosSDK.WalletServices.Helpers.Loggers;
using UnityEngine;

namespace TezosSDK.WalletServices.Beacon
{

	public class BeaconClientManager : IDisposable
	{
		private readonly EventDispatcher _eventDispatcher;

		private readonly IWalletConnector _walletConnector;
		private WalletInfo _activeWallet; // Keep track of the active wallet
		private bool _isInitialized;

		public BeaconClientManager(WalletEventManager eventManager, IWalletConnector walletConnector)
		{
			_eventDispatcher = new EventDispatcher(eventManager);
			_walletConnector = walletConnector;
			eventManager.WalletConnected += OnWalletConnected;
			eventManager.WalletDisconnected += OnWalletDisconnected;
			_walletConnector.OperationRequested += OperationRequestedHandler;
		}

		public DappBeaconClient BeaconDappClient { get; private set; }

		public void Dispose()
		{
			BeaconDappClient?.Disconnect();
		}

		// private void OperationRequestedHandler(WalletMessageType messageType)
		// {
		// 	switch (messageType)
		// 	{
		// 		case WalletMessageType.ConnectionRequest:
		// 			// Handle connection request logic
		// 			break;
		// 		case WalletMessageType.OperationRequest:
		// 			// Handle operation request logic
		// 			break;
		// 		case WalletMessageType.SignPayloadRequest:
		// 			// Handle sign payload request logic
		// 			break;
		// 		case WalletMessageType.DisconnectionRequest:
		// 			// Handle disconnection request logic
		// 			break;
		// 	}
		// }

		private void OperationRequestedHandler(WalletMessageType messageType)
		{
			TezosLog.Debug($"OperationRequestedHandler - MessageType: {messageType}");

			switch (messageType)
			{
				case WalletMessageType.ConnectionRequest:
					// _walletConnector.ConnectWallet();
					break;
				case WalletMessageType.OperationRequest:
					// _walletConnector.RequestOperation(operationDetails);
					break;
				case WalletMessageType.SignPayloadRequest:
					// _walletConnector.RequestSignPayload(signPayloadDetails);
					break;
				case WalletMessageType.DisconnectionRequest:
					// _walletConnector.DisconnectWallet();
					break;
			}
		}

		private void OnWalletDisconnected(WalletInfo obj)
		{
			_activeWallet = null; // Reset active wallet
		}

		private void OnWalletConnected(WalletInfo wallet)
		{
			_activeWallet = wallet; // Set active wallet
		}

		public async Task Initalize()
		{
			if (_isInitialized)
			{
				TezosLog.Warning("BeaconClientManager already initialized");
				return;
			}

			if (BeaconDappClient == null)
			{
				TezosLog.Error("BeaconDappClient is null");
				return;
			}

			TezosLog.Info("Initializing BeaconDappClient");

			await BeaconDappClient.InitAsync();

			TezosLog.Info("BeaconDappClient initialized");
		}

		public void Connect()
		{
			try
			{
				if (BeaconDappClient == null)
				{
					TezosLog.Error("BeaconDappClient is null");
					return;
				}

				BeaconDappClient.Connect();

				var activePeer = BeaconDappClient.GetActivePeer();

				TezosLog.Info($"BeaconDappClient initialized. Logged in: {BeaconDappClient.LoggedIn} - " +
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
				TezosLog.Error($"Error during dapp initialization: {e.Message}");
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
				TezosLog.Info("No already active wallet");
				return false;
			}

			TezosLog.Info("We already have connection with wallet: " + activeAccountPermissions.Print());
			return true;
		}

		/// <summary>
		///     Creates the Beacon client.
		/// </summary>
		public void Create()
		{
			TezosLog.Debug("Creating Beacon client");
			var options = CreateBeaconOptions();

			BeaconDappClient =
				BeaconClientFactory.Create<IDappBeaconClient>(options, new ConnectorLoggerProvider()) as
					DappBeaconClient;

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
				TezosLog.Debug("OnConnectedClientsListChanged - Connected clients list changed");
				_eventDispatcher.DispatchWalletConnectedEvent(BeaconDappClient);
			}
			else
			{
				TezosLog.Debug("OnConnectedClientsListChanged - Connected clients list is empty");
				_eventDispatcher.DispatchWalletDisconnectedEvent(_activeWallet);
			}
		}

		/// <summary>
		///     Handles the event of the Beacon Dapp client getting disconnected.
		/// </summary>
		private void OnBeaconDappClientDisconnected()
		{
			TezosLog.Debug("OnBeaconDappClientDisconnected - Dapp disconnected");
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

			if (e.PairingDone)
			{
				HandlePairingDone();
				return;
			}

			TezosLog.Debug($"Received beacon message of type: {e.Request?.Type} - ID: {e.Request?.Id}");

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

			TezosLog.Debug(permissionResponse.Print());
		}

		/// <summary>
		///     Handles an operation response message.
		/// </summary>
		/// <param name="operationResponse">The operation response message to handle.</param>
		/// <remarks>
		///     We only get beacon messages for operations that were injected.
		///     To confirm completion, we need to track the operation hash using
		///     <see cref="OperationTracker" />
		///     and then dispatch <see cref="WalletEventManager.EventTypeOperationCompleted" /> or
		///     <see cref="WalletEventManager.EventTypeOperationFailed" /> or any other event as needed.
		/// </remarks>
		private void HandleOperationResponse(OperationResponse operationResponse)
		{
			if (operationResponse == null)
			{
				return;
			}

			_eventDispatcher.DispatchOperationInjectedEvent(operationResponse);
		}

		/// <summary>
		///     Asynchronously handles a sign payload response message.
		/// </summary>
		/// <param name="signPayloadResponse">The sign payload response message to handle.</param>
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
			TezosLog.Debug("Received message of type Pairing done");

			if (_activeWallet != null)
			{
				TezosLog.Debug("Active wallet already exists, ignoring duplicate pairing done event");
				return;
			}

			TezosManager.Instance.WalletConnector.ConnectWallet();
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
				TezosLog.Warning("Dapp is not connected - nothing to disconnect");
				return;
			}

			TezosLog.Debug("Disconnecting wallet");
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