using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkResponseSectionView
    {
        internal void Draw(
            ExpressionLinkDefinition draft,
            ref string activeStrengthText,
            Action markDirty)
        {
            ExpressionLinkGUILayout.Heading("3. Choose how it reacts");
            ExpressionLinkGUILayout.BeginFieldRow();
            ExpressionLinkGUILayout.FieldLabel("Behavior");

            bool switchSelected =
                draft.Mode == ExpressionLinkMode.Binary;
            if (ExpressionLinkGUILayout.ChoiceButton(
                    "Switch fully",
                    switchSelected) &&
                !switchSelected)
            {
                draft.Mode = ExpressionLinkMode.Binary;
                markDirty();
            }

            bool followSelected =
                draft.Mode == ExpressionLinkMode.FollowSource;
            if (ExpressionLinkGUILayout.ChoiceButton(
                    "Follow expression",
                    followSelected) &&
                !followSelected)
            {
                draft.Mode = ExpressionLinkMode.FollowSource;
                markDirty();
            }

            ExpressionLinkGUILayout.EndFieldRow();
            ExpressionLinkGUILayout.Help(
                draft.Mode == ExpressionLinkMode.Binary
                    ? "Switch fully turns the shape on after the expression " +
                      "passes the activation point."
                    : "Follow expression blends the shape gradually with " +
                      "the facial expression.");

            string original = activeStrengthText ?? string.Empty;
            string edited = ExpressionLinkGUILayout.NumericTextField(
                "Maximum strength",
                original);
            if (!string.Equals(edited, original, StringComparison.Ordinal))
            {
                activeStrengthText = edited;
                markDirty();
            }
        }
    }
}
