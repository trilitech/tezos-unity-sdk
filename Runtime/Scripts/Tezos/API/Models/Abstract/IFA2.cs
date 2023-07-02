using System;
using TezosSDK.Tezos.API.Models.Tokens;

namespace TezosSDK.Tezos.API.Models.Abstract
{
    public interface IFA2
    {
        public string Address { get; }


        void Mint(
            // todo: Action<TokenBalance> ?
            Action<string> callback,
            TokenMetadata tokenMetadata,
            string destination,
            int amount);

        void Transfer(Action<string> transactionHash);

        void Deploy(Action<string> contractAddress);
    }
}