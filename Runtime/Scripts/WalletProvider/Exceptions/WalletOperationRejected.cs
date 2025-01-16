using System;

namespace Tezos.WalletProvider
{
	public class WalletOperationRejected : Exception
	{
		public WalletOperationRejected() { }

		public WalletOperationRejected(string message) : base(message) { }

		public WalletOperationRejected(string message, Exception inner) : base(message, inner) { }
	}
}