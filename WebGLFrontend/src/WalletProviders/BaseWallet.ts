import { char2Bytes } from "@taquito/utils";
import {
  PartialTezosOriginationOperation,
  PartialTezosTransactionOperation,
  SigningType,
  TezosOperationType,
} from "@airgap/beacon-types";
import {
  AbstractWallet,
  AccountInfo,
  OperationResult,
  SignResult,
} from "./Types";

class BaseWallet implements AbstractWallet {
  dappName: string;
  dappUrl: string;
  iconUrl: string;

  constructor(appName: string, appUrl: string, iconUrl: string) {
    this.dappName = appName;
    this.dappUrl = appUrl;
    this.iconUrl = iconUrl;
  }
  
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
    localStorage.removeItem("dappName");
    localStorage.removeItem("dappUrl");
    localStorage.removeItem("iconUrl");
  }

  CallUnityOnAccountConnected(accountInfo: AccountInfo) {
    this.CallUnityMethod("OnAccountConnected", { accountInfo });
    localStorage.setItem("dappName", this.dappName);
    localStorage.setItem("dappUrl", this.dappUrl);
    localStorage.setItem("iconUrl", this.iconUrl);
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

  GetOriginationOperationsList(script: string, delegateAddress?: string) {
    const operations: PartialTezosOriginationOperation[] = [
      {
        kind: TezosOperationType.ORIGINATION,
        balance: "0",
        delegate: delegateAddress,
        script: JSON.parse(script),
      },
    ];
    return operations;
  }
}

export default BaseWallet;
