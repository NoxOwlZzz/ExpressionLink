using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class VisibilitySettingsView
    {
        private readonly VisibilitySettingsDraft _draft;
        private readonly ManualVisibilityRuntimeView _runtimeView;
        private readonly VisibilityTargetEditorView _targetEditor;
        private bool _showLiveDetails;
        private bool _showManualControls;
        private bool _showAdvancedTargetSetup;

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

            QuickSettingsGui.Heading("Highlight and card");
            _draft.FollowBaseGameHighlightVisibility =
                QuickSettingsGui.Toggle(
                    _draft.FollowBaseGameHighlightVisibility,
                    "Hide custom highlights with Erase Highlight");
            _draft.CardPersistenceEnabled = QuickSettingsGui.Toggle(
                _draft.CardPersistenceEnabled,
                "Save choices with character card");

            _showManualControls = QuickSettingsGui.Disclosure(
                _showManualControls,
                "Manual part controls");
            if (_showManualControls)
            {
                QuickSettingsGui.Help(
                    "Changes in this section take effect immediately.");
                _runtimeView.Draw(controller, compactLayout);
            }

            _showAdvancedTargetSetup = QuickSettingsGui.Disclosure(
                _showAdvancedTargetSetup,
                "Advanced target setup");
            if (_showAdvancedTargetSetup)
            {
                QuickSettingsGui.Help(
                    "Change these values only when your headmod uses " +
                    "different names.");
                _draft.ManualHideBlendshapeWeight =
                    QuickSettingsGui.Slider(
                        "Hidden blendshape strength",
                        _draft.ManualHideBlendshapeWeight,
                        0f,
                        100f,
                        "0");
                _targetEditor.Draw();
            }

            _showLiveDetails = QuickSettingsGui.Disclosure(
                _showLiveDetails,
                "Live details");
            if (_showLiveDetails)
            {
                if (controller == null)
                {
                    QuickSettingsGui.Help("No character selected.");
                }
                else
                {
                    QuickSettingsGui.Help(status.HighlightStateLine);
                    QuickSettingsGui.Help(status.CardDataLine);
                }
            }
        }
    }
}
