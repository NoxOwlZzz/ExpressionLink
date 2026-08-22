using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsDragOverlay : IDisposable
    {
        private const int OverlaySortingOrder = 32000;

        private readonly QuickSettingsWindowHost _host;
        private GameObject _root;
        private RectTransform _headerTransform;
        private QuickSettingsHeaderDragSurface _dragSurface;
        private bool _disposed;
        private bool _missingEventSystemWarningLogged;

        internal QuickSettingsDragOverlay(
            Transform parent,
            QuickSettingsWindowHost host,
            float headerHeight)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }

            _host = host;
            CreateHierarchy(parent, headerHeight);
            _host.BoundsChanged += HandleBoundsChanged;
            _host.VisibilityChanged += HandleVisibilityChanged;
            ApplyBounds(_host.Bounds);
            HandleVisibilityChanged(_host.Visible);
        }

        internal void RefreshViewport()
        {
            if (_disposed)
            {
                return;
            }

            _host.UpdateViewport(Screen.width, Screen.height);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _host.BoundsChanged -= HandleBoundsChanged;
            _host.VisibilityChanged -= HandleVisibilityChanged;
            if (_dragSurface != null)
            {
                _dragSurface.Detach();
            }

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
            }

            _dragSurface = null;
            _headerTransform = null;
            _root = null;
        }

        private void CreateHierarchy(Transform parent, float headerHeight)
        {
            _root = new GameObject(
                "KK_ExpressionLink_QuickSettingsDragOverlay",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            _root.hideFlags = HideFlags.HideAndDontSave;
            if (parent != null)
            {
                _root.transform.SetParent(parent, false);
            }

            Canvas canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = OverlaySortingOrder;

            CanvasScaler scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            scaler.referencePixelsPerUnit = 100f;

            GameObject header = new GameObject(
                "HeaderDragSurface",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(QuickSettingsHeaderDragSurface));
            header.hideFlags = HideFlags.HideAndDontSave;
            header.transform.SetParent(_root.transform, false);

            _headerTransform = header.GetComponent<RectTransform>();
            _headerTransform.anchorMin = new Vector2(0f, 1f);
            _headerTransform.anchorMax = new Vector2(0f, 1f);
            _headerTransform.pivot = new Vector2(0f, 1f);
            _headerTransform.sizeDelta = new Vector2(0f, headerHeight);

            Image image = header.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;

            _dragSurface =
                header.GetComponent<QuickSettingsHeaderDragSurface>();
            _dragSurface.Initialize(_host);
        }

        private void HandleBoundsChanged(QuickSettingsWindowBounds bounds)
        {
            ApplyBounds(bounds);
        }

        private void HandleVisibilityChanged(bool visible)
        {
            if (visible)
            {
                WarnIfEventSystemMissing();
            }

            if (_root != null && _root.activeSelf != visible)
            {
                _root.SetActive(visible);
            }
        }

        private void WarnIfEventSystemMissing()
        {
            if (_missingEventSystemWarningLogged ||
                EventSystem.current != null)
            {
                return;
            }

            _missingEventSystemWarningLogged = true;
            if (Plugin.Log != null)
            {
                Plugin.Log.LogWarning(
                    "Quick Settings drag is unavailable because the " +
                    "current scene has no active Unity EventSystem.");
            }
        }

        private void ApplyBounds(QuickSettingsWindowBounds bounds)
        {
            if (_headerTransform == null)
            {
                return;
            }

            _headerTransform.anchoredPosition =
                new Vector2(bounds.X, -bounds.Y);
            _headerTransform.sizeDelta = new Vector2(
                bounds.Width,
                _headerTransform.sizeDelta.y);
        }
    }
}
