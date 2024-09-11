using TezosSDK.Logger;
using UnityEngine;

namespace TezosSDK.WalletProvider
{

	public class BeaconConnector
	{
		private OperationRequestHandler _operationRequestHandler;
		private BeaconProvider _beaconProvider;

		public BeaconConnector(OperationRequestHandler operationRequestHandler) => _operationRequestHandler = operationRequestHandler;

		public async void RequestOperation(WalletOperationRequest operationRequest)
		{
			// Adjust the method to accept the WalletOperationRequest parameter
			TezosLogger.LogDebug("RequestOperation");

			Application.OpenURL("tezos://");
			await _operationRequestHandler.RequestTezosOperation(operationRequest.Destination, operationRequest.EntryPoint, operationRequest.Arg, operationRequest.Amount,
				_beaconProvider.BeaconDappClient);
		}

		public async void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
		{
			TezosLogger.LogDebug("RequestContractOrigination - BeaconDotNet");

			await _operationRequestHandler.RequestContractOrigination(originationRequest.Script, originationRequest.DelegateAddress, _beaconProvider.BeaconDappClient);
		}

		public async void RequestSignPayload(WalletSignPayloadRequest signRequest)
		{
			await _beaconProvider.BeaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signRequest.SigningType, signRequest.Payload), signRequest.SigningType);
		}

		// private void OnWalletDisconnected(WalletInfo obj)
		// {
		// 	PairingRequestData = null;
		// }

		private void OnPairingRequested(string data)
		{
			TezosLogger.LogDebug("WalletProvider.OnHandshakeReceived");

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			PairWithWallet();
#endif
		}

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		private void PairWithWallet()
		{
			//TezosLogger.LogDebug("Pairing with wallet...");

			UnityMainThreadDispatcher.Instance().Enqueue(() =>
			{
				var url = $"tezos://?type=tzip10&data={data}";
				//TezosLogger.LogDebug("Opening URL: " + url);
				Application.OpenURL(url);
			});
		}
#endif

		/// <summary>
		///     Triggered when a message/operation is sent to the wallet.
		///     We simply forward the event to any listeners.
		/// </summary>
		// private void OnBeaconMessageSent(BeaconMessageType beaconMessageType)
		// {
		// 	switch (beaconMessageType)
		// 	{
		// 		case BeaconMessageType.permission_request:
		// 			OperationRequested?.Invoke(WalletMessageType.ConnectionRequest);
		// 			break;
		// 		case BeaconMessageType.operation_request:
		// 			OperationRequested?.Invoke(WalletMessageType.OperationRequest);
		// 			break;
		// 		case BeaconMessageType.sign_payload_request:
		// 			OperationRequested?.Invoke(WalletMessageType.SignPayloadRequest);
		// 			break;
		// 		case BeaconMessageType.disconnect:
		// 			OperationRequested?.Invoke(WalletMessageType.DisconnectionRequest);
		// 			break;
		// 		default:
		// 			throw new ArgumentOutOfRangeException(nameof(beaconMessageType), beaconMessageType, null);
		// 	}
		// }
	}

}