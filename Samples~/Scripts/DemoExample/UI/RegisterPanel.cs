using System;
using System.Collections;
using Unity.VisualScripting;
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
		var payload = "05010000004254657a6f73205369676e6564204d6573736167653a206d796461707" +
		              "02e636f6d20323032312d30312d31345431353a31363a30345a2048656c6c6f20776f726c6421";
		
		ExampleFactory.Instance.GetExampleManager().RequestSignPayload(0, payload);
	}

	public void VerifySignatureTest()
	{
		var payload = "05010000004254657a6f73205369676e6564204d6573736167653a206d796461707" +
		              "02e636f6d20323032312d30312d31345431353a31363a30345a2048656c6c6f20776f726c6421";
		
		var verified = ExampleFactory.Instance.GetExampleManager().VerifyPayload(payload);
		Debug.Log("Verification success: " + verified);
	}
}

