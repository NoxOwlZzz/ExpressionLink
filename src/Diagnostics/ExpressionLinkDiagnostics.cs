using System.Globalization;
using System.Text;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkDiagnostics
    {
        private static readonly CultureInfo InvariantCulture =
            CultureInfo.InvariantCulture;

        internal static void Append(
            StringBuilder builder,
            EyeMotionCharacterController controller)
        {
            builder.AppendLine("Expression links:");
            if (controller == null)
            {
                builder.AppendLine("  Controller unavailable.");
                return;
            }

            ExpressionLinkDefinition[] links =
                controller.CopyExpressionLinks();
            builder.AppendLine(
                "  Count: " + links.Length.ToString(InvariantCulture));
            builder.AppendLine(
                "  Loaded card schema: " +
                controller.LoadedCardDataVersion.ToString(InvariantCulture));
            for (int i = 0; i < links.Length; i++)
            {
                ExpressionLinkDefinition link = links[i];
                builder.AppendLine(
                    "  Link " + (i + 1).ToString("00", InvariantCulture) +
                    ": " + link.Name);
                builder.AppendLine(
                    "    Enabled / status: " + link.Enabled + " / " +
                    controller.GetExpressionLinkStatus(i));
                builder.AppendLine("    Source: " + link.Source);
                builder.AppendLine(
                    "    Target: " + link.Scope + " slot " +
                    link.SlotIndex.ToString(InvariantCulture) + " / " +
                    (link.RendererPath.Length == 0
                        ? "<auto>"
                        : link.RendererPath) + " / component " +
                    link.ComponentIndex.ToString(InvariantCulture) + " / " +
                    link.BlendshapeName);
                builder.AppendLine(
                    "    Renderer / mesh hints: " +
                    link.RendererHint + " / " + link.MeshHint);
                builder.AppendLine(
                    "    Mode / input / output: " + link.Mode + " / " +
                    Format(link.InputMin) + ".." + Format(link.InputMax) +
                    " / " + Format(link.OutputMin) + ".." +
                    Format(link.OutputMax));
                builder.AppendLine(
                    "    Threshold / smoothing / priority: " +
                    Format(link.Threshold) + " / " +
                    Format(link.SmoothingSpeed) + " / " +
                    link.Priority.ToString(InvariantCulture));
            }
        }

        private static string Format(float value)
        {
            return value.ToString("0.######", InvariantCulture);
        }
    }
}
