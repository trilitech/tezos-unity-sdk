import BeaconWallet from './WalletProviders/Beacon';
import IpfsUploader from './IpfsUploader/IpfsUploader';
import KukaiWallet from './WalletProviders/Kukai';
import { FileUploader, FileUploaderConfig } from './IpfsUploader/Types';
import { Wallet, WalletType } from './WalletProviders/Types';

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

function InitFileUploader(config: FileUploaderConfig) {
  if (window.FileUploader) return;
  const ipfsUploader: FileUploader = new IpfsUploader();
  ipfsUploader.InitFileUploader(config);

  window.FileUploader = ipfsUploader;
}

window.InitWalletProvider = InitWalletProvider;
window.InitFileUploader = InitFileUploader;

declare global {
  interface Window {
    unityInstance: any;
    WalletProvider: Wallet | null;
    InitWalletProvider(
      networkName: string,
      rpcUrl: string,
      walletType: WalletType
    ): void;
    InitFileUploader(config: FileUploaderConfig): void;
    FileUploader: FileUploader | null;
  }
}
