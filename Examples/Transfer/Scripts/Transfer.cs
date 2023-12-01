using TezosSDK.Tezos;
using TMPro;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Transfer.Scripts
{

	public class Transfer : MonoBehaviour
	{
		[SerializeField] private TMP_InputField id;
		[SerializeField] private TMP_InputField address;
		[SerializeField] private TMP_InputField amount;

		public void HandleTransfer()
		{
			TezosManager
				.Instance
				.Tezos
				.TokenContract
				.Transfer(
					completedCallback: TransferCompleted,
					destination: address.text,
					tokenId: int.Parse(id.text),
					amount: int.Parse(amount.text));
		}

		private void TransferCompleted(string txHash)
		{
			Logger.LogDebug($"Transfer complete with transaction hash {txHash}");
		}
	}

}