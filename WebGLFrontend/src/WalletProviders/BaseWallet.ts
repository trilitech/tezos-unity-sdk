import { char2Bytes } from "@taquito/utils";
import {
  AbstractWallet,
  AccountInfo,
  ErrorInfo,
  EventType,
  OperationResult,
  SignResult,
  UnityEvent,
} from "./Types";
import {
  PartialTezosOriginationOperation,
  PartialTezosTransactionOperation,
  SigningType,
  TezosOperationType,
} from "@airgap/beacon-types";

class BaseWallet implements AbstractWallet {
  dappName: string;
  dappUrl: string;
  iconUrl: string;

  constructor(appName: string, appUrl: string, iconUrl: string) {
    console.log("BaseWallet constructor", appName, appUrl, iconUrl);
    this.dappName = appName;
    this.dappUrl = appUrl;
    this.iconUrl = iconUrl;
  }

  CallUnityOnSDKInitialized() {
    const eventData: UnityEvent = {
      eventType: EventType.sdkInitialized,
      data: {}, // No additional data is needed for this event.
    };
    this.CallUnityMethod(eventData);
  }
  
  CallUnityOnAccountFailedToConnect(error: ErrorInfo) {
    const eventData: UnityEvent = {
      eventType: EventType.accountConnectionFailed,
      data: error,
    };
    this.CallUnityMethod(eventData);
  }

  CallUnityOnContractCallInjected(result: OperationResult) {
    const eventData: UnityEvent = {
      eventType: EventType.contractCallInjected,
      data: result,
    };
    this.CallUnityMethod(eventData);
  }

  CallUnityOnContractCallFailed(error: ErrorInfo) {
    const eventData: UnityEvent = {
      eventType: EventType.contractCallFailed,
      data: error,
    };
    this.CallUnityMethod(eventData);
  }

  CallUnityOnPayloadSigned(result: SignResult) {
    const eventData: UnityEvent = {
      eventType: EventType.payloadSigned,
      data: result,
    };
    this.CallUnityMethod(eventData);
  }

  CallUnityOnAccountDisconnected(accountInfo: AccountInfo) {
    const eventData: UnityEvent = {
      eventType: EventType.accountDisconnected,
      data: accountInfo,
    };
    this.CallUnityMethod(eventData);
    localStorage.removeItem("dappName");
    localStorage.removeItem("dappUrl");
    localStorage.removeItem("iconUrl");
  }

  CallUnityOnAccountConnected(accountInfo: AccountInfo) {
    const eventData: UnityEvent = {
      eventType: EventType.accountConnected,
      data: accountInfo,
    };

    this.CallUnityMethod(eventData);
    localStorage.setItem("dappName", this.dappName);
    localStorage.setItem("dappUrl", this.dappUrl);
    localStorage.setItem("iconUrl", this.iconUrl);
  }

  private CallUnityMethod(eventData: UnityEvent) {
    const resultEventData = {
      EventType: eventData.eventType,
      Data: JSON.stringify(this.CapitalizeKeys(eventData.data)),
    };

    window.unityInstance.SendMessage(
      "WalletEventManager",
      "HandleEvent",
      JSON.stringify(resultEventData)
    );
  }

  private CapitalizeKeys(obj: any): any {
    if (Array.isArray(obj)) {
      return obj.map((o) => this.CapitalizeKeys(o));
    } else if (typeof obj === "object" && obj !== null) {
      return Object.entries(obj).reduce(
        (r, [k, v]) => ({
          ...r,
          [`${k.charAt(0).toUpperCase()}${k.slice(1)}`]: this.CapitalizeKeys(v),
        }),
        {}
      );
    } else {
      return obj;
    }
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
