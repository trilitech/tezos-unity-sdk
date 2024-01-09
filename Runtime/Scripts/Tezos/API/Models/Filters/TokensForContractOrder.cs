namespace TezosSDK.Tezos.API.Models.Filters
{

	public abstract record TokensForContractOrder
	{
		public record Default(long lastId) : TokensForContractOrder
		{
			public long lastId { get; } = lastId;
		}

		public record ByHoldersCountAsc(long page) : TokensForContractOrder
		{
			public long page { get; } = page;
		}

		public record ByHoldersCountDesc(long page) : TokensForContractOrder
		{
			public long page { get; } = page;
		}

		public record ByLastTimeAsc(long page) : TokensForContractOrder
		{
			public long page { get; } = page;
		}

		public record ByLastTimeDesc(long page) : TokensForContractOrder
		{
			public long page { get; } = page;
		}
	}

}