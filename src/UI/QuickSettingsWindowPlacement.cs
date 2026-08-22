using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class QuickSettingsWindowPlacement
    {
        internal static float ClampHorizontal(
            float x,
            float windowWidth,
            float screenWidth,
            float screenMargin,
            float minimumVisibleHeaderWidth)
        {
            return ClampVisibleSpan(
                x,
                windowWidth,
                screenWidth,
                minimumVisibleHeaderWidth,
                screenMargin);
        }

        internal static float ClampVertical(
            float y,
            float screenHeight,
            float screenMargin,
            float headerHeight)
        {
            return ClampVisibleSpan(
                y,
                headerHeight,
                screenHeight,
                headerHeight,
                screenMargin);
        }

        internal static float CalculateDraggedHorizontal(
            float panelStart,
            float pointerStart,
            float pointerCurrent)
        {
            return CalculateDraggedCoordinate(
                panelStart,
                pointerCurrent,
                pointerStart);
        }

        internal static float CalculateDraggedVerticalFromBottomOrigin(
            float panelStart,
            float pointerStart,
            float pointerCurrent)
        {
            return CalculateDraggedCoordinate(
                panelStart,
                pointerStart,
                pointerCurrent);
        }

        internal static float ConvertInputYToGui(
            float viewportHeight,
            float pointerInputY)
        {
            viewportHeight = SanitizeLength(viewportHeight);
            pointerInputY = SanitizePosition(pointerInputY);
            float result = viewportHeight - pointerInputY;
            return float.IsNaN(result) || float.IsInfinity(result)
                ? 0f
                : result;
        }

        private static float CalculateDraggedCoordinate(
            float panelStart,
            float positivePointerCoordinate,
            float negativePointerCoordinate)
        {
            panelStart = SanitizePosition(panelStart);
            positivePointerCoordinate = SanitizePosition(
                positivePointerCoordinate);
            negativePointerCoordinate = SanitizePosition(
                negativePointerCoordinate);
            float result = panelStart + positivePointerCoordinate -
                negativePointerCoordinate;
            return float.IsNaN(result) || float.IsInfinity(result)
                ? panelStart
                : result;
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
            float maximumMargin =
                Math.Max(0f, (viewportLength - visibleLength) * 0.5f);
            float effectiveMargin = Math.Min(margin, maximumMargin);
            float minimumPosition =
                effectiveMargin + visibleLength - spanLength;
            float maximumPosition =
                viewportLength - effectiveMargin - visibleLength;

            if (position < minimumPosition)
            {
                return minimumPosition;
            }

            if (position > maximumPosition)
            {
                return maximumPosition;
            }

            return position;
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
