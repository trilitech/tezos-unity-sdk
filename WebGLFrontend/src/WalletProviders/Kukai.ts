import BaseWallet from "./BaseWallet";
import { KukaiEmbed, Template } from "kukai-embed";
import { Wallet } from "./Types";

class KukaiWallet extends BaseWallet implements Wallet {
  kukaiEmbed: KukaiEmbed | null;
  defaultUIConfig: Template = { silent: true };
  networkName: string;

  SetNetwork(networkName: string, rpcUrl: string) {
    this.networkName = networkName;
  }

  async ConnectAccount() {
    if (!this.kukaiEmbed?.initialized) {
      this.kukaiEmbed = new KukaiEmbed({
        net: this.networkName,
      });

      await this.kukaiEmbed.init();
    }

    if (this.kukaiEmbed.user) {
      this.CallUnityOnAccountConnected({
        address: this.kukaiEmbed.user.pkh,
        publicKey: this.kukaiEmbed.user.pk,
      });
    } else {
      try {
        await this.kukaiEmbed.login();
        this.CallUnityOnAccountConnected({
          address: this.kukaiEmbed.user.pkh,
          publicKey: this.kukaiEmbed.user.pk,
        });
      } catch (error) {
        console.error(`Error during connecting account, ${error.message}`);
        this.CallUnityOnAccountFailedToConnect(error);
      }
    }
  }

  GetActiveAccountAddress() {
    return this.kukaiEmbed?.user?.pkh ?? "";
  }

  async SendContract(
    destination: string,
    amount: string,
    entryPoint: string,
    parameter: string
  ) {
    try {
      const transactionHash = await this.kukaiEmbed.send(
        this.GetOperationsList(destination, amount, entryPoint, parameter),
        this.defaultUIConfig
      );

      this.CallUnityOnContractCallCompleted({ transactionHash });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async OriginateContract(script: string, delegateAddress?: string) {
    try {
      const transactionHash = await this.kukaiEmbed.send(
        // @ts-ignore
        this.GetOriginationOperationsList(script, delegateAddress),
        this.defaultUIConfig
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
    const signature = await this.kukaiEmbed.signExpr(
      hexPayload,
      this.defaultUIConfig
    );
    this.CallUnityOnPayloadSigned({ signature });
  }

  async DisconnectAccount() {
    const connectedAccount = this.GetActiveAccountAddress();
    await this.kukaiEmbed.logout();
    this.CallUnityOnAccountDisconnected(connectedAccount);
  }
}

export default KukaiWallet;
