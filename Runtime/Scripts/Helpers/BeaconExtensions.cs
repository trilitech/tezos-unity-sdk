using System.Collections.Generic;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Core.Domain.Entities;

namespace TezosSDK.Helpers
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
			return $"Received permissions: \"{permissionsString}\", " +
			       $"from: \"{response.AppMetadata.Name}\", " +
			       $"with public key: \"{response.PublicKey}\"";
		}
	}

}