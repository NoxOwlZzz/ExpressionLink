using System;

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class QuickSettingsHeaderDragStateMachineTests
    {
        private const float Margin = 8f;
        private const float HeaderHeight = 24f;
        private const float PanelWidth = 500f;
        private const float VisibleHeaderWidth = 120f;
        private const float ViewportWidth = 1920f;
        private const float ViewportHeight = 1080f;
        private static int _checks;
        private static int _failures;

        internal static int Run(out int checks)
        {
            _checks = 0;
            _failures = 0;

            TestPressMustStartInsideHeader();
            TestHeaderBoundsAreExclusive();
            TestPointerOffsetIsPreserved();
            TestRepeatedSamplesDoNotDrift();
            TestDragUsesRawInputCoordinates();
            TestRawYCoordinatesAcrossViewport();
            TestDragContinuesOutsideHeader();
            TestReleaseEndsDrag();
            TestInterruptedReleaseDoesNotJump();
            TestHiddenPanelCancelsDrag();
            TestReopenWhileHeldDoesNotBegin();
            TestFocusLossCancelsDrag();
            TestFreshPressWorksAfterCancellation();
            TestDraggedPanelKeepsHeaderReachable();
            TestViewportResizeReclampsActiveDrag();

            checks = _checks;
            return _failures;
        }

        private static void TestPressMustStartInsideHeader()
        {
            QuickSettingsHeaderDragStateMachine state =
                new QuickSettingsHeaderDragStateMachine();

            QuickSettingsHeaderDragResult outside = state.Step(
                Frame(
                    true,
                    true,
                    true,
                    true,
                    false,
                    150f,
                    RawY(120f),
                    100f,
                    80f));
            Result(
                "press below header is ignored",
                outside,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                false);

            QuickSettingsHeaderDragResult held = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    300f,
                    RawY(200f),
                    100f,
                    80f));
            Result(
                "held input cannot begin a drag",
                held,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                false);

            QuickSettingsHeaderDragResult inside = state.Step(
                Frame(
                    true,
                    true,
                    true,
                    true,
                    false,
                    150f,
                    RawY(90f),
                    100f,
                    80f));
            Result(
                "press inside header begins drag",
                inside,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.Began,
                true);
        }

        private static void TestHeaderBoundsAreExclusive()
        {
            QuickSettingsHeaderDragStateMachine rightState =
                new QuickSettingsHeaderDragStateMachine();
            QuickSettingsHeaderDragResult right = rightState.Step(
                Frame(
                    true,
                    true,
                    true,
                    true,
                    false,
                    100f + PanelWidth,
                    RawY(90f),
                    100f,
                    80f));
            Result(
                "header right edge is exclusive",
                right,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                false);

            QuickSettingsHeaderDragStateMachine bottomState =
                new QuickSettingsHeaderDragStateMachine();
            QuickSettingsHeaderDragResult bottom = bottomState.Step(
                Frame(
                    true,
                    true,
                    true,
                    true,
                    false,
                    150f,
                    RawY(80f + HeaderHeight),
                    100f,
                    80f));
            Result(
                "header bottom edge is exclusive",
                bottom,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                false);

            QuickSettingsHeaderDragStateMachine topLeftState =
                new QuickSettingsHeaderDragStateMachine();
            QuickSettingsHeaderDragResult topLeft = topLeftState.Step(
                Frame(
                    true,
                    true,
                    true,
                    true,
                    false,
                    100f,
                    RawY(80f),
                    100f,
                    80f));
            Result(
                "header top-left edge is inclusive",
                topLeft,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.Began,
                true);
        }

        private static void TestPointerOffsetIsPreserved()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                350f,
                90f);

            QuickSettingsHeaderDragResult stationary = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    350f,
                    RawY(90f),
                    100f,
                    80f));
            Result(
                "stationary pointer never snaps panel",
                stationary,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                true);

            QuickSettingsHeaderDragResult moved = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    351f,
                    RawY(90f),
                    stationary.PanelX,
                    stationary.PanelY));
            Result(
                "one-pixel pointer delta moves one pixel",
                moved,
                101f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                true);
        }

        private static void TestRepeatedSamplesDoNotDrift()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                100f,
                90f);

            QuickSettingsHeaderDragResult first = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    110f,
                    RawY(90f),
                    100f,
                    80f));
            Result(
                "first ten-pixel sample",
                first,
                110f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                true);

            QuickSettingsHeaderDragResult repeated = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    110f,
                    RawY(90f),
                    first.PanelX,
                    first.PanelY));
            Result(
                "repeated pointer sample has no drift",
                repeated,
                110f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                true);

            QuickSettingsHeaderDragResult second = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    120f,
                    RawY(90f),
                    repeated.PanelX,
                    repeated.PanelY));
            Result(
                "pointer 110 to 120 is twenty total from panel start",
                second,
                120f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                true);
        }

        private static void TestDragUsesRawInputCoordinates()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);

            QuickSettingsHeaderDragResult result = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    180f,
                    RawY(110f),
                    100f,
                    80f));
            Result(
                "raw input Y is converted exactly once",
                result,
                130f,
                100f,
                QuickSettingsHeaderDragTransition.None,
                true);
        }

        private static void TestRawYCoordinatesAcrossViewport()
        {
            QuickSettingsHeaderDragStateMachine topState = BeginDrag(
                100f,
                400f,
                150f,
                410f);
            QuickSettingsHeaderDragResult top = topState.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    150f,
                    ViewportHeight,
                    100f,
                    400f));
            Result(
                "raw Y at screen top maps to GUI top",
                top,
                100f,
                Margin,
                QuickSettingsHeaderDragTransition.None,
                true);

            QuickSettingsHeaderDragStateMachine middleState = BeginDrag(
                100f,
                400f,
                150f,
                410f);
            QuickSettingsHeaderDragResult middle = middleState.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    150f,
                    RawY(410f),
                    100f,
                    400f));
            Result(
                "raw Y in screen middle preserves position",
                middle,
                100f,
                400f,
                QuickSettingsHeaderDragTransition.None,
                true);

            QuickSettingsHeaderDragStateMachine bottomState = BeginDrag(
                100f,
                400f,
                150f,
                410f);
            QuickSettingsHeaderDragResult bottom = bottomState.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    150f,
                    0f,
                    100f,
                    400f));
            Result(
                "raw Y at screen bottom maps to GUI bottom",
                bottom,
                100f,
                ViewportHeight - Margin - HeaderHeight,
                QuickSettingsHeaderDragTransition.None,
                true);
        }

        private static void TestDragContinuesOutsideHeader()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);

            QuickSettingsHeaderDragResult result = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    700f,
                    RawY(400f),
                    100f,
                    80f));
            Result(
                "active drag follows pointer outside header",
                result,
                650f,
                390f,
                QuickSettingsHeaderDragTransition.None,
                true);
        }

        private static void TestReleaseEndsDrag()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);
            QuickSettingsHeaderDragResult moved = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    200f,
                    RawY(120f),
                    100f,
                    80f));

            QuickSettingsHeaderDragResult released = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    false,
                    true,
                    230f,
                    RawY(150f),
                    moved.PanelX,
                    moved.PanelY));
            Result(
                "release applies final pointer endpoint and ends drag",
                released,
                180f,
                140f,
                QuickSettingsHeaderDragTransition.Ended,
                false);

            QuickSettingsHeaderDragResult laterHeld = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    500f,
                    RawY(500f),
                    released.PanelX,
                    released.PanelY));
            Result(
                "movement after release needs a fresh press",
                laterHeld,
                released.PanelX,
                released.PanelY,
                QuickSettingsHeaderDragTransition.None,
                false);
        }

        private static void TestInterruptedReleaseDoesNotJump()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);
            QuickSettingsHeaderDragResult moved = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    200f,
                    RawY(120f),
                    100f,
                    80f));
            Result(
                "interrupted-release movement precondition",
                moved,
                150f,
                110f,
                QuickSettingsHeaderDragTransition.None,
                true);

            QuickSettingsHeaderDragResult interrupted = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    false,
                    false,
                    900f,
                    RawY(700f),
                    moved.PanelX,
                    moved.PanelY));
            Result(
                "missing MouseUp cancels without a cursor jump",
                interrupted,
                moved.PanelX,
                moved.PanelY,
                QuickSettingsHeaderDragTransition.Cancelled,
                false);
        }

        private static void TestHiddenPanelCancelsDrag()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);

            QuickSettingsHeaderDragResult cancelled = state.Step(
                Frame(
                    false,
                    true,
                    false,
                    true,
                    false,
                    300f,
                    RawY(300f),
                    100f,
                    80f));
            Result(
                "hiding panel cancels active drag",
                cancelled,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.Cancelled,
                false);
        }

        private static void TestReopenWhileHeldDoesNotBegin()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);
            QuickSettingsHeaderDragResult hidden = state.Step(
                Frame(
                    false,
                    true,
                    false,
                    true,
                    false,
                    180f,
                    RawY(100f),
                    100f,
                    80f));
            Result(
                "hide cancels before reopen test",
                hidden,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.Cancelled,
                false);

            QuickSettingsHeaderDragResult reopened = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    180f,
                    RawY(90f),
                    hidden.PanelX,
                    hidden.PanelY));
            Result(
                "reopening with button held cannot begin drag",
                reopened,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                false);
        }

        private static void TestFocusLossCancelsDrag()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);

            QuickSettingsHeaderDragResult cancelled = state.Step(
                Frame(
                    true,
                    false,
                    false,
                    true,
                    false,
                    300f,
                    RawY(300f),
                    100f,
                    80f));
            Result(
                "focus loss cancels active drag",
                cancelled,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.Cancelled,
                false);
        }

        private static void TestFreshPressWorksAfterCancellation()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);
            state.Cancel(100f, 80f);

            QuickSettingsHeaderDragResult held = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    180f,
                    RawY(100f),
                    100f,
                    80f));
            Result(
                "cancelled drag ignores stale held input",
                held,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.None,
                false);

            QuickSettingsHeaderDragResult restarted = state.Step(
                Frame(
                    true,
                    true,
                    true,
                    true,
                    false,
                    180f,
                    RawY(90f),
                    100f,
                    80f));
            Result(
                "fresh press restarts drag after cancel",
                restarted,
                100f,
                80f,
                QuickSettingsHeaderDragTransition.Began,
                true);
        }

        private static void TestDraggedPanelKeepsHeaderReachable()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                100f,
                80f,
                150f,
                90f);

            QuickSettingsHeaderDragResult upperLeft = state.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    -10000f,
                    RawY(-10000f),
                    100f,
                    80f));
            Result(
                "drag clamps upper-left to reachable header",
                upperLeft,
                Margin + VisibleHeaderWidth - PanelWidth,
                Margin,
                QuickSettingsHeaderDragTransition.None,
                true);

            QuickSettingsHeaderDragStateMachine second = BeginDrag(
                100f,
                80f,
                150f,
                90f);
            QuickSettingsHeaderDragResult lowerRight = second.Step(
                Frame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    10000f,
                    RawY(10000f),
                    100f,
                    80f));
            Result(
                "drag clamps lower-right to reachable header",
                lowerRight,
                ViewportWidth - Margin - VisibleHeaderWidth,
                ViewportHeight - Margin - HeaderHeight,
                QuickSettingsHeaderDragTransition.None,
                true);
        }

        private static void TestViewportResizeReclampsActiveDrag()
        {
            QuickSettingsHeaderDragStateMachine state = BeginDrag(
                1300f,
                700f,
                1350f,
                710f);

            QuickSettingsHeaderDragResult result = state.Step(
                new QuickSettingsHeaderDragFrame(
                    true,
                    true,
                    false,
                    true,
                    false,
                    1350f,
                    -110f,
                    1300f,
                    700f,
                    PanelWidth,
                    800f,
                    600f,
                    Margin,
                    HeaderHeight,
                    VisibleHeaderWidth));
            Result(
                "viewport resize reclamps active panel",
                result,
                800f - Margin - VisibleHeaderWidth,
                600f - Margin - HeaderHeight,
                QuickSettingsHeaderDragTransition.None,
                true);
        }

        private static QuickSettingsHeaderDragStateMachine BeginDrag(
            float panelX,
            float panelY,
            float pointerX,
            float pointerGuiY)
        {
            QuickSettingsHeaderDragStateMachine state =
                new QuickSettingsHeaderDragStateMachine();
            QuickSettingsHeaderDragResult result = state.Step(
                Frame(
                    true,
                    true,
                    true,
                    true,
                    false,
                    pointerX,
                    RawY(pointerGuiY),
                    panelX,
                    panelY));
            Result(
                "begin drag precondition",
                result,
                panelX,
                panelY,
                QuickSettingsHeaderDragTransition.Began,
                true);
            return state;
        }

        private static QuickSettingsHeaderDragFrame Frame(
            bool panelVisible,
            bool applicationFocused,
            bool primaryPressed,
            bool primaryHeld,
            bool primaryReleased,
            float pointerInputX,
            float pointerInputY,
            float panelX,
            float panelY)
        {
            return new QuickSettingsHeaderDragFrame(
                panelVisible,
                applicationFocused,
                primaryPressed,
                primaryHeld,
                primaryReleased,
                pointerInputX,
                pointerInputY,
                panelX,
                panelY,
                PanelWidth,
                ViewportWidth,
                ViewportHeight,
                Margin,
                HeaderHeight,
                VisibleHeaderWidth);
        }

        private static float RawY(float guiY)
        {
            return ViewportHeight - guiY;
        }

        private static void Result(
            string name,
            QuickSettingsHeaderDragResult actual,
            float expectedX,
            float expectedY,
            QuickSettingsHeaderDragTransition expectedTransition,
            bool expectedDragging)
        {
            Near(name + " X", expectedX, actual.PanelX);
            Near(name + " Y", expectedY, actual.PanelY);
            Check(
                name + " transition",
                actual.Transition == expectedTransition);
            Check(name + " dragging", actual.IsDragging == expectedDragging);
        }

        private static void Near(string name, float expected, float actual)
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
