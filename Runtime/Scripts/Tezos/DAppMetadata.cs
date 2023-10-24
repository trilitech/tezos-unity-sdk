namespace TezosSDK.Tezos
{
    public class DAppMetadata
    {
        /// <summary>
        /// SDK consumer DApp name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// SDK consumer App Url address, starts with https://.
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// SDK consumer Icon Url address, starts with https://.
        /// </summary>
        public string Icon { get; set; }
    }
}