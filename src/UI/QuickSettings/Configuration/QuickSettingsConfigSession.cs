using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsConfigSession
    {
        private readonly IQuickSettingsConfigStore _store;
        private QuickSettingsConfigSnapshot _baseline;

        internal QuickSettingsConfigSession()
            : this(new PluginConfigQuickSettingsStore())
        {
        }

        internal QuickSettingsConfigSession(IQuickSettingsConfigStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            _store = store;
            Motion = new MotionSettingsDraft();
            Iris = new IrisSettingsDraft();
            Visibility = new VisibilitySettingsDraft();
            Reload();
        }

        internal MotionSettingsDraft Motion { get; private set; }

        internal IrisSettingsDraft Iris { get; private set; }

        internal VisibilitySettingsDraft Visibility { get; private set; }

        internal bool HasUnsavedChanges
        {
            get
            {
                return _baseline != null && !_baseline.Matches(
                    Motion,
                    Iris,
                    Visibility);
            }
        }

        internal bool ReloadIfClean()
        {
            if (HasUnsavedChanges)
            {
                return false;
            }

            Reload();
            return true;
        }

        internal void Reload()
        {
            _store.Load(Motion, Iris, Visibility);
            _baseline = new QuickSettingsConfigSnapshot(
                Motion, Iris, Visibility);
        }

        internal void Apply()
        {
            Motion.NormalizeForSave();
            Iris.NormalizeForSave();
            Visibility.NormalizeForSave();
            _store.Save(Motion, Iris, Visibility);
            Reload();
        }
    }
}
