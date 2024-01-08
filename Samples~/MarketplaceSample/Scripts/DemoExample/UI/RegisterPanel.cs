using System.Collections;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Helpers;
using TezosSDK.Tezos;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TezosSDK.Samples.DemoExample
{
    public class RegisterPanel : PanelController
    {
        [SerializeField, Header("Components")] private Button _deepLinkPair;
        [SerializeField, Header("Components")] private Button _socialLoginButton;
        [SerializeField, Header("Manager")] private UIManager _uiManager;

        private IExampleManager _exampleManager;
        private const string PayloadToSign = "Tezos Signed Message: mydap.com 2021-01-14T15:16:04Z Hello world!";

        private IEnumerator Start()
        {
            // skip a frame before start accessing Database
            yield return null;

            _exampleManager = ExampleFactory.Instance.GetExampleManager();

            SetButtonState(_deepLinkPair, false, false);
            SetButtonState(_socialLoginButton, false, false);
#if UNITY_STANDALONE || UNITY_EDITOR
#elif (UNITY_IOS || UNITY_ANDROID)
		    SetButtonState(_deepLinkPair, true, true);
#elif UNITY_WEBGL
            ExampleFactory
                .Instance
                .GetExampleManager()
                .OnReady();

		    SetButtonState(_deepLinkPair, true, true);
		    SetButtonState(_socialLoginButton, true, true);
#endif
        }

        /// <summary>
        /// For use on Deep Link connection button to pair to an on device wallet.
        /// </summary>
        public void DeepLinkPair()
        {
            _exampleManager.Login(WalletProviderType.beacon);
        }

        /// <summary>
        /// Login with social networks.
        /// </summary>
        public void SocialLoginHandler()
        {
            _exampleManager.Login(WalletProviderType.kukai);
        }

        /// <summary>
        /// To unpair from wallets.
        /// </summary>
        public void UnPair()
        {
            _exampleManager.Unpair();
        }

        /// <summary>
        /// Allows to set the button's state in a single line.
        /// </summary>
        /// <param name="button">The button in question.</param>
        /// <param name="active">The state of activity of it's GameObject.</param>
        /// <param name="interactable">If the button can be interactable or not.</param>
        private void SetButtonState(Button button, bool active, bool interactable)
        {
            button.gameObject.SetActive(active);
            button.interactable = interactable;
        }

        public void SignPayloadTest()
        {
            ExampleFactory
                .Instance
                .GetExampleManager()
                .RequestSignPayload(SignPayloadType.micheline, PayloadToSign);
        }

        public void VerifySignatureTest()
        {
            var verified = ExampleFactory
                .Instance.GetExampleManager()
                .VerifyPayload(SignPayloadType.micheline, PayloadToSign);
            Debug.Log("Verification success: " + verified);
        }

        public void DeployContract()
        {
            _exampleManager.DeployContract(_ =>
            {
                _uiManager.UpdateContractAddress();
                _uiManager.UpdateContracts();
            });
        }

        public void MintFA2()
        {
            _exampleManager.MintFA2(mintedTokenBalance =>
            {
                _uiManager.DisplayPopup($"Minted token with ID: {mintedTokenBalance.TokenId}");
            });
        }


        public void ChangeContract()
        {
            _uiManager.ChangeContract();
        }
    }
}