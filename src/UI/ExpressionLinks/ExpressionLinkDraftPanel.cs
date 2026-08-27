using System;
using System.Globalization;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkDraftPanel
    {
        private static readonly CultureInfo InvariantCulture =
            CultureInfo.InvariantCulture;

        private readonly ExpressionLinkSourceSectionView _sourceSection =
            new ExpressionLinkSourceSectionView();
        private readonly ExpressionLinkTargetSectionView _targetSection =
            new ExpressionLinkTargetSectionView();
        private readonly ExpressionLinkResponseSectionView _responseSection =
            new ExpressionLinkResponseSectionView();

        private ExpressionLinkDefinition _draft;
        private bool _dirty;
        private bool _showAdvanced;
        private bool _changedDuringLastDraw;
        private string _thresholdText = string.Empty;
        private string _inputMinText = string.Empty;
        private string _inputMaxText = string.Empty;
        private string _outputMinText = string.Empty;
        private string _outputMaxText = string.Empty;
        private string _smoothingSpeedText = string.Empty;
        private string _priorityText = string.Empty;
        private string _componentIndexText = string.Empty;
        private string _slotIndexText = string.Empty;

        internal bool HasDraft
        {
            get { return _draft != null; }
        }

        internal bool IsDirty
        {
            get { return _dirty; }
        }

        internal bool ChangedDuringLastDraw
        {
            get { return _changedDuringLastDraw; }
        }

        internal string DisplayName
        {
            get
            {
                return _draft == null
                    ? string.Empty
                    : _draft.Name ?? string.Empty;
            }
        }

        internal void Reset()
        {
            _draft = null;
            _dirty = false;
            _changedDuringLastDraw = false;
            _showAdvanced = false;
            ClearNumericText();
        }

        internal void Load(ExpressionLinkDefinition definition)
        {
            _draft = definition == null ? null : definition.Clone();
            _dirty = false;
            _changedDuringLastDraw = false;
            _showAdvanced = false;
            LoadNumericText();
        }

        internal string Draw(EyeMotionCharacterController controller)
        {
            _changedDuringLastDraw = false;
            if (_draft == null)
            {
                return string.Empty;
            }

            bool enabled = ExpressionLinkGUILayout.Toggle(
                _draft.Enabled,
                "Expression link enabled");
            if (enabled != _draft.Enabled)
            {
                _draft.Enabled = enabled;
                MarkDirty();
            }

            string feedback = _sourceSection.Draw(
                controller,
                _draft,
                MarkDirty);
            _targetSection.Draw(
                _draft,
                MarkDirty,
                HandleScopeChanged);
            _responseSection.Draw(
                _draft,
                ref _outputMaxText,
                MarkDirty);
            DrawAdvancedFields();
            return feedback;
        }

        internal bool TryBuildCandidate(
            out ExpressionLinkDefinition candidate,
            out string error)
        {
            candidate = null;
            error = string.Empty;
            if (_draft == null)
            {
                error = "No expression link is selected.";
                return false;
            }

            if (_draft.Enabled &&
                ExpressionLinkDraftRules.IsBlank(_draft.Source))
            {
                error =
                    "Step 1 is incomplete. Capture a game expression " +
                    "before saving this enabled link.";
                return false;
            }

            if (_draft.Enabled &&
                ExpressionLinkDraftRules.IsBlank(_draft.BlendshapeName))
            {
                error =
                    "Step 2 is incomplete. Enter the destination " +
                    "blendshape name before saving this enabled link.";
                return false;
            }

            float threshold;
            float inputMin;
            float inputMax;
            float outputMin;
            float outputMax;
            float smoothingSpeed;
            int priority;
            int componentIndex;
            int slotIndex;
            if (!TryParseFloat(
                    _thresholdText,
                    "Activation point",
                    out threshold,
                    out error) ||
                !TryParseFloat(
                    _inputMinText,
                    "Expression range start",
                    out inputMin,
                    out error) ||
                !TryParseFloat(
                    _inputMaxText,
                    "Expression range end",
                    out inputMax,
                    out error) ||
                !TryParseFloat(
                    _outputMinText,
                    "Inactive strength",
                    out outputMin,
                    out error) ||
                !TryParseFloat(
                    _outputMaxText,
                    "Maximum strength",
                    out outputMax,
                    out error) ||
                !TryParseFloat(
                    _smoothingSpeedText,
                    "Transition speed",
                    out smoothingSpeed,
                    out error) ||
                !TryParseInteger(
                    _priorityText,
                    "Priority",
                    out priority,
                    out error) ||
                !TryParseInteger(
                    _componentIndexText,
                    "Renderer component",
                    out componentIndex,
                    out error) ||
                !TryParseInteger(
                    _slotIndexText,
                    "Character slot",
                    out slotIndex,
                    out error))
            {
                return false;
            }

            candidate = _draft.Clone();
            candidate.Threshold = threshold;
            candidate.InputMin = inputMin;
            candidate.InputMax = inputMax;
            candidate.OutputMin = outputMin;
            candidate.OutputMax = outputMax;
            candidate.SmoothingSpeed = smoothingSpeed;
            candidate.Priority = priority;
            candidate.ComponentIndex = componentIndex;
            candidate.SlotIndex = slotIndex;
            return true;
        }

        private void DrawAdvancedFields()
        {
            _showAdvanced = ExpressionLinkGUILayout.Disclosure(
                _showAdvanced,
                "Advanced targeting and tuning");

            if (!_showAdvanced)
            {
                return;
            }

            _draft.Name = DrawDraftTextField(
                "Link name",
                _draft.Name);
            _draft.Source = DrawDraftTextField(
                "Expression selector",
                _draft.Source);
            ExpressionLinkGUILayout.Help(
                "The selector is filled automatically by the capture buttons. " +
                "Edit it only when you need an exact brow:N, eyes:N, mouth:N, " +
                "or FBS name.");

            _draft.RendererPath = DrawDraftTextField(
                "Renderer path",
                _draft.RendererPath);
            ExpressionLinkGUILayout.Help(
                "Leave the renderer path blank when the blendshape is unique " +
                "in the selected character area.");

            if (ScopeUsesSlots(_draft.Scope) ||
                !IsDefaultSlotFilter())
            {
                DrawNumericTextField(
                    "Character slot (-1 = any)",
                    ref _slotIndexText);
            }

            DrawNumericTextField(
                "Renderer component (-1 = any)",
                ref _componentIndexText);
            _draft.RendererHint = DrawDraftTextField(
                "Preferred renderer",
                _draft.RendererHint);
            _draft.MeshHint = DrawDraftTextField(
                "Preferred mesh",
                _draft.MeshHint);
            DrawNumericTextField(
                "Inactive strength",
                ref _outputMinText);
            DrawNumericTextField(
                "Expression range start",
                ref _inputMinText);
            DrawNumericTextField(
                "Expression range end",
                ref _inputMaxText);
            DrawNumericTextField(
                "Activation point",
                ref _thresholdText);
            DrawNumericTextField(
                "Transition speed",
                ref _smoothingSpeedText);
            DrawNumericTextField("Priority", ref _priorityText);
        }

        private void LoadNumericText()
        {
            if (_draft == null)
            {
                ClearNumericText();
                return;
            }

            _thresholdText = FormatFloat(_draft.Threshold);
            _inputMinText = FormatFloat(_draft.InputMin);
            _inputMaxText = FormatFloat(_draft.InputMax);
            _outputMinText = FormatFloat(_draft.OutputMin);
            _outputMaxText = FormatFloat(_draft.OutputMax);
            _smoothingSpeedText = FormatFloat(_draft.SmoothingSpeed);
            _priorityText = _draft.Priority.ToString(InvariantCulture);
            _componentIndexText =
                _draft.ComponentIndex.ToString(InvariantCulture);
            _slotIndexText = _draft.SlotIndex.ToString(InvariantCulture);
        }

        private void ClearNumericText()
        {
            _thresholdText = string.Empty;
            _inputMinText = string.Empty;
            _inputMaxText = string.Empty;
            _outputMinText = string.Empty;
            _outputMaxText = string.Empty;
            _smoothingSpeedText = string.Empty;
            _priorityText = string.Empty;
            _componentIndexText = string.Empty;
            _slotIndexText = string.Empty;
        }

        private string DrawDraftTextField(string label, string value)
        {
            string original = value ?? string.Empty;
            string next = ExpressionLinkGUILayout.TextField(label, original);
            if (!string.Equals(next, original, StringComparison.Ordinal))
            {
                MarkDirty();
            }

            return next;
        }

        private void DrawNumericTextField(string label, ref string value)
        {
            string original = value ?? string.Empty;
            string next = ExpressionLinkGUILayout.NumericTextField(
                label,
                original);
            if (!string.Equals(next, original, StringComparison.Ordinal))
            {
                value = next;
                MarkDirty();
            }
        }

        private void MarkDirty()
        {
            _dirty = true;
            _changedDuringLastDraw = true;
        }

        private void HandleScopeChanged(ExpressionTargetScope scope)
        {
            if (ScopeUsesSlots(scope))
            {
                return;
            }

            _draft.SlotIndex = -1;
            _slotIndexText = "-1";
        }

        private bool IsDefaultSlotFilter()
        {
            return string.Equals(_slotIndexText, "-1", StringComparison.Ordinal);
        }

        private static bool ScopeUsesSlots(ExpressionTargetScope scope)
        {
            return scope == ExpressionTargetScope.Any ||
                scope == ExpressionTargetScope.Hair ||
                scope == ExpressionTargetScope.Clothes ||
                scope == ExpressionTargetScope.Accessory;
        }

        private static bool TryParseFloat(
            string text,
            string label,
            out float value,
            out string error)
        {
            if (!float.TryParse(
                    text,
                    NumberStyles.Float,
                    InvariantCulture,
                    out value) ||
                float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                error = label +
                    " must be a finite number using '.' as decimal separator.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool TryParseInteger(
            string text,
            string label,
            out int value,
            out string error)
        {
            if (!int.TryParse(
                    text,
                    NumberStyles.Integer,
                    InvariantCulture,
                    out value))
            {
                error = label + " must be a whole number.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.######", InvariantCulture);
        }
    }
}
