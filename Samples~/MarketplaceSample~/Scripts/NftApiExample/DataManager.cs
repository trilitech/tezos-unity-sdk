using System;
using System.Linq;
using Tezos.API;
using Tezos.WalletProvider;
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

		private void Start()
		{
			TezosAPI.WalletConnected += OnAccountConnected;
		}

		private void OnAccountConnected(WalletProviderData walletProviderData)
		{
			_connectedAddress = walletProviderData.WalletAddress;
		}

		public async void GetTokensForOwners()
		{
			var walletAddress = string.IsNullOrEmpty(_checkAddress) ? _connectedAddress : _checkAddress;
			
			var res = await TezosAPI.GetTokensForOwner(walletAddress, false, MaxTokens, new TokensForOwnerOrder.Default(0));

			var tokens = res.ToList();

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
		}

		public async void IsHolderOfContract()
		{
			var walletAddress = string.IsNullOrEmpty(_checkAddress) ? _connectedAddress : _checkAddress;

			if (string.IsNullOrEmpty(_checkContract))
			{
				DataReceived.Invoke("Enter contract address");
				Debug.Log("Enter contract address");
				return;
			}
			
			var res = await TezosAPI.IsHolderOfContract(walletAddress, _checkContract);

			DataReceived.Invoke(res ? $"{walletAddress} is HOLDER of contract {_checkContract}" : $"{walletAddress} is NOT HOLDER of contract {_checkContract}");
		}

		public async void IsHolderOfToken()
		{
			var walletAddress = string.IsNullOrEmpty(_checkAddress) ? _connectedAddress : _checkAddress;

			var tokenId = string.IsNullOrEmpty(_checkTokenId) ? 0 : Convert.ToUInt32(_checkTokenId);

			if (string.IsNullOrEmpty(_checkContract))
			{
				DataReceived.Invoke("Enter contract address");
				Debug.Log("Enter contract address");
				return;
			}
			
			var res = await TezosAPI.IsHolderOfToken(walletAddress, _checkContract, tokenId);
			
			DataReceived.Invoke(res ? $"{walletAddress} is HOLDER of token {tokenId} on contract {_checkContract}" : $"{walletAddress} is NOT HOLDER of token {tokenId} on contract {_checkContract}");
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