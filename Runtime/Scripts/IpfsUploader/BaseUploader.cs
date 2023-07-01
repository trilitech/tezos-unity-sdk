using UnityEngine;

namespace TezosSDK.Scripts.IpfsUploader
{
    public abstract class BaseUploader : MonoBehaviour
    {
        public string ApiUrl { get; } = "https://api.pinata.cloud/pinning/pinFileToIPFS";
        
        public string ApiKey { get; } =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySW5mb3JtYXRpb24iOnsiaWQiOiIzYWY5Nzk2YS1jNWZkLTRjMTMtYmE4Zi0wMzlmOWQ4MTE5OTIiLCJlbWFpbCI6ImpvaG55c2FsdmVzZW5AZ21haWwuY29tIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsInBpbl9wb2xpY3kiOnsicmVnaW9ucyI6W3siaWQiOiJGUkExIiwiZGVzaXJlZFJlcGxpY2F0aW9uQ291bnQiOjF9LHsiaWQiOiJOWUMxIiwiZGVzaXJlZFJlcGxpY2F0aW9uQ291bnQiOjF9XSwidmVyc2lvbiI6MX0sIm1mYV9lbmFibGVkIjpmYWxzZSwic3RhdHVzIjoiQUNUSVZFIn0sImF1dGhlbnRpY2F0aW9uVHlwZSI6InNjb3BlZEtleSIsInNjb3BlZEtleUtleSI6ImM5MmEzNDE2NGNkNDA2MDA5NGQ2Iiwic2NvcGVkS2V5U2VjcmV0IjoiZDY5N2QyYWU1MWExNTE3YTBkMDE0OGJiMmJkNTFjYTJmNGU5YmU2Y2Q1YjdiNWI5NDZmMzAzZjNiMDBmOTYwMCIsImlhdCI6MTY4ODE0NzYzNX0.U15IRa4ifJLvKwQ5GPC4nYf835Bo157L3f3-TYItNto";
        
        public string SupportedFileExtensions { get; } = ".jpg, .jpeg, .png";

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}