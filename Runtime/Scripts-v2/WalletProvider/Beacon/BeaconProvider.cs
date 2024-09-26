using System;
using System.IO;
using Beacon.Sdk;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Error;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.BeaconClients.Abstract;
using Microsoft.Extensions.Logging;
using Netezos.Keys;
using Tezos.Configs;
using Tezos.Cysharp;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MainThreadDispatcher;
using Tezos.MessageSystem;
using Tezos.Operation;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using OperationRequest = Tezos.Operation.OperationRequest;
using OperationResponse = Beacon.Sdk.Beacon.Operation.OperationResponse;
using SignPayloadRequest = Tezos.Operation.SignPayloadRequest;
using SignPayloadResponse = Beacon.Sdk.Beacon.Sign.SignPayloadResponse;

namespace Tezos.WalletProvider
{
#region DummyLogger

	public class DummyLoggerProvider : ILoggerProvider
	{
		public void Dispose() { }

		public ILogger CreateLogger(string categoryName) { return new Logger(); }
	}

	public class Logger : ILogger
	{
		public IDisposable BeginScope<TState>(TState state) { return null; }

		public bool IsEnabled(LogLevel logLevel) { return true; }

		public void Log<TState>(
			LogLevel                        logLevel,
			EventId                         eventId,
			TState                          state,
			Exception                       exception,
			Func<TState, Exception, string> formatter
			)
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

			if (logLevel      == LogLevel.Error) TezosLogger.LogError("BEACON MESSAGE: "      + message);
			else if (logLevel == LogLevel.Critical) TezosLogger.LogError("BEACON MESSAGE: "   + message);
			else if (logLevel == LogLevel.Debug) TezosLogger.LogDebug("BEACON MESSAGE: "      + message);
			else if (logLevel == LogLevel.Warning) TezosLogger.LogWarning("BEACON MESSAGE: "  + message);
			else if (logLevel == LogLevel.Information) TezosLogger.LogInfo("BEACON MESSAGE: " + message);
			else TezosLogger.LogInfo("BEACON MESSAGE: "                                       + message);
		}
	}

#endregion

	public class BeaconProvider : IWalletProvider
	{
		public event Action<WalletProviderData> WalletConnected;
		public event Action                     WalletDisconnected;
		public event Action<string>             PairingRequested;

		public WalletType WalletType => WalletType.BEACON;

		private UniTaskCompletionSource<Operation.OperationResponse>   _operationTcs;
		private UniTaskCompletionSource<Operation.SignPayloadResponse> _signPayloadTcs;
		private UniTaskCompletionSource<WalletProviderData>            _walletConnectionTcs;
		private UniTaskCompletionSource<bool>                          _walletDisconnectionTcs;

		private BeaconConnector         _connector;
		private OperationRequestHandler _operationRequestHandler;

		public UniTask Init()
		{
			_operationRequestHandler = new OperationRequestHandler();
			_connector               = new BeaconConnector(_operationRequestHandler, this);
			return CreateAsync();
		}

		public UniTask<WalletProviderData> Connect(WalletProviderData data)
		{
			if (_walletConnectionTcs != null && _walletConnectionTcs.Task.Status == UniTaskStatus.Pending) return _walletConnectionTcs.Task;

			_walletConnectionTcs = new();
			TezosLogger.LogDebug($"Connect method entered");
			try
			{
				if (BeaconDappClient == null)
				{
					TezosLogger.LogWarning($"BeaconDappClient is null");
					return UniTask.FromResult(_walletData);
				}

				if (HandleExistingConnection())
				{
					// We already have an active connection
					return _walletConnectionTcs.Task;
				}

				// We need to establish a new connection
				var pairingRequestInfo = BeaconDappClient.GetPairingRequestInfo();
				_walletData            = data;
				_walletData.PairingUri = pairingRequestInfo;
				_connector.PairingRequested(pairingRequestInfo);
				PairingRequested?.Invoke(pairingRequestInfo);
				TezosLogger.LogDebug($"pairingRequestInfo: {pairingRequestInfo}");
			}
			catch (Exception e)
			{
				TezosLogger.LogError($"Error during dapp connection: {e.Message}");
			}

			return _walletConnectionTcs.WithTimeout(30 * 1000, "Wallet connection task timeout.");
		}

		public async UniTask<bool> Disconnect()
		{
			if (_walletDisconnectionTcs != null && _walletDisconnectionTcs.Task.Status == UniTaskStatus.Pending) return await _walletDisconnectionTcs.Task;

			_walletDisconnectionTcs = new();
			if (!BeaconDappClient.Connected)
			{
				TezosLogger.LogWarning("Dapp is not connected - nothing to disconnect");
				return false;
			}

			TezosLogger.LogDebug("Disconnecting wallet");
			await BeaconDappClient.RemovePeerAsync(BeaconDappClient.GetActivePeer()!.SenderId);
			BeaconDappClient.Disconnect();
			await UnityMainThreadDispatcher.Instance().EnqueueAsync(HandleDisconnection);
			return await _walletDisconnectionTcs.Task;
		}

		public async UniTask<Operation.OperationResponse> RequestOperation(OperationRequest operationRequest)
		{
			if (_operationTcs != null && _operationTcs.Task.Status == UniTaskStatus.Pending) return await _operationTcs.Task;

			_operationTcs = new();
			_connector.RequestOperation(operationRequest);
			return await _operationTcs.WithTimeout(10 * 1000, "Request operation task timeout.");
		}

		public async UniTask<Operation.SignPayloadResponse> RequestSignPayload(SignPayloadRequest signRequest)
		{
			if (_signPayloadTcs != null && _signPayloadTcs.Task.Status == UniTaskStatus.Pending) return await _signPayloadTcs.Task;

			_signPayloadTcs = new();
			_connector.RequestSignPayload(signRequest);
			return await _signPayloadTcs.WithTimeout(10 * 1000, "Sign payload task timeout.");
		}

		public UniTask RequestContractOrigination(OriginateContractRequest originationRequest) => _connector.RequestContractOrigination(originationRequest);

		public bool IsAlreadyConnected() => BeaconDappClient.Connected;

#region Implementation

		private WalletProviderData _walletData = new();

		public DappBeaconClient BeaconDappClient { get; private set; }

		private async void RequestTezosPermission() => await _operationRequestHandler.RequestTezosPermission(BeaconDappClient);

		private async UniTask InitAsync()
		{
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
			_walletConnectionTcs.TrySetResult(_walletData);
			return true;
		}

		/// <summary>
		///     Creates and initializes the Beacon client.
		/// </summary>
		private async UniTask CreateAsync()
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
			// BeaconDappClient.OnDisconnected          += OnBeaconDappClientDisconnected;
			await InitAsync();
		}

		/// <summary>
		///     Handles the event of the Beacon Dapp client getting disconnected.
		/// </summary>
		private void OnBeaconDappClientDisconnected()
		{
			TezosLogger.LogDebug("OnBeaconDappClientDisconnected - Dapp disconnected");
			// DoDisconnect();
		}

		/// <summary>
		///     Receives and processes Beacon messages asynchronously.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">Event arguments containing the received Beacon message.</param>
		/// <remarks>The event is raised on a background thread and must be marshalled to the UI thread.</remarks>
		private void OnBeaconDappClientMessageReceived(object sender, BeaconMessageEventArgs e)
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
			UnityMainThreadDispatcher.Instance().Enqueue(
			                                             () =>
			                                             {
				                                             switch (e.Request?.Type)
				                                             {
					                                             case BeaconMessageType.permission_response:
						                                             HandlePermissionResponse(e.Request as PermissionResponse);
						                                             break;
					                                             case BeaconMessageType.operation_response:
						                                             HandleOperationResponse(e.Request as OperationResponse);
						                                             break;
					                                             case BeaconMessageType.sign_payload_response:
						                                             HandleSignPayloadResponse(e.Request as SignPayloadResponse);
						                                             break;
					                                             case BeaconMessageType.error:
						                                             HandleOperationError(e.Request as BaseBeaconError);
						                                             break;
					                                             case BeaconMessageType.disconnect:
						                                             HandleDisconnect();
						                                             break;
				                                             }
			                                             }
			                                            );
		}

		/// <summary>
		///     Handles a permission response message.
		/// </summary>
		/// <param name="permissionResponse">The permission response message to handle.</param>
		private void HandlePermissionResponse(PermissionResponse permissionResponse)
		{
			if (_walletConnectionTcs.Task.Status == UniTaskStatus.Faulted)
			{
				TezosLogger.LogWarning("Wallet connection response received but probably task is timed out.");
				return;
			}

			if (!string.IsNullOrEmpty(_walletData.WalletAddress)) TezosLogger.LogWarning("Active wallet already exists!");

			TezosLogger.LogInfo($"Received permission response from {permissionResponse.AppMetadata.Name}!");
			_walletData = new WalletProviderData
			              {
				              WalletType    = WalletType.BEACON,
				              WalletAddress = PubKey.FromBase58(permissionResponse.PublicKey).Address,
				              PublicKey     = permissionResponse.PublicKey
			              };

			if (_walletConnectionTcs != null) _walletConnectionTcs.TrySetResult(_walletData);

			WalletConnected?.Invoke(_walletData);
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
			TezosLogger.LogDebug($"Received operation response. operationResponse is null:{operationResponse is null}");
			
			if (_operationTcs.Task.Status == UniTaskStatus.Faulted)
			{
				TezosLogger.LogWarning("Operation response received but probably task is timed out.");
				return;
			}

			if (operationResponse == null)
			{
				TezosLogger.LogWarning("Operation response is null.");
				return;
			}

			Operation.OperationResponse opResponse = new Operation.OperationResponse
			                                         {
				                                         TransactionHash = operationResponse.TransactionHash,
				                                         Id              = operationResponse.Id,
				                                         SenderId        = operationResponse.SenderId,
			                                         };
			_operationTcs.TrySetResult(opResponse);
		}

		/// <summary>
		///     Asynchronously handles a sign payload response message.
		/// </summary>
		/// <param name="signPayloadResponse">The sign payload response message to handle.</param>
		private void HandleSignPayloadResponse(SignPayloadResponse signPayloadResponse)
		{
			TezosLogger.LogDebug($"Received sign payload response. signPayloadResponse is null:{signPayloadResponse is null}");
			
			if (_signPayloadTcs.Task.Status == UniTaskStatus.Faulted)
			{
				TezosLogger.LogWarning("Sign payload response received but probably task is timed out.");
				return;
			}

			if (signPayloadResponse == null)
			{
				TezosLogger.LogWarning("Sign payload response is null.");
				return;
			}

			var signPayloadResult = new Operation.SignPayloadResponse
			                        {
				                        Signature = signPayloadResponse.Signature,
				                        Id        = signPayloadResponse.Id,
				                        SenderId  = signPayloadResponse.SenderId,
			                        };

			_signPayloadTcs.TrySetResult(signPayloadResult);
		}

		private void HandleOperationError(BaseBeaconError baseBeaconError)
		{
			TezosLogger.LogWarning($"Operation failed, user might have rejected. Reason: {baseBeaconError.ErrorType}");
			UnityMainThreadDispatcher.Instance().Enqueue(
			                                             () =>
			                                             {
				                                             if (_walletConnectionTcs != null && _walletConnectionTcs.Task.Status == UniTaskStatus.Pending)
				                                             {
					                                             _walletConnectionTcs.TrySetException(new WalletConnectionRejected("Wallet connection rejected."));
					                                             _walletConnectionTcs = null;
				                                             }

				                                             else if (_operationTcs != null && _operationTcs.Task.Status == UniTaskStatus.Pending)
				                                             {
					                                             _operationTcs.TrySetException(new WalletOperationRejected("Wallet operation rejected."));
					                                             _operationTcs = null;
				                                             }

				                                             else if (_signPayloadTcs != null && _signPayloadTcs.Task.Status == UniTaskStatus.Pending)
				                                             {
					                                             _signPayloadTcs.TrySetException(new WalletSignPayloadRejected("Sign payload rejected."));
					                                             _signPayloadTcs = null;
				                                             }
				                                             else
				                                             {
					                                             TezosLogger.LogError($"Unknown error caught: {baseBeaconError.ErrorType}");
				                                             }
			                                             }
			                                            );
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
			UnityMainThreadDispatcher.Instance().Enqueue(HandleDisconnection);
		}

		private async void HandleDisconnection()
		{
			// BeaconDappClient.OnBeaconMessageReceived -= OnBeaconDappClientMessageReceived;
			// BeaconDappClient.OnDisconnected          -= OnBeaconDappClientDisconnected;
			//
			// BeaconDappClient = null;

			BeaconDappClient.Disconnect();

			await CreateAsync();

			// meaning dapp removed via wallet app
			if (_walletDisconnectionTcs == null) _walletDisconnectionTcs = new();

			_walletDisconnectionTcs.TrySetResult(true);
			_walletData = new();
			WalletDisconnected?.Invoke();
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
				       AppName                  = appConfig.AppName,
				       AppUrl                   = appConfig.AppUrl,
				       IconUrl                  = appConfig.AppIcon,
				       KnownRelayServers        = Constants.KnownRelayServers,
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
		private string GetDbPath() => Path.Combine(Application.persistentDataPath, "beacon.db");

#endregion
	}
}