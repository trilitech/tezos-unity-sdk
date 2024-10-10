using System;

namespace Tezos.API
{
	public class AlreadyConnectedException : Exception
	{
		public AlreadyConnectedException() { }

		public AlreadyConnectedException(string message) : base(message) { }

		public AlreadyConnectedException(string message, Exception inner) : base(message, inner) { }
	}
}