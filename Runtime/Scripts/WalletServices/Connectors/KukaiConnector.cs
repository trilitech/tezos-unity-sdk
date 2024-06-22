
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beacon.Sdk.Beacon;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using TezosSDK.WalletServices.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace TezosSDK.WalletServices.Connectors
{
    public class KukaiConnector : IWalletConnector
    {
	    private WalletInfo _activeWallet; // Keep track of the active wallet

	    private class ParsedURLData
	    {
		    private readonly Dictionary<string, string> _parameters;

		    public ParsedURLData(Dictionary<string, string> parameters)
		    {
			    _parameters = parameters;
		    }

		    public string GetParameter(string key)
		    {
			    return _parameters.TryGetValue(key, out var value) ? value : string.Empty;
		    }
	    }

	    private readonly EventDispatcher _eventDispatcher;

        public KukaiConnector(WalletEventManager eventManager)
        {
	        _eventDispatcher = new EventDispatcher(eventManager);
            ConnectorType = ConnectorType.Kukai;
            
            InitializeDeepLinking();
        }
        
        private void OnDeepLinkActivated(string url)
		{
			// Parse the URL and extract the necessary info
			var parsedData = ParseURL(url);
			
			if (parsedData == null)
			{
				TezosLogger.LogError($"Error parsing URL: {url}");
				return;
			}
			
			if (parsedData.GetParameter("type") == "login")
			{
				// Handle the login request
				TezosLogger.LogDebug("Login");
				
				var wallet = new WalletInfo
				{
					Address = parsedData.GetParameter("address"),
					PublicKey = parsedData.GetParameter("public_key")
				};
	
				_activeWallet = wallet;
				_eventDispatcher.DispatchWalletConnectedEvent(wallet);
			}
			else if (parsedData.GetParameter("type") == "operation_response")
			{
				// Handle the operation request
				TezosLogger.LogDebug("Operation");

				var operation = new OperationInfo(parsedData.GetParameter("operation_hash"), 
					parsedData.GetParameter("operation_id"), 
					BeaconMessageType.operation_response);
				
				_eventDispatcher.DispatchOperationInjectedEvent(operation);
			}
			else if (parsedData.GetParameter("type") == "sign")
			{
				// Handle the sign request
				TezosLogger.LogDebug("Sign");
			}
			else if (parsedData.GetParameter("type") == "originate")
			{
				// Handle the contract origination request
				TezosLogger.LogDebug("Contract origination");
			}
			else
			{
				TezosLogger.LogWarning($"Unknown request type: {parsedData.GetParameter("type")}");
			}
			
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

			if (string.IsNullOrEmpty(queryString))
			{
				return queryParameters;
			}

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

					// Check for duplicate keys
					if (queryParameters.ContainsKey(key))
					{
						TezosLogger.LogWarning($"Duplicate key found: {key}. Overwriting the existing value.");
					}

					queryParameters[key] = value;
				}
				else if (keyValue.Length == 1)
				{
					// Handle cases where there is a key without a value
					var key = UnityWebRequest.UnEscapeURL(keyValue[0]);
					if (!queryParameters.ContainsKey(key))
					{
						queryParameters[key] = string.Empty;
					}
				}
			}

			return queryParameters;
		}

		private ParsedURLData ParseURL(string url)
		{
			try
			{
				// Create a Uri instance from the URL string to safely parse it
				var uri = new Uri(url);

				// Parse the query string
				var queryDict = Parse(uri.Query);

				return new ParsedURLData(queryDict);
			}
			catch (Exception e)
			{
				TezosLogger.LogError($"Error parsing URL: {url}. Error: {e.Message}");
				return null;
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
            // string kukaiUrl = "https://embed-ghostnet.kukai.app";
            string kukaiUrl = "http://192.168.0.74:3000";
            Application.OpenURL(kukaiUrl);
        }

        public string GetWalletAddress()
        {
            return _activeWallet?.Address;
        }

        public void DisconnectWallet()
        {
	        var wallet = _activeWallet;
	        _activeWallet = null;
			_eventDispatcher.DispatchWalletDisconnectedEvent(wallet);
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