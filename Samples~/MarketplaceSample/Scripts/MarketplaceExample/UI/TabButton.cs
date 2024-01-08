using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	[RequireComponent(typeof(Image))]
	public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
	{
		[SerializeField] private TabGroup _tabGroup;
		[SerializeField] private Image _background;

		public UnityEvent OnTabSelected;
		public UnityEvent OnTabDeselected;

		public Image Background
		{
			get => _background;
		}

		public TabGroup TabGroup
		{
			get => _tabGroup;
		}

		#region IPointerClickHandler Implementation

		public void OnPointerClick(PointerEventData eventData)
		{
			TabGroup.OnTabSelected(this);
		}

		#endregion

		#region IPointerEnterHandler Implementation

		public void OnPointerEnter(PointerEventData eventData)
		{
			TabGroup.OnTabEnter(this);
		}

		#endregion

		#region IPointerExitHandler Implementation

		public void OnPointerExit(PointerEventData eventData)
		{
			TabGroup.OnTabExit(this);
		}

		#endregion

		private void Awake()
		{
			if (_background == null)
			{
				_background = GetComponent<Image>();
			}

			_tabGroup.Subscribe(this);
		}

		/// <summary>
		///     To be called when the button is deselected so that event callbacks can be used.
		/// </summary>
		public void Deselect()
		{
			if (OnTabDeselected != null)
			{
				OnTabDeselected.Invoke();
			}
		}

		/// <summary>
		///     To be called when the button is selected so that event callbacks can be used.
		/// </summary>
		public void Select()
		{
			if (OnTabSelected != null)
			{
				OnTabSelected.Invoke();
			}
		}
	}

}