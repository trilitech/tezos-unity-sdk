using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Netezos.Contracts;
using Netezos.Encoding;
using Netezos.Rpc;
using UnityEngine;
using Netezos.Keys;


namespace BeaconSDK
{
    public static class NetezosExtensions
    {
        private static Dictionary<string, ContractScript> _contracts = new Dictionary<string, ContractScript>();

        public static IEnumerator ReadTZBalance(string rpcUri, string sender, Action<ulong> callback)
        {
            var rpc = new TezosRpc(rpcUri);
            var t = Task.Run(async () =>  await rpc.Blocks.Head.Context.Contracts[sender].Balance.GetAsync<ulong>());
            yield return new WaitUntil(() => t.IsCompleted);
            callback.Invoke(t.Result);
        }

        public static IEnumerator ReadView(string rpcUri, string destination, string entrypoint,
            object input, Action<JsonElement> onComplete = null)
        {
            var rpc = new TezosRpc(rpcUri);

            var t = Task.Run(async () =>
            {
                var result = await rpc.Blocks.Head.Helpers.Scripts.RunScriptView
                    .PostAsync<JsonElement>(destination, entrypoint, input);

                if (result.ValueKind != JsonValueKind.Null && result.ValueKind != JsonValueKind.Undefined &&
                    result.TryGetProperty("data", out JsonElement val))
                    return val;
                else
                {
                    Debug.LogError("Invalid data");
                    return new JsonElement();
                }
                
            });
            yield return new WaitUntil(() => t.IsCompleted);
            onComplete(t.Result);
        }

        public static IEnumerator HumanizeValue<T>(JsonElement val, string rpcUri, string destination,
            string humanizeEntrypoint, Action<T> onComplete)
        {
            yield return FetchContractCode(rpcUri, destination);
            var cs = _contracts[destination];
            // getting parameters section as readable json:
            var json = cs.HumanizeParameter(humanizeEntrypoint, Micheline.FromJson(val));
            var readResult = JsonSerializer.Deserialize<T>(json);
            onComplete?.Invoke(readResult);
        }

        private static IEnumerator FetchContractCode(string rpcUri, string contract)
        {
            if (!_contracts.ContainsKey(contract))
            {
                var rpc = new TezosRpc(rpcUri);

                var t = Task.Run(async () =>
                {
                    var script = await rpc.Blocks.Head.Context.Contracts[contract].Script.GetAsync<JsonElement>();
                    var codeElement = script.GetProperty("code").GetRawText();
                    return Micheline.FromJson(codeElement);
                });

                yield return new WaitUntil(() => t.IsCompleted);
                _contracts[contract] = new ContractScript(t.Result);
            }
        }

        // private static IEnumerator RpcRequest<T>(IEnumerator op, Action<T> callback)
        // {
        //     var counterRoutine = new CoroutineWrapper<T>(op);
        //     yield return counterRoutine;
        //     var counter = counterRoutine.Result;
        //     callback?.Invoke(counter);
        // }

        public static IEnumerator CompileToJSONMichelson(string rpcUri, string destination,
            string entry, object objArg, Action<string> onComplete)
        {
            yield return FetchContractCode(rpcUri, destination);
            var cs = _contracts[destination];

            var asMichelson = cs.BuildParameter(entry, objArg);
            onComplete?.Invoke(asMichelson.ToJson());
        }

        public static bool VerifySignature(string pubKey, string payload, string signature)
        {
            var pubkey = PubKey.FromBase58(pubKey);
            var payloadBytes = Hex.Parse(payload);
            return pubkey.Verify(payloadBytes, signature);
        }
    }
}