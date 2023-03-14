package it.airgap.beaconsdk.dapp

import androidx.lifecycle.viewModelScope
import android.util.Log
import it.airgap.beaconsdk.blockchain.substrate.substrate
import it.airgap.beaconsdk.blockchain.tezos.data.TezosNetwork
import it.airgap.beaconsdk.blockchain.tezos.data.TezosPermission
import it.airgap.beaconsdk.blockchain.tezos.data.operation.MichelineMichelsonV1Expression
import it.airgap.beaconsdk.blockchain.tezos.data.operation.TezosOperation
import it.airgap.beaconsdk.blockchain.tezos.data.operation.TezosTransactionOperation
import it.airgap.beaconsdk.blockchain.tezos.extension.requestTezosBroadcast
import it.airgap.beaconsdk.blockchain.tezos.extension.requestTezosOperation
import it.airgap.beaconsdk.blockchain.tezos.extension.requestTezosPermission
import it.airgap.beaconsdk.blockchain.tezos.extension.requestTezosSignPayload
import it.airgap.beaconsdk.blockchain.tezos.message.response.BroadcastTezosResponse
import it.airgap.beaconsdk.blockchain.tezos.message.response.OperationTezosResponse
import it.airgap.beaconsdk.blockchain.tezos.message.response.SignPayloadTezosResponse
import it.airgap.beaconsdk.blockchain.tezos.tezos
import it.airgap.beaconsdk.client.dapp.BeaconDAppClient
import it.airgap.beaconsdk.core.data.Connection
import it.airgap.beaconsdk.core.data.SigningType
import it.airgap.beaconsdk.core.message.*
import it.airgap.beaconsdk.transport.p2p.matrix.p2pMatrix
import kotlinx.coroutines.*
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.onEach
import kotlinx.coroutines.flow.onStart
import kotlinx.coroutines.flow.onCompletion
import kotlinx.coroutines.launch
import kotlinx.serialization.decodeFromString
import kotlinx.serialization.json.Json
import com.unity3d.player.UnityPlayer;
import it.airgap.beaconsdk.utils.toJson

class DAppViewModel
{
    private var beaconClient: BeaconDAppClient? = null
    private var awaitingResponse: BeaconResponse? = null

    //Starts up a Beacon client
    fun startBeacon(){
        CoroutineScope(Dispatchers.Default).launch {
            val client = BeaconDAppClient("Beacon SDK Demo (DApp)", clientId = "__dapp__") {
                support(tezos(), substrate())
                use(p2pMatrix())

                ignoreUnsupportedBlockchains = true
            }.also { beaconClient = it }
            client.connect()
                .onStart {
                    checkForActiveAccount()
                    UnityPlayer.UnitySendMessage("UnityBeacon", "OnClientCreated", "client Created")
                }
                .onCompletion { UnityPlayer.UnitySendMessage("UnityBeacon", "OnPairingCompleted", "Pairing Completed") }
                .collect { result ->
                    result.getOrNull()?.let { onBeaconResponse( it) }
                    result.exceptionOrNull()?.let { onError( it) }
                }
        }
    }

    fun stopBeacon(){
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.removeAllPeers()
            beaconClient?.removeAllPermissions()
            startBeacon()
            UnityPlayer.UnitySendMessage("UnityBeacon", "OnAccountDisconnected", "Disconnected from wallets")
        }
    }

    //Makes a handshake request for QR and DeepLink. Spits out serializerPairingRequest to receiver
    fun requestHandshake(){
        Log.w("requestHandshake", "IN REQUEST HANDSHAKE: beaconClient = $beaconClient")
        val beaconClient = beaconClient ?: return
        CoroutineScope(Dispatchers.Default).launch {
            val pairingRequest = beaconClient.pair()
            val serializerPairingRequest = beaconClient.serializePairingData(pairingRequest)

            requestTezosPermission()
            UnityPlayer.UnitySendMessage("UnityBeacon", "OnHandshakeReceived", serializerPairingRequest)
        }
    }

    //Allows waiting for permissions to be granted for pairing. Use this one if you want to use default settings.
    fun requestTezosPermission(){
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosPermission()
        }
    }

    //Allows waiting for permissions to be granted for pairing. Use this one if you want to define a network.
    fun requestTezosPermission( networkName: String = "", networkRPC: String){
        val network: TezosNetwork = TezosNetwork.Custom(networkName, networkRPC)
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosPermission(network)
        }
    }

    //Allows waiting for permissions to be granted for pairing. Use this one if you want to define a network, scope, and connection type.
    fun requestTezosPermission(networkName: String = "", networkRPC: String, scopes: List<TezosPermission.Scope>, transportType: Connection.Type = Connection.Type.P2P){
        val network: TezosNetwork = TezosNetwork.Custom(networkName, networkRPC)
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosPermission(network, scopes, transportType)
        }
    }

    //Allows requesting a new operation. This version will use full default values for the parameters. (So should only be used for testing)
    fun requestTezosOperation(){
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosOperation()
        }
        "--=--Tezos Operation Requested--=--"
    }

    //Allows requesting a new operation. This version will only allow for a single operation but all portions of the operation will be fillable through C#.
    fun requestTezosOperation(destination: String = "tz1RmqagvEGJxVwPwf94m2vcGmhJRh7uiexS", amount: String = "1", entrypoint: String = "default", arg: String? = null, networkName: String = "", networkRPC: String = ""){
        val network: TezosNetwork = TezosNetwork.Custom(networkName, networkRPC)

        CoroutineScope(Dispatchers.Default).launch {
            val parameters = arg?.let {
                val expression = Json.decodeFromString<MichelineMichelsonV1Expression>(it)
                TezosTransactionOperation.Parameters(entrypoint, expression)
            }

            beaconClient?.requestTezosOperation(
                listOf(
                    TezosTransactionOperation(amount = amount, destination = destination, parameters = parameters),
                ), network
            )
        }
    }
    //Allows requesting a new operation. This version will allow for multiple TezosOperations as well as network and connection types.
    fun requestTezosOperation(operationDetails: List<TezosOperation> = emptyList(), transportType: Connection.Type = Connection.Type.P2P, networkName: String = "", networkRPC: String = ""){
        val network: TezosNetwork = TezosNetwork.Custom(networkName, networkRPC)

        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosOperation(operationDetails, network, transportType)
        }
    }

    //Allows for changing an int to a SigningType for use in C#.
    fun getSingingType(index: Int): SigningType {
        return when (index){
            0 -> SigningType.Raw
            1 -> SigningType.Operation
            2 -> SigningType.Micheline
            else -> SigningType.Micheline
        }
    }

    //Allows Signing of a Payload. This version will assume it's in P2P.
    fun requestTezosSignPayload(signingType: Int, payload: String){
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosSignPayload(
                getSingingType(signingType),
                payload,
                Connection.Type.P2P
            )
        }
    }

    //Allows Signing of a Payload. This version will allow the Connection Type to be selected.
    fun requestTezosSignPayload(signingType: Int, payload: String, transportType: Connection.Type = Connection.Type.P2P){
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosSignPayload(
                getSingingType(signingType),
                payload,
                transportType
            )
        }
    }

    //Allows Signing of a Payload. This version will allow for full default parameters in case a method of calling it in c# is found.
    fun requestTezosSignPayload(signingType: SigningType, payload: String, transportType: Connection.Type = Connection.Type.P2P){
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosSignPayload(
                signingType,
                payload,
                transportType)
        }
    }

    //Allows requesting a new broadcast. This version will use full default values for the parameters. (So should only be used for testing)
    fun requestTezosBroadcast(signedTransaction: String){
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosBroadcast(signedTransaction)
        }
    }

    //Allows requesting a new broadcast. This version will take in custom values for the network so it's callable from C#
    fun requestTezosBroadcast(  signedTransaction: String, networkName: String = "", networkRPC: String = "", transportType: Connection.Type = Connection.Type.P2P){
        val network: TezosNetwork = TezosNetwork.Custom(networkName, networkRPC)
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosBroadcast(signedTransaction, network, transportType)
        }
    }

    //Allows requesting a new broadcast. This version will work just as it does in BeaconSDK
    fun requestTezosBroadcast(  signedTransaction: String, transportType: Connection.Type = Connection.Type.P2P, networkName: String = "", networkRPC: String = ""){
        val network: TezosNetwork = TezosNetwork.Custom(networkName, networkRPC)
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.requestTezosBroadcast(signedTransaction, network, transportType)
        }
    }

    //Stops the wait for a response
    fun clearResponse() {
        awaitingResponse = null
        checkForAwaitingResponses()
    }

    //Resets the beacon client.
    fun reset() {
        CoroutineScope(Dispatchers.Default).launch {
            beaconClient?.reset()
            checkForActiveAccount()
        }
    }

    //Checks if there is an active account paired. Returns the full account info.
    private fun checkForActiveAccount():String = runBlocking{
        val activeAccount = beaconClient?.getActiveAccount().toString()

        activeAccount
    }

    //Checks if there is an active account paired. Returns only the account address.
    fun getActiveAccountAddress():String = runBlocking{
        val activeAccountAddress = beaconClient?.getActiveAccount()?.address.toString()

        activeAccountAddress
    }

    //Not implimented
    private fun checkForAwaitingResponses() {
    }

    //Not sure what it does but it was in the Demo.
    private fun saveAwaitingResponse(message: BeaconMessage) {
        awaitingResponse = if (message is BeaconResponse) message else null
        checkForAwaitingResponses()
    }

    //Does something when a BeaconResponse is triggered.
    private suspend fun onBeaconResponse(response: BeaconResponse) {
        when(response)
        {
            is OperationTezosResponse -> UnityPlayer.UnitySendMessage("UnityBeacon", "OnContractCallCompleted", response.toJson().toString())
            is SignPayloadTezosResponse -> UnityPlayer.UnitySendMessage("UnityBeacon", "OnPayloadSigned", response.signature.toString())
            is BroadcastTezosResponse -> UnityPlayer.UnitySendMessage("UnityBeacon", "OnBroadcastResponse", response.toString())
            is PermissionBeaconResponse -> UnityPlayer.UnitySendMessage("UnityBeacon", "OnAccountConnected", response.toJson().toString())
            //is BlockchainBeaconResponse -> UnityPlayer.UnitySendMessage("UnityBeacon", "OnHandshakeReceived", response.destination.id)
            is AcknowledgeBeaconResponse -> UnityPlayer.UnitySendMessage("UnityBeacon", "OnAcknowledgeResponse", response.toString())
            is ErrorBeaconResponse -> UnityPlayer.UnitySendMessage("UnityBeacon", "OnErrorResponse", response.toJson().toString())
        }

        awaitingResponse = response
        checkForActiveAccount()
    }

    //Does something when there is an error.
    private fun onError(error: Throwable) {
        UnityPlayer.UnitySendMessage("UnityBeacon", "OnErrorResponse", error.toString())
    }
}