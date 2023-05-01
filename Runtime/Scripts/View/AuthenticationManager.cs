using TezosAPI;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    [SerializeField]
    private QRCodeView qrCodeView;
    [SerializeField]
    private GameObject logoutPanel;
    [SerializeField]
    private GameObject deepLinkButton;
    [SerializeField] private GameObject qrCodePanel;

    private bool _isMobile;
    
    void Start()
    {
#if (UNITY_IOS || UNITY_ANDROID)
		_isMobile = true;
#else
        _isMobile = false;
#endif
        TezosSingleton.Instance.MessageReceiver.HandshakeReceived += OnHandshakeReceived;
        TezosSingleton.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
        TezosSingleton.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
    }

    void OnHandshakeReceived(string handshake)
    {
        EnableUI(isAuthenticated: false);
        qrCodeView.SetQrCode(handshake);
    }

    void OnAccountConnected(string result)
    {
        EnableUI(isAuthenticated: true);
        Debug.Log("OnAccountConnected");
    }

    void OnAccountDisconnected(string result)
    {
        Debug.Log("OnAccountDisconnected");
    }

    public void DisconnectWallet()
    {
        EnableUI(isAuthenticated: false);
        TezosSingleton.Instance.DisconnectWallet();
    }

    public void ConnectByDeeplink()
    {
        TezosSingleton.Instance.ConnectWallet();
    }

    void EnableUI(bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            deepLinkButton.SetActive(false);
            qrCodePanel.SetActive(false);   
        }
        else
        {
            if (_isMobile)
            {
                deepLinkButton.SetActive(true);
                qrCodePanel.SetActive(false);
            }
            else
            {
                qrCodePanel.SetActive(true);
                deepLinkButton.SetActive(false);
            }   
        }

        logoutPanel.SetActive(isAuthenticated);
    }
}