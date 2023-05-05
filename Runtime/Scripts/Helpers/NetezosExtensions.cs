using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
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
            return new CoroutineWrapper<ulong>(getBalanceRequest, callback);
        }

        public static IEnumerator ReadView(string rpcUri, string destination, string entrypoint,
            object input, Action<JsonElement> onComplete = null)
        {
            var rpc = new Rpc(rpcUri);
            var runViewOp = rpc.RunView<JsonElement>(destination, entrypoint, input);

            return new CoroutineWrapper<JsonElement>(runViewOp, (JsonElement result) =>
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
            yield return new CoroutineWrapper<JsonElement>(scriptOp, (JsonElement script) =>
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

        public static bool VerifySignature(string pubKey, SignPayloadType signingType, string payload, string signature)
        {
            var parsedPubKey = PubKey.FromBase58(pubKey);
            var payloadBytes = signingType == SignPayloadType.raw
                ? Encoding.UTF8.GetBytes(GetPayloadString(signingType, payload))
                : Hex.Parse(GetPayloadString(signingType, payload));

            return parsedPubKey.Verify(payloadBytes, signature);
        }

        public static string GetPayloadString(SignPayloadType signingType, string plainTextPayload)
        {
            switch (signingType)
            {
                case SignPayloadType.raw:
                    return plainTextPayload;
                case SignPayloadType.micheline or SignPayloadType.operation:
                {
                    var bytes = Hex.Convert(Encoding.UTF8.GetBytes(plainTextPayload));
                    var bytesLength = (bytes.Length / 2).ToString("x");
                    var addPadding = "00000000" + bytesLength;
                    var paddedBytesLength = addPadding[^8..];
                    var startPrefix = signingType == SignPayloadType.micheline ? "0501" : "0300";
                    var payloadBytes = startPrefix + paddedBytesLength + bytes;
                    return payloadBytes;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(signingType), signingType, null);
            }
        }
    }
}