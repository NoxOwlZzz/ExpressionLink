using System.Collections.Generic;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal struct ExpressionLinkConflictCandidate
    {
        internal ExpressionLinkConflictCandidate(
            ExpressionLinkDefinition definition,
            float weight) : this()
        {
            Definition = definition;
            Weight = weight;
        }

        internal ExpressionLinkDefinition Definition { get; private set; }

        internal float Weight { get; private set; }
    }

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

        internal static bool TryResolve(
            IList<ExpressionLinkConflictCandidate> candidates,
            out ExpressionLinkConflictCandidate winner)
        {
            winner = default(ExpressionLinkConflictCandidate);
            if (candidates == null || candidates.Count == 0)
            {
                return false;
            }

            bool found = false;
            for (int i = 0; i < candidates.Count; i++)
            {
                ExpressionLinkConflictCandidate candidate = candidates[i];
                if (candidate.Definition == null || !candidate.Definition.Enabled)
                {
                    continue;
                }

                if (!found || CandidateWins(
                    winner.Definition.Priority,
                    winner.Weight,
                    candidate.Definition.Priority,
                    candidate.Weight))
                {
                    winner = candidate;
                    found = true;
                }
            }

            return found;
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
