using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beacon.Sdk;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Operation;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.BeaconClients;
using Beacon.Sdk.BeaconClients.Abstract;
using Beacon.Sdk.Core.Domain.Entities;
using Beacon.Sdk.Core.Domain.Services;
using Netezos.Keys;
using Newtonsoft.Json.Linq;
using TezosSDK.Helpers;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Tezos;

namespace TezosSDK.Beacon
{

	/// <summary>
	///     A helper class to handle operation requests for the Tezos blockchain via the Dapp Beacon client.
	/// </summary>
	public class OperationRequestHandler
	{
		/// <summary>
		///     Raised when a message is sent using the Beacon client's SendResponseAsync method.
		/// </summary>
		/// <remarks>Can be listened to trigger UI updates or to open the wallet.</remarks>
		/// <seealso cref="IBaseBeaconClient.SendResponseAsync" />
		/// <seealso cref="IBaseBeaconClient" />
		public event Action<BeaconMessageType> MessageSent;

		/// <summary>
		///     Requests Tezos permissions asynchronously.
		/// </summary>
		/// <param name="beaconDappClient">The Dapp Beacon client instance.</param>
		/// <remarks>
		///     This method attempts to get the active peer from the beacon client. If found,
		///     it creates a permission request and sends it to the active peer.
		/// </remarks>
		public async Task RequestTezosPermission(DappBeaconClient beaconDappClient)
		{
			// Check for active peer and log error if not found
			var activePeer = beaconDappClient.GetActivePeer();

			if (activePeer == null)
			{
				Logger.LogError("No active peer found");
				return;
			}

			var network = CreateNetwork();
			var permissionRequest = CreatePermissionRequest(beaconDappClient, network);
			await SendBeaconClientResponseAsync(beaconDappClient, activePeer.SenderId, permissionRequest);
			Logger.LogInfo("Permission request sent");
		}

		/// <summary>
		///     Initiates a Tezos operation request asynchronously.
		/// </summary>
		/// <param name="destination">The destination address of the transaction.</param>
		/// <param name="entryPoint">The entry point for the transaction.</param>
		/// <param name="input">The input data for the transaction.</param>
		/// <param name="amount">The amount to be transferred.</param>
		/// <param name="beaconDappClient">The Dapp Beacon client instance.</param>
		/// <returns>A Task representing the asynchronous operation.</returns>
		public Task RequestTezosOperation(
			string destination,
			string entryPoint,
			string input,
			ulong amount,
			DappBeaconClient beaconDappClient)
		{
			return RequestOperation(beaconDappClient,
				() => CreateTransactionOperation(destination, entryPoint, input, amount),
				BeaconMessageType.operation_request);
		}

		/// <summary>
		///     Requests contract origination in Tezos asynchronously.
		/// </summary>
		/// <param name="script">The script of the contract to be originated.</param>
		/// <param name="delegateAddress">The delegate address for the contract.</param>
		/// <param name="beaconDappClient">The Dapp Beacon client instance.</param>
		/// <returns>A Task representing the asynchronous operation.</returns>
		public Task RequestContractOrigination(string script, string delegateAddress, DappBeaconClient beaconDappClient)
		{
			return RequestOperation(beaconDappClient, () => CreateOriginationOperation(script, delegateAddress),
				BeaconMessageType.operation_request);
		}

		/// <summary>
		///     Handles the request for an operation in Tezos.
		/// </summary>
		/// <param name="beaconDappClient">The Dapp Beacon client instance.</param>
		/// <param name="operationFactory">
		///     A function that returns a list of Tezos base operations. This factory pattern is used
		///     to create different types of operations (e.g., transaction, origination) based on the context.
		/// </param>
		/// <param name="messageType">The type of the beacon message.</param>
		/// <returns>A Task representing the asynchronous operation.</returns>
		/// <remarks>
		///     This method checks for active account permissions and then uses the provided
		///     operationFactory function to generate a list of operations which are then used
		///     to create and send an operation request.
		/// </remarks>
		private Task RequestOperation(
			IDappBeaconClient beaconDappClient,
			Func<List<TezosBaseOperation>> operationFactory,
			BeaconMessageType messageType)
		{
			// Check for active account permissions and return if not found
			var activeAccountPermissions = beaconDappClient.GetActiveAccount();

			if (activeAccountPermissions == null)
			{
				Logger.LogError("No active peer found");
				return Task.CompletedTask;
			}

			// Invoke the operation factory to create operation details
			var operationDetails = operationFactory.Invoke();

			// Create and log the operation request
			var operationRequest = CreateOperationRequest(beaconDappClient, activeAccountPermissions, operationDetails,
				messageType);

			Logger.LogDebug("Requesting Operation: " + operationRequest.Print());

			// Send the operation request
			return SendBeaconClientResponseAsync(beaconDappClient, activeAccountPermissions.SenderId, operationRequest);
		}

		/// <summary>
		///     Creates an origination operation for Tezos.
		/// </summary>
		/// <param name="script">The script for the contract origination.</param>
		/// <param name="delegateAddress">The delegate address for the contract.</param>
		/// <returns>A list of Tezos base operations for origination.</returns>
		/// <remarks>
		///     This method prepares an origination operation with the given script and delegate address.
		/// </remarks>
		private List<TezosBaseOperation> CreateOriginationOperation(string script, string delegateAddress)
		{
			var operationDetails = new List<TezosBaseOperation>();

			var partialTezosOriginationOperation =
				new PartialTezosOriginationOperation("0", JObject.Parse(script), delegateAddress);

			operationDetails.Add(partialTezosOriginationOperation);

			return operationDetails;
		}

		/// <summary>
		///     Creates a transaction operation for Tezos.
		/// </summary>
		/// <param name="destination">The transaction's destination address.</param>
		/// <param name="entryPoint">The entry point for the transaction.</param>
		/// <param name="input">The input data for the transaction.</param>
		/// <param name="amount">The transaction amount.</param>
		/// <returns>A list of Tezos base operations for the transaction.</returns>
		/// <remarks>
		///     This method sets up a transaction operation with the provided parameters,
		///     creating a partial transaction operation object and adding it to the list.
		/// </remarks>
		private List<TezosBaseOperation> CreateTransactionOperation(
			string destination,
			string entryPoint,
			string input,
			ulong amount)
		{
			var operationDetails = new List<TezosBaseOperation>();

			// Create partial Tezos transaction operation with provided details
			var partialTezosTransactionOperation = new PartialTezosTransactionOperation(amount.ToString(), destination,
				new JObject
				{
					["entrypoint"] = entryPoint,
					["value"] = JToken.Parse(input)
				});

			operationDetails.Add(partialTezosTransactionOperation);

			return operationDetails;
		}

		/// <summary>
		///     Creates an operation request for Tezos.
		/// </summary>
		/// <param name="beaconDappClient">The Dapp Beacon client instance.</param>
		/// <param name="activeAccountPermissions">Information about the active account's permissions.</param>
		/// <param name="operationDetails">Details of the Tezos base operations.</param>
		/// <param name="messageType">The type of the beacon message.</param>
		/// <returns>An OperationRequest object configured with the provided details.</returns>
		/// <remarks>
		///     This method constructs an operation request using the client's sender ID, active account permissions,
		///     and the details of the operation to be performed.
		/// </remarks>
		private OperationRequest CreateOperationRequest(
			IBaseBeaconClient beaconDappClient,
			PermissionInfo activeAccountPermissions,
			List<TezosBaseOperation> operationDetails,
			BeaconMessageType messageType)
		{
			var pubKey = PubKey.FromBase58(activeAccountPermissions.PublicKey);

			var operationRequest = new OperationRequest(messageType, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), beaconDappClient.SenderId, activeAccountPermissions.Network,
				operationDetails, pubKey.Address);

			return operationRequest;
		}

		private Network CreateNetwork()
		{
			return new Network
			{
				Type = TezosManager.Instance.Config.Network,
				Name = TezosManager.Instance.Config.Network.ToString(),
				RpcUrl = TezosManager.Instance.Config.Rpc
			};
		}

		/// <summary>
		///     Creates a permission request for the Tezos blockchain.
		/// </summary>
		/// <param name="beaconDappClient">The Dapp Beacon client instance.</param>
		/// <param name="network">The network information for the request.</param>
		/// <returns>A PermissionRequest object configured with the provided details.</returns>
		private PermissionRequest CreatePermissionRequest(IBaseBeaconClient beaconDappClient, Network network)
		{
			var permissionScopes = new List<PermissionScope>
			{
				PermissionScope.operation_request,
				PermissionScope.sign
			};

			return new PermissionRequest(BeaconMessageType.permission_request, Constants.BeaconVersion,
				KeyPairService.CreateGuid(), beaconDappClient.SenderId, beaconDappClient.Metadata, network,
				permissionScopes);
		}

		/// <summary>
		///     Sends a response asynchronously via the Beacon client.
		/// </summary>
		private async Task SendBeaconClientResponseAsync(
			IBaseBeaconClient beaconDappClient,
			string receiverId,
			BaseBeaconMessage message)
		{
			await beaconDappClient.SendResponseAsync(receiverId, message);
			MessageSent?.Invoke(message.Type);
		}
	}

}