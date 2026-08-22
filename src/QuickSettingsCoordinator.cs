using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsCoordinator : IDisposable
    {
        private const float PreferredWindowWidth = 500f;
        private const float PreferredWindowHeight = 520f;
        private const float MinimumWindowWidth = 360f;
        private const float MinimumWindowHeight = 300f;
        private const float ScreenMargin = 8f;
        private const float HeaderHeight = 24f;
        private const float MinimumVisibleHeaderWidth = 120f;

        private readonly QuickSettingsWindowHost _windowHost;
        private readonly QuickSettingsDragOverlay _dragOverlay;
        private readonly QuickSettingsConfigSession _configSession;
        private readonly CharacterSelectionModel _selection;
        private readonly LiveStatusPresenter _statusPresenter;
        private readonly QuickSettingsSurfaceView _surfaceView;
        private bool _disposed;

        internal QuickSettingsCoordinator(Transform owner)
        {
            QuickSettingsWindowConstraints constraints =
                new QuickSettingsWindowConstraints(
                    MinimumWindowWidth,
                    MinimumWindowHeight,
                    ScreenMargin,
                    HeaderHeight,
                    MinimumVisibleHeaderWidth);
            _windowHost = new QuickSettingsWindowHost(
                new QuickSettingsWindowBounds(
                    24f,
                    72f,
                    PreferredWindowWidth,
                    PreferredWindowHeight),
                Screen.width,
                Screen.height,
                constraints);
            _dragOverlay = new QuickSettingsDragOverlay(
                owner,
                _windowHost,
                HeaderHeight);
            _configSession = new QuickSettingsConfigSession();
            _selection = new CharacterSelectionModel();
            _statusPresenter = new LiveStatusPresenter();
            _surfaceView = new QuickSettingsSurfaceView(
                Plugin.PluginName + " " + Plugin.PluginVersion +
                " - Quick Settings",
                _selection,
                _statusPresenter,
                new MotionSettingsView(_configSession.Motion),
                new IrisSettingsView(_configSession.Iris),
                new ExpressionSettingsView(_configSession.Expressions),
                new VisibilitySettingsView(_configSession.Visibility),
                new ExpressionLinkEditorView(),
                ApplyConfiguration,
                ReloadConfiguration,
                WriteDiagnostics,
                Close);
            _selection.SelectionChanged += HandleSelectionChanged;
        }

        internal bool Visible
        {
            get { return _windowHost.Visible; }
        }

        internal void Toggle()
        {
            if (_windowHost.Visible)
            {
                Close();
                return;
            }

            _configSession.Reload();
            _dragOverlay.RefreshViewport();
            _windowHost.Show();
        }

        internal void Draw()
        {
            if (!_windowHost.Visible || _disposed)
            {
                return;
            }

            _dragOverlay.RefreshViewport();
            _surfaceView.Draw(_windowHost.Bounds);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _selection.SelectionChanged -= HandleSelectionChanged;
            _windowHost.Hide();
            _dragOverlay.Dispose();
        }

        private void ApplyConfiguration()
        {
            _configSession.Apply();
        }

        private void ReloadConfiguration()
        {
            _configSession.Reload();
        }

        private static void WriteDiagnostics()
        {
            Diagnostics.WriteReport();
        }

        private void Close()
        {
            _windowHost.Hide();
        }

        private void HandleSelectionChanged(
            EyeMotionCharacterController previous,
            EyeMotionCharacterController current)
        {
            _statusPresenter.Invalidate();
        }
    }
}
