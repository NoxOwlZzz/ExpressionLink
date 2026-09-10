using System;
using System.Reflection;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class SliderHighlightRendererOwnership
    {
        private readonly FieldInfo _faceRenderer;
        private readonly FieldInfo _bodyRenderer;

        private SliderHighlightRendererOwnership(FieldInfo face, FieldInfo body)
        {
            _faceRenderer = face;
            _bodyRenderer = body;
        }

        internal static SliderHighlightRendererOwnership TryCreate(
            Type pluginType, Type rendererType)
        {
            if (pluginType == null || rendererType == null)
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Static |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            FieldInfo face = pluginType.GetField("_smrFac", flags);
            FieldInfo body = pluginType.GetField("_smrBod", flags);
            if (face == null || body == null ||
                face.FieldType != rendererType || body.FieldType != rendererType)
            {
                return null;
            }

            return new SliderHighlightRendererOwnership(face, body);
        }

        internal bool ShouldExclude(
            object renderer, bool exactPathMatch, bool exactNameMatch)
        {
            if (renderer == null || exactPathMatch || exactNameMatch)
            {
                return false;
            }

            // SliderHighlight replaces its overlays on Maker reload. Read the
            // current owners; retaining a renderer would retain a stale clone.
            return ReferenceEquals(renderer, _faceRenderer.GetValue(null)) ||
                ReferenceEquals(renderer, _bodyRenderer.GetValue(null));
        }
    }
}
