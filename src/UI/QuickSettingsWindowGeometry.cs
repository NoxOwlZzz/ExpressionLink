using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal struct QuickSettingsPoint
    {
        internal QuickSettingsPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        internal float X { get; private set; }
        internal float Y { get; private set; }
    }

    internal struct QuickSettingsWindowBounds
    {
        internal QuickSettingsWindowBounds(
            float x,
            float y,
            float width,
            float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        internal float X { get; private set; }
        internal float Y { get; private set; }
        internal float Width { get; private set; }
        internal float Height { get; private set; }

        internal QuickSettingsWindowBounds WithPosition(float x, float y)
        {
            return new QuickSettingsWindowBounds(x, y, Width, Height);
        }

        internal QuickSettingsWindowBounds WithSize(
            float width,
            float height)
        {
            return new QuickSettingsWindowBounds(X, Y, width, height);
        }
    }

    internal struct QuickSettingsWindowConstraints
    {
        internal QuickSettingsWindowConstraints(
            float minimumWidth,
            float minimumHeight,
            float screenMargin,
            float headerHeight,
            float minimumVisibleHeaderWidth)
        {
            MinimumWidth = minimumWidth;
            MinimumHeight = minimumHeight;
            ScreenMargin = screenMargin;
            HeaderHeight = headerHeight;
            MinimumVisibleHeaderWidth = minimumVisibleHeaderWidth;
        }

        internal float MinimumWidth { get; private set; }
        internal float MinimumHeight { get; private set; }
        internal float ScreenMargin { get; private set; }
        internal float HeaderHeight { get; private set; }
        internal float MinimumVisibleHeaderWidth { get; private set; }
    }

    internal static class QuickSettingsWindowGeometry
    {
        internal static QuickSettingsWindowBounds FitToViewport(
            QuickSettingsWindowBounds bounds,
            float viewportWidth,
            float viewportHeight,
            QuickSettingsWindowConstraints constraints)
        {
            viewportWidth = SanitizeLength(viewportWidth);
            viewportHeight = SanitizeLength(viewportHeight);

            float margin = SanitizeLength(constraints.ScreenMargin);
            float minimumWidth = SanitizeLength(
                constraints.MinimumWidth);
            float minimumHeight = SanitizeLength(
                constraints.MinimumHeight);
            float availableWidth = Math.Max(
                1f,
                viewportWidth - (margin * 2f));
            float availableHeight = Math.Max(
                1f,
                viewportHeight - (margin * 2f));
            minimumWidth = Math.Min(minimumWidth, availableWidth);
            minimumHeight = Math.Min(minimumHeight, availableHeight);

            QuickSettingsWindowBounds sized = bounds.WithSize(
                Clamp(
                    SanitizeLength(bounds.Width),
                    minimumWidth,
                    availableWidth),
                Clamp(
                    SanitizeLength(bounds.Height),
                    minimumHeight,
                    availableHeight));
            return KeepHeaderAccessible(
                sized,
                viewportWidth,
                viewportHeight,
                constraints);
        }

        internal static QuickSettingsWindowBounds KeepHeaderAccessible(
            QuickSettingsWindowBounds bounds,
            float viewportWidth,
            float viewportHeight,
            QuickSettingsWindowConstraints constraints)
        {
            float x = ClampVisibleSpan(
                bounds.X,
                bounds.Width,
                viewportWidth,
                constraints.MinimumVisibleHeaderWidth,
                constraints.ScreenMargin);
            float y = ClampVisibleSpan(
                bounds.Y,
                constraints.HeaderHeight,
                viewportHeight,
                constraints.HeaderHeight,
                constraints.ScreenMargin);
            return bounds.WithPosition(x, y);
        }

        internal static QuickSettingsPoint PointerToGui(
            float pointerX,
            float pointerInputY,
            float viewportHeight)
        {
            float x = SanitizePosition(pointerX);
            float y = SanitizeLength(viewportHeight) -
                SanitizePosition(pointerInputY);
            return new QuickSettingsPoint(
                x,
                SanitizePosition(y));
        }

        internal static bool AreEqual(
            QuickSettingsWindowBounds left,
            QuickSettingsWindowBounds right)
        {
            return left.X == right.X &&
                left.Y == right.Y &&
                left.Width == right.Width &&
                left.Height == right.Height;
        }

        private static float ClampVisibleSpan(
            float position,
            float spanLength,
            float viewportLength,
            float requiredVisibleLength,
            float margin)
        {
            position = SanitizePosition(position);
            spanLength = SanitizeLength(spanLength);
            viewportLength = SanitizeLength(viewportLength);
            requiredVisibleLength = SanitizeLength(requiredVisibleLength);
            margin = SanitizeLength(margin);

            if (viewportLength <= 0f)
            {
                return 0f;
            }

            float visibleLength = Math.Min(
                requiredVisibleLength,
                Math.Min(spanLength, viewportLength));
            float maximumMargin = Math.Max(
                0f,
                (viewportLength - visibleLength) * 0.5f);
            float effectiveMargin = Math.Min(margin, maximumMargin);
            float minimumPosition =
                effectiveMargin + visibleLength - spanLength;
            float maximumPosition =
                viewportLength - effectiveMargin - visibleLength;
            return Clamp(position, minimumPosition, maximumPosition);
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }

        private static float SanitizePosition(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }

        private static float SanitizeLength(float value)
        {
            return float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value < 0f ? 0f : value;
        }
    }
}
