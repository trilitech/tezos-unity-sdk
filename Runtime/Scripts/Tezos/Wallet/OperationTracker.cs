using System;
using System.Collections;
using TezosSDK.Helpers.Coroutines;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tezos.Wallet
{

	/// <summary>
	///     Tracks the status of a blockchain operation by its hash and reports the result via callbacks.
	/// </summary>
	public class OperationTracker
	{
		private const float TIMEOUT = 30f; // seconds
		private const float WAIT_TIME = 2f; // seconds
		private readonly Action<bool, string> _onComplete;
		private readonly string _operationHash;

		/// <summary>
		///     Initializes a new instance of the <see cref="OperationTracker" /> class.
		/// </summary>
		/// <param name="operationHash">The hash of the operation to track.</param>
		/// <param name="onComplete">
		///     Callback invoked when tracking is complete. The first parameter indicates success,
		///     and the second parameter carries an error message if the operation failed.
		/// </param>
		public OperationTracker(string operationHash, Action<bool, string> onComplete)
		{
			_operationHash = operationHash;
			_onComplete = onComplete;
		}

		/// <summary>
		///     Begins tracking the status of the operation.
		/// </summary>
		public void BeginTracking()
		{
			Logger.LogDebug($"Starting to track operation with hash: {_operationHash}");
			CoroutineRunner.Instance.StartWrappedCoroutine(TrackOperationCoroutine());
		}

		/// <summary>
		///     Coroutine that polls the blockchain operation status until it is confirmed, fails, or times out.
		/// </summary>
		private IEnumerator TrackOperationCoroutine()
		{
			var startTime = Time.time;
			bool? operationConfirmed = null;
			string errorMessage = null;

			// Begin polling loop for operation status
			Logger.LogDebug($"Begin polling for operation status with hash: {_operationHash}");

			var remainingTime = TIMEOUT - (Time.time - startTime);

			while (remainingTime > 0)
			{
				yield return TezosManager.Instance.Tezos.API.GetOperationStatus(Callback, _operationHash);

				// If the operation is positively confirmed, exit the loop.
				if (operationConfirmed == true)
				{
					Logger.LogDebug("Operation is confirmed. Exiting polling loop.");
					break;
				}

				// Handle the case where operationConfirmed is null or false
				if (!operationConfirmed.HasValue)
				{
					errorMessage = "Operation status check failed or returned null.";
					Logger.LogDebug($"Operation status check for hash {_operationHash} failed or returned null.");
				}

				// Wait before the next status check
				yield return new WaitForSecondsRealtime(WAIT_TIME);

				Logger.LogDebug($"Waiting {WAIT_TIME} seconds before next operation status check. " +
				                $"Remaining time: {remainingTime}");

				remainingTime = TIMEOUT - (Time.time - startTime);
			}

			// Determine final success and handle possible timeout
			var success = operationConfirmed == true;

			if (!success)
			{
				errorMessage ??= "Operation tracking timed out.";
				Logger.LogDebug(errorMessage);
			}

			Logger.LogDebug("Operation tracking complete. " + $"Success: {success}, ErrorMessage: {errorMessage}");

			// Invoke the callback with the result
			_onComplete?.Invoke(success, errorMessage);
			yield break;

			void Callback(bool? result)
			{
				operationConfirmed = result;

				if (operationConfirmed.HasValue)
				{
					Logger.LogDebug($"Operation status check returned: {operationConfirmed.Value}");
				}
				else
				{
					Logger.LogDebug($"Operation status check for hash \"{_operationHash}\" returned null.");
				}
			}
		}
	}

}