using System;
using System.Globalization;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal enum ExpressionTriggerPart : byte
    {
        None = 0,
        Brow = 1,
        Eyes = 2,
        Mouth = 3
    }

    internal enum ExpressionTriggerResolutionStatus : byte
    {
        Disabled = 0,
        Ready = 1,
        Missing = 2,
        Ambiguous = 3,
        Excluded = 4,
        Invalid = 5,
        Unavailable = 6
    }

    internal struct ExpressionTriggerSelector : IEquatable<ExpressionTriggerSelector>
    {
        internal readonly ExpressionTriggerPart Part;
        internal readonly int PatternIndex;

        internal bool IsValid
        {
            get { return Part != ExpressionTriggerPart.None && PatternIndex >= 0; }
        }

        internal ExpressionTriggerSelector(ExpressionTriggerPart part, int patternIndex)
        {
            Part = part;
            PatternIndex = patternIndex;
        }

        public bool Equals(ExpressionTriggerSelector other)
        {
            return Part == other.Part && PatternIndex == other.PatternIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is ExpressionTriggerSelector &&
                   Equals((ExpressionTriggerSelector)obj);
        }

        public override int GetHashCode()
        {
            return ((int)Part * 397) ^ PatternIndex;
        }

        public override string ToString()
        {
            return IsValid ? ExpressionTriggerSyntax.FormatSelector(this) : "None";
        }

        public static bool operator ==(
            ExpressionTriggerSelector left,
            ExpressionTriggerSelector right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            ExpressionTriggerSelector left,
            ExpressionTriggerSelector right)
        {
            return !left.Equals(right);
        }
    }

    internal struct ParsedExpressionTrigger
    {
        internal readonly bool IsSelector;
        internal readonly ExpressionTriggerSelector Selector;
        internal readonly string BlendshapeName;

        internal ParsedExpressionTrigger(ExpressionTriggerSelector selector)
        {
            IsSelector = true;
            Selector = selector;
            BlendshapeName = string.Empty;
        }

        internal ParsedExpressionTrigger(string blendshapeName)
        {
            IsSelector = false;
            Selector = default(ExpressionTriggerSelector);
            BlendshapeName = blendshapeName ?? string.Empty;
        }
    }

    internal static class ExpressionTriggerSyntax
    {
        internal const int SlotCount = 4;
        internal const string ExcludedNamePrefix = "eye_motion.";

        internal static bool TryParse(
            string configuredTrigger,
            out ParsedExpressionTrigger parsed,
            out ExpressionTriggerResolutionStatus failureStatus,
            out string message)
        {
            parsed = default(ParsedExpressionTrigger);
            failureStatus = ExpressionTriggerResolutionStatus.Invalid;
            message = string.Empty;

            string value = configuredTrigger == null
                ? string.Empty
                : configuredTrigger.Trim();
            if (value.Length == 0)
            {
                failureStatus = ExpressionTriggerResolutionStatus.Disabled;
                message = "No expression trigger is configured.";
                return false;
            }

            ExpressionTriggerPart part;
            int prefixLength;
            if (TryGetSelectorPrefix(value, out part, out prefixLength))
            {
                string patternText = value.Substring(prefixLength).Trim();
                int patternIndex;
                if (!int.TryParse(
                    patternText,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out patternIndex) ||
                    patternIndex < 0)
                {
                    failureStatus = ExpressionTriggerResolutionStatus.Invalid;
                    message =
                        "The selector must use brow:N, eyes:N, or mouth:N with a non-negative integer.";
                    return false;
                }

                parsed = new ParsedExpressionTrigger(
                    new ExpressionTriggerSelector(part, patternIndex));
                return true;
            }

            if (value.StartsWith(
                ExcludedNamePrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                failureStatus = ExpressionTriggerResolutionStatus.Excluded;
                message =
                    "eye_motion.* blendshapes are destinations and cannot be expression triggers.";
                return false;
            }

            parsed = new ParsedExpressionTrigger(value);
            return true;
        }

        internal static string FormatSelector(ExpressionTriggerSelector selector)
        {
            string prefix;
            switch (selector.Part)
            {
                case ExpressionTriggerPart.Brow:
                    prefix = "brow:";
                    break;
                case ExpressionTriggerPart.Eyes:
                    prefix = "eyes:";
                    break;
                case ExpressionTriggerPart.Mouth:
                    prefix = "mouth:";
                    break;
                default:
                    return "None";
            }

            return prefix + selector.PatternIndex.ToString(CultureInfo.InvariantCulture);
        }

        private static bool TryGetSelectorPrefix(
            string value,
            out ExpressionTriggerPart part,
            out int prefixLength)
        {
            if (value.StartsWith("brow:", StringComparison.OrdinalIgnoreCase))
            {
                part = ExpressionTriggerPart.Brow;
                prefixLength = 5;
                return true;
            }

            if (value.StartsWith("eyes:", StringComparison.OrdinalIgnoreCase))
            {
                part = ExpressionTriggerPart.Eyes;
                prefixLength = 5;
                return true;
            }

            if (value.StartsWith("mouth:", StringComparison.OrdinalIgnoreCase))
            {
                part = ExpressionTriggerPart.Mouth;
                prefixLength = 6;
                return true;
            }

            part = ExpressionTriggerPart.None;
            prefixLength = 0;
            return false;
        }
    }
}
