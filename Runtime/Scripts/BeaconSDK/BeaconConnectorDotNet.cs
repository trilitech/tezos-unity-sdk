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
using Beacon.Sdk.Core.Domain.Services;
using Microsoft.Extensions.Logging;
using Netezos.Keys;
using Newtonsoft.Json.Linq;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = Helpers.Logger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BeaconSDK
{
    public class BeaconConnectorDotNet : IBeaconConnector
    {
        private static BeaconMessageReceiver _messageReceiver;
        private DappBeaconClient _beaconDappClient { get; set; }
        private string _network;
        private string _rpc;

        #region IBeaconConnector

        public async void ConnectAccount()
        {
            var pathToDb = Path.Combine(Application.persistentDataPath, "beacon.db");
            Logger.LogDebug($"DB file stored in {pathToDb}");
            
            var options = new BeaconOptions
            {
                AppName = "Tezos Unity SDK",
                AppUrl = "https://tezos.com/unity",
                IconUrl = "https://unity.com/sites/default/files/2022-09/unity-tab-small.png",
                KnownRelayServers = Constants.KnownRelayServers,
                DatabaseConnectionString = $"Filename={pathToDb};Connection=direct;Upgrade=true"
            };

            _beaconDappClient = (DappBeaconClient)BeaconClientFactory
                .Create<IDappBeaconClient>(options, new MyLoggerProvider());
            _beaconDappClient.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;

            await _beaconDappClient.InitAsync();
            Logger.LogInfo($"Dapp initialized: {_beaconDappClient.LoggedIn}");
            _beaconDappClient.Connect();
            Logger.LogInfo($"Dapp connected: {_beaconDappClient.Connected}");

            var activeAccountPermissions = _beaconDappClient.GetActiveAccount();
            if (activeAccountPermissions != null)
            {
                var permissionsString = activeAccountPermissions.Scopes.Aggregate(string.Empty,
                    (res, scope) => res + $"{scope}, ") ?? string.Empty;
                Logger.LogInfo(
                    $"We have active peer {activeAccountPermissions.AppMetadata.Name} with permissions {permissionsString}");

                UnityMainThreadDispatcher.Enqueue(
                    _messageReceiver.OnAccountConnected,
                    new JObject
                    {
                        ["account"] = new JObject
                        {
                            ["address"] = activeAccountPermissions.Address,
                            ["publicKey"] = activeAccountPermissions.PublicKey
                        }
                    }.ToString());
            }
            else
            {
                var pairingRequestQrData = _beaconDappClient.GetPairingRequestInfo();
                _messageReceiver.OnHandshakeReceived(pairingRequestQrData);
            }
        }

        public string GetActiveAccountAddress() => _beaconDappClient?.GetActiveAccount()?.Address ?? string.Empty;

        public void RequestHandshake()
        {
        }

        public void DisconnectAccount()
        {
            _beaconDappClient.RemoveActiveAccounts();
            var pairingRequestQrData = _beaconDappClient.GetPairingRequestInfo();
            _messageReceiver.OnHandshakeReceived(pairingRequestQrData);
            UnityMainThreadDispatcher.Enqueue(_messageReceiver.OnAccountDisconnected, string.Empty);
        }

        public void SetNetwork(string network, string rpc)
        {
            _network = network;
            _rpc = rpc;
        }

        public void SetBeaconMessageReceiver(BeaconMessageReceiver messageReceiver)
        {
            _messageReceiver = messageReceiver;
        }

        public async void RequestTezosPermission(string networkName = "", string networkRPC = "")
        {
            if (!Enum.TryParse(networkName, out Beacon.Sdk.Beacon.Permission.NetworkType networkType))
                networkType = Beacon.Sdk.Beacon.Permission.NetworkType.ghostnet;

            var network = new Beacon.Sdk.Beacon.Permission.Network
            {
                Type = networkType,
                Name = _network,
                RpcUrl = _rpc
            };

            var permissionScopes = new List<PermissionScope>
            {
                PermissionScope.operation_request,
                PermissionScope.sign
            };

            var permissionRequest = new PermissionRequest(
                type: BeaconMessageType.permission_request,
                version: Constants.BeaconVersion,
                id: KeyPairService.CreateGuid(),
                senderId: _beaconDappClient.SenderId,
                appMetadata: _beaconDappClient.Metadata,
                network: network,
                scopes: permissionScopes
            );

            var activePeer = _beaconDappClient.GetActivePeer();
            if (activePeer != null)
            {
                await _beaconDappClient.SendResponseAsync(activePeer.SenderId, permissionRequest);
                Logger.LogInfo("Permission request sent");
            }
            else
            {
                Logger.LogError("No active peer found");
            }
        }

        public async void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null,
            ulong amount = 0,
            string networkName = "", string networkRPC = "")
        {
            var operationDetails = new List<TezosBaseOperation>();
            var partialTezosTransactionOperation = new PartialTezosTransactionOperation(
                amount.ToString(),
                destination,
                new JObject
                {
                    ["entrypoint"] = entryPoint,
                    ["value"] = JObject.Parse(arg)
                }
            );

            operationDetails.Add(partialTezosTransactionOperation);

            var activeAccountPermissions = _beaconDappClient.GetActiveAccount();
            if (activeAccountPermissions == null)
            {
                Logger.LogError("No active permissions");
                return;
            }

            var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

            var operationRequest = new OperationRequest(
                type: BeaconMessageType.operation_request,
                version: Constants.BeaconVersion,
                id: KeyPairService.CreateGuid(),
                senderId: _beaconDappClient.SenderId,
                network: activeAccountPermissions.Network,
                operationDetails: operationDetails,
                sourceAddress: pubKey.Address);

            Logger.LogDebug("requesting operation: " + operationRequest);
            await _beaconDappClient.SendResponseAsync(activeAccountPermissions.SenderId, operationRequest);
        }

        public void RequestTezosSignPayload(SignPayloadType signingType, string payload)
        {
            _beaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signingType, payload), signingType);
        }
        
        public void RequestTezosBroadcast(string signedTransaction, string networkName = "", string networkRPC = "")
        {
        }

        #endregion

        #region BeaconSDK

        private async void OnBeaconDappClientMessageReceived(object sender, BeaconMessageEventArgs e)
        {
            if (e.PairingDone)
            {
                _messageReceiver.OnPairingCompleted("paired");
                return;
            }

            var message = e.Request;
            switch (message.Type)
            {
                case BeaconMessageType.permission_response:
                {
                    if (message is not PermissionResponse permissionResponse)
                        return;

                    var permissionsString = permissionResponse.Scopes.Aggregate(string.Empty,
                        (res, scope) => res + $"{scope}, ");
                    Debug.Log(
                        $"{_beaconDappClient.AppName} received permissions {permissionsString} from {permissionResponse.AppMetadata.Name} with public key {permissionResponse.PublicKey}");

                    UnityMainThreadDispatcher.Enqueue(
                        _messageReceiver.OnAccountConnected, //permissionResponse.PublicKey);
                        new JObject
                        {
                            ["account"] = new JObject
                            {
                                ["address"] = PubKey.FromBase58(permissionResponse.PublicKey).Address,
                                ["publicKey"] = permissionResponse.PublicKey
                            }
                        }.ToString());

                    break;
                }

                case BeaconMessageType.operation_response:
                {
                    if (message is not OperationResponse operationResponse)
                        return;

                    UnityMainThreadDispatcher.Enqueue(
                        _messageReceiver.OnContractCallInjected, //operationResponse.TransactionHash);
                        new JObject
                        {
                            ["transactionHash"] = operationResponse.TransactionHash,
                            ["success"] = "true"
                        }.ToString());

                    Debug.Log($"Operation completed with transaction hash {operationResponse.TransactionHash}");
                    break;
                }

                case BeaconMessageType.sign_payload_response:
                {
                    if (message is not SignPayloadResponse signPayloadResponse)
                        return;

                    var senderPermissions = await _beaconDappClient
                        .PermissionInfoRepository
                        .TryReadBySenderIdAsync(signPayloadResponse.SenderId);
                    if (senderPermissions == null) return;

                    _messageReceiver.OnPayloadSigned( //signPayloadResponse.Signature);
                        new JObject
                        {
                            ["signature"] = signPayloadResponse.Signature
                        }.ToString());

                    break;
                }
            }
        }

        #endregion
    }
}

public class MyLoggerProvider : ILoggerProvider
{
    public class MyLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (exception != null)
                Debug.LogException(exception);

            //Debug.Log(state.ToString());
        }

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable BeginScope<TState>(TState state) => null;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName) => new MyLogger();
}