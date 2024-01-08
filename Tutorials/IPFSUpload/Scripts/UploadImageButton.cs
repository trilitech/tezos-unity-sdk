#region

using TezosSDK.FileUploaders;
using TezosSDK.Tezos;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

#endregion

namespace TezosSDK.Tutorials.IPFSUpload.Scripts
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