import BaseWallet from "./BaseWallet";
import { KukaiEmbed } from "kukai-embed";
import { Wallet } from "./Types";

class KukaiWallet extends BaseWallet implements Wallet {
  client: KukaiEmbed | null;
  networkName: string;

  constructor(appName: string, appUrl: string, iconUrl: string) {
    super(appName, appUrl, iconUrl);
  }

  SetNetwork(networkName: string, rpcUrl: string) {
    this.networkName = networkName;
    localStorage.setItem("networkName", networkName);
  }

  async ConnectAccount() {
    if (!this.client?.initialized) {
      this.client = new KukaiEmbed({
        net: this.networkName,
      });

      await this.client.init();
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
        this.GetOperationsList(destination, amount, entryPoint, parameter),
      );

      this.CallUnityOnContractCallCompleted({ transactionHash });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async OriginateContract(script: string, delegateAddress?: string) {
    try {
      const transactionHash = await this.client.send(
        // @ts-ignore
        this.GetOriginationOperationsList(script, delegateAddress),
      );

      this.CallUnityOnContractCallCompleted({ transactionHash });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async SignPayload(signingType: number, plainTextPayload: string) {
    const hexPayload = this.GetHexPayloadString(
      this.NumToSigningType(signingType),
      plainTextPayload
    );
    const signature = await this.client.signExpr(
      hexPayload,
    );
    this.CallUnityOnPayloadSigned({ signature });
  }

  async DisconnectAccount() {
    const connectedAccount = this.GetActiveAccountAddress();
    await this.client.logout();
    localStorage.removeItem("networkName");
    this.CallUnityOnAccountDisconnected(connectedAccount);
  }
}

export default KukaiWallet;
