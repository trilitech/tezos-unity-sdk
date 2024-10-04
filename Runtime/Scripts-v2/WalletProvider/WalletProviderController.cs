using System;
using System.Collections.Generic;
using System.Linq;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MessageSystem;
using Tezos.Operation;
using Tezos.Provider;
using Tezos.Reflection;
using Tezos.SaveSystem;
using UnityEngine;
using OperationRequest = Tezos.Operation.OperationRequest;
using OperationResponse = Tezos.Operation.OperationResponse;

namespace Tezos.WalletProvider
{
	public class WalletProviderController : IProviderController
	{
		public event Action<WalletProviderData> WalletConnected;
		public event Action                     WalletDisconnected;
		public event Action<string>             PairingRequested;

		private const string KEY_WALLET = "key-wallet-provider";

		private List<IWalletProvider> _walletProviders;
		private WalletProviderData    _connectedWalletData;
		private SaveController        _saveController;

		public ProviderType ProviderType  => ProviderType.WALLET;
		public bool         IsConnected   => !string.IsNullOrEmpty(_connectedWalletData?.WalletAddress);
		public bool         IsInitialized { get; private set; }

		public WalletProviderController(SaveController saveController) => _saveController = saveController;

		public async UniTask Initialize(IContext context)
		{
			_connectedWalletData = await _saveController.Load<WalletProviderData>(KEY_WALLET);

#if UNITY_EDITOR || UNITY_ANDROID
			_walletProviders = ReflectionHelper.CreateInstancesOfType<IAndroidProvider>().Cast<IWalletProvider>().ToList();
#elif UNITY_IOS
			_walletProviders = ReflectionHelper.CreateInstancesOfType<IiOSProvider>().Cast<IWalletProvider>().ToList();
#elif UNITY_WEBGL
			_walletProviders = ReflectionHelper.CreateInstancesOfType<IWebGLProvider>().Cast<IWalletProvider>().ToList();
#else
			TezosLogger.LogError($"Unsupported platform:{Application.platform}");
#endif
			var initTasks = new List<UniTask>();
			foreach (var walletProvider in _walletProviders)
			{
				walletProvider.WalletConnected    += OnWalletConnected;
				walletProvider.WalletDisconnected += OnWalletDisconnected;
				walletProvider.PairingRequested   += OnPairingRequested;
				initTasks.Add(walletProvider.Init());
			}

			await UniTask.WhenAll(initTasks);
			IsInitialized = true;
		}

		private async void OnWalletConnected(WalletProviderData walletProviderData)
		{
			await _saveController.Save(KEY_WALLET, walletProviderData);
			WalletConnected?.Invoke(walletProviderData);
		}

		private void OnWalletDisconnected()
		{
			TezosLogger.LogDebug($"Wallet disconnected");
			_connectedWalletData = null;
			_saveController.Delete(KEY_WALLET);
			WalletDisconnected?.Invoke();
		}

		private void OnPairingRequested(string pairingData) => PairingRequested?.Invoke(pairingData);

		public WalletProviderData GetWalletProviderData() => _connectedWalletData;

		public IWalletProvider GetWalletProvider<T>() where T : IWalletProvider => _walletProviders.Find(p => p is T);

		public async UniTask<WalletProviderData> Connect(WalletProviderData walletProviderData)
		{
			_connectedWalletData = await _walletProviders.Find(wp => wp.WalletType == walletProviderData.WalletType).Connect(walletProviderData);
			return _connectedWalletData;
		}

		public async UniTask<bool> Disconnect()
		{
			bool result = await _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).Disconnect();
			return result;
		}

		public UniTask<string>              GetBalance()                                                                        => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData?.WalletType).GetBalance(_connectedWalletData.WalletAddress);
		public UniTask<OperationResponse>   RequestOperation(OperationRequest                   walletOperationRequest)         => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData?.WalletType).RequestOperation(walletOperationRequest);
		public UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest               walletSignPayloadRequest)       => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData?.WalletType).RequestSignPayload(walletSignPayloadRequest);
		public UniTask                      RequestContractOrigination(OriginateContractRequest walletOriginateContractRequest) => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData?.WalletType).RequestContractOrigination(walletOriginateContractRequest);
	}
}