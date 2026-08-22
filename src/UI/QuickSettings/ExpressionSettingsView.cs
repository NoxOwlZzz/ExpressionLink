using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionSettingsView
    {
        private readonly ExpressionSettingsDraft _draft;
        private readonly string[] _triggers =
            new string[ExpressionTriggerSyntax.SlotCount];

        private int _triggerControllerInstanceId;
        private int _triggerRevision = -1;
        private string _feedback = string.Empty;

        internal ExpressionSettingsView(ExpressionSettingsDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            _draft = draft;
        }

        internal void Draw(EyeMotionCharacterController controller)
        {
            _draft.AutomationEnabled = QuickSettingsGui.Toggle(
                _draft.AutomationEnabled,
                "Automatic expressions");
            _draft.ActivationThreshold = QuickSettingsGui.Slider(
                "Threshold",
                _draft.ActivationThreshold,
                0f,
                1f,
                "0.000");

            if (controller == null)
            {
                QuickSettingsGui.Label("No character.");
                return;
            }

            EnsureTriggerBuffer(controller);
            for (int i = 0; i < _triggers.Length; i++)
            {
                _triggers[i] = QuickSettingsGui.LabeledTextField(
                    "ExpressionMesh " + (i + 1).ToString("00"),
                    _triggers[i]);
                QuickSettingsGui.Label(
                    "  " + controller.GetExpressionTriggerStatus(i));
                DrawCaptureButtons(controller, i);
            }

            if (QuickSettingsGui.Button("Apply triggers"))
            {
                controller.SetExpressionTriggers(
                    _triggers,
                    out _feedback);
                _triggerRevision = controller.ExpressionTriggerRevision;
            }

            if (_feedback.Length > 0)
            {
                QuickSettingsGui.Label(_feedback);
            }
        }

        private void EnsureTriggerBuffer(
            EyeMotionCharacterController controller)
        {
            int instanceId = controller.GetInstanceID();
            int revision = controller.ExpressionTriggerRevision;
            if (_triggerControllerInstanceId == instanceId &&
                _triggerRevision == revision)
            {
                return;
            }

            _triggerControllerInstanceId = instanceId;
            _triggerRevision = revision;
            _feedback = string.Empty;
            for (int i = 0; i < _triggers.Length; i++)
            {
                _triggers[i] = controller.GetExpressionTrigger(i);
            }
        }

        private void DrawCaptureButtons(
            EyeMotionCharacterController controller,
            int slotIndex)
        {
            QuickSettingsGui.BeginHorizontal();
            QuickSettingsGui.Label("Capture:");
            if (QuickSettingsGui.Button("Brow"))
            {
                CaptureCurrentExpression(
                    controller,
                    slotIndex,
                    ExpressionTriggerPart.Brow);
            }

            if (QuickSettingsGui.Button("Eyes"))
            {
                CaptureCurrentExpression(
                    controller,
                    slotIndex,
                    ExpressionTriggerPart.Eyes);
            }

            if (QuickSettingsGui.Button("Mouth"))
            {
                CaptureCurrentExpression(
                    controller,
                    slotIndex,
                    ExpressionTriggerPart.Mouth);
            }

            QuickSettingsGui.EndHorizontal();
        }

        private void CaptureCurrentExpression(
            EyeMotionCharacterController controller,
            int slotIndex,
            ExpressionTriggerPart part)
        {
            string selector = controller.GetCurrentExpressionSelector(part);
            if (selector.Length == 0)
            {
                _feedback =
                    "No current " + part.ToString().ToLowerInvariant() +
                    " pattern is available.";
                return;
            }

            _triggers[slotIndex] = selector;
            _feedback =
                "Captured " + selector + " for " +
                (slotIndex + 1).ToString("00") +
                ". Press Apply triggers.";
        }
    }
}
