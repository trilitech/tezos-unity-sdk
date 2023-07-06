using System.Collections.Generic;
using UnityEngine;

namespace TezosSDK.Samples.DemoExample
{
    public class SnapController : MonoBehaviour
    {
        [SerializeField] private List<ItemSnapPoint> snapPoints;
        [SerializeField] private List<Draggable> draggables;
        [SerializeField] private float snapRange = 100f;

        private ItemSnapPoint _currentHighlighedSlot = null;
        private Draggable _currentDraggable = null;
        private bool _isDragging = false;

        private void Update()
        {
            if (_currentHighlighedSlot != null)
                _currentHighlighedSlot.Highlight(false);

            if (_isDragging)
            {
                _currentHighlighedSlot = GetNearestSlotFromMouse();
                if (_currentHighlighedSlot != null)
                    _currentHighlighedSlot.Highlight(true);
            }
        }

        public List<ItemSnapPoint> SnapPoints
        {
            get => snapPoints;
            set => snapPoints = value;
        }

        public List<Draggable> Draggables
        {
            get => draggables;
            set
            {
                draggables = value;
                foreach (var draggable in draggables)
                {
                    draggable.OnBeginDragging = OnItemDragBegin;
                    draggable.OnEndDragging = OnItemDragEnd;
                }
            }
        }

        public void OnItemDragBegin(Draggable draggable)
        {
            _isDragging = true;
            _currentDraggable = draggable;
        }

        public void OnItemDragEnd(Draggable draggable)
        {
            _isDragging = false;
            _currentDraggable = null;
            if (_currentHighlighedSlot != null)
                draggable.SetCurrentSlot(_currentHighlighedSlot);
            else
                draggable.ResetPosition();
        }

        private void EnableSlotHighlighting(bool enable)
        {
            foreach (var snapPoint in snapPoints)
            {
                snapPoint.EnableHighlighting(enable);
            }
        }

        private ItemSnapPoint GetNearestSlotFromMouse()
        {
            float closestDistance = -1;
            ItemSnapPoint closestSnapPoint = null;
            foreach (var snapPoint in snapPoints)
            {
                // check if slot is snappable and empty
                if ((!snapPoint.IsSnappable) || (snapPoint.HasItem))
                    continue;
                // check if slot accepts item type
                if (!snapPoint.AllowsAllItemTypes)
                    if (!snapPoint.AcceptsItem(_currentDraggable.Item))
                        continue;
                // calculate distance from mouse to slot
                float distance = Vector2.Distance(Input.mousePosition, snapPoint.transform.position);
                if (closestSnapPoint == null || distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSnapPoint = snapPoint;
                }
            }

            if (closestDistance <= snapRange)
                return closestSnapPoint;
            else
                return null;
        }
    }
}