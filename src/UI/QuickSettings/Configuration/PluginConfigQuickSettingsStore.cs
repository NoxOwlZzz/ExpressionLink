using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class PluginConfigQuickSettingsStore :
        IQuickSettingsConfigStore
    {
        public void Load(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            VisibilitySettingsDraft visibility)
        {
            ValidateDrafts(motion, iris, visibility);
            LoadMotion(motion);
            LoadIris(iris);
            LoadVisibility(visibility);
        }

        public void Save(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            VisibilitySettingsDraft visibility)
        {
            ValidateDrafts(motion, iris, visibility);
            PluginConfig.ApplyBatch(delegate
            {
                SaveMotion(motion);
                SaveIris(iris);
                SaveVisibility(visibility);
            });
        }

        private static void LoadMotion(MotionSettingsDraft draft)
        {
            draft.Enabled = PluginConfig.Enabled.Value;
            draft.InvertX = PluginConfig.InvertX.Value;
            draft.InvertY = PluginConfig.InvertY.Value;
            draft.SmoothingEnabled = PluginConfig.SmoothingEnabled.Value;
            draft.HorizontalCenterOffset =
                PluginConfig.HorizontalCenterOffset.Value;
            draft.PositiveXInputLimit = PluginConfig.PositiveXInputLimit.Value;
            draft.NegativeXInputLimit = PluginConfig.NegativeXInputLimit.Value;
            draft.PositiveXMaxWeight = PluginConfig.PositiveXMaxWeight.Value;
            draft.NegativeXMaxWeight = PluginConfig.NegativeXMaxWeight.Value;
            draft.VerticalCenterOffset = PluginConfig.VerticalCenterOffset.Value;
            draft.PositiveYInputLimit = PluginConfig.PositiveYInputLimit.Value;
            draft.NegativeYInputLimit = PluginConfig.NegativeYInputLimit.Value;
            draft.PositiveYMaxWeight = PluginConfig.PositiveYMaxWeight.Value;
            draft.NegativeYMaxWeight = PluginConfig.NegativeYMaxWeight.Value;
            draft.BlinkMaxWeight = PluginConfig.BlinkMaxWeight.Value;
            draft.SmoothingSpeed = PluginConfig.SmoothingSpeed.Value;
        }

        private static void SaveMotion(MotionSettingsDraft draft)
        {
            PluginConfig.Enabled.Value = draft.Enabled;
            PluginConfig.InvertX.Value = draft.InvertX;
            PluginConfig.InvertY.Value = draft.InvertY;
            PluginConfig.SmoothingEnabled.Value = draft.SmoothingEnabled;
            PluginConfig.HorizontalCenterOffset.Value =
                draft.HorizontalCenterOffset;
            PluginConfig.PositiveXInputLimit.Value =
                draft.PositiveXInputLimit;
            PluginConfig.NegativeXInputLimit.Value =
                draft.NegativeXInputLimit;
            PluginConfig.PositiveXMaxWeight.Value =
                draft.PositiveXMaxWeight;
            PluginConfig.NegativeXMaxWeight.Value =
                draft.NegativeXMaxWeight;
            PluginConfig.VerticalCenterOffset.Value =
                draft.VerticalCenterOffset;
            PluginConfig.PositiveYInputLimit.Value =
                draft.PositiveYInputLimit;
            PluginConfig.NegativeYInputLimit.Value =
                draft.NegativeYInputLimit;
            PluginConfig.PositiveYMaxWeight.Value =
                draft.PositiveYMaxWeight;
            PluginConfig.NegativeYMaxWeight.Value =
                draft.NegativeYMaxWeight;
            PluginConfig.BlinkMaxWeight.Value = draft.BlinkMaxWeight;
            PluginConfig.SmoothingSpeed.Value = draft.SmoothingSpeed;
        }

        private static void LoadIris(IrisSettingsDraft draft)
        {
            draft.Enabled = PluginConfig.EyeAdjustmentEnabled.Value;
            draft.IrisYMaxWeight = PluginConfig.IrisYMaxWeight.Value;
            draft.IrisSizeMaxWeight = PluginConfig.IrisSizeMaxWeight.Value;
            for (int i = 0; i < draft.BlendshapeNameCount; i++)
            {
                draft.SetBlendshapeName(
                    i,
                    PluginConfig.GetEyeAdjustmentBlendshapeName(i));
            }
        }

        private static void SaveIris(IrisSettingsDraft draft)
        {
            PluginConfig.EyeAdjustmentEnabled.Value = draft.Enabled;
            PluginConfig.IrisYMaxWeight.Value = draft.IrisYMaxWeight;
            PluginConfig.IrisSizeMaxWeight.Value = draft.IrisSizeMaxWeight;
            for (int i = 0; i < draft.BlendshapeNameCount; i++)
            {
                PluginConfig.EyeAdjustmentBlendshapeNames[i].Value =
                    draft.GetBlendshapeName(i);
            }
        }

        private static void LoadVisibility(VisibilitySettingsDraft draft)
        {
            draft.ManualHideBlendshapeWeight =
                PluginConfig.ManualHideBlendshapeWeight.Value;
            draft.FollowBaseGameHighlightVisibility =
                PluginConfig.FollowBaseGameHighlightVisibility.Value;
            draft.CardPersistenceEnabled =
                PluginConfig.CardPersistenceEnabled.Value;
            for (int i = 0; i < draft.BlendshapeNameCount; i++)
            {
                draft.SetBlendshapeName(
                    i,
                    PluginConfig.GetManualVisibilityBlendshapeName(i));
            }
        }

        private static void SaveVisibility(VisibilitySettingsDraft draft)
        {
            PluginConfig.ManualHideBlendshapeWeight.Value =
                draft.ManualHideBlendshapeWeight;
            PluginConfig.FollowBaseGameHighlightVisibility.Value =
                draft.FollowBaseGameHighlightVisibility;
            PluginConfig.CardPersistenceEnabled.Value =
                draft.CardPersistenceEnabled;
            for (int i = 0; i < draft.BlendshapeNameCount; i++)
            {
                PluginConfig.ManualVisibilityBlendshapeNames[i].Value =
                    draft.GetBlendshapeName(i);
            }
        }

        private static void ValidateDrafts(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            VisibilitySettingsDraft visibility)
        {
            if (motion == null)
            {
                throw new ArgumentNullException("motion");
            }

            if (iris == null)
            {
                throw new ArgumentNullException("iris");
            }

            if (visibility == null)
            {
                throw new ArgumentNullException("visibility");
            }
        }
    }
}
