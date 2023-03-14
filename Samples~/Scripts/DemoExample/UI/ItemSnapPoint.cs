using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemSnapPoint : MonoBehaviour
{
    [SerializeField] private bool snappable = true;
    [SerializeField] private bool allowAllItemTypes = true;
    [SerializeField] private ItemType[] allowedTypes;

    private Draggable _currentItemInSlot;
    private Image _image;
    private bool _highledEnabled = true;
    private Color _originalColor;
    private Color _highlightColor;

    public bool AllowsAllItemTypes => allowAllItemTypes;
    public bool HasItem => _currentItemInSlot != null;
    public bool IsSnappable => snappable;
    public Draggable CurrentItemInSlot => _currentItemInSlot;
    
    [Header("Events:")]
    [SerializeField] public UnityEvent OnItemSet;
    [SerializeField] public UnityEvent OnItemRemoved;

    private void Start()
    {
        _image = GetComponent<Image>();
        _originalColor = _image.color;
        _highlightColor = Color.white;
    }

    public void SetItemInSlot(Draggable item)
    {
        _currentItemInSlot = item;
        OnItemSet?.Invoke();
    }

    public void RemoveItemInSlot()
    {
        _currentItemInSlot = null;
        OnItemRemoved?.Invoke();
    }

    public void Highlight(bool highlight)
    {
        _image.color = (highlight) ? _highlightColor : _originalColor;
    }

    public void EnableHighlighting(bool enable)
    {
        _highledEnabled = enable;
        if (!_highledEnabled)
            _image.color = _originalColor;
    }

    public bool AcceptsItem(IItemModel item)
    {
        return allowedTypes.Contains(item.Type);
    }
}
