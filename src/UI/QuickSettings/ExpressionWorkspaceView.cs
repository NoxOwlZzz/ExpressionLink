using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionWorkspaceView
    {
        private readonly ExpressionLinkEditorView _linkEditor;
        private readonly ExpressionSettingsView _compatibilityView;
        private bool _showExpressionMeshCompatibility;

        internal ExpressionWorkspaceView(
            ExpressionLinkEditorView linkEditor,
            ExpressionSettingsView compatibilityView)
        {
            _linkEditor = Require(linkEditor, "linkEditor");
            _compatibilityView = Require(
                compatibilityView,
                "compatibilityView");
        }

        internal bool HasUnsavedChanges
        {
            get
            {
                return _linkEditor.HasUnsavedChanges ||
                    _compatibilityView.HasUnsavedChanges;
            }
        }

        internal void Draw(EyeMotionCharacterController controller)
        {
            _linkEditor.Draw(controller);
            QuickSettingsGui.Space(7f);

            bool requestedCompatibilityVisibility =
                QuickSettingsGui.Disclosure(
                    _showExpressionMeshCompatibility,
                    "Separate ExpressionMesh compatibility");
            if (!requestedCompatibilityVisibility &&
                _compatibilityView.HasUnsavedChanges)
            {
                _showExpressionMeshCompatibility = true;
                QuickSettingsGui.Warning(
                    "Save or discard the ExpressionMesh mapping edits " +
                    "before closing this section.");
            }
            else
            {
                _showExpressionMeshCompatibility =
                    requestedCompatibilityVisibility;
            }

            if (!_showExpressionMeshCompatibility)
            {
                return;
            }

            QuickSettingsGui.Help(
                "Use this only for headmods with separate renderers named " +
                "ExpressionMesh_01 through ExpressionMesh_04.");
            _compatibilityView.Draw(controller);
        }

        internal void RejectNavigationChange()
        {
            if (_linkEditor.HasUnsavedChanges)
            {
                _linkEditor.RejectNavigationChange();
                return;
            }

            _compatibilityView.RejectNavigationChange();
        }

        internal void RejectControllerChange()
        {
            if (_linkEditor.HasUnsavedChanges)
            {
                _linkEditor.RejectControllerChange();
                return;
            }

            _compatibilityView.RejectNavigationChange();
        }

        private static T Require<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }
    }
}
