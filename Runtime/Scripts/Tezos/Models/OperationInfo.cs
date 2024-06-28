using System;
using Beacon.Sdk.Beacon;

namespace TezosSDK.Tezos.Models
{

	/// <summary>
	///     Contains the result of a blockchain operation, indicating its success and transaction details.
	/// </summary>
	public class OperationInfo
	{
		/// <summary>
		///     The hash of the transaction associated with the operation.
		/// </summary>
		public readonly string Hash;

		/// <summary>
		///     The ID of the operation.
		/// </summary>
		public readonly string Id;

		/// <summary>
		///     The type of operation.
		/// </summary>
		public readonly BeaconMessageType OperationType;

		/// <summary>
		///     Conveys the error message if the operation failed.
		/// </summary>
		public readonly string ErrorMesssage;

		public OperationInfo(
			string hash,
			string id,
			BeaconMessageType operationType,
			string errorMessage = null)
		{
			Hash = hash;
			Id = id;
			OperationType = operationType;
			ErrorMesssage = errorMessage;
		}
	}

}