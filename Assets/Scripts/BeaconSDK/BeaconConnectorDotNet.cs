using System;
//using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Beacon.Sdk;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.BeaconClients.Abstract;
using Beacon.Sdk.Core.Domain.Services;
using Microsoft.Extensions.Logging;
using Netezos.Encoding;
using Netezos.Keys;
using Newtonsoft.Json.Linq;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BeaconSDK
{
	public class BeaconConnectorDotNet : IBeaconConnector
	{
		private static BeaconMessageReceiver _messageReceiver;
		private DappBeaconClient BeaconDappClient { get; set; }
		
		private PermissionResponse _permission;
		private string _address;
			
		const string DbPath = "dapp-sample.db";

		private string _network;
		private string _rpc;
		
		#region IBeaconConnector

		public async void ConnectAccount()
		{
			var options = new BeaconOptions
			{
				AppName = "Dapp sample",
				AppUrl = "https://awesome-dapp.com",
				IconUrl = "https://bcd-static-assets.fra1.digitaloceanspaces.com/dapps/atomex/atomex_logo.jpg",
				KnownRelayServers = Constants.KnownRelayServers,
				
				DatabaseConnectionString = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? $"Filename={DbPath}; Connection=Shared;"
					: $"Filename={DbPath}; Mode=Exclusive;"
				
			};
			
			BeaconDappClient = (DappBeaconClient)BeaconClientFactory.Create<IDappBeaconClient>(options, new MyLoggerProvider());
			BeaconDappClient.OnBeaconMessageReceived += OnBeaconDappClientMessageReceived;


			await BeaconDappClient.InitAsync();
			Debug.Log("Dapp initialized");
			BeaconDappClient.Connect();
			Debug.Log("Dapp connected");
			
			
			string pairingRequestQrData = await BeaconDappClient.GetPairingRequestInfo();
			_messageReceiver.OnHandshakeReceived(pairingRequestQrData);


			// Sign-on feature. Disabled for feature parity and making the user to log in every time
			/*
			var activePeer = BeaconDappClient.GetActivePeer().Result;

			if (activePeer != null)
			{
				var permissions = BeaconDappClient
					.PermissionInfoRepository
					.TryReadBySenderIdAsync(activePeer.SenderId)
					.Result;

				var permissionsString = permissions?.Scopes.Aggregate(string.Empty,
					(res, scope) => res + $"{scope}, ") ?? string.Empty;

				Debug.Log($"We have active peer {activePeer.Name} with permissions {permissionsString}");
			}
			*/
		}

		public string GetActiveAccountAddress()
		{
			return _address;
		}

		public void RequestHandshake()
		{

		}

		public void DisconnectAccount()
		{
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
			var network = new Beacon.Sdk.Beacon.Permission.Network
			{
				Type = Beacon.Sdk.Beacon.Permission.NetworkType.custom,
				Name = _network,//"mainnet",
				RpcUrl = _rpc//"https://rpc.tzkt.io/mainnet"
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

			// this could also be just cached from Pairing Complete
			var peer = await BeaconDappClient.GetActivePeer();

			if (peer != null)
			{
				await BeaconDappClient.SendResponseAsync(peer.SenderId, permissionRequest);
				Debug.Log("Permission request sent");
			}
			else
			{
				Debug.LogError("No active peer found");
			}
		}

		public async void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null, ulong amount = 0,
			string networkName = "", string networkRPC = "")
		{
			var operationDetails = new List<PartialTezosTransactionOperation>
			{
				new(
					Amount: amount.ToString(),
					Destination: destination,
					Parameters: new JObject
					{
						["entrypoint"] = entryPoint,
						["value"] = JObject.Parse(arg)
					})
			};

			var peer = await BeaconDappClient.GetActivePeer();
			if (peer == null)
			{
				Debug.LogError("No active peer");
				return;
			}
			
			if (_permission == null)
			{
				Debug.LogError("No active permissions");
				return;
			}
			
			var pubKey = PubKey.FromBase58(_permission.PublicKey);
			
			var operationRequest = new OperationRequest(
				type: BeaconMessageType.operation_request,
				version: Constants.BeaconVersion,
				id: KeyPairService.CreateGuid(),
				senderId: BeaconDappClient.SenderId,
				network: _permission.Network,
				operationDetails: operationDetails,
				sourceAddress: pubKey.Address);

			Debug.Log("requesting operation: " + operationRequest );
			await BeaconDappClient.SendResponseAsync(peer.SenderId, operationRequest);
		}

		public async void RequestTezosSignPayload(int signingType, string payload)
		{
			var peer = await BeaconDappClient.GetActivePeer();
			if (peer == null)
			{
				Debug.LogError("No active peer");
				return;
			}
			

			if (_permission == null)
			{
				Debug.LogError("No active permissions");
			//	_permission = await BeaconDappClient.PermissionInfoRepository.TryReadBySenderIdAsync(peer.SenderId);
				return;
			}

			var pubKey = PubKey.FromBase58(_permission.PublicKey);
			
			var signPayloadRequest = new SignPayloadRequest(
				id: KeyPairService.CreateGuid(),
				version: Constants.BeaconVersion,
				senderId: BeaconDappClient.SenderId,
				signingType: SignPayloadType.raw,
				payload: payload,
				sourceAddress: pubKey.Address);

			await BeaconDappClient.SendResponseAsync(peer.SenderId, signPayloadRequest);
		}

		public void RequestTezosBroadcast(string signedTransaction, string networkName = "", string networkRPC = "")
		{
		
		}
		
		#endregion

		#region BeaconSDK

		private IEnumerator OnAccountConnectedMainThread(Action<string> function, string parameter)
		{
			yield return null;
			function.Invoke(parameter);
		}

		private async void OnBeaconDappClientMessageReceived(object? sender, BeaconMessageEventArgs e)
		{
			if (e.PairingDone)
			{
				var peer = await BeaconDappClient.GetActivePeer();
				if (peer == null) return;
				
				Debug.Log("Paired. Active peer: " + peer.Name);
				_messageReceiver.OnPairingCompleted("paired");
				//_messageReceiver.SendMessage("OnPairingCompleted", "paired");
				
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
						$"{BeaconDappClient.AppName} received permissions {permissionsString} from {permissionResponse.AppMetadata.Name} with address {permissionResponse.Address} and public key {permissionResponse.PublicKey}"
					);

					_permission = permissionResponse;
					_address = PubKey.FromBase58(_permission.PublicKey).Address;
					
					UnityMainThreadDispatcher.Instance().Enqueue(OnAccountConnectedMainThread(

						_messageReceiver.OnAccountConnected, //permissionResponse.PublicKey);
						new JObject
						{
							["account"] = new JObject
							{
								["address"] = _address,
								["publicKey"] = permissionResponse.PublicKey
							}
						}.ToString())
					
					);
					break;
				}
				
				case BeaconMessageType.operation_response:
				{
					if (message is not OperationResponse operationResponse)
						return;

					UnityMainThreadDispatcher.Instance().Enqueue(OnAccountConnectedMainThread(
					
						_messageReceiver.OnContractCallCompleted, //operationResponse.TransactionHash);
						new JObject
						{
							["transactionHash"] = operationResponse.TransactionHash
						}.ToString())
						
						);

					Debug.Log($"Operation completed with transaction hash {operationResponse.TransactionHash}");
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
					
					_messageReceiver.OnPayloadSigned(//signPayloadResponse.Signature);
						new JObject
						{
							["signature"] = signPayloadResponse.Signature
						}.ToString());
/*
					var pubKey = PubKey.FromBase58(senderPermissions.PublicKey);
					var payloadBytes = Hex.Parse(PayloadToSign);
					var verified = pubKey.Verify(payloadBytes, signPayloadResponse.Signature);
					var stringVerifyResult = verified ? "Successfully" : "Unsuccessfully";

					Debug.Log(
						$"{stringVerifyResult} signed payload by {senderPermissions.AppMetadata.Name}, signature is {signPayloadResponse.Signature}");
*/						
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
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if(exception != null)
				Debug.LogException(exception);
				
			//Debug.Log(state.ToString());
		}

		public bool IsEnabled(LogLevel logLevel) => true;
		public IDisposable BeginScope<TState>(TState state) => null;
	}
	
	public void Dispose() { }

	public ILogger CreateLogger(string categoryName) => new MyLogger();
}