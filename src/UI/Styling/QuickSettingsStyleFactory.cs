using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class QuickSettingsStyleFactory
    {
        internal static GUIStyle CreatePanel(
            GUIStyle source,
            Texture2D background,
            int left,
            int right,
            int top,
            int bottom,
            int borderSlice)
        {
            GUIStyle style = new GUIStyle(source);
            style.normal.background = background;
            style.hover.background = background;
            style.active.background = background;
            style.focused.background = background;
            style.border = CreateBorder(borderSlice);
            style.padding = new RectOffset(left, right, top, bottom);
            style.margin = new RectOffset(0, 0, 0, 0);
            return style;
        }

        internal static GUIStyle CreateButton(
            GUIStyle source,
            Texture2D normal,
            Texture2D hover,
            Texture2D pressed,
            Color textColor)
        {
            GUIStyle style = new GUIStyle(source);
            ConfigureState(style.normal, normal, textColor);
            ConfigureState(style.hover, hover, textColor);
            ConfigureState(style.active, pressed, textColor);
            ConfigureState(style.focused, hover, textColor);
            ConfigureState(style.onNormal, normal, textColor);
            ConfigureState(style.onHover, hover, textColor);
            ConfigureState(style.onActive, pressed, textColor);
            ConfigureState(style.onFocused, hover, textColor);
            style.alignment = TextAnchor.MiddleCenter;
            style.fixedHeight = QuickSettingsTheme.Metrics.RowHeight;
            style.padding = new RectOffset(7, 7, 2, 2);
            style.margin = new RectOffset(1, 1, 1, 1);
            style.border = CreateBorder(
                QuickSettingsTheme.Metrics.ControlSlice);
            return style;
        }

        internal static void ConfigureInput(
            GUIStyle style,
            Texture2D normal,
            Texture2D focused)
        {
            ConfigureState(
                style.normal,
                normal,
                QuickSettingsTheme.Colors.PrimaryText);
            ConfigureState(
                style.hover,
                normal,
                QuickSettingsTheme.Colors.PrimaryText);
            ConfigureState(
                style.active,
                focused,
                QuickSettingsTheme.Colors.PrimaryText);
            ConfigureState(
                style.focused,
                focused,
                QuickSettingsTheme.Colors.PrimaryText);
            style.fixedHeight = 21f;
            style.padding = new RectOffset(5, 5, 2, 2);
            style.margin = new RectOffset(2, 2, 1, 1);
            style.border = CreateBorder(
                QuickSettingsTheme.Metrics.ControlSlice);
        }

        internal static void ConfigureSliderTrack(
            GUIStyle style,
            Texture2D background)
        {
            Color textColor = QuickSettingsTheme.Colors.PrimaryText;
            ConfigureState(style.normal, background, textColor);
            ConfigureState(style.hover, background, textColor);
            ConfigureState(style.active, background, textColor);
            ConfigureState(style.focused, background, textColor);
            ConfigureState(style.onNormal, background, textColor);
            ConfigureState(style.onHover, background, textColor);
            ConfigureState(style.onActive, background, textColor);
            ConfigureState(style.onFocused, background, textColor);
            style.border = CreateBorder(
                QuickSettingsTheme.Metrics.SliderSlice);
        }

        internal static void ConfigureSliderThumb(
            GUIStyle style,
            Texture2D normal,
            Texture2D hover,
            Texture2D pressed)
        {
            ConfigureState(
                style.normal,
                normal,
                QuickSettingsTheme.Colors.PrimaryText);
            ConfigureState(
                style.hover,
                hover,
                QuickSettingsTheme.Colors.PrimaryText);
            ConfigureState(
                style.active,
                pressed,
                QuickSettingsTheme.Colors.PrimaryText);
            ConfigureState(
                style.focused,
                hover,
                QuickSettingsTheme.Colors.PrimaryText);
            style.fixedWidth = 14f;
            style.fixedHeight = 18f;
            style.border = CreateBorder(
                QuickSettingsTheme.Metrics.ThumbSlice);
        }

        internal static void ConfigureText(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            style.onNormal.textColor = color;
            style.onHover.textColor = color;
            style.onActive.textColor = color;
            style.onFocused.textColor = color;
        }

        internal static RectOffset CreateBorder(int slice)
        {
            return new RectOffset(slice, slice, slice, slice);
        }

        private static void ConfigureState(
            GUIStyleState state,
            Texture2D background,
            Color textColor)
        {
            state.background = background;
            state.textColor = textColor;
        }
    }
}
