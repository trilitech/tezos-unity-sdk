using System;
using System.Linq;
using TezosSDK.Tezos.Filters;
using TezosSDK.Tezos.Interfaces;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using UnityEngine;

namespace TezosSDK.Samples.MarketplaceSample.NftApiExample
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
			_tezos.WalletEventProvider.EventManager.WalletConnected += OnAccountConnected;
		}

		private void OnAccountConnected(WalletInfo walletInfo)
		{
			_connectedAddress = walletInfo.Address;
		}

		public void GetTokensForOwners()
		{
			var walletAddress = string.IsNullOrEmpty(_checkAddress) ? _connectedAddress : _checkAddress;

			StartCoroutine(_tezos.API.GetTokensForOwner(result =>
			{
				if (!result.Success)
				{
					DataReceived.Invoke(result.ErrorMessage);
					Debug.Log(result.ErrorMessage);
					return;
				}

				var tokens = result.Data.ToList();

				if (tokens.Count > 0)
				{
					var message = "";

					foreach (var tb in tokens)
					{
						message += $"{walletAddress} has {tb.Balance} tokens on contract {tb.TokenContract.Address}" +
						           "\r\n" + "\r\n";

						Debug.Log($"{walletAddress} has {tb.Balance} tokens on contract {tb.TokenContract.Address}");
					}

					DataReceived.Invoke(message);
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

			StartCoroutine(_tezos.API.IsHolderOfContract(result =>
			{
				if (result.Success)
				{
					var flag = result.Data;

					var message = flag
						? $"{walletAddress} is HOLDER of contract {_checkContract}"
						: $"{walletAddress} is NOT HOLDER of contract {_checkContract}";

					DataReceived.Invoke(message);
					Debug.Log(message);
				}
				else
				{
					DataReceived.Invoke(result.ErrorMessage);
					Debug.Log(result.ErrorMessage);
				}
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

			StartCoroutine(_tezos.API.IsHolderOfToken(result =>
			{
				if (result.Success)
				{
					var flag = result.Data;

					var message = flag
						? $"{walletAddress} is HOLDER of token {tokenId} on contract {_checkContract}"
						: $"{walletAddress} is NOT HOLDER of token {tokenId} on contract {_checkContract}";

					DataReceived.Invoke(message);
					Debug.Log(message);
				}
				else
				{
					DataReceived.Invoke(result.ErrorMessage);
					Debug.Log(result.ErrorMessage);
				}
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