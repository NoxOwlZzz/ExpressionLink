using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkTargetSectionView
    {
        private const int FirstScope = (int)ExpressionTargetScope.Any;
        private const int LastScope = (int)ExpressionTargetScope.Other;

        internal void Draw(
            ExpressionLinkDefinition draft,
            Action markDirty,
            Action<ExpressionTargetScope> scopeChanged)
        {
            ExpressionLinkGUILayout.Heading("2. Choose what changes");
            ExpressionLinkGUILayout.Help(
                "Choose the character area, then enter the blendshape name " +
                "exactly as it appears on the mesh.");

            string original = draft.BlendshapeName ?? string.Empty;
            string edited = ExpressionLinkGUILayout.TextField(
                "Blendshape name",
                original);
            if (!string.Equals(edited, original, StringComparison.Ordinal))
            {
                draft.BlendshapeName = edited;
                markDirty();
            }

            if (ExpressionLinkDraftRules.IsBlank(draft.BlendshapeName))
            {
                if (draft.Enabled)
                {
                    ExpressionLinkGUILayout.Warning(
                        "Enter a blendshape name before saving this enabled link.");
                }
                else
                {
                    ExpressionLinkGUILayout.Help(
                        "A target is optional while this link is disabled.");
                }
            }

            ExpressionLinkGUILayout.BeginFieldRow();
            ExpressionLinkGUILayout.FieldLabel("Character area");
            if (ExpressionLinkGUILayout.NarrowButton("<"))
            {
                CycleScope(
                    draft,
                    -1,
                    markDirty,
                    scopeChanged);
            }

            if (ExpressionLinkGUILayout.Button(GetScopeName(draft.Scope)))
            {
                CycleScope(
                    draft,
                    1,
                    markDirty,
                    scopeChanged);
            }

            if (ExpressionLinkGUILayout.NarrowButton(">"))
            {
                CycleScope(
                    draft,
                    1,
                    markDirty,
                    scopeChanged);
            }

            ExpressionLinkGUILayout.EndFieldRow();
        }

        private static void CycleScope(
            ExpressionLinkDefinition draft,
            int direction,
            Action markDirty,
            Action<ExpressionTargetScope> scopeChanged)
        {
            int value = (int)draft.Scope + direction;
            if (value < FirstScope)
            {
                value = LastScope;
            }
            else if (value > LastScope)
            {
                value = FirstScope;
            }

            draft.Scope = (ExpressionTargetScope)value;
            markDirty();
            if (scopeChanged != null)
            {
                scopeChanged(draft.Scope);
            }
        }

        private static string GetScopeName(ExpressionTargetScope scope)
        {
            switch (scope)
            {
                case ExpressionTargetScope.Head:
                    return "Head";
                case ExpressionTargetScope.Hair:
                    return "Hair";
                case ExpressionTargetScope.Body:
                    return "Body";
                case ExpressionTargetScope.Clothes:
                    return "Clothes";
                case ExpressionTargetScope.Accessory:
                    return "Accessory";
                case ExpressionTargetScope.Other:
                    return "Other";
                default:
                    return "All character areas";
            }
        }
    }
}
