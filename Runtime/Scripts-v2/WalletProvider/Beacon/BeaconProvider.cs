﻿using System;
using System.IO;
using System.Threading.Tasks;
using Beacon.Sdk;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Error;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.BeaconClients.Abstract;
using Microsoft.Extensions.Logging;
using Netezos.Keys;
using Tezos.Configs;
using Tezos.Logger;
using Tezos.MessageSystem;
using UnityEngine;
using UnityEngine.UIElements;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tezos.WalletProvider
{
	#region DummyLogger

	public class DummyLoggerProvider : ILoggerProvider
	{
		public void Dispose()
		{
			
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new Logger();
		}
	}

	public class Logger : ILogger
	{
		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			var message = formatter(state, exception);

			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			if (exception != null)
			{
				message += "\nException: " + exception;
			}

			TezosLogger.LogError("BEACON MESSAGE: " + message);
		}
	}

	#endregion
	
	public class BeaconProvider : IWalletProvider
	{
		public WalletType WalletType => WalletType.BEACON;

		private BeaconConnector         _connector;
		private OperationRequestHandler _operationRequestHandler;
		private TaskCompletionSource<WalletProviderData> _walletConnectionTcs;
		private TaskCompletionSource<bool> _walletDisconnectionTcs;

		public Task Init(IContext context)
		{
			_operationRequestHandler = new OperationRequestHandler();
			_connector = new BeaconConnector(_operationRequestHandler);
			
			return CreateAsync();
		}

		public Task<WalletProviderData> Connect(WalletProviderData data)
		{
			if(_walletConnectionTcs != null && _walletConnectionTcs.Task.Status == TaskStatus.Running)
				return _walletConnectionTcs.Task;
			
			_walletConnectionTcs = new();
			TezosLogger.LogWarning($"Connect method entered");
			try
			{
				if (BeaconDappClient == null)
				{
					TezosLogger.LogWarning($"BeaconDappClient is null");
					return Task.FromResult<WalletProviderData>(null);
				}

				if (HandleExistingConnection())
				{
					// We already have an active connection
					return _walletConnectionTcs.Task;
				}

				// We need to establish a new connection
				var pairingRequestInfo = BeaconDappClient.GetPairingRequestInfo();
				data.PairingUri = pairingRequestInfo;
				_connector.PairingRequested(pairingRequestInfo);
				TezosLogger.LogDebug($"pairingRequestInfo: {pairingRequestInfo}");
			}
			catch (Exception e)
			{
				TezosLogger.LogError($"Error during dapp connection: {e.Message}");
				throw;
			}
			return _walletConnectionTcs.Task;
		}

		public Task<bool> Disconnect()
		{
			if(_walletDisconnectionTcs != null && _walletDisconnectionTcs.Task.Status == TaskStatus.Running)
				return _walletDisconnectionTcs.Task;

			_walletDisconnectionTcs = new();
			if (!BeaconDappClient.Connected)
			{
				TezosLogger.LogWarning("Dapp is not connected - nothing to disconnect");
				return Task.FromResult(false);
			}

			TezosLogger.LogDebug("Disconnecting wallet");
			BeaconDappClient.RemoveActiveAccounts();
			BeaconDappClient.Disconnect();
			return Task.FromResult(true);
		}

		public Task RequestOperation(WalletOperationRequest operationRequest)
		{
			return _connector.RequestOperation(operationRequest);
		}

		public Task RequestSignPayload(WalletSignPayloadRequest signRequest)
		{
			throw new NotImplementedException();
		}

		public Task RequestContractOrigination(WalletOriginateContractRequest originationRequest)
		{
			throw new NotImplementedException();
		}

		public bool IsAlreadyConnected() => BeaconDappClient.Connected;

		#region Implementation

		private readonly EventDispatcher _eventDispatcher;

		private WalletProviderData _walletData = new();
		private bool _isInitialized;

		public DappBeaconClient BeaconDappClient { get; private set; }

		private async void RequestTezosPermission() => await _operationRequestHandler.RequestTezosPermission(BeaconDappClient);

		private async Task InitAsync()
		{
			if (_isInitialized)
			{
				TezosLogger.LogWarning("BeaconClientManager already initialized");
				return;
			}

			if (BeaconDappClient == null)
			{
				TezosLogger.LogError("BeaconDappClient is null here");
				return;
			}

			TezosLogger.LogInfo("Initializing BeaconDappClient");
			await BeaconDappClient.InitAsync();
			BeaconDappClient.Connect();
			TezosLogger.LogInfo("BeaconDappClient initialized");
		}

		/// <summary>
		///     Checks if there is an existing connection with a wallet.
		/// </summary>
		/// <returns>Returns true if an active connection exists, otherwise false.</returns>
		private bool HandleExistingConnection()
		{
			TezosLogger.LogWarning($"HandleExistingConnection entered");
			
			var activeAccountPermissions = BeaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				TezosLogger.LogInfo("No already active wallet");
				return false;
			}

			TezosLogger.LogInfo($"We already have connection with wallet: {activeAccountPermissions.AppMetadata.Name}");

			_walletData = new WalletProviderData
			{
				WalletType    = WalletType.BEACON,
				WalletAddress = activeAccountPermissions.Address,
				PublicKey     = activeAccountPermissions.PublicKey
			};

			_walletConnectionTcs.SetResult(_walletData);
			return true;
		}

		/// <summary>
		///     Creates and initializes the Beacon client.
		/// </summary>
		private async Task CreateAsync()
		{
			TezosLogger.LogDebug("Creating Beacon client");
			var options = CreateBeaconOptions();

			BeaconDappClient = BeaconClientFactory.Create<IDappBeaconClient>(options, new DummyLoggerProvider()) as DappBeaconClient;
			TezosLogger.LogDebug($"BeaconDappClient is null:{BeaconDappClient is null}");

			if (BeaconDappClient == null)
			{
				throw new Exception("Failed to create Beacon client");
			}

			BeaconDappClient.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;
			BeaconDappClient.OnDisconnected += OnBeaconDappClientDisconnected;

			await InitAsync();
		}

		/// <summary>
		///     Handles the event of the Beacon Dapp client getting disconnected.
		/// </summary>
		private void OnBeaconDappClientDisconnected()
		{
			TezosLogger.LogDebug("OnBeaconDappClientDisconnected - Dapp disconnected");
			DoDisconnect();
		}

		/// <summary>
		///     Receives and processes Beacon messages asynchronously.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">Event arguments containing the received Beacon message.</param>
		/// <remarks>The event is raised on a background thread and must be marshalled to the UI thread.</remarks>
		private async void OnBeaconDappClientMessageReceived(object sender, BeaconMessageEventArgs e)
		{
			TezosLogger.LogDebug($"OnBeaconDappClientMessageReceived entered");
			
			if (e == null)
			{
				TezosLogger.LogError($"BeaconMessageEventArgs is null");
				return;
			}

			if (e.PairingDone)
			{
				TezosLogger.LogDebug($"OnBeaconDappClientMessageReceived PairingDone");
				HandlePairingDone();
				return;
			}

			TezosLogger.LogDebug($"(2) Received beacon message of type: {e.Request?.Type} - ID: {e.Request?.Id} - Request: {e.Request}");

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
				case BeaconMessageType.error:
					HandleOperationError(e.Request as BaseBeaconError);
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
			if (!string.IsNullOrEmpty(_walletData.WalletAddress))
			{
				TezosLogger.LogWarning("Active wallet already exists!");
			}

			if (permissionResponse == null)
			{
				return;
			}

			TezosLogger.LogInfo($"Received permission response from {permissionResponse.AppMetadata.Name}!");

			_walletData = new WalletProviderData
			{
				WalletType    = WalletType.BEACON,
				WalletAddress = PubKey.FromBase58(permissionResponse.PublicKey).Address,
				PublicKey     = permissionResponse.PublicKey
			};
			
			_walletConnectionTcs.SetResult(_walletData);
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

			// var operation = new OperationInfo(operationResponse.TransactionHash, operationResponse.Id, operationResponse.Type);
			//
			// _eventDispatcher.DispatchOperationInjectedEvent(operation);
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

			var senderPermissions = await BeaconDappClient.PermissionInfoRepository.TryReadBySenderIdAsync(signPayloadResponse.SenderId);

			if (senderPermissions == null)
			{
				return;
			}
			
			_walletConnectionTcs.SetResult(_walletData);
		}

		private void HandleOperationError(BaseBeaconError baseBeaconError)
		{
			TezosLogger.LogWarning($"Operation failed, user might have rejected.");
			// _eventDispatcher.DispatchOperationFailedEvent(new OperationInfo(String.Empty, baseBeaconError.Id, BeaconMessageType.error, baseBeaconError.ErrorType.ToString()));
			_walletData.Error = baseBeaconError.ErrorType.ToString();
		}

		/// <summary>
		///     Handles a disconnect type Beacon message.
		/// </summary>
		/// <remarks>
		///     Dispatches a wallet disconnected event on the main (UI) thread.
		/// </remarks>
		private void HandleDisconnect()
		{
			TezosLogger.LogDebug("Handling disconnect message");
			DoDisconnect();
		}

		private void DoDisconnect()
		{
			TezosLogger.LogDebug("Setting active wallet to null and dispatching wallet disconnected event");
			_walletDisconnectionTcs.SetResult(true);
			_walletData = null;
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

			if (!string.IsNullOrEmpty(_walletData.WalletAddress))
			{
				TezosLogger.LogDebug("Active wallet already exists, ignoring duplicate pairing done event");
				return;
			}

			RequestTezosPermission();
		}

		/// <summary>
		///     Creates a new instance of BeaconOptions for the Beacon client.
		/// </summary>
		/// <returns>A configured instance of BeaconOptions.</returns>
		private BeaconOptions CreateBeaconOptions()
		{
			AppConfig appConfig = ConfigGetter.GetOrCreateConfig<AppConfig>();
			return new BeaconOptions
			{
				AppName = appConfig.AppName,
				AppUrl  = appConfig.AppUrl,
				IconUrl = appConfig.AppIcon,
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
		///     Retrieves the address of the active wallet.
		/// </summary>
		/// <returns>The address of the active wallet if any, otherwise an empty string.</returns>
		public string GetActiveWalletAddress()
		{
			return BeaconDappClient?.GetActiveAccount()?.Address ?? string.Empty;
		}

		#endregion
	}
}
