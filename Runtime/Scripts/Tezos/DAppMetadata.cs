using System;
using UnityEngine;

namespace TezosSDK.Tezos
{
    [Serializable]
    public class DAppMetadata
    {
        public string Name { get; private set; }
        public string Url { get; private set; }
        public string Icon { get; private set; }
        public string Description { get; private set; }

        public DAppMetadata(string name, string url, string icon, string description)
        {
            Name = name;
            Url = url;
            Icon = icon;
            Description = description;
        }
    }

}