using System;
using System.Collections;

namespace TezosSDK.Scripts.IpfsUploader
{
    public interface IFileUploader
    {
        string ApiUrl { get; }
        string ApiKey { get; }
        string SupportedFileExtensions { get; }

        /// <summary>
        /// Upload file that user will select through native menu file picker.
        /// </summary>
        /// <param name="callback">Response with uploaded file ID (e.g. IPFS file hash).</param>
        IEnumerator UploadFile(Action<string> callback);
    }
}