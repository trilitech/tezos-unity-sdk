import { DAppClient } from "@airgap/beacon-sdk";
import { KukaiEmbed } from "kukai-embed";
import { AccountInfo } from '../../node_modules/@airgap/beacon-types/dist/esm/types/AccountInfo';

enum WalletType {
  beacon = "beacon",
  kukai = "kukai",
}

enum EventType {
  accountConnected = "AccountConnected",
  accountDisconnected = "AccountDisconnected",
  accountConnectionFailed = "AccountConnectionFailed",
  contractCallInjected = "ContractCallInjected",
  contractCallFailed = "ContractCallFailed",
  payloadSigned = "PayloadSigned",
  payloadSigFailed = "PayloadSignFailed",
  sdkInitialized = "SDKInitialized",
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
  CallUnityOnAccountConnected(accountInfo: AccountInformation): void;
  CallUnityOnAccountDisconnected(accountInfo: AccountInformation): void;
  CallUnityOnPayloadSigned(result: SignResult): void;
  CallUnityOnContractCallInjected(result: OperationResult): void;
  CallUnityOnAccountFailedToConnect(error: Error): void;
  CallUnityOnContractCallFailed(error: Error): void;
  CallUnityOnPayloadSignFailed(error: Error): void;
}

interface AccountInformation {
  publicKey: string;
  walletAddress: string;
  accountInfo: AccountInfo
}

interface SignResult {
  signature: string;
}

interface OperationResult {
  transactionHash: string;
}

interface ErrorInfo {
  message: string;
}

interface SDKIntializedEvent {}

interface UnityEvent {
  eventType: EventType;
  data:
    | AccountInformation
    | SignResult
    | ErrorInfo
    | OperationResult
    | SDKIntializedEvent;
}

export {
  Wallet,
  WalletType,
  AbstractWallet,
  AccountInformation,
  SignResult,
  OperationResult,
  UnityEvent,
  EventType,
  ErrorInfo,
};
