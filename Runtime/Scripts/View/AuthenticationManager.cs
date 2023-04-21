using System;
using System.Collections.Generic;
using TezosAPI;
using System.Text.Json;
using TezosAPI.Models;
using TezosAPI.Models.Tokens;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationManager : MonoBehaviour
{
    private ITezosAPI _tezos;
    [SerializeField] private QRCodeView qrCodeView;
    [SerializeField] private GameObject logoutPanel;
    [SerializeField] private GameObject deepLinkButton;
    [SerializeField] private GameObject qrCodePanel;
    [SerializeField] private InputField addressInputField;
    [SerializeField] private InputField contractInputField;
    [SerializeField] private Text ResultText;

    private string _address;
    private bool _isMobile;

    private string checkContract;
    private string checkAddress;

    public const int MAX_TOKENS = 20;

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

        addressInputField.onEndEdit.AddListener(delegate { OnEndEditAddress(addressInputField); });
        contractInputField.onEndEdit.AddListener(delegate { OnEndEditContract(contractInputField); });
    }

    void OnHandshakeReceived(string handshake)
    {
        EnableUI(isAuthenticated: false);
        qrCodeView.SetQrCode(handshake);
    }

    void OnAccountConnected(string result)
    {
        EnableUI(isAuthenticated: true);
        var json = JsonSerializer.Deserialize<JsonElement>(result);
        var account = json.GetProperty("account");
        _address = account.GetProperty("address").GetString();
        // Debug.Log("OnAccountConnected with address = " + _address);
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

    public void GetTokensForOwners()
    {
        var walletAddress = string.IsNullOrEmpty(checkAddress)
            ? _address
            : checkAddress;

        CoroutineRunner.Instance.StartCoroutine(_tezos.GetTokensForOwner((tbs) =>
            {
                ResultText.text = string.Empty;
                
                if (tbs == null)
                {
                    ResultText.text = $"Incorrect address - {walletAddress}";
                    Debug.Log($"Incorrect address - {walletAddress}");
                    return;
                }

                List<TokenBalance> tokens = new List<TokenBalance>(tbs);
                if (tokens.Count > 0)
                {
                    foreach (var tb in tokens)
                    {
                        var text = ResultText.text;
                        ResultText.text = text + $"{walletAddress} has {tb.Balance} tokens on contract {tb.TokenContract.Address}" + "\r\n" + "\r\n" ;
                        Debug.Log($"{walletAddress} has {tb.Balance} tokens on contract {tb.TokenContract.Address}");
                    }
                }
                else
                {
                    ResultText.text = $"{walletAddress} has no tokens";
                    Debug.Log($"{walletAddress} has no tokens");
                }
            },
            owner: walletAddress,
            withMetadata: false,
            maxItems: MAX_TOKENS,
            orderBy: new TokensForOwnerOrder.Default(0)));
    }

    public void IsHolderOfContract()
    {
        ResultText.text = string.Empty;
        
        var walletAddress = string.IsNullOrEmpty(checkAddress)
            ? _address
            : checkAddress;

        if (string.IsNullOrEmpty(checkContract))
        {
            ResultText.text = "Enter contract address";
            Debug.Log("Enter contract address");
            return;
        }

        CoroutineRunner.Instance.StartCoroutine(_tezos.IsHolderOfContract((flag) =>
            {
                var message = flag
                    ? $"{walletAddress} is HOLDER of contract {checkContract}"
                    : $"{walletAddress} is NOT HOLDER of contract {checkContract}";

                ResultText.text = message;
                Debug.Log(message);
            },
            wallet: walletAddress,
            contractAddress: checkContract));
    }

    void OnEndEditAddress(InputField input)
    {
        checkAddress = input.text;
    }

    void OnEndEditContract(InputField input)
    {
        checkContract = input.text;
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