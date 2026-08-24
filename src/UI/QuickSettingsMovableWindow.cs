using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsMovableWindow :
        UIBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler
    {
        private Vector2 _cachedDragPosition;
        private Vector2 _cachedMousePosition;
        private bool _pointerDownCalled;

        internal event Action<PointerEventData> PointerDown;
        internal event Action<PointerEventData> Dragged;
        internal event Action<PointerEventData> PointerUp;

        internal RectTransform ToDrag { get; set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (ToDrag == null)
            {
                return;
            }

            _pointerDownCalled = true;
            _cachedDragPosition = ToDrag.position;
            _cachedMousePosition = Input.mousePosition;

            Action<PointerEventData> handler = PointerDown;
            if (handler != null)
            {
                handler(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_pointerDownCalled || ToDrag == null)
            {
                return;
            }

            Vector3 newPosition = _cachedDragPosition +
                ((Vector2)Input.mousePosition - _cachedMousePosition);
            ToDrag.position = newPosition;

            Action<PointerEventData> handler = Dragged;
            if (handler != null)
            {
                handler(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pointerDownCalled)
            {
                return;
            }

            _pointerDownCalled = false;
            Action<PointerEventData> handler = PointerUp;
            if (handler != null)
            {
                handler(eventData);
            }
        }

        internal void Cancel()
        {
            _pointerDownCalled = false;
        }

        protected override void OnDisable()
        {
            Cancel();
            base.OnDisable();
        }
    }
}
