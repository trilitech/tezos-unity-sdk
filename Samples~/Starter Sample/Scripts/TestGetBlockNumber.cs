using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

    public class TestGetBlockNumber : MonoBehaviour
    {
        [SerializeField] private Button _button;
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
