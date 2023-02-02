package it.airgap.beaconsdk.utils

import it.airgap.beaconsdk.blockchain.substrate.message.request.BlockchainSubstrateRequest
import it.airgap.beaconsdk.blockchain.substrate.message.request.PermissionSubstrateRequest
import it.airgap.beaconsdk.blockchain.substrate.message.request.SignPayloadSubstrateRequest
import it.airgap.beaconsdk.blockchain.substrate.message.request.TransferSubstrateRequest
import it.airgap.beaconsdk.blockchain.substrate.message.response.*
import it.airgap.beaconsdk.blockchain.tezos.message.request.*
import it.airgap.beaconsdk.blockchain.tezos.message.response.*
import it.airgap.beaconsdk.core.data.BeaconError
import it.airgap.beaconsdk.core.data.Connection
import it.airgap.beaconsdk.core.internal.utils.failWithIllegalArgument
import it.airgap.beaconsdk.core.message.*
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonElement
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.encodeToJsonElement

fun BeaconMessage.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is BeaconRequest -> toJson(json)
        is BeaconResponse -> toJson(json)
        is DisconnectBeaconMessage -> toJson(json)
    }

fun BeaconRequest.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is PermissionBeaconRequest -> toJson(json)
        is BlockchainBeaconRequest -> toJson(json)
    }

fun PermissionBeaconRequest.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is PermissionTezosRequest -> toJson(json)
        is PermissionSubstrateRequest -> toJson(json)
        else -> failWithUnknownPermissionBeaconRequest(this)
    }

fun BlockchainBeaconRequest.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is BlockchainTezosRequest -> toJson(json)
        is BlockchainSubstrateRequest -> toJson(json)
        else -> failWithUnknownBlockchainBeaconRequest(this)
    }

fun BeaconResponse.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is PermissionBeaconResponse -> toJson(json)
        is BlockchainBeaconResponse -> toJson(json)
        is AcknowledgeBeaconResponse -> toJson(json)
        is ErrorBeaconResponse -> toJson(json)
    }

fun PermissionBeaconResponse.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is PermissionTezosResponse -> toJson(json)
        is PermissionSubstrateResponse -> toJson(json)
        else -> failWithUnknownPermissionBeaconResponse(this)
    }

fun BlockchainBeaconResponse.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is BlockchainTezosResponse -> toJson(json)
        is BlockchainSubstrateResponse -> toJson(json)
        else -> failWithUnknownBlockchainBeaconResponse(this)
    }

fun AcknowledgeBeaconResponse.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("acknowledge_response"),
            "id" to json.encodeToJsonElement(id),
            "senderId" to json.encodeToJsonElement(senderId),
            "version" to json.encodeToJsonElement(version),
        )
    )

fun ErrorBeaconResponse.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("error_response"),
            "id" to json.encodeToJsonElement(id),
            "errorType" to json.encodeToJsonElement(BeaconError.serializer(errorType.blockchainIdentifier), errorType),
            "version" to json.encodeToJsonElement(version),
        )
    )

fun DisconnectBeaconMessage.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "id" to json.encodeToJsonElement(id),
            "senderId" to json.encodeToJsonElement(senderId),
            "version" to json.encodeToJsonElement(version),
            "origin" to origin.toJson(json),
        )
    )

// -- Tezos --

fun PermissionTezosRequest.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("tezos_permission_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "network" to json.encodeToJsonElement(network),
            "scopes" to json.encodeToJsonElement(scopes),
        )
    )

fun BlockchainTezosRequest.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is OperationTezosRequest -> toJson(json)
        is SignPayloadTezosRequest -> toJson(json)
        is BroadcastTezosRequest -> toJson(json)
    }

fun OperationTezosRequest.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("tezos_operation_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "network" to json.encodeToJsonElement(network),
            "operationDetails" to json.encodeToJsonElement(operationDetails),
            "sourceAddress" to json.encodeToJsonElement(sourceAddress),
        )
    )

fun SignPayloadTezosRequest.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("tezos_sign_payload_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "signingType" to json.encodeToJsonElement(signingType),
            "payload" to json.encodeToJsonElement(payload),
            "sourceAddress" to json.encodeToJsonElement(sourceAddress),
        )
    )

fun BroadcastTezosRequest.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("tezos_broadcast_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "network" to json.encodeToJsonElement(network),
            "signedTransaction" to json.encodeToJsonElement(signedTransaction),
        )
    )

fun PermissionTezosResponse.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("tezos_permission_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "account" to json.encodeToJsonElement(account),
            "scopes" to json.encodeToJsonElement(scopes),
        )
    )

fun BlockchainTezosResponse.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is OperationTezosResponse -> toJson(json)
        is SignPayloadTezosResponse -> toJson(json)
        is BroadcastTezosResponse -> toJson(json)
    }

fun OperationTezosResponse.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("tezos_operation_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "transactionHash" to json.encodeToJsonElement(transactionHash),
        )
    )

fun SignPayloadTezosResponse.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("tezos_sign_payload_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "signingType" to json.encodeToJsonElement(signingType),
            "signature" to json.encodeToJsonElement(signature),
        )
    )

fun BroadcastTezosResponse.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("tezos_broadcast_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "transactionHash" to json.encodeToJsonElement(transactionHash),
        )
    )

// -- Substrate --

fun PermissionSubstrateRequest.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_permission_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "origin" to origin.toJson(json),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "scopes" to json.encodeToJsonElement(scopes),
            "networks" to json.encodeToJsonElement(networks),
        )
    )

fun BlockchainSubstrateRequest.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is TransferSubstrateRequest -> toJson(json)
        is SignPayloadSubstrateRequest -> toJson(json)
    }

fun TransferSubstrateRequest.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is TransferSubstrateRequest.Submit -> toJson(json)
        is TransferSubstrateRequest.SubmitAndReturn -> toJson(json)
        is TransferSubstrateRequest.Return -> toJson(json)
    }

fun SignPayloadSubstrateRequest.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is SignPayloadSubstrateRequest.Submit -> toJson(json)
        is SignPayloadSubstrateRequest.SubmitAndReturn -> toJson(json)
        is SignPayloadSubstrateRequest.Return -> toJson(json)
    }

fun TransferSubstrateRequest.Submit.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_transfer_broadcast_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "accountId" to json.encodeToJsonElement(accountId),
            "scope" to json.encodeToJsonElement(scope),
            "sourceAddress" to json.encodeToJsonElement(sourceAddress),
            "amount" to json.encodeToJsonElement(amount),
            "recipient" to json.encodeToJsonElement(recipient),
            "network" to json.encodeToJsonElement(network),
        )
    )

fun TransferSubstrateRequest.SubmitAndReturn.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_transfer_broadcast_and_return_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "accountId" to json.encodeToJsonElement(accountId),
            "scope" to json.encodeToJsonElement(scope),
            "sourceAddress" to json.encodeToJsonElement(sourceAddress),
            "amount" to json.encodeToJsonElement(amount),
            "recipient" to json.encodeToJsonElement(recipient),
            "network" to json.encodeToJsonElement(network),
        )
    )

fun TransferSubstrateRequest.Return.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_transfer_return_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "accountId" to json.encodeToJsonElement(accountId),
            "scope" to json.encodeToJsonElement(scope),
            "sourceAddress" to json.encodeToJsonElement(sourceAddress),
            "amount" to json.encodeToJsonElement(amount),
            "recipient" to json.encodeToJsonElement(recipient),
            "network" to json.encodeToJsonElement(network),
        )
    )

fun SignPayloadSubstrateRequest.Submit.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_sign_payload_broadcast_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "accountId" to json.encodeToJsonElement(accountId),
            "scope" to json.encodeToJsonElement(scope),
            "address" to json.encodeToJsonElement(address),
            "payload" to json.encodeToJsonElement(payload),
        )
    )

fun SignPayloadSubstrateRequest.SubmitAndReturn.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_sign_payload_broadcast_and_return_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "accountId" to json.encodeToJsonElement(accountId),
            "scope" to json.encodeToJsonElement(scope),
            "address" to json.encodeToJsonElement(address),
            "payload" to json.encodeToJsonElement(payload),
        )
    )

fun SignPayloadSubstrateRequest.Return.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_sign_payload_return_request"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "senderId" to json.encodeToJsonElement(senderId),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "origin" to origin.toJson(json),
            "accountId" to json.encodeToJsonElement(accountId),
            "scope" to json.encodeToJsonElement(scope),
            "address" to json.encodeToJsonElement(address),
            "payload" to json.encodeToJsonElement(payload),
        )
    )

fun PermissionSubstrateResponse.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_permission_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "destination" to destination.toJson(json),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "appMetadata" to json.encodeToJsonElement(appMetadata),
            "scopes" to json.encodeToJsonElement(scopes),
            "accounts" to json.encodeToJsonElement(accounts),
        )
    )

fun BlockchainSubstrateResponse.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is TransferSubstrateResponse -> toJson(json)
        is SignPayloadSubstrateResponse -> toJson(json)
    }

fun TransferSubstrateResponse.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is TransferSubstrateResponse.Submit -> toJson(json)
        is TransferSubstrateResponse.SubmitAndReturn -> toJson(json)
        is TransferSubstrateResponse.Return -> toJson(json)
    }

fun SignPayloadSubstrateResponse.toJson(json: Json = Json.Default): JsonElement =
    when (this) {
        is SignPayloadSubstrateResponse.Submit -> toJson(json)
        is SignPayloadSubstrateResponse.SubmitAndReturn -> toJson(json)
        is SignPayloadSubstrateResponse.Return -> toJson(json)
    }

fun TransferSubstrateResponse.Submit.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_transfer_broadcast_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "destination" to destination.toJson(json),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "transactionHash" to json.encodeToJsonElement(transactionHash),
        )
    )

fun TransferSubstrateResponse.SubmitAndReturn.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_transfer_broadcast_and_return_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "destination" to destination.toJson(json),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "transactionHash" to json.encodeToJsonElement(transactionHash),
            "signature" to json.encodeToJsonElement(signature),
            "payload" to json.encodeToJsonElement(payload),
        )
    )

fun TransferSubstrateResponse.Return.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_transfer_return_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "destination" to destination.toJson(json),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "signature" to json.encodeToJsonElement(signature),
            "payload" to json.encodeToJsonElement(payload),
        )
    )

fun SignPayloadSubstrateResponse.Submit.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_sign_payload_broadcast_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "destination" to destination.toJson(json),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "transactionHash" to json.encodeToJsonElement(transactionHash),
        )
    )

fun SignPayloadSubstrateResponse.SubmitAndReturn.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_sign_payload_broadcast_and_return_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "destination" to destination.toJson(json),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "transactionHash" to json.encodeToJsonElement(transactionHash),
            "signature" to json.encodeToJsonElement(signature),
            "payload" to json.encodeToJsonElement(payload),
        )
    )

fun SignPayloadSubstrateResponse.Return.toJson(json: Json = Json.Default): JsonElement =
    JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement("substrate_sign_payload_return_response"),
            "id" to json.encodeToJsonElement(id),
            "version" to json.encodeToJsonElement(version),
            "destination" to destination.toJson(json),
            "blockchainIdentifier" to json.encodeToJsonElement(blockchainIdentifier),
            "signature" to json.encodeToJsonElement(signature),
            "payload" to json.encodeToJsonElement(payload),
        )
    )

private fun Connection.Id.toJson(json: Json = Json.Default): JsonElement {
    val type = when (this) {
        is Connection.Id.P2P -> "p2p"
    }
    
    return JsonObject(
        mapOf(
            "type" to json.encodeToJsonElement(type),
            "id" to json.encodeToJsonElement(id),
        )
    )
}

private fun failWithUnknownPermissionBeaconRequest(request: PermissionBeaconRequest): Nothing =
    failWithIllegalArgument("Unknown Beacon permission request of type ${request::class}")

private fun failWithUnknownBlockchainBeaconRequest(request: BlockchainBeaconRequest): Nothing =
    failWithIllegalArgument("Unknown Beacon blockchain request of type ${request::class}")

private fun failWithUnknownPermissionBeaconResponse(response: PermissionBeaconResponse): Nothing =
    failWithIllegalArgument("Unknown Beacon permission response of type ${response::class}")

private fun failWithUnknownBlockchainBeaconResponse(response: BlockchainBeaconResponse): Nothing =
    failWithIllegalArgument("Unknown Beacon blockchain response of type ${response::class}")