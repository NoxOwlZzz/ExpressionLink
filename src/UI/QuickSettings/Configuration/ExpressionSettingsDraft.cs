namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionSettingsDraft
    {
        internal bool AutomationEnabled { get; set; }

        internal float ActivationThreshold { get; set; }

        internal void NormalizeForSave()
        {
            ActivationThreshold =
                QuickSettingsDraftNormalization.Clamp(
                    ActivationThreshold,
                    0f,
                    1f);
        }
    }
}
