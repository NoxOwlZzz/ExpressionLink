using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsTextureCatalog : IDisposable
    {
        private readonly List<Texture2D> _ownedTextures =
            new List<Texture2D>();

        internal QuickSettingsTextureCatalog()
        {
            Surface = CreateRounded(
                "ExpressionLink.Surface",
                QuickSettingsTheme.Colors.Surface,
                QuickSettingsTheme.Colors.OuterBorder,
                QuickSettingsTheme.Metrics.PanelRadius,
                QuickSettingsCornerMask.Bottom);
            ElevatedSurface = CreateRounded(
                "ExpressionLink.ElevatedSurface",
                QuickSettingsTheme.Colors.ElevatedSurface,
                QuickSettingsTheme.Colors.OuterBorder,
                QuickSettingsTheme.Metrics.ControlRadius);
            PropertyRow = CreateRounded(
                "ExpressionLink.PropertyRow",
                QuickSettingsTheme.Colors.PropertyRow,
                QuickSettingsTheme.Colors.ControlBorder,
                QuickSettingsTheme.Metrics.ControlRadius);
            Input = CreateRounded(
                "ExpressionLink.Input",
                QuickSettingsTheme.Colors.Input,
                QuickSettingsTheme.Colors.ControlBorder,
                QuickSettingsTheme.Metrics.ControlRadius);
            InputFocused = CreateRounded(
                "ExpressionLink.InputFocused",
                QuickSettingsTheme.Colors.Input,
                QuickSettingsTheme.Colors.Accent,
                QuickSettingsTheme.Metrics.ControlRadius);
            SliderTrack = CreateRounded(
                "ExpressionLink.SliderTrack",
                QuickSettingsTheme.Colors.Input,
                QuickSettingsTheme.Colors.ControlBorder,
                QuickSettingsTheme.Metrics.SliderRadius);
            Button = CreateRounded(
                "ExpressionLink.Button",
                QuickSettingsTheme.Colors.Button,
                QuickSettingsTheme.Colors.ControlBorder,
                QuickSettingsTheme.Metrics.ControlRadius);
            ButtonHover = CreateRounded(
                "ExpressionLink.ButtonHover",
                QuickSettingsTheme.Colors.ButtonHover,
                QuickSettingsTheme.Colors.ControlBorder,
                QuickSettingsTheme.Metrics.ControlRadius);
            ButtonPressed = CreateRounded(
                "ExpressionLink.ButtonPressed",
                QuickSettingsTheme.Colors.ButtonPressed,
                QuickSettingsTheme.Colors.Accent,
                QuickSettingsTheme.Metrics.ControlRadius);
            Selected = CreateRounded(
                "ExpressionLink.Selected",
                QuickSettingsTheme.Colors.Selected,
                QuickSettingsTheme.Colors.Accent,
                QuickSettingsTheme.Metrics.ControlRadius);
            SelectedHover = CreateRounded(
                "ExpressionLink.SelectedHover",
                QuickSettingsTheme.Colors.SelectedHover,
                QuickSettingsTheme.Colors.Accent,
                QuickSettingsTheme.Metrics.ControlRadius);
            SelectedPressed = CreateRounded(
                "ExpressionLink.SelectedPressed",
                QuickSettingsTheme.Colors.SelectedPressed,
                QuickSettingsTheme.Colors.Accent,
                QuickSettingsTheme.Metrics.ControlRadius);
            Section = CreateRounded(
                "ExpressionLink.Section",
                QuickSettingsTheme.Colors.Section,
                QuickSettingsTheme.Colors.ControlBorder,
                QuickSettingsTheme.Metrics.ControlRadius);
            SectionHover = CreateRounded(
                "ExpressionLink.SectionHover",
                QuickSettingsTheme.Colors.SectionHover,
                QuickSettingsTheme.Colors.ControlBorder,
                QuickSettingsTheme.Metrics.ControlRadius);
            SectionPressed = CreateRounded(
                "ExpressionLink.SectionPressed",
                QuickSettingsTheme.Colors.SectionPressed,
                QuickSettingsTheme.Colors.ControlBorder,
                QuickSettingsTheme.Metrics.ControlRadius);
            Accent = CreateRounded(
                "ExpressionLink.Accent",
                QuickSettingsTheme.Colors.Accent,
                QuickSettingsTheme.Colors.Accent,
                QuickSettingsTheme.Metrics.ControlRadius);
            WarningSurface = CreateRounded(
                "ExpressionLink.WarningSurface",
                QuickSettingsTheme.Colors.WarningSurface,
                QuickSettingsTheme.Colors.WarningText,
                QuickSettingsTheme.Metrics.ControlRadius);
        }

        internal Texture2D Surface { get; private set; }
        internal Texture2D ElevatedSurface { get; private set; }
        internal Texture2D PropertyRow { get; private set; }
        internal Texture2D Input { get; private set; }
        internal Texture2D InputFocused { get; private set; }
        internal Texture2D SliderTrack { get; private set; }
        internal Texture2D Button { get; private set; }
        internal Texture2D ButtonHover { get; private set; }
        internal Texture2D ButtonPressed { get; private set; }
        internal Texture2D Selected { get; private set; }
        internal Texture2D SelectedHover { get; private set; }
        internal Texture2D SelectedPressed { get; private set; }
        internal Texture2D Section { get; private set; }
        internal Texture2D SectionHover { get; private set; }
        internal Texture2D SectionPressed { get; private set; }
        internal Texture2D Accent { get; private set; }
        internal Texture2D WarningSurface { get; private set; }

        public void Dispose()
        {
            for (int i = 0; i < _ownedTextures.Count; i++)
            {
                Texture2D texture = _ownedTextures[i];
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }

            _ownedTextures.Clear();
        }

        private Texture2D CreateRounded(
            string name,
            Color fill,
            Color outline,
            float radius)
        {
            return CreateRounded(
                name,
                fill,
                outline,
                radius,
                QuickSettingsCornerMask.All);
        }

        private Texture2D CreateRounded(
            string name,
            Color fill,
            Color outline,
            float radius,
            QuickSettingsCornerMask corners)
        {
            Texture2D texture =
                QuickSettingsRoundedTextureFactory.Create(
                    name,
                    fill,
                    outline,
                    radius,
                    corners);
            _ownedTextures.Add(texture);
            return texture;
        }
    }
}
