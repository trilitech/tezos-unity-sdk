mergeInto(LibraryManager.library, { 
  JsConnectAccount: function(walletProvider){
    InitWalletProvider(UTF8ToString(walletProvider))
    WalletProvider.ConnectAccount();
  },

  JsGetActiveAccountAddress: function(){
    var returnStr = WalletProvider.GetActiveAccountAddress();
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  JsDisconnectAccount: function (){
    WalletProvider.DisconnectAccount();
  },

  JsSendContractCall: function (destination, amount, entryPoint, parameter){
    WalletProvider.SendContract(UTF8ToString(destination), UTF8ToString(amount), UTF8ToString(entryPoint), UTF8ToString(parameter));
  },

  JsSignPayload: function (signingType, payload){
    WalletProvider.SignPayload(signingType, UTF8ToString(payload));
  }
});