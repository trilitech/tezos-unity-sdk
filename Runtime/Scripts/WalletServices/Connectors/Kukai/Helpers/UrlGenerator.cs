using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers.Logging;
using TezosSDK.WalletServices.Connectors.Enums;
using TezosSDK.WalletServices.Data;

namespace TezosSDK.WalletServices.Connectors.Kukai.Helpers
{

	/// <summary>
	///     Generates URLs for login and wallet operations to be opened in system browsers to be sent to Kukai Embed.
	/// </summary>
	public class UrlGenerator
	{
		private readonly string _deepLinkUrl;

		public UrlGenerator(string deepLinkUrl)
		{
			_deepLinkUrl = deepLinkUrl;
		}

		/// <summary>
		///     Generates a login URL.
		/// </summary>
		/// <param name="nonce">The nonce for the login request.</param>
		/// <param name="projectId">The project ID associated with the login request.</param>
		/// <returns>A login URL to be sent to Kukai Embed.</returns>
		public string GenerateLoginLink(string nonce, string projectId)
		{
			return BuildUrl(ActionTypes.LOGIN, new Dictionary<string, string>
			{
				{
					"nonce", nonce
				},
				{
					"projectId", projectId
				}
			});
		}

		/// <summary>
		///     Builds a URL with the specified action and query parameters.
		/// </summary>
		/// <param name="action">The action to be included in the URL.</param>
		/// <param name="queryParams">The query parameters to be included in the URL.</param>
		/// <returns>A constructed URL string.</returns>
		private string BuildUrl(string action, Dictionary<string, string> queryParams)
		{
			var uriBuilder = new UriBuilder(_deepLinkUrl);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["action"] = action;

			foreach (var param in queryParams)
			{
				query[param.Key] = param.Value;
			}

			uriBuilder.Query = query.ToString();
			return uriBuilder.ToString();
		}

		/// <summary>
		///     Generates an operation URL.
		/// </summary>
		/// <param name="req">The wallet operation request.</param>
		/// <param name="walletAddress">The wallet address.</param>
		/// <param name="typeOfLogin">The type of login.</param>
		/// <returns>An operation URL to be sent to Kukai Embed.</returns>
		public string GenerateOperationLink(WalletOperationRequest req, string walletAddress, TypeOfLogin typeOfLogin)
		{
			var serializedRequest = SerializeWalletOperationRequest(req, walletAddress);
			TezosLogger.LogDebug($"JSON Payload: {serializedRequest}");

			var url = BuildUrl(ActionTypes.OPERATION, new Dictionary<string, string>
			{
				{
					"typeOfLogin", typeOfLogin.ToString().ToLower()
				},
				{
					"operation", Uri.EscapeDataString(serializedRequest)
				}
			});

			TezosLogger.LogDebug($"Generated URL: {url}");
			return url;
		}
		
		/// <summary>
		///     Serializes the wallet operation request to JSON format.
		/// </summary>
		/// <param name="request">The wallet operation request.</param>
		/// <param name="walletAddress">The wallet address.</param>
		/// <returns>A JSON string representing the wallet operation request.</returns>
		private string SerializeWalletOperationRequest(WalletOperationRequest request, string walletAddress)
		{
			var jsonObject = new JObject
			{
				["kind"] = "transaction",
				["source"] = walletAddress,
				["amount"] = request.Amount.ToString(),
				["destination"] = request.Destination,
				["parameters"] = new JObject
				{
					["entrypoint"] = request.EntryPoint,
					["value"] = JToken.Parse(request.Arg)
				}
			};

			var jsonArray = new JArray
			{
				jsonObject
			};

			return jsonArray.ToString(Formatting.None);
		}

		public string GenerateSignLink(WalletSignPayloadRequest signRequest, TypeOfLogin typeOfLogin)
		{
			return BuildUrl(ActionTypes.SIGN, new Dictionary<string, string>
			{
				{
					"typeOfLogin", typeOfLogin.ToString().ToLower()
				},
				{
					"expression", Uri.EscapeDataString(signRequest.Payload)
				}
			});
		}
	}

}