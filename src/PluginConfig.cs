using System;
using BepInEx.Configuration;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class PluginConfig
    {
        private const string DefaultPositiveXBlendshape = "eye_motion.f00_eye_posx";
        private const string DefaultNegativeXBlendshape = "eye_motion.f00_eye_negx";
        private const string DefaultPositiveYBlendshape = "eye_motion.f00_eye_posy";
        private const string DefaultNegativeYBlendshape = "eye_motion.f00_eye_negy";
        private const string DefaultBlinkBlendshape = "eye_motion.f00_eye_blink";

        private const string ImportedPositiveXBlendshape = "eye_motion.f00_eye_posx_0";
        private const string ImportedNegativeXBlendshape = "eye_motion.f00_eye_negx_0";
        private const string ImportedPositiveYBlendshape = "eye_motion.f00_eye_posy_0";
        private const string ImportedNegativeYBlendshape = "eye_motion.f00_eye_negy_0";
        private const string ImportedBlinkBlendshape = "eye_motion.f00_eye_blink_0";

        private const string LegacyPositiveXBlendshape = "EyeMotion_PosX";
        private const string LegacyNegativeXBlendshape = "EyeMotion_NegX";
        private const string LegacyPositiveYBlendshape = "EyeMotion_PosY";
        private const string LegacyNegativeYBlendshape = "EyeMotion_NegY";
        private const string LegacyBlinkBlendshape = "EyeMotion_Blink";

        private const string GeneralSection = "General";
        private const string TargetSection = "Target Headmod";
        private const string NamesSection = "Blendshape Names";
        private const string HorizontalSection = "Horizontal Movement";
        private const string VerticalSection = "Vertical Movement";
        private const string BlinkSection = "Blink";
        private const string SmoothingSection = "Smoothing";
        private const string EyeAdjustmentSection =
            "ExpressionControl Eye Adjustments";
        private const string EyeAdjustmentNamesSection =
            "ExpressionControl Eye Adjustment Blendshapes";
        private const string ExpressionAutomationSection =
            "Automatic Expressions";
        private const string ManualVisibilitySection = "Manual Visibility";
        private const string ManualVisibilityBlendshapesSection =
            "Manual Visibility Blendshapes";
        private const string ManualVisibilityRenderersSection =
            "Manual Visibility Renderers";
        private const string PersistenceSection = "Persistence";
        private const string DiagnosticsSection = "Diagnostics";

        private static ConfigFile _configFile;

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<WeightRestoreMode> RestoreMode;

        internal static ConfigEntry<string> TargetRendererPath;
        internal static ConfigEntry<string> TargetRendererName;
        internal static ConfigEntry<TargetSearchScope> SearchScope;
        internal static ConfigEntry<bool> RequireAllDirectionalBlendshapes;
        internal static ConfigEntry<bool> RequireBlinkWhenEnabled;
        internal static ConfigEntry<int> BindingRetryFrames;

        internal static ConfigEntry<string> PositiveXBlendshape;
        internal static ConfigEntry<string> NegativeXBlendshape;
        internal static ConfigEntry<string> PositiveYBlendshape;
        internal static ConfigEntry<string> NegativeYBlendshape;
        internal static ConfigEntry<string> BlinkBlendshape;

        internal static ConfigEntry<bool> InvertX;
        internal static ConfigEntry<SourceEyeMode> SourceEyeMode;
        internal static ConfigEntry<bool> ClampInputToUnitCircle;
        internal static ConfigEntry<float> HorizontalCenterOffset;
        internal static ConfigEntry<float> PositiveXInputLimit;
        internal static ConfigEntry<float> NegativeXInputLimit;
        internal static ConfigEntry<float> PositiveXMaxWeight;
        internal static ConfigEntry<float> NegativeXMaxWeight;
        internal static ConfigEntry<float> PositiveXDeadZone;
        internal static ConfigEntry<float> NegativeXDeadZone;
        internal static ConfigEntry<float> PositiveXGamma;
        internal static ConfigEntry<float> NegativeXGamma;

        internal static ConfigEntry<bool> InvertY;
        internal static ConfigEntry<float> VerticalCenterOffset;
        internal static ConfigEntry<float> PositiveYInputLimit;
        internal static ConfigEntry<float> NegativeYInputLimit;
        internal static ConfigEntry<float> PositiveYMaxWeight;
        internal static ConfigEntry<float> NegativeYMaxWeight;
        internal static ConfigEntry<float> PositiveYDeadZone;
        internal static ConfigEntry<float> NegativeYDeadZone;
        internal static ConfigEntry<float> PositiveYGamma;
        internal static ConfigEntry<float> NegativeYGamma;

        internal static ConfigEntry<bool> BlinkEnabled;
        internal static ConfigEntry<float> BlinkMaxWeight;
        internal static ConfigEntry<float> BlinkDeadZone;
        internal static ConfigEntry<float> BlinkGamma;
        internal static ConfigEntry<float> BlinkSmoothingSpeed;

        internal static ConfigEntry<bool> SmoothingEnabled;
        internal static ConfigEntry<float> SmoothingSpeed;
        internal static ConfigEntry<bool> UseUnscaledTime;

        internal static ConfigEntry<bool> EyeAdjustmentEnabled;
        internal static ConfigEntry<float> IrisYMaxWeight;
        internal static ConfigEntry<float> IrisSizeMaxWeight;
        internal static ConfigEntry<string>[] EyeAdjustmentBlendshapeNames;

        internal static ConfigEntry<bool> ExpressionAutomationEnabled;
        internal static ConfigEntry<float> ExpressionActivationThreshold;

        internal static ConfigEntry<float> ManualHideBlendshapeWeight;
        internal static ConfigEntry<bool> FollowBaseGameHighlightVisibility;
        internal static ConfigEntry<string>[] ManualVisibilityBlendshapeNames;
        internal static ConfigEntry<string>[] ManualVisibilityRendererTargets;
        internal static ConfigEntry<bool> CardPersistenceEnabled;

        internal static ConfigEntry<bool> DebugLogging;
        internal static ConfigEntry<bool> LogEyeValues;
        internal static ConfigEntry<float> LogIntervalSeconds;
        internal static ConfigEntry<KeyboardShortcut> QuickSettingsShortcut;
        internal static ConfigEntry<KeyboardShortcut> DumpDiagnosticsShortcut;

        internal static int Revision { get; private set; }
        internal static int BindingRevision { get; private set; }
        internal static int VisibilityDefinitionRevision { get; private set; }
        internal static int VisibilityRevision { get; private set; }

        internal static void Bind(ConfigFile config)
        {
            _configFile = config;

            Enabled = config.Bind(GeneralSection, "Enabled", true,
                "Enable KK_ExpressionLink globally.");
            RestoreMode = config.Bind(GeneralSection, "RestoreMode", WeightRestoreMode.InitialValues,
                "Restore the captured initial values or zero the managed blendshapes when control ends.");

            TargetRendererPath = config.Bind(TargetSection, "TargetRendererPath", string.Empty,
                "Exact path relative to the ChaControl transform. Leave empty for automatic detection.");
            TargetRendererName = config.Bind(TargetSection, "TargetRendererName", string.Empty,
                "Exact renderer GameObject name used as a priority hint. Leave empty for automatic detection.");
            SearchScope = config.Bind(TargetSection, "SearchScope", TargetSearchScope.HeadFirst,
                "HeadOnly searches ChaControl.objHead; HeadFirst falls back to the character; Character searches the full character.");
            RequireAllDirectionalBlendshapes = config.Bind(TargetSection, "RequireAllDirectionalBlendshapes", true,
                "Require all four directional blendshapes before accepting a renderer.");
            RequireBlinkWhenEnabled = config.Bind(TargetSection, "RequireBlinkWhenEnabled", true,
                "Require the blink blendshape when BlinkEnabled is true.");
            BindingRetryFrames = BindRange(config, TargetSection, "BindingRetryFrames", 60, 1, 600,
                "Maximum number of one-attempt-per-frame binding retries after a character reload.");

            PositiveXBlendshape = config.Bind(NamesSection, "PositiveXBlendshape", DefaultPositiveXBlendshape,
                "Blendshape used for positive horizontal input.");
            NegativeXBlendshape = config.Bind(NamesSection, "NegativeXBlendshape", DefaultNegativeXBlendshape,
                "Blendshape used for negative horizontal input.");
            PositiveYBlendshape = config.Bind(NamesSection, "PositiveYBlendshape", DefaultPositiveYBlendshape,
                "Blendshape used for positive vertical input.");
            NegativeYBlendshape = config.Bind(NamesSection, "NegativeYBlendshape", DefaultNegativeYBlendshape,
                "Blendshape used for negative vertical input.");
            BlinkBlendshape = config.Bind(NamesSection, "BlinkBlendshape", DefaultBlinkBlendshape,
                "Blendshape used for joint eye closure.");

            InvertX = config.Bind(HorizontalSection, "InvertX", false,
                "Invert the final common horizontal convention.");
            SourceEyeMode = config.Bind(HorizontalSection, "SourceEyeMode",
                global::NightOwlZzz.Koikatsu.EyeMotion.SourceEyeMode.Average,
                "Choose the average, left, or right common-axis rate. Average cancels normal frontal convergence.");
            ClampInputToUnitCircle = config.Bind(HorizontalSection, "ClampInputToUnitCircle", true,
                "Clamp the combined X/Y vector once. EyeLookCalc clamps each axis but not their combined magnitude.");
            HorizontalCenterOffset = BindRange(config, HorizontalSection, "HorizontalCenterOffset", 0f, -1f, 1f,
                "Raw selected horizontal value treated as the neutral center. Calibrate while looking straight ahead.");
            PositiveXInputLimit = BindUnit(config, HorizontalSection, "PositiveXInputLimit", 1f,
                "Positive X input that reaches maximum weight.");
            NegativeXInputLimit = BindUnit(config, HorizontalSection, "NegativeXInputLimit", 1f,
                "Negative X input magnitude that reaches maximum weight.");
            PositiveXMaxWeight = BindWeight(config, HorizontalSection, "PositiveXMaxWeight", 100f,
                "Maximum positive X blendshape weight.");
            NegativeXMaxWeight = BindWeight(config, HorizontalSection, "NegativeXMaxWeight", 100f,
                "Maximum negative X blendshape weight.");
            PositiveXDeadZone = BindUnit(config, HorizontalSection, "PositiveXDeadZone", 0f,
                "Positive X dead zone.");
            NegativeXDeadZone = BindUnit(config, HorizontalSection, "NegativeXDeadZone", 0f,
                "Negative X dead zone.");
            PositiveXGamma = BindGamma(config, HorizontalSection, "PositiveXGamma", 1f,
                "Positive X response curve exponent.");
            NegativeXGamma = BindGamma(config, HorizontalSection, "NegativeXGamma", 1f,
                "Negative X response curve exponent.");

            InvertY = config.Bind(VerticalSection, "InvertY", false,
                "Invert the final vertical convention. Koikatsu's normalized positive rate is up.");
            VerticalCenterOffset = BindRange(config, VerticalSection, "VerticalCenterOffset", 0f, -1f, 1f,
                "Raw vertical value treated as the neutral center. Calibrate while looking straight ahead.");
            PositiveYInputLimit = BindUnit(config, VerticalSection, "PositiveYInputLimit", 1f,
                "Positive Y input that reaches maximum weight.");
            NegativeYInputLimit = BindUnit(config, VerticalSection, "NegativeYInputLimit", 1f,
                "Negative Y input magnitude that reaches maximum weight.");
            PositiveYMaxWeight = BindWeight(config, VerticalSection, "PositiveYMaxWeight", 100f,
                "Maximum positive Y blendshape weight.");
            NegativeYMaxWeight = BindWeight(config, VerticalSection, "NegativeYMaxWeight", 100f,
                "Maximum negative Y blendshape weight.");
            PositiveYDeadZone = BindUnit(config, VerticalSection, "PositiveYDeadZone", 0f,
                "Positive Y dead zone.");
            NegativeYDeadZone = BindUnit(config, VerticalSection, "NegativeYDeadZone", 0f,
                "Negative Y dead zone.");
            PositiveYGamma = BindGamma(config, VerticalSection, "PositiveYGamma", 1f,
                "Positive Y response curve exponent.");
            NegativeYGamma = BindGamma(config, VerticalSection, "NegativeYGamma", 1f,
                "Negative Y response curve exponent.");

            BlinkEnabled = config.Bind(BlinkSection, "BlinkEnabled", true,
                "Drive the joint blink blendshape from Koikatsu's effective eye closure.");
            BlinkMaxWeight = BindWeight(config, BlinkSection, "BlinkMaxWeight", 100f,
                "Maximum blink blendshape weight.");
            BlinkDeadZone = BindUnit(config, BlinkSection, "BlinkDeadZone", 0f,
                "Closure ignored near fully open eyes.");
            BlinkGamma = BindGamma(config, BlinkSection, "BlinkGamma", 1f,
                "Blink response curve exponent.");
            BlinkSmoothingSpeed = BindRange(config, BlinkSection, "BlinkSmoothingSpeed", 0f, 0f, 100f,
                "Independent final-weight blink smoothing speed. Zero disables blink smoothing.");

            SmoothingEnabled = config.Bind(SmoothingSection, "SmoothingEnabled", false,
                "Apply exponential smoothing to the four final directional weights.");
            SmoothingSpeed = BindRange(config, SmoothingSection, "SmoothingSpeed", 12f, 0.01f, 100f,
                "Exponential smoothing speed for directional weights.");
            UseUnscaledTime = config.Bind(SmoothingSection, "UseUnscaledTime", false,
                "Use unscaled delta time for optional smoothing.");

            EyeAdjustmentEnabled = config.Bind(
                EyeAdjustmentSection,
                "Enabled",
                true,
                "Drive the optional IrisY and Size custom-head blendshapes from KK_ExpressionControl when available.");
            IrisYMaxWeight = BindWeight(
                config,
                EyeAdjustmentSection,
                "IrisYMaxWeight",
                100f,
                "Blendshape weight reached when ExpressionControl IrisY is 0.5.");
            IrisSizeMaxWeight = BindWeight(
                config,
                EyeAdjustmentSection,
                "IrisSizeMaxWeight",
                100f,
                "Blendshape weight reached when ExpressionControl Size is 1.0.");

            EyeAdjustmentBlendshapeNames =
                new ConfigEntry<string>[EyeCustomizationCatalog.ChannelCount];
            for (int i = 0; i < EyeCustomizationCatalog.ChannelCount; i++)
            {
                EyeAdjustmentBlendshapeNames[i] = config.Bind(
                    EyeAdjustmentNamesSection,
                    EyeCustomizationCatalog.ConfigKeys[i],
                    EyeCustomizationCatalog.DefaultNames[i],
                    "Optional exact runtime blendshape driven by ExpressionControl " +
                    EyeCustomizationCatalog.DisplayNames[i] +
                    ". Missing channels do not make a headmod incompatible.");
            }

            ExpressionAutomationEnabled = config.Bind(
                ExpressionAutomationSection,
                "Enabled",
                true,
                "Automatically show configured ExpressionMesh slots from current brow/eyes/mouth patterns.");
            ExpressionActivationThreshold = BindUnit(
                config,
                ExpressionAutomationSection,
                "ActivationThreshold",
                0.001f,
                "Minimum current facial-pattern weight that activates a configured expression slot.");

            ManualHideBlendshapeWeight = BindWeight(
                config,
                ManualVisibilitySection,
                "HideBlendshapeWeight",
                100f,
                "Weight used by the manual Hidden state for fused hide blendshapes.");
            FollowBaseGameHighlightVisibility = config.Bind(
                ManualVisibilitySection,
                "FollowBaseGameHighlightVisibility",
                true,
                "Hide the two configured highlight blendshapes whenever Koikatsu's " +
                "Erase Highlight state is active, then restore their manual modes.");

            ManualVisibilityBlendshapeNames =
                new ConfigEntry<string>[ManualVisibilityCatalog.BlendshapeCount];
            for (int i = 0; i < ManualVisibilityCatalog.BlendshapeCount; i++)
            {
                ManualVisibilityBlendshapeNames[i] = config.Bind(
                    ManualVisibilityBlendshapesSection,
                    ManualVisibilityCatalog.BlendshapeConfigKeys[i],
                    ManualVisibilityCatalog.DefaultBlendshapeNames[i],
                    "Exact runtime blendshape name used to hide " +
                    ManualVisibilityCatalog.BlendshapeDisplayNames[i] +
                    ". Leave empty to disable this optional slot.");
            }

            ManualVisibilityRendererTargets =
                new ConfigEntry<string>[ManualVisibilityCatalog.RendererCount];
            for (int i = 0; i < ManualVisibilityCatalog.RendererCount; i++)
            {
                ManualVisibilityRendererTargets[i] = config.Bind(
                    ManualVisibilityRenderersSection,
                    ManualVisibilityCatalog.RendererConfigKeys[i],
                    ManualVisibilityCatalog.DefaultRendererTargets[i],
                    "Exact GameObject name or ChaControl-relative path for " +
                    ManualVisibilityCatalog.RendererDisplayNames[i] +
                    ". Ambiguous names are never selected.");
            }

            CardPersistenceEnabled = config.Bind(
                PersistenceSection,
                "CardPersistenceEnabled",
                true,
                "Save per-character visibility, expression triggers, and Expression Links " +
                "in character cards through ExtensibleSaveFormat.");

            DebugLogging = config.Bind(DiagnosticsSection, "DebugLogging", false,
                "Log binding decisions and state transitions.");
            LogEyeValues = config.Bind(DiagnosticsSection, "LogEyeValues", false,
                "Periodically log sampled values. Never logs every frame.");
            LogIntervalSeconds = BindRange(config, DiagnosticsSection, "LogIntervalSeconds", 2f, 0.1f, 60f,
                "Minimum interval between eye-value log messages per character.");
            QuickSettingsShortcut = config.Bind(DiagnosticsSection, "QuickSettingsShortcut",
                new KeyboardShortcut(KeyCode.M, KeyCode.LeftControl, KeyCode.LeftShift),
                "Toggle the compact KK_ExpressionLink runtime settings window.");
            DumpDiagnosticsShortcut = config.Bind(DiagnosticsSection, "DumpDiagnosticsShortcut",
                new KeyboardShortcut(KeyCode.F10, KeyCode.LeftShift),
                "Write a detailed report under BepInEx/config/KK_ExpressionLink/diagnostics.");

            MigrateLegacyBlendshapeDefaults();
            config.SettingChanged += OnSettingChanged;
            config.ConfigReloaded += OnConfigReloaded;
            Revision++;
            BindingRevision++;
            VisibilityDefinitionRevision++;
            VisibilityRevision++;
        }

        internal static void Dispose()
        {
            if (_configFile == null)
            {
                return;
            }

            _configFile.SettingChanged -= OnSettingChanged;
            _configFile.ConfigReloaded -= OnConfigReloaded;
            _configFile = null;
        }

        internal static void ApplyBatch(Action apply)
        {
            if (apply == null)
            {
                throw new ArgumentNullException("apply");
            }

            if (_configFile == null)
            {
                apply();
                return;
            }

            bool saveOnConfigSet = _configFile.SaveOnConfigSet;
            _configFile.SaveOnConfigSet = false;
            try
            {
                apply();
                if (saveOnConfigSet)
                {
                    _configFile.Save();
                }
            }
            finally
            {
                _configFile.SaveOnConfigSet = saveOnConfigSet;
            }
        }

        private static void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            Revision++;
            if (e != null && AffectsBinding(e.ChangedSetting))
            {
                BindingRevision++;
            }

            if (e != null && AffectsVisibility(e.ChangedSetting))
            {
                VisibilityRevision++;
            }

            if (e != null && AffectsVisibilityDefinition(e.ChangedSetting))
            {
                VisibilityDefinitionRevision++;
            }
        }

        private static void OnConfigReloaded(object sender, EventArgs e)
        {
            Revision++;
            BindingRevision++;
            VisibilityDefinitionRevision++;
            VisibilityRevision++;
        }

        internal static string GetManualVisibilityBlendshapeName(int index)
        {
            if (ManualVisibilityBlendshapeNames == null ||
                index < 0 ||
                index >= ManualVisibilityBlendshapeNames.Length)
            {
                return string.Empty;
            }

            return ManualVisibilityBlendshapeNames[index].Value ?? string.Empty;
        }

        internal static string GetEyeAdjustmentBlendshapeName(int index)
        {
            if (EyeAdjustmentBlendshapeNames == null ||
                index < 0 ||
                index >= EyeAdjustmentBlendshapeNames.Length)
            {
                return string.Empty;
            }

            return EyeAdjustmentBlendshapeNames[index].Value ?? string.Empty;
        }

        internal static string GetManualVisibilityRendererTarget(int index)
        {
            if (ManualVisibilityRendererTargets == null ||
                index < 0 ||
                index >= ManualVisibilityRendererTargets.Length)
            {
                return string.Empty;
            }

            return ManualVisibilityRendererTargets[index].Value ?? string.Empty;
        }

        private static bool AffectsBinding(ConfigEntryBase entry)
        {
            if (entry == null)
            {
                return false;
            }

            ConfigDefinition definition = entry.Definition;
            return ConfigChangeClassifier.AffectsEyeBinding(
                definition.Section,
                definition.Key);
        }

        private static bool AffectsVisibility(ConfigEntryBase entry)
        {
            return entry != null &&
                   ConfigChangeClassifier.AffectsVisibilityValues(
                       entry.Definition.Section);
        }

        private static bool AffectsVisibilityDefinition(ConfigEntryBase entry)
        {
            if (entry == null)
            {
                return false;
            }

            return ConfigChangeClassifier.AffectsVisibilityDefinitions(
                entry.Definition.Section);
        }

        private static void MigrateLegacyBlendshapeDefaults()
        {
            bool usesLegacyNames =
                string.Equals(PositiveXBlendshape.Value, LegacyPositiveXBlendshape, StringComparison.Ordinal) &&
                string.Equals(NegativeXBlendshape.Value, LegacyNegativeXBlendshape, StringComparison.Ordinal) &&
                string.Equals(PositiveYBlendshape.Value, LegacyPositiveYBlendshape, StringComparison.Ordinal) &&
                string.Equals(NegativeYBlendshape.Value, LegacyNegativeYBlendshape, StringComparison.Ordinal) &&
                string.Equals(BlinkBlendshape.Value, LegacyBlinkBlendshape, StringComparison.Ordinal);
            bool usesImportedSuffixNames =
                string.Equals(PositiveXBlendshape.Value, ImportedPositiveXBlendshape, StringComparison.Ordinal) &&
                string.Equals(NegativeXBlendshape.Value, ImportedNegativeXBlendshape, StringComparison.Ordinal) &&
                string.Equals(PositiveYBlendshape.Value, ImportedPositiveYBlendshape, StringComparison.Ordinal) &&
                string.Equals(NegativeYBlendshape.Value, ImportedNegativeYBlendshape, StringComparison.Ordinal) &&
                string.Equals(BlinkBlendshape.Value, ImportedBlinkBlendshape, StringComparison.Ordinal);
            if (!usesLegacyNames && !usesImportedSuffixNames)
            {
                return;
            }

            PositiveXBlendshape.Value = DefaultPositiveXBlendshape;
            NegativeXBlendshape.Value = DefaultNegativeXBlendshape;
            PositiveYBlendshape.Value = DefaultPositiveYBlendshape;
            NegativeYBlendshape.Value = DefaultNegativeYBlendshape;
            BlinkBlendshape.Value = DefaultBlinkBlendshape;
            Plugin.Log.LogInfo(
                "Migrated the five older EyeMotion blendshape defaults to the " +
                "confirmed eye_motion.f00_eye_* runtime names.");
        }

        private static ConfigEntry<float> BindUnit(
            ConfigFile config,
            string section,
            string key,
            float value,
            string description)
        {
            return BindRange(config, section, key, value, 0f, 1f, description);
        }

        private static ConfigEntry<float> BindWeight(
            ConfigFile config,
            string section,
            string key,
            float value,
            string description)
        {
            return BindRange(config, section, key, value, 0f, 100f, description);
        }

        private static ConfigEntry<float> BindGamma(
            ConfigFile config,
            string section,
            string key,
            float value,
            string description)
        {
            return BindRange(config, section, key, value, 0.01f, 8f, description);
        }

        private static ConfigEntry<float> BindRange(
            ConfigFile config,
            string section,
            string key,
            float value,
            float minimum,
            float maximum,
            string description)
        {
            return config.Bind(
                section,
                key,
                value,
                new ConfigDescription(description, new AcceptableValueRange<float>(minimum, maximum)));
        }

        private static ConfigEntry<int> BindRange(
            ConfigFile config,
            string section,
            string key,
            int value,
            int minimum,
            int maximum,
            string description)
        {
            return config.Bind(
                section,
                key,
                value,
                new ConfigDescription(description, new AcceptableValueRange<int>(minimum, maximum)));
        }
    }
}
