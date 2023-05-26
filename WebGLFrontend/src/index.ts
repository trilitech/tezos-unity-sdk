import { Wallet, WalletType } from "./WalletProviders/Types";
import KukaiWallet from "./WalletProviders/Kukai";
import BeaconWallet from "./WalletProviders/Beacon";

let cachedKukaiWallet: KukaiWallet;

function InitWalletProvider(walletType: WalletType) {
  if (walletType === WalletType.kukai) {
    if (!cachedKukaiWallet) cachedKukaiWallet = new KukaiWallet();
    window.WalletProvider = cachedKukaiWallet;
  }

  if (walletType === WalletType.beacon) {
    window.WalletProvider = new BeaconWallet();
  }
}

window.InitWalletProvider = InitWalletProvider;

declare global {
  interface Window extends Wallet {
    unityInstance: any;
    WalletProvider: Wallet | null;
    InitWalletProvider(walletType: WalletType): void;
  }
}
