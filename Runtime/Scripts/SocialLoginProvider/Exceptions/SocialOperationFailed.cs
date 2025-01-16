using System;

namespace Tezos.SocialLoginProvider
{
	public class SocialOperationFailed : Exception
	{
		public SocialOperationFailed() { }

		public SocialOperationFailed(string message) : base(message) { }

		public SocialOperationFailed(string message, Exception inner) : base(message, inner) { }
	}
}