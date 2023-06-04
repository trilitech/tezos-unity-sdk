using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHyperlinkButton : MonoBehaviour
{
    [SerializeField] private string _url = "https://ghostnet.tzkt.io/";
    [SerializeField] private TextMeshProUGUI _urlPostfixText;

    public void OpenBlockExplorerHyperlink()
    {
        Application.OpenURL(_url + _urlPostfixText.text);
    }
}