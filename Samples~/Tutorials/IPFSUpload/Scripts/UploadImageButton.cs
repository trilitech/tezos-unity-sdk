using Tezos.Configs;
using Tezos.MessageSystem;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.IPFSUpload
{

	public class UploadImageButton : MonoBehaviour
	{
		// public void HandleUploadClick()
		// {
		// 	if (string.IsNullOrEmpty(ConfigGetter.GetOrCreateConfig<TezosConfig>().PinataApiKey))
		// 	{
		// 		//TezosLogger.LogError("Can not proceed without Pinata API key.");
		// 		return;
		// 	}
		//
		// 	var uploader = UploaderFactory.GetPinataUploader(TezosManager.Instance.Config.PinataApiKey);
		//
		// 	var uploadCoroutine = uploader.UploadFile(ipfsUrl =>
		// 	{
		// 		//TezosLogger.LogDebug($"File uploaded, url is {ipfsUrl}");
		// 	});
		//
		// 	StartCoroutine(uploadCoroutine);
		// }
	}

}