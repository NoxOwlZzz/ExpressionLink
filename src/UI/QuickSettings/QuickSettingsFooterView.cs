using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsFooterView
    {
        private readonly Action _apply;
        private readonly Action _discard;
        private readonly Action _writeDiagnostics;
        private readonly Action _close;
        private bool _showTroubleshooting;

        internal QuickSettingsFooterView(
            Action apply,
            Action discard,
            Action writeDiagnostics,
            Action close)
        {
            _apply = Require(apply, "apply");
            _discard = Require(discard, "discard");
            _writeDiagnostics = Require(
                writeDiagnostics,
                "writeDiagnostics");
            _close = Require(close, "close");
        }

        internal bool DrawActions(
            bool showSettingsActions,
            bool compactLayout)
        {
            if (compactLayout && showSettingsActions)
            {
                QuickSettingsGui.BeginHorizontal();
                DrawSettingsActions();
                QuickSettingsGui.EndHorizontal();

                QuickSettingsGui.BeginHorizontal();
                bool opened = DrawHelpAndCloseActions();
                QuickSettingsGui.EndHorizontal();
                return opened;
            }

            QuickSettingsGui.BeginHorizontal();
            if (showSettingsActions)
            {
                DrawSettingsActions();
            }

            bool troubleshootingOpened = DrawHelpAndCloseActions();
            QuickSettingsGui.EndHorizontal();
            return troubleshootingOpened;
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
                "Use Save settings for plugin options. Character mappings " +
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
            if (QuickSettingsGui.Button("Save settings"))
            {
                _apply();
            }

            if (QuickSettingsGui.Button("Discard changes"))
            {
                _discard();
            }
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
