using System;
using System.Collections.Generic;
using Tezos.Common;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.MessageSystem;
using Tezos.Operation;
using Tezos.Reflection;
using Tezos.SaveSystem;
using OperationRequest = Tezos.Operation.OperationRequest;
using OperationResponse = Tezos.Operation.OperationResponse;

namespace Tezos.WalletProvider
{
	public class WalletProviderController : IController
	{
		public event Action<string> PairingRequested;
		public event Action         WalletDisconnected;

		private const string KEY_WALLET = "key-wallet-provider";

		private List<IWalletProvider> _walletProviders;
		private IContext              _context;
		private WalletProviderData    _connectedWalletData;
		private SaveController        _saveController;

		public bool IsInitialized { get; private set; }

		public WalletProviderController(SaveController saveController) => _saveController = saveController;

		public async UniTask Initialize(IContext context)
		{
			_context         = context;
			_walletProviders = new List<IWalletProvider>(ReflectionHelper.CreateInstancesOfType<IWalletProvider>());
			List<UniTask> initTasks = new();
			foreach (IWalletProvider walletProvider in _walletProviders)
			{
				walletProvider.PairingRequested   += OnPairingRequested;
				walletProvider.WalletDisconnected += OnWalletDisconnected;
				initTasks.Add(walletProvider.Init(_context));
			}

			await UniTask.WhenAll(initTasks);
			_connectedWalletData = await _saveController.Load<WalletProviderData>(KEY_WALLET);
			IsInitialized        = true;
		}

		private void OnWalletDisconnected() => WalletDisconnected?.Invoke();

		private void OnPairingRequested(string pairingData) => PairingRequested?.Invoke(pairingData);

		public bool               IsWalletConnected()     => !string.IsNullOrEmpty(_connectedWalletData?.WalletAddress);
		public WalletProviderData GetWalletProviderData() => _connectedWalletData;

		public async UniTask<WalletProviderData> Connect(WalletProviderData walletProviderData)
		{
			_connectedWalletData = await _walletProviders.Find(wp => wp.WalletType == walletProviderData.WalletType).Connect(walletProviderData);
			_saveController.Save(KEY_WALLET, _connectedWalletData);
			return _connectedWalletData;
		}

		public async UniTask<bool> Disconnect()
		{
			bool result = await _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).Disconnect();
			_connectedWalletData = null;
			_saveController.Delete(KEY_WALLET);
			OnWalletDisconnected();
			return result;
		}

		public UniTask<OperationResponse>   RequestOperation(OperationRequest                 walletOperationRequest)         => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData?.WalletType).RequestOperation(walletOperationRequest);
		public UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest             walletSignPayloadRequest)       => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData?.WalletType).RequestSignPayload(walletSignPayloadRequest);
		public UniTask                      RequestOriginateContract(OriginateContractRequest walletOriginateContractRequest) => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData?.WalletType).RequestContractOrigination(walletOriginateContractRequest);
	}
}