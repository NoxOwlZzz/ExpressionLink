using UnityEngine;
using UnityEngine.EventSystems;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsHeaderDragSurface :
        UIBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler
    {
        private readonly QuickSettingsHeaderDragGesture _gesture =
            new QuickSettingsHeaderDragGesture();
        private QuickSettingsWindowHost _host;

        internal void Initialize(QuickSettingsWindowHost host)
        {
            _host = host;
            ResetGesture();
        }

        internal void Detach()
        {
            ResetGesture();
            _host = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_host == null ||
                !_host.Visible ||
                eventData == null ||
                eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            QuickSettingsPoint pointer = ToGuiPoint(eventData.position);
            if (!_gesture.Begin(eventData.pointerId, pointer, _host.Bounds))
            {
                return;
            }

            eventData.Use();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_host == null ||
                !_host.Visible ||
                eventData == null)
            {
                return;
            }

            QuickSettingsPoint pointer = ToGuiPoint(eventData.position);
            QuickSettingsPoint windowPosition;
            if (!_gesture.TryGetWindowPosition(
                eventData.pointerId,
                pointer,
                out windowPosition))
            {
                return;
            }

            _host.MoveTo(windowPosition.X, windowPosition.Y);
            eventData.Use();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData == null ||
                !_gesture.End(eventData.pointerId))
            {
                return;
            }

            eventData.Use();
        }

        protected override void OnDisable()
        {
            ResetGesture();
            base.OnDisable();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ResetGesture();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                ResetGesture();
            }
        }

        private static QuickSettingsPoint ToGuiPoint(Vector2 pointerPosition)
        {
            return QuickSettingsWindowGeometry.PointerToGui(
                pointerPosition.x,
                pointerPosition.y,
                Screen.height);
        }

        private void ResetGesture()
        {
            _gesture.Cancel();
        }
    }
}
