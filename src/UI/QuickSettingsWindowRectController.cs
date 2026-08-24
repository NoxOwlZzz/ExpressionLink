using System;
using UnityEngine;
using UnityEngine.UI;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsWindowRectController
    {
        private readonly RectTransform _mainPanel;
        private readonly CanvasScaler _scaler;
        private readonly QuickSettingsWindowConstraints _constraints;
        private readonly float _preferredWidth;
        private readonly float _preferredHeight;
        private float _viewportWidth;
        private float _viewportHeight;

        internal QuickSettingsWindowRectController(
            RectTransform mainPanel,
            CanvasScaler scaler,
            QuickSettingsWindowBounds initialBounds,
            QuickSettingsWindowConstraints constraints,
            float viewportWidth,
            float viewportHeight)
        {
            if (mainPanel == null)
            {
                throw new ArgumentNullException("mainPanel");
            }

            if (scaler == null)
            {
                throw new ArgumentNullException("scaler");
            }

            _mainPanel = mainPanel;
            _scaler = scaler;
            _constraints = constraints;
            _preferredWidth = SanitizePreferredLength(
                initialBounds.Width,
                constraints.MinimumWidth);
            _preferredHeight = SanitizePreferredLength(
                initialBounds.Height,
                constraints.MinimumHeight);
            _viewportWidth = SanitizeViewportLength(viewportWidth);
            _viewportHeight = SanitizeViewportLength(viewportHeight);
            ApplyReferenceResolution();

            ApplyBounds(
                QuickSettingsWindowGeometry.FitToViewport(
                    initialBounds.WithSize(
                        _preferredWidth,
                        _preferredHeight),
                    _viewportWidth,
                    _viewportHeight,
                    _constraints));
        }

        internal QuickSettingsWindowBounds Bounds
        {
            get
            {
                Vector2 position = _mainPanel.anchoredPosition;
                Vector2 size = _mainPanel.sizeDelta;
                return new QuickSettingsWindowBounds(
                    position.x,
                    -position.y,
                    size.x,
                    size.y);
            }
        }

        internal bool RefreshViewport(float width, float height)
        {
            width = SanitizeViewportLength(width);
            height = SanitizeViewportLength(height);
            if (_viewportWidth == width && _viewportHeight == height)
            {
                return false;
            }

            QuickSettingsWindowBounds current = Bounds;
            _viewportWidth = width;
            _viewportHeight = height;
            ApplyReferenceResolution();
            Canvas.ForceUpdateCanvases();
            return ApplyBounds(
                QuickSettingsWindowGeometry.FitToViewport(
                    current.WithSize(
                        _preferredWidth,
                        _preferredHeight),
                    _viewportWidth,
                    _viewportHeight,
                    _constraints));
        }

        internal bool ClampDrag()
        {
            return ApplyBounds(
                QuickSettingsWindowGeometry.KeepHeaderAccessible(
                    Bounds,
                    _viewportWidth,
                    _viewportHeight,
                    _constraints));
        }

        private bool ApplyBounds(QuickSettingsWindowBounds bounds)
        {
            if (QuickSettingsWindowGeometry.AreEqual(Bounds, bounds))
            {
                return false;
            }

            _mainPanel.sizeDelta = new Vector2(
                bounds.Width,
                bounds.Height);
            _mainPanel.anchoredPosition = new Vector2(
                bounds.X,
                -bounds.Y);
            return true;
        }

        private void ApplyReferenceResolution()
        {
            _scaler.referenceResolution = new Vector2(
                Math.Max(1f, _viewportWidth),
                Math.Max(1f, _viewportHeight));
        }

        private static float SanitizePreferredLength(
            float value,
            float fallback)
        {
            if (!float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value > 0f)
            {
                return value;
            }

            return Math.Max(
                1f,
                SanitizeViewportLength(fallback));
        }

        private static float SanitizeViewportLength(float value)
        {
            return float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value < 0f ? 0f : value;
        }
    }
}
