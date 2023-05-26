import {
  PartialTezosTransactionOperation,
  SigningType,
  TezosOperationType,
} from "@airgap/beacon-types";
import { char2Bytes } from "@taquito/utils";
import {
  AbstractWallet,
  AccountInfo,
  OperationResult,
  SignResult,
} from "./Types";

class BaseWallet implements AbstractWallet {
  CallUnityOnAccountFailedToConnect(error: Error) {
    this.CallUnityMethod("OnAccountFailedToConnect", error);
  }

  CallUnityOnContractCallCompleted(result: OperationResult) {
    this.CallUnityMethod("OnContractCallCompleted", result);
  }

  CallUnityOnContractCallFailed(error: Error) {
    this.CallUnityMethod("OnContractCallFailed", error);
  }

  CallUnityOnPayloadSigned(result: SignResult) {
    this.CallUnityMethod("OnPayloadSigned", result);
  }

  CallUnityOnAccountDisconnected(address: string) {
    this.CallUnityMethod("OnAccountDisconnected", address);
  }

  CallUnityOnAccountConnected(accountInfo: AccountInfo) {
    this.CallUnityMethod("OnAccountConnected", { accountInfo });
  }

  private CallUnityMethod(methodName: string, value: any) {
    window.unityInstance.SendMessage(
      "UnityBeacon",
      methodName,
      typeof value === "string" ? value : JSON.stringify(value)
    );
  }

  GetHexPayloadString(
    signingType: SigningType,
    plainTextPayload: string
  ): string {
    const bytes = char2Bytes(plainTextPayload);
    const bytesLength = (bytes.length / 2).toString(16);
    const addPadding = `00000000${bytesLength}`;
    const paddedBytesLength = addPadding.slice(addPadding.length - 8);
    const prefix = signingType === SigningType.MICHELINE ? "0501" : "0300";
    const payloadBytes = prefix + paddedBytesLength + bytes;
    return payloadBytes;
  }

  NumToSigningType(numSigningType: number): SigningType {
    if (numSigningType == 0) return SigningType.RAW;
    if (numSigningType == 1) return SigningType.OPERATION;
    if (numSigningType == 2) return SigningType.MICHELINE;
  }

  GetOperationsList(
    destination: string,
    amount: string,
    entryPoint: string,
    parameter: string
  ): PartialTezosTransactionOperation[] {
    const operations: PartialTezosTransactionOperation[] = [
      {
        kind: TezosOperationType.TRANSACTION,
        amount,
        destination,
        parameters: {
          entrypoint: entryPoint,
          value: JSON.parse(parameter),
        },
      },
    ];
    return operations;
  }
}

export default BaseWallet;
