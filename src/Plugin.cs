using System;
using System.Diagnostics;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Studio.UI;
using UnityEngine;
using UnityEngine.Events;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("marco.kkapi", "1.42.2")]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    [BepInProcess("Koikatu.exe")]
    [BepInProcess("CharaStudio.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "com.nightowlzzz.koikatsu.eyemotion";
        internal const string PluginName = "KK_ExpressionLink";
        internal const string PluginVersion = "0.5.0";

        internal static ManualLogSource Log;

        private QuickSettingsWindow _quickSettingsWindow;
        private Texture2D _studioToolbarIcon;

        private void Awake()
        {
            Log = Logger;
            PluginConfig.Bind(Config);
            _quickSettingsWindow = new QuickSettingsWindow();
            useGUILayout = false;
            MakerAPI.RegisterCustomSubCategories += RegisterMakerControls;
            RegisterStudioToolbarButton();

            string samplerError;
            if (!EyeStateSampler.Initialize(out samplerError))
            {
                Logger.LogWarning(
                    "Blink sampling is unavailable. Directional eye movement will remain active. " +
                    samplerError);
            }

            CharacterApi.RegisterExtraBehaviour<EyeMotionCharacterController>(PluginGuid);
            Logger.LogInfo(
                "KK_ExpressionLink 0.5.0 loaded. Eye motion, blink, optional " +
                "ExpressionControl iris adjustments, and multi-renderer expression links are active. " +
                "Manual visibility and base-game highlight synchronization remain available.");
        }

        private void Update()
        {
            KeyboardShortcut quickSettingsShortcut = PluginConfig.QuickSettingsShortcut.Value;
            if (quickSettingsShortcut.IsDown() && _quickSettingsWindow != null)
            {
                ToggleQuickSettings();
            }

            KeyboardShortcut shortcut = PluginConfig.DumpDiagnosticsShortcut.Value;
            if (shortcut.IsDown())
            {
                Diagnostics.WriteReport();
            }
        }

        private void OnGUI()
        {
            if (_quickSettingsWindow != null)
            {
                _quickSettingsWindow.Draw();
                useGUILayout = _quickSettingsWindow.Visible;
            }
        }

        private void OnDestroy()
        {
            MakerAPI.RegisterCustomSubCategories -= RegisterMakerControls;
            EyeMotionCharacterController.RestoreAll();
            if (_studioToolbarIcon != null)
            {
                UnityEngine.Object.Destroy(_studioToolbarIcon);
                _studioToolbarIcon = null;
            }

            _quickSettingsWindow = null;
            PluginConfig.Dispose();
            Log = null;
        }

        private void RegisterMakerControls(
            object sender,
            RegisterSubCategoriesEvent registration)
        {
            if (registration == null)
            {
                return;
            }

            MakerCategory category = new MakerCategory(
                "00_FaceTop",
                "tglEyeMotion",
                130,
                "Expression Link");
            registration.AddSubCategory(category);
            MakerButton button = registration.AddControl(
                new MakerButton(
                    "Open Expression Link Quick Settings",
                    category,
                    this));
            button.OnClick.AddListener(new UnityAction(ToggleQuickSettings));
        }

        private void RegisterStudioToolbarButton()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    if (!string.Equals(
                        process.ProcessName,
                        "CharaStudio",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                _studioToolbarIcon = CreateStudioToolbarIcon();
                CustomToolbarButtons.AddLeftToolbarButton(
                    _studioToolbarIcon,
                    ToggleQuickSettings);
            }
            catch (Exception exception)
            {
                Logger.LogWarning(
                    "KK_ExpressionLink could not add its Studio toolbar button: " +
                    exception.GetType().Name + ": " + exception.Message);
            }
        }

        private void ToggleQuickSettings()
        {
            if (_quickSettingsWindow != null)
            {
                _quickSettingsWindow.Toggle();
                useGUILayout = _quickSettingsWindow.Visible;
            }
        }

        private static Texture2D CreateStudioToolbarIcon()
        {
            const int size = 32;
            Texture2D texture = new Texture2D(
                size,
                size,
                TextureFormat.ARGB32,
                false);
            texture.name = "KK_ExpressionLink Studio Toolbar Icon";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float backgroundT = y / (float)(size - 1);
                    Color color = Color.Lerp(
                        new Color(0.29f, 0.30f, 0.29f, 1f),
                        new Color(0.42f, 0.43f, 0.42f, 1f),
                        backgroundT);

                    // The toolbar API replaces the stock button sprite, so
                    // bake Studio's neutral square background and bevel into
                    // this texture before drawing the eye glyph on top.
                    if (y == size - 1)
                    {
                        color = new Color(0.51f, 0.51f, 0.51f, 1f);
                    }
                    else if (y == 0)
                    {
                        color = new Color(0.42f, 0.42f, 0.42f, 1f);
                    }
                    else if (x == 0)
                    {
                        color = Color.Lerp(
                            new Color(0.46f, 0.46f, 0.46f, 1f),
                            new Color(0.52f, 0.53f, 0.52f, 1f),
                            backgroundT);
                    }
                    else if (x == size - 1)
                    {
                        color = Color.Lerp(
                            new Color(0.42f, 0.42f, 0.42f, 1f),
                            new Color(0.49f, 0.50f, 0.49f, 1f),
                            backgroundT);
                    }

                    float nx = (x - 15.5f) / 14f;
                    float ny = (y - 15.5f) / 8f;
                    float eyeDistance = nx * nx + ny * ny;
                    float irisX = (x - 15.5f) / 5.25f;
                    float irisY = (y - 15.5f) / 5.25f;
                    float irisDistance = irisX * irisX + irisY * irisY;

                    // Studio's built-in toolbar uses small neutral monochrome
                    // glyphs. Keep the existing soft gray eye on top of the
                    // matching button background.
                    if (eyeDistance >= 0.74f && eyeDistance <= 1.08f)
                    {
                        color = new Color(0.78f, 0.80f, 0.82f, 1f);
                    }

                    if (irisDistance >= 0.55f && irisDistance <= 1f)
                    {
                        color = new Color(0.88f, 0.89f, 0.90f, 1f);
                    }

                    if (irisDistance <= 0.18f)
                    {
                        color = new Color(0.68f, 0.70f, 0.72f, 1f);
                    }

                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
