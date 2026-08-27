using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkSourceSectionView
    {
        internal string Draw(
            EyeMotionCharacterController controller,
            ExpressionLinkDefinition draft,
            Action markDirty)
        {
            ExpressionLinkGUILayout.Heading("1. Choose when it happens");
            ExpressionLinkGUILayout.Help(
                "Pose the character, then capture the facial part that " +
                "best identifies the expression.");

            string feedback = string.Empty;
            ExpressionLinkGUILayout.BeginHorizontal();
            SetLatest(
                ref feedback,
                DrawCaptureButton(
                    controller,
                    draft,
                    markDirty,
                    "Use current eyes",
                    "eyes",
                    ExpressionTriggerPart.Eyes));
            SetLatest(
                ref feedback,
                DrawCaptureButton(
                    controller,
                    draft,
                    markDirty,
                    "Use current brows",
                    "brows",
                    ExpressionTriggerPart.Brow));
            SetLatest(
                ref feedback,
                DrawCaptureButton(
                    controller,
                    draft,
                    markDirty,
                    "Use current mouth",
                    "mouth",
                    ExpressionTriggerPart.Mouth));
            ExpressionLinkGUILayout.EndHorizontal();

            if (ExpressionLinkDraftRules.IsBlank(draft.Source))
            {
                if (draft.Enabled)
                {
                    ExpressionLinkGUILayout.Warning(
                        "Capture an expression before saving this enabled link.");
                }
                else
                {
                    ExpressionLinkGUILayout.Help(
                        "A source is optional while this link is disabled.");
                }
            }
            else
            {
                ExpressionLinkGUILayout.StatusBadge(
                    GetSelectedExpressionLabel(draft.Source));
            }

            return feedback;
        }

        private static string DrawCaptureButton(
            EyeMotionCharacterController controller,
            ExpressionLinkDefinition draft,
            Action markDirty,
            string buttonLabel,
            string partLabel,
            ExpressionTriggerPart part)
        {
            if (!ExpressionLinkGUILayout.Button(buttonLabel))
            {
                return string.Empty;
            }

            string selector = controller == null
                ? string.Empty
                : controller.GetCurrentExpressionSelector(part);
            if (string.IsNullOrEmpty(selector))
            {
                return "No current " + partLabel +
                    " expression is available.";
            }

            draft.Source = selector;
            markDirty();
            return "Current " + partLabel + " expression selected.";
        }

        private static string GetSelectedExpressionLabel(string selector)
        {
            if (selector.StartsWith(
                    "eyes:",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Current eyes expression selected";
            }

            if (selector.StartsWith(
                    "brow:",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Current brows expression selected";
            }

            if (selector.StartsWith(
                    "mouth:",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Current mouth expression selected";
            }

            return "Expression selected";
        }

        private static void SetLatest(
            ref string destination,
            string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                destination = message;
            }
        }
    }
}
