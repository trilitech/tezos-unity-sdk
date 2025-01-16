using System;

namespace Tezos.WalletProvider
{
	public class WalletConnectionRejected : Exception
	{
		public WalletConnectionRejected() { }

		public WalletConnectionRejected(string message) : base(message) { }

		public WalletConnectionRejected(string message, Exception inner) : base(message, inner) { }
	}
}