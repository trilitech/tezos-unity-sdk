using System.Collections.Generic;
using Tezos.API;
using Tezos.Configs;
using Tezos.MessageSystem;
using Tezos.SocialLoginProvider;
using Tezos.WalletProvider;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.WalletConnection
{

	public class WalletConnectionHandler : MonoBehaviour
	{
		[SerializeField] private TMP_InputField nameText;
		[SerializeField] private TMP_InputField descriptionText;
		[SerializeField] private TMP_InputField balanceText;
		[SerializeField] private TMP_InputField kukaiTypeOfLoginText;
		[SerializeField] private TextMeshProUGUI kukaiSignedMessageText;
		[SerializeField] private List<GameObject> kukaiOnlyObjects;
		

		private async void Start()
		{
			var appConfig   = ConfigGetter.GetOrCreateConfig<AppConfig>();
			
			nameText.text = appConfig.AppName;
			descriptionText.text = appConfig.AppDescription;
			
			CheckKukaiOnlyObjects();

			// Subscribe to wallet events
			TezosAPI.WalletConnected += OnWalletConnected;
			TezosAPI.WalletDisconnected += OnWalletDisconnected;
			
			await TezosAPI.WaitUntilSDKInitialized();

			var result = await TezosAPI.ConnectWallet(new WalletProviderData { WalletType = WalletType.BEACON });
		}

		// Check if the wallet is Kukai and disable objects on the scene that are only for Kukai if it's not
		private void CheckKukaiOnlyObjects()
		{
			if (TezosAPI.IsSocialLoggedIn())
			{
				return;
			}

			foreach (var obj in kukaiOnlyObjects)
			{
				obj.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			TezosAPI.WalletConnected -= OnWalletConnected;
			TezosAPI.WalletDisconnected -= OnWalletDisconnected;
		}

		private async void OnWalletConnected(WalletProviderData walletProviderData)
		{
			HandleKukaiOnlyObjects();

			// Balance is in microtez, so we divide it by 1.000.000 to get tez
			var balance          = ulong.Parse(await TezosAPI.GetBalance());
			int convertedBalance = (int)(balance / 1000000);
			balanceText.text = convertedBalance + " XTZ";
		}

		// If the wallet is Kukai, display additional information
		private void HandleKukaiOnlyObjects()
		{
			if (!TezosAPI.IsSocialLoggedIn())
			{
				return;
			}

			SocialProviderData socialProviderData = TezosAPI.GetSocialLoginData();
			kukaiTypeOfLoginText.text = socialProviderData.LoginType;
			// kukaiSignedMessageText.text = kukaiConnector.AuthResponse.Message; todo: auth response not exists
		}

		private void OnWalletDisconnected()
		{
			balanceText.text = "";
		}
	}

}