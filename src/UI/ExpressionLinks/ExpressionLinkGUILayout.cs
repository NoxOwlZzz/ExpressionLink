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
            ImGuiPrimitives.BeginHorizontal();
        }

        internal static void EndHorizontal()
        {
            ImGuiPrimitives.EndHorizontal();
        }

        internal static void Label(string text)
        {
            ImGuiPrimitives.Label(text);
        }

        internal static void FieldLabel(string text)
        {
            ImGuiPrimitives.Label(text, FieldLabelOptions);
        }

        internal static void CountLabel(string text)
        {
            ImGuiPrimitives.Label(text, CountLabelOptions);
        }

        internal static bool Button(string text)
        {
            return ImGuiPrimitives.Button(text);
        }

        internal static bool NarrowButton(string text)
        {
            return ImGuiPrimitives.Button(text, NarrowButtonOptions);
        }

        internal static bool Toggle(bool value, string text)
        {
            return ImGuiPrimitives.Toggle(value, text);
        }

        internal static string TextField(string label, string value)
        {
            BeginHorizontal();
            FieldLabel(label);
            string next = ImGuiPrimitives.TextField(value);
            EndHorizontal();
            return next;
        }

        internal static string NumericTextField(
            string label,
            string value)
        {
            BeginHorizontal();
            FieldLabel(label);
            string next = ImGuiPrimitives.TextField(
                value,
                NumericFieldOptions);
            EndHorizontal();
            return next;
        }
    }
}
