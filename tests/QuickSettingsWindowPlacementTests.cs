using System;

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class QuickSettingsWindowPlacementTests
    {
        private const float Margin = 8f;
        private const float WindowWidth = 500f;
        private const float VisibleHeaderWidth = 120f;
        private const float HeaderHeight = 24f;
        private static int _checks;
        private static int _failures;

        internal static int Run(out int checks)
        {
            _checks = 0;
            _failures = 0;

            TestScreen("1920x1080", 1920f, 1080f);
            TestScreen("1366x768", 1366f, 768f);
            TestScreen("1280x720", 1280f, 720f);
            TestScreen("800x600", 800f, 600f);
            TestTinyScreen();
            TestInvalidAndSmallInputs();
            TestDraggedCoordinates();

            checks = _checks;
            return _failures;
        }

        private static void TestScreen(
            string name,
            float screenWidth,
            float screenHeight)
        {
            float left = ClampHorizontal(
                -10000f,
                WindowWidth,
                screenWidth,
                Margin);
            float right = ClampHorizontal(
                10000f,
                WindowWidth,
                screenWidth,
                Margin);
            float top = ClampVertical(
                -10000f,
                screenHeight,
                Margin);
            float bottom = ClampVertical(
                10000f,
                screenHeight,
                Margin);

            Equal(
                name + " left clamp",
                Margin + VisibleHeaderWidth - WindowWidth,
                left);
            Equal(
                name + " right clamp",
                screenWidth - Margin -
                    VisibleHeaderWidth,
                right);
            Equal(name + " top clamp", Margin, top);
            Equal(
                name + " bottom clamp",
                screenHeight - Margin -
                    HeaderHeight,
                bottom);
            Equal(
                name + " horizontal idempotence left",
                left,
                ClampHorizontal(
                    left,
                    WindowWidth,
                    screenWidth,
                    Margin));
            Equal(
                name + " horizontal idempotence right",
                right,
                ClampHorizontal(
                    right,
                    WindowWidth,
                    screenWidth,
                    Margin));
            Equal(
                name + " vertical idempotence top",
                top,
                ClampVertical(
                    top,
                    screenHeight,
                    Margin));
            Equal(
                name + " vertical idempotence bottom",
                bottom,
                ClampVertical(
                    bottom,
                    screenHeight,
                    Margin));
            Equal(
                name + " in-range horizontal unchanged",
                25f,
                ClampHorizontal(
                    25f,
                    WindowWidth,
                    screenWidth,
                    Margin));
            Equal(
                name + " in-range vertical unchanged",
                25f,
                ClampVertical(
                    25f,
                    screenHeight,
                    Margin));
        }

        private static void TestTinyScreen()
        {
            const float width = 80f;
            const float height = 18f;

            float left = ClampHorizontal(
                -1000f,
                WindowWidth,
                width,
                Margin);
            float right = ClampHorizontal(
                1000f,
                WindowWidth,
                width,
                Margin);
            float top = ClampVertical(
                -1000f,
                height,
                Margin);
            float bottom = ClampVertical(
                1000f,
                height,
                Margin);

            Equal(
                "tiny screen exposes all available width left",
                width - WindowWidth,
                left);
            Equal("tiny screen exposes all available width right", 0f, right);
            Equal("tiny screen exposes all available height top", -6f, top);
            Equal("tiny screen exposes all available height bottom", 0f, bottom);
            Equal(
                "tiny horizontal clamp is idempotent",
                left,
                ClampHorizontal(
                    left,
                    WindowWidth,
                    width,
                    Margin));
            Equal(
                "tiny vertical clamp is idempotent",
                bottom,
                ClampVertical(
                    bottom,
                    height,
                    Margin));
        }

        private static void TestInvalidAndSmallInputs()
        {
            Equal(
                "small header remains fully visible",
                22f,
                ClampHorizontal(
                    1000f,
                    50f,
                    80f,
                    Margin));
            Equal(
                "oversized margin yields to complete header",
                13f,
                ClampVertical(
                    1000f,
                    50f,
                    1000f));
            Equal(
                "zero viewport is stable",
                0f,
                ClampHorizontal(
                    40f,
                    WindowWidth,
                    0f,
                    Margin));
            Equal(
                "invalid position clamps to the visible header",
                Margin,
                ClampVertical(
                    float.NaN,
                    600f,
                    Margin));
            Equal(
                "invalid viewport becomes zero",
                0f,
                ClampVertical(
                    50f,
                    float.PositiveInfinity,
                    Margin));
            Equal(
                "negative margin is ignored",
                576f,
                ClampHorizontal(
                    1000f,
                    640f,
                    696f,
                    -10f));
        }

        private static void TestDraggedCoordinates()
        {
            Equal(
                "drag follows the pointer to the right",
                34f,
                QuickSettingsWindowPlacement.CalculateDraggedCoordinate(
                    24f,
                    110f,
                    100f));
            Equal(
                "successive drag events accumulate",
                44f,
                QuickSettingsWindowPlacement.CalculateDraggedCoordinate(
                    34f,
                    110f,
                    100f));
            Equal(
                "drag follows the pointer to the left",
                14f,
                QuickSettingsWindowPlacement.CalculateDraggedCoordinate(
                    24f,
                    90f,
                    100f));
            Equal(
                "overflow preserves the current coordinate",
                float.MaxValue,
                QuickSettingsWindowPlacement.CalculateDraggedCoordinate(
                    float.MaxValue,
                    float.MaxValue,
                    -float.MaxValue));
        }

        private static float ClampHorizontal(
            float x,
            float windowWidth,
            float screenWidth,
            float screenMargin)
        {
            return QuickSettingsWindowPlacement.ClampHorizontal(
                x,
                windowWidth,
                screenWidth,
                screenMargin,
                VisibleHeaderWidth);
        }

        private static float ClampVertical(
            float y,
            float screenHeight,
            float screenMargin)
        {
            return QuickSettingsWindowPlacement.ClampVertical(
                y,
                screenHeight,
                screenMargin,
                HeaderHeight);
        }

        private static void Equal(string name, float expected, float actual)
        {
            _checks++;
            if (Math.Abs(expected - actual) <= 0.0001f)
            {
                return;
            }

            _failures++;
            Console.Error.WriteLine(
                "FAIL " + name + ": expected " + expected +
                ", actual " + actual);
        }
    }
}
