using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class QuickSettingsGui
    {
        private static readonly GUILayoutOption[] SliderLabelOptions =
        {
            GUILayout.MinWidth(116f),
            GUILayout.MaxWidth(190f)
        };
        private static readonly GUILayoutOption[] SliderOptions =
        {
            GUILayout.MinWidth(64f),
            GUILayout.ExpandWidth(true)
        };
        private static readonly GUILayoutOption[] SliderValueOptions =
        {
            GUILayout.Width(48f)
        };
        private static readonly GUILayoutOption[] VisibilityLabelOptions =
        {
            GUILayout.MinWidth(112f),
            GUILayout.MaxWidth(158f)
        };
        private static readonly GUILayoutOption[] VisibilityStatusOptions =
        {
            GUILayout.MinWidth(70f)
        };
        private static readonly GUILayoutOption[] OriginalButtonOptions =
        {
            GUILayout.MinWidth(58f)
        };
        private static readonly GUILayoutOption[] VisibilityButtonOptions =
        {
            GUILayout.MinWidth(50f)
        };
        private static GUISkin _cachedSkin;
        private static GUIStyle _headingStyle;
        private static GUIStyle _helpStyle;

        internal static void BeginHorizontal()
        {
            ImGuiPrimitives.BeginHorizontal();
        }

        internal static void EndHorizontal()
        {
            ImGuiPrimitives.EndHorizontal();
        }

        internal static void BeginVertical()
        {
            ImGuiPrimitives.BeginVertical();
        }

        internal static void EndVertical()
        {
            ImGuiPrimitives.EndVertical();
        }

        internal static void Space(float pixels)
        {
            ImGuiPrimitives.Space(pixels);
        }

        internal static void Label(string text)
        {
            ImGuiPrimitives.Label(text);
        }

        internal static void Heading(string text)
        {
            EnsureTextStyles();
            GUILayout.Label(
                text ?? string.Empty,
                _headingStyle);
        }

        internal static void Help(string text)
        {
            EnsureTextStyles();
            GUILayout.Label(
                text ?? string.Empty,
                _helpStyle);
        }

        internal static void VisibilityLabel(string text)
        {
            ImGuiPrimitives.Label(text, VisibilityLabelOptions);
        }

        internal static void VisibilityStatus(string text)
        {
            ImGuiPrimitives.Label(text, VisibilityStatusOptions);
        }

        internal static bool Button(string text)
        {
            return ImGuiPrimitives.Button(text);
        }

        internal static bool Disclosure(bool expanded, string text)
        {
            string prefix = expanded ? "[-] " : "[+] ";
            return Button(prefix + (text ?? string.Empty))
                ? !expanded
                : expanded;
        }

        internal static bool OriginalButton(string text)
        {
            return ImGuiPrimitives.Button(text, OriginalButtonOptions);
        }

        internal static bool VisibilityButton(string text)
        {
            return ImGuiPrimitives.Button(text, VisibilityButtonOptions);
        }

        internal static bool Toggle(bool value, string text)
        {
            return ImGuiPrimitives.Toggle(value, text);
        }

        internal static string TextField(string value)
        {
            return ImGuiPrimitives.TextField(value);
        }

        internal static string LabeledTextField(
            string label,
            string value)
        {
            BeginHorizontal();
            ImGuiPrimitives.Label(label, SliderLabelOptions);
            string result = TextField(value);
            EndHorizontal();
            return result;
        }

        internal static float Slider(
            string label,
            float value,
            float minimum,
            float maximum,
            string format)
        {
            BeginHorizontal();
            ImGuiPrimitives.Label(label, SliderLabelOptions);
            float result = GUILayout.HorizontalSlider(
                value,
                minimum,
                maximum,
                SliderOptions);
            ImGuiPrimitives.Label(
                result.ToString(format),
                SliderValueOptions);
            EndHorizontal();
            return result;
        }

        private static void EnsureTextStyles()
        {
            GUISkin skin = GUI.skin;
            if (_cachedSkin == skin &&
                _headingStyle != null &&
                _helpStyle != null)
            {
                return;
            }

            _cachedSkin = skin;
            _headingStyle = new GUIStyle(skin.label);
            _headingStyle.fontStyle = FontStyle.Bold;
            _headingStyle.wordWrap = true;
            _helpStyle = new GUIStyle(skin.label);
            _helpStyle.fontStyle = FontStyle.Italic;
            _helpStyle.wordWrap = true;
        }
    }
}
