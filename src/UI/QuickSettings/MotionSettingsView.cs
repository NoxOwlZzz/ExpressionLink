using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class MotionSettingsView
    {
        private readonly MotionSettingsDraft _draft;

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
            QuickSettingsGui.Label(status.MotionStateLine);
            if (controller != null)
            {
                QuickSettingsGui.Label(status.WeightsLine);
            }

            _draft.Enabled = QuickSettingsGui.Toggle(
                _draft.Enabled,
                "Plugin enabled");

            QuickSettingsGui.Space(6f);
            QuickSettingsGui.Label("Horizontal");
            _draft.HorizontalCenterOffset = QuickSettingsGui.Slider(
                "Center",
                _draft.HorizontalCenterOffset,
                -1f,
                1f,
                "0.000");
            _draft.PositiveXInputLimit = QuickSettingsGui.Slider(
                "X+ limit",
                _draft.PositiveXInputLimit,
                0.05f,
                1f,
                "0.00");
            _draft.NegativeXInputLimit = QuickSettingsGui.Slider(
                "X- limit",
                _draft.NegativeXInputLimit,
                0.05f,
                1f,
                "0.00");
            _draft.PositiveXMaxWeight = QuickSettingsGui.Slider(
                "X+ weight",
                _draft.PositiveXMaxWeight,
                0f,
                100f,
                "0");
            _draft.NegativeXMaxWeight = QuickSettingsGui.Slider(
                "X- weight",
                _draft.NegativeXMaxWeight,
                0f,
                100f,
                "0");
            _draft.InvertX = QuickSettingsGui.Toggle(
                _draft.InvertX,
                "Invert X");
            DrawHorizontalCalibrationButtons(controller, compactLayout);

            QuickSettingsGui.Space(6f);
            QuickSettingsGui.Label("Vertical");
            _draft.VerticalCenterOffset = QuickSettingsGui.Slider(
                "Center",
                _draft.VerticalCenterOffset,
                -1f,
                1f,
                "0.000");
            _draft.PositiveYInputLimit = QuickSettingsGui.Slider(
                "Y+ limit",
                _draft.PositiveYInputLimit,
                0.05f,
                1f,
                "0.00");
            _draft.NegativeYInputLimit = QuickSettingsGui.Slider(
                "Y- limit",
                _draft.NegativeYInputLimit,
                0.05f,
                1f,
                "0.00");
            _draft.PositiveYMaxWeight = QuickSettingsGui.Slider(
                "Y+ weight",
                _draft.PositiveYMaxWeight,
                0f,
                100f,
                "0");
            _draft.NegativeYMaxWeight = QuickSettingsGui.Slider(
                "Y- weight",
                _draft.NegativeYMaxWeight,
                0f,
                100f,
                "0");
            _draft.InvertY = QuickSettingsGui.Toggle(
                _draft.InvertY,
                "Invert Y");

            QuickSettingsGui.Space(6f);
            QuickSettingsGui.Label("Blink / smoothing");
            _draft.BlinkMaxWeight = QuickSettingsGui.Slider(
                "Blink weight",
                _draft.BlinkMaxWeight,
                0f,
                100f,
                "0");
            _draft.SmoothingEnabled = QuickSettingsGui.Toggle(
                _draft.SmoothingEnabled,
                "Smoothing");
            _draft.SmoothingSpeed = QuickSettingsGui.Slider(
                "Speed",
                _draft.SmoothingSpeed,
                0.01f,
                30f,
                "0.0");
        }

        private void DrawHorizontalCalibrationButtons(
            EyeMotionCharacterController controller,
            bool compactLayout)
        {
            if (compactLayout)
            {
                QuickSettingsGui.BeginHorizontal();
                DrawCenterAndCurrentButtons(controller);
                QuickSettingsGui.EndHorizontal();
                QuickSettingsGui.BeginHorizontal();
                DrawSensitivityPresetButtons();
                QuickSettingsGui.EndHorizontal();
                return;
            }

            QuickSettingsGui.BeginHorizontal();
            DrawCenterAndCurrentButtons(controller);
            DrawSensitivityPresetButtons();
            QuickSettingsGui.EndHorizontal();
        }

        private void DrawCenterAndCurrentButtons(
            EyeMotionCharacterController controller)
        {
            if (QuickSettingsGui.Button("Center X/Y"))
            {
                if (controller != null && controller.State.LookAvailable)
                {
                    _draft.CalibrateCenter(controller.State);
                }
            }

            if (QuickSettingsGui.Button("Use current X"))
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
            if (QuickSettingsGui.Button("High sensitivity 0.15"))
            {
                _draft.UseHighHorizontalSensitivity();
            }

            if (QuickSettingsGui.Button("Default X 1.00"))
            {
                _draft.UseDefaultHorizontalSensitivity();
            }
        }
    }
}
