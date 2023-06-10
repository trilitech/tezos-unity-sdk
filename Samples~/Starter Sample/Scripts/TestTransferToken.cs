using System;
using System.Collections.Generic;
using Netezos.Encoding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tezos.StarterSample
{
    public class TestTransferToken : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Button _button;

        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TMP_InputField _inputFieldAddress;
        [SerializeField] private TMP_InputField _inputFieldAmount;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnTransferTokenButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnTransferTokenButtonClicked);
        }

        private void OnTransferTokenButtonClicked()
        {
            _button.interactable = false;
            _resultText.text = "Pending...";

            string toAddress = _inputFieldAddress.text;
            decimal amount = decimal.Parse(_inputFieldAmount.text);

            // TODO:
        }
    }
}
