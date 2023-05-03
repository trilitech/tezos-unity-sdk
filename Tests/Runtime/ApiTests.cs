using System.Collections;
using System.Linq;
using NUnit.Framework;
using TezosAPI.Models;
using UnityEngine.TestTools;

public class ApiTests
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator GetTokensForOwnerTest()
    {
        var tezos = TezosSingleton.Instance;
        const int expectedItems = 5;

        yield return tezos.GetTokensForOwner(
            callback: tokenBalances => { Assert.AreEqual(expectedItems, tokenBalances.Count()); },
            owner: "KT18p94vjkkHYY3nPmernmgVR7HdZFzE7NAk",
            withMetadata: false,
            maxItems: expectedItems,
            orderBy: new TokensForOwnerOrder.ByLastTimeAsc(0));
    }

    [UnityTest]
    public IEnumerator GetOwnersForTokenTest()
    {
        var tezos = TezosSingleton.Instance;
        const int expectedItems = 5;

        yield return tezos.GetOwnersForToken(
            callback: tokenBalances => { Assert.AreEqual(expectedItems, tokenBalances.Count()); },
            contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY",
            tokenId: 1,
            maxItems: expectedItems,
            orderBy: new OwnersForTokenOrder.Default(0));
    }

    [UnityTest]
    public IEnumerator GetOwnersForContractTest()
    {
        var tezos = TezosSingleton.Instance;
        const int expectedItems = 5;

        yield return tezos.GetOwnersForContract(
            callback: tokenBalances => { Assert.AreEqual(expectedItems, tokenBalances.Count()); },
            contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY",
            maxItems: expectedItems,
            orderBy: new OwnersForContractOrder.Default(0));
    }

    [UnityTest]
    public IEnumerator IsHolderOfContractTest()
    {
        var tezos = TezosSingleton.Instance;

        yield return tezos.IsHolderOfContract(
            callback: isHolder => { Assert.AreEqual(true, isHolder); },
            wallet: "tz1TiZ74DtsT74VyWfbAuSis5KcncH1WvNB9",
            contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY");
    }

    [UnityTest]
    public IEnumerator IsHolderOfTokenTest()
    {
        var tezos = TezosSingleton.Instance;

        yield return tezos.IsHolderOfToken(
            callback: isHolder => { Assert.AreEqual(true, isHolder); },
            wallet: "tz1TiZ74DtsT74VyWfbAuSis5KcncH1WvNB9",
            contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY",
            tokenId: 1);
    }

    [UnityTest]
    public IEnumerator GetTokenMetadataTest()
    {
        var tezos = TezosSingleton.Instance;

        yield return tezos.GetTokenMetadata(
            callback: metadata => { Assert.IsFalse(string.IsNullOrEmpty(metadata.ToString())); },
            contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY",
            tokenId: 1);
    }

    [UnityTest]
    public IEnumerator GetContractMetadataTest()
    {
        var tezos = TezosSingleton.Instance;

        yield return tezos.GetContractMetadata(
            callback: metadata => { Assert.IsFalse(string.IsNullOrEmpty(metadata.ToString())); },
            contractAddress: "KT1BRADdqGk2eLmMqvyWzqVmPQ1RCBCbW5dY");
    }

    [UnityTest]
    public IEnumerator GetTokensForContractTest()
    {
        var tezos = TezosSingleton.Instance;
        const int expectedItems = 5;

        yield return tezos.GetTokensForContract(
            callback: tokens => { Assert.AreEqual(expectedItems, tokens.Count()); },
            contractAddress: "KT1RJ6PbjHpwc3M5rw5s2Nbmefwbuwbdxton",
            withMetadata: true,
            maxItems: expectedItems,
            orderBy: new TokensForContractOrder.Default(0));
    }
}