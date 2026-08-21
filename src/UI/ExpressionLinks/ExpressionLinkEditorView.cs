using System;
using System.Globalization;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkEditorView
    {
        private const int MaximumStatusLength = 180;

        private static readonly CultureInfo InvariantCulture =
            CultureInfo.InvariantCulture;

        private readonly ExpressionLinkDraftPanel _draftPanel;
        private readonly ExpressionLinkProfilePanel _profilePanel;

        private int _controllerInstanceId;
        private int _observedRevision = -1;
        private int _selectedIndex;
        private string _feedback = string.Empty;

        internal ExpressionLinkEditorView()
        {
            _draftPanel = new ExpressionLinkDraftPanel();
            _profilePanel = new ExpressionLinkProfilePanel();
        }

        internal bool HasUnsavedChanges
        {
            get { return _draftPanel.IsDirty; }
        }

        internal void ResetSelection()
        {
            _controllerInstanceId = 0;
            _observedRevision = -1;
            _selectedIndex = 0;
            _feedback = string.Empty;
            _draftPanel.Reset();
        }

        internal void RejectControllerChange()
        {
            SetFeedback(
                "Apply or revert the current link before changing character.");
        }

        internal void Draw(EyeMotionCharacterController controller)
        {
            if (controller == null)
            {
                ResetSelection();
                ExpressionLinkGUILayout.Label(
                    "Expression Links: no character selected.");
                return;
            }

            EnsureSelection(controller);
            DrawNavigation(controller);

            if (_draftPanel.HasDraft)
            {
                string draftFeedback = _draftPanel.Draw(controller);
                if (draftFeedback.Length > 0)
                {
                    SetFeedback(draftFeedback);
                }

                DrawApplyButtons(controller);
            }
            else
            {
                ExpressionLinkGUILayout.Label(
                    "No links. Press + to create one.");
            }

            string profileFeedback;
            bool linksReplaced = _profilePanel.Draw(
                controller,
                _draftPanel.IsDirty,
                out profileFeedback);
            if (linksReplaced)
            {
                _selectedIndex = 0;
                LoadSelected(controller);
            }

            if (profileFeedback.Length > 0)
            {
                SetFeedback(profileFeedback);
            }

            DrawStatus(controller);
        }

        private void EnsureSelection(
            EyeMotionCharacterController controller)
        {
            int instanceId = controller.GetInstanceID();
            int revision = controller.ExpressionLinkRevision;
            if (_controllerInstanceId == instanceId &&
                _observedRevision == revision)
            {
                return;
            }

            bool discardedDraft = _draftPanel.IsDirty;
            _controllerInstanceId = instanceId;
            ClampSelection(controller.ExpressionLinkCount);
            LoadSelected(controller);
            if (discardedDraft)
            {
                SetFeedback(
                    "Links changed; the local draft was reloaded.");
            }
        }

        private void DrawNavigation(
            EyeMotionCharacterController controller)
        {
            int count = controller.ExpressionLinkCount;
            ExpressionLinkGUILayout.BeginHorizontal();
            if (ExpressionLinkGUILayout.NarrowButton("<") && count > 1)
            {
                TrySelect(controller, _selectedIndex - 1);
            }

            ExpressionLinkGUILayout.CountLabel(
                count == 0
                    ? "0 / 0"
                    : (_selectedIndex + 1).ToString(InvariantCulture) +
                      " / " + count.ToString(InvariantCulture));

            if (ExpressionLinkGUILayout.NarrowButton(">") && count > 1)
            {
                TrySelect(controller, _selectedIndex + 1);
            }

            if (ExpressionLinkGUILayout.Button("+"))
            {
                if (RequireCleanDraft("creating another link"))
                {
                    AddDefault(controller);
                }
            }

            bool previousGuiEnabled = GUI.enabled;
            GUI.enabled = previousGuiEnabled && _draftPanel.HasDraft;
            if (ExpressionLinkGUILayout.Button("Duplicate"))
            {
                if (RequireCleanDraft("duplicating this link"))
                {
                    DuplicateSelected(controller);
                }
            }

            if (ExpressionLinkGUILayout.Button("Delete"))
            {
                if (RequireCleanDraft("deleting this link"))
                {
                    DeleteSelected(controller);
                }
            }

            GUI.enabled = previousGuiEnabled;
            ExpressionLinkGUILayout.EndHorizontal();
        }

        private void DrawApplyButtons(
            EyeMotionCharacterController controller)
        {
            ExpressionLinkGUILayout.BeginHorizontal();
            if (ExpressionLinkGUILayout.Button(
                    _draftPanel.IsDirty ? "Apply *" : "Apply"))
            {
                ApplyDraft(controller);
            }

            if (ExpressionLinkGUILayout.Button("Revert"))
            {
                LoadSelected(controller);
                SetFeedback("Local changes reverted.");
            }

            ExpressionLinkGUILayout.EndHorizontal();
        }

        private void DrawStatus(
            EyeMotionCharacterController controller)
        {
            string status = _feedback;
            if (status.Length == 0 && _draftPanel.HasDraft)
            {
                status = controller.GetExpressionLinkStatus(
                    _selectedIndex);
            }

            if (string.IsNullOrEmpty(status))
            {
                status = _draftPanel.HasDraft
                    ? "Ready."
                    : "No links.";
            }

            if (_draftPanel.IsDirty && _feedback.Length == 0)
            {
                status += " Not applied.";
            }

            ExpressionLinkGUILayout.Label(
                "Status: " + NormalizeStatus(status));
        }

        private bool RequireCleanDraft(string action)
        {
            if (!_draftPanel.IsDirty)
            {
                return true;
            }

            SetFeedback(
                "Apply or Revert before " + action + ".");
            return false;
        }

        private void TrySelect(
            EyeMotionCharacterController controller,
            int requestedIndex)
        {
            if (!RequireCleanDraft("moving to another link"))
            {
                return;
            }

            Select(controller, requestedIndex);
        }

        private void AddDefault(
            EyeMotionCharacterController controller)
        {
            int newIndex;
            string message;
            if (!controller.AddExpressionLink(
                    ExpressionLinkDefinition.CreateDefault(),
                    out newIndex,
                    out message))
            {
                SetFeedback(message.Length == 0
                    ? "The link could not be created."
                    : message);
                return;
            }

            _selectedIndex = newIndex;
            LoadSelected(controller);
            SetFeedback(message.Length == 0
                ? "Expression link created."
                : message);
        }

        private void DuplicateSelected(
            EyeMotionCharacterController controller)
        {
            ExpressionLinkDefinition duplicate;
            string error;
            if (!_draftPanel.TryBuildCandidate(
                    out duplicate,
                    out error))
            {
                SetFeedback(error);
                return;
            }

            duplicate.Id = Guid.NewGuid();
            duplicate.Name = string.IsNullOrEmpty(duplicate.Name)
                ? "Expression Link Copy"
                : duplicate.Name + " Copy";

            int newIndex;
            string message;
            if (!controller.AddExpressionLink(
                    duplicate,
                    out newIndex,
                    out message))
            {
                SetFeedback(message.Length == 0
                    ? "The link could not be duplicated."
                    : message);
                return;
            }

            _selectedIndex = newIndex;
            LoadSelected(controller);
            SetFeedback(message.Length == 0
                ? "Expression link duplicated."
                : message);
        }

        private void DeleteSelected(
            EyeMotionCharacterController controller)
        {
            if (!_draftPanel.HasDraft)
            {
                return;
            }

            string message;
            if (!controller.RemoveExpressionLink(
                    _selectedIndex,
                    out message))
            {
                SetFeedback(message.Length == 0
                    ? "The link could not be deleted."
                    : message);
                return;
            }

            ClampSelection(controller.ExpressionLinkCount);
            LoadSelected(controller);
            SetFeedback(message.Length == 0
                ? "Expression link deleted."
                : message);
        }

        private void ApplyDraft(
            EyeMotionCharacterController controller)
        {
            ExpressionLinkDefinition candidate;
            string error;
            if (!_draftPanel.TryBuildCandidate(
                    out candidate,
                    out error))
            {
                SetFeedback(error);
                return;
            }

            string message;
            if (!controller.UpdateExpressionLink(
                    _selectedIndex,
                    candidate,
                    out message))
            {
                SetFeedback(message.Length == 0
                    ? "The link could not be applied."
                    : message);
                return;
            }

            LoadSelected(controller);
            SetFeedback(message.Length == 0
                ? "Expression link applied."
                : message);
        }

        private void Select(
            EyeMotionCharacterController controller,
            int requestedIndex)
        {
            int count = controller.ExpressionLinkCount;
            _selectedIndex = count == 0
                ? 0
                : WrapIndex(requestedIndex, count);
            LoadSelected(controller);
        }

        private void LoadSelected(
            EyeMotionCharacterController controller)
        {
            int count = controller.ExpressionLinkCount;
            ClampSelection(count);
            ExpressionLinkDefinition selected = count == 0
                ? null
                : controller.GetExpressionLink(_selectedIndex);
            _draftPanel.Load(selected);
            _observedRevision = controller.ExpressionLinkRevision;
            _feedback = string.Empty;
        }

        private void ClampSelection(int count)
        {
            if (count <= 0)
            {
                _selectedIndex = 0;
                return;
            }

            if (_selectedIndex < 0)
            {
                _selectedIndex = 0;
            }
            else if (_selectedIndex >= count)
            {
                _selectedIndex = count - 1;
            }
        }

        private void SetFeedback(string message)
        {
            _feedback = NormalizeStatus(message);
        }

        private static string NormalizeStatus(string message)
        {
            string value = (message ?? string.Empty)
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Trim();
            while (value.IndexOf(
                    "  ",
                    StringComparison.Ordinal) >= 0)
            {
                value = value.Replace("  ", " ");
            }

            if (value.Length > MaximumStatusLength)
            {
                value = value.Substring(
                    0,
                    MaximumStatusLength - 3) + "...";
            }

            return value;
        }

        private static int WrapIndex(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            value %= count;
            return value < 0 ? value + count : value;
        }
    }
}
