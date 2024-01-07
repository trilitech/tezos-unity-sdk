// using System.Collections;
// using System.Linq;
// using System.Threading.Tasks;
// using Beacon.Sdk.Beacon.Permission;
// using TezosSDK.Tezos;
// using TezosSDK.Tezos.API;
// using TezosSDK.Tezos.API.Models;
// using TezosSDK.Tezos.API.Models.Filters;
// using UnityEngine;
// using UnityEngine.TestTools;
//
// namespace Tests.Runtime
// {
//     public class TokenContractTests
//     {
//         [UnityTest]
//         public IEnumerator DeployContractTest()
//         {
//             TezosConfig.Instance.Network = NetworkType.ghostnet;
//             var tokenContract = new TokenContract();
//             var deployTask = tokenContract.Deploy();
//             yield return new WaitUntil(() => deployTask.IsCompleted);
//             var result = deployTask.Result;
//         }
//     }
// }

