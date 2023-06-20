using System.Threading.Tasks;

namespace TezosSDK.Tezos.API.Models.Abstract
{
    public interface IFA2
    {
        public string Address { get; }

        Task<string> Mint();

        Task<string> Transfer();
    }
}