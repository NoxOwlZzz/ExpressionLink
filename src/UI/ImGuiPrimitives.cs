using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ImGuiPrimitives
    {
        private static readonly GUILayoutOption[] NoLayoutOptions =
            new GUILayoutOption[0];

        internal static void BeginHorizontal()
        {
            GUILayout.BeginHorizontal(NoLayoutOptions);
        }

        internal static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        internal static void BeginVertical()
        {
            GUILayout.BeginVertical(NoLayoutOptions);
        }

        internal static void EndVertical()
        {
            GUILayout.EndVertical();
        }

        internal static void Space(float pixels)
        {
            GUILayout.Space(pixels);
        }

        internal static void Label(string text)
        {
            Label(text, NoLayoutOptions);
        }

        internal static void Label(
            string text,
            GUILayoutOption[] options)
        {
            GUILayout.Label(text ?? string.Empty, options);
        }

        internal static bool Button(string text)
        {
            return Button(text, NoLayoutOptions);
        }

        internal static bool Button(
            string text,
            GUILayoutOption[] options)
        {
            return GUILayout.Button(text ?? string.Empty, options);
        }

        internal static bool Toggle(bool value, string text)
        {
            return GUILayout.Toggle(
                value,
                text ?? string.Empty,
                NoLayoutOptions);
        }

        internal static string TextField(string value)
        {
            return TextField(value, NoLayoutOptions);
        }

        internal static string TextField(
            string value,
            GUILayoutOption[] options)
        {
            return GUILayout.TextField(value ?? string.Empty, options);
        }
    }
}
