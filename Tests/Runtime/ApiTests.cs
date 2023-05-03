using NUnit.Framework;
using TezosAPI.Models;
using UnityEngine;

public class ApiTests
{
    [Test]
    public void GetTokensForOwnerTest()
    {
        var _tezos = TezosSingleton.Instance;

        _tezos.GetTokensForOwner(tokens =>
            {
                foreach (var token in tokens)
                {
                    Debug.Log(token);
                }
            },
            "KT18p94vjkkHYY3nPmernmgVR7HdZFzE7NAk", false, 2, new TokensForOwnerOrder.Default(0));

        Assert.AreEqual(1, 1);
    }

    // // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // // `yield return null;` to skip a frame.
    // [UnityTest]
    // public IEnumerator ApiTestsWithEnumeratorPasses()
    // {
    //     // Use the Assert class to test conditions.
    //     // Use yield to skip a frame.
    //     yield return null;
    // }
}