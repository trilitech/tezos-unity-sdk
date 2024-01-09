#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEditor;

namespace TezosSDK.FileUploaders.OnChain
{

	public class EditorBase64Uploader : BaseUploader, IBaseUploader
	{
		#region IBaseUploader Implementation

		public IEnumerator UploadFile(Action<string> callback)
		{
			yield return null;

			var imagePath = EditorUtility.OpenFilePanel("Select image", string.Empty,
				SupportedFileExtensions.Replace(".", string.Empty).Replace(" ", string.Empty));

			callback.Invoke(ConvertImageToBase64(imagePath));
		}

		#endregion

		private static string ConvertImageToBase64(string imagePath)
		{
			var fileExtension = Path.GetExtension(imagePath).Replace(".", string.Empty).ToLower();

			var imageBytes = File.ReadAllBytes(imagePath);
			var base64String = Convert.ToBase64String(imageBytes);
			return $"data:image/{fileExtension};base64,{base64String}";
		}
	}

}
#endif