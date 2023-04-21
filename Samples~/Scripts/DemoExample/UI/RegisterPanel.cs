using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : PanelController
{
	[SerializeField, Header("Components")]
	private Button _deepLinkPair;
	[SerializeField]
	private RawImage _qrImage;
	[SerializeField]
	private QRCodeView _qrCodeView;
	[SerializeField, Header("Manager")]
	private UIManager _uiManager;

	private bool _isMobile = false;
	
	private IExampleManager _exampleManager;

	private const string _payloadToSign = "Tezos Signed Message: Confirming my identity as Tezos Unity SDK.";

	private IEnumerator Start()
	{
		// skip a frame before start accessing Database
		yield return null;
		
		_exampleManager = ExampleFactory.Instance.GetExampleManager();
		_exampleManager.GetMessageReceiver().HandshakeReceived += (handshake) => _qrCodeView.SetQrCode(handshake);
		
#if (UNITY_IOS || UNITY_ANDROID)
		_isMobile = true;
#else
		_isMobile = false;
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
		// make QR code available for Standalone
		SetButtonState(_deepLinkPair, false, false);
		_qrImage.gameObject.SetActive(true);
#else
		if (!_isMobile)
		{
			// Disable QR button and image
			SetButtonState(_deepLinkPair, true, true);
			_deepLinkPair.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "LOGIN";
			_qrImage.gameObject.SetActive(false);
		}
		else if (!Debug.isDebugBuild)
		{
			_qrImage.gameObject.SetActive(false);
		}
#endif
	}

	/// <summary>
	/// For use on Deep Link connection button to pair to an on device wallet.
	/// </summary>
	public void DeepLinkPair()
	{
		_exampleManager.Deeplink();
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
		ExampleFactory.Instance.GetExampleManager().RequestSignPayload(_payloadToSign);
	}

	public void VerifySignatureTest()
	{
		var verified = ExampleFactory.Instance.GetExampleManager().VerifyPayload(_payloadToSign);
		Debug.Log("Verification success: " + verified);
	}
}

