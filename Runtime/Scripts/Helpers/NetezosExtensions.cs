using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Netezos.Contracts;
using Netezos.Encoding;
using UnityEngine;
using Netezos.Keys;
using TezosAPI;


namespace BeaconSDK
{
    public static class NetezosExtensions
    {
        private static readonly Dictionary<string, ContractScript> _contracts = new();

        public static IEnumerator ReadTZBalance(string rpcUri, string sender, Action<ulong> callback)
        {
            var rpc = new Rpc(rpcUri);
            var getBalanceRequest = rpc.GetTzBalance<ulong>(sender);
            return HttpClient.WrappedRequest(getBalanceRequest, callback);
        }

        public static IEnumerator ReadView(string rpcUri, string destination, string entrypoint,
            object input, Action<JsonElement> onComplete = null)
        {
            var rpc = new Rpc(rpcUri);
            var runViewOp = rpc.RunView<JsonElement>(destination, entrypoint, input);

            return HttpClient.WrappedRequest(runViewOp, (JsonElement result) =>
            {
                if (result.ValueKind != JsonValueKind.Null && result.ValueKind != JsonValueKind.Undefined &&
                    result.TryGetProperty("data", out var val))
                    onComplete(val);
                else
                    Debug.LogError("Invalid data");
            });
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
            if (_contracts.ContainsKey(contract)) yield break;
            var rpc = new Rpc(rpcUri);
            var scriptOp = rpc.GetContractCode<JsonElement>(contract);
            yield return HttpClient.WrappedRequest(scriptOp, (JsonElement script) =>
            {
                var codeElement = script.GetProperty("code").GetRawText();
                var code = Micheline.FromJson(codeElement);
                _contracts[contract] = new ContractScript(code);
            });
        }

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
            var parsedPubKey = PubKey.FromBase58(pubKey);
            return parsedPubKey.Verify(Hex.Parse(GetPayloadHexString(payload)), signature);
        }
        
        public static string GetPayloadHexString(string input)
        {
            var hexOutput = new StringBuilder();

            foreach (var asciiCode in input.Select(character => (int)character))
            {
                hexOutput.Append(asciiCode.ToString("x2"));
            }

            var bytes = hexOutput.ToString();
            var bytesLength = (bytes.Length / 2).ToString("x");
            var addPadding = "00000000" + bytesLength;
            var paddedBytesLength = addPadding[^8..];
            var payloadBytes = "05" + "01" + paddedBytesLength + bytes;

            return payloadBytes;
        }
    }
}