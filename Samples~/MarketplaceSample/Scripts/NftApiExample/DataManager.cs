using System;
using System.Collections.Generic;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models.Filters;
using TezosSDK.Tezos.API.Models.Tokens;
using UnityEngine;

namespace TezosSDK.MarketplaceSample.NftApiExample
{

	public class DataManager : MonoBehaviour
	{
		private const int MaxTokens = 20;

		public Action<string> DataReceived;
		private string _checkAddress;
		private string _checkContract;
		private string _checkTokenId;
		private string _connectedAddress;
		private ITezos _tezos;

		private void Start()
		{
			_tezos = TezosManager.Instance.Tezos;
			_tezos.Wallet.EventManager.WalletConnected += OnAccountConnected;
		}

		private void OnAccountConnected(WalletInfo walletInfo)
		{
			_connectedAddress = walletInfo.Address;
		}

		public void GetTokensForOwners()
		{
			var walletAddress = string.IsNullOrEmpty(_checkAddress) ? _connectedAddress : _checkAddress;

			CoroutineRunner.Instance.StartCoroutine(_tezos.API.GetTokensForOwner(tbs =>
			{
				if (tbs == null)
				{
					DataReceived.Invoke($"Incorrect address - {walletAddress}");
					Debug.Log($"Incorrect address - {walletAddress}");
					return;
				}

				var tokens = new List<TokenBalance>(tbs);

				if (tokens.Count > 0)
				{
					var result = "";

					foreach (var tb in tokens)
					{
						result += $"{walletAddress} has {tb.Balance} tokens on contract {tb.TokenContract.Address}" +
						          "\r\n" + "\r\n";

						Debug.Log($"{walletAddress} has {tb.Balance} tokens on contract {tb.TokenContract.Address}");
					}

					DataReceived.Invoke(result);
				}
				else
				{
					DataReceived.Invoke($"{walletAddress} has no tokens");
					Debug.Log($"{walletAddress} has no tokens");
				}
			}, walletAddress, false, MaxTokens, new TokensForOwnerOrder.Default(0)));
		}

		public void IsHolderOfContract()
		{
			var walletAddress = string.IsNullOrEmpty(_checkAddress) ? _connectedAddress : _checkAddress;

			if (string.IsNullOrEmpty(_checkContract))
			{
				DataReceived.Invoke("Enter contract address");
				Debug.Log("Enter contract address");
				return;
			}

			CoroutineRunner.Instance.StartCoroutine(_tezos.API.IsHolderOfContract(flag =>
			{
				var message = flag
					? $"{walletAddress} is HOLDER of contract {_checkContract}"
					: $"{walletAddress} is NOT HOLDER of contract {_checkContract}";

				DataReceived.Invoke(message);
				Debug.Log(message);
			}, walletAddress, _checkContract));
		}

		public void IsHolderOfToken()
		{
			var walletAddress = string.IsNullOrEmpty(_checkAddress) ? _connectedAddress : _checkAddress;

			var tokenId = string.IsNullOrEmpty(_checkTokenId) ? 0 : Convert.ToUInt32(_checkTokenId);

			if (string.IsNullOrEmpty(_checkContract))
			{
				DataReceived.Invoke("Enter contract address");
				Debug.Log("Enter contract address");
				return;
			}

			CoroutineRunner.Instance.StartCoroutine(_tezos.API.IsHolderOfToken(flag =>
			{
				var message = flag ? $"{walletAddress} is HOLDER of token" : $"{walletAddress} is NOT HOLDER of token";

				DataReceived.Invoke(message);
				Debug.Log(message);
			}, walletAddress, _checkContract, tokenId));
		}

		public void SetCheckAddress(string address)
		{
			_checkAddress = address;
		}

		public void SetCheckContract(string contract)
		{
			_checkContract = contract;
		}

		public void SetCheckTokenId(string tokenId)
		{
			_checkTokenId = tokenId;
		}
	}

}