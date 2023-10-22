using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace TezosSDK.Helpers
{
    public static class TaskExtension
    {
        public static IEnumerator ToCoroutine(this Task task)
        {
            var t = Task.Run(async () => await task);
            yield return new WaitUntil(() => t.IsCompleted);
        }
    }
}