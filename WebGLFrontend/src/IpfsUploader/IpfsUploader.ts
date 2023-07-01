import { FileUploader, FileUploaderConfig, IpfsResponse } from "./Types";

class IpfsUploader implements FileUploader {
  Config: FileUploaderConfig;
  FileUploaderDomElement: HTMLInputElement | null;

  InitFileUploader(config: FileUploaderConfig) {
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

      this.FileUploaderDomElement.onchange = this.OnChange.bind(this);
    }
  }

  RequestUserFile(fileExtensions: string) {
    if (this.FileUploaderDomElement === null)
      this.InitFileUploader(this.Config);

    if (fileExtensions !== null || fileExtensions.match(/^ *$/) === null)
      this.FileUploaderDomElement.setAttribute("accept", fileExtensions);

    this.FileUploaderDomElement.setAttribute("class", "focused");
    this.FileUploaderDomElement.click();
  }

  ResetFileUploader() {
    this.FileUploaderDomElement?.setAttribute("class", "nonfocused");
  }

  async OnChange(event: Event) {
    const { files }: HTMLInputElement = event.target as HTMLInputElement;

    if (files.length === 0) {
      this.ResetFileUploader();
      return;
    }

    const formData = new FormData();
    formData.append("file", files[0], files[0].name);
    const options = {
      method: "POST",
      body: formData,

      headers: {
        Authorization: `Bearer ${this.Config.ApiKey}`,
      },
    };

    const request = await fetch(this.Config.ApiUrl, options);
    const data: IpfsResponse = await request.json();

    window.unityInstance.SendMessage(
      this.Config.CallbackObjectName,
      this.Config.CallbackMethodName,
      data.IpfsHash
    );

    this.ResetFileUploader();
  }
}

export default IpfsUploader;
