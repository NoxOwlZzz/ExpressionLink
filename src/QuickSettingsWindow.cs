using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsWindow
    {
        private const int WindowId = 1162694995;
        private const float PreferredWindowWidth = 500f;
        private const float PreferredWindowHeight = 520f;
        private const float MinimumWindowWidth = 360f;
        private const float MinimumWindowHeight = 300f;
        private const float ScreenMargin = 8f;
        private const float FixedVerticalContentHeight = 126f;
        private const float HeaderHeight = 24f;
        private const float MinimumVisibleHeaderWidth = 120f;
        private const int MotionTab = 0;
        private const int IrisTab = 1;
        private const int ExpressionsTab = 2;
        private const int VisibilityTab = 3;
        private const int LinksTab = 4;

        private static readonly string[] BlendshapeDisplayNames =
        {
            "Highlight 01",
            "Highlight 02",
            "Inner iris",
            "Outer iris",
            "Pupil",
            "Sclera",
            "Normal eyes",
            "Expression 01 (merged)",
            "Expression 02 (merged)",
            "Expression 03 (merged)"
        };

        private static readonly GUILayoutOption[] NoLayoutOptions =
            new GUILayoutOption[0];
        private static readonly GUILayoutOption[] SelectorButtonOptions =
        {
            GUILayout.Width(32f)
        };
        private static readonly GUILayoutOption[] SliderLabelOptions =
        {
            GUILayout.MinWidth(116f),
            GUILayout.MaxWidth(190f)
        };
        private static readonly GUILayoutOption[] SliderOptions =
        {
            GUILayout.MinWidth(64f),
            GUILayout.ExpandWidth(true)
        };
        private static readonly GUILayoutOption[] SliderValueOptions =
        {
            GUILayout.Width(48f)
        };
        private static readonly GUILayoutOption[] VisibilityLabelOptions =
        {
            GUILayout.MinWidth(112f),
            GUILayout.MaxWidth(158f)
        };
        private static readonly GUILayoutOption[] VisibilityStatusOptions =
        {
            GUILayout.MinWidth(70f)
        };
        private static readonly GUILayoutOption[] OriginalButtonOptions =
        {
            GUILayout.MinWidth(58f)
        };
        private static readonly GUILayoutOption[] VisibilityButtonOptions =
        {
            GUILayout.MinWidth(50f)
        };

        private Rect _windowRect = new Rect(
            24f,
            72f,
            PreferredWindowWidth,
            PreferredWindowHeight);
        private Vector2 _scrollPosition;
        private bool _visible;
        private int _selectedTab;

        private bool _enabled;
        private bool _invertX;
        private bool _invertY;
        private bool _smoothingEnabled;
        private float _horizontalCenterOffset;
        private float _positiveXInputLimit;
        private float _negativeXInputLimit;
        private float _positiveXMaxWeight;
        private float _negativeXMaxWeight;
        private float _verticalCenterOffset;
        private float _positiveYInputLimit;
        private float _negativeYInputLimit;
        private float _positiveYMaxWeight;
        private float _negativeYMaxWeight;
        private float _blinkMaxWeight;
        private float _smoothingSpeed;
        private bool _eyeAdjustmentEnabled;
        private float _irisYMaxWeight;
        private float _irisSizeMaxWeight;
        private readonly string[] _eyeAdjustmentBlendshapeNames =
            new string[EyeCustomizationCatalog.ChannelCount];
        private bool _showEyeAdjustmentNames;
        private bool _expressionAutomationEnabled;
        private float _expressionActivationThreshold;
        private readonly string[] _expressionTriggers =
            new string[ExpressionTriggerSyntax.SlotCount];
        private int _expressionTriggerControllerInstanceId;
        private int _expressionTriggerRevision = -1;
        private string _expressionFeedback = string.Empty;
        private float _manualHideBlendshapeWeight;
        private bool _followBaseGameHighlightVisibility;
        private bool _cardPersistenceEnabled;
        private readonly string[] _manualVisibilityBlendshapeNames =
            new string[ManualVisibilityCatalog.BlendshapeCount];
        private readonly string[] _manualVisibilityRendererTargets =
            new string[ManualVisibilityCatalog.RendererCount];
        private int _selectedControllerInstanceId;
        private string _visibilityFeedback = string.Empty;
        private bool _showVisibilityTargetConfiguration;
        private readonly GUI.WindowFunction _drawWindowFunction;
        private readonly ExpressionLinkEditorView _expressionLinkEditor;
        private readonly Action _applyConfigurationValuesAction;
        private readonly string _windowTitle;
        private readonly GUILayoutOption[] _scrollViewOptions =
            new GUILayoutOption[1];
        private float _scrollViewOptionHeight = float.NaN;
        private EyeMotionCharacterController _selectedController;
        private int _liveStatusFrame = -1;
        private int _liveStatusControllerInstanceId;
        private string _selectedControllerLine = "Selected character: none";
        private string _liveStatusLine1 =
            "State: no controller";
        private string _liveStatusLine2 = string.Empty;
        private string _liveStatusLine3 = string.Empty;
        private string _baseHighlightLine = string.Empty;
        private string _cardDataLine = string.Empty;

        internal QuickSettingsWindow()
        {
            _drawWindowFunction = DrawWindow;
            _applyConfigurationValuesAction = ApplyConfigurationValues;
            _expressionLinkEditor = new ExpressionLinkEditorView();
            _windowTitle =
                Plugin.PluginName + " " + Plugin.PluginVersion + " - Quick Settings";
        }

        internal bool Visible
        {
            get { return _visible; }
        }

        internal void Toggle()
        {
            _visible = !_visible;
            if (_visible)
            {
                ReadConfiguration();
                ClampToScreen();
            }
        }

        internal void Draw()
        {
            if (!_visible)
            {
                return;
            }

            ClampToScreen();
            _windowRect = GUI.Window(
                WindowId,
                _windowRect,
                _drawWindowFunction,
                _windowTitle);
            ClampToScreen();
        }

        private void DrawWindow(int id)
        {
            BeginVertical();
            EyeMotionCharacterController selected = DrawControllerSelector();
            DrawTabs();

            _scrollPosition = GUILayout.BeginScrollView(
                _scrollPosition,
                GetScrollViewOptions());

            switch (_selectedTab)
            {
                case IrisTab:
                    DrawIrisSettings(selected);
                    break;
                case ExpressionsTab:
                    DrawExpressionSettings(selected);
                    break;
                case LinksTab:
                    _expressionLinkEditor.Draw(selected);
                    break;
                case VisibilityTab:
                    DrawVisibilitySettings(selected);
                    break;
                default:
                    DrawMotionSettings(selected);
                    break;
            }

            GUILayout.EndScrollView();

            BeginHorizontal();
            if (_selectedTab != LinksTab)
            {
                if (DrawButton("Apply"))
                {
                    ApplyConfiguration();
                }

                if (DrawButton("Reload"))
                {
                    ReadConfiguration();
                }
            }

            if (DrawButton("Diagnostics"))
            {
                Diagnostics.WriteReport();
            }

            if (DrawButton("Close"))
            {
                _visible = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0f, 0f, 10000f, HeaderHeight));
        }

        private void DrawTabs()
        {
            BeginHorizontal();
            DrawTabButton(MotionTab, "Motion");
            DrawTabButton(IrisTab, "Iris");
            DrawTabButton(ExpressionsTab, "Expressions");
            DrawTabButton(VisibilityTab, "Visibility");
            DrawTabButton(LinksTab, "Links");
            GUILayout.EndHorizontal();
        }

        private void DrawTabButton(int tab, string label)
        {
            string text = _selectedTab == tab
                ? "[" + label + "]"
                : label;
            if (DrawButton(text) && _selectedTab != tab)
            {
                _selectedTab = tab;
                _scrollPosition = Vector2.zero;
            }
        }

        private void DrawMotionSettings(
            EyeMotionCharacterController controller)
        {
            EnsureLiveStatusCache(controller);
            DrawLabel(_liveStatusLine1);
            if (controller != null)
            {
                DrawLabel(_liveStatusLine2);
            }

            _enabled = DrawToggle(_enabled, "Plugin enabled");

            GUILayout.Space(6f);
            DrawLabel("Horizontal");
            DrawSlider("Center", ref _horizontalCenterOffset, -1f, 1f, "0.000");
            DrawSlider("X+ limit", ref _positiveXInputLimit, 0.05f, 1f, "0.00");
            DrawSlider("X- limit", ref _negativeXInputLimit, 0.05f, 1f, "0.00");
            DrawSlider("X+ weight", ref _positiveXMaxWeight, 0f, 100f, "0");
            DrawSlider("X- weight", ref _negativeXMaxWeight, 0f, 100f, "0");
            _invertX = DrawToggle(_invertX, "Invert X");
            DrawHorizontalCalibrationButtons();

            GUILayout.Space(6f);
            DrawLabel("Vertical");
            DrawSlider("Center", ref _verticalCenterOffset, -1f, 1f, "0.000");
            DrawSlider("Y+ limit", ref _positiveYInputLimit, 0.05f, 1f, "0.00");
            DrawSlider("Y- limit", ref _negativeYInputLimit, 0.05f, 1f, "0.00");
            DrawSlider("Y+ weight", ref _positiveYMaxWeight, 0f, 100f, "0");
            DrawSlider("Y- weight", ref _negativeYMaxWeight, 0f, 100f, "0");
            _invertY = DrawToggle(_invertY, "Invert Y");

            GUILayout.Space(6f);
            DrawLabel("Blink / smoothing");
            DrawSlider("Blink weight", ref _blinkMaxWeight, 0f, 100f, "0");
            _smoothingEnabled = DrawToggle(_smoothingEnabled, "Smoothing");
            DrawSlider("Speed", ref _smoothingSpeed, 0.01f, 30f, "0.0");
        }

        private void DrawIrisSettings(
            EyeMotionCharacterController controller)
        {
            EnsureLiveStatusCache(controller);
            DrawLabel(_liveStatusLine3);
            _eyeAdjustmentEnabled = DrawToggle(
                _eyeAdjustmentEnabled,
                "Enable IrisY / Size");
            DrawSlider("IrisY weight", ref _irisYMaxWeight, 0f, 100f, "0");
            DrawSlider("Size weight", ref _irisSizeMaxWeight, 0f, 100f, "0");
            _showEyeAdjustmentNames = DrawToggle(
                _showEyeAdjustmentNames,
                "Edit shape names");
            if (!_showEyeAdjustmentNames)
            {
                return;
            }

            for (int i = 0; i < EyeCustomizationCatalog.ChannelCount; i++)
            {
                DrawTextField(
                    EyeCustomizationCatalog.DisplayNames[i],
                    ref _eyeAdjustmentBlendshapeNames[i]);
            }
        }

        private void DrawExpressionSettings(
            EyeMotionCharacterController controller)
        {
            _expressionAutomationEnabled = DrawToggle(
                _expressionAutomationEnabled,
                "Automatic expressions");
            DrawSlider(
                "Threshold",
                ref _expressionActivationThreshold,
                0f,
                1f,
                "0.000");
            DrawAutomaticExpressions(controller);
        }

        private void DrawVisibilitySettings(
            EyeMotionCharacterController controller)
        {
            EnsureLiveStatusCache(controller);
            _followBaseGameHighlightVisibility = DrawToggle(
                _followBaseGameHighlightVisibility,
                "Follow Erase Highlight");
            _cardPersistenceEnabled = DrawToggle(
                _cardPersistenceEnabled,
                "Save with character card");
            if (controller != null)
            {
                DrawLabel(_baseHighlightLine);
                DrawLabel(_cardDataLine);
            }

            DrawSlider(
                "Hidden weight",
                ref _manualHideBlendshapeWeight,
                0f,
                100f,
                "0");
            DrawManualVisibility(controller);

            _showVisibilityTargetConfiguration = DrawToggle(
                _showVisibilityTargetConfiguration,
                "Edit names / targets");
            if (_showVisibilityTargetConfiguration)
            {
                DrawVisibilityTargetConfiguration();
            }
        }

        private EyeMotionCharacterController DrawControllerSelector()
        {
            EyeMotionCharacterController controller = GetSelectedController();
            BeginHorizontal();
            if (GUILayout.Button("<", SelectorButtonOptions))
            {
                SelectController(-1);
                controller = GetSelectedController();
            }

            EnsureLiveStatusCache(controller);
            DrawLabel(_selectedControllerLine);

            if (GUILayout.Button(">", SelectorButtonOptions))
            {
                SelectController(1);
                controller = GetSelectedController();
            }

            GUILayout.EndHorizontal();
            return controller;
        }

        private EyeMotionCharacterController GetSelectedController()
        {
            if (_selectedController != null &&
                _selectedController.GetInstanceID() ==
                    _selectedControllerInstanceId)
            {
                return _selectedController;
            }

            IList<EyeMotionCharacterController> controllers =
                EyeMotionCharacterController.ActiveControllers;
            EyeMotionCharacterController first = null;

            for (int i = 0; i < controllers.Count; i++)
            {
                EyeMotionCharacterController controller = controllers[i];
                if (controller == null)
                {
                    continue;
                }

                if (first == null)
                {
                    first = controller;
                }

                if (controller.GetInstanceID() == _selectedControllerInstanceId)
                {
                    _selectedController = controller;
                    return controller;
                }

                if (controller.CurrentBindingState == BindingState.Bound)
                {
                    if (first.CurrentBindingState != BindingState.Bound)
                    {
                        first = controller;
                    }
                }
            }

            if (first != null)
            {
                if (_selectedControllerInstanceId != first.GetInstanceID())
                {
                    _visibilityFeedback = string.Empty;
                }

                _selectedControllerInstanceId = first.GetInstanceID();
            }

            _selectedController = first;
            return first;
        }

        private void SelectController(int direction)
        {
            if (_expressionLinkEditor.HasUnsavedChanges)
            {
                _expressionLinkEditor.RejectControllerChange();
                _selectedTab = LinksTab;
                _scrollPosition = Vector2.zero;
                return;
            }

            IList<EyeMotionCharacterController> controllers =
                EyeMotionCharacterController.ActiveControllers;
            int count = controllers.Count;
            if (count == 0)
            {
                _selectedControllerInstanceId = 0;
                _selectedController = null;
                return;
            }

            int currentIndex = -1;
            for (int i = 0; i < count; i++)
            {
                EyeMotionCharacterController controller = controllers[i];
                if (controller != null &&
                    controller.GetInstanceID() == _selectedControllerInstanceId)
                {
                    currentIndex = i;
                    break;
                }
            }

            for (int offset = 1; offset <= count; offset++)
            {
                int index = currentIndex + (direction >= 0 ? offset : -offset);
                while (index < 0)
                {
                    index += count;
                }

                index %= count;
                EyeMotionCharacterController controller = controllers[index];
                if (controller != null)
                {
                    _selectedControllerInstanceId = controller.GetInstanceID();
                    _selectedController = controller;
                    _visibilityFeedback = string.Empty;
                    return;
                }
            }
        }

        private void CalibrateHorizontalFromCurrent()
        {
            EyeMotionCharacterController controller = GetSelectedController();
            if (controller == null || !controller.State.LookAvailable)
            {
                return;
            }

            float horizontal = controller.State.FinalHorizontal;
            float limit = Clamp(Mathf.Abs(horizontal), 0.05f, 1f);
            if (horizontal > 0f)
            {
                _positiveXInputLimit = limit;
            }
            else if (horizontal < 0f)
            {
                _negativeXInputLimit = limit;
            }
        }

        private void CalibrateCenterFromCurrent()
        {
            EyeMotionCharacterController controller = GetSelectedController();
            if (controller == null || !controller.State.LookAvailable)
            {
                return;
            }

            EyeState state = controller.State;
            _horizontalCenterOffset = state.HorizontalSourceRaw;
            _horizontalCenterOffset = Clamp(_horizontalCenterOffset, -1f, 1f);
            _verticalCenterOffset = Clamp(state.Vertical, -1f, 1f);
        }

        private void EnsureLiveStatusCache(
            EyeMotionCharacterController controller)
        {
            int controllerInstanceId = controller == null
                ? 0
                : controller.GetInstanceID();
            int frame = Time.frameCount;
            if (_liveStatusFrame == frame &&
                _liveStatusControllerInstanceId == controllerInstanceId)
            {
                return;
            }

            _liveStatusFrame = frame;
            _liveStatusControllerInstanceId = controllerInstanceId;
            if (controller == null)
            {
                _selectedControllerLine = "Selected character: none";
                _liveStatusLine1 = "State: no controller";
                _liveStatusLine2 = string.Empty;
                _liveStatusLine3 = "ExpressionControl: no character";
                _baseHighlightLine = string.Empty;
                _cardDataLine = string.Empty;
                return;
            }

            string characterName = controller.GetCharacterName();
            EyeState state = controller.State;
            MappedWeights weights = controller.AppliedWeights;
            _selectedControllerLine =
                "Selected character: " + characterName;
            _liveStatusLine1 = string.Format(
                "State: {0} | X/Y: {1:F3} / {2:F3} | Raw X: {3:F3}",
                controller.CurrentBindingState,
                state.FinalHorizontal,
                state.FinalVertical,
                state.HorizontalSourceRaw);
            _liveStatusLine2 = string.Format(
                "Weights: X+ {0:F1} X- {1:F1} | Y+ {2:F1} Y- {3:F1} | B {4:F1}",
                weights.PositiveX,
                weights.NegativeX,
                weights.PositiveY,
                weights.NegativeY,
                weights.Blink);
            BlendshapeBinding binding = controller.Binding;
            if (binding == null)
            {
                _liveStatusLine3 = "ExpressionControl: not bound";
            }
            else
            {
                _liveStatusLine3 =
                    "ExpressionControl: " +
                    binding.EyeAdjustmentSourceStatus +
                    " | Shapes " + binding.EyeCustomizationShapeCount +
                    "/" + EyeCustomizationCatalog.ChannelCount +
                    string.Format(
                        " | IrisY {0:F3} Size {1:F3}",
                        binding.LastIrisY,
                        binding.LastIrisSize);
            }
            _baseHighlightLine =
                "Base highlight: " +
                (controller.BaseGameHighlightHidden ? "erased" : "visible") +
                " | Sync override: " +
                (controller.HighlightSyncEffective ? "active" : "inactive");
            _cardDataLine =
                "Card data: " + controller.CardPersistenceStatus;
        }

        private static void DrawSlider(
            string label,
            ref float value,
            float minimum,
            float maximum,
            string format)
        {
            BeginHorizontal();
            GUILayout.Label(
                label,
                SliderLabelOptions);
            value = GUILayout.HorizontalSlider(
                value,
                minimum,
                maximum,
                SliderOptions);
            GUILayout.Label(
                value.ToString(format),
                SliderValueOptions);
            GUILayout.EndHorizontal();
        }

        private void ReadConfiguration()
        {
            _enabled = PluginConfig.Enabled.Value;
            _invertX = PluginConfig.InvertX.Value;
            _invertY = PluginConfig.InvertY.Value;
            _smoothingEnabled = PluginConfig.SmoothingEnabled.Value;
            _horizontalCenterOffset = PluginConfig.HorizontalCenterOffset.Value;
            _positiveXInputLimit = PluginConfig.PositiveXInputLimit.Value;
            _negativeXInputLimit = PluginConfig.NegativeXInputLimit.Value;
            _positiveXMaxWeight = PluginConfig.PositiveXMaxWeight.Value;
            _negativeXMaxWeight = PluginConfig.NegativeXMaxWeight.Value;
            _verticalCenterOffset = PluginConfig.VerticalCenterOffset.Value;
            _positiveYInputLimit = PluginConfig.PositiveYInputLimit.Value;
            _negativeYInputLimit = PluginConfig.NegativeYInputLimit.Value;
            _positiveYMaxWeight = PluginConfig.PositiveYMaxWeight.Value;
            _negativeYMaxWeight = PluginConfig.NegativeYMaxWeight.Value;
            _blinkMaxWeight = PluginConfig.BlinkMaxWeight.Value;
            _smoothingSpeed = PluginConfig.SmoothingSpeed.Value;
            _eyeAdjustmentEnabled =
                PluginConfig.EyeAdjustmentEnabled.Value;
            _irisYMaxWeight = PluginConfig.IrisYMaxWeight.Value;
            _irisSizeMaxWeight = PluginConfig.IrisSizeMaxWeight.Value;
            _expressionAutomationEnabled =
                PluginConfig.ExpressionAutomationEnabled.Value;
            _expressionActivationThreshold =
                PluginConfig.ExpressionActivationThreshold.Value;
            for (int i = 0;
                i < EyeCustomizationCatalog.ChannelCount;
                i++)
            {
                _eyeAdjustmentBlendshapeNames[i] =
                    PluginConfig.GetEyeAdjustmentBlendshapeName(i);
            }
            _manualHideBlendshapeWeight =
                PluginConfig.ManualHideBlendshapeWeight.Value;
            _followBaseGameHighlightVisibility =
                PluginConfig.FollowBaseGameHighlightVisibility.Value;
            _cardPersistenceEnabled =
                PluginConfig.CardPersistenceEnabled.Value;
            for (int i = 0;
                i < ManualVisibilityCatalog.BlendshapeCount;
                i++)
            {
                _manualVisibilityBlendshapeNames[i] =
                    PluginConfig.GetManualVisibilityBlendshapeName(i);
            }

            for (int i = 0;
                i < ManualVisibilityCatalog.RendererCount;
                i++)
            {
                _manualVisibilityRendererTargets[i] =
                    PluginConfig.GetManualVisibilityRendererTarget(i);
            }
        }

        private void ApplyConfiguration()
        {
            PluginConfig.ApplyBatch(_applyConfigurationValuesAction);
            ReadConfiguration();
        }

        private void ApplyConfigurationValues()
        {
            PluginConfig.Enabled.Value = _enabled;
            PluginConfig.InvertX.Value = _invertX;
            PluginConfig.InvertY.Value = _invertY;
            PluginConfig.SmoothingEnabled.Value = _smoothingEnabled;
            PluginConfig.HorizontalCenterOffset.Value = Clamp(_horizontalCenterOffset, -1f, 1f);
            PluginConfig.PositiveXInputLimit.Value = Clamp(_positiveXInputLimit, 0.05f, 1f);
            PluginConfig.NegativeXInputLimit.Value = Clamp(_negativeXInputLimit, 0.05f, 1f);
            PluginConfig.PositiveXMaxWeight.Value = Clamp(_positiveXMaxWeight, 0f, 100f);
            PluginConfig.NegativeXMaxWeight.Value = Clamp(_negativeXMaxWeight, 0f, 100f);
            PluginConfig.VerticalCenterOffset.Value = Clamp(_verticalCenterOffset, -1f, 1f);
            PluginConfig.PositiveYInputLimit.Value = Clamp(_positiveYInputLimit, 0.05f, 1f);
            PluginConfig.NegativeYInputLimit.Value = Clamp(_negativeYInputLimit, 0.05f, 1f);
            PluginConfig.PositiveYMaxWeight.Value = Clamp(_positiveYMaxWeight, 0f, 100f);
            PluginConfig.NegativeYMaxWeight.Value = Clamp(_negativeYMaxWeight, 0f, 100f);
            PluginConfig.BlinkMaxWeight.Value = Clamp(_blinkMaxWeight, 0f, 100f);
            PluginConfig.SmoothingSpeed.Value = Clamp(_smoothingSpeed, 0.01f, 30f);
            PluginConfig.EyeAdjustmentEnabled.Value =
                _eyeAdjustmentEnabled;
            PluginConfig.IrisYMaxWeight.Value =
                Clamp(_irisYMaxWeight, 0f, 100f);
            PluginConfig.IrisSizeMaxWeight.Value =
                Clamp(_irisSizeMaxWeight, 0f, 100f);
            PluginConfig.ExpressionAutomationEnabled.Value =
                _expressionAutomationEnabled;
            PluginConfig.ExpressionActivationThreshold.Value =
                Clamp(_expressionActivationThreshold, 0f, 1f);
            for (int i = 0;
                i < EyeCustomizationCatalog.ChannelCount;
                i++)
            {
                PluginConfig.EyeAdjustmentBlendshapeNames[i].Value =
                    (_eyeAdjustmentBlendshapeNames[i] ?? string.Empty).Trim();
            }
            PluginConfig.ManualHideBlendshapeWeight.Value =
                Clamp(_manualHideBlendshapeWeight, 0f, 100f);
            PluginConfig.FollowBaseGameHighlightVisibility.Value =
                _followBaseGameHighlightVisibility;
            PluginConfig.CardPersistenceEnabled.Value =
                _cardPersistenceEnabled;
            for (int i = 0;
                i < ManualVisibilityCatalog.BlendshapeCount;
                i++)
            {
                PluginConfig.ManualVisibilityBlendshapeNames[i].Value =
                    (_manualVisibilityBlendshapeNames[i] ?? string.Empty).Trim();
            }

            for (int i = 0;
                i < ManualVisibilityCatalog.RendererCount;
                i++)
            {
                PluginConfig.ManualVisibilityRendererTargets[i].Value =
                    (_manualVisibilityRendererTargets[i] ?? string.Empty).Trim();
            }

        }

        private void DrawManualVisibility(
            EyeMotionCharacterController controller)
        {
            if (controller == null)
            {
                DrawLabel("No character.");
                return;
            }

            ManualVisibilityBinding binding = controller.ManualVisibility;
            if (binding == null)
            {
                DrawLabel("Visibility not bound.");
                return;
            }

            DrawLabel("Fused parts");
            for (int i = 0; i < binding.BlendshapeSlots.Length; i++)
            {
                BlendshapeVisibilitySlot slot = binding.BlendshapeSlots[i];
                DrawVisibilityRow(
                    controller,
                    false,
                    i,
                    GetBlendshapeDisplayName(i, slot.DisplayName),
                    slot.ManualMode,
                    GetResolutionText(slot.Resolution));
            }

            GUILayout.Space(4f);
            DrawLabel("Expression meshes");
            for (int i = 0; i < binding.RendererSlots.Length; i++)
            {
                RendererVisibilitySlot slot = binding.RendererSlots[i];
                string status = GetResolutionText(slot.Resolution);
                if (slot.Resolution == VisibilityResolutionStatus.Ready &&
                    !slot.IsValid())
                {
                    status = "Invalid";
                }
                else if (slot.Resolution == VisibilityResolutionStatus.Ready &&
                    slot.Renderer != null &&
                    !slot.Renderer.gameObject.activeInHierarchy)
                {
                    status = "Ready/inactive";
                }

                DrawVisibilityRow(
                    controller,
                    true,
                    i,
                    slot.DisplayName,
                    slot.ManualMode,
                    status);
            }

            BeginHorizontal();
            if (DrawButton("Restore"))
            {
                if (controller.RestoreManualVisibility(out _visibilityFeedback) &&
                    _visibilityFeedback.Length == 0)
                {
                    _visibilityFeedback = "Original visibility restored.";
                }
            }

            if (DrawButton("Re-detect"))
            {
                controller.RefreshManualVisibility(out _visibilityFeedback);
            }

            GUILayout.EndHorizontal();
            if (_visibilityFeedback.Length > 0)
            {
                DrawLabel(_visibilityFeedback);
            }
        }

        private void DrawAutomaticExpressions(
            EyeMotionCharacterController controller)
        {
            if (controller == null)
            {
                DrawLabel("No character.");
                return;
            }

            EnsureExpressionTriggerBuffer(controller);
            for (int i = 0; i < _expressionTriggers.Length; i++)
            {
                DrawTextField(
                    "ExpressionMesh " + (i + 1).ToString("00"),
                    ref _expressionTriggers[i]);
                DrawLabel("  " + controller.GetExpressionTriggerStatus(i));
                BeginHorizontal();
                DrawLabel("Capture:");
                if (DrawButton("Brow"))
                {
                    CaptureCurrentExpression(
                        controller,
                        i,
                        ExpressionTriggerPart.Brow);
                }

                if (DrawButton("Eyes"))
                {
                    CaptureCurrentExpression(
                        controller,
                        i,
                        ExpressionTriggerPart.Eyes);
                }

                if (DrawButton("Mouth"))
                {
                    CaptureCurrentExpression(
                        controller,
                        i,
                        ExpressionTriggerPart.Mouth);
                }

                GUILayout.EndHorizontal();
            }

            if (DrawButton("Apply triggers"))
            {
                controller.SetExpressionTriggers(
                    _expressionTriggers,
                    out _expressionFeedback);
                _expressionTriggerRevision =
                    controller.ExpressionTriggerRevision;
            }

            if (_expressionFeedback.Length > 0)
            {
                DrawLabel(_expressionFeedback);
            }
        }

        private void EnsureExpressionTriggerBuffer(
            EyeMotionCharacterController controller)
        {
            int instanceId = controller.GetInstanceID();
            int revision = controller.ExpressionTriggerRevision;
            if (_expressionTriggerControllerInstanceId == instanceId &&
                _expressionTriggerRevision == revision)
            {
                return;
            }

            _expressionTriggerControllerInstanceId = instanceId;
            _expressionTriggerRevision = revision;
            _expressionFeedback = string.Empty;
            for (int i = 0; i < _expressionTriggers.Length; i++)
            {
                _expressionTriggers[i] = controller.GetExpressionTrigger(i);
            }
        }

        private void CaptureCurrentExpression(
            EyeMotionCharacterController controller,
            int slotIndex,
            ExpressionTriggerPart part)
        {
            string selector = controller.GetCurrentExpressionSelector(part);
            if (selector.Length == 0)
            {
                _expressionFeedback =
                    "No current " + part.ToString().ToLowerInvariant() +
                    " pattern is available.";
                return;
            }

            _expressionTriggers[slotIndex] = selector;
            _expressionFeedback =
                "Captured " + selector + " for " +
                (slotIndex + 1).ToString("00") +
                ". Press Apply triggers.";
        }

        private void DrawVisibilityRow(
            EyeMotionCharacterController controller,
            bool rendererSlot,
            int slotIndex,
            string label,
            ManualVisibilityMode mode,
            string status)
        {
            if (UseCompactLayout())
            {
                BeginVertical();
                DrawLabel(label + " - " + status);
                BeginHorizontal();
                DrawVisibilityButtons(controller, rendererSlot, slotIndex, mode);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }

            BeginHorizontal();
            GUILayout.Label(
                label,
                VisibilityLabelOptions);
            DrawVisibilityButtons(controller, rendererSlot, slotIndex, mode);
            GUILayout.Label(status, VisibilityStatusOptions);
            GUILayout.EndHorizontal();
        }

        private void DrawVisibilityButtons(
            EyeMotionCharacterController controller,
            bool rendererSlot,
            int slotIndex,
            ManualVisibilityMode mode)
        {
            if (GUILayout.Button(
                mode == ManualVisibilityMode.Original ? "[Original]" : "Original",
                OriginalButtonOptions))
            {
                SetVisibilityMode(
                    controller,
                    rendererSlot,
                    slotIndex,
                    ManualVisibilityMode.Original);
            }

            if (GUILayout.Button(
                mode == ManualVisibilityMode.Visible ? "[Show]" : "Show",
                VisibilityButtonOptions))
            {
                SetVisibilityMode(
                    controller,
                    rendererSlot,
                    slotIndex,
                    ManualVisibilityMode.Visible);
            }

            if (GUILayout.Button(
                mode == ManualVisibilityMode.Hidden ? "[Hide]" : "Hide",
                VisibilityButtonOptions))
            {
                SetVisibilityMode(
                    controller,
                    rendererSlot,
                    slotIndex,
                    ManualVisibilityMode.Hidden);
            }
        }

        private void SetVisibilityMode(
            EyeMotionCharacterController controller,
            bool rendererSlot,
            int slotIndex,
            ManualVisibilityMode mode)
        {
            if (rendererSlot)
            {
                controller.SetManualRendererVisibility(
                    slotIndex,
                    mode,
                    out _visibilityFeedback);
            }
            else
            {
                controller.SetManualBlendshapeVisibility(
                    slotIndex,
                    mode,
                    Clamp(_manualHideBlendshapeWeight, 0f, 100f),
                    out _visibilityFeedback);
            }
        }

        private void DrawVisibilityTargetConfiguration()
        {
            DrawLabel("Hide shape names");
            for (int i = 0;
                i < ManualVisibilityCatalog.BlendshapeCount;
                i++)
            {
                DrawTextField(
                    GetBlendshapeDisplayName(
                        i,
                        ManualVisibilityCatalog.BlendshapeDisplayNames[i]),
                    ref _manualVisibilityBlendshapeNames[i]);
            }

            DrawLabel("Expression mesh targets");
            for (int i = 0;
                i < ManualVisibilityCatalog.RendererCount;
                i++)
            {
                DrawTextField(
                    ManualVisibilityCatalog.RendererDisplayNames[i],
                    ref _manualVisibilityRendererTargets[i]);
            }
        }

        private static void DrawTextField(string label, ref string value)
        {
            BeginHorizontal();
            GUILayout.Label(
                label,
                SliderLabelOptions);
            value = DrawTextField(value ?? string.Empty);
            GUILayout.EndHorizontal();
        }

        private void ClampToScreen()
        {
            ClampWindowSize();
            ClampHeaderToScreen();
        }

        private void ClampWindowSize()
        {
            float availableWidth = Mathf.Max(
                1f,
                Screen.width - (ScreenMargin * 2f));
            float availableHeight = Mathf.Max(
                1f,
                Screen.height - (ScreenMargin * 2f));
            float minimumWidth = Mathf.Min(
                MinimumWindowWidth,
                availableWidth);
            float minimumHeight = Mathf.Min(
                MinimumWindowHeight,
                availableHeight);

            _windowRect.width = Mathf.Clamp(
                PreferredWindowWidth,
                minimumWidth,
                availableWidth);
            _windowRect.height = Mathf.Clamp(
                PreferredWindowHeight,
                minimumHeight,
                availableHeight);
        }

        private void ClampHeaderToScreen()
        {
            _windowRect.x = QuickSettingsWindowPlacement.ClampHorizontal(
                _windowRect.x,
                _windowRect.width,
                Screen.width,
                ScreenMargin,
                MinimumVisibleHeaderWidth);
            _windowRect.y = QuickSettingsWindowPlacement.ClampVertical(
                _windowRect.y,
                Screen.height,
                ScreenMargin,
                HeaderHeight);
        }

        private float GetScrollViewHeight()
        {
            return Mathf.Max(80f, _windowRect.height - FixedVerticalContentHeight);
        }

        private GUILayoutOption[] GetScrollViewOptions()
        {
            float height = GetScrollViewHeight();
            if (_scrollViewOptionHeight != height)
            {
                _scrollViewOptionHeight = height;
                _scrollViewOptions[0] = GUILayout.Height(height);
            }

            return _scrollViewOptions;
        }

        private bool UseCompactLayout()
        {
            return _windowRect.width < 520f;
        }

        private void DrawHorizontalCalibrationButtons()
        {
            if (UseCompactLayout())
            {
                BeginHorizontal();
                DrawCenterAndCurrentButtons();
                GUILayout.EndHorizontal();
                BeginHorizontal();
                DrawSensitivityPresetButtons();
                GUILayout.EndHorizontal();
                return;
            }

            BeginHorizontal();
            DrawCenterAndCurrentButtons();
            DrawSensitivityPresetButtons();
            GUILayout.EndHorizontal();
        }

        private void DrawCenterAndCurrentButtons()
        {
            if (DrawButton("Center X/Y"))
            {
                CalibrateCenterFromCurrent();
            }

            if (DrawButton("Use current X"))
            {
                CalibrateHorizontalFromCurrent();
            }
        }

        private void DrawSensitivityPresetButtons()
        {
            if (DrawButton("High sensitivity 0.15"))
            {
                _positiveXInputLimit = 0.15f;
                _negativeXInputLimit = 0.15f;
            }

            if (DrawButton("Default X 1.00"))
            {
                _positiveXInputLimit = 1f;
                _negativeXInputLimit = 1f;
            }
        }

        private static string GetBlendshapeDisplayName(
            int slotIndex,
            string fallback)
        {
            if (slotIndex < 0 || slotIndex >= BlendshapeDisplayNames.Length)
            {
                return fallback;
            }

            return BlendshapeDisplayNames[slotIndex];
        }

        private static string GetResolutionText(
            VisibilityResolutionStatus status)
        {
            switch (status)
            {
                case VisibilityResolutionStatus.Ready:
                    return "Ready";
                case VisibilityResolutionStatus.Disabled:
                    return "Disabled";
                case VisibilityResolutionStatus.Missing:
                    return "Missing";
                case VisibilityResolutionStatus.Ambiguous:
                    return "Ambiguous";
                case VisibilityResolutionStatus.Conflict:
                    return "Conflict";
                case VisibilityResolutionStatus.Invalid:
                    return "Invalid";
                default:
                    return "Unknown";
            }
        }

        private static void BeginHorizontal()
        {
            GUILayout.BeginHorizontal(NoLayoutOptions);
        }

        private static void BeginVertical()
        {
            GUILayout.BeginVertical(NoLayoutOptions);
        }

        private static void DrawLabel(string text)
        {
            GUILayout.Label(text, NoLayoutOptions);
        }

        private static bool DrawButton(string text)
        {
            return GUILayout.Button(text, NoLayoutOptions);
        }

        private static bool DrawToggle(bool value, string text)
        {
            return GUILayout.Toggle(value, text, NoLayoutOptions);
        }

        private static string DrawTextField(string text)
        {
            return GUILayout.TextField(text, NoLayoutOptions);
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }

    }
}
