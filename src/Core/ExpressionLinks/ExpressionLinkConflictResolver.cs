namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkConflictResolver
    {
        internal static bool CandidateWins(
            int currentPriority,
            float currentWeight,
            int candidatePriority,
            float candidateWeight)
        {
            if (candidatePriority != currentPriority)
            {
                return candidatePriority > currentPriority;
            }

            return SanitizeWeight(candidateWeight) > SanitizeWeight(currentWeight);
        }

        private static float SanitizeWeight(float weight)
        {
            if (float.IsNaN(weight) || float.IsInfinity(weight))
            {
                return 0f;
            }

            return weight;
        }
    }
}
