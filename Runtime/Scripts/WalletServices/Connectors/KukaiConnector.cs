
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using UnityEngine;
using UnityEngine.Networking;

namespace TezosSDK.WalletServices.Connectors
{
    public class KukaiConnector : IWalletConnector
    {
        private readonly WalletEventManager _eventManager;

        public KukaiConnector(WalletEventManager eventManager)
        {
            _eventManager = eventManager;
            ConnectorType = ConnectorType.Kukai;
            
            InitializeDeepLinking();
        }
        
        private void OnDeepLinkActivated(string url)
		{
			// Parse the URL and extract the necessary info
			var parsedData = ParseURL(url);
		}

		private void InitializeDeepLinking()
		{
			Application.deepLinkActivated += OnDeepLinkActivated;
			var initialURL = Application.absoluteURL; // For Android, iOS, or Universal Windows Platform (UWP) this is a deep link URL (Read Only).

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

				if (keyValue.Length != 2)
				{
					continue;
				}

				// Use UnityWebRequest.UnEscapeURL to decode the key and value
				var key = UnityWebRequest.UnEscapeURL(keyValue[0]);
				var value = UnityWebRequest.UnEscapeURL(keyValue[1]);
				queryParameters[key] = value;
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
        
        public void Dispose()
        {
        }

        public ConnectorType ConnectorType { get; }
        
#pragma warning disable CS0067 // Event never used TODO: Fix this
        public event Action<WalletMessageType> OperationRequested;
#pragma warning restore CS0067 // Event never used
	    
        public void ConnectWallet()
        {
            // Implement logic to redirect user to Kukai embed page
            TezosLogger.LogDebug("ConnectWallet");
            string kukaiUrl = "https://embed-ghostnet.kukai.app";
            Application.OpenURL(kukaiUrl);
        }

        public string GetWalletAddress()
        {
            // Implement logic to retrieve the wallet address from the deep link
            TezosLogger.LogDebug("GetWalletAddress");
            return "tz1...";
        }

        public void DisconnectWallet()
        {
            // Implement logic to handle wallet disconnection
            TezosLogger.LogDebug("DisconnectWallet");
        }

        public void RequestOperation(WalletOperationRequest operationRequest)
        {
            string operationPayload = $"{{\"destination\":\"{operationRequest.Destination}\",\"amount\":\"{operationRequest.Amount}\",\"entrypoint\":\"{operationRequest.EntryPoint}\",\"arg\":\"{operationRequest.Arg}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?operation={operationPayload}&redirect=unitydl://main";
            TezosLogger.LogDebug("RequestOperation");
            //Application.OpenURL(kukaiUrl);
        }

        public void RequestSignPayload(WalletSignPayloadRequest signRequest)
        {
            string payload = $"{{\"signType\":\"{signRequest.SigningType}\",\"payload\":\"{signRequest.Payload}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?sign={payload}&redirect=unitydl://main";
            TezosLogger.LogDebug("RequestSignPayload");
            //Application.OpenURL(kukaiUrl);
        }

        public void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
        {
            string originationPayload = $"{{\"script\":\"{originationRequest.Script}\",\"delegate\":\"{originationRequest.DelegateAddress}\"}}";
            string kukaiUrl = $"https://embed-ghostnet.kukai.app?originate={originationPayload}&redirect=unitydl://main";
            TezosLogger.LogDebug("RequestSignPayload");
            //Application.OpenURL(kukaiUrl);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}