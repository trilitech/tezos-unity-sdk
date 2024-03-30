using System;
using System.Collections;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Helpers.HttpClients;
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
			Logger.LogDebug($"Begin tracking operation with hash: {_operationHash}");
			CoroutineRunner.Instance.StartCoroutine(TrackOperationCoroutine());
		}

		/// <summary>
		///     Coroutine that polls the blockchain operation status until it is confirmed, fails, or times out.
		/// </summary>
		private IEnumerator TrackOperationCoroutine()
		{
			var startTime = Time.time;
			bool? operationConfirmed = null;
			string errorMessage = null;

			while (Time.time - startTime < TIMEOUT)
			{
				Logger.LogDebug($"Checking operation status for hash {_operationHash}");

				yield return TezosManager.Instance.Tezos.API.GetOperationStatus(Callback, _operationHash);

				if (operationConfirmed.HasValue)
				{
					if (operationConfirmed.Value)
					{
						Logger.LogDebug("Operation is confirmed. Exiting polling loop.");
						_onComplete?.Invoke(true, null);
						yield break;
					}

					Logger.LogDebug($"Operation status check for hash {_operationHash} failed.");
				}

				yield return new WaitForSecondsRealtime(WAIT_TIME);

				Logger.LogDebug($"Waiting {WAIT_TIME} seconds before next operation status check. " +
				                $"Remaining time: {TIMEOUT - (Time.time - startTime)}");
			}

			errorMessage ??= "Operation tracking timed out.";
			Logger.LogDebug(errorMessage);
			_onComplete?.Invoke(false, errorMessage);
			yield break;

			void Callback(Result<bool?> res)
			{
				if (res.Success && res.Data.HasValue)
				{
					operationConfirmed = res.Data;
					Logger.LogDebug($"Operation status check returned: {operationConfirmed.Value}");
				}
				else
				{
					errorMessage = res.ErrorMessage;
				}
			}
		}
	}

}