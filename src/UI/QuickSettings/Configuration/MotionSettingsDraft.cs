using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class MotionSettingsDraft
    {
        internal bool Enabled { get; set; }

        internal bool InvertX { get; set; }

        internal bool InvertY { get; set; }

        internal bool SmoothingEnabled { get; set; }

        internal float HorizontalCenterOffset { get; set; }

        internal float PositiveXInputLimit { get; set; }

        internal float NegativeXInputLimit { get; set; }

        internal float PositiveXMaxWeight { get; set; }

        internal float NegativeXMaxWeight { get; set; }

        internal float VerticalCenterOffset { get; set; }

        internal float PositiveYInputLimit { get; set; }

        internal float NegativeYInputLimit { get; set; }

        internal float PositiveYMaxWeight { get; set; }

        internal float NegativeYMaxWeight { get; set; }

        internal float BlinkMaxWeight { get; set; }

        internal float SmoothingSpeed { get; set; }

        internal void NormalizeForSave()
        {
            HorizontalCenterOffset = Clamp(HorizontalCenterOffset, -1f, 1f);
            PositiveXInputLimit = Clamp(PositiveXInputLimit, 0.05f, 1f);
            NegativeXInputLimit = Clamp(NegativeXInputLimit, 0.05f, 1f);
            PositiveXMaxWeight = Clamp(PositiveXMaxWeight, 0f, 100f);
            NegativeXMaxWeight = Clamp(NegativeXMaxWeight, 0f, 100f);
            VerticalCenterOffset = Clamp(VerticalCenterOffset, -1f, 1f);
            PositiveYInputLimit = Clamp(PositiveYInputLimit, 0.05f, 1f);
            NegativeYInputLimit = Clamp(NegativeYInputLimit, 0.05f, 1f);
            PositiveYMaxWeight = Clamp(PositiveYMaxWeight, 0f, 100f);
            NegativeYMaxWeight = Clamp(NegativeYMaxWeight, 0f, 100f);
            BlinkMaxWeight = Clamp(BlinkMaxWeight, 0f, 100f);
            SmoothingSpeed = Clamp(SmoothingSpeed, 0.01f, 30f);
        }

        internal void CalibrateCenter(EyeState state)
        {
            HorizontalCenterOffset =
                Clamp(state.HorizontalSourceRaw, -1f, 1f);
            VerticalCenterOffset = Clamp(state.Vertical, -1f, 1f);
        }

        internal void CalibrateHorizontal(float horizontal)
        {
            float limit = Clamp((float)Math.Abs(horizontal), 0.05f, 1f);
            if (horizontal > 0f)
            {
                PositiveXInputLimit = limit;
            }
            else if (horizontal < 0f)
            {
                NegativeXInputLimit = limit;
            }
        }

        internal void UseHighHorizontalSensitivity()
        {
            PositiveXInputLimit = 0.15f;
            NegativeXInputLimit = 0.15f;
        }

        internal void UseDefaultHorizontalSensitivity()
        {
            PositiveXInputLimit = 1f;
            NegativeXInputLimit = 1f;
        }

        private static float Clamp(
            float value,
            float minimum,
            float maximum)
        {
            return QuickSettingsDraftNormalization.Clamp(
                value,
                minimum,
                maximum);
        }
    }
}
