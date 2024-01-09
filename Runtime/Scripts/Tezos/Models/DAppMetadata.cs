using System;

namespace TezosSDK.Tezos
{

	[Serializable]
	public class DAppMetadata
	{
		public DAppMetadata(string name, string url, string icon, string description)
		{
			Name = name;
			Url = url;
			Icon = icon;
			Description = description;
		}

		public string Description { get; private set; }
		public string Icon { get; private set; }
		public string Name { get; private set; }
		public string Url { get; private set; }
	}

}