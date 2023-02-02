using System.Collections;
using System.Collections.Generic;
using Netezos.Rpc.Queries.Post;
using UnityEngine;

namespace Netezos.Rpc.Queries
{
    public static class ScriptsQueryExtensions
    {
        public static RunViewQuery RunView(this ScriptsQuery query) => new RunViewQuery(query, "run_script_view/");
    }
}

namespace Netezos.Rpc.Queries.Post
{
    public class RunViewQuery : RpcMethod
    {
        //private const string ChainID = "NetXdQprcVkpaWU"; // mainnet
        //private const string ChainID = "NetXLH1uAxK7CCh"; // jakarta (no Views support on kztk)
        private const string ChainID = "NetXnHfVqm9iesp"; // ghostnet

        public enum UnparsingMode
        {
            Readable,
            Optimized,
            Optimized_legacy
        }
        
        internal RunViewQuery(RpcQuery baseQuery, string append) : base(baseQuery, append) { }
        
        /// <summary>
        /// Runs a piece of code in the current context and returns the storage, operations and big_map data
        /// </summary>
        /// <param name="contract">Contract handler</param>
        /// <param name="view">View entry point of the contract</param>
        /// <param name="input">Input(micheline michelson expression)</param>
        /// <param name="chain_id">Base58-check encoded network identifier</param>
        /// <param name="unparsing_mode">Gas limit (optional)</param>
        /// <param name="source">Source (optional)</param>
        /// <param name="payer">Payer (optional)</param>
        /// <param name="gas">Gas limit (optional)</param>
        /// <param name="now">Gas limit (optional)</param>
        /// <param name="level">Gas limit (optional)</param>
        /// <returns></returns>
        public IEnumerator PostAsync<T>(string contract, string view, object input, UnparsingMode unparsing_mode = UnparsingMode.Readable, string source = null, string payer = null, long? gas = null)
            => PostAsync<T>(new
            {
                contract,
                view,
                input,
                chain_id = ChainID,
                source,
                payer,
                gas = gas?.ToString(),
                unparsing_mode = unparsing_mode.ToString()
                //"now": "string",
                //"level": "string"
            });
    }
}
