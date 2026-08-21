using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    /// <summary>
    /// Resolves game facial-expression patterns once and samples their current
    /// weights without reflection or managed allocations in the frame loop.
    /// </summary>
    internal sealed class ExpressionTriggerController
    {
        private delegate Dictionary<int, float> CurrentFaceDictionaryGetter(
            FBSBase controller);

        private const int PartArrayLength = 4;
        private const string InvalidSlotMessage =
            "The expression trigger slot index is outside the valid range.";

        private static readonly CurrentFaceDictionaryGetter GetCurrentFaceDictionary =
            CreateCurrentFaceDictionaryGetter();

        private readonly FBSBase[] _partControllers =
            new FBSBase[PartArrayLength];
        private readonly Dictionary<int, float>[] _currentDictionaries =
            new Dictionary<int, float>[PartArrayLength];
        private readonly string[] _configuredTriggers;
        private readonly ExpressionTriggerResolutionStatus[] _statuses;
        private readonly string[] _messages;
        private readonly ExpressionTriggerSelector[] _selectors;
        private readonly bool[] _activeSlots;
        private readonly float[] _slotWeights;

        private readonly bool _faceControllersAvailable;
        private bool _hasSampled;

        private ExpressionTriggerController(
            ChaControl chaControl,
            string[] configuredTriggers)
        {
            int slotCount = configuredTriggers == null
                ? ExpressionTriggerSyntax.SlotCount
                : configuredTriggers.Length;
            _configuredTriggers = new string[slotCount];
            _statuses = new ExpressionTriggerResolutionStatus[slotCount];
            _messages = new string[slotCount];
            _selectors = new ExpressionTriggerSelector[slotCount];
            _activeSlots = new bool[slotCount];
            _slotWeights = new float[slotCount];

            FaceBlendShape faceBlendShape = chaControl == null
                ? null
                : chaControl.fbsCtrl;
            if (faceBlendShape != null)
            {
                _partControllers[(int)ExpressionTriggerPart.Brow] =
                    faceBlendShape.EyebrowCtrl;
                _partControllers[(int)ExpressionTriggerPart.Eyes] =
                    faceBlendShape.EyesCtrl;
                _partControllers[(int)ExpressionTriggerPart.Mouth] =
                    faceBlendShape.MouthCtrl;
            }

            _faceControllersAvailable =
                GetCurrentFaceDictionary != null &&
                (_partControllers[(int)ExpressionTriggerPart.Brow] != null ||
                 _partControllers[(int)ExpressionTriggerPart.Eyes] != null ||
                 _partControllers[(int)ExpressionTriggerPart.Mouth] != null);

            for (int i = 0; i < _configuredTriggers.Length; i++)
            {
                string configured = configuredTriggers != null &&
                                    i < configuredTriggers.Length
                    ? configuredTriggers[i]
                    : string.Empty;
                _configuredTriggers[i] = configured ?? string.Empty;
                ResolveSlot(i, _configuredTriggers[i]);
            }
        }

        internal static ExpressionTriggerController Resolve(
            ChaControl chaControl,
            string[] configuredTriggers)
        {
            return new ExpressionTriggerController(chaControl, configuredTriggers);
        }

        internal bool RuntimeAvailable
        {
            get { return _faceControllersAvailable; }
        }
        internal int SlotCount
        {
            get { return _configuredTriggers.Length; }
        }


        internal bool HasReadySlot
        {
            get
            {
                for (int i = 0; i < _statuses.Length; i++)
                {
                    if (_statuses[i] == ExpressionTriggerResolutionStatus.Ready)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Cached slot activity. The same array instance is retained for the
        /// lifetime of this controller and must be treated as read-only.
        /// </summary>
        internal bool[] ActiveSlots
        {
            get { return _activeSlots; }
        }

        internal bool HasSampled
        {
            get { return _hasSampled; }
        }

        internal string GetConfiguredTrigger(int slotIndex)
        {
            return IsValidSlot(slotIndex)
                ? _configuredTriggers[slotIndex]
                : string.Empty;
        }

        internal ExpressionTriggerResolutionStatus GetStatus(int slotIndex)
        {
            return IsValidSlot(slotIndex)
                ? _statuses[slotIndex]
                : ExpressionTriggerResolutionStatus.Invalid;
        }

        internal string GetMessage(int slotIndex)
        {
            return IsValidSlot(slotIndex)
                ? _messages[slotIndex]
                : InvalidSlotMessage;
        }

        internal ExpressionTriggerSelector GetSelector(int slotIndex)
        {
            return IsValidSlot(slotIndex)
                ? _selectors[slotIndex]
                : default(ExpressionTriggerSelector);
        }

        internal bool IsActive(int slotIndex)
        {
            return IsValidSlot(slotIndex) && _activeSlots[slotIndex];
        }

        internal float GetSlotWeight(int slotIndex)
        {
            return IsValidSlot(slotIndex) ? _slotWeights[slotIndex] : 0f;
        }

        /// <summary>
        /// Samples all three FBS dictionaries once and updates the four cached
        /// trigger states. Returns true when this is the first sample or when
        /// at least one cached active state changed.
        /// </summary>
        internal bool Sample(float threshold)
        {
            if (float.IsNaN(threshold) || threshold < 0f)
            {
                threshold = 0f;
            }

            for (int partIndex = (int)ExpressionTriggerPart.Brow;
                 partIndex <= (int)ExpressionTriggerPart.Mouth;
                 partIndex++)
            {
                _currentDictionaries[partIndex] =
                    ReadCurrentDictionary(partIndex);
            }

            bool changed = !_hasSampled;
            for (int slotIndex = 0;
                 slotIndex < _statuses.Length;
                 slotIndex++)
            {
                float weight = 0f;
                bool active = false;
                if (_statuses[slotIndex] ==
                        ExpressionTriggerResolutionStatus.Ready)
                {
                    ExpressionTriggerSelector selector = _selectors[slotIndex];
                    Dictionary<int, float> dictionary =
                        _currentDictionaries[(int)selector.Part];
                    if (dictionary != null &&
                        dictionary.TryGetValue(selector.PatternIndex, out weight) &&
                        !float.IsNaN(weight))
                    {
                        active = weight > threshold;
                    }
                    else
                    {
                        weight = 0f;
                    }
                }

                _slotWeights[slotIndex] = weight;
                if (_activeSlots[slotIndex] != active)
                {
                    _activeSlots[slotIndex] = active;
                    changed = true;
                }
            }

            _hasSampled = true;
            return changed;
        }

        /// <summary>
        /// Returns the pattern with the greatest current FBS weight for a part.
        /// After Sample this is a zero-cost cached lookup. Before the first
        /// sample it reads and scans the live dictionary once.
        /// </summary>
        internal ExpressionTriggerSelector GetCurrentSelector(
            ExpressionTriggerPart part)
        {
            int partIndex = (int)part;
            if (!IsValidPartIndex(partIndex))
            {
                return default(ExpressionTriggerSelector);
            }

            Dictionary<int, float> dictionary = _hasSampled
                ? _currentDictionaries[partIndex]
                : ReadCurrentDictionary(partIndex);
            ExpressionTriggerSelector selector;
            float ignoredWeight;
            FindGreatestCurrentPattern(
                part,
                dictionary,
                out selector,
                out ignoredWeight);
            return selector;
        }

        internal float GetCurrentWeight(ExpressionTriggerPart part)
        {
            int partIndex = (int)part;
            if (!IsValidPartIndex(partIndex))
            {
                return 0f;
            }

            Dictionary<int, float> dictionary = _hasSampled
                ? _currentDictionaries[partIndex]
                : ReadCurrentDictionary(partIndex);
            ExpressionTriggerSelector ignoredSelector;
            float weight;
            FindGreatestCurrentPattern(
                part,
                dictionary,
                out ignoredSelector,
                out weight);
            return weight;
        }

        private void ResolveSlot(int slotIndex, string configuredTrigger)
        {
            ParsedExpressionTrigger parsed;
            ExpressionTriggerResolutionStatus failureStatus;
            string parseMessage;
            if (!ExpressionTriggerSyntax.TryParse(
                configuredTrigger,
                out parsed,
                out failureStatus,
                out parseMessage))
            {
                SetSlotResolution(
                    slotIndex,
                    failureStatus,
                    default(ExpressionTriggerSelector),
                    parseMessage);
                return;
            }

            if (GetCurrentFaceDictionary == null)
            {
                SetSlotResolution(
                    slotIndex,
                    ExpressionTriggerResolutionStatus.Unavailable,
                    default(ExpressionTriggerSelector),
                    "The FBS expression-weight accessor is unavailable.");
                return;
            }

            if (parsed.IsSelector)
            {
                ResolveDirectSelector(slotIndex, parsed.Selector);
                return;
            }

            ResolveBlendshapeName(slotIndex, parsed.BlendshapeName);
        }

        private void ResolveDirectSelector(
            int slotIndex,
            ExpressionTriggerSelector selector)
        {
            FBSBase controller = GetPartController(selector.Part);
            if (controller == null)
            {
                SetSlotResolution(
                    slotIndex,
                    ExpressionTriggerResolutionStatus.Unavailable,
                    default(ExpressionTriggerSelector),
                    "The " + GetPartDisplayName(selector.Part) +
                    " expression controller is unavailable.");
                return;
            }

            if (!PartContainsPattern(controller, selector.PatternIndex))
            {
                SetSlotResolution(
                    slotIndex,
                    ExpressionTriggerResolutionStatus.Missing,
                    default(ExpressionTriggerSelector),
                    "The selector " + ExpressionTriggerSyntax.FormatSelector(selector) +
                    " does not exist in this character's FBS targets.");
                return;
            }

            SetSlotResolution(
                slotIndex,
                ExpressionTriggerResolutionStatus.Ready,
                selector,
                "Resolved selector " +
                ExpressionTriggerSyntax.FormatSelector(selector) + ".");
        }

        private void ResolveBlendshapeName(int slotIndex, string blendshapeName)
        {
            bool scannedAnyPart = false;
            bool found = false;
            bool ambiguous = false;
            ExpressionTriggerSelector resolved =
                default(ExpressionTriggerSelector);
            ExpressionTriggerSelector conflicting =
                default(ExpressionTriggerSelector);

            for (int partIndex = (int)ExpressionTriggerPart.Brow;
                 partIndex <= (int)ExpressionTriggerPart.Mouth;
                 partIndex++)
            {
                ExpressionTriggerPart part = (ExpressionTriggerPart)partIndex;
                FBSBase controller = _partControllers[partIndex];
                if (controller == null)
                {
                    continue;
                }

                scannedAnyPart = true;
                ExpressionTriggerSelector partSelector;
                ExpressionTriggerSelector partConflict;
                bool partAmbiguous;
                if (!FindBlendshapeSelector(
                    controller,
                    part,
                    blendshapeName,
                    out partSelector,
                    out partConflict,
                    out partAmbiguous))
                {
                    continue;
                }

                if (partAmbiguous)
                {
                    if (!found)
                    {
                        resolved = partSelector;
                        found = true;
                    }

                    conflicting = partConflict;
                    ambiguous = true;
                    break;
                }

                if (!found)
                {
                    resolved = partSelector;
                    found = true;
                    continue;
                }

                if (resolved != partSelector)
                {
                    conflicting = partSelector;
                    ambiguous = true;
                    break;
                }
            }

            if (ambiguous)
            {
                SetSlotResolution(
                    slotIndex,
                    ExpressionTriggerResolutionStatus.Ambiguous,
                    default(ExpressionTriggerSelector),
                    "Blendshape \"" + blendshapeName +
                    "\" maps to more than one expression selector (" +
                    ExpressionTriggerSyntax.FormatSelector(resolved) + " and " +
                    ExpressionTriggerSyntax.FormatSelector(conflicting) + "). " +
                    "Use an explicit brow:N, eyes:N, or mouth:N selector.");
                return;
            }

            if (found)
            {
                SetSlotResolution(
                    slotIndex,
                    ExpressionTriggerResolutionStatus.Ready,
                    resolved,
                    "Resolved blendshape \"" + blendshapeName + "\" to " +
                    ExpressionTriggerSyntax.FormatSelector(resolved) + ".");
                return;
            }

            SetSlotResolution(
                slotIndex,
                scannedAnyPart
                    ? ExpressionTriggerResolutionStatus.Missing
                    : ExpressionTriggerResolutionStatus.Unavailable,
                default(ExpressionTriggerSelector),
                scannedAnyPart
                    ? "Blendshape \"" + blendshapeName +
                      "\" was not found in any FBS target Close/Open pattern."
                    : "The character's FBS expression controllers are unavailable.");
        }

        private static bool FindBlendshapeSelector(
            FBSBase controller,
            ExpressionTriggerPart part,
            string blendshapeName,
            out ExpressionTriggerSelector selector,
            out ExpressionTriggerSelector conflictingSelector,
            out bool ambiguous)
        {
            selector = default(ExpressionTriggerSelector);
            conflictingSelector = default(ExpressionTriggerSelector);
            ambiguous = false;
            bool found = false;
            FBSTargetInfo[] targets = controller.FBSTarget;
            if (targets == null)
            {
                return false;
            }

            for (int targetIndex = 0;
                 targetIndex < targets.Length;
                 targetIndex++)
            {
                FBSTargetInfo target = targets[targetIndex];
                if (target == null || target.PtnSet == null)
                {
                    continue;
                }

                SkinnedMeshRenderer renderer = target.GetSkinnedMeshRenderer();
                Mesh mesh = renderer == null ? null : renderer.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                for (int patternIndex = 0;
                     patternIndex < target.PtnSet.Length;
                     patternIndex++)
                {
                    FBSTargetInfo.CloseOpen pattern = target.PtnSet[patternIndex];
                    if (pattern == null ||
                        (!BlendshapeNameEquals(
                            mesh,
                            pattern.Close,
                            blendshapeName) &&
                         !BlendshapeNameEquals(
                            mesh,
                            pattern.Open,
                            blendshapeName)))
                    {
                        continue;
                    }

                    ExpressionTriggerSelector candidate =
                        new ExpressionTriggerSelector(part, patternIndex);
                    if (!found)
                    {
                        selector = candidate;
                        found = true;
                    }
                    else if (selector != candidate)
                    {
                        conflictingSelector = candidate;
                        ambiguous = true;
                        return true;
                    }
                }
            }

            return found;
        }

        private static bool PartContainsPattern(
            FBSBase controller,
            int patternIndex)
        {
            if (controller == null || patternIndex < 0)
            {
                return false;
            }

            FBSTargetInfo[] targets = controller.FBSTarget;
            if (targets == null)
            {
                return false;
            }

            for (int i = 0; i < targets.Length; i++)
            {
                FBSTargetInfo target = targets[i];
                if (target != null &&
                    target.PtnSet != null &&
                    patternIndex < target.PtnSet.Length)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool BlendshapeNameEquals(
            Mesh mesh,
            int blendshapeIndex,
            string expectedName)
        {
            return blendshapeIndex >= 0 &&
                   blendshapeIndex < mesh.blendShapeCount &&
                   string.Equals(
                       mesh.GetBlendShapeName(blendshapeIndex),
                       expectedName,
                       StringComparison.OrdinalIgnoreCase);
        }

        private Dictionary<int, float> ReadCurrentDictionary(int partIndex)
        {
            if (GetCurrentFaceDictionary == null ||
                !IsValidPartIndex(partIndex))
            {
                return null;
            }

            FBSBase controller = _partControllers[partIndex];
            return controller == null
                ? null
                : GetCurrentFaceDictionary(controller);
        }

        private static void FindGreatestCurrentPattern(
            ExpressionTriggerPart part,
            Dictionary<int, float> dictionary,
            out ExpressionTriggerSelector selector,
            out float weight)
        {
            selector = default(ExpressionTriggerSelector);
            weight = 0f;
            if (dictionary == null || dictionary.Count == 0)
            {
                return;
            }

            bool found = false;
            int bestPattern = -1;
            float bestWeight = 0f;
            Dictionary<int, float>.Enumerator enumerator =
                dictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<int, float> item = enumerator.Current;
                if (item.Key < 0 || float.IsNaN(item.Value))
                {
                    continue;
                }

                if (!found ||
                    item.Value > bestWeight ||
                    (item.Value == bestWeight && item.Key < bestPattern))
                {
                    found = true;
                    bestPattern = item.Key;
                    bestWeight = item.Value;
                }
            }

            if (found)
            {
                selector = new ExpressionTriggerSelector(part, bestPattern);
                weight = bestWeight;
            }
        }

        private void SetSlotResolution(
            int slotIndex,
            ExpressionTriggerResolutionStatus status,
            ExpressionTriggerSelector selector,
            string message)
        {
            _statuses[slotIndex] = status;
            _selectors[slotIndex] = selector;
            _messages[slotIndex] = message ?? string.Empty;
            _activeSlots[slotIndex] = false;
            _slotWeights[slotIndex] = 0f;
        }

        private FBSBase GetPartController(ExpressionTriggerPart part)
        {
            int partIndex = (int)part;
            return IsValidPartIndex(partIndex)
                ? _partControllers[partIndex]
                : null;
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 &&
                   slotIndex < _configuredTriggers.Length;
        }

        private static bool IsValidPartIndex(int partIndex)
        {
            return partIndex >= (int)ExpressionTriggerPart.Brow &&
                   partIndex <= (int)ExpressionTriggerPart.Mouth;
        }

        private static string GetPartDisplayName(ExpressionTriggerPart part)
        {
            switch (part)
            {
                case ExpressionTriggerPart.Brow:
                    return "brow";
                case ExpressionTriggerPart.Eyes:
                    return "eyes";
                case ExpressionTriggerPart.Mouth:
                    return "mouth";
                default:
                    return "unknown";
            }
        }

        private static CurrentFaceDictionaryGetter
            CreateCurrentFaceDictionaryGetter()
        {
            try
            {
                FieldInfo field = typeof(FBSBase).GetField(
                    "dictNowFace",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null ||
                    field.FieldType != typeof(Dictionary<int, float>))
                {
                    return null;
                }

                DynamicMethod method = new DynamicMethod(
                    "EyeMotion_GetFbsCurrentFaceDictionary",
                    typeof(Dictionary<int, float>),
                    new Type[] { typeof(FBSBase) },
                    typeof(ExpressionTriggerController),
                    true);
                ILGenerator il = method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ret);
                return (CurrentFaceDictionaryGetter)method.CreateDelegate(
                    typeof(CurrentFaceDictionaryGetter));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
