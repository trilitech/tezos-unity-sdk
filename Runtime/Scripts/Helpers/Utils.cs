using System.IO;
using System.Runtime.CompilerServices;

namespace TezosSDK.Helpers
{
    public static class Utils
    {
        public static string GetThisFileDir([CallerFilePath] string path = null)
        {
            return Path.GetDirectoryName(path);
        }
    }
}