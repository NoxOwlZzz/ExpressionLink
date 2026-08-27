using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkGUILayout
    {
        private static readonly GUILayoutOption[] FieldLabelOptions =
        {
            GUILayout.Width(160f)
        };
        private static readonly GUILayoutOption[] NarrowButtonOptions =
        {
            GUILayout.Width(30f)
        };
        private static readonly GUILayoutOption[] CountLabelOptions =
        {
            GUILayout.ExpandWidth(true)
        };
        private static readonly GUILayoutOption[] NumericFieldOptions =
        {
            GUILayout.Width(92f)
        };

        internal static void BeginHorizontal()
        {
            QuickSettingsGui.BeginHorizontal();
        }

        internal static void EndHorizontal()
        {
            QuickSettingsGui.EndHorizontal();
        }

        internal static void BeginFieldRow()
        {
            QuickSettingsGui.BeginPropertyRow();
        }

        internal static void EndFieldRow()
        {
            QuickSettingsGui.EndPropertyRow();
        }

        internal static void Label(string text)
        {
            QuickSettingsGui.Label(text);
        }

        internal static void Heading(string text)
        {
            QuickSettingsGui.Heading(text);
        }

        internal static void Help(string text)
        {
            QuickSettingsGui.Help(text);
        }

        internal static void Warning(string text)
        {
            QuickSettingsGui.Warning(text);
        }

        internal static void StatusBadge(string text)
        {
            QuickSettingsGui.StatusBadge(text);
        }

        internal static void FieldLabel(string text)
        {
            QuickSettingsGui.Label(text, FieldLabelOptions);
        }

        internal static void CountLabel(string text)
        {
            QuickSettingsGui.Label(text, CountLabelOptions);
        }

        internal static bool Button(string text)
        {
            return QuickSettingsGui.Button(text);
        }

        internal static bool PrimaryButton(string text)
        {
            return QuickSettingsGui.PrimaryButton(text);
        }

        internal static bool NarrowButton(string text)
        {
            return QuickSettingsGui.Button(text, NarrowButtonOptions);
        }

        internal static bool Toggle(bool value, string text)
        {
            return QuickSettingsGui.Toggle(value, text);
        }

        internal static bool ChoiceButton(string text, bool selected)
        {
            return QuickSettingsGui.SecondaryTabButton(text, selected);
        }

        internal static bool Disclosure(bool expanded, string text)
        {
            return QuickSettingsGui.Disclosure(expanded, text);
        }

        internal static string TextField(string label, string value)
        {
            BeginFieldRow();
            FieldLabel(label);
            string next = QuickSettingsGui.TextField(value);
            EndFieldRow();
            return next;
        }

        internal static string NumericTextField(
            string label,
            string value)
        {
            BeginFieldRow();
            FieldLabel(label);
            string next = QuickSettingsGui.TextField(
                value,
                NumericFieldOptions);
            EndFieldRow();
            return next;
        }
    }
}
