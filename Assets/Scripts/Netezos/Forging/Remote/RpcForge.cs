using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Netezos.Encoding;
using Netezos.Forging.Models;
using Netezos.Rpc;

namespace Netezos.Forging
{
    public class RpcForge : IForge
    {
        readonly TezosRpc Rpc;
        
        public RpcForge(TezosRpc rpc) => Rpc = rpc;

        public IEnumerator ForgeOperationAsync(OperationContent content)
            => ForgeAsync(new List<object> { content });

        public IEnumerator ForgeOperationAsync(string branch, OperationContent content)
            => ForgeAsync(branch, new List<object> { content });

        public IEnumerator ForgeOperationGroupAsync(IEnumerable<ManagerOperationContent> contents)
            => ForgeAsync(contents.Cast<object>().ToList());

		/// <param name="branch"></param>
		/// <param name="contents"></param>
		/// <returns>Returns byte[]"/></returns>
        public IEnumerator ForgeOperationGroupAsync(string branch, IEnumerable<ManagerOperationContent> contents)
            => ForgeAsync(branch, contents.Cast<object>().ToList());

        private IEnumerator ForgeAsync(List<object> contents)
        {
            CoroutineWrapper<string> coroutineWrapper = new CoroutineWrapper<string>(Rpc.Blocks.Head.Hash.GetAsync<string>());
            yield return coroutineWrapper;
            string branch = coroutineWrapper.Result;
            // TODO: Convert to yield 
            yield return ForgeAsync(branch, contents);
        }

        private IEnumerator ForgeAsync(string branch, List<object> contents)
        {
            CoroutineWrapper<string> coroutineWrapper = new CoroutineWrapper<string>(Rpc.Blocks.Head.Helpers.Forge.Operations.PostAsync<string>(branch, contents));
            yield return coroutineWrapper;
            string result = coroutineWrapper.Result;

            yield return Hex.Parse(result);
        }
    }
}
