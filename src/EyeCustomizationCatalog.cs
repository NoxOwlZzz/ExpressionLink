namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class EyeCustomizationCatalog
    {
        internal const int ChannelCount = 2;
        internal const int IrisY = 0;
        internal const int IrisSize = 1;

        internal static readonly string[] DisplayNames =
        {
            "IrisY",
            "Size (iris shrink)"
        };

        internal static readonly string[] ConfigKeys =
        {
            "IrisYBlendshape",
            "IrisSizeBlendshape"
        };

        internal static readonly string[] DefaultNames =
        {
            "eye_motion.f00_iris_y",
            "eye_motion.f00_iris_size"
        };
    }
}
