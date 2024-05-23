using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Core.Domain.Entities;

namespace TezosSDK.Helpers.Extensions
{

	public static class StringExtensions
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

		public static string PrettyPrint(this Network network)
		{
			var builder = new StringBuilder();
			builder.AppendLine("Network {");
			builder.AppendLine($"\tType = {network.Type},");
			builder.AppendLine($"\tName = {network.Name},");
			builder.AppendLine($"\tRpcUrl = {network.RpcUrl}");
			builder.Append("}");
			return builder.ToString();
		}

		public static string PrettyPrint(this AppMetadata appMetadata)
		{
			var builder = new StringBuilder();
			builder.AppendLine("AppMetadata {");
			builder.AppendLine($"\tId = {appMetadata.Id},");
			builder.AppendLine($"\tSenderId = {appMetadata.SenderId},");
			builder.AppendLine($"\tName = {appMetadata.Name},");
			builder.AppendLine($"\tIcon = {appMetadata.Icon},");
			builder.AppendLine($"\tAppUrl = {appMetadata.AppUrl}");
			builder.Append("}");
			return builder.ToString();
		}

		public static string PrettyPrint(this PermissionResponse response)
		{
			var builder = new StringBuilder();
			builder.AppendLine("PermissionResponse {");
			builder.AppendLine($"\tReceived permissions = \"{string.Join(", ", response.Scopes)}\",");
			builder.AppendLine($"\tFrom = \"{response.AppMetadata.Name}\",");
			builder.AppendLine($"\tPublicKey = \"{response.PublicKey}\"");
			builder.Append("}");
			builder.AppendLine("");
			return builder.ToString();
		}

		public static string PrettyPrint(this PermissionRequest request)
		{
			var builder = new StringBuilder();
			builder.AppendLine("PermissionRequest:");
			builder.AppendLine("{");
			builder.AppendLine($"\tType = {request.Type},");
			builder.AppendLine($"\tVersion = {request.Version},");
			builder.AppendLine($"\tId = {request.Id},");
			builder.AppendLine($"\tSenderId = {request.SenderId},");

			builder.AppendLine("");

			// Add indentation to nested AppMetadata
			var appMetadataLines = request.AppMetadata.PrettyPrint().Split(new[]
			{
				Environment.NewLine
			}, StringSplitOptions.None);

			foreach (var line in appMetadataLines)
			{
				builder.AppendLine($"\t{line}");
			}

			builder.AppendLine("");

			// Add indentation to nested Network
			var networkLines = request.Network.PrettyPrint().Split(new[]
			{
				Environment.NewLine
			}, StringSplitOptions.None);

			foreach (var line in networkLines)
			{
				builder.AppendLine($"\t{line}");
			}

			builder.AppendLine("");
			builder.AppendLine($"\tScopes = {string.Join(", ", request.Scopes)}");
			builder.Append("}");
			builder.AppendLine("");
			return builder.ToString();
		}

		public static string PrettyPrint(this PermissionInfo info)
		{
			var builder = new StringBuilder();
			builder.AppendLine("PermissionInfo {");
			builder.AppendLine($"\tAddress = {info.Address},");
			builder.AppendLine($"\tPublicKey = {info.PublicKey},");
			builder.AppendLine($"\tConnectedAt = {info.ConnectedAt},");

			// Add indentation to nested Network
			var networkLines = info.Network.PrettyPrint().Split(new[]
			{
				Environment.NewLine
			}, StringSplitOptions.None);

			foreach (var line in networkLines)
			{
				builder.AppendLine($"\t{line}");
			}

			builder.AppendLine($"\tScopes = {info.Scopes.Print()},");

			// Add indentation to nested AppMetadata
			var appMetadataLines = info.AppMetadata.PrettyPrint().Split(new[]
			{
				Environment.NewLine
			}, StringSplitOptions.None);

			foreach (var line in appMetadataLines)
			{
				builder.AppendLine($"\t{line}");
			}

			builder.Append("}");
			builder.AppendLine("");
			return builder.ToString();
		}

		public static string Print(this List<PermissionScope> scopes)
		{
			return string.Join(", ", scopes);
		}

		public static string PrettyPrint(this Peer peer)
		{
			var builder = new StringBuilder();
			builder.AppendLine("Peer {");
			builder.AppendLine($"\tName = {peer.Name},");
			builder.AppendLine($"\tVersion = {peer.Version},");
			builder.AppendLine($"\tRelayServer = {peer.RelayServer}");
			builder.Append("}");
			builder.AppendLine("");
			return builder.ToString();
		}

		public static string PrettyPrint(this OperationRequest request)
		{
			var builder = new StringBuilder();
			builder.AppendLine("OperationRequest {");

			// Add indentation to nested Network
			var networkLines = request.Network.PrettyPrint().Split(new[]
			{
				Environment.NewLine
			}, StringSplitOptions.None);

			foreach (var line in networkLines)
			{
				builder.AppendLine($"\t{line}");
			}

			builder.AppendLine($"\tOperationDetails = {request.OperationDetails.PrettyPrint()},");
			builder.AppendLine($"\tSourceAddress = {request.SourceAddress},");
			builder.AppendLine($"\tID = {request.Id},");
			builder.AppendLine($"\tSenderID = {request.SenderId}");
			builder.Append("}");
			builder.AppendLine("");
			return builder.ToString();
		}

		private static string PrettyPrint(this List<TezosBaseOperation> operations)
		{
			return operations.Aggregate("", (current, operation) => current + (operation.Kind + "\n"));
		}
	}

}