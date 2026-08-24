namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class QuickSettingsWindowGeometryTests
    {
        private static int _checks;
        private static int _failures;

        internal static int Run(out int checks)
        {
            _checks = 0;
            _failures = 0;

            RepeatedClampPreservesMovedBounds();
            HeaderCanRemainAccessibleWhileBodyLeavesViewport();
            FitRestoresPreferredSizeWhenViewportAllowsIt();
            TinyViewportCanShrinkBelowMinimumSize();
            EqualityIncludesPositionAndSize();

            checks = _checks;
            return _failures;
        }

        private static void RepeatedClampPreservesMovedBounds()
        {
            QuickSettingsWindowBounds moved =
                new QuickSettingsWindowBounds(
                    333f,
                    222f,
                    500f,
                    520f);
            QuickSettingsWindowBounds current = moved;

            for (int index = 0; index < 1000; index++)
            {
                current = QuickSettingsWindowGeometry.KeepHeaderAccessible(
                    current,
                    1920f,
                    1080f,
                    CreateConstraints());
                IsTrue(
                    "stable clamp " + index,
                    QuickSettingsWindowGeometry.AreEqual(
                        moved,
                        current));
            }
        }

        private static void HeaderCanRemainAccessibleWhileBodyLeavesViewport()
        {
            QuickSettingsWindowConstraints constraints =
                CreateConstraints();
            QuickSettingsWindowBounds left =
                QuickSettingsWindowGeometry.KeepHeaderAccessible(
                    new QuickSettingsWindowBounds(
                        -1000f,
                        100f,
                        500f,
                        520f),
                    1920f,
                    1080f,
                    constraints);
            Equal("left recovery edge", -372f, left.X);
            Equal("left keeps y", 100f, left.Y);

            QuickSettingsWindowBounds right =
                QuickSettingsWindowGeometry.KeepHeaderAccessible(
                    new QuickSettingsWindowBounds(
                        3000f,
                        100f,
                        500f,
                        520f),
                    1920f,
                    1080f,
                    constraints);
            Equal("right recovery edge", 1792f, right.X);

            QuickSettingsWindowBounds top =
                QuickSettingsWindowGeometry.KeepHeaderAccessible(
                    new QuickSettingsWindowBounds(
                        100f,
                        -500f,
                        500f,
                        520f),
                    1920f,
                    1080f,
                    constraints);
            Equal("top header edge", 8f, top.Y);

            QuickSettingsWindowBounds bottom =
                QuickSettingsWindowGeometry.KeepHeaderAccessible(
                    new QuickSettingsWindowBounds(
                        100f,
                        2000f,
                        500f,
                        520f),
                    1920f,
                    1080f,
                    constraints);
            Equal("bottom header edge", 1048f, bottom.Y);
        }

        private static void FitRestoresPreferredSizeWhenViewportAllowsIt()
        {
            QuickSettingsWindowBounds fitted =
                QuickSettingsWindowGeometry.FitToViewport(
                    new QuickSettingsWindowBounds(
                        1300f,
                        800f,
                        500f,
                        520f),
                    420f,
                    330f,
                    CreateConstraints());
            Equal("small viewport width", 404f, fitted.Width);
            Equal("small viewport height", 314f, fitted.Height);
            Equal("small viewport header x", 292f, fitted.X);
            Equal("small viewport header y", 298f, fitted.Y);

            QuickSettingsWindowBounds expanded =
                QuickSettingsWindowGeometry.FitToViewport(
                    fitted.WithSize(500f, 520f),
                    1920f,
                    1080f,
                    CreateConstraints());
            Equal("expanded preferred width", 500f, expanded.Width);
            Equal("expanded preferred height", 520f, expanded.Height);
        }

        private static void TinyViewportCanShrinkBelowMinimumSize()
        {
            QuickSettingsWindowBounds fitted =
                QuickSettingsWindowGeometry.FitToViewport(
                    new QuickSettingsWindowBounds(
                        24f,
                        72f,
                        500f,
                        520f),
                    100f,
                    80f,
                    CreateConstraints());
            Equal("tiny viewport width", 84f, fitted.Width);
            Equal("tiny viewport height", 64f, fitted.Height);
            Equal("tiny viewport x", 8f, fitted.X);
            Equal("tiny viewport y", 48f, fitted.Y);
        }

        private static void EqualityIncludesPositionAndSize()
        {
            QuickSettingsWindowBounds baseline =
                new QuickSettingsWindowBounds(10f, 20f, 30f, 40f);
            IsTrue(
                "equal bounds",
                QuickSettingsWindowGeometry.AreEqual(
                    baseline,
                    new QuickSettingsWindowBounds(
                        10f,
                        20f,
                        30f,
                        40f)));
            IsFalse(
                "different x",
                QuickSettingsWindowGeometry.AreEqual(
                    baseline,
                    baseline.WithPosition(11f, 20f)));
            IsFalse(
                "different size",
                QuickSettingsWindowGeometry.AreEqual(
                    baseline,
                    baseline.WithSize(31f, 40f)));
        }

        private static QuickSettingsWindowConstraints CreateConstraints()
        {
            return new QuickSettingsWindowConstraints(
                360f,
                300f,
                8f,
                24f,
                120f);
        }

        private static void IsTrue(string name, bool value)
        {
            _checks++;
            if (value)
            {
                return;
            }

            _failures++;
            System.Console.Error.WriteLine("FAIL " + name);
        }

        private static void IsFalse(string name, bool value)
        {
            IsTrue(name, !value);
        }

        private static void Equal(
            string name,
            float expected,
            float actual)
        {
            _checks++;
            if (System.Math.Abs(expected - actual) <= 0.0001f)
            {
                return;
            }

            _failures++;
            System.Console.Error.WriteLine(
                "FAIL " + name + ": expected " + expected +
                ", got " + actual);
        }
    }
}
