import BaseWallet from "./BaseWallet";
import { DAppClient } from "@airgap/beacon-sdk";
import { Wallet } from "./Types";
import {
  Network,
  NetworkType,
  PartialTezosOriginationOperation,
  PermissionResponseOutput,
  TezosOperationType,
} from "@airgap/beacon-types";

class BeaconWallet extends BaseWallet implements Wallet {
  clientName: string = "Tezos Unity SDK";
  beaconClient: DAppClient | null;
  activePermissions: PermissionResponseOutput | null;
  networkType: NetworkType;
  rpcUrl: string;

  SetNetwork(networkName: string, rpcUrl: string) {
    this.networkType =
      NetworkType[networkName.toUpperCase() as keyof typeof NetworkType];
    this.rpcUrl = rpcUrl;
  }

  async ConnectAccount() {
    if (!this.beaconClient) {
      this.beaconClient = new DAppClient({
        name: this.clientName,
        preferredNetwork: this.networkType,
      });
    }

    try {
      const network: Network = {
        type: this.beaconClient.preferredNetwork,
        name: this.beaconClient.preferredNetwork,
        rpcUrl: this.rpcUrl,
      };

      this.activePermissions = await this.beaconClient.requestPermissions({
        network,
      });

      this.CallUnityOnAccountConnected({
        address: this.activePermissions.accountInfo.address,
        publicKey: this.activePermissions.accountInfo.publicKey,
      });
    } catch (error) {
      console.error(`Error during connecting account, ${error.message}`);
      this.CallUnityOnAccountFailedToConnect(error);
    }
  }

  GetActiveAccountAddress() {
    return this.activePermissions.accountInfo.address;
  }

  async SendContract(
    destination: string,
    amount: string,
    entryPoint: string,
    parameter: string
  ) {
    try {
      const operationResult = await this.beaconClient.requestOperation({
        operationDetails: this.GetOperationsList(
          destination,
          amount,
          entryPoint,
          parameter
        ),
      });

      this.CallUnityOnContractCallCompleted({
        transactionHash: operationResult.transactionHash,
      });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async OriginateContract(script: string, delegateAddress?: string) {
    try {
      const operationResult = await this.beaconClient.requestOperation({
        operationDetails: this.GetOriginationOperationsList(
          script,
          delegateAddress
        ),
      });

      this.CallUnityOnContractCallCompleted({
        transactionHash: operationResult.transactionHash,
      });
    } catch (error) {
      this.CallUnityOnContractCallFailed(error);
    }
  }

  async SignPayload(signingType: number, plainTextPayload: string) {
    const parsedSigningType = this.NumToSigningType(signingType);

    const result = await this.beaconClient.requestSignPayload({
      signingType: parsedSigningType,
      payload: this.GetHexPayloadString(parsedSigningType, plainTextPayload),
    });

    this.CallUnityOnPayloadSigned({ signature: result.signature });
  }

  async DisconnectAccount() {
    await this.beaconClient.removeAllPeers();
    this.CallUnityOnAccountDisconnected(this.activePermissions.address);
    this.activePermissions = null;
  }
}

export default BeaconWallet;
