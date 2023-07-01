interface FileUploader {
  Config: FileUploaderConfig;
  FileUploaderDomElement: HTMLInputElement;
  InitFileUploader(config: FileUploaderConfig): void;
  RequestUserFile(fileExtensions: string): void;
  ResetFileUploader(): void;
}

interface FileUploaderConfig {
  CallbackObjectName: string;
  CallbackMethodName: string;
  ApiUrl: string;
  ApiKey: string;
}

interface IpfsResponse {
  IpfsHash: string;
  PinSize: number;
  Timestamp: Date;
  isDuplicate: boolean;
}

export { FileUploader, FileUploaderConfig, IpfsResponse };
