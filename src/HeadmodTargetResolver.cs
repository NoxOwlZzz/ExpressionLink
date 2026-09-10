using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class RendererCandidate
    {
        internal SkinnedMeshRenderer Renderer;
        internal Mesh Mesh;
        internal string RelativePath;
        internal string ObjectName;
        internal string MeshName;
        internal int RendererInstanceId;
        internal int MeshInstanceId;

        internal int PositiveXIndex = -1;
        internal int NegativeXIndex = -1;
        internal int PositiveYIndex = -1;
        internal int NegativeYIndex = -1;
        internal int BlinkIndex = -1;
        internal readonly int[] EyeCustomizationIndices;

        internal int DirectionalShapeCount;
        internal int EyeCustomizationShapeCount;
        internal bool Compatible;
        internal bool ExactPathMatch;
        internal bool ExactNameMatch;
        internal string Decision;

        internal RendererCandidate()
        {
            EyeCustomizationIndices =
                new int[EyeCustomizationCatalog.ChannelCount];
            for (int i = 0; i < EyeCustomizationIndices.Length; i++)
            {
                EyeCustomizationIndices[i] = -1;
            }
        }
    }

    internal sealed class ResolveResult
    {
        internal ResolveStatus Status;
        internal BlendshapeBinding Binding;
        internal RendererCandidate SelectedCandidate;
        internal List<RendererCandidate> Candidates;
        internal List<RendererCandidate> AmbiguousCandidates;
        internal string Message;
    }

    internal static class HeadmodTargetResolver
    {
        internal static ResolveResult Resolve(ChaControl owner)
        {
            List<RendererCandidate> diagnostics = new List<RendererCandidate>();
            if (owner == null)
            {
                return Retry(diagnostics, "ChaControl is unavailable.");
            }

            string exactPath = NormalizeConfiguredPath(owner, PluginConfig.TargetRendererPath.Value);
            if (exactPath.Length > 0)
            {
                Transform exactTransform = owner.transform.Find(exactPath);
                if (exactTransform != null)
                {
                    SkinnedMeshRenderer[] exactRenderers =
                        exactTransform.GetComponents<SkinnedMeshRenderer>();
                    List<RendererCandidate> exactCompatible =
                        new List<RendererCandidate>();
                    for (int i = 0; i < exactRenderers.Length; i++)
                    {
                        SkinnedMeshRenderer exactRenderer = exactRenderers[i];
                        if (exactRenderer == null || !BelongsToOwner(exactRenderer, owner))
                        {
                            continue;
                        }

                        RendererCandidate exact = Evaluate(owner, exactRenderer, true);
                        diagnostics.Add(exact);
                        if (exact.Compatible)
                        {
                            exactCompatible.Add(exact);
                        }
                    }

                    if (exactCompatible.Count == 1)
                    {
                        RendererCandidate exact = exactCompatible[0];
                        exact.Decision = Acceptance(
                            exact,
                            "Accepted by exact TargetRendererPath.");
                        return Bound(diagnostics, exact, exact.Decision);
                    }

                    if (exactCompatible.Count > 1)
                    {
                        return Ambiguous(
                            diagnostics,
                            exactCompatible,
                            "TargetRendererPath contains multiple compatible renderers.");
                    }

                    if (exactRenderers.Length == 0)
                    {
                        diagnostics.Add(CreatePathFailure(
                            exactPath,
                            "Configured path exists but does not contain a SkinnedMeshRenderer."));
                    }
                }
                else
                {
                    diagnostics.Add(CreatePathFailure(exactPath, "Configured TargetRendererPath was not found."));
                }
            }

            switch (PluginConfig.SearchScope.Value)
            {
                case TargetSearchScope.HeadOnly:
                    if (owner.objHead == null)
                    {
                        return Retry(diagnostics, "ChaControl.objHead is not ready.");
                    }

                    AddEvaluatedRenderers(owner, owner.objHead, diagnostics);
                    return Choose(diagnostics, 0);

                case TargetSearchScope.Character:
                    AddEvaluatedRenderers(owner, owner.gameObject, diagnostics);
                    return Choose(diagnostics, 0);

                default:
                    if (owner.objHead == null)
                    {
                        return Retry(diagnostics, "ChaControl.objHead is not ready.");
                    }

                    int headStart = diagnostics.Count;
                    AddEvaluatedRenderers(owner, owner.objHead, diagnostics);
                    int headCompatible = CountCompatible(diagnostics, headStart);
                    if (headCompatible > 0)
                    {
                        return Choose(diagnostics, headStart);
                    }

                    int characterStart = diagnostics.Count;
                    AddEvaluatedRenderers(owner, owner.gameObject, diagnostics);
                    return Choose(diagnostics, characterStart);
            }
        }

        private static ResolveResult Choose(List<RendererCandidate> diagnostics, int startIndex)
        {
            List<RendererCandidate> compatible = new List<RendererCandidate>();
            for (int i = startIndex; i < diagnostics.Count; i++)
            {
                RendererCandidate candidate = diagnostics[i];
                if (candidate.Compatible)
                {
                    compatible.Add(candidate);
                }
            }

            if (compatible.Count == 0)
            {
                return Retry(diagnostics, "No compatible renderer is available in the selected scope.");
            }

            string configuredName = PluginConfig.TargetRendererName.Value ?? string.Empty;
            if (configuredName.Length > 0)
            {
                List<RendererCandidate> nameMatches = new List<RendererCandidate>();
                for (int i = 0; i < compatible.Count; i++)
                {
                    if (compatible[i].ExactNameMatch)
                    {
                        nameMatches.Add(compatible[i]);
                    }
                }

                if (nameMatches.Count == 1)
                {
                    RendererCandidate selectedByName = nameMatches[0];
                    selectedByName.Decision = Acceptance(
                        selectedByName,
                        "Accepted by exact TargetRendererName.");
                    return Bound(diagnostics, selectedByName, selectedByName.Decision);
                }

                if (nameMatches.Count > 1)
                {
                    return Ambiguous(
                        diagnostics,
                        nameMatches,
                        "Multiple compatible renderers share TargetRendererName. Configure TargetRendererPath.");
                }
            }

            if (compatible.Count == 1)
            {
                RendererCandidate only = compatible[0];
                only.Decision = Acceptance(
                    only,
                    "Accepted as the only compatible renderer in scope.");
                return Bound(diagnostics, only, only.Decision);
            }

            return Ambiguous(
                diagnostics,
                compatible,
                "Multiple compatible renderers remain. Configure TargetRendererPath.");
        }

        private static void AddEvaluatedRenderers(
            ChaControl owner,
            GameObject root,
            List<RendererCandidate> diagnostics)
        {
            if (root == null)
            {
                return;
            }

            SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = renderers[i];
                if (renderer == null || !BelongsToOwner(renderer, owner) || AlreadyEvaluated(diagnostics, renderer))
                {
                    continue;
                }

                diagnostics.Add(Evaluate(owner, renderer, false));
            }
        }

        private static RendererCandidate Evaluate(
            ChaControl owner,
            SkinnedMeshRenderer renderer,
            bool exactPathMatch)
        {
            RendererCandidate candidate = new RendererCandidate
            {
                Renderer = renderer,
                RelativePath = GetRelativePath(owner.transform, renderer.transform),
                ObjectName = renderer.gameObject.name,
                RendererInstanceId = renderer.GetInstanceID(),
                ExactPathMatch = exactPathMatch,
                ExactNameMatch = string.Equals(
                    renderer.gameObject.name,
                    PluginConfig.TargetRendererName.Value,
                    StringComparison.Ordinal),
                Decision = string.Empty
            };

            Mesh mesh = renderer.sharedMesh;
            candidate.Mesh = mesh;
            if (mesh == null)
            {
                candidate.Decision = "Rejected: sharedMesh is null.";
                return candidate;
            }

            candidate.MeshName = mesh.name;
            candidate.MeshInstanceId = mesh.GetInstanceID();
            if (SliderHighlightCompatibility.ShouldExclude(
                renderer, candidate.ExactPathMatch, candidate.ExactNameMatch))
            {
                candidate.Decision = "Rejected: SliderHighlight-owned selection overlay.";
                return candidate;
            }

            string categoryRejection = GetAutomaticCategoryRejection(owner, renderer);
            if (categoryRejection.Length > 0)
            {
                candidate.Decision = "Rejected: " + categoryRejection;
                return candidate;
            }

            if (mesh.blendShapeCount <= 0)
            {
                candidate.Decision = "Rejected: mesh has no blendshapes.";
                return candidate;
            }

            candidate.PositiveXIndex = FindBlendshape(mesh, PluginConfig.PositiveXBlendshape.Value);
            candidate.NegativeXIndex = FindBlendshape(mesh, PluginConfig.NegativeXBlendshape.Value);
            candidate.PositiveYIndex = FindBlendshape(mesh, PluginConfig.PositiveYBlendshape.Value);
            candidate.NegativeYIndex = FindBlendshape(mesh, PluginConfig.NegativeYBlendshape.Value);
            candidate.BlinkIndex = FindBlendshape(mesh, PluginConfig.BlinkBlendshape.Value);
            for (int i = 0;
                i < candidate.EyeCustomizationIndices.Length;
                i++)
            {
                candidate.EyeCustomizationIndices[i] = FindBlendshape(
                    mesh,
                    PluginConfig.GetEyeAdjustmentBlendshapeName(i));
            }

            RemoveConflictingEyeCustomizationIndices(candidate);

            candidate.DirectionalShapeCount =
                CountPresent(candidate.PositiveXIndex) +
                CountPresent(candidate.NegativeXIndex) +
                CountPresent(candidate.PositiveYIndex) +
                CountPresent(candidate.NegativeYIndex);
            candidate.EyeCustomizationShapeCount = CountPresent(
                candidate.EyeCustomizationIndices);

            bool directionsValid = PluginConfig.RequireAllDirectionalBlendshapes.Value
                ? candidate.DirectionalShapeCount == 4
                : candidate.DirectionalShapeCount > 0;

            if (!directionsValid)
            {
                candidate.Decision = "Rejected: directional blendshape requirements failed.";
                return candidate;
            }

            if (HasDuplicateManagedIndices(candidate))
            {
                candidate.Decision = "Rejected: two configured managed names resolve to the same blendshape.";
                return candidate;
            }

            bool blinkRequired = PluginConfig.BlinkEnabled.Value &&
                                 EyeStateSampler.BlinkSamplingAvailable &&
                                 PluginConfig.RequireBlinkWhenEnabled.Value;
            if (blinkRequired && candidate.BlinkIndex < 0)
            {
                candidate.Decision = "Rejected: required blink blendshape is missing.";
                return candidate;
            }

            candidate.Compatible = true;
            candidate.Decision =
                "Compatible; identified by exact blendshape names. " +
                candidate.EyeCustomizationShapeCount + "/" +
                EyeCustomizationCatalog.ChannelCount +
                " optional ExpressionControl eye-adjustment channels found.";
            return candidate;
        }

        private static void RemoveConflictingEyeCustomizationIndices(
            RendererCandidate candidate)
        {
            for (int i = 0;
                i < candidate.EyeCustomizationIndices.Length;
                i++)
            {
                int index = candidate.EyeCustomizationIndices[i];
                if (index < 0 || IsCoreManagedIndex(candidate, index))
                {
                    candidate.EyeCustomizationIndices[i] = -1;
                    continue;
                }

                for (int previous = 0; previous < i; previous++)
                {
                    if (candidate.EyeCustomizationIndices[previous] == index)
                    {
                        candidate.EyeCustomizationIndices[i] = -1;
                        break;
                    }
                }
            }
        }

        private static bool IsCoreManagedIndex(
            RendererCandidate candidate,
            int index)
        {
            return index == candidate.PositiveXIndex ||
                   index == candidate.NegativeXIndex ||
                   index == candidate.PositiveYIndex ||
                   index == candidate.NegativeYIndex ||
                   (PluginConfig.BlinkEnabled.Value &&
                    EyeStateSampler.BlinkSamplingAvailable &&
                    index == candidate.BlinkIndex);
        }

        private static bool HasDuplicateManagedIndices(RendererCandidate candidate)
        {
            int positiveX = candidate.PositiveXIndex;
            int negativeX = candidate.NegativeXIndex;
            int positiveY = candidate.PositiveYIndex;
            int negativeY = candidate.NegativeYIndex;
            int blink = PluginConfig.BlinkEnabled.Value &&
                        EyeStateSampler.BlinkSamplingAvailable
                ? candidate.BlinkIndex
                : -1;

            return SamePresentIndex(positiveX, negativeX) ||
                   SamePresentIndex(positiveX, positiveY) ||
                   SamePresentIndex(positiveX, negativeY) ||
                   SamePresentIndex(positiveX, blink) ||
                   SamePresentIndex(negativeX, positiveY) ||
                   SamePresentIndex(negativeX, negativeY) ||
                   SamePresentIndex(negativeX, blink) ||
                   SamePresentIndex(positiveY, negativeY) ||
                   SamePresentIndex(positiveY, blink) ||
                   SamePresentIndex(negativeY, blink);
        }

        private static bool SamePresentIndex(int left, int right)
        {
            return left >= 0 && left == right;
        }

        private static int FindBlendshape(Mesh mesh, string name)
        {
            return string.IsNullOrEmpty(name) ? -1 : mesh.GetBlendShapeIndex(name);
        }

        private static int CountPresent(int index)
        {
            return index >= 0 ? 1 : 0;
        }

        private static int CountPresent(int[] indices)
        {
            int count = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] >= 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountCompatible(List<RendererCandidate> candidates, int startIndex)
        {
            int count = 0;
            for (int i = startIndex; i < candidates.Count; i++)
            {
                if (candidates[i].Compatible)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool AlreadyEvaluated(
            List<RendererCandidate> diagnostics,
            SkinnedMeshRenderer renderer)
        {
            for (int i = 0; i < diagnostics.Count; i++)
            {
                if (diagnostics[i].Renderer == renderer)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool BelongsToOwner(SkinnedMeshRenderer renderer, ChaControl owner)
        {
            return renderer.GetComponentInParent<ChaControl>() == owner;
        }

        private static string GetAutomaticCategoryRejection(
            ChaControl owner,
            SkinnedMeshRenderer renderer)
        {
            Transform target = renderer.transform;
            if (IsUnder(target, owner.objBody))
            {
                return "renderer belongs to the body hierarchy.";
            }

            if (IsUnderAny(target, owner.objHair))
            {
                return "renderer belongs to a hair hierarchy.";
            }

            if (IsUnderAny(target, owner.objClothes) ||
                IsUnderAny(target, owner.objParts))
            {
                return "renderer belongs to a clothing hierarchy.";
            }

            if (IsUnderAny(target, owner.objAccessory))
            {
                return "renderer belongs to an accessory hierarchy.";
            }

            if (IsUnder(target, owner.objHead) ||
                IsUnder(target, owner.objHeadBone))
            {
                return string.Empty;
            }

            return "renderer is outside the confirmed head/head-bone hierarchies.";
        }

        private static bool IsUnderAny(Transform target, GameObject[] roots)
        {
            if (roots == null)
            {
                return false;
            }

            for (int i = 0; i < roots.Length; i++)
            {
                if (IsUnder(target, roots[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsUnder(Transform target, GameObject root)
        {
            if (target == null || root == null)
            {
                return false;
            }

            Transform current = target;
            Transform expected = root.transform;
            while (current != null)
            {
                if (current == expected)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static string Acceptance(RendererCandidate candidate, string reason)
        {
            return reason;
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return string.Empty;
            }

            if (root == target)
            {
                return string.Empty;
            }

            Stack<string> names = new Stack<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                names.Push(current.name);
                current = current.parent;
            }

            if (current != root)
            {
                return "<outside ChaControl>";
            }

            return string.Join("/", names.ToArray());
        }

        private static string NormalizeConfiguredPath(ChaControl owner, string configured)
        {
            if (string.IsNullOrEmpty(configured))
            {
                return string.Empty;
            }

            string path = configured.Trim().Replace('\\', '/').Trim('/');
            string rootPrefix = owner.transform.name + "/";
            if (path.StartsWith(rootPrefix, StringComparison.Ordinal))
            {
                path = path.Substring(rootPrefix.Length);
            }

            return path;
        }

        private static RendererCandidate CreatePathFailure(string path, string reason)
        {
            return new RendererCandidate
            {
                RelativePath = path,
                ExactPathMatch = true,
                Decision = "Rejected: " + reason
            };
        }

        private static ResolveResult Bound(
            List<RendererCandidate> diagnostics,
            RendererCandidate selected,
            string message)
        {
            try
            {
                return new ResolveResult
                {
                    Status = ResolveStatus.Bound,
                    Binding = new BlendshapeBinding(selected),
                    SelectedCandidate = selected,
                    Candidates = diagnostics,
                    AmbiguousCandidates = new List<RendererCandidate>(),
                    Message = message
                };
            }
            catch (Exception exception)
            {
                selected.Compatible = false;
                selected.Decision = "Binding failed: " + exception.GetType().Name + ": " + exception.Message;
                return Retry(diagnostics, selected.Decision);
            }
        }

        private static ResolveResult Retry(List<RendererCandidate> diagnostics, string message)
        {
            return new ResolveResult
            {
                Status = ResolveStatus.Retry,
                Candidates = diagnostics,
                AmbiguousCandidates = new List<RendererCandidate>(),
                Message = message
            };
        }

        private static ResolveResult Ambiguous(
            List<RendererCandidate> diagnostics,
            List<RendererCandidate> ambiguous,
            string message)
        {
            for (int i = 0; i < ambiguous.Count; i++)
            {
                ambiguous[i].Decision = "Ambiguous compatible candidate; not selected.";
            }

            return new ResolveResult
            {
                Status = ResolveStatus.Ambiguous,
                Candidates = diagnostics,
                AmbiguousCandidates = ambiguous,
                Message = message
            };
        }
    }
}
