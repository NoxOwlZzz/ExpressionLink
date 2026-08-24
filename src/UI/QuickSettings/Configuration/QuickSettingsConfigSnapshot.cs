using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsConfigSnapshot
    {
        private readonly bool _motionEnabled;
        private readonly bool _invertX;
        private readonly bool _invertY;
        private readonly bool _smoothingEnabled;
        private readonly float _horizontalCenterOffset;
        private readonly float _positiveXInputLimit;
        private readonly float _negativeXInputLimit;
        private readonly float _positiveXMaxWeight;
        private readonly float _negativeXMaxWeight;
        private readonly float _verticalCenterOffset;
        private readonly float _positiveYInputLimit;
        private readonly float _negativeYInputLimit;
        private readonly float _positiveYMaxWeight;
        private readonly float _negativeYMaxWeight;
        private readonly float _blinkMaxWeight;
        private readonly float _smoothingSpeed;

        private readonly bool _irisEnabled;
        private readonly float _irisYMaxWeight;
        private readonly float _irisSizeMaxWeight;
        private readonly string[] _irisBlendshapeNames;

        private readonly bool _expressionAutomationEnabled;
        private readonly float _expressionActivationThreshold;

        private readonly float _manualHideBlendshapeWeight;
        private readonly bool _followBaseGameHighlightVisibility;
        private readonly bool _cardPersistenceEnabled;
        private readonly string[] _visibilityBlendshapeNames;
        private readonly string[] _visibilityRendererTargets;

        internal QuickSettingsConfigSnapshot(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            ExpressionSettingsDraft expressions,
            VisibilitySettingsDraft visibility)
        {
            _motionEnabled = motion.Enabled;
            _invertX = motion.InvertX;
            _invertY = motion.InvertY;
            _smoothingEnabled = motion.SmoothingEnabled;
            _horizontalCenterOffset = motion.HorizontalCenterOffset;
            _positiveXInputLimit = motion.PositiveXInputLimit;
            _negativeXInputLimit = motion.NegativeXInputLimit;
            _positiveXMaxWeight = motion.PositiveXMaxWeight;
            _negativeXMaxWeight = motion.NegativeXMaxWeight;
            _verticalCenterOffset = motion.VerticalCenterOffset;
            _positiveYInputLimit = motion.PositiveYInputLimit;
            _negativeYInputLimit = motion.NegativeYInputLimit;
            _positiveYMaxWeight = motion.PositiveYMaxWeight;
            _negativeYMaxWeight = motion.NegativeYMaxWeight;
            _blinkMaxWeight = motion.BlinkMaxWeight;
            _smoothingSpeed = motion.SmoothingSpeed;

            _irisEnabled = iris.Enabled;
            _irisYMaxWeight = iris.IrisYMaxWeight;
            _irisSizeMaxWeight = iris.IrisSizeMaxWeight;
            _irisBlendshapeNames = CopyIrisNames(iris);

            _expressionAutomationEnabled = expressions.AutomationEnabled;
            _expressionActivationThreshold = expressions.ActivationThreshold;

            _manualHideBlendshapeWeight =
                visibility.ManualHideBlendshapeWeight;
            _followBaseGameHighlightVisibility =
                visibility.FollowBaseGameHighlightVisibility;
            _cardPersistenceEnabled = visibility.CardPersistenceEnabled;
            _visibilityBlendshapeNames = CopyVisibilityNames(visibility);
            _visibilityRendererTargets = CopyRendererTargets(visibility);
        }

        internal bool Matches(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            ExpressionSettingsDraft expressions,
            VisibilitySettingsDraft visibility)
        {
            return MotionMatches(motion) &&
                IrisMatches(iris) &&
                ExpressionMatches(expressions) &&
                VisibilityMatches(visibility);
        }

        private bool MotionMatches(MotionSettingsDraft draft)
        {
            return _motionEnabled == draft.Enabled &&
                _invertX == draft.InvertX &&
                _invertY == draft.InvertY &&
                _smoothingEnabled == draft.SmoothingEnabled &&
                _horizontalCenterOffset == draft.HorizontalCenterOffset &&
                _positiveXInputLimit == draft.PositiveXInputLimit &&
                _negativeXInputLimit == draft.NegativeXInputLimit &&
                _positiveXMaxWeight == draft.PositiveXMaxWeight &&
                _negativeXMaxWeight == draft.NegativeXMaxWeight &&
                _verticalCenterOffset == draft.VerticalCenterOffset &&
                _positiveYInputLimit == draft.PositiveYInputLimit &&
                _negativeYInputLimit == draft.NegativeYInputLimit &&
                _positiveYMaxWeight == draft.PositiveYMaxWeight &&
                _negativeYMaxWeight == draft.NegativeYMaxWeight &&
                _blinkMaxWeight == draft.BlinkMaxWeight &&
                _smoothingSpeed == draft.SmoothingSpeed;
        }

        private bool IrisMatches(IrisSettingsDraft draft)
        {
            if (_irisEnabled != draft.Enabled ||
                _irisYMaxWeight != draft.IrisYMaxWeight ||
                _irisSizeMaxWeight != draft.IrisSizeMaxWeight ||
                _irisBlendshapeNames.Length != draft.BlendshapeNameCount)
            {
                return false;
            }

            for (int i = 0; i < _irisBlendshapeNames.Length; i++)
            {
                if (!Same(_irisBlendshapeNames[i], draft.GetBlendshapeName(i)))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ExpressionMatches(ExpressionSettingsDraft draft)
        {
            return _expressionAutomationEnabled == draft.AutomationEnabled &&
                _expressionActivationThreshold == draft.ActivationThreshold;
        }

        private bool VisibilityMatches(VisibilitySettingsDraft draft)
        {
            if (_manualHideBlendshapeWeight !=
                    draft.ManualHideBlendshapeWeight ||
                _followBaseGameHighlightVisibility !=
                    draft.FollowBaseGameHighlightVisibility ||
                _cardPersistenceEnabled != draft.CardPersistenceEnabled ||
                _visibilityBlendshapeNames.Length !=
                    draft.BlendshapeNameCount ||
                _visibilityRendererTargets.Length !=
                    draft.RendererTargetCount)
            {
                return false;
            }

            for (int i = 0; i < _visibilityBlendshapeNames.Length; i++)
            {
                if (!Same(
                        _visibilityBlendshapeNames[i],
                        draft.GetBlendshapeName(i)))
                {
                    return false;
                }
            }

            for (int i = 0; i < _visibilityRendererTargets.Length; i++)
            {
                if (!Same(
                        _visibilityRendererTargets[i],
                        draft.GetRendererTarget(i)))
                {
                    return false;
                }
            }

            return true;
        }

        private static string[] CopyIrisNames(IrisSettingsDraft draft)
        {
            string[] values = new string[draft.BlendshapeNameCount];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = draft.GetBlendshapeName(i) ?? string.Empty;
            }

            return values;
        }

        private static string[] CopyVisibilityNames(
            VisibilitySettingsDraft draft)
        {
            string[] values = new string[draft.BlendshapeNameCount];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = draft.GetBlendshapeName(i) ?? string.Empty;
            }

            return values;
        }

        private static string[] CopyRendererTargets(
            VisibilitySettingsDraft draft)
        {
            string[] values = new string[draft.RendererTargetCount];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = draft.GetRendererTarget(i) ?? string.Empty;
            }

            return values;
        }

        private static bool Same(string left, string right)
        {
            return string.Equals(
                left ?? string.Empty,
                right ?? string.Empty,
                StringComparison.Ordinal);
        }
    }
}
