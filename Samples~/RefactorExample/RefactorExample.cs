using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Netezos.Encoding;
using Newtonsoft.Json;
using Tezos.API;
using Tezos.Operation;
using Tezos.QR;
using Tezos.SocialLoginProvider;
using Tezos.WalletProvider;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefactorExample : MonoBehaviour
{
	[SerializeField] private TMP_Text        _informationText;
	[SerializeField] private TMP_Text        _payloadText;
	[SerializeField] private Button          _connectButton;
	[SerializeField] private Button          _disconnectButton;
	[SerializeField] private Button          _logInButton;
	[SerializeField] private Button          _fetchBalanceButton;
	[SerializeField] private Button          _signPayloadButton;
	[SerializeField] private Button          _sendOperationButton;
	[SerializeField] private QrCodeGenerator _qrCodeGenerator;
	[SerializeField] private GameObject      _qrCodeContainer;

	private async void Awake()
	{
		await TezosAPI.WaitUntilSDKInitialized();

		TezosAPI.WalletConnected    += OnWalletConnected;
		TezosAPI.WalletDisconnected += OnWalletDisconnected;
		TezosAPI.PairingRequested   += OnPairingRequested;

		_connectButton.onClick.AddListener(OnConnectClicked);
		_disconnectButton.onClick.AddListener(OnDisconnectClicked);
		_logInButton.onClick.AddListener(OnLogInClicked);
		_fetchBalanceButton.onClick.AddListener(OnFetchBalanceClicked);
		_signPayloadButton.onClick.AddListener(OnSignPayloadClicked);
		_sendOperationButton.onClick.AddListener(OnSendOperationClicked);

		if (TezosAPI.IsConnected()) _informationText.text = TezosAPI.GetConnectionAddress();
	}

	private void OnWalletConnected(WalletProviderData walletProviderData)
	{
		Debug.Log($"OnWalletConnected received, walletAddress{walletProviderData.WalletAddress}");
		_informationText.text = $"{walletProviderData.WalletAddress}";
	}

	private async void OnSendOperationClicked()
	{
		try
		{
			var parameter = new MichelinePrim
			                {
				                Prim = PrimType.Pair,
				                Args = new List<IMicheline>
				                       {
					                       new MichelineInt(11000856),
					                       new MichelinePrim
					                       {
						                       Prim = PrimType.Pair,
						                       Args = new List<IMicheline>
						                              {
							                              new MichelineInt(1),
							                              new MichelinePrim
							                              {
								                              Prim = PrimType.Pair,
								                              Args = new List<IMicheline>
								                                     {
									                                     new MichelinePrim { Prim = PrimType.None },
									                                     new MichelinePrim
									                                     {
										                                     Prim = PrimType.Pair,
										                                     Args = new List<IMicheline>
										                                            {
											                                            new MichelinePrim { Prim = PrimType.None },
											                                            new MichelineArray()
										                                            }
									                                     }
								                                     }
							                              }
						                              }
					                       }
				                       }
			                }.ToJson();

			const string entryPoint  = "fulfill_ask";
			const string destination = "KT1MFWsAXGUZ4gFkQnjByWjrrVtuQi4Tya8G";
			const string amount      = "1500000";

			var req = new OperationRequest
			          {
				          Destination = destination,
				          EntryPoint  = entryPoint,
				          Arg         = parameter,
				          Amount      = amount
			          };

			_payloadText.text = (await TezosAPI.RequestOperation(req)).TransactionHash;
		}
		catch (Exception e)
		{
			_informationText.text = $"{e.Message}";
		}
	}

	private void OnWalletDisconnected() => _informationText.text = $"Wallet disconnected";

	private void OnPairingRequested(string pairingData)
	{
		_qrCodeContainer.SetActive(true);
		_qrCodeGenerator.SetQrCode(pairingData);
	}

	private async void OnSignPayloadClicked()
	{
		try
		{
			var payload    = "Hello World!";
			var bytes      = Encoding.UTF8.GetBytes(payload);
			var hexPayload = BitConverter.ToString(bytes);
			hexPayload = hexPayload.Replace("-", "");
			hexPayload = "05" + hexPayload;
			var result = await TezosAPI.RequestSignPayload(
			                                               new SignPayloadRequest
			                                               {
				                                               Payload     = hexPayload,
				                                               SigningType = SignPayloadType.MICHELINE
			                                               }
			                                              );
			_payloadText.text = $"Signature: {result.Signature}";
		}
		catch (Exception e)
		{
			_informationText.text = $"{e.Message}";
			_payloadText.text     = $"{e.StackTrace}";
		}
	}

	private async void OnFetchBalanceClicked()
	{
		try
		{
			var   balance          = ulong.Parse(await TezosAPI.GetBalance());
			float convertedBalance = balance / 1000000f;
			_informationText.text = $"Balance: {convertedBalance}";
		}
		catch (Exception e)
		{
			_informationText.text = e.Message;
			_payloadText.text     = e.StackTrace;
		}
	}

	private async void OnConnectClicked()
	{
		try
		{
			var result = await TezosAPI.ConnectWallet(new WalletProviderData { WalletType = WalletType.WALLETCONNECT });
			OnWalletConnected(result);
		}
		catch (Exception e)
		{
			_informationText.text = e.Message;
		}
	}

	private async void OnDisconnectClicked()
	{
		try
		{
			var result = await TezosAPI.Disconnect();
			_informationText.text = $"{result}";
		}
		catch (Exception e)
		{
			_informationText.text = e.Message;
		}
	}

	private async void OnLogInClicked()
	{
		try
		{
			var result = await TezosAPI.SocialLogIn(new SocialProviderData { SocialLoginType = SocialLoginType.Kukai });
			_informationText.text = $"{result.WalletAddress}";
		}
		catch (Exception e)
		{
			_informationText.text = e.Message;
		}
	}
}