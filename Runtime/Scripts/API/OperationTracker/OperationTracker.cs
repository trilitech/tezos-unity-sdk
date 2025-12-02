using System;
using System.IO;
using Tezos.Configs;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MessageSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace Tezos.API
{
	/// <summary>
	///     Tracks the status of a blockchain operation by its hash and reports the result via callbacks.
	/// </summary>
	public class OperationTracker
	{
		private const    float                WAIT_TIME = 2000f; // milliseconds (2 seconds)
		private readonly Action<bool, string> _onComplete;
		private readonly string               _operationHash;
		private          bool                 _isTracking;
		private          string               _rpc;

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
			_onComplete    = onComplete;
			_rpc           = ConfigGetter.GetOrCreateConfig<TezosConfig>().Rpc;
		}

		/// <summary>
		///     Begins tracking the status of the operation.
		/// </summary>
		public async void BeginTracking()
		{
			TezosLogger.LogDebug($"Begin tracking operation with hash: {_operationHash}");
			_isTracking = true;
			await TrackOperationAsync();
		}

		/// <summary>
		///     Asynchronously tracks the operation status.
		/// </summary>
		private async UniTask TrackOperationAsync()
		{
			float startTime = Time.time;
			var   timeout   = ConfigGetter.GetOrCreateConfig<TezosConfig>().RequestTimeoutSeconds;

			while (_isTracking && Time.time - startTime < timeout)
			{
				TezosLogger.LogDebug($"Checking operation status for hash {_operationHash}");
				bool? result = await GetOperationStatusAsync(_operationHash);
				if (result == true)
				{
					TezosLogger.LogDebug("Operation is confirmed. Exiting polling loop.");
					_onComplete?.Invoke(true, null);
					return;
				}

				if (result == false)
				{
					TezosLogger.LogDebug("Operation is not confirmed yet. Continuing to check.");
				}
				else
				{
					TezosLogger.LogError("Failed to get operation status due to an error.");
					_onComplete?.Invoke(false, "Error checking operation status.");
					return;
				}

				TezosLogger.LogDebug($"Waiting {WAIT_TIME / 1000} seconds before next operation status check. Remaining time: {timeout - (Time.time - startTime)}");
				await UniTask.Delay((int)WAIT_TIME); // Wait before checking again
			}

			TezosLogger.LogError("Operation tracking timed out.");
			_onComplete?.Invoke(false, "Operation tracking timed out.");
		}

		/// <summary>
		///     Asynchronously retrieves the status of the operation.
		/// </summary>
		private async UniTask<bool?> GetOperationStatusAsync(string operationHash)
		{
			string                url     = Path.Combine(_rpc, $"operations/{operationHash}/status");
			using UnityWebRequest request = UnityWebRequest.Get(url);
			request.SetRequestHeader("Accept", "application/json");
			request.timeout = 10; // Timeout in seconds
			var operation = request.SendWebRequest();
			while (!operation.isDone)
			{
				await UniTask.Yield();
			}

			if (request.result != UnityWebRequest.Result.Success)
			{
				TezosLogger.LogError($"Failed to get operation status: {request.error}");
				return null; // Returning null to indicate an error
			}

			// Parse response
			string responseText = request.downloadHandler.text;
			TezosLogger.LogDebug($"Received operation status response: {responseText}");
			try
			{
				// Assuming the response contains a boolean indicating success or failure
				bool isConfirmed = JsonUtility.FromJson<bool>(responseText); // Adjust this to match the actual response format
				return isConfirmed;
			}
			catch (Exception ex)
			{
				TezosLogger.LogError($"Failed to deserialize operation status: {ex.Message}");
				return null; // Returning null to indicate an error
			}
		}
	}
}