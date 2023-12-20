#region

using TezosSDK.Scripts.FileUploaders;
using TezosSDK.Tezos;
using UnityEngine;
using UnityEngine.UI;
using Logger = TezosSDK.Helpers.Logger;

#endregion

namespace TezosSDK.IPFSUpload.Scripts
{

	public class UploadImageButton : MonoBehaviour
	{
		public void HandleUploadClick()
		{
			if (string.IsNullOrEmpty(TezosManager.PinataApiKey))
			{
				Logger.LogError("Can not proceed without Pinata API key.");
				return;
			}

			var uploader = UploaderFactory.GetPinataUploader(TezosManager.PinataApiKey);

			var uploadCoroutine = uploader.UploadFile(ipfsUrl =>
			{
				Logger.LogDebug($"File uploaded, url is {ipfsUrl}");
			});

			StartCoroutine(uploadCoroutine);
		}
	}

}