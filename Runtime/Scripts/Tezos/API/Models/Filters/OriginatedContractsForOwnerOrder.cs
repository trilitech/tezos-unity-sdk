namespace TezosSDK.Tezos.API.Models.Filters
{

	public abstract record OriginatedContractsForOwnerOrder
	{
		public record Default(long lastId) : OriginatedContractsForOwnerOrder
		{
			public long lastId { get; } = lastId;
		}

		public record ByLastActivityTimeAsc(long page) : OriginatedContractsForOwnerOrder
		{
			public long page { get; } = page;
		}

		public record ByLastActivityTimeDesc(long page) : OriginatedContractsForOwnerOrder
		{
			public long page { get; } = page;
		}
	}

}