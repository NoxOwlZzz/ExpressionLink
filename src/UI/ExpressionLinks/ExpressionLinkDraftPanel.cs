using System;
using System.Globalization;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkDraftPanel
    {
        private const int FirstScope = (int)ExpressionTargetScope.Any;
        private const int LastScope = (int)ExpressionTargetScope.Other;

        private static readonly CultureInfo InvariantCulture =
            CultureInfo.InvariantCulture;

        private ExpressionLinkDefinition _draft;
        private bool _dirty;
        private bool _showAdvanced;
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

        internal void Reset()
        {
            _draft = null;
            _dirty = false;
            ClearNumericText();
        }

        internal void Load(ExpressionLinkDefinition definition)
        {
            _draft = definition == null ? null : definition.Clone();
            _dirty = false;
            LoadNumericText();
        }

        internal string Draw(EyeMotionCharacterController controller)
        {
            if (_draft == null)
            {
                return string.Empty;
            }

            string feedback = DrawBasicFields(controller);
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
                error = "No link is selected.";
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
                    "Activation threshold",
                    out threshold,
                    out error) ||
                !TryParseFloat(
                    _inputMinText,
                    "Input minimum",
                    out inputMin,
                    out error) ||
                !TryParseFloat(
                    _inputMaxText,
                    "Input maximum",
                    out inputMax,
                    out error) ||
                !TryParseFloat(
                    _outputMinText,
                    "Inactive weight",
                    out outputMin,
                    out error) ||
                !TryParseFloat(
                    _outputMaxText,
                    "Active weight",
                    out outputMax,
                    out error) ||
                !TryParseFloat(
                    _smoothingSpeedText,
                    "Smoothing speed",
                    out smoothingSpeed,
                    out error) ||
                !TryParseInteger(
                    _priorityText,
                    "Priority",
                    out priority,
                    out error) ||
                !TryParseInteger(
                    _componentIndexText,
                    "Component index",
                    out componentIndex,
                    out error) ||
                !TryParseInteger(
                    _slotIndexText,
                    "Slot index",
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

        private string DrawBasicFields(
            EyeMotionCharacterController controller)
        {
            bool enabled = ExpressionLinkGUILayout.Toggle(
                _draft.Enabled,
                "Enabled");
            if (enabled != _draft.Enabled)
            {
                _draft.Enabled = enabled;
                _dirty = true;
            }

            _draft.Name = DrawDraftTextField("Name", _draft.Name);
            _draft.Source = DrawDraftTextField("Source", _draft.Source);

            string feedback = string.Empty;
            ExpressionLinkGUILayout.BeginHorizontal();
            ExpressionLinkGUILayout.FieldLabel("Capture source");
            SetLatestFeedback(
                ref feedback,
                DrawCaptureButton(
                    controller,
                    "Brow",
                    ExpressionTriggerPart.Brow));
            SetLatestFeedback(
                ref feedback,
                DrawCaptureButton(
                    controller,
                    "Eyes",
                    ExpressionTriggerPart.Eyes));
            SetLatestFeedback(
                ref feedback,
                DrawCaptureButton(
                    controller,
                    "Mouth",
                    ExpressionTriggerPart.Mouth));
            ExpressionLinkGUILayout.EndHorizontal();

            _draft.BlendshapeName = DrawDraftTextField(
                "Target blendshape",
                _draft.BlendshapeName);
            DrawScopeField();
            DrawModeField();
            DrawNumericTextField("Active weight", ref _outputMaxText);
            return feedback;
        }

        private string DrawCaptureButton(
            EyeMotionCharacterController controller,
            string label,
            ExpressionTriggerPart part)
        {
            if (!ExpressionLinkGUILayout.Button(label))
            {
                return string.Empty;
            }

            string selector = controller.GetCurrentExpressionSelector(part);
            if (string.IsNullOrEmpty(selector))
            {
                return "No current " + label.ToLowerInvariant() +
                    " source is available.";
            }

            _draft.Source = selector;
            _dirty = true;
            return "Captured " + selector + ". Press Apply.";
        }

        private void DrawScopeField()
        {
            ExpressionLinkGUILayout.BeginHorizontal();
            ExpressionLinkGUILayout.FieldLabel("Target scope");
            if (ExpressionLinkGUILayout.NarrowButton("<"))
            {
                CycleScope(-1);
            }

            if (ExpressionLinkGUILayout.Button(GetScopeName(_draft.Scope)))
            {
                CycleScope(1);
            }

            if (ExpressionLinkGUILayout.NarrowButton(">"))
            {
                CycleScope(1);
            }

            ExpressionLinkGUILayout.EndHorizontal();
        }

        private void DrawModeField()
        {
            ExpressionLinkGUILayout.BeginHorizontal();
            ExpressionLinkGUILayout.FieldLabel("Mode");
            string label = _draft.Mode == ExpressionLinkMode.FollowSource
                ? "Follow source"
                : "Binary";
            if (ExpressionLinkGUILayout.Button(label))
            {
                _draft.Mode = _draft.Mode == ExpressionLinkMode.Binary
                    ? ExpressionLinkMode.FollowSource
                    : ExpressionLinkMode.Binary;
                _dirty = true;
            }

            ExpressionLinkGUILayout.EndHorizontal();
        }

        private void DrawAdvancedFields()
        {
            string label = _showAdvanced
                ? "[-] Advanced"
                : "[+] Advanced";
            if (ExpressionLinkGUILayout.Button(label))
            {
                _showAdvanced = !_showAdvanced;
            }

            if (!_showAdvanced)
            {
                return;
            }

            _draft.RendererPath = DrawDraftTextField(
                "Renderer path (blank = auto)",
                _draft.RendererPath);
            DrawNumericTextField("Inactive weight", ref _outputMinText);
            DrawNumericTextField("Input minimum", ref _inputMinText);
            DrawNumericTextField("Input maximum", ref _inputMaxText);
            DrawNumericTextField("Activation threshold", ref _thresholdText);
            DrawNumericTextField("Smoothing speed", ref _smoothingSpeedText);
            DrawNumericTextField("Priority", ref _priorityText);
            DrawNumericTextField("Component index", ref _componentIndexText);
            DrawNumericTextField("Slot index", ref _slotIndexText);
            _draft.RendererHint = DrawDraftTextField(
                "Renderer name hint",
                _draft.RendererHint);
            _draft.MeshHint = DrawDraftTextField(
                "Mesh name hint",
                _draft.MeshHint);
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

        private void CycleScope(int direction)
        {
            int value = (int)_draft.Scope + direction;
            if (value < FirstScope)
            {
                value = LastScope;
            }
            else if (value > LastScope)
            {
                value = FirstScope;
            }

            _draft.Scope = (ExpressionTargetScope)value;
            _dirty = true;
        }

        private string DrawDraftTextField(string label, string value)
        {
            string original = value ?? string.Empty;
            string next = ExpressionLinkGUILayout.TextField(label, original);
            if (!string.Equals(next, original, StringComparison.Ordinal))
            {
                _dirty = true;
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
                _dirty = true;
            }
        }

        private static void SetLatestFeedback(
            ref string destination,
            string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                destination = message;
            }
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

        private static string GetScopeName(ExpressionTargetScope scope)
        {
            switch (scope)
            {
                case ExpressionTargetScope.Head:
                    return "Head";
                case ExpressionTargetScope.Hair:
                    return "Hair";
                case ExpressionTargetScope.Body:
                    return "Body";
                case ExpressionTargetScope.Clothes:
                    return "Clothes";
                case ExpressionTargetScope.Accessory:
                    return "Accessory";
                case ExpressionTargetScope.Other:
                    return "Other";
                default:
                    return "Any";
            }
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.######", InvariantCulture);
        }
    }
}
