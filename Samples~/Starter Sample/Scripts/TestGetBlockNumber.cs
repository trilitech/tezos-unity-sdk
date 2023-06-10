using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tezos.StarterSample
{
    public class TestGetBlockNumber : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Button _button;

        [SerializeField] private TextMeshProUGUI _resultText;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnGetBlockNumberButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnGetBlockNumberButtonClicked);
        }

        private void OnGetBlockNumberButtonClicked()
        {
            // TODO:
        }
    }
}