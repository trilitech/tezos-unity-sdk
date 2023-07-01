mergeInto(LibraryManager.library, {
  JsInitFileLoader: function (
      callbackObjectName,
      callbackMethodName,
      apiUrl,
      apiKey
  ) {
    InitFileUploader({
      CallbackObjectName: UTF8ToString(callbackObjectName),
      CallbackMethodName: UTF8ToString(callbackMethodName),
      ApiUrl: UTF8ToString(apiUrl),
      ApiKey: UTF8ToString(apiKey)
    });
  },

  JsRequestUserFile: function (extensions) {
    FileUploader.RequestUserFile(UTF8ToString(extensions));
  },
});