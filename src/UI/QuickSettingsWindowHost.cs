using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsWindowHost
    {
        private readonly QuickSettingsWindowConstraints _constraints;
        private readonly float _preferredWidth;
        private readonly float _preferredHeight;
        private QuickSettingsWindowBounds _bounds;
        private float _viewportWidth;
        private float _viewportHeight;
        private bool _visible;

        internal QuickSettingsWindowHost(
            QuickSettingsWindowBounds initialBounds,
            float viewportWidth,
            float viewportHeight,
            QuickSettingsWindowConstraints constraints)
        {
            _constraints = constraints;
            _preferredWidth = SanitizePreferredLength(
                initialBounds.Width,
                constraints.MinimumWidth);
            _preferredHeight = SanitizePreferredLength(
                initialBounds.Height,
                constraints.MinimumHeight);
            _viewportWidth = SanitizeViewportLength(viewportWidth);
            _viewportHeight = SanitizeViewportLength(viewportHeight);
            _bounds = QuickSettingsWindowGeometry.FitToViewport(
                initialBounds.WithSize(
                    _preferredWidth,
                    _preferredHeight),
                _viewportWidth,
                _viewportHeight,
                _constraints);
        }

        internal event Action<QuickSettingsWindowBounds> BoundsChanged;
        internal event Action<bool> VisibilityChanged;

        internal QuickSettingsWindowBounds Bounds
        {
            get { return _bounds; }
        }

        internal bool Visible
        {
            get { return _visible; }
        }

        internal void Show()
        {
            SetVisible(true);
        }

        internal void Hide()
        {
            SetVisible(false);
        }

        internal void Toggle()
        {
            SetVisible(!_visible);
        }

        internal bool MoveTo(float x, float y)
        {
            QuickSettingsWindowBounds moved =
                QuickSettingsWindowGeometry.KeepHeaderAccessible(
                    _bounds.WithPosition(x, y),
                    _viewportWidth,
                    _viewportHeight,
                    _constraints);
            return SetBounds(moved);
        }

        internal bool UpdateViewport(float width, float height)
        {
            width = SanitizeViewportLength(width);
            height = SanitizeViewportLength(height);
            if (_viewportWidth == width && _viewportHeight == height)
            {
                return false;
            }

            _viewportWidth = width;
            _viewportHeight = height;
            QuickSettingsWindowBounds fitted =
                QuickSettingsWindowGeometry.FitToViewport(
                    _bounds.WithSize(
                        _preferredWidth,
                        _preferredHeight),
                    _viewportWidth,
                    _viewportHeight,
                    _constraints);
            return SetBounds(fitted);
        }

        private void SetVisible(bool visible)
        {
            if (_visible == visible)
            {
                return;
            }

            _visible = visible;
            Action<bool> handler = VisibilityChanged;
            if (handler != null)
            {
                handler(_visible);
            }
        }

        private bool SetBounds(QuickSettingsWindowBounds bounds)
        {
            if (QuickSettingsWindowGeometry.AreEqual(_bounds, bounds))
            {
                return false;
            }

            _bounds = bounds;
            Action<QuickSettingsWindowBounds> handler = BoundsChanged;
            if (handler != null)
            {
                handler(_bounds);
            }

            return true;
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

            return SanitizeViewportLength(fallback);
        }

        private static float SanitizeViewportLength(float value)
        {
            return float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value < 0f ? 0f : value;
        }
    }
}
