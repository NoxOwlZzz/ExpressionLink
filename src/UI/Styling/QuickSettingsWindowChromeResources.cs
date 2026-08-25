using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsWindowChromeResources : IDisposable
    {
        private Texture2D _headerTexture;

        internal QuickSettingsWindowChromeResources()
        {
            _headerTexture = QuickSettingsRoundedTextureFactory.Create(
                "ExpressionLink.Header",
                QuickSettingsTheme.Colors.ElevatedSurface,
                QuickSettingsTheme.Colors.OuterBorder,
                QuickSettingsTheme.Metrics.PanelRadius,
                QuickSettingsCornerMask.Top);
            HeaderSprite = Sprite.Create(
                _headerTexture,
                new Rect(
                    0f,
                    0f,
                    _headerTexture.width,
                    _headerTexture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(
                    QuickSettingsTheme.Metrics.PanelSlice,
                    QuickSettingsTheme.Metrics.PanelSlice,
                    QuickSettingsTheme.Metrics.PanelSlice,
                    QuickSettingsTheme.Metrics.PanelSlice));
            HeaderSprite.name = "ExpressionLink.HeaderSprite";
            HeaderSprite.hideFlags = HideFlags.HideAndDontSave;
        }

        internal Sprite HeaderSprite { get; private set; }

        public void Dispose()
        {
            if (HeaderSprite != null)
            {
                UnityEngine.Object.Destroy(HeaderSprite);
                HeaderSprite = null;
            }

            if (_headerTexture != null)
            {
                UnityEngine.Object.Destroy(_headerTexture);
                _headerTexture = null;
            }
        }
    }
}
