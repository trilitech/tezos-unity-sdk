using Beacon.Sdk.Beacon.Permission;

namespace TezosSDK.Tezos
{
    public class TezosConfig
    {
        private static TezosConfig _instance;

        // Singleton instance
        public static TezosConfig Instance => _instance ??= new TezosConfig();
        public NetworkType Network { get; set; } = NetworkType.ghostnet;
        public string DefaultDAppName { get; set; } = "Tezos Unity SDK";
        public string DefaultAppUrl { get; set; } = "https://tezos.com/unity";

        public string DefaultIconUrl { get; set; } =
            "https://unity.com/sites/default/files/2022-09/unity-tab-small.png";

        public string RpcBaseUrl => $"https://{Network}.tezos.marigold.dev";
        public int DefaultTimeoutSeconds => 45;
    }

    public interface IDataProviderConfig
    {
        int TimeoutSeconds { get; }
        string BaseUrl { get; }
    }

    public class TzKTProviderConfig : IDataProviderConfig
    {
        private int? _timeoutSeconds;

        public int TimeoutSeconds
        {
            get => _timeoutSeconds ??= TezosConfig.Instance.DefaultTimeoutSeconds;
            set => _timeoutSeconds = value;
        }

        public string BaseUrl
        {
            get
            {
                var networkPart = TezosConfig.Instance.Network != NetworkType.mainnet
                    ? $"{TezosConfig.Instance.Network}."
                    : "";
                return $"https://api.{networkPart}tzkt.io/v1/";
            }
        }
    }
}