import {BaseFileUploaderType, BaseUploaderConfig, IpfsUploaderConfig, IpfsUploaderType,} from "./FileUploaders/Types";
import {Wallet, WalletType} from "./WalletProviders/Types";

import Base64Uploader from "./FileUploaders/Base64Uploader";
import IpfsUploader from "./FileUploaders/IpfsUploader";
import BeaconWallet from "./WalletProviders/Beacon";
import KukaiWallet from "./WalletProviders/Kukai";
import {AccountInfo, DAppClient} from "@airgap/beacon-sdk";

let kukaiWallet: KukaiWallet;
let beaconWallet: BeaconWallet;

function InitWalletProvider(
  networkName: string,
  rpcUrl: string,
  walletType: WalletType
) {
  if (walletType === WalletType.kukai) {
    if (!kukaiWallet) kukaiWallet = new KukaiWallet();
    window.WalletProvider = kukaiWallet;
  }
  
  if (walletType === WalletType.beacon) {
    if (!beaconWallet) beaconWallet = new BeaconWallet();
    window.WalletProvider = beaconWallet;
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
window.UnityReadyEvent = UnityReadyEvent;

async function UnityReadyEvent() {
  const dAppClient: DAppClient = new DAppClient({ name: "Tezos SDK DApp" });
  const beaconActiveAccount: AccountInfo = await dAppClient.getActiveAccount();

  if (beaconActiveAccount) {
    InitWalletProvider(beaconActiveAccount.network.type, beaconActiveAccount.network.rpcUrl, WalletType.beacon);
    window.WalletProvider.client = dAppClient;
    window.WalletProvider.ConnectAccount();
  } else {
    const networkName = localStorage.getItem("networkName")
    if (networkName) {
      InitWalletProvider(networkName, "", WalletType.kukai);
      window.WalletProvider.ConnectAccount();
    }
  }
}

declare global {
  interface Window {
    unityInstance: any;
    WalletProvider: Wallet | null;
    UnityReadyEvent(): void;
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
