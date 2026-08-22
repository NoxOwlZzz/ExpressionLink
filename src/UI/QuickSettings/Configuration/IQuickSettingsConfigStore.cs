namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal interface IQuickSettingsConfigStore
    {
        void Load(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            ExpressionSettingsDraft expressions,
            VisibilitySettingsDraft visibility);

        void Save(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            ExpressionSettingsDraft expressions,
            VisibilitySettingsDraft visibility);
    }
}
