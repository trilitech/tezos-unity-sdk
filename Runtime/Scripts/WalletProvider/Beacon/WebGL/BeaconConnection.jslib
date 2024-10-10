mergeInto(LibraryManager.library, {
  JsInitWallet: function (network, rpc, walletProvider, appName, appUrl, iconUrl) {
    InitWalletProvider(
        UTF8ToString(network),
        UTF8ToString(rpc),
        UTF8ToString(walletProvider),
        UTF8ToString(appName),
        UTF8ToString(appUrl),
        UTF8ToString(iconUrl)
    );
  },

  JsConnectAccount: function () {
    WalletProvider.ConnectAccount();
  },

  JsGetActiveAccountAddress: function () {
    var returnStr = WalletProvider.GetActiveAccountAddress();
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  JsDisconnectAccount: function () {
    WalletProvider.DisconnectAccount();
  },

  JsSendContractCall: function (destination, amount, entryPoint, parameter) {
    WalletProvider.SendContract(
        UTF8ToString(destination),
        UTF8ToString(amount),
        UTF8ToString(entryPoint),
        UTF8ToString(parameter)
    );
  },

  JsSignPayload: function (signingType, payload) {
    WalletProvider.SignPayload(signingType, UTF8ToString(payload));
  },

  JsRequestContractOrigination: function (script, delegateAddress) {
    WalletProvider.OriginateContract(
        UTF8ToString(script),
        UTF8ToString(delegateAddress)
    );
  },
  
  JsUnityReadyEvent: function () {
    UnityReadyEvent();
  }
});