using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal enum ExpressionLinkTargetStatus : byte
    {
        Ready = 0,
        Missing = 1,
        Ambiguous = 2,
        Invalid = 3,
        Conflict = 4
    }

    internal sealed class ExpressionLinkTargetDiagnostic
    {
        internal ExpressionLinkTargetDiagnostic(
            int linkIndex,
            Guid linkId,
            ExpressionLinkTargetStatus status,
            string message)
        {
            LinkIndex = linkIndex;
            LinkId = linkId;
            Set(status, message, null, -1);
        }

        internal int LinkIndex { get; private set; }

        internal Guid LinkId { get; private set; }

        internal ExpressionLinkTargetStatus Status { get; private set; }

        internal string Message { get; private set; }

        internal string ResolvedPath { get; private set; }

        internal int ComponentIndex { get; private set; }

        internal int BlendshapeIndex { get; private set; }

        internal int RendererInstanceId { get; private set; }

        internal bool IsReady
        {
            get { return Status == ExpressionLinkTargetStatus.Ready; }
        }

        internal void Set(
            ExpressionLinkTargetStatus status,
            string message,
            CharacterRendererRecord record,
            int blendshapeIndex)
        {
            Status = status;
            Message = message ?? string.Empty;
            ResolvedPath = record == null
                ? string.Empty
                : record.RelativePath;
            ComponentIndex = record == null ? -1 : record.ComponentIndex;
            BlendshapeIndex = blendshapeIndex;
            RendererInstanceId = record == null
                ? 0
                : record.RendererInstanceId;
        }
    }

    internal sealed class ExpressionLinkRuntime
    {
        private sealed class CompiledLink
        {
            internal ExpressionLinkDefinition Definition;
            internal int GroupIndex = -1;
            internal ExpressionLinkTargetDiagnostic Diagnostic;
        }

        private sealed class TargetGroup
        {
            internal ExpressionLinkTargetBinding Binding;
            internal bool HasCandidate;
            internal int WinnerPriority;
            internal float DesiredWeight;
            internal float SmoothingSpeed;
            internal int WinnerLinkIndex;
            internal bool HasSmoothedWeight;
            internal float SmoothedWeight;

            internal void ResetCandidates()
            {
                HasCandidate = false;
                WinnerPriority = 0;
                DesiredWeight = 0f;
                SmoothingSpeed = 0f;
                WinnerLinkIndex = -1;
            }

            internal void ResetSmoothing()
            {
                HasSmoothedWeight = false;
                SmoothedWeight = 0f;
            }
        }

        private struct TargetKey : IEquatable<TargetKey>
        {
            internal TargetKey(
                int rendererInstanceId,
                int meshInstanceId,
                int blendshapeIndex)
            {
                RendererInstanceId = rendererInstanceId;
                MeshInstanceId = meshInstanceId;
                BlendshapeIndex = blendshapeIndex;
            }

            private int RendererInstanceId;
            private int MeshInstanceId;
            private int BlendshapeIndex;

            public bool Equals(TargetKey other)
            {
                return RendererInstanceId == other.RendererInstanceId &&
                    MeshInstanceId == other.MeshInstanceId &&
                    BlendshapeIndex == other.BlendshapeIndex;
            }

            public override bool Equals(object value)
            {
                return value is TargetKey && Equals((TargetKey)value);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = RendererInstanceId;
                    hash = (hash * 397) ^ MeshInstanceId;
                    return (hash * 397) ^ BlendshapeIndex;
                }
            }
        }

        private readonly CharacterRendererCatalog _catalog =
            new CharacterRendererCatalog();
        private readonly ExpressionLinkTargetResolver _targetResolver;
        private CompiledLink[] _links = new CompiledLink[0];
        private TargetGroup[] _groups = new TargetGroup[0];
        private bool _hasUnresolvedLinks;
        private bool _requiresRebuild;
        private string _lastError = string.Empty;

        internal ExpressionLinkRuntime()
        {
            _targetResolver = new ExpressionLinkTargetResolver(_catalog);
        }

        internal CharacterRendererCatalog Catalog
        {
            get { return _catalog; }
        }

        internal int LinkCount
        {
            get { return _links.Length; }
        }

        internal int TargetCount
        {
            get { return _groups.Length; }
        }

        internal bool HasUnresolvedLinks
        {
            get { return _hasUnresolvedLinks; }
        }

        internal bool RequiresRebuild
        {
            get { return _requiresRebuild; }
        }

        internal string LastError
        {
            get { return _lastError; }
        }

        internal ExpressionLinkTargetDiagnostic GetDiagnostic(int linkIndex)
        {
            return linkIndex >= 0 && linkIndex < _links.Length
                ? _links[linkIndex].Diagnostic
                : null;
        }

        internal bool ManagesTarget(
            SkinnedMeshRenderer renderer,
            int blendshapeIndex)
        {
            if (renderer == null || blendshapeIndex < 0)
            {
                return false;
            }

            for (int i = 0; i < _groups.Length; i++)
            {
                ExpressionLinkTargetBinding binding = _groups[i].Binding;
                if (binding.Renderer == renderer &&
                    binding.BlendshapeIndex == blendshapeIndex)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool Rebuild(
            ChaControl owner,
            IList<ExpressionLinkDefinition> definitions,
            BlendshapeBinding reservedEyeBinding,
            ManualVisibilityBinding reservedVisibility)
        {
            string restoreError;
            RestoreAll(out restoreError);
            _requiresRebuild = false;
            _lastError = string.Empty;
            _hasUnresolvedLinks = false;
            _catalog.Rebuild(owner);

            int count = definitions == null ? 0 : definitions.Count;
            _links = new CompiledLink[count];
            List<TargetGroup> groups = new List<TargetGroup>();
            Dictionary<TargetKey, int> groupIndices =
                new Dictionary<TargetKey, int>();

            for (int i = 0; i < count; i++)
            {
                ExpressionLinkDefinition input = definitions[i];
                Guid id = input == null ? Guid.Empty : input.Id;
                CompiledLink link = new CompiledLink();
                link.Diagnostic = new ExpressionLinkTargetDiagnostic(
                    i,
                    id,
                    ExpressionLinkTargetStatus.Invalid,
                    string.Empty);
                _links[i] = link;

                ExpressionLinkDefinition definition;
                string validationError;
                if (!ExpressionLinkValidator.TryNormalize(
                    input,
                    out definition,
                    out validationError))
                {
                    link.Diagnostic.Set(
                        ExpressionLinkTargetStatus.Invalid,
                        validationError,
                        null,
                        -1);
                    if (input == null || input.Enabled)
                    {
                        _hasUnresolvedLinks = true;
                    }

                    continue;
                }

                link.Definition = definition;
                if (!definition.Enabled)
                {
                    link.Diagnostic.Set(
                        ExpressionLinkTargetStatus.Ready,
                        "Link disabled; target resolution skipped.",
                        null,
                        -1);
                    continue;
                }

                if (string.IsNullOrEmpty(definition.BlendshapeName))
                {
                    link.Diagnostic.Set(
                        ExpressionLinkTargetStatus.Invalid,
                        "The target blendshape name is empty.",
                        null,
                        -1);
                    if (definition.Enabled)
                    {
                        _hasUnresolvedLinks = true;
                    }

                    continue;
                }

                ExpressionLinkTargetResolver.Resolution resolution =
                    _targetResolver.Resolve(definition);
                if (resolution.Status != ExpressionLinkTargetStatus.Ready)
                {
                    link.Diagnostic.Set(
                        resolution.Status,
                        resolution.Message,
                        resolution.Record,
                        resolution.BlendshapeIndex);
                    if (definition.Enabled)
                    {
                        _hasUnresolvedLinks = true;
                    }

                    continue;
                }

                if (reservedEyeBinding != null &&
                    resolution.Record.Renderer == reservedEyeBinding.Renderer &&
                    reservedEyeBinding.IsManagedIndex(
                        resolution.BlendshapeIndex))
                {
                    link.Diagnostic.Set(
                        ExpressionLinkTargetStatus.Conflict,
                        "The target is already managed by the eye-motion binding.",
                        resolution.Record,
                        resolution.BlendshapeIndex);
                    if (definition.Enabled)
                    {
                        _hasUnresolvedLinks = true;
                    }

                    continue;
                }
                if (reservedEyeBinding != null &&
                    reservedVisibility != null &&
                    resolution.Record.Renderer == reservedEyeBinding.Renderer &&
                    IsManualVisibilityIndex(
                        reservedVisibility,
                        resolution.BlendshapeIndex))
                {
                    link.Diagnostic.Set(
                        ExpressionLinkTargetStatus.Conflict,
                        "The target is already managed by legacy visibility.",
                        resolution.Record,
                        resolution.BlendshapeIndex);
                    if (definition.Enabled)
                    {
                        _hasUnresolvedLinks = true;
                    }

                    continue;
                }

                TargetKey key = new TargetKey(
                    resolution.Record.RendererInstanceId,
                    resolution.Record.MeshInstanceId,
                    resolution.BlendshapeIndex);
                int groupIndex;
                if (!groupIndices.TryGetValue(key, out groupIndex))
                {
                    groupIndex = groups.Count;
                    TargetGroup group = new TargetGroup();
                    try
                    {
                        group.Binding = new ExpressionLinkTargetBinding(
                            resolution.Record,
                            resolution.BlendshapeIndex,
                            definition.BlendshapeName);
                    }
                    catch (Exception exception)
                    {
                        link.Diagnostic.Set(
                            ExpressionLinkTargetStatus.Invalid,
                            exception.GetType().Name + ": " + exception.Message,
                            resolution.Record,
                            resolution.BlendshapeIndex);
                        if (definition.Enabled)
                        {
                            _hasUnresolvedLinks = true;
                        }

                        continue;
                    }

                    groupIndices.Add(key, groupIndex);
                    groups.Add(group);
                }

                link.GroupIndex = groupIndex;
                link.Diagnostic.Set(
                    ExpressionLinkTargetStatus.Ready,
                    "Target resolved.",
                    resolution.Record,
                    resolution.BlendshapeIndex);
            }

            _groups = groups.ToArray();
            return !_hasUnresolvedLinks;
        }

        internal bool Apply(
            float[] weights,
            bool[] ready,
            float deltaTime)
        {
            if (_requiresRebuild)
            {
                return false;
            }

            if (float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime) ||
                deltaTime < 0f)
            {
                deltaTime = 0f;
            }

            for (int i = 0; i < _groups.Length; i++)
            {
                _groups[i].ResetCandidates();
            }

            int sourceCount = weights == null || ready == null
                ? 0
                : Math.Min(weights.Length, ready.Length);
            for (int i = 0; i < _links.Length; i++)
            {
                CompiledLink link = _links[i];
                if (link.GroupIndex < 0 ||
                    link.Definition == null ||
                    !link.Definition.Enabled ||
                    i >= sourceCount ||
                    !ready[i])
                {
                    continue;
                }

                float candidateWeight = ExpressionLinkEvaluator.Evaluate(
                    link.Definition,
                    weights[i]);
                TargetGroup group = _groups[link.GroupIndex];
                if (!group.HasCandidate ||
                    ExpressionLinkConflictResolver.CandidateWins(
                        group.WinnerPriority,
                        group.DesiredWeight,
                        link.Definition.Priority,
                        candidateWeight))
                {
                    group.HasCandidate = true;
                    group.WinnerPriority = link.Definition.Priority;
                    group.DesiredWeight = candidateWeight;
                    group.SmoothingSpeed = link.Definition.SmoothingSpeed;
                    group.WinnerLinkIndex = i;
                }
            }

            bool valid = true;
            for (int i = 0; i < _groups.Length; i++)
            {
                TargetGroup group = _groups[i];
                string error;
                if (!group.HasCandidate)
                {
                    if (!group.Binding.Restore(out error))
                    {
                        MarkGroupInvalid(i, error);
                        valid = false;
                    }

                    group.ResetSmoothing();
                    continue;
                }

                float output;
                if (!TryGetSmoothedWeight(
                    group,
                    deltaTime,
                    out output,
                    out error))
                {
                    MarkGroupInvalid(i, error);
                    valid = false;
                    continue;
                }

                if (!group.Binding.Apply(output, out error))
                {
                    MarkGroupInvalid(i, error);
                    valid = false;
                }
            }

            return valid;
        }

        internal bool RestoreAll()
        {
            string error;
            return RestoreAll(out error);
        }

        internal bool RestoreAll(out string error)
        {
            error = string.Empty;
            bool restored = true;
            for (int i = 0; i < _groups.Length; i++)
            {
                string targetError;
                if (!_groups[i].Binding.Restore(out targetError))
                {
                    restored = false;
                    if (error.Length == 0)
                    {
                        error = targetError;
                    }
                }

                _groups[i].ResetCandidates();
                _groups[i].ResetSmoothing();
            }

            return restored;
        }
        private static bool IsManualVisibilityIndex(
            ManualVisibilityBinding visibility,
            int blendshapeIndex)
        {
            if (visibility == null || blendshapeIndex < 0)
            {
                return false;
            }

            BlendshapeVisibilitySlot[] slots = visibility.BlendshapeSlots;
            int count = Math.Min(
                ManualVisibilityCatalog.UserFacingBlendshapeCount,
                slots.Length);
            for (int i = 0; i < count; i++)
            {
                BlendshapeVisibilitySlot slot = slots[i];
                if (slot != null &&
                    slot.Resolution == VisibilityResolutionStatus.Ready &&
                    slot.BlendshapeIndex == blendshapeIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetSmoothedWeight(
            TargetGroup group,
            float deltaTime,
            out float output,
            out string error)
        {
            output = group.DesiredWeight;
            error = string.Empty;
            if (group.SmoothingSpeed <= 0f)
            {
                group.SmoothedWeight = group.DesiredWeight;
                group.HasSmoothedWeight = true;
                return true;
            }

            if (!group.HasSmoothedWeight)
            {
                if (!group.Binding.TryGetCurrentWeight(
                    out group.SmoothedWeight,
                    out error))
                {
                    return false;
                }

                if (float.IsNaN(group.SmoothedWeight) ||
                    float.IsInfinity(group.SmoothedWeight))
                {
                    group.SmoothedWeight = group.DesiredWeight;
                }

                group.HasSmoothedWeight = true;
            }

            float maximumDelta = group.SmoothingSpeed * deltaTime;
            group.SmoothedWeight = MoveTowards(
                group.SmoothedWeight,
                group.DesiredWeight,
                maximumDelta);
            output = group.SmoothedWeight;
            return true;
        }

        private static float MoveTowards(
            float current,
            float target,
            float maximumDelta)
        {
            if (maximumDelta <= 0f)
            {
                return current;
            }

            float difference = target - current;
            if (Math.Abs(difference) <= maximumDelta)
            {
                return target;
            }

            return current + (difference > 0f ? maximumDelta : -maximumDelta);
        }

        private void MarkGroupInvalid(int groupIndex, string error)
        {
            _requiresRebuild = true;
            _hasUnresolvedLinks = true;
            _lastError = string.IsNullOrEmpty(error)
                ? "An expression-link target became invalid."
                : error;

            for (int i = 0; i < _links.Length; i++)
            {
                if (_links[i].GroupIndex == groupIndex)
                {
                    _links[i].Diagnostic.Set(
                        ExpressionLinkTargetStatus.Invalid,
                        _lastError,
                        null,
                        -1);
                }
            }
        }
    }
}
