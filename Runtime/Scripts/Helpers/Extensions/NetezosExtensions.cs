using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Beacon.Sdk.Beacon.Sign;
using Netezos.Contracts;
using Netezos.Encoding;
using Netezos.Keys;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API;

namespace TezosSDK.Helpers.Extensions
{

	public static class NetezosExtensions
	{
		private static readonly Dictionary<string, ContractScript> Contracts = new();

		public static IEnumerator HumanizeValue<T>(
			JsonElement val,
			string rpcUri,
			string destination,
			string humanizeEntrypoint,
			Action<T> onComplete)
		{
			yield return FetchContractCode(rpcUri, destination);
			var cs = Contracts[destination];
			// getting parameters section as readable json:
			var json = cs.HumanizeParameter(humanizeEntrypoint, Micheline.FromJson(val));
			var readResult = JsonSerializer.Deserialize<T>(json);
			onComplete?.Invoke(readResult);
		}

		private static IEnumerator FetchContractCode(string rpcUri, string contract)
		{
			if (Contracts.ContainsKey(contract))
			{
				yield break;
			}

			var rpc = new Rpc(TezosManager.Instance.Config.DataProvider);
			var scriptOp = rpc.GetContractCode<JsonElement>(contract);

			yield return new CoroutineWrapper<JsonElement>(scriptOp, script =>
			{
				var codeElement = script.GetProperty("code").GetRawText();
				var code = Micheline.FromJson(codeElement);
				Contracts[contract] = new ContractScript(code);
			});
		}

		public static IEnumerator CompileToJsonMichelson(
			string rpcUri,
			string destination,
			string entry,
			object objArg,
			Action<string> onComplete)
		{
			yield return FetchContractCode(rpcUri, destination);
			var cs = Contracts[destination];

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