using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class QuickSettingsTheme
    {
        internal static class Colors
        {
            internal static readonly Color Surface =
                new Color32(30, 36, 44, 255);
            internal static readonly Color ElevatedSurface =
                new Color32(37, 45, 55, 255);
            internal static readonly Color PropertyRow =
                new Color32(41, 50, 61, 255);
            internal static readonly Color Input =
                new Color32(21, 27, 34, 255);
            internal static readonly Color Button =
                new Color32(39, 50, 61, 255);
            internal static readonly Color ButtonHover =
                new Color32(52, 74, 92, 255);
            internal static readonly Color ButtonPressed =
                new Color32(41, 72, 84, 255);
            internal static readonly Color Selected =
                new Color32(53, 110, 137, 255);
            internal static readonly Color SelectedHover =
                new Color32(66, 126, 152, 255);
            internal static readonly Color Section =
                new Color32(81, 84, 109, 255);
            internal static readonly Color SectionHover =
                new Color32(96, 100, 126, 255);
            internal static readonly Color Accent =
                new Color32(98, 176, 193, 255);
            internal static readonly Color OuterBorder =
                new Color32(14, 20, 26, 255);
            internal static readonly Color ControlBorder =
                new Color32(61, 74, 87, 255);
            internal static readonly Color PrimaryText =
                new Color32(230, 236, 242, 255);
            internal static readonly Color SecondaryText =
                new Color32(170, 182, 195, 255);
            internal static readonly Color WarningSurface =
                new Color32(73, 61, 42, 255);
            internal static readonly Color WarningText =
                new Color32(215, 164, 74, 255);
        }

        internal static class Metrics
        {
            internal const int ContentPadding = 7;
            internal const float RowHeight = 24f;
            internal const float SectionHeight = 26f;
            internal const float TabHeight = 25f;
            internal const float ToolbarHeight = 26f;
            internal const float NarrowButtonWidth = 32f;
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
