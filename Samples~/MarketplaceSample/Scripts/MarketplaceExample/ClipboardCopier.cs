using System.Collections;
using TezosSDK.Helpers.Coroutines;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TezosSDK.MarketplaceSample.MarketplaceExample
{

	public class ClipboardCopier : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private TextMeshProUGUI text;
		[SerializeField] private TMP_InputField inputField;

		private bool _blockCopy;

		#region IPointerClickHandler Implementation

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_blockCopy)
			{
				return;
			}

#if UNITY_WEBGL
			inputField.gameObject.SetActive(true);
			inputField.text = text.text;
			text.gameObject.SetActive(false);
#endif

			// copy text to the clipboard
			GUIUtility.systemCopyBuffer = text.text;
			CoroutineRunner.Instance.StartWrappedCoroutine(OnTextCopied());
		}

		#endregion

		private void Start()
		{
			inputField.gameObject.SetActive(false);
		}

		private IEnumerator OnTextCopied()
		{
			_blockCopy = true;
			var origin = text.text;
			text.text = "Copied to clipboard.";
			yield return new WaitForSeconds(1.5f);
			text.text = origin;
			_blockCopy = false;
		}
	}

}