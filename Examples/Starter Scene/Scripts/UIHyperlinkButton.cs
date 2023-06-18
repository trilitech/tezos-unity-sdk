using TMPro;
using UnityEngine;

namespace Tezos.StarterSample
{
    public class UIHyperlinkButton : MonoBehaviour
    {
        [SerializeField] private string _url = "https://ghostnet.tzkt.io/";
        [SerializeField] private TextMeshProUGUI _urlPostfixText;

        public void OpenBlockExplorerHyperlink()
        {
            Application.OpenURL(_url + _urlPostfixText.text);
        }
    }
}