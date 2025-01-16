namespace TezosSDK.Tezos.Interfaces.Wallet
{

	public interface IWalletContract
	{
		void OriginateContract(string script, string delegateAddress = null);
	}

}