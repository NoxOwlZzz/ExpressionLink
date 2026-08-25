using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    [Flags]
    internal enum QuickSettingsCornerMask
    {
        None = 0,
        BottomLeft = 1,
        BottomRight = 2,
        TopLeft = 4,
        TopRight = 8,
        Bottom = BottomLeft | BottomRight,
        Top = TopLeft | TopRight,
        All = Bottom | Top
    }

    internal static class QuickSettingsRoundedTextureFactory
    {
        private const int SamplesPerAxis = 4;
        private const int SamplesPerPixel =
            SamplesPerAxis * SamplesPerAxis;

        internal static Texture2D Create(
            string name,
            Color fill,
            Color outline,
            float radius,
            QuickSettingsCornerMask corners)
        {
            int size = QuickSettingsTheme.Metrics.RoundedTextureSize;
            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false);
            texture.name = name;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(BuildPixels(
                size,
                radius,
                QuickSettingsTheme.Metrics.OutlineWidth,
                fill,
                outline,
                corners));
            texture.Apply(false, true);
            return texture;
        }

        private static Color32[] BuildPixels(
            int size,
            float radius,
            float outlineWidth,
            Color fill,
            Color outline,
            QuickSettingsCornerMask corners)
        {
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int outerHits = 0;
                    int innerHits = 0;
                    for (int sampleY = 0;
                        sampleY < SamplesPerAxis;
                        sampleY++)
                    {
                        for (int sampleX = 0;
                            sampleX < SamplesPerAxis;
                            sampleX++)
                        {
                            float pointX = x +
                                ((sampleX + 0.5f) / SamplesPerAxis);
                            float pointY = y +
                                ((sampleY + 0.5f) / SamplesPerAxis);
                            if (IsInside(
                                    pointX,
                                    pointY,
                                    size,
                                    0f,
                                    radius,
                                    corners))
                            {
                                outerHits++;
                            }

                            if (IsInside(
                                    pointX,
                                    pointY,
                                    size,
                                    outlineWidth,
                                    Mathf.Max(0f, radius - outlineWidth),
                                    corners))
                            {
                                innerHits++;
                            }
                        }
                    }

                    pixels[(y * size) + x] = ComposePixel(
                        fill,
                        outline,
                        innerHits / (float)SamplesPerPixel,
                        Mathf.Max(0, outerHits - innerHits) /
                            (float)SamplesPerPixel);
                }
            }

            return pixels;
        }

        private static bool IsInside(
            float x,
            float y,
            float size,
            float inset,
            float radius,
            QuickSettingsCornerMask corners)
        {
            float minimum = inset;
            float maximum = size - inset;
            if (x < minimum || x > maximum ||
                y < minimum || y > maximum)
            {
                return false;
            }

            if (radius <= 0f)
            {
                return true;
            }

            if ((corners & QuickSettingsCornerMask.BottomLeft) != 0 &&
                x < minimum + radius && y < minimum + radius)
            {
                return IsInsideCorner(
                    x,
                    y,
                    minimum + radius,
                    minimum + radius,
                    radius);
            }

            if ((corners & QuickSettingsCornerMask.BottomRight) != 0 &&
                x > maximum - radius && y < minimum + radius)
            {
                return IsInsideCorner(
                    x,
                    y,
                    maximum - radius,
                    minimum + radius,
                    radius);
            }

            if ((corners & QuickSettingsCornerMask.TopLeft) != 0 &&
                x < minimum + radius && y > maximum - radius)
            {
                return IsInsideCorner(
                    x,
                    y,
                    minimum + radius,
                    maximum - radius,
                    radius);
            }

            if ((corners & QuickSettingsCornerMask.TopRight) != 0 &&
                x > maximum - radius && y > maximum - radius)
            {
                return IsInsideCorner(
                    x,
                    y,
                    maximum - radius,
                    maximum - radius,
                    radius);
            }

            return true;
        }

        private static bool IsInsideCorner(
            float x,
            float y,
            float centerX,
            float centerY,
            float radius)
        {
            float deltaX = x - centerX;
            float deltaY = y - centerY;
            return (deltaX * deltaX) + (deltaY * deltaY) <=
                radius * radius;
        }

        private static Color32 ComposePixel(
            Color fill,
            Color outline,
            float fillCoverage,
            float outlineCoverage)
        {
            float fillAlpha = fill.a * fillCoverage;
            float outlineAlpha = outline.a * outlineCoverage;
            float alpha = fillAlpha + outlineAlpha;
            if (alpha <= 0f)
            {
                return new Color32(0, 0, 0, 0);
            }

            return new Color32(
                ToByte(((fill.r * fillAlpha) +
                    (outline.r * outlineAlpha)) / alpha),
                ToByte(((fill.g * fillAlpha) +
                    (outline.g * outlineAlpha)) / alpha),
                ToByte(((fill.b * fillAlpha) +
                    (outline.b * outlineAlpha)) / alpha),
                ToByte(alpha));
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.RoundToInt(
                Mathf.Clamp01(value) * byte.MaxValue);
        }
    }
}
