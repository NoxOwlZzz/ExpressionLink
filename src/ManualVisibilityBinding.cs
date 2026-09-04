using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class BlendshapeVisibilitySlot
    {
        internal readonly int SlotIndex;
        internal readonly string DisplayName;
        internal readonly string ConfiguredName;
        internal int BlendshapeIndex = -1;
        internal VisibilityResolutionStatus Resolution;
        internal string StatusMessage;
        internal FloatVisibilityState State;
        internal ManualVisibilityMode ManualMode;
        internal bool AutomaticHidden;
        internal bool AutomaticExpressionConfigured;
        internal bool AutomaticExpressionActive;
        internal bool SuppressedByExpressionLink;

        internal ManualVisibilityMode EffectiveMode
        {
            get
            {
                if (SuppressedByExpressionLink)
                {
                    return ManualVisibilityMode.Original;
                }

                if (AutomaticHidden)
                {
                    return ManualVisibilityMode.Hidden;
                }

                if (ManualMode != ManualVisibilityMode.Original)
                {
                    return ManualMode;
                }

                return AutomaticExpressionConfigured
                    ? (AutomaticExpressionActive
                        ? ManualVisibilityMode.Visible
                        : ManualVisibilityMode.Hidden)
                    : ManualVisibilityMode.Original;
            }
        }

        internal BlendshapeVisibilitySlot(int index, string configuredName)
        {
            SlotIndex = index;
            DisplayName = ManualVisibilityCatalog.BlendshapeDisplayNames[index];
            ConfiguredName = configuredName ?? string.Empty;
            Resolution = VisibilityResolutionStatus.Disabled;
            StatusMessage = "Not configured.";
            State.Mode = ManualVisibilityMode.Original;
            ManualMode = ManualVisibilityMode.Original;
        }
    }

    internal sealed class RendererVisibilitySlot
    {
        internal readonly int SlotIndex;
        internal readonly string DisplayName;
        internal readonly string ConfiguredTarget;
        internal readonly ChaControl Owner;
        internal Renderer Renderer;
        internal int RendererInstanceId;
        internal int GameObjectInstanceId;
        internal GameObject HeadRoot;
        internal int HeadRootInstanceId;
        internal string RelativePath;
        internal VisibilityResolutionStatus Resolution;
        internal string StatusMessage;
        internal BoolVisibilityState State;
        internal ManualVisibilityMode ManualMode;
        internal bool AutomaticExpressionConfigured;
        internal bool AutomaticExpressionActive;

        internal ManualVisibilityMode EffectiveMode
        {
            get
            {
                if (ManualMode != ManualVisibilityMode.Original)
                {
                    return ManualMode;
                }

                return AutomaticExpressionConfigured
                    ? (AutomaticExpressionActive
                        ? ManualVisibilityMode.Visible
                        : ManualVisibilityMode.Hidden)
                    : ManualVisibilityMode.Original;
            }
        }

        internal RendererVisibilitySlot(
            int index,
            string configuredTarget,
            ChaControl owner)
        {
            SlotIndex = index;
            DisplayName = ManualVisibilityCatalog.RendererDisplayNames[index];
            ConfiguredTarget = configuredTarget ?? string.Empty;
            Owner = owner;
            RelativePath = string.Empty;
            Resolution = VisibilityResolutionStatus.Disabled;
            StatusMessage = "Not configured.";
            State.Mode = ManualVisibilityMode.Original;
            ManualMode = ManualVisibilityMode.Original;
        }

        internal bool IsValid()
        {
            return IsIdentityValid() &&
                   HeadRoot != null &&
                   HeadRoot.GetInstanceID() == HeadRootInstanceId &&
                   Owner != null &&
                   (Owner.objHead == HeadRoot || Owner.objHeadBone == HeadRoot) &&
                   IsUnder(Renderer.transform, HeadRoot);
        }

        internal bool IsIdentityValid()
        {
            return Renderer != null &&
                   Renderer.GetInstanceID() == RendererInstanceId &&
                   Renderer.gameObject != null &&
                   Renderer.gameObject.GetInstanceID() == GameObjectInstanceId;
        }

        private static bool IsUnder(Transform target, GameObject root)
        {
            if (target == null || root == null)
            {
                return false;
            }

            Transform expected = root.transform;
            Transform current = target;
            while (current != null)
            {
                if (current == expected)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }

    internal sealed class ManualVisibilityBinding
    {
        private readonly ChaControl _owner;
        private readonly BlendshapeBinding _eyeBinding;

        internal readonly BlendshapeVisibilitySlot[] BlendshapeSlots;
        internal readonly RendererVisibilitySlot[] RendererSlots;

        internal static int TotalResolveCount { get; private set; }

        private ManualVisibilityBinding(ChaControl owner, BlendshapeBinding eyeBinding)
        {
            _owner = owner;
            _eyeBinding = eyeBinding;
            BlendshapeSlots =
                new BlendshapeVisibilitySlot[ManualVisibilityCatalog.BlendshapeCount];
            RendererSlots =
                new RendererVisibilitySlot[ManualVisibilityCatalog.RendererCount];

            ResolveBlendshapeSlots();
            ResolveRendererSlots();
        }

        internal static ManualVisibilityBinding Resolve(
            ChaControl owner,
            BlendshapeBinding eyeBinding)
        {
            TotalResolveCount++;
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (eyeBinding == null || !eyeBinding.IsValid())
            {
                throw new ArgumentException("A valid EyeMotion binding is required.", "eyeBinding");
            }

            return new ManualVisibilityBinding(owner, eyeBinding);
        }

        internal bool SetBlendshapeMode(
            int slotIndex,
            ManualVisibilityMode mode,
            out string message)
        {
            return SetBlendshapeMode(
                slotIndex,
                mode,
                PluginConfig.ManualHideBlendshapeWeight.Value,
                out message);
        }

        internal bool SetBlendshapeMode(
            int slotIndex,
            ManualVisibilityMode mode,
            float hiddenWeight,
            out string message)
        {
            message = string.Empty;
            if (slotIndex < 0 || slotIndex >= BlendshapeSlots.Length)
            {
                message = "Invalid visibility blendshape index.";
                return false;
            }

            BlendshapeVisibilitySlot slot = BlendshapeSlots[slotIndex];
            if (slot.Resolution != VisibilityResolutionStatus.Ready)
            {
                message = slot.DisplayName + ": " + slot.StatusMessage;
                return false;
            }

            slot.ManualMode = mode;
            return ApplyBlendshapeMode(slot, hiddenWeight, out message);
        }

        internal bool SetAutomaticHighlightHidden(
            bool hidden,
            out string message)
        {
            bool success = true;
            message = string.Empty;
            int count = Math.Min(2, BlendshapeSlots.Length);
            for (int i = 0; i < count; i++)
            {
                BlendshapeVisibilitySlot slot = BlendshapeSlots[i];
                slot.AutomaticHidden = hidden;
                if (slot.Resolution != VisibilityResolutionStatus.Ready)
                {
                    success = false;
                    AppendMessage(
                        ref message,
                        slot.DisplayName + ": " + slot.StatusMessage);
                    continue;
                }

                string slotMessage;
                if (!ApplyBlendshapeMode(
                    slot,
                    PluginConfig.ManualHideBlendshapeWeight.Value,
                    out slotMessage))
                {
                    success = false;
                }

                if (!string.IsNullOrEmpty(slotMessage))
                {
                    AppendMessage(ref message, slotMessage);
                }
            }

            return success;
        }

        private bool ApplyBlendshapeMode(
            BlendshapeVisibilitySlot slot,
            float hiddenWeight,
            out string message)
        {
            message = string.Empty;
            if (!_eyeBinding.IsValid())
            {
                message = slot.DisplayName +
                          ": the renderer or sharedMesh changed.";
                return false;
            }

            try
            {
                float current = _eyeBinding.Renderer.GetBlendShapeWeight(
                    slot.BlendshapeIndex);
                FloatVisibilityState next = slot.State;
                bool shouldWrite;
                float valueToWrite;
                bool restorationSkipped;
                next.Transition(
                    slot.EffectiveMode,
                    current,
                    hiddenWeight,
                    out shouldWrite,
                    out valueToWrite,
                    out restorationSkipped);

                if (shouldWrite)
                {
                    _eyeBinding.Renderer.SetBlendShapeWeight(
                        slot.BlendshapeIndex,
                        valueToWrite);
                }

                slot.State = next;
                slot.StatusMessage = BuildOperationMessage(
                    slot.EffectiveMode,
                    restorationSkipped,
                    shouldWrite);
                if (slot.AutomaticHidden)
                {
                    slot.StatusMessage +=
                        " Hidden by the base-game highlight setting.";
                }
                else if (slot.SuppressedByExpressionLink)
                {
                    slot.StatusMessage +=
                        " Managed by an expression link.";
                }
                else if (slot.AutomaticExpressionConfigured &&
                    slot.ManualMode == ManualVisibilityMode.Original)
                {
                    slot.StatusMessage += slot.AutomaticExpressionActive
                        ? " Shown by the expression trigger."
                        : " Hidden until the expression trigger activates.";
                }

                message = slot.DisplayName + ": " + slot.StatusMessage;
                return !restorationSkipped;
            }
            catch (Exception exception)
            {
                message = slot.DisplayName + ": " +
                          exception.GetType().Name + ": " + exception.Message;
                slot.StatusMessage = message;
                return false;
            }
        }

        internal bool SetRendererMode(
            int slotIndex,
            ManualVisibilityMode mode,
            out string message)
        {
            message = string.Empty;
            if (slotIndex < 0 || slotIndex >= RendererSlots.Length)
            {
                message = "Invalid visibility renderer index.";
                return false;
            }

            RendererVisibilitySlot slot = RendererSlots[slotIndex];
            if (slot.Resolution != VisibilityResolutionStatus.Ready)
            {
                message = slot.DisplayName + ": " + slot.StatusMessage;
                return false;
            }

            slot.ManualMode = mode;
            return ApplyRendererMode(slot, slot.EffectiveMode, out message);
        }

        private bool ApplyRendererMode(
            RendererVisibilitySlot slot,
            ManualVisibilityMode mode,
            out string message)
        {
            message = string.Empty;

            bool targetValid = mode == ManualVisibilityMode.Original
                ? slot.IsIdentityValid()
                : slot.IsValid() && IsSupportedHeadRenderer(slot.Renderer);
            if (!targetValid)
            {
                message = slot.DisplayName +
                          ": the renderer no longer exists or is not a valid target.";
                return false;
            }

            try
            {
                bool current = slot.Renderer.enabled;
                BoolVisibilityState next = slot.State;
                bool shouldWrite;
                bool valueToWrite;
                bool restorationSkipped;
                next.Transition(
                    mode,
                    current,
                    out shouldWrite,
                    out valueToWrite,
                    out restorationSkipped);

                if (shouldWrite)
                {
                    slot.Renderer.enabled = valueToWrite;
                }

                slot.State = next;
                slot.StatusMessage = BuildOperationMessage(
                    mode,
                    restorationSkipped,
                    shouldWrite);
                if (mode == ManualVisibilityMode.Visible &&
                    !slot.Renderer.gameObject.activeInHierarchy)
                {
                    slot.StatusMessage +=
                        " Renderer.enabled is true, but a parent GameObject is inactive.";
                }
                if (slot.AutomaticExpressionConfigured &&
                    slot.ManualMode == ManualVisibilityMode.Original)
                {
                    slot.StatusMessage += slot.AutomaticExpressionActive
                        ? " Shown by the expression trigger."
                        : " Hidden until the expression trigger activates.";
                }

                message = slot.DisplayName + ": " + slot.StatusMessage;
                return !restorationSkipped;
            }
            catch (Exception exception)
            {
                message = slot.DisplayName + ": " +
                          exception.GetType().Name + ": " + exception.Message;
                slot.StatusMessage = message;
                return false;
            }
        }

        internal bool SetAutomaticExpressionState(
            int expressionSlotIndex,
            bool configured,
            bool active,
            out string message)
        {
            return SetAutomaticExpressionState(
                expressionSlotIndex,
                configured,
                active,
                true,
                out message);
        }

        internal bool SetAutomaticExpressionState(
            int expressionSlotIndex,
            bool configured,
            bool active,
            bool manageFusedBlendshape,
            out string message)
        {
            if (expressionSlotIndex < 0 ||
                expressionSlotIndex >= ManualVisibilityCatalog.RendererCount)
            {
                message = "Invalid automatic expression slot index.";
                return false;
            }

            bool success = true;
            message = string.Empty;
            if (expressionSlotIndex <
                ManualVisibilityCatalog.LegacyFusedBlendshapeCount)
            {
                int fusedIndex =
                    ManualVisibilityCatalog.LegacyFusedBlendshapeStartIndex +
                    expressionSlotIndex;
                BlendshapeVisibilitySlot blendshape =
                    BlendshapeSlots[fusedIndex];
                bool previouslyManaged =
                    blendshape.AutomaticExpressionConfigured;
                bool suppressFusedBlendshape =
                    !manageFusedBlendshape;
                bool suppressionChanged =
                    blendshape.SuppressedByExpressionLink !=
                    suppressFusedBlendshape;
                blendshape.SuppressedByExpressionLink =
                    suppressFusedBlendshape;
                blendshape.AutomaticExpressionConfigured =
                    manageFusedBlendshape && configured;
                blendshape.AutomaticExpressionActive =
                    manageFusedBlendshape && active;
                if (blendshape.Resolution == VisibilityResolutionStatus.Ready)
                {
                    string slotMessage = string.Empty;
                    if ((manageFusedBlendshape ||
                         previouslyManaged ||
                         suppressionChanged) &&
                        !ApplyBlendshapeMode(
                            blendshape,
                            PluginConfig.ManualHideBlendshapeWeight.Value,
                            out slotMessage))
                    {
                        success = false;
                    }

                    AppendMessage(ref message, slotMessage);
                }
            }

            RendererVisibilitySlot renderer =
                RendererSlots[expressionSlotIndex];
            renderer.AutomaticExpressionConfigured = configured;
            renderer.AutomaticExpressionActive = active;
            if (renderer.Resolution == VisibilityResolutionStatus.Ready)
            {
                string slotMessage;
                if (!ApplyRendererMode(
                    renderer,
                    renderer.EffectiveMode,
                    out slotMessage))
                {
                    success = false;
                }

                AppendMessage(ref message, slotMessage);
            }

            return success;
        }

        internal bool ReapplyBlendshapeOverrides(out string message)
        {
            bool success = true;
            message = string.Empty;
            for (int i = 0; i < BlendshapeSlots.Length; i++)
            {
                BlendshapeVisibilitySlot slot = BlendshapeSlots[i];
                ManualVisibilityMode mode = slot.EffectiveMode;
                if (mode == ManualVisibilityMode.Original)
                {
                    continue;
                }

                string slotMessage;
                if (!ApplyBlendshapeMode(
                    slot,
                    PluginConfig.ManualHideBlendshapeWeight.Value,
                    out slotMessage))
                {
                    success = false;
                    AppendMessage(ref message, slotMessage);
                }
            }

            return success;
        }

        internal bool RestoreAll(out string message)
        {
            bool success = true;
            message = string.Empty;

            for (int i = 0; i < BlendshapeSlots.Length; i++)
            {
                BlendshapeVisibilitySlot slot = BlendshapeSlots[i];
                slot.ManualMode = ManualVisibilityMode.Original;
                slot.AutomaticHidden = false;
                slot.AutomaticExpressionConfigured = false;
                slot.AutomaticExpressionActive = false;
                slot.SuppressedByExpressionLink = false;
                if (!slot.State.OwnsValue &&
                    slot.State.Mode == ManualVisibilityMode.Original)
                {
                    continue;
                }

                string slotMessage;
                if (!ApplyBlendshapeMode(
                    slot,
                    PluginConfig.ManualHideBlendshapeWeight.Value,
                    out slotMessage))
                {
                    success = false;
                    AppendMessage(ref message, slotMessage);
                }
            }

            for (int i = 0; i < RendererSlots.Length; i++)
            {
                RendererVisibilitySlot slot = RendererSlots[i];
                slot.ManualMode = ManualVisibilityMode.Original;
                slot.AutomaticExpressionConfigured = false;
                slot.AutomaticExpressionActive = false;
                if (!slot.State.OwnsValue &&
                    slot.State.Mode == ManualVisibilityMode.Original)
                {
                    continue;
                }

                string slotMessage;
                if (!ApplyRendererMode(
                    slot,
                    ManualVisibilityMode.Original,
                    out slotMessage))
                {
                    success = false;
                    AppendMessage(ref message, slotMessage);
                }
            }

            return success;
        }

        internal string GetSummary()
        {
            int readyBlendshapes = 0;
            int readyRenderers = 0;
            for (int i = 0; i < BlendshapeSlots.Length; i++)
            {
                if (BlendshapeSlots[i].Resolution == VisibilityResolutionStatus.Ready)
                {
                    readyBlendshapes++;
                }
            }

            for (int i = 0; i < RendererSlots.Length; i++)
            {
                if (RendererSlots[i].Resolution == VisibilityResolutionStatus.Ready)
                {
                    readyRenderers++;
                }
            }

            return readyBlendshapes + "/" + BlendshapeSlots.Length +
                   " hide blendshapes, " + readyRenderers + "/" +
                   RendererSlots.Length + " expression renderers.";
        }

        private void ResolveBlendshapeSlots()
        {
            Mesh mesh = _eyeBinding.Mesh;
            for (int i = 0; i < BlendshapeSlots.Length; i++)
            {
                string configured =
                    PluginConfig.GetManualVisibilityBlendshapeName(i).Trim();
                BlendshapeVisibilitySlot slot =
                    new BlendshapeVisibilitySlot(i, configured);
                BlendshapeSlots[i] = slot;

                if (configured.Length == 0)
                {
                    continue;
                }

                int index = mesh.GetBlendShapeIndex(configured);
                if (index < 0)
                {
                    slot.Resolution = VisibilityResolutionStatus.Missing;
                    slot.StatusMessage = "Not found on the bound sharedMesh.";
                    continue;
                }

                slot.BlendshapeIndex = index;
                if (IsEyeMotionIndex(index))
                {
                    slot.Resolution = VisibilityResolutionStatus.Conflict;
                    slot.StatusMessage =
                        "Conflicts with X/Y/Blink and will not be managed.";
                    continue;
                }

                slot.Resolution = VisibilityResolutionStatus.Ready;
                slot.StatusMessage = "Ready.";
            }

            for (int i = 0; i < BlendshapeSlots.Length; i++)
            {
                BlendshapeVisibilitySlot left = BlendshapeSlots[i];
                if (left.Resolution != VisibilityResolutionStatus.Ready)
                {
                    continue;
                }

                for (int j = i + 1; j < BlendshapeSlots.Length; j++)
                {
                    BlendshapeVisibilitySlot right = BlendshapeSlots[j];
                    if (right.Resolution != VisibilityResolutionStatus.Ready ||
                        left.BlendshapeIndex != right.BlendshapeIndex)
                    {
                        continue;
                    }

                    left.Resolution = VisibilityResolutionStatus.Conflict;
                    right.Resolution = VisibilityResolutionStatus.Conflict;
                    left.StatusMessage = "Duplicate index with " + right.DisplayName + ".";
                    right.StatusMessage = "Duplicate index with " + left.DisplayName + ".";
                }
            }
        }

        private void ResolveRendererSlots()
        {
            Renderer[] allRenderers = CollectHeadRenderers();

            for (int i = 0; i < RendererSlots.Length; i++)
            {
                string configured =
                    PluginConfig.GetManualVisibilityRendererTarget(i).Trim();
                RendererVisibilitySlot slot =
                    new RendererVisibilitySlot(i, configured, _owner);
                RendererSlots[i] = slot;

                if (configured.Length == 0)
                {
                    continue;
                }

                List<Renderer> matches = new List<Renderer>();
                if (IsPath(configured))
                {
                    string normalized = NormalizeConfiguredPath(configured);
                    for (int rendererIndex = 0;
                        rendererIndex < allRenderers.Length;
                        rendererIndex++)
                    {
                        Renderer renderer = allRenderers[rendererIndex];
                        if (renderer != null &&
                            IsSupportedHeadRenderer(renderer) &&
                            string.Equals(
                                GetRelativePath(
                                    _owner.transform,
                                    renderer.transform),
                                normalized,
                                StringComparison.Ordinal))
                        {
                            matches.Add(renderer);
                        }
                    }
                }
                else
                {
                    for (int rendererIndex = 0;
                        rendererIndex < allRenderers.Length;
                        rendererIndex++)
                    {
                        Renderer renderer = allRenderers[rendererIndex];
                        if (renderer != null &&
                            string.Equals(
                                renderer.gameObject.name,
                                configured,
                                StringComparison.Ordinal) &&
                            IsSupportedHeadRenderer(renderer))
                        {
                            matches.Add(renderer);
                        }
                    }
                }

                if (matches.Count == 0)
                {
                    slot.Resolution = VisibilityResolutionStatus.Missing;
                    slot.StatusMessage = "No valid mesh Renderer was found.";
                    continue;
                }

                if (matches.Count > 1)
                {
                    slot.Resolution = VisibilityResolutionStatus.Ambiguous;
                    slot.StatusMessage =
                        "Matches " + matches.Count +
                        " renderers; configure an exact path.";
                    continue;
                }

                Renderer selected = matches[0];
                if (selected == _eyeBinding.Renderer)
                {
                    slot.Resolution = VisibilityResolutionStatus.Conflict;
                    slot.StatusMessage =
                        "Targets the main renderer; use a separate mesh.";
                    continue;
                }

                slot.Renderer = selected;
                slot.RendererInstanceId = selected.GetInstanceID();
                slot.GameObjectInstanceId = selected.gameObject.GetInstanceID();
                slot.HeadRoot = GetContainingHeadRoot(selected.transform);
                slot.HeadRootInstanceId = slot.HeadRoot.GetInstanceID();
                slot.RelativePath = GetRelativePath(
                    _owner.transform,
                    selected.transform);
                slot.Resolution = VisibilityResolutionStatus.Ready;
                slot.StatusMessage = "Ready.";
            }

            for (int i = 0; i < RendererSlots.Length; i++)
            {
                RendererVisibilitySlot left = RendererSlots[i];
                if (left.Resolution != VisibilityResolutionStatus.Ready)
                {
                    continue;
                }

                for (int j = i + 1; j < RendererSlots.Length; j++)
                {
                    RendererVisibilitySlot right = RendererSlots[j];
                    if (right.Resolution != VisibilityResolutionStatus.Ready ||
                        left.Renderer != right.Renderer)
                    {
                        continue;
                    }

                    left.Resolution = VisibilityResolutionStatus.Conflict;
                    right.Resolution = VisibilityResolutionStatus.Conflict;
                    left.StatusMessage = "Duplicate Renderer with " + right.DisplayName + ".";
                    right.StatusMessage = "Duplicate Renderer with " + left.DisplayName + ".";
                }
            }
        }

        private bool IsEyeMotionIndex(int index)
        {
            return _eyeBinding.IsManagedIndex(index);
        }

        private bool IsSupportedHeadRenderer(Renderer renderer)
        {
            if (renderer == null ||
                (!(renderer is SkinnedMeshRenderer) && !(renderer is MeshRenderer)))
            {
                return false;
            }

            SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
            if (skinned != null && skinned.sharedMesh == null)
            {
                return false;
            }

            MeshRenderer meshRenderer = renderer as MeshRenderer;
            if (meshRenderer != null)
            {
                MeshFilter filter = meshRenderer.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null)
                {
                    return false;
                }
            }

            if (renderer.GetComponentInParent<ChaControl>() != _owner)
            {
                return false;
            }

            return IsUnder(renderer.transform, _owner.objHead) ||
                   IsUnder(renderer.transform, _owner.objHeadBone);
        }

        private Renderer[] CollectHeadRenderers()
        {
            List<Renderer> renderers = new List<Renderer>();
            AddUniqueRenderers(_owner.objHead, renderers);
            AddUniqueRenderers(_owner.objHeadBone, renderers);
            return renderers.ToArray();
        }

        private static void AddUniqueRenderers(
            GameObject root,
            List<Renderer> destination)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] found = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < found.Length; i++)
            {
                Renderer renderer = found[i];
                if (renderer != null && !destination.Contains(renderer))
                {
                    destination.Add(renderer);
                }
            }
        }

        private GameObject GetContainingHeadRoot(Transform target)
        {
            if (IsUnder(target, _owner.objHead))
            {
                return _owner.objHead;
            }

            return IsUnder(target, _owner.objHeadBone)
                ? _owner.objHeadBone
                : null;
        }

        private string NormalizeConfiguredPath(string configured)
        {
            string path = configured.Trim().Replace('\\', '/').Trim('/');
            string rootPrefix = _owner.transform.name + "/";
            if (path.StartsWith(rootPrefix, StringComparison.Ordinal))
            {
                path = path.Substring(rootPrefix.Length);
            }

            return path;
        }

        private static bool IsPath(string configured)
        {
            return configured.IndexOf('/') >= 0 || configured.IndexOf('\\') >= 0;
        }

        private static bool IsUnder(Transform target, GameObject root)
        {
            if (target == null || root == null)
            {
                return false;
            }

            Transform expected = root.transform;
            Transform current = target;
            while (current != null)
            {
                if (current == expected)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return string.Empty;
            }

            Stack<string> names = new Stack<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return current == root
                ? string.Join("/", names.ToArray())
                : "<outside ChaControl>";
        }

        private static string BuildOperationMessage(
            ManualVisibilityMode mode,
            bool restorationSkipped,
            bool wroteValue)
        {
            if (restorationSkipped)
            {
                return "Restore skipped because another system changed the managed value.";
            }

            if (mode == ManualVisibilityMode.Original)
            {
                return wroteValue
                    ? "Original value restored."
                    : "Original state; no write was needed.";
            }

            return wroteValue
                ? mode + " state applied."
                : mode + " state was already applied.";
        }

        private static void AppendMessage(ref string destination, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (destination.Length > 0)
            {
                destination += " | ";
            }

            destination += message;
        }
    }
}
