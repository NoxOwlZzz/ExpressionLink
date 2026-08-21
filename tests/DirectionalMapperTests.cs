using System;

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class DirectionalMapperTests
    {
        private static int _checks;
        private static int _failures;

        private static int Main()
        {
            TestDirectionalMapping();
            TestInvalidDirectionalInputs();
            TestUnitCircleClamp();
            TestSmoothing();
            TestManagedWeightState();
            TestEyeCustomizationMapping();
            TestExpressionTriggerSyntax();
            TestSanitization();
            TestCenterOffsets();
            TestEyeCommonAxisRates();
            TestManualVisibilityStateMachines();
            TestConfigChangeClassification();
            TestVisibilityCardData();
            int expressionLinkChecks;
            int expressionLinkFailures =
                ExpressionLinkCoreTests.Run(out expressionLinkChecks);

            int smokeChecks;
            int smokeFailures = LocalApiSmokeTests.Run(out smokeChecks);

            Console.WriteLine(
                "DirectionalMapper checks: " + _checks +
                ", failures: " + _failures);
            Console.WriteLine(
                "Expression Link core checks: " + expressionLinkChecks +
                ", failures: " + expressionLinkFailures);
            Console.WriteLine(
                "Local API smoke checks: " + smokeChecks +
                ", failures: " + smokeFailures);
            return _failures == 0 && expressionLinkFailures == 0 &&
                smokeFailures == 0 ? 0 : 1;
        }

        private static void TestDirectionalMapping()
        {
            Equal("zero", 0f, DirectionalMapper.MapDirectional(0f, 0f, 1f, 100f, 1f));
            Equal("dead-zone edge", 0f, DirectionalMapper.MapDirectional(0.2f, 0.2f, 1f, 100f, 1f));
            Equal("linear midpoint", 50f, DirectionalMapper.MapDirectional(0.6f, 0.2f, 1f, 100f, 1f));
            Equal("input saturation", 80f, DirectionalMapper.MapDirectional(2f, 0f, 0.5f, 80f, 1f));
            Equal("weight clamp", 100f, DirectionalMapper.MapDirectional(1f, 0f, 1f, 200f, 1f));
            Equal("gamma curve", 25f, DirectionalMapper.MapDirectional(0.5f, 0f, 1f, 100f, 2f));
            Equal("gamma fallback", 50f, DirectionalMapper.MapDirectional(0.5f, 0f, 1f, 100f, 0f));

            float positive = DirectionalMapper.MapDirectional(0.75f, 0.25f, 0.75f, 60f, 1f);
            float negative = DirectionalMapper.MapDirectional(0.75f, 0f, 1f, 40f, 1f);
            Equal("independent positive settings", 60f, positive);
            Equal("independent negative settings", 30f, negative);
        }

        private static void TestInvalidDirectionalInputs()
        {
            Equal("NaN input", 0f, DirectionalMapper.MapDirectional(float.NaN, 0f, 1f, 100f, 1f));
            Equal("infinite input", 0f, DirectionalMapper.MapDirectional(float.PositiveInfinity, 0f, 1f, 100f, 1f));
            Equal("NaN setting", 0f, DirectionalMapper.MapDirectional(0.5f, float.NaN, 1f, 100f, 1f));
            Equal("limit equals dead zone", 0f, DirectionalMapper.MapDirectional(0.8f, 0.5f, 0.5f, 100f, 1f));
            Equal("limit below dead zone", 0f, DirectionalMapper.MapDirectional(0.8f, 0.8f, 0.2f, 100f, 1f));
            Equal("negative max weight", 0f, DirectionalMapper.MapDirectional(1f, 0f, 1f, -50f, 1f));
        }

        private static void TestUnitCircleClamp()
        {
            float x = 0.3f;
            float y = 0.4f;
            DirectionalMapper.ClampToUnitCircle(ref x, ref y);
            Near("inside circle x unchanged", 0.3f, x, 0.000001f);
            Near("inside circle y unchanged", 0.4f, y, 0.000001f);

            x = 1f;
            y = 1f;
            DirectionalMapper.ClampToUnitCircle(ref x, ref y);
            Near("diagonal circle x", 0.7071068f, x, 0.00001f);
            Near("diagonal circle y", 0.7071068f, y, 0.00001f);

            x = float.NaN;
            y = 0.5f;
            DirectionalMapper.ClampToUnitCircle(ref x, ref y);
            Equal("invalid circle x reset", 0f, x);
            Equal("invalid circle y reset", 0f, y);
        }

        private static void TestSmoothing()
        {
            Near(
                "frame-rate independent exponential",
                63.212055f,
                DirectionalMapper.SmoothTowards(0f, 100f, 1f, 1f),
                0.0001f);
            Equal(
                "zero speed reaches target",
                100f,
                DirectionalMapper.SmoothTowards(0f, 100f, 0f, 1f));
            Equal(
                "zero delta preserves current",
                0f,
                DirectionalMapper.SmoothTowards(0f, 100f, 1f, 0f));
            Equal(
                "invalid current starts at target",
                25f,
                DirectionalMapper.SmoothTowards(float.NaN, 25f, 10f, 0.016f));
            Equal(
                "invalid target becomes zero",
                0f,
                DirectionalMapper.SmoothTowards(20f, float.NaN, 0f, 0.016f));

            float alpha = DirectionalMapper.CalculateSmoothingAlpha(1f, 1f);
            Near("shared smoothing alpha", 0.63212055f, alpha, 0.000001f);
            Near(
                "shared alpha application",
                63.212055f,
                DirectionalMapper.ApplySmoothingAlpha(0f, 100f, alpha),
                0.0001f);
            Equal(
                "zero speed alpha reaches target",
                1f,
                DirectionalMapper.CalculateSmoothingAlpha(0f, 1f));
            Equal(
                "zero delta alpha preserves current",
                0f,
                DirectionalMapper.CalculateSmoothingAlpha(1f, 0f));
            Equal(
                "invalid alpha safely reaches target",
                30f,
                DirectionalMapper.ApplySmoothingAlpha(
                    10f,
                    30f,
                    float.NaN));
        }

        private static void TestManagedWeightState()
        {
            ManagedWeightState state = new ManagedWeightState();
            Check("new weight state has no expectation", !state.HasExpectedValue);
            Check("first target is dirty", state.TargetChanged(0f));

            state.Commit(25f);
            Check("commit records expectation", state.HasExpectedValue);
            Equal("commit records value", 25f, state.ExpectedValue);
            Check("stable target skips direct write", !state.TargetChanged(25f));
            Check(
                "sub-epsilon target remains stable",
                !state.TargetChanged(25.0005f));
            Check(
                "larger target change writes directly",
                state.TargetChanged(25.002f));
            Check(
                "matching current needs no correction",
                !state.CurrentNeedsCorrection(25f, 25f));
            Check(
                "external drift is corrected",
                state.CurrentNeedsCorrection(40f, 25f));
            Check(
                "invalid renderer value is corrected",
                state.CurrentNeedsCorrection(float.NaN, 25f));

            state.Commit(10f);
            Check(
                "small movement initially coalesces",
                !state.TargetChanged(10.0005f));
            state.Commit(10f);
            Check(
                "coalesced movement accumulates",
                state.TargetChanged(10.0012f));

            state.Reset();
            Check("reset clears expectation", !state.HasExpectedValue);
            Check("reset makes next target dirty", state.TargetChanged(10f));
        }

        private static void TestEyeCustomizationMapping()
        {
            Equal(
                "ExpressionControl IrisY maximum",
                0.5f,
                EyeCustomizationMapper.IrisYMaximum);
            Equal(
                "ExpressionControl Size maximum",
                1f,
                EyeCustomizationMapper.IrisSizeMaximum);
            Equal(
                "IrisY zero is neutral",
                0f,
                EyeCustomizationMapper.MapIrisY(0f, 100f));
            Equal(
                "IrisY midpoint",
                50f,
                EyeCustomizationMapper.MapIrisY(0.25f, 100f));
            Equal(
                "IrisY reaches configured maximum",
                80f,
                EyeCustomizationMapper.MapIrisY(0.5f, 80f));
            Equal(
                "IrisY source clamps",
                100f,
                EyeCustomizationMapper.MapIrisY(5f, 100f));
            Equal(
                "Size zero is neutral",
                0f,
                EyeCustomizationMapper.MapIrisSize(0f, 100f));
            Equal(
                "Size midpoint",
                35f,
                EyeCustomizationMapper.MapIrisSize(0.5f, 70f));
            Equal(
                "Size reaches configured maximum",
                100f,
                EyeCustomizationMapper.MapIrisSize(1f, 100f));
            Equal(
                "Size source clamps",
                100f,
                EyeCustomizationMapper.MapIrisSize(4f, 100f));
            Equal(
                "invalid source becomes neutral",
                0f,
                EyeCustomizationMapper.MapIrisY(float.NaN, 100f));
            Equal(
                "infinite source becomes neutral",
                0f,
                EyeCustomizationMapper.MapIrisSize(
                    float.PositiveInfinity,
                    100f));
            Equal(
                "negative source becomes neutral",
                0f,
                EyeCustomizationMapper.MapIrisY(-0.1f, 100f));
            Equal(
                "maximum weight clamps to 100",
                100f,
                EyeCustomizationMapper.MapIrisSize(1f, 250f));
            Equal(
                "invalid maximum weight becomes zero",
                0f,
                EyeCustomizationMapper.MapIrisSize(1f, float.NaN));

#if LEGACY_EYE_CUSTOMIZATION_MAPPING_TESTS
            Equal(
                "eye geometry neutral constant",
                0.5f,
                EyeCustomizationMapper.EyeGeometryNeutral);
            Equal(
                "iris size neutral constant",
                0.9f,
                EyeCustomizationMapper.IrisSizeNeutral);
            Equal(
                "pupil offset neutral constant",
                0.5f,
                EyeCustomizationMapper.PupilOffsetNeutral);
            Equal(
                "customization maximum weight",
                100f,
                EyeCustomizationMapper.MaximumWeight);

            EyeGeometryWeights geometry =
                EyeCustomizationMapper.MapEyeGeometry(0.5f, 0.5f);
            Equal("neutral eye height negative", 0f, geometry.Height.Negative);
            Equal("neutral eye height positive", 0f, geometry.Height.Positive);
            Equal("neutral eye width negative", 0f, geometry.Width.Negative);
            Equal("neutral eye width positive", 0f, geometry.Width.Positive);

            geometry = EyeCustomizationMapper.MapEyeGeometry(0f, 1f);
            Equal("minimum eye height reaches negative maximum", 100f, geometry.Height.Negative);
            Equal("minimum eye height clears positive", 0f, geometry.Height.Positive);
            Equal("maximum eye width clears negative", 0f, geometry.Width.Negative);
            Equal("maximum eye width reaches positive maximum", 100f, geometry.Width.Positive);

            geometry = EyeCustomizationMapper.MapEyeGeometry(0.25f, 0.75f);
            Equal("eye height negative midpoint", 50f, geometry.Height.Negative);
            Equal("eye height midpoint clears positive", 0f, geometry.Height.Positive);
            Equal("eye width midpoint clears negative", 0f, geometry.Width.Negative);
            Equal("eye width positive midpoint", 50f, geometry.Width.Positive);

            geometry = EyeCustomizationMapper.MapEyeGeometry(-4f, 8f);
            Equal("eye height finite lower clamp", 100f, geometry.Height.Negative);
            Equal("eye width finite upper clamp", 100f, geometry.Width.Positive);

            geometry = EyeCustomizationMapper.MapEyeGeometry(
                float.NaN,
                float.PositiveInfinity);
            Equal("invalid eye height falls back to neutral negative", 0f, geometry.Height.Negative);
            Equal("invalid eye height falls back to neutral positive", 0f, geometry.Height.Positive);
            Equal("invalid eye width falls back to neutral negative", 0f, geometry.Width.Negative);
            Equal("invalid eye width falls back to neutral positive", 0f, geometry.Width.Positive);

            geometry = EyeCustomizationMapper.MapEyeGeometry(
                float.NegativeInfinity,
                float.NaN);
            Equal("negative infinity eye height is neutral", 0f, geometry.Height.Negative);
            Equal("NaN eye width is neutral", 0f, geometry.Width.Positive);

            IrisSizeWeights iris =
                EyeCustomizationMapper.MapIrisSize(0.9f, 0.9f);
            Equal("neutral iris width negative", 0f, iris.Width.Negative);
            Equal("neutral iris width positive", 0f, iris.Width.Positive);
            Equal("neutral iris height negative", 0f, iris.Height.Negative);
            Equal("neutral iris height positive", 0f, iris.Height.Positive);

            iris = EyeCustomizationMapper.MapIrisSize(0f, 1f);
            Equal("minimum iris width reaches negative maximum", 100f, iris.Width.Negative);
            Equal("minimum iris width clears positive", 0f, iris.Width.Positive);
            Equal("maximum iris height clears negative", 0f, iris.Height.Negative);
            Equal("maximum iris height reaches positive maximum", 100f, iris.Height.Positive);

            iris = EyeCustomizationMapper.MapIrisSize(0.45f, 0.95f);
            Equal("iris below-neutral midpoint uses long range", 50f, iris.Width.Negative);
            Equal("iris below-neutral midpoint clears positive", 0f, iris.Width.Positive);
            Equal("iris above-neutral midpoint clears negative", 0f, iris.Height.Negative);
            Near(
                "iris above-neutral midpoint uses short range",
                50f,
                iris.Height.Positive,
                0.0001f);

            iris = EyeCustomizationMapper.MapIrisSize(0.85f, 0.91f);
            Near(
                "iris lower range remains asymmetric",
                5.555556f,
                iris.Width.Negative,
                0.0001f);
            Near(
                "iris upper range remains asymmetric",
                10f,
                iris.Height.Positive,
                0.0001f);

            iris = EyeCustomizationMapper.MapIrisSize(-1f, 2f);
            Equal("iris finite lower clamp", 100f, iris.Width.Negative);
            Equal("iris finite upper clamp", 100f, iris.Height.Positive);

            iris = EyeCustomizationMapper.MapIrisSize(
                float.NaN,
                float.NegativeInfinity);
            Equal("invalid iris width falls back to neutral", 0f, iris.Width.Negative);
            Equal("invalid iris width has no positive weight", 0f, iris.Width.Positive);
            Equal("invalid iris height falls back to neutral", 0f, iris.Height.Negative);
            Equal("invalid iris height has no positive weight", 0f, iris.Height.Positive);

            PupilOffsetWeights offset =
                EyeCustomizationMapper.MapPupilOffset(0.5f, 0.5f);
            Equal("neutral pupil X negative", 0f, offset.X.Negative);
            Equal("neutral pupil X positive", 0f, offset.X.Positive);
            Equal("neutral pupil Y negative", 0f, offset.Y.Negative);
            Equal("neutral pupil Y positive", 0f, offset.Y.Positive);

            offset = EyeCustomizationMapper.MapPupilOffset(0f, 1f);
            Equal("minimum pupil X reaches negative maximum", 100f, offset.X.Negative);
            Equal("minimum pupil X clears positive", 0f, offset.X.Positive);
            Equal("maximum pupil Y clears negative", 0f, offset.Y.Negative);
            Equal("maximum pupil Y reaches positive maximum", 100f, offset.Y.Positive);

            offset = EyeCustomizationMapper.MapPupilOffset(0.25f, 0.75f);
            Equal("pupil X negative midpoint", 50f, offset.X.Negative);
            Equal("pupil X midpoint clears positive", 0f, offset.X.Positive);
            Equal("pupil Y midpoint clears negative", 0f, offset.Y.Negative);
            Equal("pupil Y positive midpoint", 50f, offset.Y.Positive);

            offset = EyeCustomizationMapper.MapPupilOffset(-2f, 3f);
            Equal("pupil X finite lower clamp", 100f, offset.X.Negative);
            Equal("pupil Y finite upper clamp", 100f, offset.Y.Positive);

            offset = EyeCustomizationMapper.MapPupilOffset(
                float.PositiveInfinity,
                float.NaN);
            Equal("invalid pupil X falls back to neutral", 0f, offset.X.Negative);
            Equal("invalid pupil X has no positive weight", 0f, offset.X.Positive);
            Equal("invalid pupil Y falls back to neutral", 0f, offset.Y.Negative);
            Equal("invalid pupil Y has no positive weight", 0f, offset.Y.Positive);

            float[] representativeValues =
            {
                -10f,
                0f,
                0.25f,
                0.5f,
                0.75f,
                0.9f,
                0.95f,
                1f,
                10f,
                float.NaN,
                float.NegativeInfinity,
                float.PositiveInfinity
            };
            for (int i = 0; i < representativeValues.Length; i++)
            {
                SplitCustomizationWeights split =
                    EyeCustomizationMapper.MapAroundNeutral(
                        representativeValues[i],
                        0.9f);
                Check(
                    "customization weights stay in range " + i,
                    split.Negative >= 0f &&
                    split.Negative <= 100f &&
                    split.Positive >= 0f &&
                    split.Positive <= 100f);
                Check(
                    "customization split stays one-sided " + i,
                    split.Negative == 0f || split.Positive == 0f);
            }

            SplitCustomizationWeights invalidNeutral =
                EyeCustomizationMapper.MapAroundNeutral(
                    0.5f,
                    float.NaN);
            Equal("invalid neutral safely falls back negative", 0f, invalidNeutral.Negative);
            Equal("invalid neutral safely falls back positive", 0f, invalidNeutral.Positive);
#endif
        }

        private static void TestSanitization()
        {
            Equal("signed upper clamp", 1f, DirectionalMapper.SanitizeSignedUnit(2f));
            Equal("signed lower clamp", -1f, DirectionalMapper.SanitizeSignedUnit(-2f));
            Equal("signed NaN reset", 0f, DirectionalMapper.SanitizeSignedUnit(float.NaN));
            Equal("clamp01 lower", 0f, DirectionalMapper.Clamp01(-1f));
            Equal("clamp01 upper", 1f, DirectionalMapper.Clamp01(4f));
            Equal("clamp01 NaN reset", 0f, DirectionalMapper.Clamp01(float.NaN));
            Equal("clamp01 infinity reset", 0f, DirectionalMapper.Clamp01(float.PositiveInfinity));
        }

        private static void TestEyeCommonAxisRates()
        {
            Equal(
                "left rate preserves positive sign",
                0.75f,
                DirectionalMapper.EyeHorizontalToCommon(0.75f));
            Equal(
                "right rate preserves positive sign",
                0.75f,
                DirectionalMapper.EyeHorizontalToCommon(0.75f));
            Equal(
                "left rate preserves negative sign",
                -0.5f,
                DirectionalMapper.EyeHorizontalToCommon(-0.5f));
            Equal(
                "right rate preserves negative sign",
                -0.5f,
                DirectionalMapper.EyeHorizontalToCommon(-0.5f));

            float leftCommon =
                DirectionalMapper.EyeHorizontalToCommon(0.8f);
            float rightCommon =
                DirectionalMapper.EyeHorizontalToCommon(0.6f);
            Equal(
                "common-axis average preserves lateral gaze",
                0.7f,
                (leftCommon + rightCommon) * 0.5f);

            leftCommon =
                DirectionalMapper.EyeHorizontalToCommon(0.4f);
            rightCommon =
                DirectionalMapper.EyeHorizontalToCommon(-0.4f);
            Equal(
                "symmetric convergence cancels in common average",
                0f,
                (leftCommon + rightCommon) * 0.5f);

            leftCommon = DirectionalMapper.EyeHorizontalToCommon(0.21996f);
            rightCommon = DirectionalMapper.EyeHorizontalToCommon(-0.21996f);
            Equal(
                "observed frontal convergence produces neutral X",
                0f,
                (leftCommon + rightCommon) * 0.5f);
        }

        private static void TestCenterOffsets()
        {
            Equal("center offset neutral", 0f, DirectionalMapper.CenterSignedInput(-0.22f, -0.22f));
            Equal("center offset positive", 0.12f, DirectionalMapper.CenterSignedInput(-0.10f, -0.22f));
            Equal("center offset negative", -0.18f, DirectionalMapper.CenterSignedInput(-0.40f, -0.22f));
            Equal("center offset upper clamp", 1f, DirectionalMapper.CenterSignedInput(0.8f, -0.5f));
            Equal("center offset lower clamp", -1f, DirectionalMapper.CenterSignedInput(-0.8f, 0.5f));
            Equal("center offset invalid value", 0f, DirectionalMapper.CenterSignedInput(float.NaN, 0.2f));
            Equal("center offset invalid center", 0.3f, DirectionalMapper.CenterSignedInput(0.3f, float.NaN));
        }

        private static void TestManualVisibilityStateMachines()
        {
            FloatVisibilityState shape = new FloatVisibilityState();
            bool write;
            float floatValue;
            bool skipped;

            shape.Transition(
                ManualVisibilityMode.Hidden,
                37f,
                100f,
                out write,
                out floatValue,
                out skipped);
            Check("shape Original->Hidden writes", write);
            Equal("shape hidden value", 100f, floatValue);
            Equal("shape captures original", 37f, shape.OriginalValue);
            Check("shape owns after override", shape.OwnsValue);

            shape.Transition(
                ManualVisibilityMode.Visible,
                100f,
                100f,
                out write,
                out floatValue,
                out skipped);
            Check("shape Hidden->Visible writes", write);
            Equal("shape visible value", 0f, floatValue);
            Equal("shape preserves first baseline", 37f, shape.OriginalValue);

            shape.Transition(
                ManualVisibilityMode.Hidden,
                0f,
                65f,
                out write,
                out floatValue,
                out skipped);
            Check("shape Visible->Hidden writes", write);
            Equal("shape uses configured hide weight", 65f, floatValue);

            shape.Transition(
                ManualVisibilityMode.Original,
                65f,
                65f,
                out write,
                out floatValue,
                out skipped);
            Check("shape active->Original writes", write);
            Equal("shape restores exact baseline", 37f, floatValue);
            Check("shape releases ownership", !shape.OwnsValue);
            Check("shape restore not skipped", !skipped);

            FloatVisibilityState conflictedShape = new FloatVisibilityState();
            conflictedShape.Transition(
                ManualVisibilityMode.Hidden,
                12f,
                100f,
                out write,
                out floatValue,
                out skipped);
            conflictedShape.Transition(
                ManualVisibilityMode.Original,
                44f,
                100f,
                out write,
                out floatValue,
                out skipped);
            Check("shape external change skips restore", skipped);
            Check("shape skipped restore does not write", !write);
            Check("shape skipped restore releases ownership", !conflictedShape.OwnsValue);

            FloatVisibilityState recapturedShape = new FloatVisibilityState();
            recapturedShape.Transition(
                ManualVisibilityMode.Hidden,
                10f,
                100f,
                out write,
                out floatValue,
                out skipped);
            recapturedShape.Transition(
                ManualVisibilityMode.Visible,
                55f,
                100f,
                out write,
                out floatValue,
                out skipped);
            Equal("shape external drift becomes new baseline", 55f, recapturedShape.OriginalValue);
            recapturedShape.Transition(
                ManualVisibilityMode.Original,
                0f,
                100f,
                out write,
                out floatValue,
                out skipped);
            Equal("shape restores recaptured baseline", 55f, floatValue);

            FloatVisibilityState clampedShape = new FloatVisibilityState();
            clampedShape.Transition(
                ManualVisibilityMode.Hidden,
                0f,
                180f,
                out write,
                out floatValue,
                out skipped);
            Equal("shape hide weight clamps to 100", 100f, floatValue);

            FloatVisibilityState alreadyHiddenShape = new FloatVisibilityState();
            alreadyHiddenShape.Transition(
                ManualVisibilityMode.Hidden,
                100f,
                100f,
                out write,
                out floatValue,
                out skipped);
            Check(
                "shape records active mode without claiming unchanged value",
                !write &&
                !alreadyHiddenShape.OwnsValue &&
                alreadyHiddenShape.Mode == ManualVisibilityMode.Hidden);
            alreadyHiddenShape.Transition(
                ManualVisibilityMode.Original,
                100f,
                100f,
                out write,
                out floatValue,
                out skipped);
            Check(
                "shape unchanged override can return to Original",
                !write &&
                alreadyHiddenShape.Mode == ManualVisibilityMode.Original);

            BoolVisibilityState renderer = new BoolVisibilityState();
            bool boolValue;
            renderer.Transition(
                ManualVisibilityMode.Visible,
                false,
                out write,
                out boolValue,
                out skipped);
            Check("renderer Original->Visible writes", write && boolValue);
            Check("renderer captures disabled baseline", !renderer.OriginalValue);

            renderer.Transition(
                ManualVisibilityMode.Hidden,
                true,
                out write,
                out boolValue,
                out skipped);
            Check("renderer Visible->Hidden writes", write && !boolValue);

            renderer.Transition(
                ManualVisibilityMode.Original,
                false,
                out write,
                out boolValue,
                out skipped);
            Check("renderer restores disabled baseline without redundant write", !write);
            Check("renderer releases ownership", !renderer.OwnsValue);

            BoolVisibilityState conflictedRenderer = new BoolVisibilityState();
            conflictedRenderer.Transition(
                ManualVisibilityMode.Hidden,
                true,
                out write,
                out boolValue,
                out skipped);
            conflictedRenderer.Transition(
                ManualVisibilityMode.Original,
                true,
                out write,
                out boolValue,
                out skipped);
            Check("renderer external change skips restore", skipped && !write);

            BoolVisibilityState recapturedRenderer = new BoolVisibilityState();
            recapturedRenderer.Transition(
                ManualVisibilityMode.Visible,
                false,
                out write,
                out boolValue,
                out skipped);
            recapturedRenderer.Transition(
                ManualVisibilityMode.Visible,
                false,
                out write,
                out boolValue,
                out skipped);
            Check(
                "renderer external drift is recaptured",
                recapturedRenderer.OwnsValue &&
                !recapturedRenderer.OriginalValue &&
                write && boolValue);

            BoolVisibilityState alreadyHiddenRenderer = new BoolVisibilityState();
            alreadyHiddenRenderer.Transition(
                ManualVisibilityMode.Hidden,
                false,
                out write,
                out boolValue,
                out skipped);
            Check(
                "renderer records active mode without claiming unchanged value",
                !write &&
                !alreadyHiddenRenderer.OwnsValue &&
                alreadyHiddenRenderer.Mode == ManualVisibilityMode.Hidden);
            alreadyHiddenRenderer.Transition(
                ManualVisibilityMode.Original,
                false,
                out write,
                out boolValue,
                out skipped);
            Check(
                "renderer unchanged override can return to Original",
                !write &&
                alreadyHiddenRenderer.Mode == ManualVisibilityMode.Original);
        }

        private static void TestVisibilityCardData()
        {
            ManualVisibilityMode[] blendshapes =
                new ManualVisibilityMode[ManualVisibilityCatalog.BlendshapeCount];
            ManualVisibilityMode[] renderers =
                new ManualVisibilityMode[ManualVisibilityCatalog.RendererCount];
            blendshapes[0] = ManualVisibilityMode.Hidden;
            blendshapes[3] = ManualVisibilityMode.Visible;
            renderers[2] = ManualVisibilityMode.Hidden;

            byte[] encoded = VisibilityCardData.Encode(blendshapes, renderers);
            Check("card payload has canonical length",
                encoded.Length == VisibilityCardData.ModeCount);
            Check("non-default card payload detected",
                !VisibilityCardData.IsDefault(encoded));

            ManualVisibilityMode[] decodedBlendshapes =
                new ManualVisibilityMode[ManualVisibilityCatalog.BlendshapeCount];
            ManualVisibilityMode[] decodedRenderers =
                new ManualVisibilityMode[ManualVisibilityCatalog.RendererCount];
            string error;
            Check(
                "card payload round-trips",
                VisibilityCardData.TryDecode(
                    VisibilityCardData.SchemaVersion,
                    encoded,
                    decodedBlendshapes,
                    decodedRenderers,
                    out error));
            Check("card payload round-trip error empty",
                string.IsNullOrEmpty(error));
            Check("card blendshape Hidden round-trips",
                decodedBlendshapes[0] == ManualVisibilityMode.Hidden);
            Check("card blendshape Visible round-trips",
                decodedBlendshapes[3] == ManualVisibilityMode.Visible);
            Check("card renderer Hidden round-trips",
                decodedRenderers[2] == ManualVisibilityMode.Hidden);
            Check(
                "legacy card payload remains readable",
                VisibilityCardData.TryDecode(
                    VisibilityCardData.LegacySchemaVersion,
                    encoded,
                    decodedBlendshapes,
                    decodedRenderers,
                    out error));
            Check(
                "schema 2 card payload remains readable",
                VisibilityCardData.TryDecode(
                    VisibilityCardData.PreviousSchemaVersion,
                    encoded,
                    decodedBlendshapes,
                    decodedRenderers,
                    out error));
            Check(
                "expression-link card schema is version 3",
                VisibilityCardData.SchemaVersion == 3);
            Check(
                "previous card schema remains version 2",
                VisibilityCardData.PreviousSchemaVersion == 2);
            Check(
                "expression-link payload key is stable",
                VisibilityCardData.LinksKey == "expressionLinksBinary");
            Check(
                "expression trigger key 01 is stable",
                VisibilityCardData.GetExpressionTriggerKey(0) ==
                    "expressionTrigger01");
            Check(
                "expression trigger key 04 is stable",
                VisibilityCardData.GetExpressionTriggerKey(3) ==
                    "expressionTrigger04");
            Check(
                "expression trigger text is normalized",
                VisibilityCardData.NormalizeExpressionTrigger(
                    "  eyes:4  ") == "eyes:4");
            Check(
                "non-empty expression triggers are detected",
                VisibilityCardData.HasExpressionTriggers(
                    new string[] { string.Empty, "mouth:2", "", null }));
            Check(
                "empty expression triggers stay default",
                !VisibilityCardData.HasExpressionTriggers(
                    new string[] { "", "  ", null, string.Empty }));

            Check(
                "unknown card schema rejected",
                !VisibilityCardData.TryDecode(
                    99,
                    encoded,
                    decodedBlendshapes,
                    decodedRenderers,
                    out error));
            Check(
                "wrong card payload length rejected",
                !VisibilityCardData.TryDecode(
                    VisibilityCardData.SchemaVersion,
                    new byte[1],
                    decodedBlendshapes,
                    decodedRenderers,
                    out error));

            byte[] invalid = new byte[VisibilityCardData.ModeCount];
            invalid[5] = 255;
            Check(
                "invalid card mode rejected",
                !VisibilityCardData.TryDecode(
                    VisibilityCardData.SchemaVersion,
                    invalid,
                    decodedBlendshapes,
                    decodedRenderers,
                    out error));
            Check("default card payload detected",
                VisibilityCardData.IsDefault(
                    new byte[VisibilityCardData.ModeCount]));
        }

        private static void TestExpressionTriggerSyntax()
        {
            ParsedExpressionTrigger parsed;
            ExpressionTriggerResolutionStatus status;
            string message;
            Check(
                "empty expression trigger is disabled",
                !ExpressionTriggerSyntax.TryParse(
                    "  ", out parsed, out status, out message) &&
                status == ExpressionTriggerResolutionStatus.Disabled);
            Check(
                "eyes selector parses case-insensitively",
                ExpressionTriggerSyntax.TryParse(
                    " EYES:4 ", out parsed, out status, out message) &&
                parsed.IsSelector &&
                parsed.Selector.Part == ExpressionTriggerPart.Eyes &&
                parsed.Selector.PatternIndex == 4);
            Check(
                "selector formats canonically",
                ExpressionTriggerSyntax.FormatSelector(parsed.Selector) ==
                    "eyes:4");
            Check(
                "negative selector is invalid",
                !ExpressionTriggerSyntax.TryParse(
                    "mouth:-1", out parsed, out status, out message) &&
                status == ExpressionTriggerResolutionStatus.Invalid);
            Check(
                "destination blendshape cannot be a trigger",
                !ExpressionTriggerSyntax.TryParse(
                    "eye_motion.f00_eye_posx",
                    out parsed,
                    out status,
                    out message) &&
                status == ExpressionTriggerResolutionStatus.Excluded);
            Check(
                "exact game blendshape name parses",
                ExpressionTriggerSyntax.TryParse(
                    "happy_blendshape",
                    out parsed,
                    out status,
                    out message) &&
                !parsed.IsSelector &&
                parsed.BlendshapeName == "happy_blendshape");
        }

        private static void TestConfigChangeClassification()
        {
            Check(
                "target changes rebind eyes",
                ConfigChangeClassifier.AffectsEyeBinding(
                    "Target Headmod",
                    "TargetRendererPath"));
            Check(
                "eye name changes rebind eyes",
                ConfigChangeClassifier.AffectsEyeBinding(
                    "Blendshape Names",
                    "PositiveXBlendshape"));
            Check(
                "blink enable changes rebind eyes",
                ConfigChangeClassifier.AffectsEyeBinding(
                    "Blink",
                    "BlinkEnabled"));
            Check(
                "ExpressionControl eye shape names rebind eyes",
                ConfigChangeClassifier.AffectsEyeBinding(
                    "ExpressionControl Eye Adjustment Blendshapes",
                    "IrisSizeBlendshape"));
            Check(
                "ExpressionControl eye value changes stay live",
                ConfigChangeClassifier.AffectsEyeAdjustmentValues(
                    "ExpressionControl Eye Adjustments"));
            Check(
                "blink weight does not rebind eyes",
                !ConfigChangeClassifier.AffectsEyeBinding(
                    "Blink",
                    "BlinkMaxWeight"));
            Check(
                "movement changes do not rebind eyes",
                !ConfigChangeClassifier.AffectsEyeBinding(
                    "Horizontal Movement",
                    "PositiveXInputLimit"));
            Check(
                "visibility names do not rebind eyes",
                !ConfigChangeClassifier.AffectsEyeBinding(
                    "Manual Visibility Blendshapes",
                    "Highlight01Blendshape"));
            Check(
                "hide names refresh visibility definitions",
                ConfigChangeClassifier.AffectsVisibilityDefinitions(
                    "Manual Visibility Blendshapes"));
            Check(
                "renderer targets refresh visibility definitions",
                ConfigChangeClassifier.AffectsVisibilityDefinitions(
                    "Manual Visibility Renderers"));
            Check(
                "hide weight reapplies visibility values",
                ConfigChangeClassifier.AffectsVisibilityValues(
                    "Manual Visibility"));
            Check(
                "diagnostics do not affect visibility",
                !ConfigChangeClassifier.AffectsVisibilityValues(
                    "Diagnostics"));
        }

        private static void Equal(string name, float expected, float actual)
        {
            Near(name, expected, actual, 0.0001f);
        }

        private static void Near(
            string name,
            float expected,
            float actual,
            float tolerance)
        {
            _checks++;
            if (Math.Abs(expected - actual) <= tolerance)
            {
                return;
            }

            _failures++;
            Console.Error.WriteLine(
                "FAIL " + name + ": expected " + expected + ", actual " + actual);
        }

        private static void Check(string name, bool condition)
        {
            _checks++;
            if (condition)
            {
                return;
            }

            _failures++;
            Console.Error.WriteLine("FAIL " + name);
        }
    }
}
