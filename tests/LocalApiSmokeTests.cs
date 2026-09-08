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
            ResolveEventHandler resolver = null;

            try
            {
                string projectRoot = TestGameTarget.FindProjectRoot();
                resolver = TestGameTarget.CreateReferenceResolver(projectRoot);
                AppDomain.CurrentDomain.AssemblyResolve += resolver;
                RunChecks(projectRoot);
            }
            catch (Exception exception)
            {
                _failures++;
                Console.Error.WriteLine(
                    "FAIL local API smoke suite: " +
                    exception.GetBaseException().Message);
            }

            finally
            {
                if (resolver != null)
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= resolver;
                }
            }

            checks = _checks;
            return _failures;
        }

        private static void RunChecks(string projectRoot)
        {
            string pluginSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\Plugin.cs"));
            string controllerSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\EyeMotionCharacterController.cs"));
            string quickSettingsSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\QuickSettingsCoordinator.cs"));
            string windowFrameSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\QuickSettingsWindowFrame.cs"));
            string movableWindowSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\QuickSettingsMovableWindow.cs"));
            string visualFactorySource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\QuickSettingsWindowVisualFactory.cs"));
            string surfaceSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\QuickSettings\QuickSettingsSurfaceView.cs"));
            string navigationSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\QuickSettings\QuickSettingsNavigationView.cs"));
            string styleResourcesSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\Styling\QuickSettingsGuiResources.cs"));
            string styleFactorySource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\Styling\QuickSettingsStyleFactory.cs"));
            string textureCatalogSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\Styling\QuickSettingsTextureCatalog.cs"));
            string roundedTextureSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\Styling\QuickSettingsRoundedTextureFactory.cs"));
            string chromeResourcesSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\Styling\QuickSettingsWindowChromeResources.cs"));
            string expressionLayoutSource = File.ReadAllText(
                Path.Combine(projectRoot, @"src\UI\ExpressionLinks\ExpressionLinkGUILayout.cs"));
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
            Check(
                "quick-settings uses a semantic theme",
                sourceTree.IndexOf(
                    "internal static class QuickSettingsTheme",
                    StringComparison.Ordinal) >= 0);
            Check(
                "quick-settings rounded textures are generated once",
                roundedTextureSource.IndexOf(
                    "private const int SamplesPerAxis = 4",
                    StringComparison.Ordinal) >= 0 &&
                roundedTextureSource.IndexOf(
                    "FilterMode.Bilinear",
                    StringComparison.Ordinal) >= 0 &&
                roundedTextureSource.IndexOf(
                    "texture.Apply(false, true)",
                    StringComparison.Ordinal) >= 0);
            Check(
                "quick-settings catalog owns rounded assets",
                textureCatalogSource.IndexOf(
                    "CreateRounded(",
                    StringComparison.Ordinal) >= 0 &&
                textureCatalogSource.IndexOf(
                    "InputFocused",
                    StringComparison.Ordinal) >= 0);
            Check(
                "quick-settings styles use nine-slice borders",
                styleFactorySource.IndexOf(
                    "CreateBorder(borderSlice)",
                    StringComparison.Ordinal) >= 0);
            Check(
                "quick-settings header uses a sliced rounded sprite",
                visualFactorySource.IndexOf(
                    "Image.Type.Sliced",
                    StringComparison.Ordinal) >= 0 &&
                chromeResourcesSource.IndexOf(
                    "Sprite.Create(",
                    StringComparison.Ordinal) >= 0);
            Check(
                "quick-settings does not replace the global GUI skin",
                sourceTree.IndexOf(
                    "GUI.skin =",
                    StringComparison.Ordinal) < 0);
            Check(
                "quick-settings tabs use selected styles",
                navigationSource.IndexOf(
                    "TabButton(label, selected)",
                    StringComparison.Ordinal) >= 0 &&
                navigationSource.IndexOf(
                    "FormatTab(",
                    StringComparison.Ordinal) < 0);
            Check(
                "quick-settings styles are cached by skin",
                styleResourcesSource.IndexOf(
                    "_skin == skin",
                    StringComparison.Ordinal) >= 0);
            Check(
                "quick-settings style resources are disposed",
                quickSettingsSource.IndexOf(
                    "QuickSettingsGui.DisposeResources();",
                    StringComparison.Ordinal) >= 0);
            Check(
                "quick-settings body uses remaining height",
                surfaceSource.IndexOf(
                    "ExpandHeightOptions",
                    StringComparison.Ordinal) >= 0 &&
                surfaceSource.IndexOf(
                    "FixedVerticalContentHeight",
                    StringComparison.Ordinal) < 0);
            Check(
                "expression links share quick-settings styles",
                expressionLayoutSource.IndexOf(
                    "QuickSettingsGui.BeginPropertyRow();",
                    StringComparison.Ordinal) >= 0);
            Check(
                "styled header has its own accent",
                visualFactorySource.IndexOf(
                    "QuickSettingsHeaderAccent",
                    StringComparison.Ordinal) >= 0);
            Check(
                "visibility selection no longer uses brackets",
                sourceTree.IndexOf(
                    "\"[Game default]\"",
                    StringComparison.Ordinal) < 0 &&
                sourceTree.IndexOf(
                    "\"[Show]\"",
                    StringComparison.Ordinal) < 0 &&
                sourceTree.IndexOf(
                    "\"[Hide]\"",
                    StringComparison.Ordinal) < 0);
            string referenceDirectory = TestGameTarget.ReferenceDirectory;
            string[] loadOrder =
            {
                Path.Combine(referenceDirectory,
                    TestGameTarget.UnityRuntimeAssemblyName + ".dll"),
                Path.Combine(referenceDirectory, "UnityEngine.UI.dll"),
                Path.Combine(referenceDirectory, "Assembly-CSharp.dll"),
                Path.Combine(referenceDirectory, "BepInEx.dll"),
                Path.Combine(referenceDirectory,
                    TestGameTarget.ExtendedSaveAssemblyName + ".dll"),
                Path.Combine(referenceDirectory,
                    TestGameTarget.ApiAssemblyName + ".dll"),
                Path.Combine(TestGameTarget.ReleaseDirectory,
                    TestGameTarget.PluginAssemblyName + ".dll")
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

            Assembly pluginAssembly = assemblies[TestGameTarget.PluginAssemblyName];
            Check(
                "assembly name matches target",
                pluginAssembly.GetName().Name == TestGameTarget.PluginAssemblyName);
            Check(
                "assembly CLR matches target framework",
                pluginAssembly.ImageRuntimeVersion.StartsWith(
                    TestGameTarget.ClrVersionPrefix, StringComparison.Ordinal));
            Check(
                "assembly version",
                pluginAssembly.GetName().Version.ToString() == "0.5.1.0");

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
                GetConstructorArgument(pluginAttribute, 2) == "0.5.1");
            Check(
                "Extended Save dependency attribute",
                CountAttributesWithFirstArgument(
                    pluginType,
                    "BepInEx.BepInDependency",
                    "com.bepis.bepinex.extendedsave") == 1);

            Check(
                "target main-game process",
                CountAttributesWithFirstArgument(
                    pluginType, "BepInEx.BepInProcess", TestGameTarget.MainProcess) == 1);
            Check(
                "Studio process",
                CountAttributesWithFirstArgument(
                    pluginType, "BepInEx.BepInProcess", "CharaStudio.exe") == 1);
            Check(
                "other main-game process excluded",
                CountAttributesWithFirstArgument(
                    pluginType, "BepInEx.BepInProcess",
                    TestGameTarget.OtherMainProcess) == 0);
            Type compatibilityType = GetPluginType(
                pluginAssembly, "GameCompatibility");
            CheckConstant(
                compatibilityType, "GameId", TestGameTarget.GameId,
                BindingFlags.Static | BindingFlags.NonPublic);
            CheckConstant(
                compatibilityType, "MainProcess", TestGameTarget.MainProcess,
                BindingFlags.Static | BindingFlags.NonPublic);
            int profileChecks;
            _failures += ExpressionLinkProfilePolicyTests.Run(
                pluginAssembly, out profileChecks);
            _checks += profileChecks;

            BindingFlags instanceNonPublic =
                BindingFlags.Instance | BindingFlags.NonPublic;
            Check(
                "plugin polling focus callback removed",
                pluginType.GetMethod(
                    "OnApplicationFocus", instanceNonPublic) == null);
            FieldInfo studioToolbarButtonField = pluginType.GetField(
                "_studioToolbarButton",
                instanceNonPublic);
            Check(
                "Studio toolbar button handle retained",
                studioToolbarButtonField != null &&
                typeof(IDisposable).IsAssignableFrom(
                    studioToolbarButtonField.FieldType));
            int toolbarDisposeIndex = pluginSource.IndexOf(
                "_studioToolbarButton.Dispose();",
                StringComparison.Ordinal);
            int toolbarIconDestroyIndex = pluginSource.IndexOf(
                "UnityEngine.Object.Destroy(_studioToolbarIcon);",
                StringComparison.Ordinal);
            Check(
                "Studio toolbar button disposed before icon",
                toolbarDisposeIndex >= 0 &&
                toolbarIconDestroyIndex > toolbarDisposeIndex);
            Check(
                "Studio toolbar button registration handle stored",
                pluginSource.IndexOf(
                    "_studioToolbarButton =",
                    StringComparison.Ordinal) >= 0 &&
                pluginSource.IndexOf(
                    "CustomToolbarButtons.AddLeftToolbarButton(",
                    StringComparison.Ordinal) >= 0);
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
            Check(
                "quick-settings semantic theme type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsTheme") != null);
            Check(
                "quick-settings style resource type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsGuiResources") != null);
            Check(
                "quick-settings texture owner type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsTextureCatalog") != null);
            Check(
                "quick-settings style factory type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsStyleFactory") != null);
            Check(
                "quick-settings rounded texture factory type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsRoundedTextureFactory") != null);
            Check(
                "quick-settings window chrome resource type",
                GetPluginType(
                    pluginAssembly,
                    "QuickSettingsWindowChromeResources") != null);
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
            Check(
                "legacy ExpressionMesh settings view removed",
                pluginAssembly.GetType(
                    PluginNamespace + "ExpressionSettingsView",
                    false) == null);
            Check(
                "legacy expression workspace removed",
                pluginAssembly.GetType(
                    PluginNamespace + "ExpressionWorkspaceView",
                    false) == null);
            Type linkEditorType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkEditorView");
            Check("expression-link editor type", linkEditorType != null);
            Check(
                "new expression links remain drafts until saved",
                linkEditorType.GetField(
                    "_isNewDraft", instanceNonPublic) != null);
            MethodInfo ensureLinkSelectionMethod = linkEditorType.GetMethod(
                "EnsureSelection",
                instanceNonPublic);
            Check(
                "expression-link selection can block stale refresh",
                ensureLinkSelectionMethod != null &&
                ensureLinkSelectionMethod.ReturnType == typeof(bool));
            Type linkDraftPanelType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkDraftPanel");
            Check("expression-link draft panel type", linkDraftPanelType != null);
            Check(
                "link draft reports edits from current draw",
                linkDraftPanelType.GetProperty(
                    "ChangedDuringLastDraw", instanceNonPublic) != null);
            Type draftDefinitionType = GetPluginType(
                pluginAssembly,
                "ExpressionLinkDefinition");
            object defaultDraft = draftDefinitionType.GetMethod(
                "CreateDefault",
                BindingFlags.Static | BindingFlags.NonPublic).Invoke(
                    null,
                    null);
            object draftPanelInstance = Activator.CreateInstance(
                linkDraftPanelType,
                true);
            linkDraftPanelType.GetMethod(
                "Load",
                instanceNonPublic).Invoke(
                    draftPanelInstance,
                    new object[] { defaultDraft });
            object[] incompleteCandidateArguments =
            {
                null,
                string.Empty
            };
            bool incompleteDraftAccepted = Convert.ToBoolean(
                linkDraftPanelType.GetMethod(
                    "TryBuildCandidate",
                    instanceNonPublic).Invoke(
                        draftPanelInstance,
                        incompleteCandidateArguments));
            Check(
                "enabled incomplete expression link rejected",
                !incompleteDraftAccepted &&
                ((string)incompleteCandidateArguments[1]).IndexOf(
                    "Step 1",
                    StringComparison.Ordinal) >= 0);
            Type targetScopeType = GetPluginType(
                pluginAssembly,
                "ExpressionTargetScope");
            MethodInfo scopeUsesSlotsMethod = linkDraftPanelType.GetMethod(
                "ScopeUsesSlots",
                BindingFlags.Static | BindingFlags.NonPublic);
            Check(
                "Any target scope preserves slot filters",
                scopeUsesSlotsMethod != null &&
                Convert.ToBoolean(
                    scopeUsesSlotsMethod.Invoke(
                        null,
                        new object[]
                        {
                            Enum.Parse(targetScopeType, "Any")
                        })));
            Check(
                "Head target scope does not expose slot filters",
                !Convert.ToBoolean(
                    scopeUsesSlotsMethod.Invoke(
                        null,
                        new object[]
                        {
                            Enum.Parse(targetScopeType, "Head")
                        })));
            Check(
                "quick-settings owns per-character expression-link editor",
                quickSettingsSurfaceType.GetField(
                    "_expressionLinkEditor",
                    instanceNonPublic) != null);
            Check(
                "expression source section type",
                GetPluginType(
                    pluginAssembly,
                    "ExpressionLinkSourceSectionView") != null);
            Check(
                "expression target section type",
                GetPluginType(
                    pluginAssembly,
                    "ExpressionLinkTargetSectionView") != null);
            Check(
                "expression response section type",
                GetPluginType(
                    pluginAssembly,
                    "ExpressionLinkResponseSectionView") != null);
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
                "profile replacement bound to one character revision",
                linkProfilePanelType.GetField(
                    "_confirmControllerInstanceId",
                    instanceNonPublic) != null &&
                linkProfilePanelType.GetField(
                    "_confirmControllerRevision",
                    instanceNonPublic) != null);
            Check(
                "profile replacement confirmation can be cancelled",
                linkProfilePanelType.GetMethod(
                    "CancelPendingReplace",
                    instanceNonPublic) != null);
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
            CheckMethod(
                linkRuntimeType,
                "ManagesTarget",
                "expression-link target ownership query",
                instanceNonPublic);
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
            MethodInfo beginRuntimeMethod = controllerType.GetMethod(
                "BeginRuntime",
                staticNonPublic);
            MethodInfo shutdownRuntimeMethod = controllerType.GetMethod(
                "ShutdownAndRestoreAll",
                staticNonPublic);
            PropertyInfo runtimeShuttingDownProperty =
                controllerType.GetProperty(
                    "RuntimeShuttingDown",
                    staticNonPublic);
            Check(
                "controller runtime shutdown API",
                beginRuntimeMethod != null &&
                shutdownRuntimeMethod != null &&
                runtimeShuttingDownProperty != null);
            if (beginRuntimeMethod != null &&
                shutdownRuntimeMethod != null &&
                runtimeShuttingDownProperty != null)
            {
                beginRuntimeMethod.Invoke(null, null);
                Check(
                    "controller runtime begins active",
                    !Convert.ToBoolean(
                        runtimeShuttingDownProperty.GetValue(null, null)));
                shutdownRuntimeMethod.Invoke(null, null);
                shutdownRuntimeMethod.Invoke(null, null);
                Check(
                    "controller shutdown is idempotent",
                    Convert.ToBoolean(
                        runtimeShuttingDownProperty.GetValue(null, null)));
                beginRuntimeMethod.Invoke(null, null);
            }
            int lateUpdateIndex = controllerSource.IndexOf(
                "private void LateUpdate()",
                StringComparison.Ordinal);
            int lateUpdateShutdownGateIndex = controllerSource.IndexOf(
                "if (_runtimeShuttingDown)",
                lateUpdateIndex,
                StringComparison.Ordinal);
            int bindingRevisionCheckIndex = controllerSource.IndexOf(
                "_observedBindingRevision != PluginConfig.BindingRevision",
                lateUpdateIndex,
                StringComparison.Ordinal);
            Check(
                "shutdown gate precedes LateUpdate work",
                lateUpdateIndex >= 0 &&
                lateUpdateShutdownGateIndex > lateUpdateIndex &&
                bindingRevisionCheckIndex > lateUpdateShutdownGateIndex);
            Check(
                "binding coroutine observes shutdown gate",
                controllerSource.IndexOf(
                    "if (_runtimeShuttingDown ||",
                    StringComparison.Ordinal) >= 0);
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
            Type blendshapeVisibilitySlotType = GetPluginType(
                pluginAssembly,
                "BlendshapeVisibilitySlot");
            CheckField(
                blendshapeVisibilitySlotType,
                "SuppressedByExpressionLink",
                "legacy fused target suppression state",
                instanceNonPublic);
            Check(
                "manual blendshape commands",
                CountMethods(
                    visibilityBindingType,
                    "SetBlendshapeMode",
                    instanceNonPublic) == 2);
            CheckMethod(visibilityBindingType, "SetRendererMode", "manual renderer command", instanceNonPublic);
            CheckMethod(visibilityBindingType, "RestoreAll", "manual visibility restoration", instanceNonPublic);
            CheckMethod(visibilityBindingType, "SetAutomaticHighlightHidden", "automatic base-highlight synchronization", instanceNonPublic);
            Check(
                "automatic expression visibility overloads",
                CountMethods(
                    visibilityBindingType,
                    "SetAutomaticExpressionState",
                    instanceNonPublic) == 2);
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
            CheckMethod(controllerType, "RefreshManualVisibility", "controller manual visibility refresh", instanceNonPublic);

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
                "seven user-facing manual hide slots",
                Convert.ToInt32(
                    catalogType.GetField(
                        "UserFacingBlendshapeCount",
                        staticNonPublic).GetRawConstantValue()) == 7);
            Check(
                "legacy fused slots start after user-facing slots",
                Convert.ToInt32(
                    catalogType.GetField(
                        "LegacyFusedBlendshapeStartIndex",
                        staticNonPublic).GetRawConstantValue()) == 7);
            Check(
                "three legacy fused slots remain compatible",
                Convert.ToInt32(
                    catalogType.GetField(
                        "LegacyFusedBlendshapeCount",
                        staticNonPublic).GetRawConstantValue()) == 3);
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
            AssemblyName runtimeReference = null;
            for (int i = 0; i < referencedAssemblies.Length; i++)
            {
                referenceNames.Add(referencedAssemblies[i].Name);
                if (referencedAssemblies[i].Name == "mscorlib")
                {
                    runtimeReference = referencedAssemblies[i];
                }
            }

            Check("Assembly-CSharp reference", referenceNames.Contains("Assembly-CSharp"));
            Check("Unity runtime reference",
                referenceNames.Contains(TestGameTarget.UnityRuntimeAssemblyName));
            Check("UnityEngine.UI reference", referenceNames.Contains("UnityEngine.UI"));
            Check("BepInEx reference", referenceNames.Contains("BepInEx"));
            Check("game API reference",
                referenceNames.Contains(TestGameTarget.ApiAssemblyName));
            Check("other game API excluded",
                !referenceNames.Contains(TestGameTarget.OtherApiAssemblyName));
            Check("other plugin target excluded",
                !referenceNames.Contains(TestGameTarget.OtherPluginAssemblyName));
            Check("Extended Save reference",
                referenceNames.Contains(TestGameTarget.ExtendedSaveAssemblyName));
            Check(
                "mscorlib matches target framework",
                runtimeReference != null &&
                runtimeReference.Version.Major == TestGameTarget.MscorlibMajorVersion);
            Check("no Harmony reference", !referenceNames.Contains("0Harmony"));
            Check(
                "no hard ExpressionControl reference",
                !referenceNames.Contains("KK_ExpressionControl") &&
                !referenceNames.Contains("KKS_ExpressionControl"));

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

            Assembly unityAssembly =
                assemblies[TestGameTarget.UnityRuntimeAssemblyName];
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
                Path.Combine(projectRoot, TestGameTarget.ReleaseDirectory));
            bool onlyExpectedFiles = true;
            for (int i = 0; i < releaseFiles.Length; i++)
            {
                string fileName = Path.GetFileName(releaseFiles[i]);
                if (fileName != TestGameTarget.PluginAssemblyName + ".dll" &&
                    fileName != TestGameTarget.PluginAssemblyName + ".pdb")
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
