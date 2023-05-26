import { DAppClient } from "@airgap/beacon-sdk";
import {
  Network,
  NetworkType,
  PermissionResponseOutput,
} from "@airgap/beacon-types";
import BaseWallet from "./BaseWallet";
import { Wallet } from "./Types";

class BeaconWallet extends BaseWallet implements Wallet {
  beaconClient: DAppClient | null;
  activePermissions: PermissionResponseOutput | null;

  constructor() {
    super();

    this.beaconClient = new DAppClient({
      name: "Tezos Unity SDK",
      //todo: make configurable.
      preferredNetwork: NetworkType.GHOSTNET,
    });
  }

  async ConnectAccount() {
    try {
      const network: Network = {
        type: this.beaconClient.preferredNetwork,
        name: this.beaconClient.preferredNetwork,
        rpcUrl: `https://rpc.${this.beaconClient.preferredNetwork}.teztnets.xyz`,
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
