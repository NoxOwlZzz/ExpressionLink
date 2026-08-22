namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class VisibilitySettingsDraft
    {
        private readonly string[] _blendshapeNames =
            new string[ManualVisibilityCatalog.BlendshapeCount];
        private readonly string[] _rendererTargets =
            new string[ManualVisibilityCatalog.RendererCount];

        internal float ManualHideBlendshapeWeight { get; set; }

        internal bool FollowBaseGameHighlightVisibility { get; set; }

        internal bool CardPersistenceEnabled { get; set; }

        internal int BlendshapeNameCount
        {
            get { return _blendshapeNames.Length; }
        }

        internal int RendererTargetCount
        {
            get { return _rendererTargets.Length; }
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

        internal string GetRendererTarget(int index)
        {
            QuickSettingsDraftNormalization.ValidateIndex(
                index,
                _rendererTargets.Length);
            return _rendererTargets[index];
        }

        internal void SetRendererTarget(int index, string value)
        {
            QuickSettingsDraftNormalization.ValidateIndex(
                index,
                _rendererTargets.Length);
            _rendererTargets[index] = value ?? string.Empty;
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

            for (int i = 0; i < _rendererTargets.Length; i++)
            {
                _rendererTargets[i] =
                    QuickSettingsDraftNormalization.Name(
                        _rendererTargets[i]);
            }
        }
    }
}
