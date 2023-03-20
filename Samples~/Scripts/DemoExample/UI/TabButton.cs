using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private TabGroup _tabGroup;
    [SerializeField] private Image _background;

    public TabGroup TabGroup => _tabGroup;
    public Image Background => _background;

    public UnityEvent OnTabSelected;
    public UnityEvent OnTabDeselected;

    public void OnPointerClick(PointerEventData eventData)
	{
        TabGroup.OnTabSelected(this);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
        TabGroup.OnTabEnter(this);
    }

	public void OnPointerExit(PointerEventData eventData)
	{
        TabGroup.OnTabExit(this);
    }

	void Awake()
    {
        if (_background == null)
        {
            _background = GetComponent<Image>();
        }

        _tabGroup.Subscribe(this);
    }

    /// <summary>
    /// To be called when the button is selected so that event callbacks can be used.
    /// </summary>
	public void Select()
	{
        if (OnTabSelected != null)
        {
            OnTabSelected.Invoke();
        }
	}

    /// <summary>
    /// To be called when the button is deselected so that event callbacks can be used.
    /// </summary>
    public void Deselect()
    {
        if (OnTabDeselected != null)
        {
            OnTabDeselected.Invoke();
        }
    }
}
