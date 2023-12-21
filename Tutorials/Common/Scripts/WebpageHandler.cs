using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TezosSDK
{
    public class WebpageHandler : MonoBehaviour
    {
        [SerializeField] private string webpage;
        
        public void OpenWebpage()
        {
            Application.OpenURL(webpage);
        }
    }
}
