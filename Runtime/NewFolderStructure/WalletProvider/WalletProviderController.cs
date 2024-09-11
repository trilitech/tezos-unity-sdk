using System.Collections.Generic;
using System.Linq;
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

		public bool IsInitialized { get; private set; }

		public async Task Initialize(IContext context)
		{
			_context = context;
			_walletProviders = ReflectionHelper.CreateInstancesOfType<IWalletProvider>();
			List<Task> initTasks = new();
			foreach (IWalletProvider walletProvider in _walletProviders)
			{
				walletProvider.WalletConnected    += OnWalletConnected;
				walletProvider.WalletDisconnected += OnWalletDisconnected;
				initTasks.Add(walletProvider.Init(_context));
			}
			
			_context.MessageSystem.AddListener<WalletConnectionRequestCommand>(OnWalletConnectionRequest);
			_context.MessageSystem.AddListener<WalletDisconnectionRequestCommand>(OnWalletDisconnectionRequest);
			_context.MessageSystem.AddListener<WalletAlreadyConnectedRequestCommand>(OnWalletAlreadyConnectedRequest);

			await Task.WhenAll(initTasks);
			
			IsInitialized = true;
		}

		private void OnWalletAlreadyConnectedRequest(WalletAlreadyConnectedRequestCommand requestCommand)
		{
			// checking if there is any connection exists
			bool connected = _walletProviders.Any(wp=> wp.IsAlreadyConnected());
			_context.MessageSystem.InvokeMessage(new WalletAlreadyConnectedResultCommand(connected));
		}

		private void OnWalletConnectionRequest(WalletConnectionRequestCommand command)       => _walletProviders.First(wp => wp.WalletType == command.GetData().WalletType).Connect(command.GetData());
		private void OnWalletDisconnectionRequest(WalletDisconnectionRequestCommand command) => _walletProviders.First(wp => wp.WalletType == command.GetData().WalletType).Disconnect();

		private void OnWalletConnected(WalletProviderData    data) => _context.MessageSystem.InvokeMessage(new WalletConnectedCommand(data));
		private void OnWalletDisconnected(WalletProviderData data) => _context.MessageSystem.InvokeMessage(new WalletDisconnectedCommand(data));
	}
}
