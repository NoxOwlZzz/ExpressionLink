using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsWindowFrame : IDisposable
    {
        private QuickSettingsWindowVisual _visual;
        private QuickSettingsWindowRectController _rectController;
        private bool _disposed;

        internal QuickSettingsWindowFrame(
            Transform owner,
            QuickSettingsWindowBounds initialBounds,
            QuickSettingsWindowConstraints constraints,
            string title)
        {
            _visual = QuickSettingsWindowVisualFactory.Create(
                owner,
                title,
                constraints.HeaderHeight,
                Screen.width,
                Screen.height);
            _rectController =
                new QuickSettingsWindowRectController(
                    _visual.MainPanel,
                    _visual.Scaler,
                    initialBounds,
                    constraints,
                    Screen.width,
                    Screen.height);
            _visual.MovableWindow.Dragged += HandleDrag;
            _visual.Root.SetActive(false);
        }

        internal QuickSettingsWindowBounds Bounds
        {
            get { return _rectController.Bounds; }
        }

        internal bool Visible
        {
            get
            {
                return !_disposed &&
                    _visual != null &&
                    _visual.Root.activeSelf;
            }
        }

        internal void Show()
        {
            if (_disposed || _visual == null)
            {
                return;
            }

            RefreshViewport();
            _visual.Root.SetActive(true);
        }

        internal void Hide()
        {
            if (_disposed || _visual == null)
            {
                return;
            }

            _visual.MovableWindow.Cancel();
            _visual.Root.SetActive(false);
        }

        internal bool RefreshViewport()
        {
            return !_disposed && _rectController != null &&
                _rectController.RefreshViewport(
                    Screen.width,
                    Screen.height);
        }

        internal void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_visual != null)
            {
                _visual.MovableWindow.Dragged -= HandleDrag;
                _visual.MovableWindow.Cancel();
                _visual.MovableWindow.ToDrag = null;
                _visual.Dispose();
            }

            _rectController = null;
            _visual = null;
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        private void HandleDrag(PointerEventData eventData)
        {
            if (!_disposed && _rectController != null)
            {
                _rectController.ClampDrag();
            }
        }
    }
}
