using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class IrisSettingsView
    {
        private readonly IrisSettingsDraft _draft;
        private bool _showBlendshapeNames;

        internal IrisSettingsView(IrisSettingsDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            _draft = draft;
        }

        internal void Draw(LiveStatusSnapshot status)
        {
            status = status ?? LiveStatusSnapshot.NoCharacter;
            QuickSettingsGui.Label(status.IrisStateLine);
            _draft.Enabled = QuickSettingsGui.Toggle(
                _draft.Enabled,
                "Enable IrisY / Size");
            _draft.IrisYMaxWeight = QuickSettingsGui.Slider(
                "IrisY weight",
                _draft.IrisYMaxWeight,
                0f,
                100f,
                "0");
            _draft.IrisSizeMaxWeight = QuickSettingsGui.Slider(
                "Size weight",
                _draft.IrisSizeMaxWeight,
                0f,
                100f,
                "0");
            _showBlendshapeNames = QuickSettingsGui.Toggle(
                _showBlendshapeNames,
                "Edit shape names");
            if (!_showBlendshapeNames)
            {
                return;
            }

            int count = Math.Min(
                _draft.BlendshapeNameCount,
                EyeCustomizationCatalog.ChannelCount);
            for (int i = 0; i < count; i++)
            {
                string name = QuickSettingsGui.LabeledTextField(
                    EyeCustomizationCatalog.DisplayNames[i],
                    _draft.GetBlendshapeName(i));
                _draft.SetBlendshapeName(i, name);
            }
        }
    }
}
