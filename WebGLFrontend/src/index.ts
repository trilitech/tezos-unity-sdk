import {
  BaseFileUploaderType,
  BaseUploaderConfig,
  IpfsUploaderConfig,
  IpfsUploaderType,
} from "./FileUploaders/Types";
import { Wallet, WalletType } from "./WalletProviders/Types";

import Base64Uploader from "./FileUploaders/Base64Uploader";
import IpfsUploader from "./FileUploaders/IpfsUploader";
import BeaconWallet from "./WalletProviders/Beacon";
import KukaiWallet from "./WalletProviders/Kukai";

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

function InitIpfsUploader(config: IpfsUploaderConfig) {
  if (window.FileUploader instanceof IpfsUploader) return;

  const uploader: IpfsUploaderType = new IpfsUploader();
  uploader.Init(config);
  window.FileUploader = uploader;
}

function InitBase64Uploader(config: BaseUploaderConfig) {
  const uploader = new Base64Uploader();
  uploader.Init(config);
  window.FileUploader = uploader;
}

window.InitWalletProvider = InitWalletProvider;
window.InitIpfsUploader = InitIpfsUploader;
window.InitBase64Uploader = InitBase64Uploader;

declare global {
  interface Window {
    unityInstance: any;
    WalletProvider: Wallet | null;
    InitWalletProvider(
      networkName: string,
      rpcUrl: string,
      walletType: WalletType
    ): void;
    InitIpfsUploader(config: IpfsUploaderConfig): void;
    InitBase64Uploader(config: BaseUploaderConfig): void;
    FileUploader: BaseFileUploaderType | null;
  }
}
