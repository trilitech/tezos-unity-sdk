namespace TezosSDK.Tests.Runtime
{

	public class ApiTests
	{
		//     private static IDataProviderConfig GetDataProviderConfig()
		//     {
		//         return new TzKTProviderConfig();
		//     }
		//
		//     // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
		//     // `yield return null;` to skip a frame.
		//     [UnityTest]
		//     public IEnumerator GetTokensForOwnerTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//         const int expectedItems = 5;
		//
		//         yield return api.GetTokensForOwner(
		//             callback: tokenBalances => { Assert.AreEqual(expectedItems, tokenBalances.Count()); },
		//             owner: "KT18p94vjkkHYY3nPmernmgVR7HdZFzE7NAk",
		//             withMetadata: false,
		//             maxItems: expectedItems,
		//             orderBy: new TokensForOwnerOrder.ByLastTimeAsc(0));
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator GetOwnersForTokenTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//         const int expectedItems = 5;
		//
		//         yield return api.GetOwnersForToken(
		//             callback: tokenBalances => { Assert.AreEqual(expectedItems, tokenBalances.Count()); },
		//             contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY",
		//             tokenId: 1,
		//             maxItems: expectedItems,
		//             orderBy: new OwnersForTokenOrder.Default(0));
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator GetOwnersForContractTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//         const int expectedItems = 5;
		//
		//         yield return api.GetOwnersForContract(
		//             callback: tokenBalances => { Assert.AreEqual(expectedItems, tokenBalances.Count()); },
		//             contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY",
		//             maxItems: expectedItems,
		//             orderBy: new OwnersForContractOrder.Default(0));
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator IsHolderOfContractTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//
		//         yield return api.IsHolderOfContract(
		//             callback: isHolder => { Assert.AreEqual(true, isHolder); },
		//             wallet: "tz1TiZ74DtsT74VyWfbAuSis5KcncH1WvNB9",
		//             contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY");
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator IsHolderOfTokenTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//
		//         yield return api.IsHolderOfToken(
		//             callback: isHolder => { Assert.AreEqual(true, isHolder); },
		//             wallet: "tz1TiZ74DtsT74VyWfbAuSis5KcncH1WvNB9",
		//             contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY",
		//             tokenId: 1);
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator GetTokenMetadataTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//
		//         yield return api.GetTokenMetadata(
		//             callback: metadata => { Assert.IsFalse(string.IsNullOrEmpty(metadata.ToString())); },
		//             contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY",
		//             tokenId: 1);
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator GetContractMetadataTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//
		//         yield return api.GetContractMetadata(
		//             callback: metadata => { Assert.IsFalse(string.IsNullOrEmpty(metadata.ToString())); },
		//             contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY");
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator GetTokensForContractTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//         const int expectedItems = 5;
		//
		//         yield return api.GetTokensForContract(
		//             callback: tokens => { Assert.AreEqual(expectedItems, tokens.Count()); },
		//             contractAddress: "KT1RJ6PbjHpwc3M5rw5s2Nbmefwbuwbdxton",
		//             withMetadata: true,
		//             maxItems: expectedItems,
		//             orderBy: new TokensForContractOrder.Default(0));
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator GetOperationStatusTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.ghostnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//         const bool expectedResult = true;
		//
		//         yield return api.GetOperationStatus(status => { Assert.AreEqual(expectedResult, status); },
		//             "oo4gj5tfvnE1LKsRp6BSm7VB5LAoqzogJXPwGWSYBjmUgNsmk8M");
		//     }
		//
		//     [UnityTest]
		//     public IEnumerator GetLatestBlockLevelTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.mainnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//
		//         yield return api.GetLatestBlockLevel(latestBlockLevel => { Assert.IsTrue(latestBlockLevel > 0); });
		//     }
		//     
		//     [UnityTest]
		//     public IEnumerator GetOriginatedContractsForOwnerTest()
		//     {
		//         TezosConfig.Instance.Network = NetworkType.ghostnet;
		//         var api = new TezosAPI(GetDataProviderConfig());
		//
		//         var codeHash = Resources.Load<TextAsset>("Contracts/FA2TokenContractCodeHash")
		//             .text;
		//
		//         yield return api.GetOriginatedContractsForOwner(
		//             callback: originatedContracts =>
		//             {
		//                 Assert.IsTrue(originatedContracts.Any());
		//             },
		//             creator: "tz1Xqbt2AZcSqRXxBZp6fBnnm1eEm9JxJPRB",
		//             codeHash: codeHash,
		//             maxItems: 1000,
		//             orderBy: new OriginatedContractsForOwnerOrder.ByLastActivityTimeDesc(0));
		//     }
	}

}