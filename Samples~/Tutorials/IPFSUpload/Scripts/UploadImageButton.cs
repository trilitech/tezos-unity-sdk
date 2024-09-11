using TezosSDK.FileUploaders;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Managers;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.IPFSUpload
{

	public class UploadImageButton : MonoBehaviour
	{
		public void HandleUploadClick()
		{
			if (string.IsNullOrEmpty(TezosManager.Instance.Config.PinataApiKey))
			{
				//TezosLogger.LogError("Can not proceed without Pinata API key.");
				return;
			}

			var uploader = UploaderFactory.GetPinataUploader(TezosManager.Instance.Config.PinataApiKey);

			var uploadCoroutine = uploader.UploadFile(ipfsUrl =>
			{
				//TezosLogger.LogDebug($"File uploaded, url is {ipfsUrl}");
			});

			StartCoroutine(uploadCoroutine);
		}
	}

}