interface IpfsUploaderType extends BaseFileUploaderType {
  Config: IpfsUploaderConfig;
  Init(config: IpfsUploaderConfig): void;
}

interface IpfsUploaderConfig extends BaseUploaderConfig {
  ApiUrl: string;
  ApiKey: string;
}

interface IpfsResponse {
  IpfsHash: string;
  PinSize: number;
  Timestamp: Date;
  isDuplicate: boolean;
}

interface BaseFileUploaderType {
  FileUploaderDomElement: HTMLInputElement;
  Config: BaseUploaderConfig;
  Init(config: BaseUploaderConfig): void;
  RequestUserFile(fileExtensions: string): void;
  ResetFileUploader(): void;
}

interface BaseUploaderConfig {
  CallbackObjectName: string;
  CallbackMethodName: string;
}

export {
  BaseFileUploaderType,
  BaseUploaderConfig,
  IpfsResponse,
  IpfsUploaderConfig,
  IpfsUploaderType,
};
