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
            string label = _showProfiles
                ? "[-] Reusable profiles"
                : "[+] Reusable profiles";
            if (ExpressionLinkGUILayout.Button(label))
            {
                _showProfiles = !_showProfiles;
                if (!_showProfiles)
                {
                    _confirmReplace = false;
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

            ExpressionLinkGUILayout.Label(
                "Profiles contain the complete Custom links list.");
            string editedProfileName = ExpressionLinkGUILayout.TextField(
                "Profile name",
                _profileName);
            if (!string.Equals(
                    editedProfileName,
                    _profileName,
                    StringComparison.Ordinal))
            {
                _profileName = editedProfileName;
                _confirmReplace = false;
            }

            ExpressionLinkGUILayout.BeginHorizontal();
            if (ExpressionLinkGUILayout.Button("Save / overwrite profile"))
            {
                feedback = hasUnappliedDraft
                    ? "Save or discard changes first."
                    : SaveProfile(controller);
                _confirmReplace = false;
            }

            if (ExpressionLinkGUILayout.Button("Replace all links"))
            {
                if (hasUnappliedDraft)
                {
                    feedback =
                        "Save or discard changes first.";
                }
                else
                {
                    _confirmReplace = true;
                    feedback =
                        "Select Confirm replace to replace every link.";
                }
            }

            ExpressionLinkGUILayout.EndHorizontal();

            if (_confirmReplace)
            {
                ExpressionLinkGUILayout.BeginHorizontal();
                if (ExpressionLinkGUILayout.Button("Confirm replace"))
                {
                    linksReplaced = TryLoadProfile(
                        controller,
                        out feedback);
                    _confirmReplace = false;
                }

                if (ExpressionLinkGUILayout.Button("Cancel"))
                {
                    _confirmReplace = false;
                    feedback = "Replace cancelled.";
                }

                ExpressionLinkGUILayout.EndHorizontal();
            }

            ExpressionLinkGUILayout.BeginHorizontal();

            if (ExpressionLinkGUILayout.NarrowButton("<"))
            {
                _confirmReplace = false;
                feedback = CycleProfile(-1);
            }

            if (ExpressionLinkGUILayout.NarrowButton(">"))
            {
                _confirmReplace = false;
                feedback = CycleProfile(1);
            }

            if (ExpressionLinkGUILayout.Button("Refresh profiles"))
            {
                _confirmReplace = false;
                feedback = RefreshProfileNames(true);
            }

            ExpressionLinkGUILayout.EndHorizontal();

            if (_profileNames.Length > 0)
            {
                ExpressionLinkGUILayout.Label(
                    "Saved profiles: " +
                    _profileNames.Length.ToString(InvariantCulture));
            }

            return linksReplaced;
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
                    ? "The profile could not be saved."
                    : error;
            }

            _profileName = savedName;
            RefreshProfileNames(false);
            return "Profile saved: " + savedName + ".";
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
                    ? "The profile could not be loaded."
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
                    ? "The profile could not be applied to the character."
                    : message;
                return false;
            }

            _profileName = profile.Name;
            feedback = "Profile loaded: " + profile.Name + ".";
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
                      " profile(s) found."
                    : string.Empty;
            }
            catch (Exception exception)
            {
                _profileNames = new string[0];
                _profileIndex = -1;
                _profileNamesLoaded = true;
                return "Profiles unavailable: " + exception.Message;
            }
        }

        private string CycleProfile(int direction)
        {
            if (_profileNames.Length == 0)
            {
                return "No saved profiles were found.";
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
            return "Selected profile: " + _profileName + ".";
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
