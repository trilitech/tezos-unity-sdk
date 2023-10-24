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
using TezosSDK.Helpers;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = TezosSDK.Helpers.Logger;
using BeaconNetwork = global::Beacon.Sdk.Beacon.Permission.Network;

namespace TezosSDK.Beacon
{
    public class BeaconConnectorDotNet : IBeaconConnector, IDisposable
    {
        private static WalletMessageReceiver _walletMessageReceiver;
        private DappBeaconClient BeaconDappClient { get; set; }
        private string _network;
        private string _rpc;
        private DAppMetadata _dAppMetadata;

        #region IBeaconConnector
        
        public async void ConnectAccount()
        {
            if (BeaconDappClient != null) return;

            var pathToDb = Path.Combine(Application.persistentDataPath, "beacon.db");
            Logger.LogDebug($"DB file stored in {pathToDb}");

            var options = new BeaconOptions
            {
                AppName = _dAppMetadata.Name,
                AppUrl = _dAppMetadata.Url,
                IconUrl = _dAppMetadata.Icon,
                KnownRelayServers = Constants.KnownRelayServers,
                DatabaseConnectionString = $"Filename={pathToDb};Connection=direct;Upgrade=true"
            };

            BeaconDappClient = (DappBeaconClient)BeaconClientFactory
                .Create<IDappBeaconClient>(options, new MyLoggerProvider());
            BeaconDappClient.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;

            await BeaconDappClient.InitAsync();
            Logger.LogInfo($"Dapp initialized: {BeaconDappClient.LoggedIn}");
            BeaconDappClient.Connect();
            Logger.LogInfo($"Dapp connected: {BeaconDappClient.Connected}");

            var activeAccountPermissions = BeaconDappClient.GetActiveAccount();
            if (activeAccountPermissions != null)
            {
                var permissionsString = activeAccountPermissions.Scopes.Aggregate(string.Empty,
                    (res, scope) => res + $"{scope}, ") ?? string.Empty;
                Logger.LogInfo(
                    $"We have active peer {activeAccountPermissions.AppMetadata.Name} with permissions {permissionsString}");

                UnityMainThreadDispatcher.Enqueue(
                    _walletMessageReceiver.OnAccountConnected,
                    new JObject
                    {
                        ["accountInfo"] = new JObject
                        {
                            ["address"] = activeAccountPermissions.Address,
                            ["publicKey"] = activeAccountPermissions.PublicKey
                        }
                    }.ToString());
            }
            else
            {
                _walletMessageReceiver.OnHandshakeReceived(BeaconDappClient.GetPairingRequestInfo());
            }
        }

        public string GetActiveAccountAddress() => BeaconDappClient?.GetActiveAccount()?.Address ?? string.Empty;

        public void DisconnectAccount()
        {
            BeaconDappClient.RemoveActiveAccounts();
            var pairingRequestQrData = BeaconDappClient.GetPairingRequestInfo();
            _walletMessageReceiver.OnHandshakeReceived(pairingRequestQrData);
            UnityMainThreadDispatcher.Enqueue(_walletMessageReceiver.OnAccountDisconnected, string.Empty);
        }

        public void InitWalletProvider(
            string network,
            string rpc,
            WalletProviderType walletProviderType,
            DAppMetadata dAppMetadata)
        {
            _network = network;
            _rpc = rpc;
            _dAppMetadata = dAppMetadata;
        }

        public void SetWalletMessageReceiver(WalletMessageReceiver messageReceiver)
        {
            _walletMessageReceiver = messageReceiver;
        }

        public async void RequestTezosPermission(string networkName = "", string networkRPC = "")
        {
            if (!Enum.TryParse(networkName, out NetworkType networkType))
                networkType = TezosConfig.Instance.Network;

            var network = new BeaconNetwork
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
                senderId: BeaconDappClient.SenderId,
                appMetadata: BeaconDappClient.Metadata,
                network: network,
                scopes: permissionScopes
            );

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
            var partialTezosTransactionOperation = new PartialTezosTransactionOperation(
                amount.ToString(),
                destination,
                new JObject
                {
                    ["entrypoint"] = entryPoint,
                    ["value"] = JToken.Parse(arg)
                }
            );

            operationDetails.Add(partialTezosTransactionOperation);

            var operationRequest = new OperationRequest(
                type: BeaconMessageType.operation_request,
                version: Constants.BeaconVersion,
                id: KeyPairService.CreateGuid(),
                senderId: BeaconDappClient.SenderId,
                network: activeAccountPermissions.Network,
                operationDetails: operationDetails,
                sourceAddress: pubKey.Address);

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
            var partialTezosTransactionOperation = new PartialTezosOriginationOperation(
                Balance: "0",
                Script: JObject.Parse(script),
                Delegate: delegateAddress
            );

            operationDetails.Add(partialTezosTransactionOperation);
            
            var operationRequest = new OperationRequest(
                type: BeaconMessageType.operation_request,
                version: Constants.BeaconVersion,
                id: KeyPairService.CreateGuid(),
                senderId: BeaconDappClient.SenderId,
                network: activeAccountPermissions.Network,
                operationDetails: operationDetails,
                sourceAddress: pubKey.Address);

            Logger.LogDebug("requesting operation: " + operationRequest);
            await BeaconDappClient.SendResponseAsync(activeAccountPermissions.SenderId, operationRequest);
        }

        public void RequestTezosSignPayload(SignPayloadType signingType, string payload)
        {
            BeaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signingType, payload), signingType);
        }

        #endregion

        #region BeaconSDK

        private async void OnBeaconDappClientMessageReceived(object sender, BeaconMessageEventArgs e)
        {
            if (e.PairingDone)
            {
                _walletMessageReceiver.OnPairingCompleted("paired");
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
                    Logger.LogDebug(
                        $"{BeaconDappClient.AppName} received permissions {permissionsString} from {permissionResponse.AppMetadata.Name} with public key {permissionResponse.PublicKey}");

                    UnityMainThreadDispatcher.Enqueue(
                        _walletMessageReceiver.OnAccountConnected, //permissionResponse.PublicKey);
                        new JObject
                        {
                            ["accountInfo"] = new JObject
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
                        _walletMessageReceiver.OnContractCallInjected,
                        new JObject
                        {
                            ["transactionHash"] = operationResponse.TransactionHash,
                            ["success"] = "true"
                        }.ToString());

                    Logger.LogDebug($"Received operation with hash {operationResponse.TransactionHash}");
                    break;
                }

                case BeaconMessageType.sign_payload_response:
                {
                    if (message is not SignPayloadResponse signPayloadResponse)
                        return;

                    var senderPermissions = await BeaconDappClient
                        .PermissionInfoRepository
                        .TryReadBySenderIdAsync(signPayloadResponse.SenderId);
                    if (senderPermissions == null) return;

                    _walletMessageReceiver.OnPayloadSigned( //signPayloadResponse.Signature);
                        new JObject
                        {
                            ["signature"] = signPayloadResponse.Signature
                        }.ToString());

                    break;
                }
            }
        }
        
        public void OnReady()
        {
        }

        #endregion

        public void Dispose()
        {
            BeaconDappClient.Disconnect();
        }
    }

    // todo: this logger didn't work inside Beacon, improve this.
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
}