using TezosSDK.Tezos.Models;

namespace TezosSDK.Tezos.Interfaces.Wallet
{

	public interface IWalletConnection
	{
		bool IsConnected { get; }
		HandshakeData HandshakeData { get; }

		void Connect(WalletProviderType walletProvider);
		void Disconnect();
	}

}