using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tezos.StarterSample
{
    public class TestGetLatestBlockLevel : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _resultText;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnGetLatestBlockLevelButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnGetLatestBlockLevelButtonClicked);
        }

        private void OnGetLatestBlockLevelButtonClicked()
        {
            _resultText.text = "Pending...";
            
            var routine = StarterTezosManager.Instance.GetLatestBlockLevel(latestBlockLevel =>
            {
                _resultText.text = latestBlockLevel.ToString();
            });
            CoroutineRunner.Instance.StartWrappedCoroutine(routine);
        }
    }
}