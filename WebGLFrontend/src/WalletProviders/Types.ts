import {DAppClient} from "@airgap/beacon-sdk";
import {KukaiEmbed} from "kukai-embed";

enum WalletType {
  beacon = "beacon",
  kukai = "kukai",
}

// this methods called from Unity.
interface Wallet {
  client: DAppClient | KukaiEmbed | null;
  ConnectAccount(): void;
  SetNetwork(networkName: string, rpcUrl: string): void;
  GetActiveAccountAddress(): string;
  SendContract(
    destination: string,
    amount: string,
    entryPoint: string,
    parameter: string
  ): void;
  SignPayload(signingType: number, plainTextPayload: string): void;
  DisconnectAccount(): void;
  OriginateContract(script: string, delegateAddress?: string): void;
}

interface AbstractWallet {
  CallUnityOnAccountConnected(accountInfo: AccountInfo): void;
  CallUnityOnAccountDisconnected(address: string): void;
  CallUnityOnPayloadSigned(result: SignResult): void;
  CallUnityOnContractCallCompleted(result: OperationResult): void;
  CallUnityOnAccountFailedToConnect(error: Error): void;
  CallUnityOnContractCallFailed(error: Error): void;
}

interface AccountInfo {
  publicKey: string;
  address: string;
}

interface SignResult {
  signature: string;
}

interface OperationResult {
  transactionHash: string;
}

export {
  Wallet,
  WalletType,
  AbstractWallet,
  AccountInfo,
  SignResult,
  OperationResult,
};
