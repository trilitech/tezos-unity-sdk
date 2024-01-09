import Base64Uploader from "./FileUploaders/Base64Uploader";
import BeaconWallet from "./WalletProviders/Beacon";
import IpfsUploader from "./FileUploaders/IpfsUploader";
import KukaiWallet from "./WalletProviders/Kukai";
import { AccountInfo, DAppClient } from "@airgap/beacon-sdk";
import { Wallet, WalletType } from "./WalletProviders/Types";
import BaseWallet from "./WalletProviders/BaseWallet";

import {
  BaseFileUploaderType,
  BaseUploaderConfig,
  IpfsUploaderConfig,
  IpfsUploaderType,
} from "./FileUploaders/Types";

let kukaiWallet: KukaiWallet;
let beaconWallet: BeaconWallet;

function InitWalletProvider(
  networkName: string,
  rpcUrl: string,
  walletType: WalletType,
  appName: string,
  appUrl: string,
  iconUrl: string
) {
  
    console.log("InitWalletProvider");
    console.log("networkName", networkName);
    console.log("rpcUrl", rpcUrl);
    console.log("walletType", walletType);
    console.log("appName", appName);
    console.log("appUrl", appUrl);
    console.log("iconUrl", iconUrl);
    
  
  if (walletType === WalletType.kukai) {
    if (!kukaiWallet) kukaiWallet = new KukaiWallet(appName, appUrl, iconUrl);
    window.WalletProvider = kukaiWallet;
  }

  if (walletType === WalletType.beacon) {
    if (!beaconWallet)
      beaconWallet = new BeaconWallet(appName, appUrl, iconUrl);
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
  
  console.log("UnityReadyEvent");
  
  const dAppClient: DAppClient = new DAppClient({ name: "Tezos SDK DApp" });
  const beaconActiveAccount: AccountInfo = await dAppClient.getActiveAccount();

  const dappName = localStorage.getItem("dappName");
  const dappUrl = localStorage.getItem("dappUrl");
  const iconUrl = localStorage.getItem("iconUrl");

  console.log("beaconActiveAccount", beaconActiveAccount);
  
  if (beaconActiveAccount) {
    InitWalletProvider(
      beaconActiveAccount.network.type,
      beaconActiveAccount.network.rpcUrl,
      WalletType.beacon,
      dappName,
      dappUrl,
      iconUrl
    );

    window.WalletProvider.client = dAppClient;
    window.WalletProvider.ConnectAccount();
  } else {
    const networkName = localStorage.getItem("networkName");
    console.log("No active account found, trying to connect to: ", networkName);
    
    if (networkName) {
      InitWalletProvider(
        networkName,
        "",
        WalletType.kukai,
        dappName,
        dappUrl,
        iconUrl
      );
      window.WalletProvider.ConnectAccount();
    }
  }

  if (window.WalletProvider instanceof BaseWallet) {
    window.WalletProvider.CallUnityOnSDKInitialized();
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
      walletType: WalletType,
      appName: string,
      appUrl: string,
      iconUrl: string
    ): void;
    InitIpfsUploader(config: IpfsUploaderConfig): void;
    InitBase64Uploader(config: BaseUploaderConfig): void;
    FileUploader: BaseFileUploaderType | null;
  }
}
