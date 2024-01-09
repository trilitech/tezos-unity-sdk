#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.FileUploaders.IPFS
{

	public class EditorPinataUploader : BaseUploader, IPinataUploader
	{
		#region IBaseUploader Implementation

		public IEnumerator UploadFile(Action<string> callback)
		{
			yield return null;

			var path = EditorUtility.OpenFilePanel("Select image", string.Empty,
				SupportedFileExtensions.Replace(".", string.Empty).Replace(" ", string.Empty));

			var filename = Path.GetFileName(path);
			var form = new WWWForm();
			form.AddBinaryData("file", File.ReadAllBytes(path), filename);

			var request = UnityWebRequest.Post(PinataCredentials.ApiUrl, form);
			request.SetRequestHeader("Authorization", $"Bearer {PinataCredentials.ApiKey}");
			yield return request.SendWebRequest();

			if (request.result == UnityWebRequest.Result.Success)
			{
				var ipfsResponse = JsonSerializer.Deserialize<IpfsResponse>(request.downloadHandler.text);
				callback.Invoke($"ipfs://{ipfsResponse.IpfsHash}");
			}
			else
			{
				Logger.LogError($"Error during upload to IPFS {request.downloadHandler.text}");
			}
		}

		#endregion

		#region IPinataUploader Implementation

		public PinataCredentials PinataCredentials { get; set; }

		#endregion
	}

}
#endif