using Beacon.Sdk.Beacon.Permission;

namespace Scripts.Tezos
{
    public class TezosConfig
    {
        private static TezosConfig _instance;

        // Singleton instance
        public static TezosConfig Instance => _instance ??= new TezosConfig();

        public NetworkType Network { get; set; } = NetworkType.ghostnet;

        public string RpcBaseUrl => $"https://rpc.{Network}.teztnets.xyz";

        public string TzKTBaseUrl =>
            $"https://api.{(Network != NetworkType.mainnet ? $"{Network}." : "")}tzkt.io/v1/";
    }
}