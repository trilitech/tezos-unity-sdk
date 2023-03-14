//
//  UnityPluginBridge.m
//  beacon_framework
//
//  Created by Victor Apihtin on 08/07/2022.
//

#import <Foundation/Foundation.h>
#include "UnityFramework/UnityFramework-Swift.h"


extern "C" {
    typedef void (*ReceiveCallback)(int type, const char* msg, int length);
}

ReceiveCallback receiveMessageCallback;

static void NativeSendMessageToUnity(int type, const char* msg, int length)
{
    if (receiveMessageCallback != NULL) {
        receiveMessageCallback(type, msg, length);
    }
}

static void UnitySendMessageToUnity(const char* func, const char* msg)
{
    UnitySendMessage("UnityBeacon", func, msg);
}

extern "C" {

    void _RegisterReceiveMessageCallback(ReceiveCallback callback) {
        receiveMessageCallback = callback;
        
        const void *n =(const void*)&NativeSendMessageToUnity;
        [[UnityPlugin shared] RegisterNativeSendMessageWithCallback:(n)];
        
        const void *u =(const void*)&UnitySendMessageToUnity;
        [[UnityPlugin shared] RegisterUnitySendMessageWithCallback:(u)];
    }

    void _IOSConnectAccount() {
        [[UnityPlugin shared] start];
    }
    
    void _IOSDisconnectAccount() {
        [[UnityPlugin shared] unpair];
        //[[UnityPlugin shared] stop];
    }

    void _IOSPair() {
        [[UnityPlugin shared] pair];
    }
        
    void _IOSRequestTezosOperation(
        char* destination,
        char* entryPoint,
        char* arg,
        char* amount,
        char* networkName,
        char* networkRPC
    ) {
        [[UnityPlugin shared] RequestTezosOperationWithDestination:destination entryPoint:entryPoint arg:arg amount:amount networkName:networkName networkRPC:networkRPC];
    }
    
    void _IOSRequestTezosPermission(
        char* networkName,
        char* networkRPC
    ) {
        [[UnityPlugin shared] requestPermissionWithNetworkName:networkName networkRPC:networkRPC];
    }

    void _IOSRequestTezosSignPayload(
        int signingType,
        char* payload
    ) {
        [[UnityPlugin shared] RequestTezosSignPayloadWithSigningType:signingType payload:payload];
    }

    void _IOSRequestTezosBroadcast(
        char* signedTransaction,
        char* networkName,
        char* networkRPC
    ) {
        [[UnityPlugin shared] RequestTezosBroadcastWithSignedTransaction:signedTransaction networkName:networkName networkRPC:networkRPC];
    }

    void _IOSGetActiveAccount()
    {
        [[UnityPlugin shared] GetActiveAccount];
    }
}
