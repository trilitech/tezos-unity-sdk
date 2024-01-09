namespace TezosSDK.MarketplaceSample.MarketplaceExample.Core
{

	public class Item : IItemModel
	{
		public Item(
			ItemType type,
			string resourcePath,
			string name,
			StatParams parameters,
			float price,
			int id,
			string owner)
		{
			Type = type;
			ResourcePath = resourcePath;
			Name = name;
			Stats = parameters;
			Price = price;
			ID = id;
			Owner = owner;
		}

		public ItemType Type { get; }
		public string ResourcePath { get; }
		public string Name { get; }
		public StatParams Stats { get; }

		public float Price { get; }
		public int ID { get; }
		public string Owner { get; }
	}

}