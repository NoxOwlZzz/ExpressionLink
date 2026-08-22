namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class IrisSettingsDraft
    {
        private readonly string[] _blendshapeNames =
            new string[EyeCustomizationCatalog.ChannelCount];

        internal bool Enabled { get; set; }

        internal float IrisYMaxWeight { get; set; }

        internal float IrisSizeMaxWeight { get; set; }

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
            IrisYMaxWeight = QuickSettingsDraftNormalization.Clamp(
                IrisYMaxWeight,
                0f,
                100f);
            IrisSizeMaxWeight = QuickSettingsDraftNormalization.Clamp(
                IrisSizeMaxWeight,
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
