using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Managers;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.TransferToken
{

	public class TransferButton : MonoBehaviour
	{
		[SerializeField] private TMP_InputField id;
		[SerializeField] private TMP_InputField address;
		[SerializeField] private TMP_InputField amount;

		public void HandleTransfer()
		{
			TezosManager.Instance.Tezos.TokenContract.Transfer(TransferCompleted, address.text, int.Parse(id.text),
				int.Parse(amount.text));
		}

		private void TransferCompleted(string txHash)
		{
			TezosLogger.LogDebug($"Transfer complete with transaction hash {txHash}");
		}
	}

}