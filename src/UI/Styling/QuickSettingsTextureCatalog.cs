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
            Surface = Create(
                "ExpressionLink.Surface",
                QuickSettingsTheme.Colors.Surface);
            ElevatedSurface = Create(
                "ExpressionLink.ElevatedSurface",
                QuickSettingsTheme.Colors.ElevatedSurface);
            PropertyRow = Create(
                "ExpressionLink.PropertyRow",
                QuickSettingsTheme.Colors.PropertyRow);
            Input = Create(
                "ExpressionLink.Input",
                QuickSettingsTheme.Colors.Input);
            Button = Create(
                "ExpressionLink.Button",
                QuickSettingsTheme.Colors.Button);
            ButtonHover = Create(
                "ExpressionLink.ButtonHover",
                QuickSettingsTheme.Colors.ButtonHover);
            ButtonPressed = Create(
                "ExpressionLink.ButtonPressed",
                QuickSettingsTheme.Colors.ButtonPressed);
            Selected = Create(
                "ExpressionLink.Selected",
                QuickSettingsTheme.Colors.Selected);
            SelectedHover = Create(
                "ExpressionLink.SelectedHover",
                QuickSettingsTheme.Colors.SelectedHover);
            Section = Create(
                "ExpressionLink.Section",
                QuickSettingsTheme.Colors.Section);
            SectionHover = Create(
                "ExpressionLink.SectionHover",
                QuickSettingsTheme.Colors.SectionHover);
            Accent = Create(
                "ExpressionLink.Accent",
                QuickSettingsTheme.Colors.Accent);
            WarningSurface = Create(
                "ExpressionLink.WarningSurface",
                QuickSettingsTheme.Colors.WarningSurface);
        }

        internal Texture2D Surface { get; private set; }
        internal Texture2D ElevatedSurface { get; private set; }
        internal Texture2D PropertyRow { get; private set; }
        internal Texture2D Input { get; private set; }
        internal Texture2D Button { get; private set; }
        internal Texture2D ButtonHover { get; private set; }
        internal Texture2D ButtonPressed { get; private set; }
        internal Texture2D Selected { get; private set; }
        internal Texture2D SelectedHover { get; private set; }
        internal Texture2D Section { get; private set; }
        internal Texture2D SectionHover { get; private set; }
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

        private Texture2D Create(string name, Color color)
        {
            Texture2D texture = new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false);
            texture.name = name;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixel(0, 0, color);
            texture.Apply(false, true);
            _ownedTextures.Add(texture);
            return texture;
        }
    }
}
