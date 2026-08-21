using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed partial class QuickSettingsWindow
    {
        private Vector2 _headerDragStartMousePosition;
        private Vector2 _headerDragStartWindowPosition;
        private bool _headerDragActive;
        private bool _leftMouseWasDown;

        internal void UpdateInput()
        {
            bool leftMouseDown = Input.GetMouseButton(0);
            bool leftMousePressed = leftMouseDown && !_leftMouseWasDown;
            _leftMouseWasDown = leftMouseDown;

            if (!_visible)
            {
                StopHeaderDrag();
                return;
            }

            Vector2 mousePosition = GetGuiMousePosition();
            if (leftMousePressed &&
                new Rect(
                    _windowRect.x,
                    _windowRect.y,
                    _windowRect.width,
                    HeaderHeight).Contains(mousePosition))
            {
                _headerDragActive = true;
                _headerDragStartMousePosition = mousePosition;
                _headerDragStartWindowPosition = new Vector2(
                    _windowRect.x,
                    _windowRect.y);
            }

            if (!_headerDragActive)
            {
                return;
            }

            if (!leftMouseDown)
            {
                StopHeaderDrag();
                return;
            }

            _windowRect.x =
                QuickSettingsWindowPlacement.CalculateDraggedCoordinate(
                    _headerDragStartWindowPosition.x,
                    mousePosition.x,
                    _headerDragStartMousePosition.x);
            _windowRect.y =
                QuickSettingsWindowPlacement.CalculateDraggedCoordinate(
                    _headerDragStartWindowPosition.y,
                    mousePosition.y,
                    _headerDragStartMousePosition.y);
            ClampHeaderToScreen();
        }

        private static Vector2 GetGuiMousePosition()
        {
            Vector3 inputPosition = Input.mousePosition;
            return new Vector2(
                inputPosition.x,
                Screen.height - inputPosition.y);
        }

        private void StopHeaderDrag()
        {
            _headerDragActive = false;
        }
    }
}
