using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class VisibilitySettingsView
    {
        private readonly VisibilitySettingsDraft _draft;
        private readonly ManualVisibilityRuntimeView _runtimeView;
        private readonly VisibilityTargetEditorView _targetEditor;

        internal VisibilitySettingsView(VisibilitySettingsDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            _draft = draft;
            _runtimeView = new ManualVisibilityRuntimeView(draft);
            _targetEditor = new VisibilityTargetEditorView(draft);
        }

        internal void Draw(
            EyeMotionCharacterController controller,
            LiveStatusSnapshot status,
            bool compactLayout)
        {
            status = status ?? LiveStatusSnapshot.NoCharacter;

            _draft.FollowBaseGameHighlightVisibility =
                QuickSettingsGui.Toggle(
                    _draft.FollowBaseGameHighlightVisibility,
                    "Follow Erase Highlight");
            _draft.CardPersistenceEnabled = QuickSettingsGui.Toggle(
                _draft.CardPersistenceEnabled,
                "Save with character card");
            if (controller != null)
            {
                QuickSettingsGui.Label(status.HighlightStateLine);
                QuickSettingsGui.Label(status.CardDataLine);
            }

            _draft.ManualHideBlendshapeWeight = QuickSettingsGui.Slider(
                "Hidden weight",
                _draft.ManualHideBlendshapeWeight,
                0f,
                100f,
                "0");
            _runtimeView.Draw(controller, compactLayout);
            _targetEditor.Draw();
        }
    }
}
