using System;

namespace Tezos.WalletProvider
{
	public class WalletSignPayloadRejected : Exception
	{
		public WalletSignPayloadRejected() { }

		public WalletSignPayloadRejected(string message) : base(message) { }

		public WalletSignPayloadRejected(string message, Exception inner) : base(message, inner) { }
	}
}