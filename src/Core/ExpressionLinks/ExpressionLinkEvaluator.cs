namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkEvaluator
    {
        internal static float Evaluate(
            ExpressionLinkDefinition definition,
            float sourceWeight)
        {
            if (definition == null)
            {
                return 0f;
            }

            float minimumOutput = SanitizeWeight(definition.OutputMin);
            float maximumOutput = SanitizeWeight(definition.OutputMax);
            if (float.IsNaN(sourceWeight) || float.IsInfinity(sourceWeight))
            {
                return minimumOutput;
            }

            if (definition.Mode == ExpressionLinkMode.Binary)
            {
                return sourceWeight > definition.Threshold
                    ? maximumOutput
                    : minimumOutput;
            }

            if (definition.Mode != ExpressionLinkMode.FollowSource ||
                float.IsNaN(definition.InputMin) ||
                float.IsInfinity(definition.InputMin) ||
                float.IsNaN(definition.InputMax) ||
                float.IsInfinity(definition.InputMax) ||
                definition.InputMax <= definition.InputMin)
            {
                return minimumOutput;
            }

            float progress =
                (sourceWeight - definition.InputMin) /
                (definition.InputMax - definition.InputMin);
            progress = Clamp(progress, 0f, 1f);
            return Clamp(
                minimumOutput + ((maximumOutput - minimumOutput) * progress),
                ExpressionLinkLimits.MinimumBlendshapeWeight,
                ExpressionLinkLimits.MaximumBlendshapeWeight);
        }

        private static float SanitizeWeight(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 0f;
            }

            return Clamp(
                value,
                ExpressionLinkLimits.MinimumBlendshapeWeight,
                ExpressionLinkLimits.MaximumBlendshapeWeight);
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
