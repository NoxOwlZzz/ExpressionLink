namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsHeaderDragGesture
    {
        private QuickSettingsPoint _grabOffset;
        private int _pointerId;
        private bool _active;

        internal bool Active
        {
            get { return _active; }
        }

        internal bool Begin(
            int pointerId,
            QuickSettingsPoint pointer,
            QuickSettingsWindowBounds bounds)
        {
            if (_active)
            {
                return false;
            }

            _pointerId = pointerId;
            _grabOffset = new QuickSettingsPoint(
                pointer.X - bounds.X,
                pointer.Y - bounds.Y);
            _active = true;
            return true;
        }

        internal bool TryGetWindowPosition(
            int pointerId,
            QuickSettingsPoint pointer,
            out QuickSettingsPoint windowPosition)
        {
            if (!_active || pointerId != _pointerId)
            {
                windowPosition = new QuickSettingsPoint(0f, 0f);
                return false;
            }

            windowPosition = new QuickSettingsPoint(
                pointer.X - _grabOffset.X,
                pointer.Y - _grabOffset.Y);
            return true;
        }

        internal bool End(int pointerId)
        {
            if (!_active || pointerId != _pointerId)
            {
                return false;
            }

            Cancel();
            return true;
        }

        internal void Cancel()
        {
            _active = false;
            _pointerId = 0;
            _grabOffset = new QuickSettingsPoint(0f, 0f);
        }
    }
}
