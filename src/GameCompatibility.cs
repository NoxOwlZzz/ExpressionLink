namespace NightOwlZzz.Koikatsu.EyeMotion
{
    // These identifiers select the host process and profile compatibility;
    // the plugin GUID and serialized card format remain shared.
    internal static class GameCompatibility
    {
#if KK && KKS
#error Select only one game target.
#elif KKS
        internal const string GameId = "KKS";
        internal const string GameName = "Koikatsu Sunshine";
        internal const string MainProcess = "KoikatsuSunshine.exe";
#elif KK
        internal const string GameId = "KK";
        internal const string GameName = "Koikatsu";
        internal const string MainProcess = "Koikatu.exe";
#else
#error A KK or KKS game target is required.
#endif
    }
}
