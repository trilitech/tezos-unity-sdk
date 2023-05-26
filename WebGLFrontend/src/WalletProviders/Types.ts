enum WalletType {
  beacon = "beacon",
  kukai = "kukai",
}

// this methods called from Unity.
interface Wallet {
  ConnectAccount(): void;
  GetActiveAccountAddress(): string;
  SendContract(
    destination: string,
    amount: string,
    entryPoint: string,
    parameter: string
  ): void;
  SignPayload(signingType: number, plainTextPayload: string): void;
  DisconnectAccount(): void;
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
