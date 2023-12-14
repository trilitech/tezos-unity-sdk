using TezosSDK.Scripts.FileUploaders;
using TezosSDK.Tezos;
using UnityEngine;
using UnityEngine.UI;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.IPFSUpload.Scripts
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Button uploadButton;

        void Start()
        {
            var authorized = !string.IsNullOrEmpty(TezosManager.Instance.Wallet.GetActiveAddress());
            uploadButton.gameObject.SetActive(authorized);
        }

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