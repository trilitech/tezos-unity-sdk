import BaseWallet from "./BaseWallet";
import { AccountInformation, Wallet } from "./Types";
import { KukaiEmbed } from "kukai-embed";

class KukaiWallet extends BaseWallet implements Wallet {
  client: KukaiEmbed | null;
  networkName: string;
  initInProgress: boolean;

  constructor(appName: string, appUrl: string, iconUrl: string, unityObjectName: string) {
    console.log("KukaiWallet constructor", appName, appUrl, iconUrl);
    super(appName, appUrl, iconUrl, unityObjectName);
  }

  SetNetwork(networkName: string, rpcUrl: string) {
    this.networkName = networkName;
    localStorage.setItem("networkName", networkName);
  }

  async ConnectAccount() {
    console.log("KukaiWallet ConnectAccount");
    
    if (!this.client?.initialized) {
      
      console.log("!this.client?.initialized");
      console.log("initInProgress", this.initInProgress);
      
      if (this.initInProgress) return;
      
      this.client = new KukaiEmbed({
        net: this.networkName,
      });
      
      console.log("this.client", this.client)
      
      this.initInProgress = true;
      
      try {
        console.log("this.client.init() 1");
        await this.client.init();
        console.log("this.client.init() 2");
      }
      catch (error) {
        console.error(`Error during initializing Kukai, ${error.message}`);
      }
      finally {
        this.initInProgress = false;
      }
      
    }
    
    console.log("this.client.user", this.client.user);

    if (this.client.user) {
      this.CallUnityOnAccountConnected({
        walletAddress: this.client.user.pkh,
        publicKey: this.client.user.pk,
        accountInfo: null
      });
    } else {
      console.log("KukaiWallet ConnectAccount !this.client.user");
      try {
        console.log("KukaiWallet ConnectAccount try");
        await this.client.login();
        this.CallUnityOnAccountConnected({
          walletAddress: this.client.user.pkh,
          publicKey: this.client.user.pk,
          accountInfo: null
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
      if(!this.client)
        await this.ConnectAccount();
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
    try{
      const signature = await this.client.signExpr(hexPayload);
      this.CallUnityOnPayloadSigned({ signature });
    }
    catch (error) {
      console.error(`Error during payload signing, ${error.message}`);
      this.CallUnityOnPayloadSignFailed(error);
    }
  }

  async DisconnectAccount() {
    const accountInfo: AccountInformation = {
      publicKey: this.client?.user?.pk,
      walletAddress: this.GetActiveAccountAddress(),
      accountInfo: null
    };

    await this.client?.logout();
    localStorage.removeItem("networkName");
    this.CallUnityOnAccountDisconnected(accountInfo);
  }
}

export default KukaiWallet;