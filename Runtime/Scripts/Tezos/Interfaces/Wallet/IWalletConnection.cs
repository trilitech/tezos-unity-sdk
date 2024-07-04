namespace TezosSDK.Tezos.Interfaces.Wallet
{

	public interface IWalletConnection
	{
		bool IsConnected { get; }

		void Connect();
		void Disconnect();
	}

}