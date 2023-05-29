import BeaconWallet from "./WalletProviders/Beacon";
import KukaiWallet from "./WalletProviders/Kukai";
import { Wallet, WalletType } from "./WalletProviders/Types";

let cachedKukaiWallet: KukaiWallet;
let cachedBeqaconWallet: BeaconWallet;

function InitWalletProvider(
  networkName: string,
  rpcUrl: string,
  walletType: WalletType
) {
  if (walletType === WalletType.kukai) {
    if (!cachedKukaiWallet) cachedKukaiWallet = new KukaiWallet();
    window.WalletProvider = cachedKukaiWallet;
  }

  if (walletType === WalletType.beacon) {
    if (!cachedBeqaconWallet) cachedBeqaconWallet = new BeaconWallet();
    window.WalletProvider = cachedBeqaconWallet;
  }

  window.WalletProvider.SetNetwork(networkName, rpcUrl);
}

window.InitWalletProvider = InitWalletProvider;

declare global {
  interface Window {
    unityInstance: any;
    WalletProvider: Wallet | null;
    InitWalletProvider(
      networkName: string,
      rpcUrl: string,
      walletType: WalletType
    ): void;
  }
}
