namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkDraftRules
    {
        internal static bool IsBlank(string value)
        {
            return (value ?? string.Empty).Trim().Length == 0;
        }
    }
}
