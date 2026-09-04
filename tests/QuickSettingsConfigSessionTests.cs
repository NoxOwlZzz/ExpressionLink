using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class PluginConfigQuickSettingsStore :
        IQuickSettingsConfigStore
    {
        public void Load(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            VisibilitySettingsDraft visibility)
        {
            throw new InvalidOperationException(
                "The default game store is not available in pure tests.");
        }

        public void Save(
            MotionSettingsDraft motion,
            IrisSettingsDraft iris,
            VisibilitySettingsDraft visibility)
        {
            throw new InvalidOperationException(
                "The default game store is not available in pure tests.");
        }
    }
}

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class QuickSettingsConfigSessionTests
    {
        private static int _checks;
        private static int _failures;

        internal static int Run(out int checks)
        {
            _checks = 0;
            _failures = 0;

            ReloadUsesInjectedStore();
            CleanSessionRefreshesExternalSettings();
            DirtySessionPreservesLocalDraft();
            ApplyNormalizesAndSavesOnce();

            checks = _checks;
            return _failures;
        }

        private static void ReloadUsesInjectedStore()
        {
            FakeQuickSettingsConfigStore store =
                new FakeQuickSettingsConfigStore();
            store.LoadEnabled = true;
            store.LoadPositiveXInputLimit = 0.42f;
            store.LoadIrisName = "iris_a";
            store.LoadVisibilityName = "hide_a";

            QuickSettingsConfigSession session =
                new QuickSettingsConfigSession(store);
            Equal("constructor loads once", 1, store.LoadCalls);
            IsTrue("motion loaded", session.Motion.Enabled);
            Equal(
                "motion limit loaded",
                0.42f,
                session.Motion.PositiveXInputLimit);
            Equal(
                "iris name loaded",
                "iris_a",
                session.Iris.GetBlendshapeName(0));
            Equal(
                "visibility name loaded",
                "hide_a",
                session.Visibility.GetBlendshapeName(0));
            Equal(
                "seven visibility names exposed",
                7,
                session.Visibility.BlendshapeNameCount);

            store.LoadEnabled = false;
            store.LoadPositiveXInputLimit = 0.73f;
            store.LoadIrisName = "iris_b";
            store.LoadVisibilityName = "hide_b";
            session.Reload();

            Equal("explicit reload count", 2, store.LoadCalls);
            IsFalse("motion reloaded", session.Motion.Enabled);
            Equal(
                "motion limit reloaded",
                0.73f,
                session.Motion.PositiveXInputLimit);
            Equal(
                "iris name reloaded",
                "iris_b",
                session.Iris.GetBlendshapeName(0));
            Equal(
                "visibility name reloaded",
                "hide_b",
                session.Visibility.GetBlendshapeName(0));
        }

        private static void CleanSessionRefreshesExternalSettings()
        {
            FakeQuickSettingsConfigStore store =
                new FakeQuickSettingsConfigStore();
            store.LoadEnabled = false;
            store.LoadPositiveXInputLimit = 0.24f;

            QuickSettingsConfigSession session =
                new QuickSettingsConfigSession(store);
            store.LoadEnabled = true;
            store.LoadPositiveXInputLimit = 0.81f;

            IsTrue("clean session reloads", session.ReloadIfClean());
            Equal("clean reload count", 2, store.LoadCalls);
            IsTrue("external enabled value refreshed", session.Motion.Enabled);
            Equal(
                "external limit refreshed",
                0.81f,
                session.Motion.PositiveXInputLimit);
            IsFalse("clean reload remains clean", session.HasUnsavedChanges);

            session.Iris.SetBlendshapeName(0, "iris_local");
            IsTrue("iris draft is tracked", session.HasUnsavedChanges);
            session.Reload();
            IsFalse("iris reload clears dirty", session.HasUnsavedChanges);

            session.Visibility.SetBlendshapeName(0, "hide_local");
            IsTrue("visibility draft is tracked", session.HasUnsavedChanges);
            session.Reload();
            IsFalse(
                "visibility reload clears dirty",
                session.HasUnsavedChanges);
        }

        private static void DirtySessionPreservesLocalDraft()
        {
            FakeQuickSettingsConfigStore store =
                new FakeQuickSettingsConfigStore();
            store.LoadEnabled = false;
            store.LoadPositiveXInputLimit = 0.22f;

            QuickSettingsConfigSession session =
                new QuickSettingsConfigSession(store);
            session.Motion.Enabled = true;
            store.LoadPositiveXInputLimit = 0.88f;

            IsTrue("edited session is dirty", session.HasUnsavedChanges);
            IsFalse("dirty session skips reload", session.ReloadIfClean());
            Equal("dirty reload count unchanged", 1, store.LoadCalls);
            IsTrue("local enabled edit preserved", session.Motion.Enabled);
            Equal(
                "clean field remains from draft snapshot",
                0.22f,
                session.Motion.PositiveXInputLimit);

            session.Reload();
            Equal("explicit discard reloads", 2, store.LoadCalls);
            IsFalse("external enabled value restored", session.Motion.Enabled);
            Equal(
                "external limit restored",
                0.88f,
                session.Motion.PositiveXInputLimit);
            IsFalse("explicit reload clears dirty", session.HasUnsavedChanges);
        }

        private static void ApplyNormalizesAndSavesOnce()
        {
            FakeQuickSettingsConfigStore store =
                new FakeQuickSettingsConfigStore();
            QuickSettingsConfigSession session =
                new QuickSettingsConfigSession(store);

            session.Motion.HorizontalCenterOffset = 2f;
            session.Motion.PositiveXInputLimit = 0f;
            session.Motion.NegativeXInputLimit = 2f;
            session.Motion.PositiveXMaxWeight = -5f;
            session.Motion.NegativeXMaxWeight = 105f;
            session.Motion.VerticalCenterOffset = -2f;
            session.Motion.PositiveYInputLimit = -1f;
            session.Motion.NegativeYInputLimit = 5f;
            session.Motion.PositiveYMaxWeight = -3f;
            session.Motion.NegativeYMaxWeight = 103f;
            session.Motion.BlinkMaxWeight = 250f;
            session.Motion.SmoothingSpeed = 0f;
            session.Iris.IrisYMaxWeight = -2f;
            session.Iris.IrisSizeMaxWeight = 102f;
            session.Iris.SetBlendshapeName(0, "  iris_saved  ");
            session.Visibility.ManualHideBlendshapeWeight = 150f;
            session.Visibility.SetBlendshapeName(0, "  hide_saved  ");

            session.Apply();

            Equal("one store save", 1, store.SaveCalls);
            Equal("apply reloads once", 2, store.LoadCalls);
            Equal("horizontal center max", 1f, store.SavedHorizontalCenter);
            Equal("positive x limit min", 0.05f, store.SavedPositiveXLimit);
            Equal("negative x limit max", 1f, store.SavedNegativeXLimit);
            Equal("positive x weight min", 0f, store.SavedPositiveXWeight);
            Equal("negative x weight max", 100f, store.SavedNegativeXWeight);
            Equal("vertical center min", -1f, store.SavedVerticalCenter);
            Equal("positive y limit min", 0.05f, store.SavedPositiveYLimit);
            Equal("negative y limit max", 1f, store.SavedNegativeYLimit);
            Equal("positive y weight min", 0f, store.SavedPositiveYWeight);
            Equal("negative y weight max", 100f, store.SavedNegativeYWeight);
            Equal("blink weight max", 100f, store.SavedBlinkWeight);
            Equal("smoothing speed min", 0.01f, store.SavedSmoothingSpeed);
            Equal("iris y min", 0f, store.SavedIrisYWeight);
            Equal("iris size max", 100f, store.SavedIrisSizeWeight);
            Equal("visibility weight max", 100f, store.SavedVisibilityWeight);
            Equal("iris name trimmed", "iris_saved", store.SavedIrisName);
            Equal(
                "visibility name trimmed",
                "hide_saved",
                store.SavedVisibilityName);
        }

        private static void IsTrue(string name, bool value)
        {
            _checks++;
            if (!value)
            {
                _failures++;
                Console.Error.WriteLine("FAIL " + name);
            }
        }

        private static void IsFalse(string name, bool value)
        {
            IsTrue(name, !value);
        }

        private static void Equal(string name, int expected, int actual)
        {
            IsTrue(name + ": expected " + expected + ", got " + actual,
                expected == actual);
        }

        private static void Equal(string name, float expected, float actual)
        {
            IsTrue(
                name + ": expected " + expected + ", got " + actual,
                Math.Abs(expected - actual) <= 0.0001f);
        }

        private static void Equal(string name, string expected, string actual)
        {
            IsTrue(
                name + ": expected " + expected + ", got " + actual,
                string.Equals(expected, actual, StringComparison.Ordinal));
        }

        private sealed class FakeQuickSettingsConfigStore :
            IQuickSettingsConfigStore
        {
            internal int LoadCalls;
            internal int SaveCalls;
            internal bool LoadEnabled;
            internal float LoadPositiveXInputLimit;
            internal string LoadIrisName = string.Empty;
            internal string LoadVisibilityName = string.Empty;

            internal float SavedHorizontalCenter;
            internal float SavedPositiveXLimit;
            internal float SavedNegativeXLimit;
            internal float SavedPositiveXWeight;
            internal float SavedNegativeXWeight;
            internal float SavedVerticalCenter;
            internal float SavedPositiveYLimit;
            internal float SavedNegativeYLimit;
            internal float SavedPositiveYWeight;
            internal float SavedNegativeYWeight;
            internal float SavedBlinkWeight;
            internal float SavedSmoothingSpeed;
            internal float SavedIrisYWeight;
            internal float SavedIrisSizeWeight;
            internal float SavedVisibilityWeight;
            internal string SavedIrisName = string.Empty;
            internal string SavedVisibilityName = string.Empty;

            public void Load(
                MotionSettingsDraft motion,
                IrisSettingsDraft iris,
                VisibilitySettingsDraft visibility)
            {
                LoadCalls++;
                motion.Enabled = LoadEnabled;
                motion.PositiveXInputLimit = LoadPositiveXInputLimit;
                iris.SetBlendshapeName(0, LoadIrisName);
                visibility.SetBlendshapeName(0, LoadVisibilityName);
            }

            public void Save(
                MotionSettingsDraft motion,
                IrisSettingsDraft iris,
                VisibilitySettingsDraft visibility)
            {
                SaveCalls++;
                SavedHorizontalCenter = motion.HorizontalCenterOffset;
                SavedPositiveXLimit = motion.PositiveXInputLimit;
                SavedNegativeXLimit = motion.NegativeXInputLimit;
                SavedPositiveXWeight = motion.PositiveXMaxWeight;
                SavedNegativeXWeight = motion.NegativeXMaxWeight;
                SavedVerticalCenter = motion.VerticalCenterOffset;
                SavedPositiveYLimit = motion.PositiveYInputLimit;
                SavedNegativeYLimit = motion.NegativeYInputLimit;
                SavedPositiveYWeight = motion.PositiveYMaxWeight;
                SavedNegativeYWeight = motion.NegativeYMaxWeight;
                SavedBlinkWeight = motion.BlinkMaxWeight;
                SavedSmoothingSpeed = motion.SmoothingSpeed;
                SavedIrisYWeight = iris.IrisYMaxWeight;
                SavedIrisSizeWeight = iris.IrisSizeMaxWeight;
                SavedVisibilityWeight = visibility.ManualHideBlendshapeWeight;
                SavedIrisName = iris.GetBlendshapeName(0);
                SavedVisibilityName = visibility.GetBlendshapeName(0);

                LoadEnabled = motion.Enabled;
                LoadPositiveXInputLimit = motion.PositiveXInputLimit;
                LoadIrisName = SavedIrisName;
                LoadVisibilityName = SavedVisibilityName;
            }
        }
    }
}
