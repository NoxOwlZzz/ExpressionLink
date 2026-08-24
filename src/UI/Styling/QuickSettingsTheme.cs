using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class QuickSettingsTheme
    {
        internal static class Colors
        {
            internal static readonly Color Surface =
                new Color32(32, 40, 49, 255);
            internal static readonly Color ElevatedSurface =
                new Color32(37, 46, 56, 255);
            internal static readonly Color PropertyRow =
                new Color32(38, 46, 55, 255);
            internal static readonly Color Input =
                new Color32(21, 26, 32, 255);
            internal static readonly Color Button =
                new Color32(43, 54, 66, 255);
            internal static readonly Color ButtonHover =
                new Color32(54, 69, 83, 255);
            internal static readonly Color ButtonPressed =
                new Color32(43, 91, 105, 255);
            internal static readonly Color Selected =
                new Color32(49, 88, 102, 255);
            internal static readonly Color SelectedHover =
                new Color32(58, 105, 119, 255);
            internal static readonly Color Section =
                new Color32(52, 74, 86, 255);
            internal static readonly Color SectionHover =
                new Color32(61, 88, 101, 255);
            internal static readonly Color Accent =
                new Color32(86, 167, 181, 255);
            internal static readonly Color PrimaryText =
                new Color32(232, 237, 242, 255);
            internal static readonly Color SecondaryText =
                new Color32(167, 177, 188, 255);
            internal static readonly Color WarningSurface =
                new Color32(70, 56, 36, 255);
            internal static readonly Color WarningText =
                new Color32(232, 190, 111, 255);
        }

        internal static class Metrics
        {
            internal const int ContentPadding = 7;
            internal const float RowHeight = 24f;
            internal const float SectionHeight = 26f;
            internal const float TabHeight = 25f;
            internal const float ToolbarHeight = 26f;
            internal const float NarrowButtonWidth = 32f;
        }

        internal static class Glyphs
        {
            internal const string Collapsed = "\u25B8";
            internal const string Expanded = "\u25BE";
        }
    }
}
