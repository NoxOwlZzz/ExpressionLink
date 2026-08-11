using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using BepInEx;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class Diagnostics
    {
        internal static void WriteReport()
        {
            try
            {
                string directory = Path.Combine(
                    Paths.ConfigPath,
                    Path.Combine("KK_EyeMotion", "diagnostics"));
                Directory.CreateDirectory(directory);

                DateTime now = DateTime.Now;
                string path = Path.Combine(
                    directory,
                    "EyeMotion_" + now.ToString("yyyyMMdd_HHmmss_fff") + ".txt");

                StringBuilder builder = new StringBuilder(32768);
                AppendHeader(builder, now);

                IList<EyeMotionCharacterController> controllers =
                    EyeMotionCharacterController.ActiveControllers;
                builder.AppendLine("Character count: " + controllers.Count);
                builder.AppendLine();

                for (int i = 0; i < controllers.Count; i++)
                {
                    AppendController(builder, controllers[i], i);
                }

                File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
                Plugin.Log.LogInfo("EyeMotion diagnostics written to " + path);
            }
            catch (Exception exception)
            {
                Plugin.Log.LogError(
                    "EyeMotion diagnostic report failed: " +
                    exception.GetType().Name + ": " + exception.Message);
            }
        }

        private static void AppendHeader(StringBuilder builder, DateTime now)
        {
            builder.AppendLine("EyeMotion diagnostics");
            builder.AppendLine("=====================");
            builder.AppendLine("Plugin: " + Plugin.PluginName + " " + Plugin.PluginVersion);
            builder.AppendLine("GUID: " + Plugin.PluginGuid);
            builder.AppendLine("Timestamp (local): " + now.ToString("O"));
            builder.AppendLine("Process: " + GetProcessDescription());
            builder.AppendLine("Unity: " + Application.unityVersion);
            builder.AppendLine("Look capture: " + EyeStateSampler.LookCapturePoint);
            builder.AppendLine("Blink capture: " + EyeStateSampler.BlinkCapturePoint);
            builder.AppendLine(
                "Normal-frame policy: numeric reads, mapping, optional smoothing, " +
                "lightweight identity/highlight-state checks, and managed eye channels. " +
                "Optional ExpressionControl IrisY/Size shapes write only after a source/config change " +
                "and verify on a staggered 64-frame cadence. Expression automation uses " +
                "three cached FBS dictionary reads plus up to four numeric lookups, with " +
                "no per-frame reflection or string/mesh searches. Visibility writes only " +
                "on commands and state transitions.");
            builder.AppendLine();
        }

        private static void AppendController(
            StringBuilder builder,
            EyeMotionCharacterController controller,
            int index)
        {
            builder.AppendLine("Character " + (index + 1));
            builder.AppendLine("--------------------");

            if (controller == null)
            {
                builder.AppendLine("Controller: destroyed");
                builder.AppendLine();
                return;
            }

            try
            {
                ChaControl owner = controller.ChaControl;
                builder.AppendLine("Name: " + controller.GetCharacterName());
                builder.AppendLine("Controller Instance ID: " + controller.GetInstanceID());
                builder.AppendLine(
                    "ChaControl Instance ID: " +
                    (owner == null ? "<destroyed>" : owner.GetInstanceID().ToString()));
                builder.AppendLine(
                    "Head root: " +
                    (owner == null || owner.objHead == null
                        ? "<unavailable>"
                        : GetObjectPath(owner.transform, owner.objHead.transform)));
                builder.AppendLine("Binding state: " + controller.CurrentBindingState);
                builder.AppendLine("Status: " + controller.StatusMessage);
                builder.AppendLine(
                    "Base-game highlight hidden: " +
                    controller.BaseGameHighlightHidden);
                builder.AppendLine(
                    "Highlight sync override active: " +
                    controller.HighlightSyncEffective);
                builder.AppendLine(
                    "Card persistence: " + controller.CardPersistenceStatus);

                AppendEyeState(builder, controller.State, controller.AppliedWeights);
                AppendBinding(builder, controller.Binding);
                AppendManualVisibility(
                    builder,
                    controller.ManualVisibility,
                    controller.Binding);
                AppendExpressionTriggers(builder, controller);
                AppendResolveResult(builder, controller.LastResolve);
            }
            catch (Exception exception)
            {
                builder.AppendLine(
                    "Character diagnostics failed safely: " +
                    exception.GetType().Name + ": " + exception.Message);
            }

            builder.AppendLine();
        }

        private static void AppendEyeState(
            StringBuilder builder,
            EyeState state,
            MappedWeights weights)
        {
            builder.AppendLine("Look available: " + state.LookAvailable);
            builder.AppendLine("Blink available: " + state.BlinkAvailable);
            builder.AppendLine(
                "Horizontal left (native common axis): " +
                Format(state.LeftHorizontal));
            builder.AppendLine(
                "Horizontal right (native common axis): " +
                Format(state.RightHorizontal));
            builder.AppendLine(
                "Horizontal left (common axis): " +
                Format(state.LeftHorizontalCommon));
            builder.AppendLine(
                "Horizontal right (common axis): " +
                Format(state.RightHorizontalCommon));
            builder.AppendLine(
                "Selected raw horizontal: " +
                Format(state.HorizontalSourceRaw));
            builder.AppendLine("Vertical: " + Format(state.Vertical));
            builder.AppendLine(
                "Configured center offset (X, Y): (" +
                Format(PluginConfig.HorizontalCenterOffset.Value) + ", " +
                Format(PluginConfig.VerticalCenterOffset.Value) + ")");
            builder.AppendLine(
                "Final vector: (" +
                Format(state.FinalHorizontal) + ", " +
                Format(state.FinalVertical) + ")");
            builder.AppendLine("Raw blink open rate: " + Format(state.BlinkOpenRate));
            builder.AppendLine("Current effective open: " + Format(state.CurrentOpen));
            builder.AppendLine(
                "Effective maximum open: " + Format(state.EffectiveMaximumOpen));
            builder.AppendLine("Calculated closure: " + Format(state.Closure));
            builder.AppendLine("Last sample frame: " + state.LastSampleFrame);
            builder.AppendLine("Last application frame: " + state.LastApplicationFrame);
            builder.AppendLine(
                "Final weights [PosX, NegX, PosY, NegY, Blink]: [" +
                Format(weights.PositiveX) + ", " +
                Format(weights.NegativeX) + ", " +
                Format(weights.PositiveY) + ", " +
                Format(weights.NegativeY) + ", " +
                Format(weights.Blink) + "]");
        }

        private static void AppendBinding(
            StringBuilder builder,
            BlendshapeBinding binding)
        {
            if (binding == null)
            {
                builder.AppendLine("Active binding: <none>");
                return;
            }

            builder.AppendLine("Active binding:");
            builder.AppendLine("  Relative path: " + binding.RelativePath);
            builder.AppendLine("  Renderer Instance ID: " + binding.RendererInstanceId);
            builder.AppendLine("  Mesh Instance ID: " + binding.MeshInstanceId);
            builder.AppendLine("  Pair still valid: " + binding.IsValid());
            builder.AppendLine(
                "  Managed indices [PosX, NegX, PosY, NegY, Blink]: [" +
                binding.PositiveXIndex + ", " +
                binding.NegativeXIndex + ", " +
                binding.PositiveYIndex + ", " +
                binding.NegativeYIndex + ", " +
                binding.BlinkIndex + "]");
            builder.AppendLine(
                "  Captured initial weights: [" +
                Format(binding.PositiveXInitial) + ", " +
                Format(binding.NegativeXInitial) + ", " +
                Format(binding.PositiveYInitial) + ", " +
                Format(binding.NegativeYInitial) + ", " +
                Format(binding.BlinkInitial) + "]");
            builder.AppendLine(
                "  Optional ExpressionControl eye shapes: " +
                binding.EyeCustomizationShapeCount + "/" +
                EyeCustomizationCatalog.ChannelCount);
            builder.AppendLine(
                "  ExpressionControl source: " +
                binding.EyeAdjustmentSourceStatus);
            builder.AppendLine(
                "  Last ExpressionControl values [IrisY, Size]: [" +
                Format(binding.LastIrisY) + ", " +
                Format(binding.LastIrisSize) + "]");
            builder.Append("  ExpressionControl eye indices: [");
            for (int i = 0; i < binding.EyeCustomizationIndices.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(binding.EyeCustomizationIndices[i]);
            }

            builder.AppendLine("]");
        }

        private static void AppendResolveResult(
            StringBuilder builder,
            ResolveResult result)
        {
            if (result == null)
            {
                builder.AppendLine("Last resolver result: <none>");
                return;
            }

            builder.AppendLine("Last resolver status: " + result.Status);
            builder.AppendLine("Last resolver message: " + result.Message);
            builder.AppendLine(
                "Selected candidate: " +
                (result.SelectedCandidate == null
                    ? "<none>"
                    : result.SelectedCandidate.RelativePath));

            List<RendererCandidate> candidates = result.Candidates;
            int count = candidates == null ? 0 : candidates.Count;
            builder.AppendLine("Renderer candidates inspected: " + count);
            for (int i = 0; i < count; i++)
            {
                AppendCandidate(builder, candidates[i], i);
            }

            List<RendererCandidate> ambiguous = result.AmbiguousCandidates;
            int ambiguousCount = ambiguous == null ? 0 : ambiguous.Count;
            builder.AppendLine("Ambiguous compatible candidates: " + ambiguousCount);
            for (int i = 0; i < ambiguousCount; i++)
            {
                RendererCandidate candidate = ambiguous[i];
                builder.AppendLine(
                    "  - " +
                    (candidate == null ? "<destroyed>" : candidate.RelativePath));
            }
        }

        private static void AppendManualVisibility(
            StringBuilder builder,
            ManualVisibilityBinding visibility,
            BlendshapeBinding eyeBinding)
        {
            builder.AppendLine("Manual visibility:");
            builder.AppendLine(
                "  Total resolver executions (all characters): " +
                ManualVisibilityBinding.TotalResolveCount);
            if (visibility == null)
            {
                builder.AppendLine("  Binding: <none>");
                return;
            }

            builder.AppendLine("  Summary: " + visibility.GetSummary());
            builder.AppendLine(
                "  Hide blendshape weight: " +
                Format(PluginConfig.ManualHideBlendshapeWeight.Value));
            builder.AppendLine("  Fused blendshape slots:");
            for (int i = 0; i < visibility.BlendshapeSlots.Length; i++)
            {
                BlendshapeVisibilitySlot slot = visibility.BlendshapeSlots[i];
                builder.AppendLine("    [" + i + "] " + slot.DisplayName);
                builder.AppendLine("      Configured name: " + slot.ConfiguredName);
                builder.AppendLine("      Resolution: " + slot.Resolution);
                builder.AppendLine("      Blendshape index: " + slot.BlendshapeIndex);
                builder.AppendLine("      Manual mode: " + slot.ManualMode);
                builder.AppendLine("      Effective mode: " + slot.EffectiveMode);
                builder.AppendLine(
                    "      Automatic highlight override: " +
                    slot.AutomaticHidden);
                builder.AppendLine(
                    "      Automatic expression configured / active: " +
                    slot.AutomaticExpressionConfigured + " / " +
                    slot.AutomaticExpressionActive);
                builder.AppendLine("      Owns value: " + slot.State.OwnsValue);
                builder.AppendLine(
                    "      Captured original / last written: " +
                    Format(slot.State.OriginalValue) + " / " +
                    Format(slot.State.LastWrittenValue));
                builder.AppendLine("      Status: " + slot.StatusMessage);

                if (eyeBinding != null &&
                    eyeBinding.IsValid() &&
                    slot.BlendshapeIndex >= 0)
                {
                    try
                    {
                        builder.AppendLine(
                            "      Current weight: " +
                            Format(eyeBinding.Renderer.GetBlendShapeWeight(
                                slot.BlendshapeIndex)));
                    }
                    catch (Exception exception)
                    {
                        builder.AppendLine(
                            "      Current weight unavailable: " +
                            exception.GetType().Name);
                    }
                }
            }

            builder.AppendLine("  Separate renderer slots:");
            for (int i = 0; i < visibility.RendererSlots.Length; i++)
            {
                RendererVisibilitySlot slot = visibility.RendererSlots[i];
                builder.AppendLine("    [" + i + "] " + slot.DisplayName);
                builder.AppendLine("      Configured target: " + slot.ConfiguredTarget);
                builder.AppendLine("      Resolution: " + slot.Resolution);
                builder.AppendLine("      Relative path: " + slot.RelativePath);
                builder.AppendLine(
                    "      Renderer Instance ID: " + slot.RendererInstanceId);
                builder.AppendLine(
                    "      GameObject Instance ID: " + slot.GameObjectInstanceId);
                builder.AppendLine(
                    "      Captured head root Instance ID: " +
                    slot.HeadRootInstanceId);
                builder.AppendLine(
                    "      Identity still valid: " + slot.IsIdentityValid());
                builder.AppendLine(
                    "      Head binding still valid: " + slot.IsValid());
                builder.AppendLine("      Manual mode: " + slot.ManualMode);
                builder.AppendLine("      Effective mode: " + slot.EffectiveMode);
                builder.AppendLine(
                    "      Automatic expression configured / active: " +
                    slot.AutomaticExpressionConfigured + " / " +
                    slot.AutomaticExpressionActive);
                builder.AppendLine("      Owns value: " + slot.State.OwnsValue);
                builder.AppendLine(
                    "      Captured original / last written: " +
                    slot.State.OriginalValue + " / " +
                    slot.State.LastWrittenValue);
                builder.AppendLine("      Status: " + slot.StatusMessage);

                if (slot.IsIdentityValid())
                {
                    try
                    {
                        builder.AppendLine(
                            "      Current enabled: " + slot.Renderer.enabled);
                        builder.AppendLine(
                            "      activeInHierarchy: " +
                            slot.Renderer.gameObject.activeInHierarchy);
                    }
                    catch (Exception exception)
                    {
                        builder.AppendLine(
                            "      Current renderer state unavailable: " +
                            exception.GetType().Name);
                    }
                }
            }
        }

        private static void AppendExpressionTriggers(
            StringBuilder builder,
            EyeMotionCharacterController controller)
        {
            builder.AppendLine("Automatic expression triggers:");
            builder.AppendLine(
                "  Globally enabled: " +
                PluginConfig.ExpressionAutomationEnabled.Value);
            builder.AppendLine(
                "  Activation threshold: " +
                Format(PluginConfig.ExpressionActivationThreshold.Value));
            ExpressionTriggerController runtime = controller.ExpressionTriggers;
            builder.AppendLine(
                "  Runtime available: " +
                (runtime != null && runtime.RuntimeAvailable));
            for (int i = 0; i < ExpressionTriggerSyntax.SlotCount; i++)
            {
                builder.AppendLine(
                    "  ExpressionMesh " + (i + 1).ToString("00") + ":");
                builder.AppendLine(
                    "    Configured trigger: " +
                    controller.GetExpressionTrigger(i));
                if (runtime == null)
                {
                    builder.AppendLine("    Status: Unavailable");
                    continue;
                }

                builder.AppendLine(
                    "    Status: " + runtime.GetStatus(i));
                builder.AppendLine(
                    "    Selector: " + runtime.GetSelector(i));
                builder.AppendLine(
                    "    Active / weight: " + runtime.IsActive(i) + " / " +
                    Format(runtime.GetSlotWeight(i)));
                builder.AppendLine(
                    "    Message: " + runtime.GetMessage(i));
            }

            builder.AppendLine(
                "  Current brow / eyes / mouth: " +
                controller.GetCurrentExpressionSelector(
                    ExpressionTriggerPart.Brow) + " / " +
                controller.GetCurrentExpressionSelector(
                    ExpressionTriggerPart.Eyes) + " / " +
                controller.GetCurrentExpressionSelector(
                    ExpressionTriggerPart.Mouth));
        }

        private static void AppendCandidate(
            StringBuilder builder,
            RendererCandidate candidate,
            int index)
        {
            builder.AppendLine("  Candidate " + (index + 1) + ":");
            if (candidate == null)
            {
                builder.AppendLine("    <destroyed>");
                return;
            }

            builder.AppendLine("    Relative path: " + candidate.RelativePath);
            builder.AppendLine("    GameObject: " + candidate.ObjectName);
            builder.AppendLine("    sharedMesh: " + candidate.MeshName);
            builder.AppendLine("    Renderer Instance ID: " + candidate.RendererInstanceId);
            builder.AppendLine("    Mesh Instance ID: " + candidate.MeshInstanceId);
            builder.AppendLine("    Compatible: " + candidate.Compatible);
            builder.AppendLine("    Exact path match: " + candidate.ExactPathMatch);
            builder.AppendLine("    Exact name match: " + candidate.ExactNameMatch);
            builder.AppendLine("    Decision: " + candidate.Decision);
            builder.AppendLine(
                "    EyeMotion indices [PosX, NegX, PosY, NegY, Blink]: [" +
                candidate.PositiveXIndex + ", " +
                candidate.NegativeXIndex + ", " +
                candidate.PositiveYIndex + ", " +
                candidate.NegativeYIndex + ", " +
                candidate.BlinkIndex + "]");
            builder.AppendLine(
                "    Optional ExpressionControl eye channels: " +
                candidate.EyeCustomizationShapeCount + "/" +
                EyeCustomizationCatalog.ChannelCount);

            Mesh mesh = candidate.Mesh;
            if (mesh == null)
            {
                builder.AppendLine("    Mesh details: <unavailable>");
            }
            else
            {
                try
                {
                    builder.AppendLine("    mesh.isReadable: " + mesh.isReadable);
                    builder.AppendLine("    vertexCount: " + mesh.vertexCount);
                    builder.AppendLine("    blendShapeCount: " + mesh.blendShapeCount);
                    builder.AppendLine("    Complete blendshape list:");
                    int shapeCount = mesh.blendShapeCount;
                    for (int shape = 0; shape < shapeCount; shape++)
                    {
                        builder.AppendLine(
                            "      [" + shape + "] " + mesh.GetBlendShapeName(shape));
                    }
                }
                catch (Exception exception)
                {
                    builder.AppendLine(
                        "    Mesh details failed safely: " +
                        exception.GetType().Name + ": " + exception.Message);
                }
            }

        }

        private static string GetProcessDescription()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.ProcessName + ".exe (" + process.Id + ")";
                }
            }
            catch (Exception exception)
            {
                return "<unavailable: " + exception.GetType().Name + ">";
            }
        }

        private static string GetObjectPath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return "<unavailable>";
            }

            if (root == target)
            {
                return root.name;
            }

            Stack<string> names = new Stack<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                names.Push(current.name);
                current = current.parent;
            }

            if (current != root)
            {
                return "<outside ChaControl>/" + target.name;
            }

            return root.name + "/" + string.Join("/", names.ToArray());
        }

        private static string Format(float value)
        {
            return value.ToString("0.000000");
        }
    }
}
