namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal enum QuickSettingsHeaderDragTransition
    {
        None = 0,
        Began = 1,
        Ended = 2,
        Cancelled = 3
    }

    internal struct QuickSettingsHeaderDragFrame
    {
        internal QuickSettingsHeaderDragFrame(
            bool panelVisible,
            bool applicationFocused,
            bool primaryPressed,
            bool primaryHeld,
            bool primaryReleased,
            float pointerInputX,
            float pointerInputY,
            float panelX,
            float panelY,
            float panelWidth,
            float viewportWidth,
            float viewportHeight,
            float screenMargin,
            float headerHeight,
            float minimumVisibleHeaderWidth)
        {
            PanelVisible = panelVisible;
            ApplicationFocused = applicationFocused;
            PrimaryPressed = primaryPressed;
            PrimaryHeld = primaryHeld;
            PrimaryReleased = primaryReleased;
            PointerInputX = pointerInputX;
            PointerInputY = pointerInputY;
            PanelX = panelX;
            PanelY = panelY;
            PanelWidth = panelWidth;
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            ScreenMargin = screenMargin;
            HeaderHeight = headerHeight;
            MinimumVisibleHeaderWidth = minimumVisibleHeaderWidth;
        }

        internal bool PanelVisible { get; private set; }
        internal bool ApplicationFocused { get; private set; }
        internal bool PrimaryPressed { get; private set; }
        internal bool PrimaryHeld { get; private set; }
        internal bool PrimaryReleased { get; private set; }
        internal float PointerInputX { get; private set; }
        internal float PointerInputY { get; private set; }
        internal float PanelX { get; private set; }
        internal float PanelY { get; private set; }
        internal float PanelWidth { get; private set; }
        internal float ViewportWidth { get; private set; }
        internal float ViewportHeight { get; private set; }
        internal float ScreenMargin { get; private set; }
        internal float HeaderHeight { get; private set; }
        internal float MinimumVisibleHeaderWidth { get; private set; }
    }

    internal struct QuickSettingsHeaderDragResult
    {
        internal QuickSettingsHeaderDragResult(
            float panelX,
            float panelY,
            QuickSettingsHeaderDragTransition transition,
            bool isDragging)
        {
            PanelX = panelX;
            PanelY = panelY;
            Transition = transition;
            IsDragging = isDragging;
        }

        internal float PanelX { get; private set; }
        internal float PanelY { get; private set; }
        internal QuickSettingsHeaderDragTransition Transition
        {
            get;
            private set;
        }

        internal bool IsDragging { get; private set; }
    }

    internal sealed class QuickSettingsHeaderDragStateMachine
    {
        private float _panelStartX;
        private float _panelStartY;
        private float _pointerStartX;
        private float _pointerStartY;
        private bool _isDragging;

        internal float PanelStartX
        {
            get { return _panelStartX; }
        }

        internal float PanelStartY
        {
            get { return _panelStartY; }
        }

        internal float PointerStartX
        {
            get { return _pointerStartX; }
        }

        internal float PointerStartY
        {
            get { return _pointerStartY; }
        }

        internal QuickSettingsHeaderDragResult Step(
            QuickSettingsHeaderDragFrame frame)
        {
            float panelX = QuickSettingsWindowPlacement.ClampHorizontal(
                frame.PanelX,
                frame.PanelWidth,
                frame.ViewportWidth,
                frame.ScreenMargin,
                frame.MinimumVisibleHeaderWidth);
            float panelY = QuickSettingsWindowPlacement.ClampVertical(
                frame.PanelY,
                frame.ViewportHeight,
                frame.ScreenMargin,
                frame.HeaderHeight);

            if (!frame.PanelVisible || !frame.ApplicationFocused)
            {
                return Cancel(panelX, panelY);
            }

            if (_isDragging)
            {
                if (!frame.PrimaryHeld && !frame.PrimaryReleased)
                {
                    return Cancel(
                        panelX,
                        panelY);
                }

                if (IsFinitePointer(frame))
                {
                    panelX = QuickSettingsWindowPlacement.ClampHorizontal(
                        QuickSettingsWindowPlacement.CalculateDraggedHorizontal(
                            _panelStartX,
                            _pointerStartX,
                            frame.PointerInputX),
                        frame.PanelWidth,
                        frame.ViewportWidth,
                        frame.ScreenMargin,
                        frame.MinimumVisibleHeaderWidth);
                    panelY = QuickSettingsWindowPlacement.ClampVertical(
                        QuickSettingsWindowPlacement.
                            CalculateDraggedVerticalFromBottomOrigin(
                                _panelStartY,
                                _pointerStartY,
                                frame.PointerInputY),
                        frame.ViewportHeight,
                        frame.ScreenMargin,
                        frame.HeaderHeight);
                }

                if (frame.PrimaryReleased || !frame.PrimaryHeld)
                {
                    _isDragging = false;
                    return Result(
                        panelX,
                        panelY,
                        QuickSettingsHeaderDragTransition.Ended);
                }

                return Result(
                    panelX,
                    panelY,
                    QuickSettingsHeaderDragTransition.None);
            }

            if (!frame.PrimaryPressed ||
                !frame.PrimaryHeld ||
                frame.PrimaryReleased ||
                !ContainsHeader(frame, panelX, panelY))
            {
                return Result(
                    panelX,
                    panelY,
                    QuickSettingsHeaderDragTransition.None);
            }

            _panelStartX = panelX;
            _panelStartY = panelY;
            _pointerStartX = frame.PointerInputX;
            _pointerStartY = frame.PointerInputY;
            _isDragging = true;
            return Result(
                panelX,
                panelY,
                QuickSettingsHeaderDragTransition.Began);
        }

        internal QuickSettingsHeaderDragResult Cancel(
            float panelX,
            float panelY)
        {
            if (!_isDragging)
            {
                return Result(
                    panelX,
                    panelY,
                    QuickSettingsHeaderDragTransition.None);
            }

            _isDragging = false;
            return Result(
                panelX,
                panelY,
                QuickSettingsHeaderDragTransition.Cancelled);
        }

        private QuickSettingsHeaderDragResult Result(
            float panelX,
            float panelY,
            QuickSettingsHeaderDragTransition transition)
        {
            return new QuickSettingsHeaderDragResult(
                panelX,
                panelY,
                transition,
                _isDragging);
        }

        private static bool ContainsHeader(
            QuickSettingsHeaderDragFrame frame,
            float panelX,
            float panelY)
        {
            if (!IsFinitePointer(frame))
            {
                return false;
            }

            float pointerGuiY =
                QuickSettingsWindowPlacement.ConvertInputYToGui(
                    frame.ViewportHeight,
                    frame.PointerInputY);
            return frame.PointerInputX >= panelX &&
                frame.PointerInputX < panelX + frame.PanelWidth &&
                pointerGuiY >= panelY &&
                pointerGuiY < panelY + frame.HeaderHeight;
        }

        private static bool IsFinitePointer(
            QuickSettingsHeaderDragFrame frame)
        {
            return !float.IsNaN(frame.PointerInputX) &&
                !float.IsInfinity(frame.PointerInputX) &&
                !float.IsNaN(frame.PointerInputY) &&
                !float.IsInfinity(frame.PointerInputY);
        }
    }
}
