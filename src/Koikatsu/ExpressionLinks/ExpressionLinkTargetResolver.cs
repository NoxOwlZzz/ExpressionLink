using System;
using System.Collections.Generic;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkTargetResolver
    {
        internal struct Resolution
        {
            internal ExpressionLinkTargetStatus Status;
            internal string Message;
            internal CharacterRendererRecord Record;
            internal int BlendshapeIndex;
        }

        private struct ResolvedCandidate
        {
            internal CharacterRendererRecord Record;
            internal int BlendshapeIndex;
        }

        private readonly CharacterRendererCatalog _catalog;

        internal ExpressionLinkTargetResolver(
            CharacterRendererCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            _catalog = catalog;
        }

        internal Resolution Resolve(
            ExpressionLinkDefinition definition)
        {
            string configuredPath = definition.RendererPath ?? string.Empty;
            bool hasConfiguredPath = configuredPath.Trim().Length > 0;
            string path = RendererPathUtility.NormalizeRelativePath(
                _catalog.Owner == null ? null : _catalog.Owner.transform,
                configuredPath);

            if (!hasConfiguredPath)
            {
                return ResolveUniqueBlendshape(definition);
            }

            List<ResolvedCandidate> candidates =
                new List<ResolvedCandidate>();
            for (int i = 0; i < _catalog.Count; i++)
            {
                CharacterRendererRecord record = _catalog.GetRecord(i);
                if (record == null ||
                    !string.Equals(
                        record.RelativePath,
                        path,
                        StringComparison.Ordinal) ||
                    !MatchesScopeAndSlot(definition, record) ||
                    (definition.ComponentIndex >= 0 &&
                     record.ComponentIndex != definition.ComponentIndex))
                {
                    continue;
                }

                int blendshapeIndex = TryFindBlendshape(
                    record,
                    definition.BlendshapeName);
                if (blendshapeIndex >= 0)
                {
                    candidates.Add(new ResolvedCandidate
                    {
                        Record = record,
                        BlendshapeIndex = blendshapeIndex
                    });
                }
            }

            if (candidates.Count == 0)
            {
                return Failure(
                    ExpressionLinkTargetStatus.Missing,
                    "No matching renderer at the configured path contains the blendshape.");
            }

            if (candidates.Count == 1)
            {
                return Ready(candidates[0]);
            }

            int hintedIndex = FindUniqueHintedCandidate(
                candidates,
                definition.RendererHint,
                definition.MeshHint);
            if (hintedIndex >= 0)
            {
                return Ready(candidates[hintedIndex]);
            }

            return Failure(
                ExpressionLinkTargetStatus.Ambiguous,
                "The configured path resolves to multiple matching renderers; set ComponentIndex.");
        }

        private Resolution ResolveUniqueBlendshape(
            ExpressionLinkDefinition definition)
        {
            ResolvedCandidate candidate = default(ResolvedCandidate);
            int matches = 0;
            for (int i = 0; i < _catalog.Count; i++)
            {
                CharacterRendererRecord record = _catalog.GetRecord(i);
                if (!MatchesScopeAndSlot(definition, record) ||
                    (definition.ComponentIndex >= 0 &&
                     record.ComponentIndex != definition.ComponentIndex))
                {
                    continue;
                }

                int blendshapeIndex = TryFindBlendshape(
                    record,
                    definition.BlendshapeName);
                if (blendshapeIndex < 0)
                {
                    continue;
                }

                candidate.Record = record;
                candidate.BlendshapeIndex = blendshapeIndex;
                matches++;
                if (matches > 1)
                {
                    return Failure(
                        ExpressionLinkTargetStatus.Ambiguous,
                        "The blendshape exists on multiple character renderers; capture an exact renderer path.");
                }
            }

            if (matches == 0)
            {
                return Failure(
                    ExpressionLinkTargetStatus.Missing,
                    "The blendshape was not found on the character.");
            }

            if (!MatchesScopeAndSlot(definition, candidate.Record))
            {
                return Failure(
                    ExpressionLinkTargetStatus.Missing,
                    "The unique blendshape target does not match the configured scope or slot.");
            }

            return Ready(candidate);
        }

        private static bool MatchesScopeAndSlot(
            ExpressionLinkDefinition definition,
            CharacterRendererRecord record)
        {
            if (definition == null || record == null)
            {
                return false;
            }

            if (definition.Scope != ExpressionTargetScope.Any &&
                definition.Scope != record.Scope)
            {
                return false;
            }

            return definition.SlotIndex < 0 ||
                definition.SlotIndex == record.SlotIndex;
        }

        private static int TryFindBlendshape(
            CharacterRendererRecord record,
            string blendshapeName)
        {
            if (record == null ||
                !record.IsValid ||
                string.IsNullOrEmpty(blendshapeName))
            {
                return -1;
            }

            try
            {
                return record.FindBlendshape(blendshapeName);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static int FindUniqueHintedCandidate(
            List<ResolvedCandidate> candidates,
            string rendererHint,
            string meshHint)
        {
            bool hasRendererHint = !string.IsNullOrEmpty(rendererHint);
            bool hasMeshHint = !string.IsNullOrEmpty(meshHint);
            if (!hasRendererHint && !hasMeshHint)
            {
                return -1;
            }

            int found = -1;
            for (int i = 0; i < candidates.Count; i++)
            {
                CharacterRendererRecord record = candidates[i].Record;
                if ((hasRendererHint &&
                     !string.Equals(
                         record.RendererName,
                         rendererHint,
                         StringComparison.Ordinal)) ||
                    (hasMeshHint &&
                     !string.Equals(
                         record.MeshName,
                         meshHint,
                         StringComparison.Ordinal)))
                {
                    continue;
                }

                if (found >= 0)
                {
                    return -1;
                }

                found = i;
            }

            return found;
        }

        private static Resolution Ready(
            ResolvedCandidate candidate)
        {
            return new Resolution
            {
                Status = ExpressionLinkTargetStatus.Ready,
                Message = "Target resolved.",
                Record = candidate.Record,
                BlendshapeIndex = candidate.BlendshapeIndex
            };
        }

        private static Resolution Failure(
            ExpressionLinkTargetStatus status,
            string message)
        {
            return new Resolution
            {
                Status = status,
                Message = message ?? string.Empty,
                Record = null,
                BlendshapeIndex = -1
            };
        }
    }
}
