using System;
using System.Collections;

namespace TezosSDK.FileUploaders.Interfaces
{

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