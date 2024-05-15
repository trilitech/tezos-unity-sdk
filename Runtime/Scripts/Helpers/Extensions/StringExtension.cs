namespace TezosSDK.Helpers.Extensions
{

	public static class StringExtension
	{
		public static byte[] ToByteArray(this string input)
		{
			var bytes = new byte[input.Length];

			for (var i = 0; i < input.Length; i++)
			{
				bytes[i] = (byte)input[i];
			}

			return bytes;
		}

		public static string FirstCharToLowerCase(this string input)
		{
			if (!string.IsNullOrEmpty(input) && char.IsUpper(input[0]))
			{
				return input.Length == 1 ? char.ToLower(input[0]).ToString() : char.ToLower(input[0]) + input[1..];
			}

			return input;
		}
	}

}