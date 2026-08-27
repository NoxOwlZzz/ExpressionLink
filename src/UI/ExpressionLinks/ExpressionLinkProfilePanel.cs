using System;
using System.Globalization;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkProfilePanel
    {
        private static readonly CultureInfo InvariantCulture =
            CultureInfo.InvariantCulture;

        private readonly ExpressionLinkProfileStore _profileStore;
        private bool _showProfiles;
        private string _profileName = "Default";
        private string[] _profileNames = new string[0];
        private int _profileIndex = -1;
        private bool _profileNamesLoaded;
        private bool _confirmReplace;
        private int _confirmControllerInstanceId;
        private int _confirmControllerRevision = -1;

        internal ExpressionLinkProfilePanel()
        {
            _profileStore = new ExpressionLinkProfileStore();
        }

        internal bool Draw(
            EyeMotionCharacterController controller,
            bool hasUnappliedDraft,
            out string feedback)
        {
            feedback = string.Empty;
            bool linksReplaced = false;
            if (_confirmReplace &&
                (hasUnappliedDraft || !ConfirmationMatches(controller)))
            {
                CancelPendingReplace();
                feedback = hasUnappliedDraft
                    ? "Replace cancelled. Save or cancel the current " +
                      "expression link first."
                    : "Replace confirmation cancelled because the selected " +
                      "character changed.";
            }

            bool showProfiles = ExpressionLinkGUILayout.Disclosure(
                _showProfiles,
                "Reusable link sets");
            if (showProfiles != _showProfiles)
            {
                _showProfiles = showProfiles;
                if (!_showProfiles)
                {
                    CancelPendingReplace();
                }

                if (_showProfiles && !_profileNamesLoaded)
                {
                    feedback = RefreshProfileNames(false);
                }
            }

            if (!_showProfiles)
            {
                return false;
            }

            ExpressionLinkGUILayout.Help(
                "A reusable set saves or loads every expression link for " +
                "compatible characters.");
            string editedProfileName = ExpressionLinkGUILayout.TextField(
                "Set name",
                _profileName);
            if (!string.Equals(
                    editedProfileName,
                    _profileName,
                    StringComparison.Ordinal))
            {
                _profileName = editedProfileName;
                CancelPendingReplace();
            }
            ExpressionLinkGUILayout.Help(
                "Using an existing name overwrites that saved set.");

            ExpressionLinkGUILayout.BeginHorizontal();
            if (ExpressionLinkGUILayout.Button("Save or overwrite set"))
            {
                feedback = hasUnappliedDraft
                    ? "Save or cancel the current expression link first."
                    : SaveProfile(controller);
                CancelPendingReplace();
            }

            if (ExpressionLinkGUILayout.Button("Load and replace links"))
            {
                if (hasUnappliedDraft)
                {
                    feedback =
                        "Save or cancel the current expression link first.";
                }
                else
                {
                    BeginReplaceConfirmation(controller);
                    feedback =
                        "Select Confirm replace to load this set and replace " +
                        "every current link.";
                }
            }

            ExpressionLinkGUILayout.EndHorizontal();

            if (_confirmReplace)
            {
                ExpressionLinkGUILayout.BeginHorizontal();
                if (ExpressionLinkGUILayout.Button("Confirm replace"))
                {
                    if (hasUnappliedDraft)
                    {
                        feedback =
                            "Replace cancelled. Save or cancel the current " +
                            "expression link first.";
                    }
                    else if (!ConfirmationMatches(controller))
                    {
                        feedback =
                            "Replace cancelled because the selected " +
                            "character changed.";
                    }
                    else
                    {
                        linksReplaced = TryLoadProfile(
                            controller,
                            out feedback);
                    }

                    CancelPendingReplace();
                }

                if (ExpressionLinkGUILayout.Button("Cancel"))
                {
                    CancelPendingReplace();
                    feedback = "Replace cancelled.";
                }

                ExpressionLinkGUILayout.EndHorizontal();
            }

            ExpressionLinkGUILayout.BeginHorizontal();

            if (ExpressionLinkGUILayout.NarrowButton("<"))
            {
                CancelPendingReplace();
                feedback = CycleProfile(-1);
            }

            if (ExpressionLinkGUILayout.NarrowButton(">"))
            {
                CancelPendingReplace();
                feedback = CycleProfile(1);
            }

            if (ExpressionLinkGUILayout.Button("Refresh saved sets"))
            {
                CancelPendingReplace();
                feedback = RefreshProfileNames(true);
            }

            ExpressionLinkGUILayout.EndHorizontal();

            if (_profileNames.Length > 0)
            {
                ExpressionLinkGUILayout.Label(
                    "Saved sets: " +
                    _profileNames.Length.ToString(InvariantCulture));
            }

            return linksReplaced;
        }

        internal void CancelPendingReplace()
        {
            _confirmReplace = false;
            _confirmControllerInstanceId = 0;
            _confirmControllerRevision = -1;
        }

        private void BeginReplaceConfirmation(
            EyeMotionCharacterController controller)
        {
            _confirmReplace = true;
            _confirmControllerInstanceId = controller.GetInstanceID();
            _confirmControllerRevision = controller.ExpressionLinkRevision;
        }

        private bool ConfirmationMatches(
            EyeMotionCharacterController controller)
        {
            return controller != null &&
                _confirmReplace &&
                controller.GetInstanceID() ==
                    _confirmControllerInstanceId &&
                controller.ExpressionLinkRevision ==
                    _confirmControllerRevision;
        }

        private string SaveProfile(
            EyeMotionCharacterController controller)
        {
            ExpressionLinkProfile profile = ExpressionLinkProfile.Create(
                _profileName,
                string.Empty,
                controller.CopyExpressionLinks());
            string savedName;
            string error;
            if (!_profileStore.TrySave(
                    _profileName,
                    profile,
                    out savedName,
                    out error))
            {
                return error.Length == 0
                    ? "The set could not be saved."
                    : error;
            }

            _profileName = savedName;
            RefreshProfileNames(false);
            return "Set saved: " + savedName + ".";
        }

        private bool TryLoadProfile(
            EyeMotionCharacterController controller,
            out string feedback)
        {
            ExpressionLinkProfile profile;
            string error;
            if (!_profileStore.TryLoad(
                    _profileName,
                    out profile,
                    out error))
            {
                feedback = error.Length == 0
                    ? "The set could not be loaded."
                    : error;
                return false;
            }

            ExpressionLinkDefinition[] links = profile.CloneForCharacter();
            string message;
            if (!controller.ReplaceExpressionLinks(
                    links,
                    false,
                    out message))
            {
                feedback = message.Length == 0
                    ? "The set could not be applied to the character."
                    : message;
                return false;
            }

            _profileName = profile.Name;
            feedback = "Set loaded: " + profile.Name + ".";
            return true;
        }

        private string RefreshProfileNames(bool reportResult)
        {
            try
            {
                _profileNames = _profileStore.ListNames();
                _profileNamesLoaded = true;
                _profileIndex = FindProfileIndex(_profileName);
                if (_profileIndex < 0 && _profileNames.Length > 0)
                {
                    _profileIndex = 0;
                }

                return reportResult
                    ? _profileNames.Length.ToString(InvariantCulture) +
                      " saved set(s) found."
                    : string.Empty;
            }
            catch (Exception exception)
            {
                _profileNames = new string[0];
                _profileIndex = -1;
                _profileNamesLoaded = true;
                return "Saved sets unavailable: " + exception.Message;
            }
        }

        private string CycleProfile(int direction)
        {
            if (_profileNames.Length == 0)
            {
                return "No saved sets were found.";
            }

            if (_profileIndex < 0)
            {
                _profileIndex = 0;
            }
            else
            {
                _profileIndex = WrapIndex(
                    _profileIndex + direction,
                    _profileNames.Length);
            }

            _profileName = _profileNames[_profileIndex];
            return "Selected set: " + _profileName + ".";
        }

        private int FindProfileIndex(string name)
        {
            for (int i = 0; i < _profileNames.Length; i++)
            {
                if (string.Equals(
                        _profileNames[i],
                        name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
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
