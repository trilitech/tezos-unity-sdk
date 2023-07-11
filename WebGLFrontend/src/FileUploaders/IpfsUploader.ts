import { IpfsUploaderConfig, IpfsUploaderType } from "./Types";

import BaseFileUploader from "./BaseFileUploader";

class IpfsUploader extends BaseFileUploader implements IpfsUploaderType {
  Config: IpfsUploaderConfig;

  Init(config: IpfsUploaderConfig) {
    super.Init(config);
    this.Config = config;
  }

  async FileReceived(file: File) {
    const formData = new FormData();
    formData.append("file", file, file.name);
    const options = {
      method: "POST",
      body: formData,

      headers: {
        Authorization: `Bearer ${this.Config.ApiKey}`,
      },
    };

    try {
      const request = await fetch(this.Config.ApiUrl, options);
      const data: string = await request.text();

      window.unityInstance.SendMessage(
        this.Config.CallbackObjectName,
        this.Config.CallbackMethodName,
        data
      );
    } catch (error) {
      console.error(
        `Error during uploading file to ${this.Config.ApiUrl}\n${error}`
      );
    }

    this.ResetFileUploader();
  }
}

export default IpfsUploader;
