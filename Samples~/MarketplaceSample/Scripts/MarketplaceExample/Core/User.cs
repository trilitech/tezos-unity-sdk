namespace TezosSDK.MarketplaceSample.MarketplaceExample.Core
{

	public class User : IUserModel
	{
		public User(string name, string id, string address)
		{
			Name = name;
			Identifier = id;
			Address = address;
		}

		public string Address { get; private set; }

		public string Name { get; }
		public string Identifier { get; }

		public void UpdateAddress(string address)
		{
			Address = address;
		}
	}

}