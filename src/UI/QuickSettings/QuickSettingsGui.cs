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
    }
}
