using System.Globalization;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed partial class QuickSettingsWindow
    {
        private readonly QuickSettingsHeaderDragStateMachine
            _headerDragStateMachine =
                new QuickSettingsHeaderDragStateMachine();

        internal void UpdateInput()
        {
            if (!_visible)
            {
                StopHeaderDrag();
                return;
            }

            ClampWindowSize();

            Vector3 pointerInput = Input.mousePosition;
            QuickSettingsHeaderDragFrame frame =
                new QuickSettingsHeaderDragFrame(
                    _visible,
                    Application.isFocused,
                    Input.GetMouseButtonDown(0),
                    Input.GetMouseButton(0),
                    Input.GetMouseButtonUp(0),
                    pointerInput.x,
                    pointerInput.y,
                    _windowRect.x,
                    _windowRect.y,
                    _windowRect.width,
                    Screen.width,
                    Screen.height,
                    ScreenMargin,
                    HeaderHeight,
                    MinimumVisibleHeaderWidth);
            QuickSettingsHeaderDragResult result =
                _headerDragStateMachine.Step(frame);

            _windowRect.x = result.PanelX;
            _windowRect.y = result.PanelY;
            LogHeaderDragTransition(result, pointerInput);
        }

        internal void HandleApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                return;
            }

            StopHeaderDrag("focus lost");
        }

        private void StopHeaderDrag()
        {
            StopHeaderDrag("panel hidden");
        }

        private void StopHeaderDrag(string reason)
        {
            QuickSettingsHeaderDragResult result =
                _headerDragStateMachine.Cancel(
                    _windowRect.x,
                    _windowRect.y);
            if (result.Transition !=
                QuickSettingsHeaderDragTransition.Cancelled)
            {
                return;
            }

            LogHeaderDragEnd(
                result,
                Input.mousePosition,
                reason);
        }

        private void LogHeaderDragTransition(
            QuickSettingsHeaderDragResult result,
            Vector3 pointerInput)
        {
            if (result.Transition ==
                QuickSettingsHeaderDragTransition.Began)
            {
                if (HeaderDragLoggingEnabled())
                {
                    Plugin.Log.LogInfo(string.Format(
                        CultureInfo.InvariantCulture,
                        "Quick Settings drag begin: pointerInput=({0:0.0}, " +
                        "{1:0.0}), panelStart=({2:0.0}, {3:0.0}).",
                        _headerDragStateMachine.PointerStartX,
                        _headerDragStateMachine.PointerStartY,
                        _headerDragStateMachine.PanelStartX,
                        _headerDragStateMachine.PanelStartY));
                }

                return;
            }

            if (result.Transition ==
                QuickSettingsHeaderDragTransition.Ended)
            {
                LogHeaderDragEnd(result, pointerInput, "released");
                return;
            }

            if (result.Transition ==
                QuickSettingsHeaderDragTransition.Cancelled)
            {
                string reason = !_visible
                    ? "panel hidden"
                    : !Application.isFocused
                        ? "focus lost"
                        : "cancelled";
                LogHeaderDragEnd(result, pointerInput, reason);
            }
        }

        private static void LogHeaderDragEnd(
            QuickSettingsHeaderDragResult result,
            Vector3 pointerInput,
            string reason)
        {
            if (!HeaderDragLoggingEnabled())
            {
                return;
            }

            Plugin.Log.LogInfo(string.Format(
                CultureInfo.InvariantCulture,
                "Quick Settings drag end ({0}): pointerInput=({1:0.0}, " +
                "{2:0.0}), panel=({3:0.0}, {4:0.0}).",
                reason,
                pointerInput.x,
                pointerInput.y,
                result.PanelX,
                result.PanelY));
        }

        private static bool HeaderDragLoggingEnabled()
        {
            return Plugin.Log != null &&
                PluginConfig.DebugLogging != null &&
                PluginConfig.DebugLogging.Value;
        }
    }
}
