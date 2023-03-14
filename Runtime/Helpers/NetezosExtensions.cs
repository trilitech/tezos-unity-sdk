using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using Netezos.Contracts;
using Netezos.Encoding;
using Netezos.Forging.Models;
using Netezos.Rpc;
using Netezos.Rpc.Queries;
using Netezos.Rpc.Queries.Post;
using UnityEngine;
using BeaconSDK;
using Netezos.Keys;

namespace BeaconSDK
{
	public static class NetezosExtensions
	{
		private static Dictionary<string, ContractScript> _contracts = new Dictionary<string, ContractScript>();

		public static IEnumerator ReadTZBalance(string rpcUri, string sender, Action<ulong> callback)
		{
			TezosRpc rpc = new TezosRpc(
				rpcUri
			);
			var getCounter = rpc.Blocks.Head.Context.Contracts[sender].Balance.GetAsync<ulong>();
			yield return RpcRequest(getCounter, callback);
		}

		public static IEnumerator ReadView(string rpcUri, string destination, string entrypoint,
			object input, Action<JsonElement> onComplete = null)
		{
			TezosRpc rpc = new TezosRpc(
				rpcUri
			);
			var runViewOp = rpc.Blocks.Head.Helpers.Scripts.RunView()
				.PostAsync<JsonElement>(destination, entrypoint, input);

			yield return RpcRequest(runViewOp, (JsonElement result) =>
			{
				if (result.ValueKind != JsonValueKind.Null && result.ValueKind != JsonValueKind.Undefined &&
				    result.TryGetProperty("data", out JsonElement val))
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
			if (!_contracts.ContainsKey(contract))
			{
				TezosRpc rpc = new TezosRpc(
					rpcUri
				);
				var scriptOp = rpc.Blocks.Head.Context.Contracts[contract].Script.GetAsync<JsonElement>();
				yield return RpcRequest(scriptOp,
					(JsonElement script) =>
					{
						var codeElement = script.GetProperty("code").GetRawText();
						var code = Micheline.FromJson(codeElement);
						_contracts[contract] = new ContractScript(code);
					});
			}
		}

		private static IEnumerator RpcRequest<T>(IEnumerator op, Action<T> callback)
		{
			var counterRoutine = new CoroutineWrapper<T>(op);
			yield return counterRoutine;
			var counter = counterRoutine.Result;
			callback?.Invoke(counter);
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
			var pubkey = PubKey.FromBase58(pubKey);
			var payloadBytes = Hex.Parse(payload);
			return pubkey.Verify(payloadBytes, signature);
		}
	}
}
