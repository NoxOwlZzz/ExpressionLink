using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class QuickSettingsTheme
    {
        internal static class Colors
        {
            internal static readonly Color Surface =
                new Color32(36, 43, 52, 255);
            internal static readonly Color ElevatedSurface =
                new Color32(42, 49, 59, 255);
            internal static readonly Color PropertyRow =
                new Color32(42, 49, 59, 255);
            internal static readonly Color Input =
                new Color32(22, 27, 34, 255);
            internal static readonly Color Button =
                new Color32(32, 38, 47, 255);
            internal static readonly Color ButtonHover =
                new Color32(51, 70, 92, 255);
            internal static readonly Color ButtonPressed =
                new Color32(57, 69, 83, 255);
            internal static readonly Color Selected =
                new Color32(52, 105, 153, 255);
            internal static readonly Color SelectedHover =
                new Color32(60, 110, 155, 255);
            internal static readonly Color SelectedPressed =
                new Color32(49, 95, 137, 255);
            internal static readonly Color Section =
                new Color32(111, 90, 142, 255);
            internal static readonly Color SectionHover =
                new Color32(118, 95, 151, 255);
            internal static readonly Color SectionPressed =
                new Color32(95, 76, 125, 255);
            internal static readonly Color Accent =
                new Color32(143, 199, 232, 255);
            internal static readonly Color OuterBorder =
                new Color32(16, 21, 27, 255);
            internal static readonly Color ControlBorder =
                new Color32(62, 74, 88, 255);
            internal static readonly Color PrimaryText =
                new Color32(230, 236, 242, 255);
            internal static readonly Color SecondaryText =
                new Color32(170, 182, 195, 255);
            internal static readonly Color WarningSurface =
                new Color32(64, 55, 42, 255);
            internal static readonly Color WarningText =
                new Color32(226, 194, 135, 255);
        }

        internal static class Metrics
        {
            internal const int ContentPadding = 7;
            internal const float RowHeight = 24f;
            internal const float SectionHeight = 26f;
            internal const float TabHeight = 25f;
            internal const float ToolbarHeight = 26f;
            internal const int RoundedTextureSize = 24;
            internal const float ControlRadius = 5f;
            internal const float PanelRadius = 7f;
            internal const float SliderRadius = 3f;
            internal const float OutlineWidth = 1f;
            internal const int ControlSlice = 6;
            internal const int PanelSlice = 8;
            internal const int SliderSlice = 3;
            internal const int ThumbSlice = 5;
        }

        internal static class Glyphs
        {
            internal const string Collapsed = "\u25B8";
            internal const string Expanded = "\u25BE";
        }
    }
}
