mergeInto(LibraryManager.library, {
  JsInitPinataUploader: function (
    callbackObjectName,
    callbackMethodName,
    apiUrl,
    apiKey
  ) {
    InitIpfsUploader({
      CallbackObjectName: UTF8ToString(callbackObjectName),
      CallbackMethodName: UTF8ToString(callbackMethodName),
      ApiUrl: UTF8ToString(apiUrl),
      ApiKey: UTF8ToString(apiKey),
    });
  },

  JsInitBase64Uploader: function (
    callbackObjectName,
    callbackMethodName
  ) {
    InitBase64Uploader({
      CallbackObjectName: UTF8ToString(callbackObjectName),
      CallbackMethodName: UTF8ToString(callbackMethodName)
    });
  },

  JsRequestUserFile: function (extensions) {
    FileUploader.RequestUserFile(UTF8ToString(extensions));
  },
});
