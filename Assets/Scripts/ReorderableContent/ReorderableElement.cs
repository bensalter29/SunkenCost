﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReorderableContent
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class ReorderableElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private ReorderableGrid _currentReorderableGrid;

        private RectTransform _rectTransform;
        private LayoutElement _layoutElement;

        private int _currentSiblingIndex;
        private ReorderableGrid _listHoveringOver;
        private RectTransform _emptySpaceRect;

        private bool _justSpawnedFake;
        private bool _isDragging;
        private bool _isHoveringOverReorderableElement;

        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
        private ReorderableElement _elementToMergeWith;

        private event Action OnStartedDrag;
        private event Action<ReorderableGrid> OnHoveringOverList;
        private event Action OnEndedDrag;

        private event Action OnStartMerge;
        private event Action OnCancelMerge;
        private event Action<ReorderableElement> OnFinaliseMerge;

        private bool _currentlyMerging;
        private Func<ReorderableElement, bool> TryMergeInto;
        
        [field: SerializeField] public bool IsMergeable { get; private set; }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _layoutElement = gameObject.AddComponent<LayoutElement>();
            
            var rect = _rectTransform.rect;
            _layoutElement.preferredWidth = rect.size.x;
            _layoutElement.preferredHeight = rect.size.y;
        }

        public void Init(ReorderableGrid reorderableGrid, IReorderableElementEventListener listener = null)
        {
            _canvas = reorderableGrid.Canvas;
            _currentReorderableGrid = reorderableGrid;
            _currentSiblingIndex = transform.GetSiblingIndex();

            IsMergeable = reorderableGrid.IsMergeable;
            
            if (listener != null)
            {
                OnStartedDrag += listener.Grabbed;
                OnHoveringOverList += listener.HoveringOverList;
                OnEndedDrag += listener.Released;

                if (IsMergeable)
                {
                    if (listener is IMergeableReorderableEventListener mergeableListener)
                    {
                        TryMergeInto = mergeableListener.GetIfCanMerge();
                        OnStartMerge += mergeableListener.StartMerge;
                        OnCancelMerge += mergeableListener.CancelMerge;
                        OnFinaliseMerge += mergeableListener.FinaliseMerge;
                    }
                    else
                    {
                        throw new Exception($"Listener {listener.GetType().Name} is not a mergeable listener!");
                    }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !_currentReorderableGrid.CanGrabElements) return;

            _isDragging = true;
            _canvasGroup.blocksRaycasts = false;
            OnStartedDrag?.Invoke();

            _currentSiblingIndex = _rectTransform.GetSiblingIndex();
            _listHoveringOver = _currentReorderableGrid;

            
            // Create an empty space where the current plank is
            var emptySpace = new GameObject("Empty Space");
            _emptySpaceRect = emptySpace.AddComponent<RectTransform>();
            _emptySpaceRect.SetParent(_currentReorderableGrid.Content, false);
            _emptySpaceRect.SetSiblingIndex(_currentSiblingIndex);
            _emptySpaceRect.sizeDelta = _rectTransform.sizeDelta;
            emptySpace.AddComponent<LayoutElement>();
            
            // Move this plank out of the content area
            _rectTransform.SetParent(_currentReorderableGrid.DraggingArea);
            _rectTransform.SetAsLastSibling();

            _justSpawnedFake = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;
            
            // Little hack - wait one frame for the empty space object to register
            if (_justSpawnedFake)
            {
                _justSpawnedFake = false;
                return;
            }

            // Move to cursor
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), 
                eventData.position, _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? _canvas.worldCamera : null, out var worldPoint);
            _rectTransform.position = worldPoint;
            
            // Check everything under the cursor to find a MergeableList
            EventSystem.current.RaycastAll(eventData, _raycastResults);

            _listHoveringOver =  _raycastResults
                .Select(r => r.gameObject.GetComponent<ReorderableGrid>())
                .FirstOrDefault(r => r is not null);

            if (IsMergeable)
            {
                var oldElement = _elementToMergeWith;
                _elementToMergeWith = _raycastResults
                    .Select(r => r.gameObject.GetComponent<ReorderableElement>())
                    .FirstOrDefault(r => r is not null && r != this && r.IsMergeable && TryMergeInto.Invoke(r));

                if (_elementToMergeWith is not null && _listHoveringOver == _currentReorderableGrid)
                {
                    if (oldElement != _elementToMergeWith)
                    {
                        _currentlyMerging = true;
                        OnStartMerge?.Invoke();
                    }
                }
                else if (_currentlyMerging)
                {
                    _currentlyMerging = false;
                    OnCancelMerge?.Invoke();
                }
            }
            
            // Nothing found or not droppable - put the fake element outside
            if (_listHoveringOver == null || !_listHoveringOver.CanDropElements)
            {
                _emptySpaceRect.SetParent(_currentReorderableGrid.DraggingArea);
                return;
            }
            
            _currentReorderableGrid = _listHoveringOver;

            if (_currentlyMerging)
            {
                _emptySpaceRect.SetParent(_currentReorderableGrid.DraggingArea);
                return;
            }
            
            // Update the parent if we've changed lists
            if (_emptySpaceRect.parent != _listHoveringOver.Content)
            {
                OnHoveringOverList?.Invoke(_listHoveringOver);
                _emptySpaceRect.SetParent(_listHoveringOver.Content, false);
            }

            // Put the empty space in the right place
            var distanceOfClosestElement = float.PositiveInfinity;
            var closestPlankSiblingIndex = 0;

            for (var i = 0; i < _listHoveringOver.Content.childCount; i++)
            {
                var rectPosition = _listHoveringOver.Content.GetChild(i).GetComponent<RectTransform>().position;
                var distance = Mathf.Abs
                    (rectPosition.x - worldPoint.x) + Mathf.Abs(rectPosition.y - worldPoint.y);

                if (distance < distanceOfClosestElement)
                {
                    distanceOfClosestElement = distance;
                    closestPlankSiblingIndex = i;
                }
            }
            
            _emptySpaceRect.SetSiblingIndex(closestPlankSiblingIndex);
            _currentSiblingIndex = closestPlankSiblingIndex;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging || eventData.button != PointerEventData.InputButton.Left) return;

            if (_currentlyMerging)
            {
                OnFinaliseMerge?.Invoke(_elementToMergeWith);
                Destroy(_emptySpaceRect.gameObject);
                Destroy(gameObject);
                return;
            }
            
            _isDragging = false;
            OnEndedDrag?.Invoke();

            var oldList = _currentReorderableGrid;
            var oldSiblingIndex = _currentSiblingIndex;

            if (_listHoveringOver != null)
            {
                _currentReorderableGrid = _listHoveringOver;
                _currentSiblingIndex = _emptySpaceRect.GetSiblingIndex();
            }

            _rectTransform.SetParent(_currentReorderableGrid.Content, false);
            _rectTransform.SetSiblingIndex(_currentSiblingIndex);

            Destroy(_emptySpaceRect.gameObject);
            _canvasGroup.blocksRaycasts = true;

            if (_listHoveringOver != null && (oldSiblingIndex != _currentSiblingIndex || _currentReorderableGrid != oldList))
            {
                _currentReorderableGrid.ElementOrderAlteredByDrag();
            }
        }
    }
}