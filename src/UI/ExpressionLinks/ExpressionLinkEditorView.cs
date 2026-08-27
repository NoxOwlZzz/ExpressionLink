using System;
using System.Globalization;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkEditorView
    {
        private const int MaximumStatusLength = 180;
        private const int MaximumNavigationNameLength = 24;

        private static readonly CultureInfo InvariantCulture =
            CultureInfo.InvariantCulture;

        private readonly ExpressionLinkDraftPanel _draftPanel;
        private readonly ExpressionLinkProfilePanel _profilePanel;

        private int _controllerInstanceId;
        private int _observedRevision = -1;
        private int _selectedIndex;
        private int _pendingDeleteIndex = -1;
        private bool _isNewDraft;
        private string _feedback = string.Empty;

        internal ExpressionLinkEditorView()
        {
            _draftPanel = new ExpressionLinkDraftPanel();
            _profilePanel = new ExpressionLinkProfilePanel();
        }

        internal bool HasUnsavedChanges
        {
            get { return _isNewDraft || _draftPanel.IsDirty; }
        }

        internal void ResetSelection()
        {
            _controllerInstanceId = 0;
            _observedRevision = -1;
            _selectedIndex = 0;
            _pendingDeleteIndex = -1;
            _isNewDraft = false;
            _feedback = string.Empty;
            _draftPanel.Reset();
            _profilePanel.CancelPendingReplace();
        }

        internal void RejectControllerChange()
        {
            RejectNavigationChange();
        }

        internal void RejectNavigationChange()
        {
            SetFeedback("Save or cancel the current expression link first.");
        }

        internal void Draw(EyeMotionCharacterController controller)
        {
            ExpressionLinkGUILayout.Heading("Expression links");
            ExpressionLinkGUILayout.Help(
                "Choose a game expression, then choose the blendshape it " +
                "should control.");

            if (controller == null)
            {
                if (HasUnsavedChanges)
                {
                    SetFeedback(
                        "The character being edited is no longer available. " +
                        "The draft is still preserved.");
                    DrawUnavailableDraft(null);
                }
                else
                {
                    ResetSelection();
                    ExpressionLinkGUILayout.Help(
                        "Select a character to create or edit expression links.");
                }

                return;
            }

            if (!EnsureSelection(controller))
            {
                DrawUnavailableDraft(controller);
                return;
            }

            DrawNavigation(controller);

            if (_draftPanel.HasDraft)
            {
                string draftFeedback = _draftPanel.Draw(controller);
                if (_draftPanel.ChangedDuringLastDraw)
                {
                    _feedback = string.Empty;
                    _pendingDeleteIndex = -1;
                }

                if (draftFeedback.Length > 0)
                {
                    SetFeedback(draftFeedback);
                }

                if (!IsDeleteConfirmationPending())
                {
                    DrawSaveButtons(controller);
                }
            }
            else
            {
                ExpressionLinkGUILayout.Help(
                    "No expression links yet. Select Create link to begin.");
            }

            string profileFeedback;
            bool linksReplaced = _profilePanel.Draw(
                controller,
                HasUnsavedChanges,
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

        private bool EnsureSelection(EyeMotionCharacterController controller)
        {
            int instanceId = controller.GetInstanceID();
            int revision = controller.ExpressionLinkRevision;
            if (_controllerInstanceId == instanceId &&
                _observedRevision == revision)
            {
                return true;
            }

            if (HasUnsavedChanges)
            {
                SetFeedback(_controllerInstanceId != instanceId
                    ? "The selected character changed while this draft was " +
                      "open. The draft is preserved until you discard it."
                    : "The stored links changed while this draft was open. " +
                      "The draft is preserved until you discard it.");
                return false;
            }

            _controllerInstanceId = instanceId;
            ClampSelection(controller.ExpressionLinkCount);
            LoadSelected(controller);
            return true;
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
                GetNavigationLabel(count));

            if (ExpressionLinkGUILayout.NarrowButton(">") && count > 1)
            {
                TrySelect(controller, _selectedIndex + 1);
            }

            ExpressionLinkGUILayout.EndHorizontal();

            ExpressionLinkGUILayout.BeginHorizontal();
            if (ExpressionLinkGUILayout.Button("Create link"))
            {
                if (RequireCleanDraft())
                {
                    BeginNewDraft();
                }
            }

            bool previousGuiEnabled = GUI.enabled;
            GUI.enabled = previousGuiEnabled && _draftPanel.HasDraft;
            if (ExpressionLinkGUILayout.Button("Duplicate"))
            {
                if (RequireCleanDraft())
                {
                    BeginDuplicateDraft();
                }
            }

            if (ExpressionLinkGUILayout.Button("Delete"))
            {
                if (RequireCleanDraft())
                {
                    _pendingDeleteIndex = _selectedIndex;
                    SetFeedback(
                        "Select Confirm delete to remove this link.");
                }
            }

            GUI.enabled = previousGuiEnabled;
            ExpressionLinkGUILayout.EndHorizontal();

            if (!IsDeleteConfirmationPending())
            {
                return;
            }

            ExpressionLinkGUILayout.BeginHorizontal();
            if (ExpressionLinkGUILayout.Button("Confirm delete"))
            {
                if (RequireCleanDraft())
                {
                    DeleteSelected(controller);
                }
            }

            if (ExpressionLinkGUILayout.Button("Cancel"))
            {
                _pendingDeleteIndex = -1;
                SetFeedback("Delete cancelled.");
            }

            ExpressionLinkGUILayout.EndHorizontal();
        }

        private void DrawSaveButtons(
            EyeMotionCharacterController controller)
        {
            ExpressionLinkGUILayout.BeginHorizontal();
            if (ExpressionLinkGUILayout.PrimaryButton(
                    "Save expression link"))
            {
                SaveDraft(controller);
            }

            if (ExpressionLinkGUILayout.Button("Cancel"))
            {
                CancelDraft(controller);
            }

            ExpressionLinkGUILayout.EndHorizontal();
        }

        private void DrawStatus(
            EyeMotionCharacterController controller)
        {
            if (_feedback.Length > 0)
            {
                ExpressionLinkGUILayout.Help(
                    "Message: " + NormalizeStatus(_feedback));
            }

            string status;
            if (_isNewDraft)
            {
                status =
                    "Complete the three steps, then save the expression link.";
            }
            else if (_draftPanel.HasDraft)
            {
                status = controller.GetExpressionLinkStatus(_selectedIndex);
            }
            else
            {
                status = "No expression links.";
            }

            status = NormalizeStatus(status);
            if (string.IsNullOrEmpty(status))
            {
                status = "Ready.";
            }

            if (HasUnsavedChanges && !_isNewDraft)
            {
                status += " The open draft has not been saved.";
            }

            ExpressionLinkGUILayout.Help(
                "Status: " + NormalizeStatus(status));
        }

        private void DrawUnavailableDraft(
            EyeMotionCharacterController controller)
        {
            ExpressionLinkGUILayout.Warning(_feedback);
            string label = controller == null
                ? "Discard unavailable draft"
                : "Discard draft and refresh";
            if (!ExpressionLinkGUILayout.Button(label))
            {
                return;
            }

            if (controller == null)
            {
                ResetSelection();
                return;
            }

            _controllerInstanceId = controller.GetInstanceID();
            LoadSelected(controller);
            SetFeedback("Draft discarded; selected character refreshed.");
        }

        private bool RequireCleanDraft()
        {
            if (!HasUnsavedChanges)
            {
                return true;
            }

            RejectNavigationChange();
            return false;
        }

        private void TrySelect(
            EyeMotionCharacterController controller,
            int requestedIndex)
        {
            if (!RequireCleanDraft())
            {
                return;
            }

            Select(controller, requestedIndex);
        }

        private void BeginNewDraft()
        {
            _draftPanel.Load(ExpressionLinkDefinition.CreateDefault());
            _isNewDraft = true;
            _pendingDeleteIndex = -1;
            _profilePanel.CancelPendingReplace();
            SetFeedback(
                "Capture an expression, choose a target, and save when ready.");
        }

        private void BeginDuplicateDraft()
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
            _draftPanel.Load(duplicate);
            _isNewDraft = true;
            _pendingDeleteIndex = -1;
            _profilePanel.CancelPendingReplace();
            SetFeedback(
                "Edit the copy, then save it as a new expression link.");
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
                    ? "The expression link could not be deleted."
                    : message);
                return;
            }

            ClampSelection(controller.ExpressionLinkCount);
            LoadSelected(controller);
            SetFeedback(message.Length == 0
                ? "Expression link deleted."
                : message);
        }

        private void SaveDraft(
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
            if (_isNewDraft)
            {
                int newIndex;
                if (!controller.AddExpressionLink(
                        candidate,
                        out newIndex,
                        out message))
                {
                    SetFeedback(message.Length == 0
                        ? "The expression link could not be saved."
                        : message);
                    return;
                }

                _selectedIndex = newIndex;
            }
            else if (!controller.UpdateExpressionLink(
                    _selectedIndex,
                    candidate,
                    out message))
            {
                SetFeedback(message.Length == 0
                    ? "The expression link could not be saved."
                    : message);
                return;
            }

            LoadSelected(controller);
            SetFeedback(message.Length == 0
                ? "Expression link saved."
                : message);
        }

        private void CancelDraft(
            EyeMotionCharacterController controller)
        {
            bool cancelledNewDraft = _isNewDraft;
            LoadSelected(controller);
            SetFeedback(cancelledNewDraft
                ? "New expression link cancelled."
                : "Edits discarded.");
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
            _profilePanel.CancelPendingReplace();
            int count = controller.ExpressionLinkCount;
            ClampSelection(count);
            ExpressionLinkDefinition selected = count == 0
                ? null
                : controller.GetExpressionLink(_selectedIndex);
            _draftPanel.Load(selected);
            _pendingDeleteIndex = -1;
            _isNewDraft = false;
            _observedRevision = controller.ExpressionLinkRevision;
            _feedback = string.Empty;
        }

        private string GetNavigationLabel(int count)
        {
            if (_isNewDraft)
            {
                return "New expression link";
            }

            if (count <= 0)
            {
                return "No expression links";
            }

            string label = "Link " +
                (_selectedIndex + 1).ToString(InvariantCulture) +
                " of " + count.ToString(InvariantCulture);
            string name = _draftPanel.DisplayName.Trim();
            if (name.Length > MaximumNavigationNameLength)
            {
                name = name.Substring(
                    0,
                    MaximumNavigationNameLength - 3) + "...";
            }

            return name.Length == 0 ? label : label + " - " + name;
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

        private bool IsDeleteConfirmationPending()
        {
            return _pendingDeleteIndex == _selectedIndex &&
                !_isNewDraft;
        }
    }
}
