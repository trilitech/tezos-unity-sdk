using System;
using System.Collections.Generic;
using Beacon.Sdk.Beacon.Operation;
using Tezos.Common;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.MessageSystem;
using Tezos.Reflection;

namespace Tezos.WalletProvider
{
	public class WalletProviderController : IController
	{
		public event Action<string> PairingRequested; 
		
		private List<IWalletProvider> _walletProviders;
		private IContext              _context;
		private WalletProviderData    _connectedWalletData;

		public bool IsInitialized { get; private set; }

		public async UniTask Initialize(IContext context)
		{
			_context         = context;
			_walletProviders = new List<IWalletProvider>(ReflectionHelper.CreateInstancesOfType<IWalletProvider>());
			List<UniTask> initTasks = new();
			foreach (IWalletProvider walletProvider in _walletProviders)
			{
				walletProvider.PairingRequested += OnPairingRequested;
				initTasks.Add(walletProvider.Init(_context));
			}

			await UniTask.WhenAll(initTasks);
			IsInitialized = true;
		}

		private void OnPairingRequested(string pairingData) => PairingRequested?.Invoke(pairingData);

		public bool               IsWalletConnected()     => !string.IsNullOrEmpty(_connectedWalletData?.WalletAddress);
		public WalletProviderData GetWalletProviderData() => _connectedWalletData;

		public async UniTask<WalletProviderData> Connect(WalletProviderData walletProviderData)
		{
			_connectedWalletData = await _walletProviders.Find(wp => wp.WalletType == walletProviderData.WalletType).Connect(walletProviderData);
			return _connectedWalletData;
		}

		public async UniTask<bool> Disconnect()
		{
			bool result = await _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).Disconnect();
			_connectedWalletData = null;
			return result;
		}

		public UniTask<OperationResponse>  RequestOperation(WalletOperationRequest                 walletOperationRequest)         => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).RequestOperation(walletOperationRequest);
		public UniTask<WalletProviderData> RequestSignPayload(WalletSignPayloadRequest             walletSignPayloadRequest)       => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).RequestSignPayload(walletSignPayloadRequest);
		public UniTask                     RequestOriginateContract(WalletOriginateContractRequest walletOriginateContractRequest) => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).RequestContractOrigination(walletOriginateContractRequest);
	}
}