using System;
using System.Collections.Generic;
using TezosSDK.Patterns;
using UnityEngine;
using UnityEngine.Networking;

namespace TezosSDK.Tezos
{

	public class DeepLinkManager : SingletonMonoBehaviour<DeepLinkManager>
	{
		public event Action<string> OnDeepLinkReceived;

		protected override void Awake()
		{
			base.Awake();
			InitializeDeepLinking();
		}

		private void OnDestroy()
		{
			Application.deepLinkActivated -= OnDeepLinkActivated;
		}

		private void OnDeepLinkActivated(string url)
		{
			// Parse the URL and extract the necessary info
			var parsedData = ParseURL(url);

			// Notify other parts of the SDK
			OnDeepLinkReceived?.Invoke(parsedData);
		}

		private void InitializeDeepLinking()
		{
			Application.deepLinkActivated += OnDeepLinkActivated;
			var initialURL = Application.absoluteURL;

			if (!string.IsNullOrEmpty(initialURL))
			{
				OnDeepLinkActivated(initialURL);
			}
		}

		private Dictionary<string, string> Parse(string queryString)
		{
			var queryParameters = new Dictionary<string, string>();

			// Remove the '?' character at the beginning of the query string if it exists
			queryString = queryString.TrimStart('?');

			// Split the query string into individual key-value pairs
			var pairs = queryString.Split('&');

			foreach (var pair in pairs)
			{
				var keyValue = pair.Split('=');

				if (keyValue.Length == 2)
				{
					// Use UnityWebRequest.UnEscapeURL to decode the key and value
					var key = UnityWebRequest.UnEscapeURL(keyValue[0]);
					var value = UnityWebRequest.UnEscapeURL(keyValue[1]);
					queryParameters[key] = value;
				}
			}

			return queryParameters;
		}

		private string ParseURL(string url)
		{
			// TODO: Process deep link data for authentication - (What are the fields we need to extract?)

			/*
			 From google docs:
				This web-client will send a deep link back to the unity application.
				The deep link contains information about the user (address, public key, social provider) and
				it is of this form: unitydl://main/?address=<tezos_address>&provider=<social_provider>
			 */

			try
			{
				// Create a Uri instance from the URL string to safely parse it
				var uri = new Uri(url);

				// Parse the query string
				var queryDict = Parse(uri.Query);

				var hasAddress = queryDict.TryGetValue("address", out var addess) ? addess : string.Empty;
				var hasProvider = queryDict.TryGetValue("provider", out var provider) ? provider : string.Empty;

				return string.Empty; // TODO: Return the parsed data or the dictionary
			}
			catch (Exception e)
			{
				Debug.LogError($"Error parsing URL: {url}. Error: {e.Message}");
				return string.Empty;
			}
		}
	}

}