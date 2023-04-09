mergeInto(LibraryManager.library, {
  JsSetNetwork: function(network, rpc){
    SetNetwork(UTF8ToString(network), UTF8ToString(rpc));
  },
 
  JsConnectAccount: function(){
    ConnectAccount();
  },

  JsGetActiveAccountAddress: function(){
    var returnStr = GetActiveAccountAddress();
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  JsSwitchAccounts: function (){
    SwitchAccounts();
  },

  JsRemovePeer: function (){
    RemovePeer();
  },
  
  JsSendMutezAsString: function (amount, address){
    SendMutez(UTF8ToString(amount), UTF8ToString(address));
  },

  JsSendContractCall: function (destination, amount, entryPoint, parameter){
    SendContract(UTF8ToString(destination), UTF8ToString(amount), UTF8ToString(entryPoint), UTF8ToString(parameter));
  },

  JsReset: function (){
    Reset();
  },

  JsSignPayload: function (signingType, payload){
    SignPayload(signingType, UTF8ToString(payload));
  }
});