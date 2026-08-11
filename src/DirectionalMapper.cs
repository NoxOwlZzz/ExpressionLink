using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class DirectionalMapper
    {
        public static float MapDirectional(
            float absoluteInput,
            float deadZone,
            float inputLimit,
            float maxWeight,
            float gamma)
        {
            if (!IsFinite(absoluteInput) ||
                !IsFinite(deadZone) ||
                !IsFinite(inputLimit) ||
                !IsFinite(maxWeight) ||
                !IsFinite(gamma))
            {
                return 0f;
            }

            absoluteInput = Clamp(absoluteInput, 0f, 1f);
            deadZone = Clamp(deadZone, 0f, 1f);
            inputLimit = Clamp(inputLimit, 0f, 1f);
            maxWeight = Clamp(maxWeight, 0f, 100f);

            if (absoluteInput <= deadZone || inputLimit <= deadZone)
            {
                return 0f;
            }

            if (gamma <= 0f)
            {
                gamma = 1f;
            }

            float normalized = Clamp((absoluteInput - deadZone) / (inputLimit - deadZone), 0f, 1f);
            if (gamma == 1f || normalized == 1f)
            {
                return Clamp(normalized * maxWeight, 0f, 100f);
            }

            double curvedDouble = Math.Pow(normalized, gamma);
            if (double.IsNaN(curvedDouble) || double.IsInfinity(curvedDouble))
            {
                return 0f;
            }

            float result = (float)curvedDouble * maxWeight;
            return IsFinite(result) ? Clamp(result, 0f, 100f) : 0f;
        }

        public static void ClampToUnitCircle(ref float x, ref float y)
        {
            if (!IsFinite(x) || !IsFinite(y))
            {
                x = 0f;
                y = 0f;
                return;
            }

            double magnitudeSquared = (double)x * x + (double)y * y;
            if (magnitudeSquared <= 1d)
            {
                return;
            }

            double magnitude = Math.Sqrt(magnitudeSquared);
            if (magnitude <= 0d || double.IsNaN(magnitude) || double.IsInfinity(magnitude))
            {
                x = 0f;
                y = 0f;
                return;
            }

            x = (float)(x / magnitude);
            y = (float)(y / magnitude);
        }

        public static float SmoothTowards(float current, float target, float speed, float deltaTime)
        {
            return ApplySmoothingAlpha(
                current,
                target,
                CalculateSmoothingAlpha(speed, deltaTime));
        }

        public static float CalculateSmoothingAlpha(float speed, float deltaTime)
        {
            if (!IsFinite(speed) || speed <= 0f)
            {
                return 1f;
            }

            if (!IsFinite(deltaTime) || deltaTime <= 0f)
            {
                return 0f;
            }

            double alphaDouble = 1d - Math.Exp(-(double)speed * deltaTime);
            if (double.IsNaN(alphaDouble) || double.IsInfinity(alphaDouble))
            {
                return 1f;
            }

            return Clamp((float)alphaDouble, 0f, 1f);
        }

        public static float ApplySmoothingAlpha(
            float current,
            float target,
            float alpha)
        {
            if (!IsFinite(target))
            {
                target = 0f;
            }

            if (!IsFinite(current))
            {
                current = target;
            }

            if (!IsFinite(alpha))
            {
                return target;
            }

            alpha = Clamp(alpha, 0f, 1f);
            float result = current + (target - current) * alpha;
            return IsFinite(result) ? result : target;
        }

        public static float Clamp01(float value)
        {
            return IsFinite(value) ? Clamp(value, 0f, 1f) : 0f;
        }

        public static float SanitizeSignedUnit(float value)
        {
            return IsFinite(value) ? Clamp(value, -1f, 1f) : 0f;
        }

        public static float CenterSignedInput(float value, float center)
        {
            if (!IsFinite(value))
            {
                return 0f;
            }

            if (!IsFinite(center))
            {
                center = 0f;
            }

            return SanitizeSignedUnit(value - center);
        }

        public static float EyeHorizontalToCommon(float rate)
        {
            return SanitizeSignedUnit(rate);
        }

        public static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }
    }
}
