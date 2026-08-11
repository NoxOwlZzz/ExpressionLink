using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal struct FloatVisibilityState
    {
        internal ManualVisibilityMode Mode;
        internal bool OwnsValue;
        internal float OriginalValue;
        internal float LastWrittenValue;

        internal void Transition(
            ManualVisibilityMode requested,
            float currentValue,
            float hiddenWeight,
            out bool shouldWrite,
            out float valueToWrite,
            out bool restorationSkipped)
        {
            shouldWrite = false;
            valueToWrite = currentValue;
            restorationSkipped = false;

            if (requested == ManualVisibilityMode.Original)
            {
                if (OwnsValue)
                {
                    if (NearlyEqual(currentValue, LastWrittenValue))
                    {
                        valueToWrite = OriginalValue;
                        shouldWrite = !NearlyEqual(currentValue, OriginalValue);
                    }
                    else
                    {
                        restorationSkipped = true;
                    }
                }

                Mode = ManualVisibilityMode.Original;
                OwnsValue = false;
                return;
            }

            valueToWrite = requested == ManualVisibilityMode.Hidden
                ? ClampWeight(hiddenWeight)
                : 0f;
            shouldWrite = !NearlyEqual(currentValue, valueToWrite);
            if (OwnsValue && !NearlyEqual(currentValue, LastWrittenValue))
            {
                OwnsValue = false;
            }

            if (!OwnsValue && shouldWrite)
            {
                OriginalValue = currentValue;
                OwnsValue = true;
            }

            LastWrittenValue = valueToWrite;
            Mode = requested;
        }

        internal static bool NearlyEqual(float left, float right)
        {
            return Math.Abs(left - right) <= 0.001f;
        }

        private static float ClampWeight(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                return 0f;
            }

            return value > 100f ? 100f : value;
        }
    }

    internal struct BoolVisibilityState
    {
        internal ManualVisibilityMode Mode;
        internal bool OwnsValue;
        internal bool OriginalValue;
        internal bool LastWrittenValue;

        internal void Transition(
            ManualVisibilityMode requested,
            bool currentValue,
            out bool shouldWrite,
            out bool valueToWrite,
            out bool restorationSkipped)
        {
            shouldWrite = false;
            valueToWrite = currentValue;
            restorationSkipped = false;

            if (requested == ManualVisibilityMode.Original)
            {
                if (OwnsValue)
                {
                    if (currentValue == LastWrittenValue)
                    {
                        valueToWrite = OriginalValue;
                        shouldWrite = currentValue != OriginalValue;
                    }
                    else
                    {
                        restorationSkipped = true;
                    }
                }

                Mode = ManualVisibilityMode.Original;
                OwnsValue = false;
                return;
            }

            valueToWrite = requested == ManualVisibilityMode.Visible;
            shouldWrite = currentValue != valueToWrite;
            if (OwnsValue && currentValue != LastWrittenValue)
            {
                OwnsValue = false;
            }

            if (!OwnsValue && shouldWrite)
            {
                OriginalValue = currentValue;
                OwnsValue = true;
            }

            LastWrittenValue = valueToWrite;
            Mode = requested;
        }
    }
}
