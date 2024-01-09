using System;
using System.Collections;
using TezosSDK.FileUploaders.IPFS;
using UnityEngine;

namespace TezosSDK.FileUploaders
{

	public abstract class BaseUploader : MonoBehaviour
	{
		public string SupportedFileExtensions
		{
			get => ".jpg, .jpeg, .png";
		}

		private void Start()
		{
			DontDestroyOnLoad(gameObject);
		}
	}

	public interface IPinataUploader : IBaseUploader
	{
		PinataCredentials PinataCredentials { get; set; }
	}

	public interface IBaseUploader
	{
		string SupportedFileExtensions { get; }

		/// <summary>
		///     Upload file that user will select through native menu file picker.
		/// </summary>
		/// <param name="callback">
		///     Executes after asset uploaded with data address.
		/// </param>
		IEnumerator UploadFile(Action<string> callback);
	}

}