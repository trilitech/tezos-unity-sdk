namespace TezosSDK.Tezos.API.Models.Filters
{

	public abstract record OwnersForTokenOrder
	{
		public record Default(long lastId) : OwnersForTokenOrder
		{
			public long lastId { get; } = lastId;
		}

		public record ByBalanceAsc(long page) : OwnersForTokenOrder
		{
			public long page { get; } = page;
		}

		public record ByBalanceDesc(long page) : OwnersForTokenOrder
		{
			public long page { get; } = page;
		}

		public record ByLastTimeAsc(long page) : OwnersForTokenOrder
		{
			public long page { get; } = page;
		}

		public record ByLastTimeDesc(long page) : OwnersForTokenOrder
		{
			public long page { get; } = page;
		}
	}

}