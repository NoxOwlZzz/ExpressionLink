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
        private int _selectedSlotIndex;
        private bool _showFineTuning;
        private bool _triggersDirty;
        private string _feedback = string.Empty;

        internal ExpressionSettingsView(ExpressionSettingsDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            _draft = draft;
        }

        internal bool HasUnsavedChanges
        {
            get { return _triggersDirty; }
        }

        internal void RejectNavigationChange()
        {
            _feedback =
                "Save or discard the ExpressionMesh mappings first.";
        }

        internal void Draw(EyeMotionCharacterController controller)
        {
            QuickSettingsGui.Heading("All characters");
            _draft.AutomationEnabled = QuickSettingsGui.Toggle(
                _draft.AutomationEnabled,
                "Enable separate ExpressionMesh switching");

            _showFineTuning = QuickSettingsGui.Disclosure(
                _showFineTuning,
                "Activation tuning");
            if (_showFineTuning)
            {
                _draft.ActivationThreshold = QuickSettingsGui.Slider(
                    "Activation point",
                    _draft.ActivationThreshold,
                    0f,
                    1f,
                    "0.000");
            }

            QuickSettingsGui.Help(
                "Apply global settings stores the options above.");
            QuickSettingsGui.Heading("Selected character");
            QuickSettingsGui.Help(
                "Each mapping shows or hides a separate renderer named " +
                "ExpressionMesh_01 through ExpressionMesh_04. The mappings " +
                "are saved with the character.");
            if (controller == null)
            {
                QuickSettingsGui.Help(
                    "Select a character to edit ExpressionMesh mappings.");
                if (_triggersDirty)
                {
                    QuickSettingsGui.Help(
                        "The character being edited is no longer available.");
                    if (QuickSettingsGui.Button(
                            "Discard unavailable mapping edits"))
                    {
                        DiscardUnavailableMappings();
                    }
                }

                if (_feedback.Length > 0)
                {
                    QuickSettingsGui.Help(_feedback);
                }

                return;
            }

            if (HasStaleTriggerBuffer(controller))
            {
                QuickSettingsGui.Warning(
                    "These mapping edits belong to another or earlier " +
                    "character state. They are preserved until you discard " +
                    "them.");
                if (QuickSettingsGui.Button(
                        "Discard mapping edits and refresh"))
                {
                    ReloadTriggerBuffer(controller);
                    _feedback =
                        "Mapping edits discarded; selected character refreshed.";
                }

                if (_feedback.Length > 0)
                {
                    QuickSettingsGui.Help(_feedback);
                }
                return;
            }

            EnsureTriggerBuffer(controller);
            DrawSlotNavigation();

            string previous = _triggers[_selectedSlotIndex];
            string edited = QuickSettingsGui.LabeledTextField(
                "Show this mesh when",
                previous);
            if (!string.Equals(
                    edited,
                    previous,
                    StringComparison.Ordinal))
            {
                _triggers[_selectedSlotIndex] = edited;
                _triggersDirty = true;
                _feedback = string.Empty;
            }

            QuickSettingsGui.Help(
                "Status: " + controller.GetExpressionTriggerStatus(
                    _selectedSlotIndex));
            DrawCaptureButtons(controller, _selectedSlotIndex);

            QuickSettingsGui.BeginHorizontal();
            if (QuickSettingsGui.PrimaryButton(
                    "Save ExpressionMesh mappings"))
            {
                controller.SetExpressionTriggers(
                    _triggers,
                    out _feedback);
                _triggerRevision = controller.ExpressionTriggerRevision;
                _triggersDirty = false;
            }

            if (QuickSettingsGui.Button("Discard mapping edits"))
            {
                ReloadTriggerBuffer(controller);
                _feedback = "ExpressionMesh mapping edits discarded.";
            }

            QuickSettingsGui.EndHorizontal();
            if (_triggersDirty)
            {
                QuickSettingsGui.Warning("Unsaved ExpressionMesh mappings.");
            }

            if (_feedback.Length > 0)
            {
                QuickSettingsGui.Help(_feedback);
            }
        }

        private void DrawSlotNavigation()
        {
            QuickSettingsGui.BeginHorizontal();
            if (QuickSettingsGui.Button("<"))
            {
                MoveSelectedSlot(-1);
            }

            QuickSettingsGui.Label(
                "Separate mesh " +
                (_selectedSlotIndex + 1).ToString("00") +
                " of " + _triggers.Length.ToString("00"));

            if (QuickSettingsGui.Button(">"))
            {
                MoveSelectedSlot(1);
            }

            QuickSettingsGui.EndHorizontal();
        }

        private void MoveSelectedSlot(int direction)
        {
            int count = _triggers.Length;
            _selectedSlotIndex =
                (_selectedSlotIndex + direction + count) % count;
            _feedback = string.Empty;
        }

        private bool HasStaleTriggerBuffer(
            EyeMotionCharacterController controller)
        {
            if (!_triggersDirty)
            {
                return false;
            }

            return _triggerControllerInstanceId !=
                    controller.GetInstanceID() ||
                _triggerRevision != controller.ExpressionTriggerRevision;
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

            if (_triggersDirty)
            {
                return;
            }

            ReloadTriggerBuffer(controller);
        }

        private void ReloadTriggerBuffer(
            EyeMotionCharacterController controller)
        {
            _triggerControllerInstanceId = controller.GetInstanceID();
            _triggerRevision = controller.ExpressionTriggerRevision;
            _triggersDirty = false;
            _feedback = string.Empty;
            for (int i = 0; i < _triggers.Length; i++)
            {
                _triggers[i] = controller.GetExpressionTrigger(i);
            }
        }

        private void DiscardUnavailableMappings()
        {
            _triggerControllerInstanceId = 0;
            _triggerRevision = -1;
            _triggersDirty = false;
            _feedback = "Unavailable mapping edits discarded.";
            for (int i = 0; i < _triggers.Length; i++)
            {
                _triggers[i] = string.Empty;
            }
        }

        private void DrawCaptureButtons(
            EyeMotionCharacterController controller,
            int slotIndex)
        {
            QuickSettingsGui.Help("Capture the current expression:");
            QuickSettingsGui.BeginHorizontal();
            if (QuickSettingsGui.Button("Eyes"))
            {
                CaptureCurrentExpression(
                    controller,
                    slotIndex,
                    ExpressionTriggerPart.Eyes);
            }

            if (QuickSettingsGui.Button("Brows"))
            {
                CaptureCurrentExpression(
                    controller,
                    slotIndex,
                    ExpressionTriggerPart.Brow);
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
                    " expression is available.";
                return;
            }

            _triggers[slotIndex] = selector;
            _triggersDirty = true;
            _feedback =
                "Current expression captured for separate mesh " +
                (slotIndex + 1).ToString() + ".";
        }
    }
}
