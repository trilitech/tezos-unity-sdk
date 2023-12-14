import BaseWallet from "./BaseWallet";
import { AccountInfo, Wallet } from "./Types";
import { KukaiEmbed } from "kukai-embed";

class KukaiWallet extends BaseWallet implements Wallet {
  client: KukaiEmbed | null;
  networkName: string;
  initInProgress: boolean;

  constructor(appName: string, appUrl: string, iconUrl: string) {
    super(appName, appUrl, iconUrl);
  }

  SetNetwork(networkName: string, rpcUrl: string) {
    this.networkName = networkName;
    localStorage.setItem("networkName", networkName);
  }

  async ConnectAccount() {
    if (!this.client?.initialized) {
      if (this.initInProgress) return;
      this.client = new KukaiEmbed({
        net: this.networkName,
      });
      
      this.initInProgress = true;
      await this.client.init();
      this.initInProgress = false;
    }

    if (this.client.user) {
      this.CallUnityOnAccountConnected({
        address: this.client.user.pkh,
        publicKey: this.client.user.pk,
      });
    } else {
      try {
        await this.client.login();
        this.CallUnityOnAccountConnected({
          address: this.client.user.pkh,
          publicKey: this.client.user.pk,
        });
      } catch (error) {
        console.error(`Error during connecting account, ${error.message}`);
        this.CallUnityOnAccountFailedToConnect(error);
      }
    }
  }

  GetActiveAccountAddress() {
    return this.client?.user?.pkh ?? "";
  }

  async SendContract(
    destination: string,
    amount: string,
    entryPoint: string,
    parameter: string
  ) {
    try {
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
    const signature = await this.client.signExpr(hexPayload);
    this.CallUnityOnPayloadSigned({ signature });
  }

  async DisconnectAccount() {
    const accountInfo: AccountInfo = {
      publicKey: this.client?.user?.pk,
      address: this.GetActiveAccountAddress(),
    };

    await this.client.logout();
    localStorage.removeItem("networkName");
    this.CallUnityOnAccountDisconnected(accountInfo);
  }
}

export default KukaiWallet;
