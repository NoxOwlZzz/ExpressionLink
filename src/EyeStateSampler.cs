using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class EyeStateSampler
    {
        private delegate float FloatFieldGetter(FBSBase instance);

        private static FloatFieldGetter _openRateGetter;
        private static FloatFieldGetter _correctOpenMaxGetter;

        internal const string LookCapturePoint =
            "EyeMotionCharacterController.LateUpdate (DefaultExecutionOrder 32000) -> EyeLookCalc.GetAngleHRate/GetAngleVRate";

        internal const string BlinkCapturePoint =
            "EyeMotionCharacterController.LateUpdate (DefaultExecutionOrder 32000) -> FBSCtrlEyes effective openness after FaceBlendShape.LateUpdate";

        internal static bool BlinkSamplingAvailable
        {
            get { return _openRateGetter != null && _correctOpenMaxGetter != null; }
        }

        internal static bool Initialize(out string error)
        {
            try
            {
                _openRateGetter = CreateFloatGetter("openRate");
                _correctOpenMaxGetter = CreateFloatGetter("correctOpenMax");
                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                _openRateGetter = null;
                _correctOpenMaxGetter = null;
                error = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        internal static void Sample(
            ChaControl owner,
            int frame,
            bool sampleBlink,
            ref EyeState state)
        {
            state.LastSampleFrame = frame;
            SampleLook(owner, ref state);
            if (sampleBlink)
            {
                SampleBlink(owner, ref state);
            }
            else
            {
                ResetBlink(ref state);
            }
        }

        private static void SampleLook(ChaControl owner, ref EyeState state)
        {
            state.LookAvailable = false;
            state.LeftHorizontal = 0f;
            state.RightHorizontal = 0f;
            state.LeftHorizontalCommon = 0f;
            state.RightHorizontalCommon = 0f;
            state.HorizontalSourceRaw = 0f;
            state.Vertical = 0f;
            state.FinalHorizontal = 0f;
            state.FinalVertical = 0f;

            if (owner == null || owner.eyeLookCtrl == null || owner.eyeLookCtrl.eyeLookScript == null)
            {
                return;
            }

            EyeLookCalc calculator = owner.eyeLookCtrl.eyeLookScript;
            float left = DirectionalMapper.EyeHorizontalToCommon(
                calculator.GetAngleHRate(EYE_LR.EYE_L));
            float right = DirectionalMapper.EyeHorizontalToCommon(
                calculator.GetAngleHRate(EYE_LR.EYE_R));
            float vertical = DirectionalMapper.SanitizeSignedUnit(calculator.GetAngleVRate());

            state.LeftHorizontal = left;
            state.RightHorizontal = right;
            float leftCommon = left;
            float rightCommon = right;
            state.LeftHorizontalCommon = leftCommon;
            state.RightHorizontalCommon = rightCommon;
            state.Vertical = vertical;

            float horizontal;
            switch (PluginConfig.SourceEyeMode.Value)
            {
                case SourceEyeMode.Left:
                    horizontal = leftCommon;
                    break;
                case SourceEyeMode.Right:
                    horizontal = rightCommon;
                    break;
                default:
                    horizontal = (leftCommon + rightCommon) * 0.5f;
                    break;
            }

            state.HorizontalSourceRaw = DirectionalMapper.SanitizeSignedUnit(horizontal);

            horizontal = DirectionalMapper.CenterSignedInput(
                horizontal,
                PluginConfig.HorizontalCenterOffset.Value);
            vertical = DirectionalMapper.CenterSignedInput(
                vertical,
                PluginConfig.VerticalCenterOffset.Value);

            if (PluginConfig.InvertX.Value)
            {
                horizontal = -horizontal;
            }

            if (PluginConfig.InvertY.Value)
            {
                vertical = -vertical;
            }

            if (PluginConfig.ClampInputToUnitCircle.Value)
            {
                DirectionalMapper.ClampToUnitCircle(ref horizontal, ref vertical);
            }

            state.FinalHorizontal = DirectionalMapper.SanitizeSignedUnit(horizontal);
            state.FinalVertical = DirectionalMapper.SanitizeSignedUnit(vertical);
            state.LookAvailable = true;
        }

        private static void SampleBlink(ChaControl owner, ref EyeState state)
        {
            ResetBlink(ref state);

            if (_openRateGetter == null ||
                _correctOpenMaxGetter == null ||
                owner == null ||
                owner.fbsCtrl == null ||
                owner.fbsCtrl.EyesCtrl == null)
            {
                return;
            }

            FBSCtrlEyes eyes = owner.fbsCtrl.EyesCtrl;
            float openRate = DirectionalMapper.Clamp01(_openRateGetter(eyes));
            float correctedMaximum = _correctOpenMaxGetter(eyes);
            float effectiveMaximum = correctedMaximum < 0f ? eyes.OpenMax : correctedMaximum;
            effectiveMaximum = DirectionalMapper.Clamp01(effectiveMaximum);

            float currentOpen = Mathf.Lerp(eyes.OpenMin, effectiveMaximum, openRate);
            if (eyes.FixedRate >= 0f)
            {
                currentOpen = eyes.FixedRate;
            }

            currentOpen = DirectionalMapper.Clamp01(currentOpen);
            float normalizedOpen;
            if (effectiveMaximum > 0.00001f)
            {
                normalizedOpen = DirectionalMapper.Clamp01(currentOpen / effectiveMaximum);
            }
            else
            {
                normalizedOpen = currentOpen <= 0.00001f
                    ? 0f
                    : DirectionalMapper.Clamp01(currentOpen);
            }

            state.BlinkOpenRate = openRate;
            state.CurrentOpen = currentOpen;
            state.EffectiveMaximumOpen = effectiveMaximum;
            state.Closure = DirectionalMapper.Clamp01(1f - normalizedOpen);
            state.BlinkAvailable = true;
        }

        private static void ResetBlink(ref EyeState state)
        {
            state.BlinkAvailable = false;
            state.BlinkOpenRate = 1f;
            state.CurrentOpen = 1f;
            state.EffectiveMaximumOpen = 1f;
            state.Closure = 0f;
        }

        private static FloatFieldGetter CreateFloatGetter(string fieldName)
        {
            FieldInfo field = typeof(FBSBase).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new MissingFieldException(typeof(FBSBase).FullName, fieldName);
            }

            if (field.FieldType != typeof(float))
            {
                throw new InvalidOperationException(fieldName + " is not a Single.");
            }

            DynamicMethod method = new DynamicMethod(
                "EyeMotion_Get_" + fieldName,
                typeof(float),
                new[] { typeof(FBSBase) },
                typeof(EyeStateSampler),
                true);

            ILGenerator generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, field);
            generator.Emit(OpCodes.Ret);

            return (FloatFieldGetter)method.CreateDelegate(typeof(FloatFieldGetter));
        }
    }
}
