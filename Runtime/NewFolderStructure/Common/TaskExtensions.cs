using System.Threading.Tasks;
using UnityEngine;

namespace TezosSDK.Common
{
    public static class TaskExtensions
    {
        public static void LogExceptionIfFaulted(this Task task) {
            task.ContinueWith(t => {
                if (t.IsFaulted) {
                    Debug.LogException(t.Exception.Flatten().InnerException);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}