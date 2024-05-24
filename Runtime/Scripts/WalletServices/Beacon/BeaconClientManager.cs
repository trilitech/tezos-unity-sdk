using System;
using System.IO;
using System.Threading;
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

		private WalletInfo _activeWallet; // Keep track of the active wallet
		private bool _isInitialized;
		private readonly OperationRequestHandler _operationRequestHandler;

		public BeaconClientManager(WalletEventManager eventManager, OperationRequestHandler operationRequestHandler)
		{
			_eventDispatcher = new EventDispatcher(eventManager);
			_operationRequestHandler = operationRequestHandler;
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

		private async Task InitAsync()
		{
			if (_isInitialized)
			{
				TezosLogger.LogWarning("BeaconClientManager already initialized");
				return;
			}

			if (BeaconDappClient == null)
			{
				TezosLogger.LogError("BeaconDappClient is null");
				return;
			}

			TezosLogger.LogInfo("Initializing BeaconDappClient");
			await BeaconDappClient.InitAsync();
			TezosLogger.LogInfo("BeaconDappClient initialized");
		}

		public void Connect()
		{
			try
			{
				if (BeaconDappClient == null)
				{
					TezosLogger.LogError("BeaconDappClient is null");
					return;
				}

				BeaconDappClient.Connect();

				var activePeer = BeaconDappClient.GetActivePeer();

				TezosLogger.LogInfo($"BeaconDappClient: Logged in: {BeaconDappClient.LoggedIn} - " +
				               $"Connected: {BeaconDappClient.Connected} - " + $"Active peer: {activePeer?.Name}");

				if (HandleExistingConnection())
				{
					// We already have an active connection
					return;
				}

				// We need to establish a new connection
				var pairingRequestInfo = BeaconDappClient.GetPairingRequestInfo();
				_eventDispatcher.DispatchHandshakeEvent(pairingRequestInfo);
				_operationRequestHandler.RequestTezosPermission(BeaconDappClient);
			}
			catch (Exception e)
			{
				TezosLogger.LogError($"Error during dapp connection: {e.Message}");
				throw;
			}
		}

		/// <summary>
		///     Checks if there is an existing connection with a wallet.
		/// </summary>
		/// <returns>Returns true if an active connection exists, otherwise false.</returns>
		private bool HandleExistingConnection()
		{
			var activeAccountPermissions = BeaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				TezosLogger.LogInfo("No already active wallet");
				return false;
			}
			
			TezosLogger.LogDebug($"Active account permissions: {activeAccountPermissions.PrettyPrint()}");
			TezosLogger.LogInfo($"We already have connection with wallet: {activeAccountPermissions.AppMetadata.Name}");
			
			_activeWallet = new WalletInfo
			{
				Address = activeAccountPermissions.Address,
				PublicKey = activeAccountPermissions.PublicKey
			};

			_eventDispatcher.DispatchWalletConnectedEvent(_activeWallet);

			return true;
		}

		/// <summary>
		///     Creates and initializes the Beacon client.
		/// </summary>
		public async Task CreateAsync()
		{
			TezosLogger.LogDebug("Creating Beacon client");
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
			// BeaconDappClient.OnConnectedClientsListChanged += OnConnectedClientsListChanged;
			
			await InitAsync();
		}

		// /// <summary>
		// ///     Handles the event when the list of connected clients changes.
		// /// </summary>
		// /// <param name="sender">The source of the event.</param>
		// /// <param name="e">Null when a client disconnects</param>
		// private void OnConnectedClientsListChanged(object sender, ConnectedClientsListChangedEventArgs e)
		// {
		// 	if (e != null)
		// 	{
		// 		TezosLogger.LogDebug("OnConnectedClientsListChanged - Connected clients list changed");
		// 		_eventDispatcher.DispatchWalletConnectedEvent(BeaconDappClient);
		// 	}
		// 	else
		// 	{
		// 		TezosLogger.LogDebug("OnConnectedClientsListChanged - Connected clients list is empty");
		// 		_eventDispatcher.DispatchWalletDisconnectedEvent(_activeWallet);
		// 	}
		// }

		/// <summary>
		///     Handles the event of the Beacon Dapp client getting disconnected.
		/// </summary>
		private void OnBeaconDappClientDisconnected()
		{
			TezosLogger.LogDebug("OnBeaconDappClientDisconnected - Dapp disconnected");
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

			TezosLogger.LogDebug($"Received beacon message of type: {e.Request?.Type} - ID: {e.Request?.Id}");

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
		private void HandlePermissionResponse(PermissionResponse permissionResponse)
		{
			if (permissionResponse == null)
			{
				return;
			}

			TezosLogger.LogDebug(permissionResponse.PrettyPrint());
			TezosLogger.LogInfo($"Received permission response from {permissionResponse.AppMetadata.Name}!");
			_eventDispatcher.DispatchWalletConnectedEvent(_activeWallet);
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
			TezosLogger.LogDebug("Received message of type Pairing done");

			if (_activeWallet != null)
			{
				TezosLogger.LogDebug("Active wallet already exists, ignoring duplicate pairing done event");
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
				TezosLogger.LogWarning("Dapp is not connected - nothing to disconnect");
				return;
			}

			TezosLogger.LogDebug("Disconnecting wallet");
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