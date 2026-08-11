using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class EyeCustomizationMapper
    {
        internal const float IrisYMaximum = 0.5f;
        internal const float IrisSizeMaximum = 1f;
        internal const float MaximumWeight = 100f;

        internal static float MapIrisY(float value, float maximumWeight)
        {
            return MapOneSided(value, IrisYMaximum, maximumWeight);
        }

        internal static float MapIrisSize(float value, float maximumWeight)
        {
            return MapOneSided(value, IrisSizeMaximum, maximumWeight);
        }

        internal static float MapOneSided(
            float value,
            float sourceMaximum,
            float maximumWeight)
        {
            if (!IsFinite(value) || value <= 0f ||
                !IsFinite(sourceMaximum) || sourceMaximum <= 0f ||
                !IsFinite(maximumWeight) || maximumWeight <= 0f)
            {
                return 0f;
            }

            if (value > sourceMaximum)
            {
                value = sourceMaximum;
            }

            if (maximumWeight > MaximumWeight)
            {
                maximumWeight = MaximumWeight;
            }

            return (value / sourceMaximum) * maximumWeight;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
