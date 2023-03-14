//
//  UnityPlugin.swift
//  beacon_framework
//
//  Created by Victor Apihtin on 08/07/2022.
//

import Foundation
import Base58Swift
import BeaconClientDApp

import BeaconCore
//import BeaconBlockchainSubstrate
import BeaconBlockchainTezos
import BeaconClientDApp
import BeaconTransportP2PMatrix

@objc public class UnityPlugin : NSObject {
    @objc public static let shared = UnityPlugin()
    
    @Published private(set) var started: Bool = false
    @Published private(set) var beaconResponse: String? = nil
    @Published private(set) var pairingRequest: String? = nil
    
    let encoder = JSONEncoder()
    
    public var beaconClient: Beacon.DAppClient? = nil {
        didSet {
            started = beaconClient != nil
        }
    }
    
    var pairingBackgroundTaskID: UIBackgroundTaskIdentifier?
    
    var onNativeSendMessage:UnsafeRawPointer?
    var onUnitySendMessage:UnsafeRawPointer?
    
    enum MessageType : Int
    {
        case Log
        case Initialization
        case Handshake
        case Account
        case Signature
        case PublicKey
        case Pairing
        case Operation
    }

    public func NativeSendMessage(type: Int, str:String, length: Int)
    {
        let c = unsafeBitCast(onNativeSendMessage, to: (@convention(c) (Int, UnsafePointer<CChar>, Int) -> ()).self)
        c(type, str, length);
    }
 
    
    public func UnitySendMessage(clbck:String, msg: String)
    {
        let c = unsafeBitCast(onUnitySendMessage, to: (@convention(c) (UnsafePointer<CChar>, UnsafePointer<CChar>) -> ()).self)
        c(clbck, msg)
    }

    @objc public func RegisterNativeSendMessage(callback: UnsafeRawPointer?)
    {
        onNativeSendMessage = callback;
    }
    
    @objc public func RegisterUnitySendMessage(callback: UnsafeRawPointer?)
    {
        onUnitySendMessage = callback;
    }

    @objc public func start()
    {
        guard beaconClient == nil else {
            listenForResponses()
            return
        }
        
        do {
            Beacon.DAppClient.create(
                with: .init(
                    name: "iOS Beacon SDK Demo (DApp)",
                    blockchains: [Tezos.factory/*, Substrate.factory*/],
                    connections: [try Transport.P2P.Matrix.connection()]
                )
            ) { result in
                switch result {
                case let .success(client):
                    print("Beacon client created")
                    
                    DispatchQueue.main.async {
                        self.beaconClient = client
                        self.listenForResponses()
                    }
                case let .failure(error):
                    print("Could not create Beacon client, got error: \(error)")
                }
            }
        } catch {
            print("Could not create Beacon client, got error: \(error)")
        }
    }
    
    private func listenForResponses() {
        beaconClient?.connect { result in
            switch result {
            case .success(_):
                print("Beacon client connected")
                self.beaconClient?.listen(onResponse: self.onTezosResponse)
                
                self.UnitySendMessage(clbck: "OnClientCreated", msg: "success")
                
            case let .failure(error):
                print("Error while connecting for messages \(error)")
            }
        }
    }
    
    @objc public func stop()
    {
        beaconClient?.disconnect {
            print("disconnected \($0)")
            
            self.UnitySendMessage(clbck: "OnAccountDisconnected", msg: "disconnected")
        }
    }

    func clearResponse() {
        DispatchQueue.main.async {
            self.beaconResponse = nil
        }
    }
    
    @objc public func pair() {
    
        self.beaconClient?.pair(using: .p2p) { result in
            switch result {
            case let .success(pairingMessage):
                switch pairingMessage {
                case let .request(pairingRequest):
                    if let serializedRequest = try? self.beaconClient?.serializePairingData(.request(pairingRequest)) {
                        
                        self.pairingBackgroundTaskID = UIApplication.shared.beginBackgroundTask(withName: "Pair") {
                            guard let backgroundTaskID = self.pairingBackgroundTaskID, backgroundTaskID != .invalid else {
                                return
                            }
                            
                            UIApplication.shared.endBackgroundTask(backgroundTaskID)
                            self.pairingBackgroundTaskID = .invalid
                        }
                        
                        
                        DispatchQueue.main.async {
                            self.pairingRequest = serializedRequest
                            let txt = self.pairingRequest!
                            
                            self.UnitySendMessage(clbck: "OnHandshakeReceived", msg: txt)
                            //self.NativeSendMessage(type:MessageType.Handshake.rawValue, str: txt, length:txt.count)
                        }

                    } else {
                        print("Failed to pair, unable to serialize a pairing request")
                    }
                case let .response(pairingResponse):
                    print("pairing response:")
                    print(pairingResponse)
                    DispatchQueue.main.async {
                        
                        let data = try? self.encoder.encode(pairingResponse)
                        let json = String(data: data!, encoding: .utf8)!
                        
                        //self.UnitySendMessage(clbck: "OnPairingCompleted", msg: json)
                        self.NativeSendMessage(type: MessageType.Pairing.rawValue, str: json, length: json.count)
                    }
                }
            case let .failure(error):
                print("Failed to pair, got error: \(error)")
            }
        }
    }
    
    @objc public func unpair()
    {
        beaconClient?.removeAllPeers { result in
            switch result {
            case .success(_):
                print("Successfully removed peers")
                self.UnitySendMessage(clbck: "OnAccountDisconnected", msg: "disconnected")
            case let .failure(error):
                print("Failed to remove peers, got error: \(error)")
            }
        }
    }
    
    /* should be performed AFTER a successful pairing */
    @objc public func requestPermission(
        networkName:UnsafePointer<CChar>,
        networkRPC:UnsafePointer<CChar>
    ) {
        let networkNameString = String(cString: UnsafePointer<CChar>(networkName))
        let rpcString = String(cString: UnsafePointer<CChar>(networkRPC))
        
        let network = Tezos.Network.init(type:.custom, name:networkNameString, rpcURL:rpcString)
        
        /* should be performed AFTER a successful pairing */
        beaconClient?.requestTezosPermission(on: network) { result in
               
            switch result {
               case .success(_):
                
                   if let backgroundTaskID = self.pairingBackgroundTaskID, backgroundTaskID != .invalid {
                       UIApplication.shared.endBackgroundTask(backgroundTaskID)
                       self.pairingBackgroundTaskID = nil
                   }
                
                   print("Sent the request")
               case let .failure(error):
                   print("Failed to send the request, got error: \(error)")
               }
           }
    }

    @objc public func RequestTezosOperation(
        destination:UnsafePointer<CChar>,
        entryPoint:UnsafePointer<CChar>,
        arg:UnsafePointer<CChar>,
        amount:UnsafePointer<CChar>
        , networkName:UnsafePointer<CChar>
        , networkRPC:UnsafePointer<CChar>
    ) {
        
        let destinationString = String(cString: UnsafePointer<CChar>(destination))
        let amountString = String(cString: UnsafePointer<CChar>(amount))
        let entryPointString = String(cString: UnsafePointer<CChar>(entryPoint))
        let argString = String(cString: UnsafePointer<CChar>(arg))
        
        let netNameString = String(cString: UnsafePointer<CChar>(networkName))
        let netRPCString = String(cString: UnsafePointer<CChar>(networkRPC))
        
        var contractParameter : Tezos.Operation.Parameters? = nil;

        let entry : Tezos.Operation.Parameters.Entrypoint = .custom(entryPointString)
                
        let decoder = JSONDecoder()
        do {
            let expression = try decoder.decode(Micheline.MichelsonV1Expression.self, from: Data(argString.utf8))
            contractParameter = .init(entrypoint: entry , value: expression)
        }
        catch
        {
            print(error)
        }
        
        let network = Tezos.Network.init(type:.custom, name:netNameString, rpcURL:netRPCString)
        
        beaconClient?.requestTezosOperation(operationDetails: [
            .transaction(.init(
                amount: amountString,
                destination: destinationString,
                parameters: contractParameter
            ))
        ], on: network
        ) { result in
            switch result {
            case let .success(message):
                print(message)
            case let .failure(error):
                print(error)
            }
        }
    }

    @objc public func RequestTezosSignPayload(
        signingType:Int,
        payload:UnsafePointer<CChar>
    ) {
        let data = String(cString: UnsafePointer<CChar>(payload))

        var type = Tezos.SigningType.raw;
        
        switch signingType {
        case 1:
            type = Tezos.SigningType.operation;
        case 2:
            type = Tezos.SigningType.micheline;
        default:
            type = Tezos.SigningType.raw;
        }
        
        print("Request sign payload with type " + type.rawValue)
        beaconClient?.requestTezosSignPayload(signingType:type, payload:data) { result in
            switch result {
            case let .success(message):
                print(message)
                
            case let .failure(error):
                print(error)
            }
        }
    }

    @objc public func RequestTezosBroadcast(
        signedTransaction:UnsafePointer<CChar>,
        networkName:UnsafePointer<CChar>,
        networkRPC:UnsafePointer<CChar>
    ) {
        let transaction = String(cString: UnsafePointer<CChar>(signedTransaction))
        let network = String(cString: UnsafePointer<CChar>(networkName))
        let rpc = String(cString: UnsafePointer<CChar>(networkRPC))
        
        beaconClient?.requestTezosBroadcast(signedTransaction:transaction) { result in
            switch result {
            case let .success(message):
                print(message)
            case let .failure(error):
                print(error)
            }
        }
    }
    
    @objc public func GetActiveAccount()
    {
        beaconClient?.getActiveAccount { accountResult in
            do {
            
                if let value = try accountResult.get()
                {
                    DispatchQueue.main.async {

                        let data = try? self.encoder.encode(value)
                        let json = String(data: data!, encoding: .utf8)!
                    
                        print(json)
                        self.UnitySendMessage(clbck: "OnAccountReceived", msg: json)
                        //self.NativeSendMessage(type: MessageType.Account.rawValue, str: json, length: json.count)
                    }
                    
                }
                else
                {
       
                }
                
            } catch {
                print("Error retrieving the value: \(error)")
            }
        }
    }

    private func onTezosResponse(_ responseResult: Result<BeaconResponse<Tezos>, Beacon.Error>) {

        switch responseResult {
        case let .success(request):
            
            switch request {
            
            case let .blockchain(chain):
                
                switch chain {
            
                case let .signPayload(payload):
                    print(payload)
                    DispatchQueue.main.async {
                        
                        let data = try? self.encoder.encode(payload)
                        let json = String(data: data!, encoding: .utf8)!
                        
                        self.UnitySendMessage(clbck: "OnPayloadSigned", msg: json)
                    }
                case let .broadcast(broadcast):
                    print(broadcast)
                    
                case let .operation(operation):
                    DispatchQueue.main.async {
                        let data = try? self.encoder.encode(operation)
                        let json = String(data: data!, encoding: .utf8)!
                        
                        self.UnitySendMessage(clbck: "OnContractCallCompleted", msg: json)
                    }
                    print(operation)
                }
                
            case let .acknowledge(acknowledge):
                print("acknowledge:")
                print(acknowledge)
                
            case let .permission(permission):
                DispatchQueue.main.async {
                    //let key = permission.account.publicKey;
                    
                    
                    let data = try? self.encoder.encode(permission)
                    let json = String(data: data!, encoding: .utf8)!
                    
                    print("permission:")
                    print(permission)
                    
                    self.UnitySendMessage(clbck: "OnAccountConnected", msg: json)
                    //self.NativeSendMessage(type: MessageType.PublicKey.rawValue, str: json, length: json.count);
                }
            case let .error(error):
                print(error)
            }
            /*
            let requestData = try? encoder.encode(request)
            
            DispatchQueue.main.async {
                self.beaconResponse = requestData.flatMap { String(data: $0, encoding: .utf8) }
                let response = self.beaconResponse!
            }
             */
        
        case let .failure(error):
            print("Error while processing incoming messages: \(error)")
            //completion(.failure(error))
        }
    }
}

extension BeaconResponse: Encodable {
    
    public func encode(to encoder: Encoder) throws {
        if let tezosResponse = self as? BeaconResponse<Tezos> {
            switch tezosResponse {
            case let .permission(content):
                try content.encode(to: encoder)
            case let .blockchain(blockchain):
                switch blockchain {
                case let .operation(content):
                    try content.encode(to: encoder)
                case let .signPayload(content):
                    try content.encode(to: encoder)
                case let .broadcast(content):
                    try content.encode(to: encoder)
                }
            case let .acknowledge(content):
                try content.encode(to: encoder)
            case let .error(content):
                try content.encode(to: encoder)
            }
        }
        else {
            throw Error.unsupportedBlockchain
        }
    }
    
    enum Error: Swift.Error {
        case unsupportedBlockchain
    }
}
