using System.Collections.Generic;
using System.Linq;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Core.Domain.Entities;

namespace TezosSDK.Helpers.Extensions
{

	public static class BeaconExtensions
	{
		public static string Print(this PermissionInfo info)
		{
			return $"Address: {info.Address} \n" + $"PublicKey: {info.PublicKey} \n" +
			       $"ConnectedAt: {info.ConnectedAt} \n" + $"Network: {info.Network} \n" +
			       $"Scopes: {info.Scopes.Print()} \n" + $"Metadata: \n{info.AppMetadata.Print()}";
		}

		public static string Print(this List<PermissionScope> scopes)
		{
			return string.Join(", ", scopes);
		}

		public static string Print(this AppMetadata data)
		{
			return $"-Name: {data.Name} \n" + $"-Icon: {data.Icon} \n" + $"-AppUrl: {data.AppUrl}";
		}

		public static string Print(this Peer peer)
		{
			return $"Name: {peer.Name}, " + $"Version: {peer.Version}, " + $"RelayServer: {peer.RelayServer}";
		}

		public static string Print(this PermissionResponse response)
		{
			var permissionsString = string.Join(", ", response.Scopes);

			return $"Received permissions: \"{permissionsString}\", " + $"from: \"{response.AppMetadata.Name}\", " +
			       $"with public key: \"{response.PublicKey}\"";
		}

		public static string Print(this OperationRequest request)
		{
			return $"{nameof(OperationRequest)} {{ " + $"Network = {request.Network}, " +
			       $"OperationDetails = {request.OperationDetails.Print()}, " +
			       $"SourceAddress = {request.SourceAddress} }}";
		}

		private static string Print(this List<TezosBaseOperation> operations)
		{
			return operations.Aggregate("", (current, operation) => current + (operation.Kind + "\n"));
		}
	}

}