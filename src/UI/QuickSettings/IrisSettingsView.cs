using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class IrisSettingsView
    {
        private readonly IrisSettingsDraft _draft;
        private bool _showBlendshapeNames;
        private bool _showLiveValues;

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
            _draft.Enabled = QuickSettingsGui.Toggle(
                _draft.Enabled,
                "Follow game eye controls");
            _draft.IrisYMaxWeight = QuickSettingsGui.Slider(
                "Iris height effect",
                _draft.IrisYMaxWeight,
                0f,
                100f,
                "0");
            _draft.IrisSizeMaxWeight = QuickSettingsGui.Slider(
                "Eye size effect",
                _draft.IrisSizeMaxWeight,
                0f,
                100f,
                "0");

            _showBlendshapeNames = QuickSettingsGui.Disclosure(
                _showBlendshapeNames,
                "Blendshape names");
            if (_showBlendshapeNames)
            {
                DrawBlendshapeNames();
            }

            _showLiveValues = QuickSettingsGui.Disclosure(
                _showLiveValues,
                "Live values");
            if (_showLiveValues)
            {
                QuickSettingsGui.Help(status.IrisStateLine);
            }
        }

        private void DrawBlendshapeNames()
        {
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
