using System;

namespace Tezos.SocialLoginProvider
{
	public class SocialLogInFailed : Exception
	{
		public SocialLogInFailed() { }

		public SocialLogInFailed(string message) : base(message) { }

		public SocialLogInFailed(string message, Exception inner) : base(message, inner) { }
	}
}