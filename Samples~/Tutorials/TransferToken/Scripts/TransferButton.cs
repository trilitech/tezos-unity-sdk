using TezosSDK.Tezos.Managers;
using TMPro;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logging.Logger;


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
			Logger.LogDebug($"Transfer complete with transaction hash {txHash}");
		}
	}

}