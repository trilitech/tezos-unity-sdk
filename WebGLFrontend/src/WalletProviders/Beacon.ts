import BaseWallet from "./BaseWallet";
import { AccountInfo, Wallet } from "./Types";
import { DAppClient } from "@airgap/beacon-sdk";
import { Network, NetworkType } from "@airgap/beacon-types";

class BeaconWallet extends BaseWallet implements Wallet {
  client: DAppClient | null;
  activeAddress: string | null;
  networkType: NetworkType;
  rpcUrl: string;
  address: string;

  constructor(appName: string, appUrl: string, iconUrl: string) {
    console.log("BeaconWallet constructor", appName, appUrl, iconUrl);
    super(appName, appUrl, iconUrl);
  }

  SetNetwork(networkName: string, rpcUrl: string) {
    this.networkType =
      NetworkType[networkName.toUpperCase() as keyof typeof NetworkType];
    this.rpcUrl = rpcUrl;
  }

  async ConnectAccount() {
    const network: Network = {
      type: this.networkType,
      rpcUrl: this.rpcUrl,
    };

    if (!this.client) {
      this.client = new DAppClient({
        name: this.dappName,
        appUrl: this.dappUrl,
        iconUrl: this.iconUrl,
        network: network,
      });
    }

    try {
      const activeAccount = await this.client.getActiveAccount();
      let publicKey: string;

      if (!activeAccount || activeAccount.scopes.length === 0) {
        const permissions = await this.client.requestPermissions();
        this.activeAddress = permissions.address;
        publicKey = permissions.publicKey;
      } else {
        this.activeAddress = activeAccount.address;
        publicKey = activeAccount.publicKey;
      }

      this.CallUnityOnAccountConnected({
        address: this.activeAddress,
        publicKey: publicKey,
      });
    } catch (error) {
      console.error(`Error during connecting account, ${error.message}`);
      this.CallUnityOnAccountFailedToConnect(error);
    }
  }

  GetActiveAccountAddress() {
    return this.activeAddress;
  }

  async SendContract(
    destination: string,
    amount: string,
    entryPoint: string,
    parameter: string
  ) {
    try {
      const operationResult = await this.client.requestOperation({
        operationDetails: this.GetOperationsList(
          destination,
          amount,
          entryPoint,
          parameter
        ),
      });

      this.CallUnityOnContractCallInjected({
        transactionHash: operationResult.transactionHash,
      });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async OriginateContract(script: string, delegateAddress?: string) {
    try {
      const operationResult = await this.client.requestOperation({
        operationDetails: this.GetOriginationOperationsList(
          script,
          delegateAddress
        ),
      });

      this.CallUnityOnContractCallInjected({
        transactionHash: operationResult.transactionHash,
      });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async SignPayload(signingType: number, plainTextPayload: string) {
    const parsedSigningType = this.NumToSigningType(signingType);

    const result = await this.client.requestSignPayload({
      signingType: parsedSigningType,
      payload: this.GetHexPayloadString(parsedSigningType, plainTextPayload),
    });

    this.CallUnityOnPayloadSigned({ signature: result.signature });
  }

  async DisconnectAccount() {
    const activeAccount = await this.client.getActiveAccount();
    this.activeAddress = "";
    await this.client.removeAllAccounts();

    const accountInfo: AccountInfo = {
      publicKey: activeAccount.publicKey,
      address: activeAccount.address,
    };

    this.CallUnityOnAccountDisconnected(accountInfo);
  }
}

export default BeaconWallet;
