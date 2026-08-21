using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class VisibilityCardData
    {
        internal const int SchemaVersion = 3;
        internal const int PreviousSchemaVersion = 2;
        internal const int LegacySchemaVersion = 1;
        internal const string ModesKey = "manualVisibilityModes";
        internal const string LinksKey = "expressionLinksBinary";
        internal const int ExpressionTriggerCount = 4;
        internal const int ModeCount =
            ManualVisibilityCatalog.BlendshapeCount +
            ManualVisibilityCatalog.RendererCount;

        internal static byte[] Encode(
            ManualVisibilityMode[] blendshapeModes,
            ManualVisibilityMode[] rendererModes)
        {
            if (blendshapeModes == null ||
                blendshapeModes.Length != ManualVisibilityCatalog.BlendshapeCount)
            {
                throw new ArgumentException(
                    "Unexpected blendshape visibility mode count.",
                    "blendshapeModes");
            }

            if (rendererModes == null ||
                rendererModes.Length != ManualVisibilityCatalog.RendererCount)
            {
                throw new ArgumentException(
                    "Unexpected renderer visibility mode count.",
                    "rendererModes");
            }

            byte[] result = new byte[ModeCount];
            for (int i = 0; i < blendshapeModes.Length; i++)
            {
                result[i] = EncodeMode(blendshapeModes[i]);
            }

            for (int i = 0; i < rendererModes.Length; i++)
            {
                result[ManualVisibilityCatalog.BlendshapeCount + i] =
                    EncodeMode(rendererModes[i]);
            }

            return result;
        }

        internal static bool TryDecode(
            int version,
            object encoded,
            ManualVisibilityMode[] blendshapeModes,
            ManualVisibilityMode[] rendererModes,
            out string error)
        {
            error = string.Empty;
            if (version != LegacySchemaVersion &&
                version != PreviousSchemaVersion &&
                version != SchemaVersion)
            {
                error = "Unsupported ExpressionLink card-data version " + version + ".";
                return false;
            }

            byte[] values = encoded as byte[];
            if (values == null || values.Length != ModeCount)
            {
                error = "ExpressionLink card data has an invalid visibility payload.";
                return false;
            }

            if (blendshapeModes == null ||
                blendshapeModes.Length != ManualVisibilityCatalog.BlendshapeCount ||
                rendererModes == null ||
                rendererModes.Length != ManualVisibilityCatalog.RendererCount)
            {
                error = "ExpressionLink visibility destination has an invalid size.";
                return false;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > (byte)ManualVisibilityMode.Hidden)
                {
                    error = "ExpressionLink card data contains an invalid visibility mode.";
                    return false;
                }
            }

            for (int i = 0; i < blendshapeModes.Length; i++)
            {
                blendshapeModes[i] = (ManualVisibilityMode)values[i];
            }

            for (int i = 0; i < rendererModes.Length; i++)
            {
                rendererModes[i] = (ManualVisibilityMode)values[
                    ManualVisibilityCatalog.BlendshapeCount + i];
            }

            return true;
        }

        internal static bool IsDefault(byte[] modes)
        {
            if (modes == null)
            {
                return true;
            }

            for (int i = 0; i < modes.Length; i++)
            {
                if (modes[i] != (byte)ManualVisibilityMode.Original)
                {
                    return false;
                }
            }

            return true;
        }

        internal static string GetExpressionTriggerKey(int index)
        {
            if (index < 0 || index >= ExpressionTriggerCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return "expressionTrigger" + (index + 1).ToString("00");
        }

        internal static string NormalizeExpressionTrigger(object value)
        {
            string text = value as string;
            return text == null ? string.Empty : text.Trim();
        }

        internal static bool HasExpressionTriggers(string[] triggers)
        {
            if (triggers == null)
            {
                return false;
            }

            int count = Math.Min(triggers.Length, ExpressionTriggerCount);
            for (int i = 0; i < count; i++)
            {
                if (!string.IsNullOrEmpty(triggers[i]) &&
                    triggers[i].Trim().Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static byte EncodeMode(ManualVisibilityMode mode)
        {
            return mode >= ManualVisibilityMode.Original &&
                   mode <= ManualVisibilityMode.Hidden
                ? (byte)mode
                : (byte)ManualVisibilityMode.Original;
        }
    }
}
