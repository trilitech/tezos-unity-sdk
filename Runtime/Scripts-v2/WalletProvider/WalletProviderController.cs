using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tezos.Common;
using Tezos.MessageSystem;
using Tezos.Reflection;
using Tezos.Logger;
using UnityEngine;

namespace Tezos.WalletProvider
{
	public class WalletProviderController: IController
	{
		private List<IWalletProvider> _walletProviders;
		private IContext                     _context;
		private WalletProviderData           _connectedWalletData;

		public bool IsInitialized { get; private set; }

		public async Task Initialize(IContext context)
		{
			_context = context;
			_walletProviders = ReflectionHelper.CreateInstancesOfType<IWalletProvider>().ToList();
			List<Task> initTasks = new();
			foreach (IWalletProvider walletProvider in _walletProviders)
			{
				// walletProvider.WalletConnected    += OnWalletConnected;
				// walletProvider.WalletDisconnected += OnWalletDisconnected;
				initTasks.Add(walletProvider.Init(_context));
			}
			
			await Task.WhenAll(initTasks);
			
			IsInitialized = true;
		}

		public bool IsWalletConnected() => !string.IsNullOrEmpty(_connectedWalletData.WalletAddress);
		public WalletProviderData GetWalletProviderData() => _connectedWalletData;

		public async Task<WalletProviderData> Connect(WalletProviderData walletProviderData)
		{
			_connectedWalletData = await _walletProviders.First(wp => wp.WalletType == walletProviderData.WalletType).Connect(walletProviderData);
			return _connectedWalletData;
		}

		public async Task<bool> Disconnect()
		{
			 bool result = await _walletProviders.First(wp => wp.WalletType == _connectedWalletData.WalletType).Disconnect();
			 _connectedWalletData = null;
			 return result;
		}
		
		public Task RequestOperation(WalletOperationRequest walletOperationRequest)                          => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).RequestOperation(walletOperationRequest); 
		public Task RequestSignPayload(WalletSignPayloadRequest walletSignPayloadRequest)                    => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).RequestSignPayload(walletSignPayloadRequest);
		public Task RequestOriginateContract(WalletOriginateContractRequest walletOriginateContractRequest)  => _walletProviders.Find(wp => wp.WalletType == _connectedWalletData.WalletType).RequestContractOrigination(walletOriginateContractRequest);
	}
}
