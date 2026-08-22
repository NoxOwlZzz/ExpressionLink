namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class QuickSettingsWindowHostTests
    {
        private static int _checks;
        private static int _failures;

        internal static int Run(out int checks)
        {
            _checks = 0;
            _failures = 0;

            RepeatedViewportUpdateHasNoWriter();
            MoveToPreservesRequestedPosition();
            ResizeClampsOnceAndRestoresPreferredSize();
            TinyViewportCanShrinkBelowMinimumSize();
            VisibilityDoesNotMoveWindow();
            GesturePreservesGrabOffsetAndStopsOnRelease();
            GestureRejectsSecondPointerAndCancels();

            checks = _checks;
            return _failures;
        }

        private static void RepeatedViewportUpdateHasNoWriter()
        {
            QuickSettingsWindowHost host = CreateHost(
                new QuickSettingsWindowBounds(24f, 72f, 500f, 520f),
                1920f,
                1080f);
            IsTrue("idle setup move", host.MoveTo(333f, 222f));
            QuickSettingsWindowBounds moved = host.Bounds;
            int changeEvents = 0;
            host.BoundsChanged += delegate { changeEvents++; };

            for (int index = 0; index < 1000; index++)
            {
                IsFalse(
                    "same viewport update " + index,
                    host.UpdateViewport(1920f, 1080f));
            }

            BoundsEqual("same viewport preserves moved bounds", moved, host.Bounds);
            Equal("same viewport emits no event", 0, changeEvents);
        }

        private static void MoveToPreservesRequestedPosition()
        {
            QuickSettingsWindowHost host = CreateHost(
                new QuickSettingsWindowBounds(24f, 72f, 500f, 520f),
                1920f,
                1080f);
            int changeEvents = 0;
            host.BoundsChanged += delegate { changeEvents++; };

            IsTrue("move changes position", host.MoveTo(320f, 240f));
            Equal("move preserves x", 320f, host.Bounds.X);
            Equal("move preserves y", 240f, host.Bounds.Y);
            IsFalse("same move is idle", host.MoveTo(320f, 240f));
            Equal("move emits one event", 1, changeEvents);
        }

        private static void ResizeClampsOnceAndRestoresPreferredSize()
        {
            QuickSettingsWindowHost host = CreateHost(
                new QuickSettingsWindowBounds(1300f, 800f, 500f, 520f),
                1920f,
                1080f);
            int changeEvents = 0;
            host.BoundsChanged += delegate { changeEvents++; };

            IsTrue(
                "resize changes fitted bounds",
                host.UpdateViewport(420f, 330f));
            Equal("resize clamps width", 404f, host.Bounds.Width);
            Equal("resize clamps height", 314f, host.Bounds.Height);
            Equal("resize keeps header reachable x", 292f, host.Bounds.X);
            Equal("resize keeps header reachable y", 298f, host.Bounds.Y);
            IsFalse(
                "same resized viewport is idle",
                host.UpdateViewport(420f, 330f));
            Equal("resize emits one event", 1, changeEvents);

            IsTrue(
                "expanded viewport restores preferred size",
                host.UpdateViewport(1920f, 1080f));
            Equal("expanded preferred width", 500f, host.Bounds.Width);
            Equal("expanded preferred height", 520f, host.Bounds.Height);
            Equal("resize sequence emits two events", 2, changeEvents);
        }

        private static void TinyViewportCanShrinkBelowMinimumSize()
        {
            QuickSettingsWindowHost host = CreateHost(
                new QuickSettingsWindowBounds(24f, 72f, 500f, 520f),
                1920f,
                1080f);

            IsTrue(
                "tiny viewport changes bounds",
                host.UpdateViewport(100f, 80f));
            Equal("tiny viewport width", 84f, host.Bounds.Width);
            Equal("tiny viewport height", 64f, host.Bounds.Height);
            Equal("tiny viewport header x", 8f, host.Bounds.X);
            Equal("tiny viewport header y", 48f, host.Bounds.Y);

            IsTrue(
                "tiny viewport expansion restores preferred size",
                host.UpdateViewport(1920f, 1080f));
            Equal("tiny expanded width", 500f, host.Bounds.Width);
            Equal("tiny expanded height", 520f, host.Bounds.Height);
        }

        private static void VisibilityDoesNotMoveWindow()
        {
            QuickSettingsWindowHost host = CreateHost(
                new QuickSettingsWindowBounds(175f, 135f, 500f, 520f),
                1920f,
                1080f);
            QuickSettingsWindowBounds initial = host.Bounds;
            int boundsEvents = 0;
            int visibilityEvents = 0;
            host.BoundsChanged += delegate { boundsEvents++; };
            host.VisibilityChanged += delegate { visibilityEvents++; };

            host.Show();
            host.Show();
            host.Hide();
            host.Hide();

            BoundsEqual("show and hide preserve bounds", initial, host.Bounds);
            Equal("show and hide emit no bounds events", 0, boundsEvents);
            Equal("show and hide emit edge events", 2, visibilityEvents);
        }

        private static void GesturePreservesGrabOffsetAndStopsOnRelease()
        {
            QuickSettingsWindowHost host = CreateHost(
                new QuickSettingsWindowBounds(100f, 200f, 500f, 520f),
                1920f,
                1080f);
            QuickSettingsHeaderDragGesture gesture =
                new QuickSettingsHeaderDragGesture();
            IsTrue(
                "gesture begins",
                gesture.Begin(
                    -1,
                    new QuickSettingsPoint(140f, 212f),
                    host.Bounds));

            QuickSettingsPoint target;
            IsTrue(
                "active gesture produces target",
                gesture.TryGetWindowPosition(
                    -1,
                    new QuickSettingsPoint(400f, 300f),
                    out target));
            Equal("grab offset x", 360f, target.X);
            Equal("grab offset y", 288f, target.Y);
            host.MoveTo(target.X, target.Y);
            QuickSettingsWindowBounds released = host.Bounds;

            IsTrue("matching pointer releases", gesture.End(-1));
            IsFalse(
                "released gesture has no writer",
                gesture.TryGetWindowPosition(
                    -1,
                    new QuickSettingsPoint(800f, 600f),
                    out target));
            BoundsEqual("release preserves last position", released, host.Bounds);
        }

        private static void GestureRejectsSecondPointerAndCancels()
        {
            QuickSettingsWindowBounds bounds =
                new QuickSettingsWindowBounds(100f, 200f, 500f, 520f);
            QuickSettingsHeaderDragGesture gesture =
                new QuickSettingsHeaderDragGesture();

            IsTrue(
                "first pointer begins",
                gesture.Begin(
                    -1,
                    new QuickSettingsPoint(140f, 212f),
                    bounds));
            IsFalse(
                "second pointer is rejected",
                gesture.Begin(
                    4,
                    new QuickSettingsPoint(900f, 700f),
                    bounds));

            QuickSettingsPoint target;
            IsTrue(
                "first pointer retains ownership",
                gesture.TryGetWindowPosition(
                    -1,
                    new QuickSettingsPoint(160f, 232f),
                    out target));
            Equal("first pointer retained x offset", 120f, target.X);
            Equal("first pointer retained y offset", 220f, target.Y);
            IsFalse(
                "second pointer cannot move",
                gesture.TryGetWindowPosition(
                    4,
                    new QuickSettingsPoint(500f, 500f),
                    out target));

            gesture.Cancel();
            IsFalse("cancel clears active gesture", gesture.Active);
            IsFalse(
                "cancelled gesture has no target",
                gesture.TryGetWindowPosition(
                    -1,
                    new QuickSettingsPoint(200f, 260f),
                    out target));
            IsTrue(
                "new pointer can begin after cancel",
                gesture.Begin(
                    4,
                    new QuickSettingsPoint(120f, 220f),
                    bounds));
        }

        private static QuickSettingsWindowHost CreateHost(
            QuickSettingsWindowBounds bounds,
            float viewportWidth,
            float viewportHeight)
        {
            return new QuickSettingsWindowHost(
                bounds,
                viewportWidth,
                viewportHeight,
                new QuickSettingsWindowConstraints(
                    360f,
                    300f,
                    8f,
                    24f,
                    120f));
        }

        private static void BoundsEqual(
            string name,
            QuickSettingsWindowBounds expected,
            QuickSettingsWindowBounds actual)
        {
            Equal(name + " x", expected.X, actual.X);
            Equal(name + " y", expected.Y, actual.Y);
            Equal(name + " width", expected.Width, actual.Width);
            Equal(name + " height", expected.Height, actual.Height);
        }

        private static void IsTrue(string name, bool value)
        {
            _checks++;
            if (!value)
            {
                _failures++;
                System.Console.Error.WriteLine("FAIL " + name);
            }
        }

        private static void IsFalse(string name, bool value)
        {
            IsTrue(name, !value);
        }

        private static void Equal(string name, int expected, int actual)
        {
            _checks++;
            if (expected != actual)
            {
                _failures++;
                System.Console.Error.WriteLine(
                    "FAIL " + name + ": expected " + expected +
                    ", got " + actual);
            }
        }

        private static void Equal(string name, float expected, float actual)
        {
            _checks++;
            if (System.Math.Abs(expected - actual) > 0.0001f)
            {
                _failures++;
                System.Console.Error.WriteLine(
                    "FAIL " + name + ": expected " + expected +
                    ", got " + actual);
            }
        }
    }
}
