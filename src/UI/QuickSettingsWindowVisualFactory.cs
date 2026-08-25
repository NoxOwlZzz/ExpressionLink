using System;
using UnityEngine;
using UnityEngine.UI;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsWindowVisual : IDisposable
    {
        private QuickSettingsWindowChromeResources _chromeResources;

        internal QuickSettingsWindowVisual(
            GameObject root,
            CanvasScaler scaler,
            RectTransform mainPanel,
            RectTransform headerPanel,
            QuickSettingsMovableWindow movableWindow,
            QuickSettingsWindowChromeResources chromeResources)
        {
            Root = root;
            Scaler = scaler;
            MainPanel = mainPanel;
            HeaderPanel = headerPanel;
            MovableWindow = movableWindow;
            _chromeResources = chromeResources;
        }

        internal GameObject Root { get; private set; }
        internal CanvasScaler Scaler { get; private set; }
        internal RectTransform MainPanel { get; private set; }
        internal RectTransform HeaderPanel { get; private set; }
        internal QuickSettingsMovableWindow MovableWindow
        {
            get;
            private set;
        }

        public void Dispose()
        {
            if (Root != null)
            {
                UnityEngine.Object.Destroy(Root);
                Root = null;
            }

            if (_chromeResources != null)
            {
                _chromeResources.Dispose();
                _chromeResources = null;
            }

            Scaler = null;
            MainPanel = null;
            HeaderPanel = null;
            MovableWindow = null;
        }
    }

    internal static class QuickSettingsWindowVisualFactory
    {
        private const int WindowSortingOrder = 1000;

        // The Canvas panel owns drag and raycasts. IMGUI draws the opaque body.
        private static readonly Color MainPanelColor =
            new Color32(29, 34, 41, 1);

        internal static QuickSettingsWindowVisual Create(
            Transform owner,
            string title,
            float headerHeight,
            float viewportWidth,
            float viewportHeight)
        {
            QuickSettingsWindowChromeResources chromeResources =
                new QuickSettingsWindowChromeResources();
            GameObject root = new GameObject(
                "KK_ExpressionLink_QuickSettingsCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            if (owner != null)
            {
                root.transform.SetParent(owner);
            }

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = WindowSortingOrder;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.Expand;
            scaler.referencePixelsPerUnit = 100f;
            scaler.referenceResolution = new Vector2(
                Math.Max(1f, viewportWidth),
                Math.Max(1f, viewportHeight));

            GraphicRaycaster raycaster =
                root.GetComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;
            raycaster.blockingObjects =
                GraphicRaycaster.BlockingObjects.None;

            RectTransform mainPanel = CreateImage(
                "QuickSettingsMainPanel",
                root.transform,
                MainPanelColor,
                null);
            mainPanel.anchorMin = new Vector2(0f, 1f);
            mainPanel.anchorMax = new Vector2(0f, 1f);
            mainPanel.pivot = new Vector2(0f, 1f);

            RectTransform headerPanel = CreateImage(
                "QuickSettingsHeaderPanel",
                mainPanel,
                Color.white,
                chromeResources.HeaderSprite);
            headerPanel.anchorMin = new Vector2(0f, 1f);
            headerPanel.anchorMax = new Vector2(1f, 1f);
            headerPanel.pivot = new Vector2(0.5f, 1f);
            headerPanel.offsetMin = new Vector2(
                0f,
                -Math.Max(1f, headerHeight));
            headerPanel.offsetMax = Vector2.zero;

            CreateHeaderTitle(
                headerPanel,
                title ?? string.Empty);
            CreateHeaderAccent(headerPanel);
            QuickSettingsMovableWindow movableWindow =
                headerPanel.gameObject
                    .AddComponent<QuickSettingsMovableWindow>();
            movableWindow.ToDrag = mainPanel;

            return new QuickSettingsWindowVisual(
                root,
                scaler,
                mainPanel,
                headerPanel,
                movableWindow,
                chromeResources);
        }

        private static RectTransform CreateImage(
            string name,
            Transform parent,
            Color color,
            Sprite sprite)
        {
            GameObject gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform rectTransform =
                gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.localScale = Vector3.one;

            Image image = gameObject.GetComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            if (sprite != null)
            {
                image.type = Image.Type.Sliced;
                image.fillCenter = true;
            }

            image.raycastTarget = true;
            return rectTransform;
        }

        private static void CreateHeaderAccent(Transform parent)
        {
            RectTransform accent = CreateImage(
                "QuickSettingsHeaderAccent",
                parent,
                QuickSettingsTheme.Colors.Accent,
                null);
            accent.anchorMin = new Vector2(0f, 0f);
            accent.anchorMax = new Vector2(1f, 0f);
            accent.pivot = new Vector2(0.5f, 0f);
            accent.offsetMin = new Vector2(6f, 0f);
            accent.offsetMax = new Vector2(-6f, 2f);
            Image image = accent.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }
        }

        private static void CreateHeaderTitle(
            Transform parent,
            string title)
        {
            GameObject gameObject = new GameObject(
                "QuickSettingsHeaderTitle",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            RectTransform rectTransform =
                gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(8f, 0f);
            rectTransform.offsetMax = new Vector2(-8f, 0f);
            rectTransform.localScale = Vector3.one;

            Text text = gameObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>(
                "Arial.ttf");
            text.fontSize = 14;
            text.color = QuickSettingsTheme.Colors.PrimaryText;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow =
                HorizontalWrapMode.Overflow;
            text.verticalOverflow =
                VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = title;
        }
    }
}
