namespace TezosSDK.Tezos.API.Models.Filters
{

	public abstract record OwnersForContractOrder
	{
		public record Default(long lastId) : OwnersForContractOrder
		{
			public long lastId { get; } = lastId;
		}

		public record ByLastTimeAsc(long page) : OwnersForContractOrder
		{
			public long page { get; } = page;
		}

		public record ByLastTimeDesc(long page) : OwnersForContractOrder
		{
			public long page { get; } = page;
		}
	}

}