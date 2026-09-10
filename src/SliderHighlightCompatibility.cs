using System;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class SliderHighlightCompatibility
    {
        private static Type _pluginType;
        private static SliderHighlightRendererOwnership _ownership;
        private static bool _warningLogged;

        internal static bool ShouldExclude(
            SkinnedMeshRenderer renderer, bool exactPathMatch, bool exactNameMatch)
        {
            if (renderer == null || exactPathMatch || exactNameMatch)
            {
                return false;
            }

            PluginInfo info;
            if (!Chainloader.PluginInfos.TryGetValue("SliderHighlight", out info) ||
                info.Instance == null)
            {
                return false;
            }

            try
            {
                Type currentType = info.Instance.GetType();
                if (_pluginType != currentType)
                {
                    _pluginType = currentType;
                    _warningLogged = false;
                    // SliderHighlight 2.2 exposes no public ownership API.
                    // Validate its private layout and match exact owned objects,
                    // never names that could also belong to a headmod.
                    _ownership = currentType.FullName == "SliderHighlight.SliderHighlightPlugin"
                        ? SliderHighlightRendererOwnership.TryCreate(
                            currentType, typeof(SkinnedMeshRenderer))
                        : null;
                    if (_ownership == null)
                    {
                        WarnOnce("The renderer ownership layout is unavailable.");
                    }
                }

                return _ownership != null && _ownership.ShouldExclude(
                    renderer, exactPathMatch, exactNameMatch);
            }
            catch (Exception exception)
            {
                _ownership = null;
                WarnOnce("Reading renderer ownership failed: " + exception.GetType().Name + ".");
                return false;
            }
        }

        private static void WarnOnce(string reason)
        {
            if (_warningLogged || Plugin.Log == null)
            {
                return;
            }

            _warningLogged = true;
            Plugin.Log.LogWarning(
                "SliderHighlight automatic overlay exclusion is unavailable. " + reason +
                " Eye target detection remains active; select TargetRendererName or " +
                "TargetRendererPath if multiple compatible renderers are reported.");
        }
    }
}
