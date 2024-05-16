using TezosSDK.FileUploaders;
using TezosSDK.Tezos.Managers;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logging.Logger;

namespace TezosSDK.Samples.Tutorials.IPFSUpload
{

	public class UploadImageButton : MonoBehaviour
	{
		public void HandleUploadClick()
		{
			if (string.IsNullOrEmpty(TezosManager.Instance.Config.PinataApiKey))
			{
				Logger.LogError("Can not proceed without Pinata API key.");
				return;
			}

			var uploader = UploaderFactory.GetPinataUploader(TezosManager.Instance.Config.PinataApiKey);

			var uploadCoroutine = uploader.UploadFile(ipfsUrl =>
			{
				Logger.LogDebug($"File uploaded, url is {ipfsUrl}");
			});

			StartCoroutine(uploadCoroutine);
		}
	}

}