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
        private static readonly GUILayoutOption[] ToolbarOptions =
        {
            GUILayout.MinHeight(QuickSettingsTheme.Metrics.ToolbarHeight)
        };
        private static readonly GUILayoutOption[] PropertyRowOptions =
        {
            GUILayout.MinHeight(QuickSettingsTheme.Metrics.RowHeight)
        };
        private static readonly GUILayoutOption[] ToolbarLabelOptions =
        {
            GUILayout.MinWidth(0f),
            GUILayout.ExpandWidth(true)
        };
        private static readonly QuickSettingsGuiResources Resources =
            new QuickSettingsGuiResources();

        internal static void BeginSurfaceArea(Rect bounds)
        {
            EnsureResources();
            GUILayout.BeginArea(bounds, Resources.Surface);
        }

        internal static void EndSurfaceArea()
        {
            GUILayout.EndArea();
        }

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

        internal static void BeginToolbar()
        {
            EnsureResources();
            GUILayout.BeginHorizontal(Resources.Toolbar, ToolbarOptions);
        }

        internal static void EndToolbar()
        {
            GUILayout.EndHorizontal();
        }

        internal static void BeginFooter()
        {
            EnsureResources();
            GUILayout.BeginVertical(Resources.Footer);
        }

        internal static void EndFooter()
        {
            GUILayout.EndVertical();
        }

        internal static void BeginPropertyRow()
        {
            EnsureResources();
            GUILayout.BeginHorizontal(
                Resources.PropertyRow,
                PropertyRowOptions);
        }

        internal static void EndPropertyRow()
        {
            GUILayout.EndHorizontal();
        }

        internal static void Space(float pixels)
        {
            ImGuiPrimitives.Space(pixels);
        }

        internal static void Label(string text)
        {
            EnsureResources();
            GUILayout.Label(text ?? string.Empty, Resources.Label);
        }

        internal static void Label(
            string text,
            GUILayoutOption[] options)
        {
            EnsureResources();
            GUILayout.Label(
                text ?? string.Empty,
                Resources.Label,
                options);
        }

        internal static void ToolbarLabel(string text)
        {
            EnsureResources();
            GUILayout.Label(
                text ?? string.Empty,
                Resources.ToolbarLabel,
                ToolbarLabelOptions);
        }

        internal static void Heading(string text)
        {
            EnsureResources();
            GUILayout.Label(text ?? string.Empty, Resources.Heading);
        }

        internal static void Help(string text)
        {
            EnsureResources();
            GUILayout.Label(text ?? string.Empty, Resources.Help);
        }

        internal static void Warning(string text)
        {
            EnsureResources();
            GUILayout.Label(text ?? string.Empty, Resources.Warning);
        }

        internal static void StatusBadge(string text)
        {
            EnsureResources();
            GUILayout.Label(text ?? string.Empty, Resources.StatusBadge);
        }

        internal static void VisibilityLabel(string text)
        {
            Label(text, VisibilityLabelOptions);
        }

        internal static void VisibilityStatus(string text)
        {
            Label(text, VisibilityStatusOptions);
        }

        internal static bool Button(string text)
        {
            EnsureResources();
            return GUILayout.Button(
                text ?? string.Empty,
                Resources.Button);
        }

        internal static bool Button(
            string text,
            GUILayoutOption[] options)
        {
            EnsureResources();
            return GUILayout.Button(
                text ?? string.Empty,
                Resources.Button,
                options);
        }

        internal static bool PrimaryButton(string text)
        {
            EnsureResources();
            return GUILayout.Button(
                text ?? string.Empty,
                Resources.PrimaryButton);
        }

        internal static bool TabButton(string text, bool selected)
        {
            EnsureResources();
            return GUILayout.Button(
                text ?? string.Empty,
                selected ? Resources.SelectedTab : Resources.Tab);
        }

        internal static bool SecondaryTabButton(
            string text,
            bool selected)
        {
            EnsureResources();
            return GUILayout.Button(
                text ?? string.Empty,
                selected
                    ? Resources.SelectedSecondaryTab
                    : Resources.SecondaryTab);
        }

        private static bool SegmentButton(
            string text,
            bool selected,
            GUILayoutOption[] options)
        {
            EnsureResources();
            return GUILayout.Button(
                text ?? string.Empty,
                selected
                    ? Resources.SelectedSegment
                    : Resources.SecondaryTab,
                options);
        }

        internal static bool OriginalSegmentButton(
            string text,
            bool selected)
        {
            return SegmentButton(
                text,
                selected,
                OriginalButtonOptions);
        }

        internal static bool VisibilitySegmentButton(
            string text,
            bool selected)
        {
            return SegmentButton(
                text,
                selected,
                VisibilityButtonOptions);
        }

        internal static bool Disclosure(bool expanded, string text)
        {
            EnsureResources();
            string prefix = expanded
                ? QuickSettingsTheme.Glyphs.Expanded
                : QuickSettingsTheme.Glyphs.Collapsed;
            return GUILayout.Button(
                    prefix + "  " + (text ?? string.Empty),
                    Resources.Disclosure)
                ? !expanded
                : expanded;
        }

        internal static bool Toggle(bool value, string text)
        {
            EnsureResources();
            BeginPropertyRow();
            bool result = GUILayout.Toggle(
                value,
                text ?? string.Empty,
                Resources.Toggle);
            EndPropertyRow();
            return result;
        }

        internal static string TextField(string value)
        {
            EnsureResources();
            return GUILayout.TextField(
                value ?? string.Empty,
                Resources.TextField);
        }

        internal static string TextField(
            string value,
            GUILayoutOption[] options)
        {
            EnsureResources();
            return GUILayout.TextField(
                value ?? string.Empty,
                Resources.TextField,
                options);
        }

        internal static string LabeledTextField(
            string label,
            string value)
        {
            BeginPropertyRow();
            Label(label, SliderLabelOptions);
            string result = TextField(value);
            EndPropertyRow();
            return result;
        }

        internal static float Slider(
            string label,
            float value,
            float minimum,
            float maximum,
            string format)
        {
            EnsureResources();
            BeginPropertyRow();
            Label(label, SliderLabelOptions);
            float result = GUILayout.HorizontalSlider(
                value,
                minimum,
                maximum,
                Resources.Slider,
                Resources.SliderThumb,
                SliderOptions);
            GUILayout.Label(
                result.ToString(format),
                Resources.ValueLabel,
                SliderValueOptions);
            EndPropertyRow();
            return result;
        }

        internal static void DisposeResources()
        {
            Resources.Dispose();
        }

        private static void EnsureResources()
        {
            Resources.Ensure(GUI.skin);
        }
    }
}
