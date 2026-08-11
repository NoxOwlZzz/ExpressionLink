using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal struct ManagedWeightState
    {
        internal const float ComparisonEpsilon = 0.001f;

        private bool _hasExpectedValue;
        private float _expectedValue;

        internal bool HasExpectedValue
        {
            get { return _hasExpectedValue; }
        }

        internal float ExpectedValue
        {
            get { return _expectedValue; }
        }

        internal bool TargetChanged(float desiredValue)
        {
            return !_hasExpectedValue ||
                   !NearlyEqual(_expectedValue, desiredValue);
        }

        internal bool CurrentNeedsCorrection(
            float currentValue,
            float desiredValue)
        {
            return !NearlyEqual(currentValue, desiredValue);
        }

        internal void Commit(float valueActuallyLeft)
        {
            _expectedValue = valueActuallyLeft;
            _hasExpectedValue = true;
        }

        internal void Reset()
        {
            _expectedValue = 0f;
            _hasExpectedValue = false;
        }

        internal static bool NearlyEqual(float left, float right)
        {
            return DirectionalMapper.IsFinite(left) &&
                   DirectionalMapper.IsFinite(right) &&
                   Math.Abs(left - right) <= ComparisonEpsilon;
        }
    }
}
