using TezosAPI;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    private ITezosAPI _tezos;
    [SerializeField] private QRCodeView qrCodeView;
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject deepLinkButton;
    [SerializeField] private GameObject logoutButton;
    [SerializeField] private GameObject qrCodePanel;
    
    private bool _isMobile;

    void Start()
    {
#if (UNITY_IOS || UNITY_ANDROID)
		_isMobile = true;
#else
        _isMobile = false;
#endif
        _tezos = TezosSingleton.Instance;

        _tezos.MessageReceiver.HandshakeReceived += OnHandshakeReceived;
        _tezos.MessageReceiver.AccountConnected += OnAccountConnected;
        _tezos.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
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
        _tezos.DisconnectWallet();
    }

    public void ConnectByDeeplink()
    {
        _tezos.ConnectWallet();
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
        
        logoutButton.SetActive(isAuthenticated);
        
        if (contentPanel == null) return;
        contentPanel.SetActive(isAuthenticated);
    }
}