namespace TezosSDK.Tezos
{
    public class DAppMetadata
    {
        /// <summary>
        /// SDK consumer DApp name.
        /// </summary>
        public string AppName { get; set; }
        
        /// <summary>
        /// SDK consumer App Url address, starts with https://.
        /// </summary>
        public string AppUrl { get; set; }
        
        /// <summary>
        /// SDK consumer Icon Url address, starts with https://.
        /// </summary>
        public string IconUrl { get; set; }
    }
}