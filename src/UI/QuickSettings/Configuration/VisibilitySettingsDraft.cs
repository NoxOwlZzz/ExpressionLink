namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class VisibilitySettingsDraft
    {
        private readonly string[] _blendshapeNames =
            new string[ManualVisibilityCatalog.UserFacingBlendshapeCount];

        internal float ManualHideBlendshapeWeight { get; set; }

        internal bool FollowBaseGameHighlightVisibility { get; set; }

        internal bool CardPersistenceEnabled { get; set; }

        internal int BlendshapeNameCount
        {
            get { return _blendshapeNames.Length; }
        }

        internal string GetBlendshapeName(int index)
        {
            QuickSettingsDraftNormalization.ValidateIndex(
                index,
                _blendshapeNames.Length);
            return _blendshapeNames[index];
        }

        internal void SetBlendshapeName(int index, string value)
        {
            QuickSettingsDraftNormalization.ValidateIndex(
                index,
                _blendshapeNames.Length);
            _blendshapeNames[index] = value ?? string.Empty;
        }

        internal void NormalizeForSave()
        {
            ManualHideBlendshapeWeight =
                QuickSettingsDraftNormalization.Clamp(
                    ManualHideBlendshapeWeight,
                    0f,
                    100f);
            for (int i = 0; i < _blendshapeNames.Length; i++)
            {
                _blendshapeNames[i] =
                    QuickSettingsDraftNormalization.Name(
                        _blendshapeNames[i]);
            }
        }
    }
}
