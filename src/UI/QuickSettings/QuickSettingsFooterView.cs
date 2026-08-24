using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsFooterView
    {
        private readonly Action _apply;
        private readonly Action _discard;
        private readonly Action _writeDiagnostics;
        private readonly Func<bool> _hasUnsavedSettings;
        private readonly Action _close;
        private bool _showTroubleshooting;

        internal QuickSettingsFooterView(
            Action apply,
            Action discard,
            Action writeDiagnostics,
            Func<bool> hasUnsavedSettings,
            Action close)
        {
            _apply = Require(apply, "apply");
            _discard = Require(discard, "discard");
            _writeDiagnostics = Require(
                writeDiagnostics,
                "writeDiagnostics");
            _hasUnsavedSettings = Require(
                hasUnsavedSettings,
                "hasUnsavedSettings");
            _close = Require(close, "close");
        }

        internal bool DrawActions(
            bool showSettingsActions,
            bool compactLayout)
        {
            QuickSettingsGui.BeginFooter();
            if (showSettingsActions && _hasUnsavedSettings())
            {
                QuickSettingsGui.StatusBadge("Unsaved plugin settings");
            }

            bool opened;
            if (compactLayout && showSettingsActions)
            {
                QuickSettingsGui.BeginHorizontal();
                DrawSettingsActions();
                QuickSettingsGui.EndHorizontal();

                QuickSettingsGui.BeginHorizontal();
                opened = DrawHelpAndCloseActions();
                QuickSettingsGui.EndHorizontal();
            }
            else
            {
                QuickSettingsGui.BeginHorizontal();
                if (showSettingsActions)
                {
                    DrawSettingsActions();
                }

                opened = DrawHelpAndCloseActions();
                QuickSettingsGui.EndHorizontal();
            }

            QuickSettingsGui.EndFooter();
            return opened;
        }

        internal void DrawTroubleshooting()
        {
            if (!_showTroubleshooting)
            {
                return;
            }

            QuickSettingsGui.Space(6f);
            QuickSettingsGui.Heading("Getting started");
            QuickSettingsGui.Help(
                "Use Apply plugin settings for plugin options. Game expressions " +
                "and Custom links have their own Save buttons.");
            QuickSettingsGui.Heading("Troubleshooting");
            QuickSettingsGui.Help(
                "Create a report when a shape cannot be found or does not move.");
            if (QuickSettingsGui.Button("Create debug report"))
            {
                _writeDiagnostics();
            }
        }

        private void DrawSettingsActions()
        {
            bool previousGuiEnabled = UnityEngine.GUI.enabled;
            UnityEngine.GUI.enabled =
                previousGuiEnabled && _hasUnsavedSettings();
            if (QuickSettingsGui.PrimaryButton("Apply plugin settings"))
            {
                _apply();
            }

            if (QuickSettingsGui.Button("Discard changes"))
            {
                _discard();
            }

            UnityEngine.GUI.enabled = previousGuiEnabled;
        }

        private bool DrawHelpAndCloseActions()
        {
            bool opened = false;
            if (QuickSettingsGui.Button(
                    _showTroubleshooting
                        ? "Hide help"
                        : "Help"))
            {
                _showTroubleshooting = !_showTroubleshooting;
                opened = _showTroubleshooting;
            }

            if (QuickSettingsGui.Button("Close"))
            {
                _close();
            }

            return opened;
        }

        private static T Require<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }
    }
}
