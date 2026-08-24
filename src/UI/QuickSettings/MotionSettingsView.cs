using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class MotionSettingsView
    {
        private readonly MotionSettingsDraft _draft;
        private bool _showFineTuning;
        private bool _showLiveValues;

        internal MotionSettingsView(MotionSettingsDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            _draft = draft;
        }

        internal void Draw(
            EyeMotionCharacterController controller,
            LiveStatusSnapshot status,
            bool compactLayout)
        {
            status = status ?? LiveStatusSnapshot.NoCharacter;
            QuickSettingsGui.Help(GetTrackingSummary(controller));

            _draft.Enabled = QuickSettingsGui.Toggle(
                _draft.Enabled,
                "Enable eye tracking");

            QuickSettingsGui.Heading("Quick setup");
            DrawQuickSetupButtons(controller, compactLayout);
            QuickSettingsGui.Help(
                "Set neutral while looking straight. Then set horizontal " +
                "range once while looking fully left and once fully right.");

            _draft.BlinkMaxWeight = QuickSettingsGui.Slider(
                "Blink strength",
                _draft.BlinkMaxWeight,
                0f,
                100f,
                "0");
            _draft.SmoothingEnabled = QuickSettingsGui.Toggle(
                _draft.SmoothingEnabled,
                "Smooth movement");
            if (_draft.SmoothingEnabled)
            {
                _draft.SmoothingSpeed = QuickSettingsGui.Slider(
                    "Smoothing speed",
                    _draft.SmoothingSpeed,
                    0.01f,
                    30f,
                    "0.0");
            }

            _showFineTuning = QuickSettingsGui.Disclosure(
                _showFineTuning,
                "Fine tuning");
            if (_showFineTuning)
            {
                DrawFineTuning();
            }

            _showLiveValues = QuickSettingsGui.Disclosure(
                _showLiveValues,
                "Live values");
            if (_showLiveValues)
            {
                QuickSettingsGui.Label(status.MotionStateLine);
                if (!string.IsNullOrEmpty(status.WeightsLine))
                {
                    QuickSettingsGui.Label(status.WeightsLine);
                }
            }
        }

        private void DrawQuickSetupButtons(
            EyeMotionCharacterController controller,
            bool compactLayout)
        {
            if (compactLayout)
            {
                QuickSettingsGui.BeginHorizontal();
                DrawPoseButtons(controller);
                QuickSettingsGui.EndHorizontal();
                QuickSettingsGui.BeginHorizontal();
                DrawSensitivityPresetButtons();
                QuickSettingsGui.EndHorizontal();
                return;
            }

            QuickSettingsGui.BeginHorizontal();
            DrawPoseButtons(controller);
            DrawSensitivityPresetButtons();
            QuickSettingsGui.EndHorizontal();
        }

        private void DrawPoseButtons(
            EyeMotionCharacterController controller)
        {
            if (QuickSettingsGui.Button("Set neutral pose"))
            {
                if (controller != null && controller.State.LookAvailable)
                {
                    _draft.CalibrateCenter(controller.State);
                }
            }

            if (QuickSettingsGui.Button("Set horizontal range"))
            {
                if (controller != null && controller.State.LookAvailable)
                {
                    _draft.CalibrateHorizontal(
                        controller.State.FinalHorizontal);
                }
            }
        }

        private void DrawSensitivityPresetButtons()
        {
            if (QuickSettingsGui.Button("Sensitive preset"))
            {
                _draft.UseHighHorizontalSensitivity();
            }

            if (QuickSettingsGui.Button("Standard preset"))
            {
                _draft.UseDefaultHorizontalSensitivity();
            }
        }

        private void DrawFineTuning()
        {
            QuickSettingsGui.Help("Lower range = more sensitive.");

            QuickSettingsGui.Heading("Horizontal movement");
            _draft.HorizontalCenterOffset = QuickSettingsGui.Slider(
                "Horizontal center",
                _draft.HorizontalCenterOffset,
                -1f,
                1f,
                "0.000");
            _draft.PositiveXInputLimit = QuickSettingsGui.Slider(
                "Right range",
                _draft.PositiveXInputLimit,
                0.05f,
                1f,
                "0.00");
            _draft.NegativeXInputLimit = QuickSettingsGui.Slider(
                "Left range",
                _draft.NegativeXInputLimit,
                0.05f,
                1f,
                "0.00");
            _draft.PositiveXMaxWeight = QuickSettingsGui.Slider(
                "Right strength",
                _draft.PositiveXMaxWeight,
                0f,
                100f,
                "0");
            _draft.NegativeXMaxWeight = QuickSettingsGui.Slider(
                "Left strength",
                _draft.NegativeXMaxWeight,
                0f,
                100f,
                "0");
            _draft.InvertX = QuickSettingsGui.Toggle(
                _draft.InvertX,
                "Swap Right / Left");

            QuickSettingsGui.Heading("Vertical movement");
            _draft.VerticalCenterOffset = QuickSettingsGui.Slider(
                "Vertical center",
                _draft.VerticalCenterOffset,
                -1f,
                1f,
                "0.000");
            _draft.PositiveYInputLimit = QuickSettingsGui.Slider(
                "Up range",
                _draft.PositiveYInputLimit,
                0.05f,
                1f,
                "0.00");
            _draft.NegativeYInputLimit = QuickSettingsGui.Slider(
                "Down range",
                _draft.NegativeYInputLimit,
                0.05f,
                1f,
                "0.00");
            _draft.PositiveYMaxWeight = QuickSettingsGui.Slider(
                "Up strength",
                _draft.PositiveYMaxWeight,
                0f,
                100f,
                "0");
            _draft.NegativeYMaxWeight = QuickSettingsGui.Slider(
                "Down strength",
                _draft.NegativeYMaxWeight,
                0f,
                100f,
                "0");
            _draft.InvertY = QuickSettingsGui.Toggle(
                _draft.InvertY,
                "Swap Up / Down");
        }

        private static string GetTrackingSummary(
            EyeMotionCharacterController controller)
        {
            if (controller == null)
            {
                return "No character selected.";
            }

            switch (controller.CurrentBindingState)
            {
                case BindingState.Bound:
                    return "Eye tracking is ready.";
                case BindingState.Searching:
                    return "Looking for compatible eye blendshapes...";
                case BindingState.Ambiguous:
                    return "Multiple matching eye meshes were found.";
                case BindingState.Incompatible:
                    return "This character has no compatible eye blendshapes.";
                case BindingState.Disabled:
                    return "Eye tracking is disabled.";
                default:
                    return "Eye tracking is not connected yet.";
            }
        }
    }
}
