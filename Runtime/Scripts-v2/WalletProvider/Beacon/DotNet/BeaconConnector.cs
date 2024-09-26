using System;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.Operation;
using UnityEngine;
using SignPayloadType = Beacon.Sdk.Beacon.Sign.SignPayloadType;

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

		public async UniTask RequestOperation(OperationRequest operationRequest)
		{
			TezosLogger.LogDebug($"RequestOperation, _operationRequestHandler is null:{_operationRequestHandler is null}");
			Application.OpenURL("tezos://");
			await _operationRequestHandler.RequestTezosOperation(
			                                                     operationRequest.Destination, operationRequest.EntryPoint, operationRequest.Arg, operationRequest.Amount,
			                                                     _beaconProvider.BeaconDappClient
			                                                    );
		}

		public async UniTask RequestContractOrigination(OriginateContractRequest originationRequest)
		{
			TezosLogger.LogDebug("RequestContractOrigination - BeaconDotNet");
			await _operationRequestHandler.RequestContractOrigination(originationRequest.Script, originationRequest.DelegateAddress, _beaconProvider.BeaconDappClient);
		}

		public async UniTask RequestSignPayload(SignPayloadRequest signRequest)
		{
			TezosLogger.LogDebug("RequestSignPayload");
			Application.OpenURL("tezos://");
			SignPayloadType signPayloadType = Enum.Parse<SignPayloadType>(signRequest.SigningType.ToString().ToLowerInvariant());
			await _beaconProvider.BeaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signPayloadType, signRequest.Payload), signPayloadType);
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

			var url = $"tezos://?type=tzip10&data={data}";
				TezosLogger.LogDebug("Opening URL: " + url);
				Application.OpenURL(url);
		}
#endif
	}
}