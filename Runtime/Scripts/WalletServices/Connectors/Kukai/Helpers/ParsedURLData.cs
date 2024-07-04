using System.Collections.Generic;

namespace TezosSDK.WalletServices.Connectors.Kukai.Helpers
{

	/// <summary>
	///     Class representing parsed URL data.
	/// </summary>
	public class ParsedURLData
	{
		private readonly Dictionary<string, string> _parameters;

		/// <summary>
		///     Initializes a new instance of the ParsedURLData class.
		/// </summary>
		/// <param name="parameters">The query parameters.</param>
		public ParsedURLData(Dictionary<string, string> parameters)
		{
			_parameters = parameters;
		}

		/// <summary>
		///     Gets the value of a query parameter by key.
		/// </summary>
		/// <param name="key">The key of the query parameter.</param>
		/// <returns>The value of the query parameter, or an empty string if the key does not exist.</returns>
		public string GetParameter(string key)
		{
			return _parameters.TryGetValue(key, out var value) ? value : string.Empty;
		}
	}

}