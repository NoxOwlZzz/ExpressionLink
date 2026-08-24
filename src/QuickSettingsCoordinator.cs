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

        private readonly Transform _owner;
        private readonly string _windowTitle;
        private readonly QuickSettingsConfigSession _configSession;
        private readonly CharacterSelectionModel _selection;
        private readonly LiveStatusPresenter _statusPresenter;
        private readonly QuickSettingsSurfaceView _surfaceView;
        private QuickSettingsWindowFrame _window;
        private bool _disposed;

        internal QuickSettingsCoordinator(Transform owner)
        {
            _owner = owner;
            _windowTitle = "Expression Link Settings";
            _configSession = new QuickSettingsConfigSession();
            _selection = new CharacterSelectionModel();
            _statusPresenter = new LiveStatusPresenter();
            _surfaceView = new QuickSettingsSurfaceView(
                HeaderHeight,
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
            get { return _window != null && _window.Visible; }
        }

        internal void Toggle()
        {
            if (_disposed)
            {
                return;
            }

            QuickSettingsWindowFrame window = EnsureWindow();
            if (window.Visible)
            {
                Close();
                return;
            }

            _configSession.ReloadIfClean();
            window.RefreshViewport();
            window.Show();
        }

        internal void Draw()
        {
            if (_window == null || !_window.Visible || _disposed)
            {
                return;
            }

            _window.RefreshViewport();
            _surfaceView.Draw(_window.Bounds);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _selection.SelectionChanged -= HandleSelectionChanged;
            if (_window != null)
            {
                _window.Dispose();
                _window = null;
            }
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
            if (_window != null)
            {
                _window.Hide();
            }
        }

        private QuickSettingsWindowFrame EnsureWindow()
        {
            if (_window != null)
            {
                return _window;
            }

            QuickSettingsWindowConstraints constraints =
                new QuickSettingsWindowConstraints(
                    MinimumWindowWidth,
                    MinimumWindowHeight,
                    ScreenMargin,
                    HeaderHeight,
                    MinimumVisibleHeaderWidth);
            _window = new QuickSettingsWindowFrame(
                _owner,
                new QuickSettingsWindowBounds(
                    24f,
                    72f,
                    PreferredWindowWidth,
                    PreferredWindowHeight),
                constraints,
                _windowTitle);
            return _window;
        }

        private void HandleSelectionChanged(
            EyeMotionCharacterController previous,
            EyeMotionCharacterController current)
        {
            _statusPresenter.Invalidate();
        }
    }
}
