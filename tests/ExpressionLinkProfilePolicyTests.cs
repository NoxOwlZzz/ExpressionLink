using System;
using System.Reflection;

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class ExpressionLinkProfilePolicyTests
    {
        private const string PluginNamespace =
            "NightOwlZzz.Koikatsu.EyeMotion.";

        private static int _checks;
        private static int _failures;

        internal static int Run(Assembly pluginAssembly, out int checks)
        {
            _checks = 0;
            _failures = 0;

            Type profileType = pluginAssembly.GetType(
                PluginNamespace + "ExpressionLinkProfile", true);
            Type definitionType = pluginAssembly.GetType(
                PluginNamespace + "ExpressionLinkDefinition", true);
            Type codecType = pluginAssembly.GetType(
                PluginNamespace + "ExpressionLinkProfileJsonCodec", true);
            BindingFlags staticNonPublic =
                BindingFlags.Static | BindingFlags.NonPublic;
            BindingFlags instanceNonPublic =
                BindingFlags.Instance | BindingFlags.NonPublic;
            object profile = profileType.GetMethod(
                "Create", staticNonPublic).Invoke(
                    null,
                    new object[]
                    {
                        "Sample",
                        "Author",
                        Array.CreateInstance(definitionType, 0)
                    });
            string[] games = (string[])profileType.GetProperty(
                "SupportedGames", instanceNonPublic).GetValue(profile, null);
            Check(
                "new profile declares only the current game",
                games.Length == 1 && games[0] == TestGameTarget.GameId);
            Check(
                "profile creation assigns an ID",
                (Guid)profileType.GetProperty(
                    "Id", instanceNonPublic).GetValue(profile, null) != Guid.Empty);
            Check(
                "profile format remains compatible",
                (string)profileType.GetField(
                    "CurrentFormat", staticNonPublic).GetRawConstantValue() ==
                    "kk-expression-link-profile");
            Check(
                "profile schema remains version 1",
                (int)profileType.GetField(
                    "CurrentVersion", staticNonPublic).GetRawConstantValue() == 1);

            // Metadata validation is managed; JsonUtility itself requires the Unity player.
            MethodInfo normalize = codecType.GetMethod(
                "TryNormalizeMetadata", staticNonPublic);
            string[] normalized;
            string error;
            Check(
                "current-game profile is accepted",
                Normalize(normalize, new[] { TestGameTarget.GameId },
                    out normalized, out error) && error.Length == 0);
            Check(
                "other-game-only profile is rejected",
                !Normalize(normalize, new[] { TestGameTarget.OtherGameId },
                    out normalized, out error) &&
                error.IndexOf(TestGameTarget.GameName, StringComparison.Ordinal) >= 0);
            Check(
                "explicit dual-game profile is accepted",
                Normalize(normalize, new[] { "KK", "KKS" },
                    out normalized, out error) && normalized.Length == 2);
            Check(
                "current game ID is normalized case-insensitively",
                Normalize(
                    normalize,
                    new[] { " " + TestGameTarget.GameId.ToLowerInvariant() + " " },
                    out normalized, out error) &&
                normalized.Length == 1 &&
                normalized[0] == TestGameTarget.GameId);
            Check(
                "duplicate game IDs are rejected",
                !Normalize(
                    normalize,
                    new[]
                    {
                        TestGameTarget.GameId,
                        TestGameTarget.GameId.ToLowerInvariant()
                    },
                    out normalized, out error));
            Check(
                "missing game declaration is rejected",
                !Normalize(normalize, null, out normalized, out error));
            Check(
                "empty game declaration is rejected",
                !Normalize(normalize, new string[0], out normalized, out error));
            Check(
                "unknown-game-only profile is rejected",
                !Normalize(normalize, new[] { "Unknown" },
                    out normalized, out error));

            checks = _checks;
            return _failures;
        }

        private static bool Normalize(
            MethodInfo normalize,
            string[] games,
            out string[] normalized,
            out string error)
        {
            object[] arguments =
            {
                "Sample", "Author", "0.5.1", games,
                null, null, null, null, null
            };
            bool accepted = (bool)normalize.Invoke(null, arguments);
            normalized = (string[])arguments[7];
            error = (string)arguments[8];
            return accepted;
        }

        private static void Check(string name, bool condition)
        {
            _checks++;
            if (!condition)
            {
                _failures++;
                Console.Error.WriteLine("FAIL profile policy: " + name);
            }
        }
    }
}
