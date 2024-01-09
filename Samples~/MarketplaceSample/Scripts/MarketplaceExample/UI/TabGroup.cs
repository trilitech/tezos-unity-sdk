using System.Collections.Generic;
using UnityEngine;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class TabGroup : MonoBehaviour
	{
		[SerializeField] private List<TabButton> _tabButtons;
		[SerializeField] private Sprite _tabIdle;
		[SerializeField] private Sprite _tabHover;
		[SerializeField] private Sprite _tabActive;
		[SerializeField] private List<GameObject> _pannelsToSwap;

		private TabButton _selectedTab;

		public Sprite TabActive
		{
			get => _tabActive;
		}

		public List<TabButton> TabButtons
		{
			get => _tabButtons;
		}

		public Sprite TabHover
		{
			get => _tabHover;
		}

		public Sprite TabIdle
		{
			get => _tabIdle;
		}

		private void Start()
		{
			if (_tabButtons.Count == 0)
			{
				return;
			}

			OnTabSelected(_tabButtons[0]);
		}

		/// <summary>
		///     To be called when a tab button is hovered over.
		/// </summary>
		/// <param name="button">The tab button that has been used.</param>
		public void OnTabEnter(TabButton button)
		{
			ResetTabs();

			if (_selectedTab == null || button != _selectedTab)
			{
				button.Background.sprite = TabHover;
			}
		}

		/// <summary>
		///     To be called when a tab button is no longer being hovered over.
		/// </summary>
		/// <param name="button">The tab button that has been exited from.</param>
		public void OnTabExit(TabButton button)
		{
			ResetTabs();
		}

		/// <summary>
		///     To be called when a new tab is selected.
		/// </summary>
		/// <param name="button">The tab button that has been used.</param>
		public void OnTabSelected(TabButton button)
		{
			if (_selectedTab != null)
			{
				_selectedTab.Deselect();
			}

			_selectedTab = button;
			_selectedTab.Select();

			ResetTabs();
			button.Background.sprite = TabActive;

			var index = button.transform.GetSiblingIndex();

			for (var i = 0; i < _pannelsToSwap.Count; i++)
			{
				_pannelsToSwap[i].SetActive(i == index ? true : false);
			}
		}

		/// <summary>
		///     Sets all tabs back to their initial state
		/// </summary>
		public void ResetTabs()
		{
			foreach (var button in _tabButtons)
			{
				if (_selectedTab != null && button == _selectedTab)
				{
					continue;
				}

				button.Background.sprite = TabIdle;
			}
		}

		/// <summary>
		///     Subscribe a tab button to this group.
		/// </summary>
		/// <param name="button">The tab button that is subscribing to this group.</param>
		public void Subscribe(TabButton button)
		{
			if (TabButtons == null)
			{
				_tabButtons.Add(button);
			}
		}
	}

}