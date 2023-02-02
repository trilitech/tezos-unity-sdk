using System.Collections;
using System.Linq;
using Netezos.Forging;
using Netezos.Forging.Models;
using Netezos.Keys;
using Netezos.Rpc;
using UnityEngine;

/// <summary>
/// This is a sample with the steps needed to transfer some Tez
/// from account A to account B.
///
/// And this also helps to exemplifies how to use <see cref="CoroutineWrapper{T}"/>
/// instead of async operations.
/// </summary>
public class NetezosTransactionSample : MonoBehaviour
{
	/// <summary>
	/// Key that will be used to transfer tez
	/// </summary>
	[SerializeField, Tooltip("Key that will be used to transfer tez")]
	private string _privateKeyBase58 = "edsk34ZXiGFoxJZrovxtQhFLTGhVLkZKRSzy3AzjnuoHTm5QDgPBp3";

	/// <summary>
	/// Address where the transfer will arrive
	/// </summary>
	[SerializeField, Tooltip("Address where the transfer will arrive")]
	private string _destinationAddress = "tz1fKvpg4tNM5EjhjSStm9yTfqKRLuL1KDFZ";

	/// <summary>
	/// URI of the network to connect the rpc
	/// </summary>
	[SerializeField, Tooltip("URI of the network to connect the rpc")]
	private string _rpcNetwork = "https://jakartanet.ecadinfra.com";

	/// <summary>
	/// Amount of ꜩ to transfer (1000000ꜩ is equal to 1 tez)
	/// </summary>
	[SerializeField, Tooltip("Amount of ꜩ to transfer (1000000ꜩ is equal to 1 tez)")]
	private long _amount = 100000000;

	[SerializeField]
	private int _gasLimit = 150307;

	[SerializeField]
	private long _fee = 1282;

	IEnumerator Start()
	{
		// This is an account used for testing purpuses.
		var key = Key.FromBase58(_privateKeyBase58);
		Debug.Log($"Secret key: {key.GetBase58()}");
		Debug.Log($"tz Address {key.Address}");
		Debug.Log($"Pub key: {key.PubKey}");

		// Connecting to jakartanet for testing purpuses
		var rpc = new TezosRpc(_rpcNetwork);

		// Get counter
		var counterOperation = new CoroutineWrapper<int>(rpc.Blocks.Head.Context.Contracts[key.PubKey.Address].Counter.GetAsync<int>());
		yield return counterOperation;
		int counter = counterOperation.Result;

		var content = new ManagerOperationContent[]
		{
			// Note: Reveal operation should be used only if the account wasn't reveled before
			// otherwise it can be removed.
			/*
			 new RevealContent
			{
				Source = key.PubKey.Address,
				Counter = ++counter,
				PublicKey = key.PubKey.GetBase58(),
				GasLimit = 1500,
				Fee = 1000 // 0.001 tez
            },
			*/
			new TransactionContent
			{
				Source = key.PubKey.Address,
				Counter = ++counter,
				// 100000000 is 100 tez
				Amount = _amount,
				Destination = _destinationAddress,
				GasLimit = _gasLimit,
				Fee = _fee,
			}
		 };

		var branchOperation = new CoroutineWrapper<string>(rpc.Blocks.Head.Hash.GetAsync<string>());
		yield return branchOperation;
		var branch = branchOperation.Result;
		Debug.Log("Branch " + branch);

		// Forge the operation locally
		var forgeOperation = new CoroutineWrapper<byte[]>(new LocalForge().ForgeOperationGroupAsync(branch, content));
		yield return forgeOperation;
		var bytes = forgeOperation.Result;
		Debug.Log("Forge");

		// Sign the operation
		byte[] signature = key.SignOperation(bytes);
		Debug.Log("Signed");

		// Inject operation
		var injectOperation = new CoroutineWrapper<dynamic>(rpc.Inject.Operation.PostAsync(bytes.Concat(signature)));
		yield return injectOperation;
		//Debug.Log("Injected " + injectOperation.Result);

		yield return null;
	}
}
