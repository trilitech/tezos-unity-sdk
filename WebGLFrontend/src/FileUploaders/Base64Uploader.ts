import BaseFileUploader from "./BaseFileUploader";

class Base64Uploader extends BaseFileUploader {
  async FileReceived(file: File) {
    window.unityInstance.SendMessage(
      this.Config.CallbackObjectName,
      this.Config.CallbackMethodName,
      await this.СonvertImageToBase64(file)
    );
  }

  СonvertImageToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onloadend = () => {
        if (typeof reader.result === "string") {
          resolve(reader.result);
        } else {
          reject(new Error("Failed to convert image to base64."));
        }
      };

      reader.onerror = (error) => {
        reject(error);
      };

      reader.readAsDataURL(file);
    });
  }
}

export default Base64Uploader;
