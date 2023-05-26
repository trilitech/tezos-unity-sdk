import { KukaiEmbed, Networks, Template } from "kukai-embed";
import BaseWallet from "./BaseWallet";
import { Wallet } from "./Types";

class KukaiWallet extends BaseWallet implements Wallet {
  kukaiEmbed: KukaiEmbed | null;
  defaultUIConfig: Template = { silent: true };

  async ConnectAccount() {
    if (!this.kukaiEmbed?.initialized) {
      this.kukaiEmbed = new KukaiEmbed({
        // todo: remove
        // net: network,
        net: Networks.local,
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
    return this.kukaiEmbed.user.pkh;
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

  async SignPayload(signingType: number, plainTextPayload: string) {
    const hexPayload = this.GetHexPayloadString(
      this.NumToSigningType(signingType),
      plainTextPayload
    );
    var signature = await this.kukaiEmbed.signExpr(
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
