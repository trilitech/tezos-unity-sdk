using TezosSDK.WalletServices.Interfaces;

namespace TezosSDK.Tezos.Interfaces.Wallet
{
	
	public interface IWalletEventProvider
	{
		IWalletEventManager EventManager { get; }
	}

}