using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkGUILayout
    {
        private static readonly GUILayoutOption[] NoLayoutOptions =
            new GUILayoutOption[0];
        private static readonly GUILayoutOption[] FieldLabelOptions =
        {
            GUILayout.Width(142f)
        };
        private static readonly GUILayoutOption[] NarrowButtonOptions =
        {
            GUILayout.Width(30f)
        };
        private static readonly GUILayoutOption[] CountLabelOptions =
        {
            GUILayout.Width(54f)
        };
        private static readonly GUILayoutOption[] NumericFieldOptions =
        {
            GUILayout.Width(92f)
        };

        internal static void BeginHorizontal()
        {
            GUILayout.BeginHorizontal(NoLayoutOptions);
        }

        internal static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        internal static void Label(string text)
        {
            GUILayout.Label(text ?? string.Empty, NoLayoutOptions);
        }

        internal static void FieldLabel(string text)
        {
            GUILayout.Label(text ?? string.Empty, FieldLabelOptions);
        }

        internal static void CountLabel(string text)
        {
            GUILayout.Label(text ?? string.Empty, CountLabelOptions);
        }

        internal static bool Button(string text)
        {
            return GUILayout.Button(text ?? string.Empty, NoLayoutOptions);
        }

        internal static bool NarrowButton(string text)
        {
            return GUILayout.Button(text ?? string.Empty, NarrowButtonOptions);
        }

        internal static bool Toggle(bool value, string text)
        {
            return GUILayout.Toggle(
                value,
                text ?? string.Empty,
                NoLayoutOptions);
        }

        internal static string TextField(string label, string value)
        {
            BeginHorizontal();
            FieldLabel(label);
            string next = GUILayout.TextField(
                value ?? string.Empty,
                NoLayoutOptions);
            EndHorizontal();
            return next;
        }

        internal static string NumericTextField(
            string label,
            string value)
        {
            BeginHorizontal();
            FieldLabel(label);
            string next = GUILayout.TextField(
                value ?? string.Empty,
                NumericFieldOptions);
            EndHorizontal();
            return next;
        }
    }
}
