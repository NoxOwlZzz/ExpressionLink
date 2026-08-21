using System;
using System.Collections.Generic;
using System.Text;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkLimits
    {
        internal const int MaximumLinks = 128;
        internal const int MaximumPayloadBytes = 256 * 1024;
        internal const int MaximumRecordBytes = 8 * 1024;

        internal const int MaximumNameBytes = 256;
        internal const int MaximumSourceBytes = 512;
        internal const int MaximumRendererPathBytes = 4096;
        internal const int MaximumRendererHintBytes = 512;
        internal const int MaximumMeshHintBytes = 512;
        internal const int MaximumBlendshapeNameBytes = 1024;

        internal const int MinimumSlotIndex = -1;
        internal const int MaximumSlotIndex = short.MaxValue;
        internal const int MinimumComponentIndex = -1;
        internal const int MaximumComponentIndex = short.MaxValue;
        internal const int MinimumPriority = short.MinValue;
        internal const int MaximumPriority = short.MaxValue;

        internal const float MinimumSourceValue = 0f;
        internal const float MaximumSourceValue = 1f;
        internal const float MinimumBlendshapeWeight = 0f;
        internal const float MaximumBlendshapeWeight = 100f;
        internal const float MinimumSmoothingSpeed = 0f;
        internal const float MaximumSmoothingSpeed = 1000f;
    }

    internal static class ExpressionLinkValidator
    {
        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        internal static bool TryNormalize(
            ExpressionLinkDefinition input,
            out ExpressionLinkDefinition normalized,
            out string error)
        {
            normalized = null;
            error = string.Empty;

            if (input == null)
            {
                error = "The expression link is null.";
                return false;
            }

            if (input.Id == Guid.Empty)
            {
                error = "The expression link ID is empty.";
                return false;
            }

            if (!IsDefined(input.Mode))
            {
                error = "The expression link mode is invalid.";
                return false;
            }

            if (!IsDefined(input.Scope))
            {
                error = "The expression target scope is invalid.";
                return false;
            }

            if (!IsInRange(
                input.SlotIndex,
                ExpressionLinkLimits.MinimumSlotIndex,
                ExpressionLinkLimits.MaximumSlotIndex))
            {
                error = "The target slot index is outside the supported range.";
                return false;
            }

            if (!IsInRange(
                input.ComponentIndex,
                ExpressionLinkLimits.MinimumComponentIndex,
                ExpressionLinkLimits.MaximumComponentIndex))
            {
                error = "The renderer component index is outside the supported range.";
                return false;
            }

            if (!IsInRange(
                input.Priority,
                ExpressionLinkLimits.MinimumPriority,
                ExpressionLinkLimits.MaximumPriority))
            {
                error = "The expression link priority is outside the supported range.";
                return false;
            }

            if (!IsFiniteInRange(
                input.Threshold,
                ExpressionLinkLimits.MinimumSourceValue,
                ExpressionLinkLimits.MaximumSourceValue))
            {
                error = "Threshold must be a finite value from 0 to 1.";
                return false;
            }

            if (!IsFiniteInRange(
                    input.InputMin,
                    ExpressionLinkLimits.MinimumSourceValue,
                    ExpressionLinkLimits.MaximumSourceValue) ||
                !IsFiniteInRange(
                    input.InputMax,
                    ExpressionLinkLimits.MinimumSourceValue,
                    ExpressionLinkLimits.MaximumSourceValue) ||
                input.InputMax <= input.InputMin)
            {
                error = "InputMin and InputMax must define an increasing finite range from 0 to 1.";
                return false;
            }

            if (!IsFiniteInRange(
                    input.OutputMin,
                    ExpressionLinkLimits.MinimumBlendshapeWeight,
                    ExpressionLinkLimits.MaximumBlendshapeWeight) ||
                !IsFiniteInRange(
                    input.OutputMax,
                    ExpressionLinkLimits.MinimumBlendshapeWeight,
                    ExpressionLinkLimits.MaximumBlendshapeWeight))
            {
                error = "Output weights must be finite values from 0 to 100.";
                return false;
            }

            if (!IsFiniteInRange(
                input.SmoothingSpeed,
                ExpressionLinkLimits.MinimumSmoothingSpeed,
                ExpressionLinkLimits.MaximumSmoothingSpeed))
            {
                error = "Smoothing speed is outside the supported range.";
                return false;
            }

            string name;
            string source;
            string rendererPath;
            string rendererHint;
            string meshHint;
            string blendshapeName;
            if (!TryNormalizeString(
                    input.Name,
                    "Name",
                    ExpressionLinkLimits.MaximumNameBytes,
                    out name,
                    out error) ||
                !TryNormalizeString(
                    input.Source,
                    "Source",
                    ExpressionLinkLimits.MaximumSourceBytes,
                    out source,
                    out error) ||
                !TryNormalizeString(
                    input.RendererPath,
                    "RendererPath",
                    ExpressionLinkLimits.MaximumRendererPathBytes,
                    out rendererPath,
                    out error) ||
                !TryNormalizeString(
                    input.RendererHint,
                    "RendererHint",
                    ExpressionLinkLimits.MaximumRendererHintBytes,
                    out rendererHint,
                    out error) ||
                !TryNormalizeString(
                    input.MeshHint,
                    "MeshHint",
                    ExpressionLinkLimits.MaximumMeshHintBytes,
                    out meshHint,
                    out error) ||
                !TryNormalizeString(
                    input.BlendshapeName,
                    "BlendshapeName",
                    ExpressionLinkLimits.MaximumBlendshapeNameBytes,
                    out blendshapeName,
                    out error))
            {
                return false;
            }

            if (name.Length == 0)
            {
                name = "Expression Link";
            }

            normalized = input.Clone();
            normalized.Name = name;
            normalized.Source = source;
            normalized.RendererPath = rendererPath;
            normalized.RendererHint = rendererHint;
            normalized.MeshHint = meshHint;
            normalized.BlendshapeName = blendshapeName;
            return true;
        }

        internal static bool TryNormalizeList(
            IList<ExpressionLinkDefinition> input,
            out ExpressionLinkDefinition[] normalized,
            out string error)
        {
            normalized = new ExpressionLinkDefinition[0];
            error = string.Empty;

            int count = input == null ? 0 : input.Count;
            if (count > ExpressionLinkLimits.MaximumLinks)
            {
                error = "The expression link count exceeds the supported maximum of " +
                    ExpressionLinkLimits.MaximumLinks + ".";
                return false;
            }

            ExpressionLinkDefinition[] result =
                new ExpressionLinkDefinition[count];
            HashSet<Guid> ids = new HashSet<Guid>();
            for (int i = 0; i < count; i++)
            {
                string itemError;
                if (!TryNormalize(input[i], out result[i], out itemError))
                {
                    error = "Expression link " + i + " is invalid: " + itemError;
                    return false;
                }

                if (!ids.Add(result[i].Id))
                {
                    error = "Expression link " + i + " duplicates ID " +
                        result[i].Id + ".";
                    return false;
                }
            }

            normalized = result;
            return true;
        }

        private static bool TryNormalizeString(
            string input,
            string fieldName,
            int maximumBytes,
            out string normalized,
            out string error)
        {
            normalized = (input ?? string.Empty).Trim();
            error = string.Empty;

            int byteCount;
            try
            {
                byteCount = StrictUtf8.GetByteCount(normalized);
            }
            catch (EncoderFallbackException)
            {
                error = fieldName + " contains invalid Unicode text.";
                return false;
            }

            if (byteCount > maximumBytes || byteCount > ushort.MaxValue)
            {
                error = fieldName + " exceeds its UTF-8 byte limit of " +
                    maximumBytes + ".";
                return false;
            }

            return true;
        }

        private static bool IsFiniteInRange(float value, float minimum, float maximum)
        {
            return !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value >= minimum &&
                value <= maximum;
        }

        private static bool IsInRange(int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }

        private static bool IsDefined(ExpressionLinkMode value)
        {
            return value == ExpressionLinkMode.Binary ||
                value == ExpressionLinkMode.FollowSource;
        }

        private static bool IsDefined(ExpressionTargetScope value)
        {
            return value >= ExpressionTargetScope.Any &&
                value <= ExpressionTargetScope.Other;
        }
    }
}
