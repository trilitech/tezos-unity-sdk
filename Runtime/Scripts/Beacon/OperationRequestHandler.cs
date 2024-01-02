#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beacon.Sdk;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.Core.Domain.Services;
using Netezos.Keys;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers;
using TezosSDK.Tezos;

#endregion

namespace TezosSDK.Beacon
{

	public class OperationRequestHandler
	{
		private readonly WalletProviderInfo _walletProviderInfo;

		public OperationRequestHandler(WalletProviderInfo walletProviderInfo)
		{
			_walletProviderInfo = walletProviderInfo;
		}

		public async Task RequestTezosPermission(
			string networkName,
			DappBeaconClient beaconDappClient)
		{
			if (!Enum.TryParse(networkName, out NetworkType networkType))
			{
				networkType = TezosConfig.Instance.Network;
			}

			var network = new Network
			{
				Type = networkType,
				Name = _walletProviderInfo.Network,
				RpcUrl = _walletProviderInfo.Rpc
			};

			var permissionScopes = new List<PermissionScope>
			{
				PermissionScope.operation_request,
				PermissionScope.sign
			};

			var permissionRequest = new PermissionRequest(
				BeaconMessageType.permission_request,
				Constants.BeaconVersion,
				KeyPairService.CreateGuid(),
				beaconDappClient.SenderId,
				beaconDappClient.Metadata,
				network,
				permissionScopes);

			var activePeer = beaconDappClient.GetActivePeer();

			if (activePeer != null)
			{
				await beaconDappClient.SendResponseAsync(activePeer.SenderId, permissionRequest);
				Logger.LogInfo("Permission request sent");
			}
			else
			{
				Logger.LogError("No active peer found");
			}
		}

		public Task RequestTezosOperation(
			string destination,
			string entryPoint,
			string input,
			ulong amount,
			DappBeaconClient beaconDappClient)
		{
			var activeAccountPermissions = beaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				Logger.LogError("No active permissions");
				return Task.CompletedTask;
			}

			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			var operationDetails = new List<TezosBaseOperation>();

			var partialTezosTransactionOperation = new PartialTezosTransactionOperation(amount.ToString(), destination,
				new JObject
				{
					["entrypoint"] = entryPoint,
					["value"] = JToken.Parse(input)
				});

			operationDetails.Add(partialTezosTransactionOperation);

			var operationRequest = new OperationRequest(BeaconMessageType.operation_request, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), beaconDappClient.SenderId, activeAccountPermissions.Network,
				operationDetails, pubKey.Address);

			Logger.LogDebug("requesting operation: " + operationRequest);
			return beaconDappClient.SendResponseAsync(activeAccountPermissions.SenderId, operationRequest);
		}

		public Task RequestContractOrigination(string script, string delegateAddress, DappBeaconClient beaconDappClient)
		{
			var activeAccountPermissions = beaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				Logger.LogError("No active permissions");
				return Task.CompletedTask;
			}

			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			var operationDetails = new List<TezosBaseOperation>();

			var partialTezosTransactionOperation = new PartialTezosOriginationOperation("0",
				JObject.Parse(script), delegateAddress);

			operationDetails.Add(partialTezosTransactionOperation);

			var operationRequest = new OperationRequest(BeaconMessageType.operation_request, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), beaconDappClient.SenderId, activeAccountPermissions.Network,
				operationDetails, pubKey.Address);

			Logger.LogDebug("requesting operation: " + operationRequest);
			return beaconDappClient.SendResponseAsync(activeAccountPermissions.SenderId, operationRequest);
		}
	}

}