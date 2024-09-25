using System;
using System.Threading;
using Tezos.Cysharp.Threading.Tasks;

namespace Tezos.Cysharp
{
	public static class UniTaskCompletionSourceExtensions
	{
		/// <summary>
		/// Adds a timeout to the UniTaskCompletionSource. If the timeout is exceeded, the task is canceled.
		/// </summary>
		/// <typeparam name="T">The type of the result for the UniTaskCompletionSource.</typeparam>
		/// <param name="source">The UniTaskCompletionSource to which the timeout will be applied.</param>
		/// <param name="timeout">The timeout duration in milliseconds.</param>
		/// <param name="cancellationMessage">An optional message to specify when cancellation occurs.</param>
		/// <returns>The original UniTask from the source, or a canceled task if the timeout occurs.</returns>
		public static async UniTask<T> WithTimeout<T>(this UniTaskCompletionSource<T> source, int timeout, string cancellationMessage = "Operation timed out")
		{
			using var cts = new CancellationTokenSource();

			var timeoutTask = UniTask.Delay(timeout, cancellationToken: cts.Token);

			(bool hasResultLeft, T result) completedTask = await UniTask.WhenAny(source.Task, timeoutTask);

			if (completedTask.hasResultLeft)
			{
				cts.Cancel();
				return await source.Task;
			}

			source.TrySetException(new TimeoutException(cancellationMessage));

			throw new TimeoutException(cancellationMessage);
		}
	}
}