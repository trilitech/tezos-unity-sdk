using System;

namespace TezosSDK.Tezos.API.Models.Abstract
{
    public interface IFA2
    {
        public string Address { get; }

        void Mint();

        void Transfer(Action<string> transactionHash);

        void Deploy(Action<string> contractAddress);
    }
}