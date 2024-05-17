using System;
using System.Text;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Permission;

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
	}

}