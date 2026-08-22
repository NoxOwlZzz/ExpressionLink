using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class VisibilityTargetEditorView
    {
        private readonly VisibilitySettingsDraft _draft;
        private bool _expanded;

        internal VisibilityTargetEditorView(VisibilitySettingsDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            _draft = draft;
        }

        internal void Draw()
        {
            _expanded = QuickSettingsGui.Toggle(
                _expanded,
                "Edit names / targets");
            if (!_expanded)
            {
                return;
            }

            QuickSettingsGui.Label("Hide shape names");
            for (int i = 0; i < _draft.BlendshapeNameCount; i++)
            {
                string label = i <
                    ManualVisibilityCatalog.BlendshapeDisplayNames.Length
                        ? ManualVisibilityCatalog.BlendshapeDisplayNames[i]
                        : "Blendshape " + (i + 1).ToString("00");
                string value = QuickSettingsGui.LabeledTextField(
                    label,
                    _draft.GetBlendshapeName(i));
                _draft.SetBlendshapeName(i, value);
            }

            QuickSettingsGui.Label("Expression mesh targets");
            for (int i = 0; i < _draft.RendererTargetCount; i++)
            {
                string label = i <
                    ManualVisibilityCatalog.RendererDisplayNames.Length
                        ? ManualVisibilityCatalog.RendererDisplayNames[i]
                        : "Renderer " + (i + 1).ToString("00");
                string value = QuickSettingsGui.LabeledTextField(
                    label,
                    _draft.GetRendererTarget(i));
                _draft.SetRendererTarget(i, value);
            }
        }
    }
}
