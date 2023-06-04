using System;
using System.Collections.Generic;
using Netezos.Encoding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestTransferToken : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TMP_InputField _inputFieldAddress;
    [SerializeField] private TMP_InputField _inputFieldAmount;

    public void OnTransferTokenButtonClicked()
    {
        TransferToken();
    }

    private void TransferToken()
    {
        _button.interactable = false;
        _resultText.text = "Pending...";

        string toAddress = _inputFieldAddress.text;
        decimal amount = decimal.Parse(_inputFieldAmount.text);

        // TODO:
    }
}
