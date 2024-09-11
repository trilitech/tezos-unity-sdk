using System;
using System.Collections.Generic;
using System.Linq;
using TezosSDK.Logger;
using UnityEngine.Networking;

namespace TezosSDK.WalletProvider
{

	/// <summary>
	///     Class for parsing URLs and extracting query parameters.
	/// </summary>
	public class UrlParser
	{
		/// <summary>
		///     Parses the query string into a dictionary of key-value pairs.
		/// </summary>
		/// <param name="queryString">The query string to parse.</param>
		/// <returns>A dictionary of query parameters.</returns>
		private Dictionary<string, string> Parse(string queryString)
		{
			var queryParameters = new Dictionary<string, string>();

			// Return an empty dictionary if the query string is null or empty
			if (string.IsNullOrEmpty(queryString))
			{
				return queryParameters;
			}

			queryString = queryString.TrimStart('?');
			var pairs = queryString.Split('&');

			pairs.ToList().ForEach(pair => ProcessKeyValuePair(pair, queryParameters));

			return queryParameters;
		}

		/// <summary>
		///     Processes a single key-value pair and adds it to the dictionary of query parameters.
		/// </summary>
		/// <param name="pair">The key-value pair to process.</param>
		/// <param name="queryParameters">The dictionary to add the key-value pair to.</param>
		private void ProcessKeyValuePair(string pair, Dictionary<string, string> queryParameters)
		{
			var keyValue = pair.Split('=');

			switch (keyValue.Length)
			{
				// If the key-value pair has both key and value
				case 2:
				{
					var key = UnityWebRequest.UnEscapeURL(keyValue[0]);
					var value = UnityWebRequest.UnEscapeURL(keyValue[1]);

					// Log a warning if a duplicate key is found and overwrite the existing value
					if (queryParameters.ContainsKey(key))
					{
						TezosLogger.LogWarning($"Duplicate key found in query string: {key}. Overwriting value: {queryParameters[key]}");
					}

					queryParameters[key] = value;
					break;
				}
				// If the key-value pair has only key
				case 1:
				{
					var key = UnityWebRequest.UnEscapeURL(keyValue[0]);

					// Add the key with an empty value if it doesn't already exist
					if (!queryParameters.ContainsKey(key))
					{
						queryParameters[key] = string.Empty;
					}

					break;
				}
			}
		}

		/// <summary>
		///     Parses the given URL and extracts query parameters into a ParsedURLData object.
		/// </summary>
		/// <param name="url">The URL to parse.</param>
		/// <returns>A ParsedURLData object containing the query parameters.</returns>
		public ParsedURLData ParseURL(string url)
		{
			try
			{
				var uri = new Uri(url);
				var queryDict = Parse(uri.Query);
				return new ParsedURLData(queryDict);
			}
			catch (Exception e)
			{
				TezosLogger.LogError($"Error parsing URL: {url}. Error: {e.Message}");
				return null;
			}
		}
	}

}