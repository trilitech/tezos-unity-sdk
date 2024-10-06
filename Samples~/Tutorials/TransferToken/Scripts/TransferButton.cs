using Tezos.API;
using Tezos.Logger;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.TransferToken
{

	public class TransferButton : MonoBehaviour
	{
		[SerializeField] private TMP_InputField id;
		[SerializeField] private TMP_InputField address;
		[SerializeField] private TMP_InputField amount;

		public async void HandleTransfer()
		{
			var hash = await TezosAPI.Transfer(address.text, int.Parse(id.text), int.Parse(amount.text));
			TezosLogger.LogDebug($"Transfer complete with transaction hash {hash}");
		}
		
	}

}