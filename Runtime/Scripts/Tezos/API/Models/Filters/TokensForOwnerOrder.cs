namespace TezosSDK.Tezos.API.Models.Filters
{

	public abstract record TokensForOwnerOrder
	{
		public record Default(long lastId) : TokensForOwnerOrder
		{
			public long lastId { get; } = lastId;
		}

		public record ByLastTimeAsc(long page) : TokensForOwnerOrder
		{
			public long page { get; } = page;
		}

		public record ByLastTimeDesc(long page) : TokensForOwnerOrder
		{
			public long page { get; } = page;
		}
	}

}