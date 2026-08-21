using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    public sealed partial class EyeMotionCharacterController
    {
        private ExpressionLinkRuntime _expressionLinkRuntime;
        private float[] _expressionLinkWeights = new float[0];
        private bool[] _expressionLinkSourcesReady = new bool[0];
        private bool _hasEnabledExpressionLinks;
        private const int ExpressionLinkSourceOffset =
            ExpressionTriggerSyntax.SlotCount;

        private readonly List<ExpressionLinkDefinition> _savedExpressionLinks =
            new List<ExpressionLinkDefinition>();
        private bool _expressionLinksIntentChanged;
        private int _expressionLinkRevision;
        private int _loadedCardDataVersion;

        internal int ExpressionLinkCount
        {
            get { return _savedExpressionLinks.Count; }
        }

        internal int ExpressionLinkRevision
        {
            get { return _expressionLinkRevision; }
        }

        internal int LoadedCardDataVersion
        {
            get { return _loadedCardDataVersion; }
        }

        internal ExpressionLinkDefinition GetExpressionLink(int index)
        {
            return index >= 0 && index < _savedExpressionLinks.Count
                ? _savedExpressionLinks[index].Clone()
                : null;
        }

        internal ExpressionLinkDefinition[] CopyExpressionLinks()
        {
            ExpressionLinkDefinition[] result =
                new ExpressionLinkDefinition[_savedExpressionLinks.Count];
            for (int i = 0; i < _savedExpressionLinks.Count; i++)
            {
                result[i] = _savedExpressionLinks[i].Clone();
            }

            return result;
        }

        internal bool AddExpressionLink(
            ExpressionLinkDefinition definition,
            out int index,
            out string message)
        {
            index = -1;
            List<ExpressionLinkDefinition> candidates =
                new List<ExpressionLinkDefinition>(CopyExpressionLinks());
            candidates.Add(definition);
            if (!TryStoreExpressionLinks(candidates, out message))
            {
                return false;
            }

            index = _savedExpressionLinks.Count - 1;
            message = "Expression link added.";
            return true;
        }

        internal bool UpdateExpressionLink(
            int index,
            ExpressionLinkDefinition definition,
            out string message)
        {
            if (index < 0 || index >= _savedExpressionLinks.Count)
            {
                message = "Invalid expression-link index.";
                return false;
            }

            List<ExpressionLinkDefinition> candidates =
                new List<ExpressionLinkDefinition>(CopyExpressionLinks());
            candidates[index] = definition;
            if (!TryStoreExpressionLinks(candidates, out message))
            {
                return false;
            }

            message = "Expression link updated.";
            return true;
        }

        internal bool RemoveExpressionLink(
            int index,
            out string message)
        {
            if (index < 0 || index >= _savedExpressionLinks.Count)
            {
                message = "Invalid expression-link index.";
                return false;
            }

            List<ExpressionLinkDefinition> candidates =
                new List<ExpressionLinkDefinition>(CopyExpressionLinks());
            candidates.RemoveAt(index);
            if (!TryStoreExpressionLinks(candidates, out message))
            {
                return false;
            }

            message = "Expression link removed.";
            return true;
        }

        internal bool ReplaceExpressionLinks(
            IList<ExpressionLinkDefinition> definitions,
            bool regenerateIds,
            out string message)
        {
            List<ExpressionLinkDefinition> candidates =
                new List<ExpressionLinkDefinition>();
            if (definitions != null)
            {
                for (int i = 0; i < definitions.Count; i++)
                {
                    ExpressionLinkDefinition candidate = definitions[i] == null
                        ? null
                        : definitions[i].Clone();
                    if (candidate != null && regenerateIds)
                    {
                        candidate.Id = Guid.NewGuid();
                    }

                    candidates.Add(candidate);
                }
            }

            if (!TryStoreExpressionLinks(candidates, out message))
            {
                return false;
            }

            message = "Expression links replaced.";
            return true;
        }

        internal void ClearExpressionLinks()
        {
            _savedExpressionLinks.Clear();
            MarkExpressionLinksChanged();
        }

        private bool TryStoreExpressionLinks(
            IList<ExpressionLinkDefinition> candidates,
            out string message)
        {
            ExpressionLinkDefinition[] normalized;
            if (!ExpressionLinkValidator.TryNormalizeList(
                candidates,
                out normalized,
                out message))
            {
                return false;
            }

            byte[] serialized;
            if (!ExpressionLinkBinaryCodec.TryEncode(
                normalized,
                out serialized,
                out message))
            {
                return false;
            }

            _savedExpressionLinks.Clear();
            _savedExpressionLinks.AddRange(normalized);
            MarkExpressionLinksChanged();
            return true;
        }

        private void MarkExpressionLinksChanged()
        {
            _expressionLinkRevision++;
            _visibilityIntentChanged = true;
            _expressionLinksIntentChanged = true;
            _preserveUnsupportedCardData = false;
            if (PluginConfig.Enabled.Value)
            {
                RebuildExpressionTriggers();
                SynchronizeAutomaticExpressions();
            }
        }

        internal string GetExpressionLinkStatus(int index)
        {
            ExpressionLinkDefinition definition =
                GetExpressionLink(index);
            if (definition == null)
            {
                return "Unavailable";
            }

            if (!definition.Enabled)
            {
                return "Disabled";
            }

            int sourceIndex = ExpressionLinkSourceOffset + index;
            ExpressionTriggerResolutionStatus sourceStatus =
                _expressionTriggers == null ||
                sourceIndex >= _expressionTriggers.SlotCount
                    ? ExpressionTriggerResolutionStatus.Unavailable
                    : _expressionTriggers.GetStatus(sourceIndex);
            ExpressionLinkTargetDiagnostic target =
                _expressionLinkRuntime == null
                    ? null
                    : _expressionLinkRuntime.GetDiagnostic(index);
            if (sourceStatus == ExpressionTriggerResolutionStatus.Ready &&
                target != null &&
                target.IsReady)
            {
                return "Ready | " +
                    _expressionTriggers.GetSelector(sourceIndex) +
                    " -> " + target.ResolvedPath + ":" +
                    definition.BlendshapeName;
            }

            string targetStatus = target == null
                ? "Unavailable"
                : target.Status.ToString();
            string message = target == null ||
                string.IsNullOrEmpty(target.Message)
                    ? string.Empty
                    : " | " + target.Message;
            return "Source " + sourceStatus +
                " | Target " + targetStatus + message;
        }

        private string[] BuildExpressionTriggerConfiguration()
        {
            string[] configuredTriggers = new string[
                ExpressionLinkSourceOffset + _savedExpressionLinks.Count];
            _hasEnabledExpressionLinks = false;
            for (int i = 0; i < _savedExpressionTriggers.Length; i++)
            {
                configuredTriggers[i] = _savedExpressionTriggers[i];
            }

            for (int i = 0; i < _savedExpressionLinks.Count; i++)
            {
                ExpressionLinkDefinition link = _savedExpressionLinks[i];
                bool enabled = link != null && link.Enabled;
                _hasEnabledExpressionLinks |= enabled;
                configuredTriggers[ExpressionLinkSourceOffset + i] =
                    enabled ? link.Source : string.Empty;
            }

            return configuredTriggers;
        }

        private void RebuildExpressionLinkRuntime()
        {
            if (_expressionLinkRuntime == null)
            {
                _expressionLinkRuntime = new ExpressionLinkRuntime();
            }

            _expressionLinkRuntime.Rebuild(
                ChaControl,
                _savedExpressionLinks,
                _binding,
                _manualVisibility);
            if (_expressionLinkWeights.Length !=
                _savedExpressionLinks.Count)
            {
                _expressionLinkWeights =
                    new float[_savedExpressionLinks.Count];
                _expressionLinkSourcesReady =
                    new bool[_savedExpressionLinks.Count];
            }
        }

        private bool ExpressionLinksNeedResolutionRetry()
        {
            return _expressionLinkRuntime != null &&
                (_expressionLinkRuntime.HasUnresolvedLinks ||
                 _expressionLinkRuntime.RequiresRebuild);
        }

        private void SynchronizeExpressionLinks()
        {
            if (!_hasEnabledExpressionLinks)
            {
                return;
            }

            for (int i = 0; i < _savedExpressionLinks.Count; i++)
            {
                int sourceIndex = ExpressionLinkSourceOffset + i;
                bool ready = sourceIndex < _expressionTriggers.SlotCount &&
                    _expressionTriggers.GetStatus(sourceIndex) ==
                        ExpressionTriggerResolutionStatus.Ready;
                _expressionLinkSourcesReady[i] = ready;
                _expressionLinkWeights[i] = ready
                    ? _expressionTriggers.GetSlotWeight(sourceIndex)
                    : 0f;
            }

            if (_expressionLinkRuntime == null)
            {
                return;
            }

            float deltaTime = PluginConfig.UseUnscaledTime.Value
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
            if (!_expressionLinkRuntime.Apply(
                    _expressionLinkWeights,
                    _expressionLinkSourcesReady,
                    deltaTime) &&
                _expressionLinkRuntime.RequiresRebuild &&
                _expressionRetryAttemptsRemaining <= 0)
            {
                _expressionRetryAttemptsRemaining = 10;
                _nextExpressionRetryFrame = Time.frameCount + 1;
            }
        }

        private void RestoreExpressionLinks()
        {
            if (_expressionLinkRuntime == null)
            {
                return;
            }

            string expressionLinkError;
            if (!_expressionLinkRuntime.RestoreAll(
                    out expressionLinkError) &&
                expressionLinkError.Length > 0)
            {
                DebugLog(GetCharacterName() + ": " + expressionLinkError);
            }
        }
    }
}
