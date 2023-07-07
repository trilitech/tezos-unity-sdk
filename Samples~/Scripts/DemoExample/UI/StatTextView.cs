using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TezosSDK.Samples.DemoExample
{
    public class StatTextView : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI statNameText;
        [SerializeField] private TMPro.TextMeshProUGUI statValueText;

        public void SetStatName(string name)
        {
            statNameText.text = name;
        }

        public void SetStatValue(string value)
        {
            statValueText.text = value;
        }
    }
}