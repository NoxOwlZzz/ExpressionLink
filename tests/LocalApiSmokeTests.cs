using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class LocalApiSmokeTests
    {
        private const string PluginNamespace =
            "NightOwlZzz.Koikatsu.EyeMotion.";

        private static int _checks;
        private static int _failures;

        internal static int Run(out int checks)
        {
            _checks = 0;
            _failures = 0;

            try
            {
                RunChecks();
            }
            catch (Exception exception)
            {
                _failures++;
                Console.Error.WriteLine(
                    "FAIL local API smoke suite: " +
                    exception.GetBaseException().Message);
            }

            checks = _checks;
            return _failures;
        }

        private static void RunChecks()
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string quickSettingsSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\QuickSettingsCoordinator.cs"));
            string windowFrameSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\QuickSettingsWindowFrame.cs"));
            string movableWindowSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\QuickSettingsMovableWindow.cs"));
            string visualFactorySource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\QuickSettingsWindowVisualFactory.cs"));
            string sourceTree = ReadSourceTree(
                Path.Combine(projectRoot, @"src"));
            Check(
                "quick-settings GUI.Window source removed",
                quickSettingsSource.IndexOf(
                    "GUI.Window",
                    StringComparison.Ordinal) < 0);
            Check(
                "quick-settings has no polling input",
                sourceTree.IndexOf(
                    "UpdateInput",
                    StringComparison.Ordinal) < 0);
            Check(
                "quick-settings has no shared mutable rect",
                sourceTree.IndexOf(
                    "_windowRect",
                    StringComparison.Ordinal) < 0);
            Check(
                "quick-settings is not partial",
                sourceTree.IndexOf(
                    "partial class QuickSettingsCoordinator",
                    StringComparison.Ordinal) < 0);
            Check(
                "quick-settings has no mouse polling",
                sourceTree.IndexOf(
                    "Input.GetMouseButton",
                    StringComparison.Ordinal) < 0);
            Check(
                "quick-settings uses progressive disclosure",
                sourceTree.IndexOf(
                    "QuickSettingsGui.Disclosure(",
                    StringComparison.Ordinal) >= 0);
            Check(
                "quick-settings window is created lazily",
                quickSettingsSource.IndexOf(
                    "EnsureWindow()",
                    StringComparison.Ordinal) >= 0);
            string reloadCall = "_configSession.Reload();";
            int reloadCallIndex = quickSettingsSource.IndexOf(
                reloadCall,
                StringComparison.Ordinal);
            Check(
                "opening quick-settings preserves unsaved settings",
                reloadCallIndex >= 0 && reloadCallIndex ==
                    quickSettingsSource.LastIndexOf(
                        reloadCall,
                        StringComparison.Ordinal));
            Check(
                "opening quick-settings refreshes clean settings",
                quickSettingsSource.IndexOf(
                    "_configSession.ReloadIfClean();",
                    StringComparison.Ordinal) >= 0);
            Check(
                "Material Editor drag samples Input.mousePosition",
                movableWindowSource.IndexOf(
                    "Input.mousePosition",
                    StringComparison.Ordinal) >= 0);
            Check(
                "Material Editor drag writes the target RectTransform",
                movableWindowSource.IndexOf(
                    "ToDrag.position = newPosition",
                    StringComparison.Ordinal) >= 0);
            Check(
                "drag does not convert PointerEventData position",
                movableWindowSource.IndexOf(
                    "eventData.position",
                    StringComparison.Ordinal) < 0);
            Check(
                "drag does not consume PointerEventData",
                movableWindowSource.IndexOf(
                    "eventData.Use",
                    StringComparison.Ordinal) < 0);
            Check(
                "drag does not retain pointer identity",
                movableWindowSource.IndexOf(
                    "pointerId",
                    StringComparison.Ordinal) < 0);
            Check(
                "window frame has no per-frame position writer",
                windowFrameSource.IndexOf(
                    "void Update(",
                    StringComparison.Ordinal) < 0);
            Check(
                "header is a real raycast graphic",
                visualFactorySource.IndexOf(
                    "image.raycastTarget = true",
                    StringComparison.Ordinal) >= 0);
            string[] loadOrder =
            {
                @"lib\UnityEngine.dll",
                @"lib\UnityEngine.UI.dll",
                @"lib\Assembly-CSharp.dll",
                @"lib\BepInEx.dll",
                @"lib\ExtensibleSaveFormat.dll",
                @"lib\KKAPI.dll",
                @"bin\Release\KK_EyeMotion.dll"
            };
            Dictionary<string, Assembly> assemblies =
                new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < loadOrder.Length; i++)
            {
                string absolutePath = Path.Combine(projectRoot, loadOrder[i]);
                Check("exists: " + loadOrder[i], File.Exists(absolutePath));
                Assembly assembly = Assembly.LoadFrom(absolutePath);
                assemblies[assembly.GetName().Name] = assembly;
            }

            Assembly pluginAssembly = assemblies["KK_EyeMotion"];
            Check("assembly name", pluginAssembly.GetName().Name == "KK_EyeMotion");
            Check(
                "assembly version",
                pluginAssembly.GetName().Version.ToString() == "0.5.0.0");

            Type pluginType = GetPluginType(pluginAssembly, "Plugin");
            CustomAttributeData pluginAttribute = FindAttribute(
                pluginType,
                "BepInEx.BepInPlugin");
            Check("BepInPlugin attribute", pluginAttribute != null);
            Check(
                "plugin GUID",
                GetConstructorArgument(pluginAttribute, 0) ==
                    "com.nightowlzzz.koikatsu.eyemotion");
            Check(
                "plugin name",
                GetConstructorArgument(pluginAttribute, 1) == "KK_ExpressionLink");
            Check(
                "plugin semantic version",
                GetConstructorArgument(pluginAttribute, 2) == "0.5.0");
            Check(
                "Extended Save dependency attribute",
                CountAttributesWithFirstArgument(
                    pluginType,
                    "BepInEx.BepInDependency",
                    "com.bepis.bepinex.extendedsave") == 1);

            BindingFlags instanceNonPublic =
                BindingFlags.Instance | BindingFlags.NonPublic;
            Check(
                "plugin polling focus callback removed",
                pluginType.GetMethod(
                    "OnApplicationFocus", instanceNonPublic) == null);
            Type quickSettingsType = GetPluginType(
                pluginAssembly,
                "QuickSettingsCoordinator");
            Check("quick-settings coordinator type", quickSettingsType != null);
            Check(
                "quick-settings toggle method",
                quickSettingsType.GetMethod("Toggle", instanceNonPublic) != null);
            Check(
                "quick-settings draw method",
                quickSettingsType.GetMethod("Draw", instanceNonPublic) != null);
            Check(
                "quick-settings polling focus handler removed",
                quickSettingsType.GetMethod(
                    "HandleApplicationFocus", instanceNonPublic) == null);
            Type quickSettingsSurfaceType = GetPluginType(
                pluginAssembly,
                "QuickSettingsSurfaceView");
            Check(
                "quick-settings surface type", quickSettingsSurfaceType != null);
            Type navigationType = GetPluginType(
                pluginAssembly,
                "QuickSettingsNavigationView");
            Type footerType = GetPluginType(
                pluginAssembly,
                "QuickSettingsFooterView");
            Check("quick-settings navigation type", navigationType != null);
            Check("quick-settings footer type", footerType != null);
            Check(
                "quick-settings selected-view dispatch",
                quickSettingsSurfaceType.GetMethod(
                    "DrawSelectedView", instanceNonPublic) != null);
            Check(
                "quick-settings owns navigation state",
                quickSettingsSurfaceType.GetField(
                    "_navigation", instanceNonPublic) != null);
            Check(
                "quick-settings owns footer actions",
                quickSettingsSurfaceType.GetField(
                    "_footer",
                    instanceNonPublic) != null);
            Check(
                "duplicated IMGUI window title removed",
                quickSettingsSurfaceType.GetField(
                    "_windowContent",
                    instanceNonPublic) == null);
            Check(
                "quick-settings old window delegate removed",
                quickSettingsSurfaceType.GetField(
                    "_drawWindowFunction",
                    instanceNonPublic) == null);
            Check(
                "quick-settings owns one window frame",
                quickSettingsType.GetField(
                    "_window",
                    instanceNonPublic) != null);
            Check(
                "quick-settings owns surface view",
                quickSettingsType.GetField(
                    "_surfaceView",
                    instanceNonPublic) != null);
            Check(
                "quick-settings config session type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsConfigSession") != null);
            Check(
                "quick-settings config store type",
                GetPluginType(
                    pluginAssembly,
                    "PluginConfigQuickSettingsStore") != null);
            Check(
                "shared IMGUI primitives type",
                GetPluginType(
                    pluginAssembly,
                    "ImGuiPrimitives") != null);
            Type windowFrameType = GetPluginType(
                pluginAssembly,
                "QuickSettingsWindowFrame");
            Type movableWindowType = GetPluginType(
                pluginAssembly,
                "QuickSettingsMovableWindow");
            Check("quick-settings window frame type", windowFrameType != null);
            Check(
                "quick-settings visual factory type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsWindowVisualFactory") != null);
            Check(
                "quick-settings rect controller type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsWindowRectController") != null);
            Check("Material Editor movable-window type", movableWindowType != null);
            BindingFlags instancePublic =
                BindingFlags.Instance | BindingFlags.Public;
            Check(
                "movable-window pointer-down handler",
                movableWindowType.GetMethod(
                    "OnPointerDown", instancePublic) != null);
            Check(
                "movable-window drag handler",
                movableWindowType.GetMethod(
                    "OnDrag", instancePublic) != null);
            Check(
                "movable-window pointer-up handler",
                movableWindowType.GetMethod(
                    "OnPointerUp", instancePublic) != null);
            Check(
                "legacy window host removed",
                pluginAssembly.GetType(
                    PluginNamespace + "QuickSettingsWindowHost",
                    false) == null);
            Check(
                "legacy drag overlay removed",
                pluginAssembly.GetType(
                    PluginNamespace + "QuickSettingsDragOverlay",
                    false) == null);
            Check(
                "legacy drag surface removed",
                pluginAssembly.GetType(
                    PluginNamespace + "QuickSettingsHeaderDragSurface",
                    false) == null);
            Check(
                "monolithic window canvas removed",
                pluginAssembly.GetType(
                    PluginNamespace + "QuickSettingsWindowCanvas",
                    false) == null);
            Check(
                "quick-settings visibility view type",
                GetPluginType(
                    pluginAssembly,
                    "VisibilitySettingsView") != null);
            Check(
                "manual visibility runtime view type",
                GetPluginType(
                    pluginAssembly,
                    "ManualVisibilityRuntimeView") != null);
            Check(
                "visibility target editor view type",
                GetPluginType(
                    pluginAssembly,
                    "VisibilityTargetEditorView") != null);
            Type expressionSettingsType = GetPluginType(
                pluginAssembly,
                "ExpressionSettingsView");
            Check("automatic mappings view type", expressionSettingsType != null);
            Check(
                "automatic mappings expose unsaved state",
                expressionSettingsType.GetProperty(
                    "HasUnsavedChanges", instanceNonPublic) != null);
            Check(
                "automatic mappings reject unsafe navigation",
                expressionSettingsType.GetMethod(
                    "RejectNavigationChange", instanceNonPublic) != null);
            Check(
                "automatic mappings can discard unavailable edits",
                expressionSettingsType.GetMethod(
                    "DiscardUnavailableMappings",
                    instanceNonPublic) != null);
            Check(
                "automatic mappings own dirty state",
                expressionSettingsType.GetField(
                    "_triggersDirty", instanceNonPublic) != null);
            Type linkEditorType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkEditorView");
            Check("expression-link editor type", linkEditorType != null);
            Type linkDraftPanelType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkDraftPanel");
            Check("expression-link draft panel type", linkDraftPanelType != null);
            Check(
                "link draft reports edits from current draw",
                linkDraftPanelType.GetProperty(
                    "ChangedDuringLastDraw", instanceNonPublic) != null);
            Check(
                "quick-settings expression-link editor",
                quickSettingsSurfaceType.GetField(
                    "_expressionLinkEditor",
                    instanceNonPublic) != null);
            Check(
                "quick-settings visible navigation feedback",
                quickSettingsSurfaceType.GetField(
                    "_navigationFeedback",
                    instanceNonPublic) != null);
            Type linkProfilePanelType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkProfilePanel");
            Check(
                "expression-link profile panel type",
                linkProfilePanelType != null);
            Check(
                "profile replacement requires confirmation state",
                linkProfilePanelType.GetField(
                    "_confirmReplace", instanceNonPublic) != null);
            Check(
                "Maker quick-settings launcher",
                pluginType.GetMethod(
                    "RegisterMakerControls",
                    instanceNonPublic) != null);
            Check(
                "Studio quick-settings launcher",
                pluginType.GetMethod(
                    "RegisterStudioToolbarButton",
                    instanceNonPublic) != null);

            Type controllerType = GetPluginType(
                pluginAssembly,
                "EyeMotionCharacterController");
            CustomAttributeData executionAttribute = FindAttribute(
                controllerType,
                "UnityEngine.DefaultExecutionOrder");
            Check("execution-order attribute", executionAttribute != null);
            Check(
                "execution order 32000",
                Convert.ToInt32(
                    executionAttribute.ConstructorArguments[0].Value) == 32000);
            Type linkDefinitionType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkDefinition");
            Type linkRuntimeType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkRuntime");
            Type targetResolverType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkTargetResolver");
            Type rendererCatalogType = GetPluginType(
                pluginAssembly,
                "CharacterRendererCatalog");
            Type profileStoreType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkProfileStore");
            Check("expression-link definition type", linkDefinitionType != null);
            Check("expression-link runtime type", linkRuntimeType != null);
            Check("expression-link target resolver type", targetResolverType != null);
            Check("multi-renderer catalog type", rendererCatalogType != null);
            Check("expression-link profile store type", profileStoreType != null);
            Check(
                "controller expression-link count",
                controllerType.GetProperty(
                    "ExpressionLinkCount",
                    instanceNonPublic) != null);
            CheckMethod(
                controllerType,
                "AddExpressionLink",
                "controller add-expression-link API",
                instanceNonPublic);

            BindingFlags staticNonPublic =
                BindingFlags.Static | BindingFlags.NonPublic;
            Type cardDataType = GetPluginType(
                pluginAssembly,
                "VisibilityCardData");
            FieldInfo schemaVersionField = cardDataType.GetField(
                "SchemaVersion",
                staticNonPublic);
            FieldInfo linksKeyField = cardDataType.GetField(
                "LinksKey",
                staticNonPublic);
            Check(
                "card schema 3",
                schemaVersionField != null &&
                Convert.ToInt32(schemaVersionField.GetRawConstantValue()) == 3);
            Check(
                "expression-links card key",
                linksKeyField != null &&
                (string)linksKeyField.GetRawConstantValue() ==
                    "expressionLinksBinary");
            PropertyInfo activeControllersProperty = controllerType.GetProperty(
                "ActiveControllers",
                staticNonPublic);
            object controllersViewA = activeControllersProperty.GetValue(null, null);
            object controllersViewB = activeControllersProperty.GetValue(null, null);
            Check(
                "cached active-controller view",
                ReferenceEquals(controllersViewA, controllersViewB));

            Type configType = GetPluginType(pluginAssembly, "PluginConfig");
            CheckField(configType, "QuickSettingsShortcut", "quick-settings shortcut config", staticNonPublic);
            CheckField(configType, "HorizontalCenterOffset", "horizontal center config", staticNonPublic);
            CheckField(configType, "VerticalCenterOffset", "vertical center config", staticNonPublic);
            CheckField(configType, "ManualHideBlendshapeWeight", "manual hide weight config", staticNonPublic);
            CheckField(configType, "FollowBaseGameHighlightVisibility", "base highlight sync config", staticNonPublic);
            CheckField(configType, "CardPersistenceEnabled", "card persistence config", staticNonPublic);
            CheckField(configType, "ManualVisibilityBlendshapeNames", "manual visibility blendshape configs", staticNonPublic);
            CheckField(configType, "ManualVisibilityRendererTargets", "manual visibility renderer configs", staticNonPublic);
            CheckField(configType, "EyeAdjustmentEnabled", "ExpressionControl eye adjustment config", staticNonPublic);
            CheckField(configType, "EyeAdjustmentBlendshapeNames", "ExpressionControl eye blendshape configs", staticNonPublic);
            CheckField(configType, "IrisYMaxWeight", "IrisY max-weight config", staticNonPublic);
            CheckField(configType, "IrisSizeMaxWeight", "iris Size max-weight config", staticNonPublic);
            CheckField(configType, "ExpressionAutomationEnabled", "expression automation config", staticNonPublic);
            CheckField(configType, "ExpressionActivationThreshold", "expression threshold config", staticNonPublic);
            Check(
                "binding-only revision",
                configType.GetProperty("BindingRevision", staticNonPublic) != null);
            Check(
                "visibility-only revision",
                configType.GetProperty("VisibilityRevision", staticNonPublic) != null);
            Check(
                "visibility-definition-only revision",
                configType.GetProperty(
                    "VisibilityDefinitionRevision",
                    staticNonPublic) != null);
            Check(
                "batched config apply helper",
                configType.GetMethod("ApplyBatch", staticNonPublic) != null);
            Check(
                "vertex config removed",
                configType.GetField("VertexMetadataMode", staticNonPublic) == null);

            CheckConstant(
                configType,
                "DefaultPositiveXBlendshape",
                "eye_motion.f00_eye_posx",
                staticNonPublic);
            CheckConstant(
                configType,
                "DefaultNegativeXBlendshape",
                "eye_motion.f00_eye_negx",
                staticNonPublic);
            CheckConstant(
                configType,
                "DefaultPositiveYBlendshape",
                "eye_motion.f00_eye_posy",
                staticNonPublic);
            CheckConstant(
                configType,
                "DefaultNegativeYBlendshape",
                "eye_motion.f00_eye_negy",
                staticNonPublic);
            CheckConstant(
                configType,
                "DefaultBlinkBlendshape",
                "eye_motion.f00_eye_blink",
                staticNonPublic);

            Type visibilityBindingType = GetPluginType(
                pluginAssembly,
                "ManualVisibilityBinding");
            Check("manual visibility binding type", visibilityBindingType != null);
            Check(
                "manual blendshape commands",
                CountMethods(
                    visibilityBindingType,
                    "SetBlendshapeMode",
                    instanceNonPublic) == 2);
            CheckMethod(visibilityBindingType, "SetRendererMode", "manual renderer command", instanceNonPublic);
            CheckMethod(visibilityBindingType, "RestoreAll", "manual visibility restoration", instanceNonPublic);
            CheckMethod(visibilityBindingType, "SetAutomaticHighlightHidden", "automatic base-highlight synchronization", instanceNonPublic);
            CheckMethod(visibilityBindingType, "SetAutomaticExpressionState", "automatic expression visibility state", instanceNonPublic);
            Check(
                "vertex metadata scanner removed",
                pluginAssembly.GetType(
                    PluginNamespace + "VertexMetadataScanner",
                    false) == null);

            Type blendshapeBindingType = GetPluginType(
                pluginAssembly,
                "BlendshapeBinding");
            CheckMethod(blendshapeBindingType, "ApplyAlreadyValidated", "single-validation blendshape apply", instanceNonPublic);
            CheckMethod(blendshapeBindingType, "ApplyEyeCustomizationAlreadyValidated", "ExpressionControl eye adjustment apply", instanceNonPublic);
            Check(
                "managed-weight write coalescing state",
                GetPluginType(pluginAssembly, "ManagedWeightState") != null);

            Type directionalMapperType = GetPluginType(
                pluginAssembly,
                "DirectionalMapper");
            BindingFlags staticPublic = BindingFlags.Static | BindingFlags.Public;
            CheckMethod(directionalMapperType, "CalculateSmoothingAlpha", "shared smoothing-alpha helper", staticPublic);
            CheckMethod(directionalMapperType, "ApplySmoothingAlpha", "shared smoothing application helper", staticPublic);

            Check(
                "controller manual blendshape commands",
                CountMethods(
                    controllerType,
                    "SetManualBlendshapeVisibility",
                    instanceNonPublic) == 2);
            CheckMethod(controllerType, "SetManualRendererVisibility", "controller manual renderer command", instanceNonPublic);
            CheckMethod(controllerType, "RefreshManualVisibility", "controller manual visibility refresh", instanceNonPublic);
            CheckMethod(controllerType, "SetExpressionTriggers", "controller expression trigger batch command", instanceNonPublic);

            Type expressionTriggerType = GetPluginType(
                pluginAssembly,
                "ExpressionTriggerController");
            CheckMethod(expressionTriggerType, "Sample", "expression trigger numeric sampler", instanceNonPublic);
            Check(
                "ExpressionControl eye adjustment mapper",
                GetPluginType(pluginAssembly, "EyeCustomizationMapper") != null);
            Type eyeCustomizationCatalogType = GetPluginType(
                pluginAssembly,
                "EyeCustomizationCatalog");
            Check(
                "two optional ExpressionControl eye channels",
                Convert.ToInt32(
                    eyeCustomizationCatalogType.GetField(
                        "ChannelCount",
                        staticNonPublic).GetRawConstantValue()) == 2);
            Type eyeSourceType = GetPluginType(
                pluginAssembly,
                "ExpressionControlEyeSource");
            Check("optional ExpressionControl bridge type", eyeSourceType != null);
            CheckMethod(eyeSourceType, "Resolve", "ExpressionControl bridge resolver", staticNonPublic);
            CheckMethod(eyeSourceType, "TrySample", "ExpressionControl bridge numeric sampler", instanceNonPublic);

            Type catalogType = GetPluginType(
                pluginAssembly,
                "ManualVisibilityCatalog");
            Check(
                "ten manual hide slots",
                Convert.ToInt32(
                    catalogType.GetField(
                        "BlendshapeCount",
                        staticNonPublic).GetRawConstantValue()) == 10);
            Check(
                "four expression renderer slots",
                Convert.ToInt32(
                    catalogType.GetField(
                        "RendererCount",
                        staticNonPublic).GetRawConstantValue()) == 4);
            string[] manualDefaults = (string[])catalogType.GetField(
                "DefaultBlendshapeNames",
                staticNonPublic).GetValue(null);
            Check("ten manual hide defaults", manualDefaults.Length == 10);
            Check(
                "manual defaults follow confirmed runtime suffix convention",
                manualDefaults[0] == "eye_motion.f00_hide_highlight01");

            List<string> referenceNames = new List<string>();
            AssemblyName[] referencedAssemblies =
                pluginAssembly.GetReferencedAssemblies();
            for (int i = 0; i < referencedAssemblies.Length; i++)
            {
                referenceNames.Add(referencedAssemblies[i].Name);
            }

            Check("Assembly-CSharp reference", referenceNames.Contains("Assembly-CSharp"));
            Check("UnityEngine reference", referenceNames.Contains("UnityEngine"));
            Check("UnityEngine.UI reference", referenceNames.Contains("UnityEngine.UI"));
            Check("BepInEx reference", referenceNames.Contains("BepInEx"));
            Check("KKAPI reference", referenceNames.Contains("KKAPI"));
            Check("Extended Save reference", referenceNames.Contains("ExtensibleSaveFormat"));
            Check("no Harmony reference", !referenceNames.Contains("0Harmony"));
            Check(
                "no hard ExpressionControl reference",
                !referenceNames.Contains("KK_ExpressionControl"));

            Assembly gameAssembly = assemblies["Assembly-CSharp"];
            Type eyeLookType = gameAssembly.GetType("EyeLookCalc", true);
            Type chaFileStatusType = gameAssembly.GetType("ChaFileStatus", true);
            BindingFlags publicInstance =
                BindingFlags.Public | BindingFlags.Instance;
            CheckMethod(eyeLookType, "GetAngleHRate", "GetAngleHRate exists", publicInstance);
            CheckMethod(eyeLookType, "GetAngleVRate", "GetAngleVRate exists", publicInstance);
            Check(
                "base-game highlight state exists",
                chaFileStatusType.GetProperty(
                    "hideEyesHighlight",
                    publicInstance) != null);

            Assembly unityAssembly = assemblies["UnityEngine"];
            Type rendererType = unityAssembly.GetType("UnityEngine.Renderer", true);
            Type skinnedRendererType = unityAssembly.GetType(
                "UnityEngine.SkinnedMeshRenderer",
                true);
            Type gameObjectType = unityAssembly.GetType(
                "UnityEngine.GameObject",
                true);
            Check(
                "Renderer.enabled exists",
                rendererType.GetProperty("enabled", publicInstance) != null);
            CheckMethod(skinnedRendererType, "GetBlendShapeWeight", "GetBlendShapeWeight exists", publicInstance);
            CheckMethod(skinnedRendererType, "SetBlendShapeWeight", "SetBlendShapeWeight exists", publicInstance);
            Check(
                "GameObject.activeInHierarchy exists",
                gameObjectType.GetProperty(
                    "activeInHierarchy",
                    publicInstance) != null);

            Type fbsBaseType = gameAssembly.GetType("FBSBase", true);
            BindingFlags allInstance =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;
            CheckField(fbsBaseType, "openRate", "FBSBase.openRate exists", allInstance);
            CheckField(fbsBaseType, "correctOpenMax", "FBSBase.correctOpenMax exists", allInstance);
            CheckField(fbsBaseType, "dictNowFace", "FBSBase current expression dictionary exists", allInstance);

            Type samplerType = GetPluginType(pluginAssembly, "EyeStateSampler");
            MethodInfo initialize = samplerType.GetMethod(
                "Initialize",
                staticNonPublic);
            object[] initializeArguments = { string.Empty };
            bool initialized = (bool)initialize.Invoke(null, initializeArguments);
            Check("blink accessors initialize", initialized);
            Check(
                "blink accessor error is empty",
                string.IsNullOrEmpty((string)initializeArguments[0]));

            object fbsInstance = Activator.CreateInstance(fbsBaseType);
            Delegate openDelegate = (Delegate)samplerType.GetField(
                "_openRateGetter",
                staticNonPublic).GetValue(null);
            Delegate correctDelegate = (Delegate)samplerType.GetField(
                "_correctOpenMaxGetter",
                staticNonPublic).GetValue(null);
            Check(
                "compiled openRate getter invokes",
                Convert.ToSingle(openDelegate.DynamicInvoke(fbsInstance)) == 0f);
            Check(
                "compiled correctOpenMax getter invokes",
                Convert.ToSingle(correctDelegate.DynamicInvoke(fbsInstance)) == -1f);

            string[] releaseFiles = Directory.GetFiles(
                Path.Combine(projectRoot, @"bin\Release"));
            bool onlyExpectedFiles = true;
            for (int i = 0; i < releaseFiles.Length; i++)
            {
                string fileName = Path.GetFileName(releaseFiles[i]);
                if (fileName != "KK_EyeMotion.dll" &&
                    fileName != "KK_EyeMotion.pdb")
                {
                    onlyExpectedFiles = false;
                    break;
                }
            }

            Check(
                "release output contains only DLL/PDB",
                onlyExpectedFiles);
        }

        private static string ReadSourceTree(string sourceRoot)
        {
            string[] files = Directory.GetFiles(
                sourceRoot,
                "*.cs",
                SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            System.Text.StringBuilder source =
                new System.Text.StringBuilder();
            for (int i = 0; i < files.Length; i++)
            {
                source.AppendLine(File.ReadAllText(files[i]));
            }

            return source.ToString();
        }

        private static Type GetPluginType(Assembly assembly, string name)
        {
            return assembly.GetType(PluginNamespace + name, true);
        }

        private static CustomAttributeData FindAttribute(
            Type type,
            string fullName)
        {
            IList<CustomAttributeData> attributes =
                CustomAttributeData.GetCustomAttributes(type);
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].Constructor.DeclaringType.FullName == fullName)
                {
                    return attributes[i];
                }
            }

            return null;
        }

        private static string GetConstructorArgument(
            CustomAttributeData attribute,
            int index)
        {
            if (attribute == null ||
                attribute.ConstructorArguments.Count <= index)
            {
                return null;
            }

            return attribute.ConstructorArguments[index].Value as string;
        }

        private static int CountAttributesWithFirstArgument(
            Type type,
            string fullName,
            string expectedValue)
        {
            int count = 0;
            IList<CustomAttributeData> attributes =
                CustomAttributeData.GetCustomAttributes(type);
            for (int i = 0; i < attributes.Count; i++)
            {
                CustomAttributeData attribute = attributes[i];
                if (attribute.Constructor.DeclaringType.FullName == fullName &&
                    GetConstructorArgument(attribute, 0) == expectedValue)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountMethods(
            Type type,
            string name,
            BindingFlags flags)
        {
            int count = 0;
            MethodInfo[] methods = type.GetMethods(flags);
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == name)
                {
                    count++;
                }
            }

            return count;
        }

        private static void CheckMethod(
            Type type,
            string methodName,
            string checkName,
            BindingFlags flags)
        {
            Check(checkName, type.GetMethod(methodName, flags) != null);
        }

        private static void CheckField(
            Type type,
            string fieldName,
            string checkName,
            BindingFlags flags)
        {
            Check(checkName, type.GetField(fieldName, flags) != null);
        }

        private static void CheckConstant(
            Type type,
            string fieldName,
            string expectedValue,
            BindingFlags flags)
        {
            FieldInfo field = type.GetField(fieldName, flags);
            Check(
                "default name: " + fieldName,
                field != null &&
                (string)field.GetRawConstantValue() == expectedValue);
        }

        private static void Check(string name, bool condition)
        {
            _checks++;
            if (condition)
            {
                return;
            }

            _failures++;
            Console.Error.WriteLine("FAIL " + name);
        }
    }
}
