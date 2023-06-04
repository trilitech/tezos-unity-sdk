using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestGetOthersTezosBalance : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TMP_InputField _inputField;
    
    private void OnEnable()
    {
        _button.onClick.AddListener(OnGetOthersTezosBalanceButtonClicked);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnGetOthersTezosBalanceButtonClicked);
    }

    private void OnGetOthersTezosBalanceButtonClicked()
    {
        _button.interactable = false;
        _resultText.text = "Pending...";

        string address = _inputField.text;
        var routine = StarterTezosManager.Instance.GetTezosBalance(balance =>
        {
            double doubleBalance = balance / 1e6;    // 6 decimals
            _resultText.text = doubleBalance.ToString();
            _button.interactable = true;
        }, address);
        CoroutineRunner.Instance.StartWrappedCoroutine(routine);
    }
}
