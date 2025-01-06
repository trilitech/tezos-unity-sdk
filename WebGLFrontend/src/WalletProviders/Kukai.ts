import BaseWallet from "./BaseWallet";
import { AccountInformation, Wallet } from "./Types";
import { KukaiEmbed } from "kukai-embed";

class KukaiWallet extends BaseWallet implements Wallet {
  client: KukaiEmbed | null;
  networkName: string;
  initInProgress: boolean;

  constructor(appName: string, appUrl: string, iconUrl: string, unityObjectName: string) {
    super(appName, appUrl, iconUrl, unityObjectName);
    this.StoreCredentials();
  }

  private async StoreCredentials() {
    const storedNetwork = localStorage.getItem("networkName");
    if (storedNetwork) {
      this.networkName = storedNetwork;
    }

    const storedPk = localStorage.getItem("kukaiUserPk");
    const storedPkh = localStorage.getItem("kukaiUserPkh");
    const storedAuth = localStorage.getItem("kukaiUserAuthResponse");
    const storedUserData = localStorage.getItem("kukaiUserData");

    if (storedPk && storedPkh) {
      this.client = new KukaiEmbed({ net: this.networkName });
      await this.client.init();
      this.client.user.pk = storedPk;
      this.client.user.pkh = storedPkh;

      if (storedAuth) {
        try {
          this.client.user.authResponse = JSON.parse(storedAuth);
        } catch {}
      }

      if (storedUserData) {
        try {
          this.client.user.userData = JSON.parse(storedUserData);
        } catch {}
      }
    }
  }

  SetNetwork(networkName: string, rpcUrl: string) {
    this.networkName = networkName;
    localStorage.setItem("networkName", networkName);
  }

  async ConnectAccount() {
    if (!this.client) {
      this.client = new KukaiEmbed({ net: this.networkName });
    }

    if (!this.client.initialized) {
      if (this.initInProgress) return;
      this.initInProgress = true;
      try {
        await this.client.init();
      } finally {
        this.initInProgress = false;
      }
    }

    if (this.client.user && this.client.user.pk && this.client.user.pkh) {
      this.CallUnityOnAccountConnected({
        walletAddress: this.client.user.pkh,
        publicKey: this.client.user.pk,
        accountInfo: null
      });
      return;
    }

    try {
      await this.client.login();
      localStorage.setItem("kukaiUserPk", this.client.user.pk);
      localStorage.setItem("kukaiUserPkh", this.client.user.pkh);

      if (this.client.user.authResponse) {
        localStorage.setItem("kukaiUserAuthResponse", JSON.stringify(this.client.user.authResponse));
      }

      if (this.client.user.userData) {
        localStorage.setItem("kukaiUserData", JSON.stringify(this.client.user.userData));
      }

      this.CallUnityOnAccountConnected({
        walletAddress: this.client.user.pkh,
        publicKey: this.client.user.pk,
        accountInfo: null
      });
    } catch (error) {
      this.CallUnityOnAccountFailedToConnect(error);
    }
  }

  GetActiveAccountAddress() {
    return this.client?.user?.pkh ?? "";
  }

  async SendContract(destination: string, amount: string, entryPoint: string, parameter: string) {
    try {
      if (!this.client) {
        await this.ConnectAccount();
      }
      const transactionHash = await this.client.send(
        this.GetOperationsList(destination, amount, entryPoint, parameter)
      );
      this.CallUnityOnContractCallInjected({ transactionHash });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async OriginateContract(script: string, delegateAddress?: string) {
    try {
      const transactionHash = await this.client.send(
        // @ts-ignore
        this.GetOriginationOperationsList(script, delegateAddress)
      );
      this.CallUnityOnContractCallInjected({ transactionHash });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async SignPayload(signingType: number, plainTextPayload: string) {
    const hexPayload = this.GetHexPayloadString(
      this.NumToSigningType(signingType),
      plainTextPayload
    );
    try {
      const signature = await this.client.signExpr(hexPayload);
      this.CallUnityOnPayloadSigned({ signature });
    } catch (error) {
      this.CallUnityOnPayloadSignFailed(error);
    }
  }

  async DisconnectAccount() {
    const accountInfo: AccountInformation = {
      publicKey: this.client?.user?.pk,
      walletAddress: this.GetActiveAccountAddress(),
      accountInfo: null
    };

    localStorage.removeItem("kukaiUserPk");
    localStorage.removeItem("kukaiUserPkh");
    localStorage.removeItem("kukaiUserAuthResponse");
    localStorage.removeItem("kukaiUserData");
    localStorage.removeItem("networkName");

    await this.client?.logout();
    this.CallUnityOnAccountDisconnected(accountInfo);
  }
}

export default KukaiWallet;