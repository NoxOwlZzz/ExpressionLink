using System;
using System.Collections;
using System.Collections.Generic;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    [DefaultExecutionOrder(32000)]
    public sealed class EyeMotionCharacterController : CharaCustomFunctionController
    {
        private static readonly List<EyeMotionCharacterController> Controllers =
            new List<EyeMotionCharacterController>();
        private static readonly IList<EyeMotionCharacterController> ControllersView =
            Controllers.AsReadOnly();

        private BlendshapeBinding _binding;
        private ManualVisibilityBinding _manualVisibility;
        private ExpressionTriggerController _expressionTriggers;
        private ResolveResult _lastResolve;
        private EyeState _eyeState;
        private MappedWeights _targetWeights;
        private MappedWeights _appliedWeights;

        private BindingState _bindingState = BindingState.Unbound;
        private string _statusMessage = "Not initialized.";
        private int _bindingGeneration;
        private int _observedBindingRevision;
        private int _observedVisibilityDefinitionRevision;
        private int _observedVisibilityRevision;
        private int _lastAppliedFrame = -1;
        private bool _smoothingInitialized;
        private bool _blinkWasApplied;
        private float _nextEyeLogTime;
        private readonly ManualVisibilityMode[] _savedBlendshapeModes =
            new ManualVisibilityMode[ManualVisibilityCatalog.BlendshapeCount];
        private readonly ManualVisibilityMode[] _savedRendererModes =
            new ManualVisibilityMode[ManualVisibilityCatalog.RendererCount];
        private readonly string[] _savedExpressionTriggers =
            new string[ExpressionTriggerSyntax.SlotCount];
        private readonly bool[] _appliedExpressionConfigured =
            new bool[ExpressionTriggerSyntax.SlotCount];
        private readonly bool[] _appliedExpressionActive =
            new bool[ExpressionTriggerSyntax.SlotCount];
        private bool _hasAppliedExpressionAutomation;
        private bool _appliedExpressionAutomationEnabled;
        private bool _hasObservedHighlightSync;
        private bool _highlightSyncEffective;
        private bool _baseGameHighlightHidden;
        private string _cardPersistenceStatus = "No card data loaded.";
        private bool _preserveUnsupportedCardData;
        private bool _visibilityIntentChanged;
        private int _expressionTriggerRevision;
        private int _expressionRetryAttemptsRemaining;
        private int _nextExpressionRetryFrame;

        internal static IList<EyeMotionCharacterController> ActiveControllers
        {
            get { return ControllersView; }
        }

        internal BlendshapeBinding Binding
        {
            get { return _binding; }
        }

        internal ResolveResult LastResolve
        {
            get { return _lastResolve; }
        }

        internal ManualVisibilityBinding ManualVisibility
        {
            get { return _manualVisibility; }
        }

        internal EyeState State
        {
            get { return _eyeState; }
        }

        internal MappedWeights AppliedWeights
        {
            get { return _appliedWeights; }
        }

        internal BindingState CurrentBindingState
        {
            get { return _bindingState; }
        }

        internal string StatusMessage
        {
            get { return _statusMessage; }
        }

        internal bool BaseGameHighlightHidden
        {
            get { return _baseGameHighlightHidden; }
        }

        internal bool HighlightSyncEffective
        {
            get { return _highlightSyncEffective; }
        }

        internal string CardPersistenceStatus
        {
            get { return _cardPersistenceStatus; }
        }

        internal ExpressionTriggerController ExpressionTriggers
        {
            get { return _expressionTriggers; }
        }

        internal int ExpressionTriggerRevision
        {
            get { return _expressionTriggerRevision; }
        }

        internal static void RestoreAll()
        {
            for (int i = 0; i < Controllers.Count; i++)
            {
                EyeMotionCharacterController controller = Controllers[i];
                if (controller != null)
                {
                    controller.StopAndRestore("Plugin shutdown.");
                }
            }
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            RegisterInstance();
            if (!maintainState)
            {
                LoadCardVisibilityState();
            }

            _observedBindingRevision = PluginConfig.BindingRevision;
            _observedVisibilityDefinitionRevision =
                PluginConfig.VisibilityDefinitionRevision;
            _observedVisibilityRevision = PluginConfig.VisibilityRevision;
            _eyeState.Reset();
            _targetWeights.Reset();
            _appliedWeights.Reset();
            _lastAppliedFrame = -1;
            _nextEyeLogTime = 0f;
            _smoothingInitialized = false;
            _blinkWasApplied = false;
            _lastResolve = null;

            RestoreAndClear("Character reload.");

            if (PluginConfig.Enabled.Value)
            {
                StartBinding("Character reload.");
            }
            else
            {
                _bindingState = BindingState.Disabled;
                _statusMessage = "Disabled by configuration.";
            }

            base.OnReload(currentGameMode, maintainState);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (!PluginConfig.CardPersistenceEnabled.Value)
            {
                SetExtendedData(null);
                _cardPersistenceStatus = "Card persistence is disabled.";
                return;
            }

            if (_preserveUnsupportedCardData && !_visibilityIntentChanged)
            {
                _cardPersistenceStatus =
                    "A newer card-data version was preserved unchanged.";
                return;
            }

            try
            {
                byte[] modes = VisibilityCardData.Encode(
                    _savedBlendshapeModes,
                    _savedRendererModes);
                bool hasExpressionTriggers =
                    VisibilityCardData.HasExpressionTriggers(
                        _savedExpressionTriggers);
                if (VisibilityCardData.IsDefault(modes) &&
                    !hasExpressionTriggers)
                {
                    SetExtendedData(null);
                    _cardPersistenceStatus =
                        "Manual modes and expression triggers are empty; no card payload was needed.";
                    _preserveUnsupportedCardData = false;
                    _visibilityIntentChanged = false;
                    return;
                }

                PluginData data = new PluginData();
                data.version = VisibilityCardData.SchemaVersion;
                data.data[VisibilityCardData.ModesKey] = modes;
                for (int i = 0;
                    i < _savedExpressionTriggers.Length;
                    i++)
                {
                    string trigger = _savedExpressionTriggers[i];
                    if (!string.IsNullOrEmpty(trigger))
                    {
                        data.data[
                            VisibilityCardData.GetExpressionTriggerKey(i)] =
                            trigger;
                    }
                }
                SetExtendedData(data);
                _cardPersistenceStatus =
                    "Manual visibility and expression triggers saved to the character card.";
                _preserveUnsupportedCardData = false;
                _visibilityIntentChanged = false;
            }
            catch (Exception exception)
            {
                _cardPersistenceStatus =
                    "Card save failed: " + exception.GetType().Name + ": " +
                    exception.Message;
                Plugin.Log.LogWarning(
                    "EyeMotion could not save card data for " +
                    GetCharacterName() + ": " + _cardPersistenceStatus);
            }
        }

        protected override void OnDestroy()
        {
            _bindingGeneration++;
            RestoreAndClear("Character controller destroyed.");
            Controllers.Remove(this);
            base.OnDestroy();
        }

        private void LateUpdate()
        {
            if (_observedBindingRevision != PluginConfig.BindingRevision)
            {
                _observedBindingRevision = PluginConfig.BindingRevision;
                _observedVisibilityDefinitionRevision =
                    PluginConfig.VisibilityDefinitionRevision;
                _observedVisibilityRevision = PluginConfig.VisibilityRevision;
                if (PluginConfig.Enabled.Value)
                {
                    RestoreAndClear("Binding configuration changed.");
                    StartBinding("Binding configuration changed.");
                }
                else
                {
                    StopAndRestore("Disabled by configuration.");
                }

                return;
            }

            if (!PluginConfig.Enabled.Value)
            {
                if (_bindingState != BindingState.Disabled || _binding != null)
                {
                    StopAndRestore("Disabled by configuration.");
                }

                return;
            }

            if (_bindingState == BindingState.Disabled)
            {
                StartBinding("Re-enabled by configuration.");
                return;
            }

            if (_binding == null)
            {
                return;
            }

            if (!_binding.IsValid())
            {
                RestoreAndClear("Renderer or sharedMesh changed.");
                StartBinding("Renderer or sharedMesh changed.");
                return;
            }

            if (_observedVisibilityDefinitionRevision !=
                PluginConfig.VisibilityDefinitionRevision)
            {
                _observedVisibilityDefinitionRevision =
                    PluginConfig.VisibilityDefinitionRevision;
                string refreshMessage;
                RefreshManualVisibility(out refreshMessage);
                DebugLog(
                    GetCharacterName() +
                    ": refreshed manual visibility definitions. " +
                    refreshMessage);
            }

            if (_observedVisibilityRevision != PluginConfig.VisibilityRevision)
            {
                _observedVisibilityRevision = PluginConfig.VisibilityRevision;
                if (_manualVisibility != null)
                {
                    string visibilityMessage;
                    if (!_manualVisibility.ReapplyBlendshapeOverrides(
                        out visibilityMessage) &&
                        visibilityMessage.Length > 0)
                    {
                        DebugLog(GetCharacterName() + ": " + visibilityMessage);
                    }
                }
            }

            RetryExpressionResolutionIfNeeded();
            SynchronizeBaseGameHighlightVisibility();
            SynchronizeAutomaticExpressions();

            int frame = Time.frameCount;
            if (_lastAppliedFrame == frame)
            {
                return;
            }

            EyeStateSampler.Sample(
                ChaControl,
                frame,
                _binding.ManagesBlink,
                ref _eyeState);
            MapTargets(ref _eyeState, ref _targetWeights);
            bool applyBlink =
                _binding.ManagesBlink && _eyeState.BlinkAvailable;
            ApplySmoothing(
                ref _targetWeights,
                ref _appliedWeights,
                applyBlink);

            string error;
            if (_binding.ManagesBlink && !applyBlink && _blinkWasApplied)
            {
                if (!_binding.RestoreBlink(out error))
                {
                    RestoreAndClear("Blink restoration failed: " + error);
                    StartBinding("Blink restoration failed.");
                    return;
                }

                _blinkWasApplied = false;
            }

            if (!_binding.ApplyAlreadyValidated(
                ref _appliedWeights,
                applyBlink,
                out error))
            {
                RestoreAndClear("Blendshape application failed: " + error);
                StartBinding("Blendshape application failed.");
                return;
            }

            if (!_binding.ApplyEyeCustomizationAlreadyValidated(
                ChaControl,
                frame,
                out error))
            {
                RestoreAndClear(
                    "ExpressionControl eye adjustment failed: " + error);
                StartBinding("ExpressionControl eye adjustment failed.");
                return;
            }

            if (applyBlink)
            {
                _blinkWasApplied = true;
            }

            _lastAppliedFrame = frame;
            _eyeState.LastApplicationFrame = frame;
            LogEyeValuesIfDue();
        }

        private void StartBinding(string reason)
        {
            _bindingGeneration++;
            int generation = _bindingGeneration;
            _bindingState = BindingState.Searching;
            _statusMessage = reason;
            StartCoroutine(BindRoutine(generation));
        }

        private IEnumerator BindRoutine(int generation)
        {
            int attempts = PluginConfig.BindingRetryFrames.Value;
            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                if (generation != _bindingGeneration || !PluginConfig.Enabled.Value)
                {
                    yield break;
                }

                ResolveResult result = HeadmodTargetResolver.Resolve(ChaControl);
                _lastResolve = result;
                _statusMessage = result.Message;

                if (result.Status == ResolveStatus.Bound)
                {
                    _binding = result.Binding;
                    _observedVisibilityDefinitionRevision =
                        PluginConfig.VisibilityDefinitionRevision;
                    _observedVisibilityRevision =
                        PluginConfig.VisibilityRevision;
                    try
                    {
                        _manualVisibility = ManualVisibilityBinding.Resolve(
                            ChaControl,
                            _binding);
                        ApplySavedVisibilityModes();
                        RebuildExpressionTriggers();
                        _hasObservedHighlightSync = false;
                        SynchronizeBaseGameHighlightVisibility();
                        SynchronizeAutomaticExpressions();
                    }
                    catch (Exception exception)
                    {
                        _manualVisibility = null;
                        Plugin.Log.LogWarning(
                            "EyeMotion manual visibility is unavailable for " +
                            GetCharacterName() + ": " +
                            exception.GetType().Name + ": " + exception.Message);
                    }

                    _bindingState = BindingState.Bound;
                    _smoothingInitialized = false;
                    _blinkWasApplied = false;

                    DebugLog(
                        "Bound " + GetCharacterName() + " to " + _binding.RelativePath +
                        " (renderer " + _binding.RendererInstanceId +
                        ", mesh " + _binding.MeshInstanceId + "). " + result.Message);
                    yield break;
                }

                if (result.Status == ResolveStatus.Ambiguous)
                {
                    _bindingState = BindingState.Ambiguous;
                    Plugin.Log.LogWarning(
                        "EyeMotion disabled for " + GetCharacterName() + ": " + result.Message +
                        " Candidates: " + BuildAmbiguousPathList(result));
                    yield break;
                }

                if (attempt < attempts)
                {
                    yield return null;
                }
            }

            if (generation == _bindingGeneration)
            {
                _bindingState = BindingState.Incompatible;
                _statusMessage = "No compatible renderer found after " + attempts + " attempts.";
                DebugLog(GetCharacterName() + ": " + _statusMessage);
            }
        }

        private void StopAndRestore(string reason)
        {
            _bindingGeneration++;
            RestoreAndClear(reason);
            _bindingState = BindingState.Disabled;
            _statusMessage = reason;
        }

        private void RestoreAndClear(string reason)
        {
            if (_manualVisibility != null)
            {
                string visibilityError;
                if (!_manualVisibility.RestoreAll(out visibilityError) &&
                    visibilityError.Length > 0)
                {
                    DebugLog(GetCharacterName() + ": " + visibilityError);
                }
            }

            if (_binding != null)
            {
                string error;
                if (!_binding.Restore(PluginConfig.RestoreMode.Value, out error) && error.Length > 0)
                {
                    DebugLog(GetCharacterName() + ": " + error);
                }
            }

            _manualVisibility = null;
            _expressionTriggers = null;
            _binding = null;
            _lastResolve = null;
            _bindingState = BindingState.Unbound;
            _statusMessage = reason;
            _lastAppliedFrame = -1;
            _smoothingInitialized = false;
            _blinkWasApplied = false;
            _hasObservedHighlightSync = false;
            _highlightSyncEffective = false;
            _baseGameHighlightHidden = false;
            _hasAppliedExpressionAutomation = false;
            _appliedExpressionAutomationEnabled = false;
            for (int i = 0; i < _appliedExpressionConfigured.Length; i++)
            {
                _appliedExpressionConfigured[i] = false;
                _appliedExpressionActive[i] = false;
            }
            _targetWeights.Reset();
            _appliedWeights.Reset();
        }

        internal bool SetManualBlendshapeVisibility(
            int slotIndex,
            ManualVisibilityMode mode,
            out string message)
        {
            if (_manualVisibility == null)
            {
                message = "Manual visibility is not bound for this character.";
                return false;
            }

            bool success = _manualVisibility.SetBlendshapeMode(
                slotIndex,
                mode,
                out message);
            RememberBlendshapeMode(slotIndex, mode);
            return success;
        }

        internal bool SetManualBlendshapeVisibility(
            int slotIndex,
            ManualVisibilityMode mode,
            float hiddenWeight,
            out string message)
        {
            if (_manualVisibility == null)
            {
                message = "Manual visibility is not bound for this character.";
                return false;
            }

            bool success = _manualVisibility.SetBlendshapeMode(
                slotIndex,
                mode,
                hiddenWeight,
                out message);
            RememberBlendshapeMode(slotIndex, mode);
            return success;
        }

        internal bool SetManualRendererVisibility(
            int slotIndex,
            ManualVisibilityMode mode,
            out string message)
        {
            if (_manualVisibility == null)
            {
                message = "Manual visibility is not bound for this character.";
                return false;
            }

            bool success = _manualVisibility.SetRendererMode(
                slotIndex,
                mode,
                out message);
            RememberRendererMode(slotIndex, mode);
            return success;
        }

        internal bool RestoreManualVisibility(out string message)
        {
            if (_manualVisibility == null)
            {
                message = "Manual visibility is not bound for this character.";
                return false;
            }

            bool success = _manualVisibility.RestoreAll(out message);
            ResetSavedVisibilityModes();
            _visibilityIntentChanged = true;
            _hasObservedHighlightSync = false;
            SynchronizeBaseGameHighlightVisibility();
            _hasAppliedExpressionAutomation = false;
            SynchronizeAutomaticExpressions();
            return success;
        }

        internal bool RefreshManualVisibility(out string message)
        {
            message = string.Empty;
            if (_binding == null || !_binding.IsValid())
            {
                message = "The eye renderer is not bound.";
                return false;
            }

            if (_manualVisibility != null)
            {
                string restoreMessage;
                if (!_manualVisibility.RestoreAll(out restoreMessage) &&
                    restoreMessage.Length > 0)
                {
                    message = restoreMessage;
                }
            }

            try
            {
                _manualVisibility = ManualVisibilityBinding.Resolve(ChaControl, _binding);
                ApplySavedVisibilityModes();
                RebuildExpressionTriggers();
                _hasObservedHighlightSync = false;
                SynchronizeBaseGameHighlightVisibility();
                SynchronizeAutomaticExpressions();
                string summary = _manualVisibility.GetSummary();
                message = message.Length == 0
                    ? summary
                    : message + " | " + summary;
                return true;
            }
            catch (Exception exception)
            {
                _manualVisibility = null;
                message = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private void SynchronizeBaseGameHighlightVisibility()
        {
            if (_manualVisibility == null || ChaControl == null)
            {
                return;
            }

            bool baseHidden = ChaControl.fileStatus != null &&
                              ChaControl.fileStatus.hideEyesHighlight;
            _baseGameHighlightHidden = baseHidden;
            bool effective =
                PluginConfig.FollowBaseGameHighlightVisibility.Value &&
                baseHidden;
            if (_hasObservedHighlightSync &&
                effective == _highlightSyncEffective)
            {
                return;
            }

            _hasObservedHighlightSync = true;
            _highlightSyncEffective = effective;
            string message;
            if (!_manualVisibility.SetAutomaticHighlightHidden(
                effective,
                out message) &&
                !string.IsNullOrEmpty(message))
            {
                DebugLog(GetCharacterName() + ": highlight sync: " + message);
            }
        }

        private void RebuildExpressionTriggers()
        {
            RebuildExpressionTriggers(true);
        }

        private void RebuildExpressionTriggers(bool resetRetryWindow)
        {
            try
            {
                _expressionTriggers = ExpressionTriggerController.Resolve(
                    ChaControl,
                    _savedExpressionTriggers);
            }
            catch (Exception exception)
            {
                _expressionTriggers = null;
                DebugLog(
                    GetCharacterName() +
                    ": expression trigger resolution failed: " +
                    exception.GetType().Name + ": " + exception.Message);
            }

            _hasAppliedExpressionAutomation = false;
            if (resetRetryWindow)
            {
                _expressionRetryAttemptsRemaining = 10;
                _nextExpressionRetryFrame = Time.frameCount + 15;
            }
        }

        private void RetryExpressionResolutionIfNeeded()
        {
            if (_expressionRetryAttemptsRemaining <= 0 ||
                Time.frameCount < _nextExpressionRetryFrame ||
                _expressionTriggers == null)
            {
                return;
            }

            bool unresolved = false;
            for (int i = 0; i < _savedExpressionTriggers.Length; i++)
            {
                if (!string.IsNullOrEmpty(_savedExpressionTriggers[i]) &&
                    _expressionTriggers.GetStatus(i) !=
                        ExpressionTriggerResolutionStatus.Ready)
                {
                    unresolved = true;
                    break;
                }
            }

            if (!unresolved)
            {
                _expressionRetryAttemptsRemaining = 0;
                return;
            }

            _expressionRetryAttemptsRemaining--;
            _nextExpressionRetryFrame = Time.frameCount + 15;
            RebuildExpressionTriggers(false);
        }

        private void SynchronizeAutomaticExpressions()
        {
            if (_manualVisibility == null || _expressionTriggers == null)
            {
                return;
            }

            bool enabled = PluginConfig.ExpressionAutomationEnabled.Value;
            bool sampledChange = enabled &&
                _expressionTriggers.HasReadySlot &&
                _expressionTriggers.Sample(
                    PluginConfig.ExpressionActivationThreshold.Value);
            bool force = !_hasAppliedExpressionAutomation ||
                         enabled != _appliedExpressionAutomationEnabled;

            for (int i = 0; i < ExpressionTriggerSyntax.SlotCount; i++)
            {
                bool configured = enabled &&
                    _expressionTriggers.GetStatus(i) ==
                        ExpressionTriggerResolutionStatus.Ready;
                bool active = configured && _expressionTriggers.IsActive(i);
                if (!force && !sampledChange &&
                    configured == _appliedExpressionConfigured[i] &&
                    active == _appliedExpressionActive[i])
                {
                    continue;
                }

                string message;
                if (!_manualVisibility.SetAutomaticExpressionState(
                    i,
                    configured,
                    active,
                    out message) &&
                    !string.IsNullOrEmpty(message))
                {
                    DebugLog(
                        GetCharacterName() +
                        ": expression slot " + (i + 1) + ": " + message);
                }

                _appliedExpressionConfigured[i] = configured;
                _appliedExpressionActive[i] = active;
            }

            _hasAppliedExpressionAutomation = true;
            _appliedExpressionAutomationEnabled = enabled;
        }

        internal string GetExpressionTrigger(int slotIndex)
        {
            return slotIndex >= 0 &&
                   slotIndex < _savedExpressionTriggers.Length
                ? _savedExpressionTriggers[slotIndex]
                : string.Empty;
        }

        internal bool SetExpressionTrigger(
            int slotIndex,
            string trigger,
            out string message)
        {
            if (slotIndex < 0 ||
                slotIndex >= _savedExpressionTriggers.Length)
            {
                message = "Invalid expression trigger slot.";
                return false;
            }

            string normalized = trigger == null
                ? string.Empty
                : trigger.Trim();
            _savedExpressionTriggers[slotIndex] = normalized;
            _expressionTriggerRevision++;
            _visibilityIntentChanged = true;
            _preserveUnsupportedCardData = false;
            RebuildExpressionTriggers();
            SynchronizeAutomaticExpressions();

            if (_expressionTriggers == null)
            {
                message = "Expression detection is unavailable.";
                return false;
            }

            message = _expressionTriggers.GetMessage(slotIndex);
            return _expressionTriggers.GetStatus(slotIndex) ==
                       ExpressionTriggerResolutionStatus.Ready ||
                   _expressionTriggers.GetStatus(slotIndex) ==
                       ExpressionTriggerResolutionStatus.Disabled;
        }

        internal bool SetExpressionTriggers(
            string[] triggers,
            out string message)
        {
            if (triggers == null ||
                triggers.Length != _savedExpressionTriggers.Length)
            {
                message = "Exactly four expression triggers are required.";
                return false;
            }

            for (int i = 0; i < _savedExpressionTriggers.Length; i++)
            {
                _savedExpressionTriggers[i] = triggers[i] == null
                    ? string.Empty
                    : triggers[i].Trim();
            }

            _expressionTriggerRevision++;
            _visibilityIntentChanged = true;
            _preserveUnsupportedCardData = false;
            RebuildExpressionTriggers();
            SynchronizeAutomaticExpressions();
            if (_expressionTriggers == null)
            {
                message = "Expression detection is unavailable.";
                return false;
            }

            bool valid = true;
            message = string.Empty;
            for (int i = 0; i < _savedExpressionTriggers.Length; i++)
            {
                ExpressionTriggerResolutionStatus status =
                    _expressionTriggers.GetStatus(i);
                if (status != ExpressionTriggerResolutionStatus.Ready &&
                    status != ExpressionTriggerResolutionStatus.Disabled)
                {
                    valid = false;
                }

                if (message.Length > 0)
                {
                    message += " | ";
                }

                message += "Expression " + (i + 1) + ": " + status;
            }

            return valid;
        }

        internal string GetExpressionTriggerStatus(int slotIndex)
        {
            if (_expressionTriggers == null)
            {
                return "Unavailable";
            }

            ExpressionTriggerResolutionStatus status =
                _expressionTriggers.GetStatus(slotIndex);
            string selector = status == ExpressionTriggerResolutionStatus.Ready
                ? " -> " + _expressionTriggers.GetSelector(slotIndex)
                : string.Empty;
            string active = status == ExpressionTriggerResolutionStatus.Ready
                ? (_expressionTriggers.IsActive(slotIndex)
                    ? " | active"
                    : " | inactive")
                : string.Empty;
            return status + selector + active;
        }

        internal string GetExpressionTriggerMessage(int slotIndex)
        {
            return _expressionTriggers == null
                ? "Expression detection is unavailable."
                : _expressionTriggers.GetMessage(slotIndex);
        }

        internal string GetCurrentExpressionSelector(
            ExpressionTriggerPart part)
        {
            if (_expressionTriggers == null)
            {
                return string.Empty;
            }

            float weight = _expressionTriggers.GetCurrentWeight(part);
            if (weight <= PluginConfig.ExpressionActivationThreshold.Value ||
                float.IsNaN(weight))
            {
                return string.Empty;
            }

            ExpressionTriggerSelector selector =
                _expressionTriggers.GetCurrentSelector(part);
            return selector.IsValid
                ? ExpressionTriggerSyntax.FormatSelector(selector)
                : string.Empty;
        }

        private void LoadCardVisibilityState()
        {
            ResetSavedVisibilityModes();
            ResetSavedExpressionTriggers();
            _expressionTriggerRevision++;
            _preserveUnsupportedCardData = false;
            _visibilityIntentChanged = false;
            if (!PluginConfig.CardPersistenceEnabled.Value)
            {
                _cardPersistenceStatus = "Card persistence is disabled.";
                return;
            }

            try
            {
                PluginData data = GetExtendedData();
                if (data == null || data.data == null)
                {
                    _cardPersistenceStatus = "No EyeMotion data on this card.";
                    return;
                }

                if (data.version != VisibilityCardData.LegacySchemaVersion &&
                    data.version != VisibilityCardData.SchemaVersion)
                {
                    _preserveUnsupportedCardData =
                        data.version > VisibilityCardData.SchemaVersion;
                    _cardPersistenceStatus =
                        "Unsupported EyeMotion card-data version " +
                        data.version + ".";
                    return;
                }

                object encoded;
                if (data.data.TryGetValue(
                    VisibilityCardData.ModesKey,
                    out encoded))
                {
                    string error;
                    if (!VisibilityCardData.TryDecode(
                        data.version,
                        encoded,
                        _savedBlendshapeModes,
                        _savedRendererModes,
                        out error))
                    {
                        ResetSavedVisibilityModes();
                        ResetSavedExpressionTriggers();
                        _cardPersistenceStatus = error;
                        Plugin.Log.LogWarning(
                            "EyeMotion ignored invalid card data for " +
                            GetCharacterName() + ": " + error);
                        return;
                    }
                }

                if (data.version >= VisibilityCardData.SchemaVersion)
                {
                    for (int i = 0;
                        i < _savedExpressionTriggers.Length;
                        i++)
                    {
                        object triggerValue;
                        if (data.data.TryGetValue(
                            VisibilityCardData.GetExpressionTriggerKey(i),
                            out triggerValue))
                        {
                            _savedExpressionTriggers[i] =
                                VisibilityCardData.NormalizeExpressionTrigger(
                                    triggerValue);
                        }
                    }
                }

                _cardPersistenceStatus =
                    "Manual visibility and expression triggers loaded from the character card.";
            }
            catch (Exception exception)
            {
                ResetSavedVisibilityModes();
                ResetSavedExpressionTriggers();
                _cardPersistenceStatus =
                    "Card load failed: " + exception.GetType().Name + ": " +
                    exception.Message;
                Plugin.Log.LogWarning(
                    "EyeMotion could not load card data for " +
                    GetCharacterName() + ": " + _cardPersistenceStatus);
            }
        }

        private void ApplySavedVisibilityModes()
        {
            if (_manualVisibility == null)
            {
                return;
            }

            string ignored;
            for (int i = 0; i < _savedBlendshapeModes.Length; i++)
            {
                ManualVisibilityMode mode = _savedBlendshapeModes[i];
                if (mode != ManualVisibilityMode.Original)
                {
                    _manualVisibility.SetBlendshapeMode(i, mode, out ignored);
                }
            }

            for (int i = 0; i < _savedRendererModes.Length; i++)
            {
                ManualVisibilityMode mode = _savedRendererModes[i];
                if (mode != ManualVisibilityMode.Original)
                {
                    _manualVisibility.SetRendererMode(i, mode, out ignored);
                }
            }
        }

        private void RememberBlendshapeMode(
            int slotIndex,
            ManualVisibilityMode mode)
        {
            if (slotIndex >= 0 && slotIndex < _savedBlendshapeModes.Length &&
                _manualVisibility != null &&
                slotIndex < _manualVisibility.BlendshapeSlots.Length &&
                _manualVisibility.BlendshapeSlots[slotIndex].Resolution ==
                    VisibilityResolutionStatus.Ready)
            {
                _savedBlendshapeModes[slotIndex] = mode;
                _visibilityIntentChanged = true;
            }
        }

        private void RememberRendererMode(
            int slotIndex,
            ManualVisibilityMode mode)
        {
            if (slotIndex >= 0 && slotIndex < _savedRendererModes.Length &&
                _manualVisibility != null &&
                slotIndex < _manualVisibility.RendererSlots.Length &&
                _manualVisibility.RendererSlots[slotIndex].Resolution ==
                    VisibilityResolutionStatus.Ready)
            {
                _savedRendererModes[slotIndex] = mode;
                _visibilityIntentChanged = true;
            }
        }

        private void ResetSavedVisibilityModes()
        {
            for (int i = 0; i < _savedBlendshapeModes.Length; i++)
            {
                _savedBlendshapeModes[i] = ManualVisibilityMode.Original;
            }

            for (int i = 0; i < _savedRendererModes.Length; i++)
            {
                _savedRendererModes[i] = ManualVisibilityMode.Original;
            }
        }

        private void ResetSavedExpressionTriggers()
        {
            for (int i = 0; i < _savedExpressionTriggers.Length; i++)
            {
                _savedExpressionTriggers[i] = string.Empty;
            }
        }

        private static void MapTargets(ref EyeState state, ref MappedWeights weights)
        {
            float horizontal = state.LookAvailable ? state.FinalHorizontal : 0f;
            float vertical = state.LookAvailable ? state.FinalVertical : 0f;

            if (horizontal > 0f)
            {
                weights.PositiveX = DirectionalMapper.MapDirectional(
                    horizontal,
                    PluginConfig.PositiveXDeadZone.Value,
                    PluginConfig.PositiveXInputLimit.Value,
                    PluginConfig.PositiveXMaxWeight.Value,
                    PluginConfig.PositiveXGamma.Value);
                weights.NegativeX = 0f;
            }
            else if (horizontal < 0f)
            {
                weights.PositiveX = 0f;
                weights.NegativeX = DirectionalMapper.MapDirectional(
                    -horizontal,
                    PluginConfig.NegativeXDeadZone.Value,
                    PluginConfig.NegativeXInputLimit.Value,
                    PluginConfig.NegativeXMaxWeight.Value,
                    PluginConfig.NegativeXGamma.Value);
            }
            else
            {
                weights.PositiveX = 0f;
                weights.NegativeX = 0f;
            }

            if (vertical > 0f)
            {
                weights.PositiveY = DirectionalMapper.MapDirectional(
                    vertical,
                    PluginConfig.PositiveYDeadZone.Value,
                    PluginConfig.PositiveYInputLimit.Value,
                    PluginConfig.PositiveYMaxWeight.Value,
                    PluginConfig.PositiveYGamma.Value);
                weights.NegativeY = 0f;
            }
            else if (vertical < 0f)
            {
                weights.PositiveY = 0f;
                weights.NegativeY = DirectionalMapper.MapDirectional(
                    -vertical,
                    PluginConfig.NegativeYDeadZone.Value,
                    PluginConfig.NegativeYInputLimit.Value,
                    PluginConfig.NegativeYMaxWeight.Value,
                    PluginConfig.NegativeYGamma.Value);
            }
            else
            {
                weights.PositiveY = 0f;
                weights.NegativeY = 0f;
            }

            weights.Blink = state.BlinkAvailable
                ? DirectionalMapper.MapDirectional(
                    state.Closure,
                    PluginConfig.BlinkDeadZone.Value,
                    1f,
                    PluginConfig.BlinkMaxWeight.Value,
                    PluginConfig.BlinkGamma.Value)
                : 0f;
        }

        private void ApplySmoothing(
            ref MappedWeights target,
            ref MappedWeights current,
            bool smoothBlink)
        {
            if (!_smoothingInitialized)
            {
                current = target;
                _smoothingInitialized = true;
                return;
            }

            bool smoothDirections = PluginConfig.SmoothingEnabled.Value;
            float blinkSpeed = smoothBlink
                ? PluginConfig.BlinkSmoothingSpeed.Value
                : 0f;
            if (!smoothDirections && blinkSpeed <= 0f)
            {
                current = target;
                return;
            }

            float deltaTime = PluginConfig.UseUnscaledTime.Value
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            if (smoothDirections)
            {
                float speed = PluginConfig.SmoothingSpeed.Value;
                float alpha = DirectionalMapper.CalculateSmoothingAlpha(
                    speed,
                    deltaTime);
                current.PositiveX = DirectionalMapper.ApplySmoothingAlpha(
                    current.PositiveX,
                    target.PositiveX,
                    alpha);
                current.NegativeX = DirectionalMapper.ApplySmoothingAlpha(
                    current.NegativeX,
                    target.NegativeX,
                    alpha);
                current.PositiveY = DirectionalMapper.ApplySmoothingAlpha(
                    current.PositiveY,
                    target.PositiveY,
                    alpha);
                current.NegativeY = DirectionalMapper.ApplySmoothingAlpha(
                    current.NegativeY,
                    target.NegativeY,
                    alpha);
            }
            else
            {
                current.PositiveX = target.PositiveX;
                current.NegativeX = target.NegativeX;
                current.PositiveY = target.PositiveY;
                current.NegativeY = target.NegativeY;
            }

            current.Blink = blinkSpeed > 0f
                ? DirectionalMapper.SmoothTowards(current.Blink, target.Blink, blinkSpeed, deltaTime)
                : target.Blink;
        }

        private void LogEyeValuesIfDue()
        {
            if (!PluginConfig.LogEyeValues.Value)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (now < _nextEyeLogTime)
            {
                return;
            }

            _nextEyeLogTime = now + PluginConfig.LogIntervalSeconds.Value;
            Plugin.Log.LogInfo(string.Format(
                "{0}: Llocal={1:F3} Rlocal={2:F3} Lcommon={3:F3} Rcommon={4:F3} Y={5:F3} final=({6:F3},{7:F3}) open={8:F3}/{9:F3} closure={10:F3} weights=({11:F1},{12:F1},{13:F1},{14:F1},{15:F1})",
                GetCharacterName(),
                _eyeState.LeftHorizontal,
                _eyeState.RightHorizontal,
                _eyeState.LeftHorizontalCommon,
                _eyeState.RightHorizontalCommon,
                _eyeState.Vertical,
                _eyeState.FinalHorizontal,
                _eyeState.FinalVertical,
                _eyeState.CurrentOpen,
                _eyeState.EffectiveMaximumOpen,
                _eyeState.Closure,
                _appliedWeights.PositiveX,
                _appliedWeights.NegativeX,
                _appliedWeights.PositiveY,
                _appliedWeights.NegativeY,
                _appliedWeights.Blink));
        }

        internal string GetCharacterName()
        {
            try
            {
                if (ChaControl != null &&
                    ChaControl.chaFile != null &&
                    ChaControl.chaFile.parameter != null &&
                    !string.IsNullOrEmpty(ChaControl.chaFile.parameter.fullname))
                {
                    return ChaControl.chaFile.parameter.fullname;
                }
            }
            catch (Exception)
            {
                // Diagnostic fallback only.
            }

            return ChaControl == null ? "<destroyed character>" : ChaControl.name;
        }

        private void RegisterInstance()
        {
            if (!Controllers.Contains(this))
            {
                Controllers.Add(this);
            }
        }

        private static string BuildAmbiguousPathList(ResolveResult result)
        {
            if (result.AmbiguousCandidates == null || result.AmbiguousCandidates.Count == 0)
            {
                return "<none>";
            }

            string text = string.Empty;
            for (int i = 0; i < result.AmbiguousCandidates.Count; i++)
            {
                if (i > 0)
                {
                    text += ", ";
                }

                text += result.AmbiguousCandidates[i].RelativePath;
            }

            return text;
        }

        private static void DebugLog(string message)
        {
            if (PluginConfig.DebugLogging.Value)
            {
                Plugin.Log.LogInfo(message);
            }
        }
    }
}
