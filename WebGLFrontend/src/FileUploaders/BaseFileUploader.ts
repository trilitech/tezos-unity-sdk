import { BaseFileUploaderType, BaseUploaderConfig } from "./Types";

abstract class BaseFileUploader implements BaseFileUploaderType {
  FileUploaderDomElement: HTMLInputElement;
  Config: BaseUploaderConfig;

  Init(config: BaseUploaderConfig) {
    this.Config = config;

    this.FileUploaderDomElement = document.getElementById(
      "fileuploader"
    ) as HTMLInputElement;

    if (!this.FileUploaderDomElement) {
      this.FileUploaderDomElement = document.createElement("input");
      this.FileUploaderDomElement.setAttribute("style", "display:none;");
      this.FileUploaderDomElement.setAttribute("type", "file");
      this.FileUploaderDomElement.setAttribute("id", "fileuploader");
      this.FileUploaderDomElement.setAttribute("class", "nonfocused");
      document
        .getElementsByTagName("body")[0]
        .appendChild(this.FileUploaderDomElement);

      this.FileUploaderDomElement.onchange = (event: Event) => {
        const { files }: HTMLInputElement = event.target as HTMLInputElement;

        if (files.length === 0) {
          this.ResetFileUploader();
        } else {
          this.FileReceived(files[0]);
        }
      };
    }
  }

  abstract FileReceived(file: File): void;

  ResetFileUploader() {
    this.FileUploaderDomElement?.setAttribute("class", "nonfocused");
  }

  RequestUserFile(fileExtensions: string) {
    if (this.FileUploaderDomElement === null) this.Init(this.Config);

    if (fileExtensions !== null || fileExtensions.match(/^ *$/) === null)
      this.FileUploaderDomElement.setAttribute("accept", fileExtensions);

    this.FileUploaderDomElement.setAttribute("class", "focused");
    this.FileUploaderDomElement.click();
  }
}

export default BaseFileUploader;
