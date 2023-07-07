using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.Samples.NFTApiSample
{
    public class UiManager : MonoBehaviour
    {
        [SerializeField] private DataManager dataManager;
        [SerializeField] private InputField addressInputField;
        [SerializeField] private InputField contractInputField;
        [SerializeField] private InputField tokenIdInputField;
        [SerializeField] private Text resultText;

        void Start()
        {
            dataManager.DataReceived += OnDataReceived;

            addressInputField.onEndEdit.AddListener(delegate { OnEndEditAddress(addressInputField); });
            contractInputField.onEndEdit.AddListener(delegate { OnEndEditContract(contractInputField); });
            tokenIdInputField.onEndEdit.AddListener(delegate { OnEndEditTokenId(tokenIdInputField); });
        }

        void OnDataReceived(string data)
        {
            resultText.text = string.Empty;
            resultText.text = data;
        }

        void OnEndEditAddress(InputField input)
        {
            dataManager.SetCheckAddress(input.text);
        }

        void OnEndEditContract(InputField input)
        {
            dataManager.SetCheckContract(input.text);
        }

        void OnEndEditTokenId(InputField input)
        {
            dataManager.SetCheckTokenId(input.text);
        }
    }
}