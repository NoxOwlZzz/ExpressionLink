namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ConfigChangeClassifier
    {
        internal static bool AffectsEyeBinding(string section, string key)
        {
            return section == "Target Headmod" ||
                   section == "Blendshape Names" ||
                   section == "ExpressionControl Eye Adjustment Blendshapes" ||
                   (section == "Blink" && key == "BlinkEnabled");
        }

        internal static bool AffectsVisibilityDefinitions(string section)
        {
            return section == "Manual Visibility Blendshapes" ||
                   section == "Manual Visibility Renderers";
        }

        internal static bool AffectsVisibilityValues(string section)
        {
            return section == "Manual Visibility";
        }
    }
}
