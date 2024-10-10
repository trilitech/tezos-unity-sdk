using System;

namespace Tezos.API
{
	public class ConnectionRequiredException : Exception
	{
		public ConnectionRequiredException() { }

		public ConnectionRequiredException(string message) : base(message) { }

		public ConnectionRequiredException(string message, Exception inner) : base(message, inner) { }
	}
}