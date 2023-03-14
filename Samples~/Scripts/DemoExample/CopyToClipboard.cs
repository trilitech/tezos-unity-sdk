using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CopyToClipboard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMPro.TextMeshProUGUI text;
    [SerializeField] TMPro.TMP_InputField inputField;

    bool _blockCopy = false;

    private void Start()
    {
        inputField.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_blockCopy)
            return;
        
#if UNITY_WEBGL
        inputField.gameObject.SetActive(true);
        inputField.text = text.text;
        text.gameObject.SetActive(false);
#endif
        
        // copy text to the clipboard
        GUIUtility.systemCopyBuffer = text.text;
        CoroutineRunner.Instance.StartCoroutine(OnTextCopied());
    }

    IEnumerator OnTextCopied()
    {
        _blockCopy = true;
        string origin = text.text;
        text.text = "Copied to clipboard.";
        yield return new WaitForSeconds(1.5f);
        text.text = origin;
        _blockCopy = false;
    }
}
