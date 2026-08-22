namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class QuickSettingsDraftNormalization
    {
        internal static float Clamp(
            float value,
            float minimum,
            float maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }

        internal static string Name(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        internal static void ValidateIndex(int index, int count)
        {
            if (index < 0 || index >= count)
            {
                throw new System.ArgumentOutOfRangeException("index");
            }
        }
    }
}
