using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsConfigSession
    {
        private readonly IQuickSettingsConfigStore _store;

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
            Expressions = new ExpressionSettingsDraft();
            Visibility = new VisibilitySettingsDraft();
            Reload();
        }

        internal MotionSettingsDraft Motion { get; private set; }

        internal IrisSettingsDraft Iris { get; private set; }

        internal ExpressionSettingsDraft Expressions { get; private set; }

        internal VisibilitySettingsDraft Visibility { get; private set; }

        internal void Reload()
        {
            _store.Load(Motion, Iris, Expressions, Visibility);
        }

        internal void Apply()
        {
            Motion.NormalizeForSave();
            Iris.NormalizeForSave();
            Expressions.NormalizeForSave();
            Visibility.NormalizeForSave();
            _store.Save(Motion, Iris, Expressions, Visibility);
            Reload();
        }
    }
}
