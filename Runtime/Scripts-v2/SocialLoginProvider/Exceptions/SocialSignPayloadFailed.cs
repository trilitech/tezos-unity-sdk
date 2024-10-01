using System;

namespace Tezos.SocialLoginProvider
{
	public class SocialSignPayloadFailed : Exception
	{
		public SocialSignPayloadFailed() { }

		public SocialSignPayloadFailed(string message) : base(message) { }

		public SocialSignPayloadFailed(string message, Exception inner) : base(message, inner) { }
	}
}