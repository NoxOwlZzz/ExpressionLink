using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ManualVisibilityRuntimeView
    {
        private readonly VisibilitySettingsDraft _draft;
        private int _controllerInstanceId;
        private string _feedback = string.Empty;

        internal ManualVisibilityRuntimeView(VisibilitySettingsDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            _draft = draft;
        }

        internal void Draw(
            EyeMotionCharacterController controller,
            bool compactLayout)
        {
            ObserveController(controller);
            if (controller == null)
            {
                QuickSettingsGui.Help("No character selected.");
                return;
            }

            ManualVisibilityBinding binding = controller.ManualVisibility;
            if (binding == null)
            {
                QuickSettingsGui.Help("Part controls are not ready.");
                return;
            }

            QuickSettingsGui.Heading("Blendshape-controlled parts");
            for (int i = 0; i < binding.BlendshapeSlots.Length; i++)
            {
                BlendshapeVisibilitySlot slot = binding.BlendshapeSlots[i];
                DrawVisibilityRow(
                    controller,
                    false,
                    i,
                    GetBlendshapeDisplayName(i, slot.DisplayName),
                    slot.ManualMode,
                    GetResolutionText(slot.Resolution),
                    compactLayout);
            }

            QuickSettingsGui.Space(4f);
            QuickSettingsGui.Heading("Separate expression meshes");
            for (int i = 0; i < binding.RendererSlots.Length; i++)
            {
                RendererVisibilitySlot slot = binding.RendererSlots[i];
                string resolution = GetResolutionText(slot.Resolution);
                if (slot.Resolution == VisibilityResolutionStatus.Ready &&
                    !slot.IsValid())
                {
                    resolution = "Invalid";
                }
                else if (
                    slot.Resolution == VisibilityResolutionStatus.Ready &&
                    slot.Renderer != null &&
                    !slot.Renderer.gameObject.activeInHierarchy)
                {
                    resolution = "Ready/inactive";
                }

                DrawVisibilityRow(
                    controller,
                    true,
                    i,
                    slot.DisplayName,
                    slot.ManualMode,
                    resolution,
                    compactLayout);
            }

            QuickSettingsGui.BeginHorizontal();
            if (QuickSettingsGui.Button("Restore all"))
            {
                if (controller.RestoreManualVisibility(out _feedback) &&
                    _feedback.Length == 0)
                {
                    _feedback = "Game visibility restored.";
                }
            }

            if (QuickSettingsGui.Button("Scan again"))
            {
                controller.RefreshManualVisibility(out _feedback);
            }

            QuickSettingsGui.EndHorizontal();
            if (_feedback.Length > 0)
            {
                QuickSettingsGui.Help(_feedback);
            }
        }

        private void ObserveController(
            EyeMotionCharacterController controller)
        {
            int instanceId = controller == null
                ? 0
                : controller.GetInstanceID();
            if (_controllerInstanceId == instanceId)
            {
                return;
            }

            _controllerInstanceId = instanceId;
            _feedback = string.Empty;
        }

        private void DrawVisibilityRow(
            EyeMotionCharacterController controller,
            bool rendererSlot,
            int slotIndex,
            string label,
            ManualVisibilityMode mode,
            string status,
            bool compactLayout)
        {
            if (compactLayout)
            {
                QuickSettingsGui.BeginVertical();
                QuickSettingsGui.Label(label + " - " + status);
                QuickSettingsGui.BeginHorizontal();
                DrawVisibilityButtons(
                    controller,
                    rendererSlot,
                    slotIndex,
                    mode);
                QuickSettingsGui.EndHorizontal();
                QuickSettingsGui.EndVertical();
                return;
            }

            QuickSettingsGui.BeginPropertyRow();
            QuickSettingsGui.VisibilityLabel(label);
            DrawVisibilityButtons(
                controller,
                rendererSlot,
                slotIndex,
                mode);
            QuickSettingsGui.VisibilityStatus(status);
            QuickSettingsGui.EndPropertyRow();
        }

        private void DrawVisibilityButtons(
            EyeMotionCharacterController controller,
            bool rendererSlot,
            int slotIndex,
            ManualVisibilityMode mode)
        {
            if (QuickSettingsGui.OriginalSegmentButton(
                    "Game default",
                    mode == ManualVisibilityMode.Original))
            {
                SetVisibilityMode(
                    controller,
                    rendererSlot,
                    slotIndex,
                    ManualVisibilityMode.Original);
            }

            if (QuickSettingsGui.VisibilitySegmentButton(
                    "Show",
                    mode == ManualVisibilityMode.Visible))
            {
                SetVisibilityMode(
                    controller,
                    rendererSlot,
                    slotIndex,
                    ManualVisibilityMode.Visible);
            }

            if (QuickSettingsGui.VisibilitySegmentButton(
                    "Hide",
                    mode == ManualVisibilityMode.Hidden))
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
                    out _feedback);
                return;
            }

            controller.SetManualBlendshapeVisibility(
                slotIndex,
                mode,
                Mathf.Clamp(
                    _draft.ManualHideBlendshapeWeight,
                    0f,
                    100f),
                out _feedback);
        }

        private static string GetBlendshapeDisplayName(
            int slotIndex,
            string fallback)
        {
            return slotIndex >= 0 &&
                slotIndex < ManualVisibilityCatalog.BlendshapeDisplayNames.Length
                    ? ManualVisibilityCatalog.BlendshapeDisplayNames[slotIndex]
                    : fallback;
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
    }
}
