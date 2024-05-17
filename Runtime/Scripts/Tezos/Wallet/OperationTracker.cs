using System;
using System.Collections;
using TezosSDK.Helpers.Coroutines;
using TezosSDK.Helpers.HttpClients;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Managers;
using UnityEngine;

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
		private Coroutine _trackingCoroutine;

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
			TezosLog.Debug($"Begin tracking operation with hash: {_operationHash}");
			_trackingCoroutine = CoroutineRunner.Instance.StartCoroutine(TrackOperationCoroutine());
		}

		private IEnumerator TrackOperationCoroutine()
		{
			var startTime = Time.time;

			while (Time.time - startTime < TIMEOUT)
			{
				TezosLog.Debug($"Checking operation status for hash {_operationHash}");

				yield return
					TezosManager.Instance.Tezos.API.GetOperationStatus(OperationStatusCallback, _operationHash);

				yield return new WaitForSecondsRealtime(WAIT_TIME);

				TezosLog.Debug(
					$"Waiting {WAIT_TIME} seconds before next operation status check. Remaining time: {TIMEOUT - (Time.time - startTime)}");
			}

			TezosLog.Error("Operation tracking timed out.");
			_onComplete?.Invoke(false, "Operation tracking timed out.");
		}

		private void OperationStatusCallback(HttpResult<bool> httpResult)
		{
			if (!httpResult.Success)
			{
				return;
			}

			if (!httpResult.Data)
			{
				return;
			}

			TezosLog.Debug("Operation is confirmed. Exiting polling loop.");
			_onComplete?.Invoke(true, null);
			CoroutineRunner.Instance.StopCoroutine(_trackingCoroutine);
		}
	}

}