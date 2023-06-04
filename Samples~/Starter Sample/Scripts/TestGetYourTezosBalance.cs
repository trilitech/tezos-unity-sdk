using System;
using System.Collections.Generic;
using System.Globalization;
using Netezos.Encoding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestGetYourTezosBalance : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _resultText;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnGetYourTezosBalanceButtonClicked);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnGetYourTezosBalanceButtonClicked);
    }

    private void OnGetYourTezosBalanceButtonClicked()
    {
        _resultText.text = "Pending...";
        
        string address = StarterTezosManager.Instance.GetActiveAddress();
        var routine = StarterTezosManager.Instance.GetTezosBalance(balance =>
        {
            // 6 decimals
            var doubleBalance = balance / 1e6;
            _resultText.text = doubleBalance.ToString();
        }, address);
        CoroutineRunner.Instance.StartWrappedCoroutine(routine);
    }
}

