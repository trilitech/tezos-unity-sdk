using System.Collections.Generic;
using System.Threading.Tasks;
using TezosSDK.Common;
using TezosSDK.MessageSystem;
using TezosSDK.Reflection;

namespace TezosSDK.WalletProvider
{
	public class WalletProviderController: IController
	{
		private IEnumerable<IWalletProvider> _walletProviders;
		private IContext                     _context;

		public bool     IsInitialized { get; }
		public IContext Context       { get; }

		public async Task Initialize(IContext context)
		{
			_context = context;
			_walletProviders = ReflectionHelper.CreateInstancesOfType<IWalletProvider>();
			List<Task> initTasks = new();
			foreach (IWalletProvider walletProvider in _walletProviders)
			{
				walletProvider.WalletConnected    += OnWalletConnected;
				walletProvider.WalletDisconnected += OnWalletDisconnected;
				initTasks.Add(walletProvider.Init(this));
			}
			
			_context.MessageSystem.AddListener<WalletConnectionRequestCommand>(OnWalletConnectionRequest);
			_context.MessageSystem.AddListener<WalletDisconnectionRequestCommand>(OnWalletDisconnectionRequest);

			await Task.WhenAll(initTasks);
		}

		private void OnWalletConnectionRequest(WalletConnectionRequestCommand _)
		{
			foreach (IWalletProvider walletProvider in _walletProviders)
			{
				walletProvider.Connect();
			}
		}
		
		private void OnWalletDisconnectionRequest(WalletDisconnectionRequestCommand _)
		{
			foreach (IWalletProvider walletProvider in _walletProviders)
			{
				walletProvider.Disconnect();
			}
		}

		private void OnWalletConnected(WalletProviderData    data) => _context.MessageSystem.InvokeMessage(new WalletConnectedCommand(data));
		private void OnWalletDisconnected(WalletProviderData data) => _context.MessageSystem.InvokeMessage(new WalletDisconnectedCommand(data));
	}
}
