using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Netezos.Forging;
using Netezos.Forging.Models;
using Netezos.Keys;
using Netezos.Rpc;
using UnityEngine;
using UnityEngine.Networking;

public class NetezosTest : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        Mnemonic mnemonic = new Mnemonic("degree saddle wedding essence rubber fire dismiss catalog deal remain about reason");
        var key = Key.FromMnemonic(mnemonic, "Testwallet123");
       

        Debug.LogError($"Secret key {key.GetBase58()}");
        Debug.LogError($"Tz address {key.PubKey.Address}");

        // use this address to receive some tez
        var address = key.PubKey.Address; // tz1fKvpg4tNM5EjhjSStm9yTfqKRLuL1KDFZ

        using var rpc = new TezosRpc("https://rpczero.tzbeta.net/");

        CoroutineWrapper<string> headCoroutine = new CoroutineWrapper<string>(rpc.Blocks.Head.Hash.GetAsync<string>());
        yield return headCoroutine;

        string head = headCoroutine.Result;
        Debug.LogError(head);

        
        // get account's counter
        CoroutineWrapper<int> counterCoroutine = new CoroutineWrapper<int>( rpc.Blocks.Head.Context.Contracts[address].Counter.GetAsync<int>());
        yield return counterCoroutine;
        int counter = counterCoroutine.Result;

        Debug.LogError(counter);

        var content = new ManagerOperationContent[]
        {
            new RevealContent
            {
                Source = address,
                Counter = ++counter,
                PublicKey = key.PubKey.GetBase58(),
                GasLimit = 1500,
                Fee = 1275 // 0.001 tez
            },
            new TransactionContent
            {
                Source = address,
                Counter = ++counter,
                Amount = 1000000, // 1 tez
                Destination = "tz1KhnTgwoRRALBX6vRHRnydDGSBFsWtcJxc",
                GasLimit = 1500,
                Fee = 1275 // 0.001 tez
            }
        };

        CoroutineWrapper<byte[]> bytesCoroutine = new CoroutineWrapper<byte[]>(new LocalForge().ForgeOperationGroupAsync(head, content));
        yield return bytesCoroutine;
        var bytes = bytesCoroutine.Result;

        // sign the operation bytes
        byte[] signature = key.SignOperation(bytes);


        // inject the operation and get its id (operation hash)
        CoroutineWrapper<dynamic> dynamicCoroutine = new CoroutineWrapper<dynamic>(rpc.Inject.Operation.PostAsync(bytes.Concat(signature)));
        yield return dynamicCoroutine;
        dynamic result = dynamicCoroutine.Result;
    }

    private IEnumerator Coroutine()
    {
        using (var newRequest = UnityWebRequest.Get("https://mainnet-tezos.giganode.io/chains/main/blocks/head/hash"))
        {
            newRequest.SetRequestHeader("Accept", "application/json");
            newRequest.SetRequestHeader("Content-Type", "application/json");
            newRequest.SetRequestHeader("User-Agent", "Netezos/" + Assembly.GetExecutingAssembly().GetName().Version.ToString(2));
            newRequest.timeout = (int)10;
            newRequest.SendWebRequest();
            while (!newRequest.isDone)
            {
                Debug.LogError(newRequest.downloadProgress);
                yield return null;
            }

            Debug.LogError(newRequest.result);
            Debug.LogError(newRequest.downloadHandler.text);
        }
    }
}
