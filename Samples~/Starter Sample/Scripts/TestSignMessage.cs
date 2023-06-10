using System;
using Beacon.Sdk.Beacon.Sign;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.Json;
namespace Tezos.StarterSample
{
    public class TestSignMessage : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Button _button;

        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TMP_InputField _inputField;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnSignMessageButtonClicked);
            StarterTezosManager.Instance.MessageReceiver.PayloadSigned += OnMessageSigned;
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnSignMessageButtonClicked);
            StarterTezosManager.Instance.MessageReceiver.PayloadSigned -= OnMessageSigned;
        }

        private void OnSignMessageButtonClicked()
        {
            _resultText.text = "";

            StarterTezosManager.Instance.RequestSignPayload(SignPayloadType.raw, _inputField.text);
        }

        private void OnMessageSigned(string result)
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                _resultText.text = result;
                var json = JsonSerializer.Deserialize<JsonElement>(result);
                var signature = json.GetProperty("signature").GetString();
                _resultText.text = "Signed.";
            });
        }
    }
}
