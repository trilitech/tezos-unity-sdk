using Tezos.Common;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using UnityEngine;

namespace Tezos.WalletProvider
{
	public class BeaconConnector
	{
		private OperationRequestHandler _operationRequestHandler;
		private BeaconProvider          _beaconProvider;

		public BeaconConnector(OperationRequestHandler operationRequestHandler, BeaconProvider beaconProvider)
		{
			_beaconProvider          = beaconProvider;
			_operationRequestHandler = operationRequestHandler;
		}

		public async UniTask RequestOperation(WalletOperationRequest operationRequest)
		{
			TezosLogger.LogDebug($"RequestOperation, _operationRequestHandler is null:{_operationRequestHandler is null}");
			Application.OpenURL("tezos://");
			await _operationRequestHandler.RequestTezosOperation(
			                                                     operationRequest.Destination, operationRequest.EntryPoint, operationRequest.Arg, operationRequest.Amount,
			                                                     _beaconProvider.BeaconDappClient
			                                                    );
		}

		public async UniTask RequestContractOrigination(WalletOriginateContractRequest originationRequest)
		{
			TezosLogger.LogDebug("RequestContractOrigination - BeaconDotNet");
			await _operationRequestHandler.RequestContractOrigination(originationRequest.Script, originationRequest.DelegateAddress, _beaconProvider.BeaconDappClient);
		}

		public async UniTask RequestSignPayload(WalletSignPayloadRequest signRequest)
		{
			TezosLogger.LogDebug("RequestSignPayload");
			Application.OpenURL("tezos://");
			await _beaconProvider.BeaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signRequest.SigningType, signRequest.Payload), signRequest.SigningType);
		}

		public void PairingRequested(string data)
		{
			TezosLogger.LogDebug("WalletProvider.OnHandshakeReceived");
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			PairWithWallet(data);
#endif
		}

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		private void PairWithWallet(string data)
		{
			TezosLogger.LogDebug("Pairing with wallet...");

			UnityMainThreadDispatcher.Instance().Enqueue(() =>
			{
				var url = $"tezos://?type=tzip10&data={data}";
				TezosLogger.LogDebug("Opening URL: " + url);
				Application.OpenURL(url);
			});
		}
#endif
	}
}