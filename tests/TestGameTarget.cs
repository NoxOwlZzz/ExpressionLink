using System;
using System.IO;
using System.Reflection;

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class TestGameTarget
    {
#if KK && KKS
#error Select only one test game target.
#elif KKS
        internal const string GameId = "KKS";
        internal const string GameName = "Koikatsu Sunshine";
        internal const string OtherGameId = "KK";
        internal const string MainProcess = "KoikatsuSunshine.exe";
        internal const string OtherMainProcess = "Koikatu.exe";
        internal const string PluginAssemblyName = "KKS_EyeMotion";
        internal const string OtherPluginAssemblyName = "KK_EyeMotion";
        internal const string ApiAssemblyName = "KKSAPI";
        internal const string OtherApiAssemblyName = "KKAPI";
        internal const string ExtendedSaveAssemblyName = "KKS_ExtensibleSaveFormat";
        internal const string UnityRuntimeAssemblyName = "UnityEngine.CoreModule";
        internal const string ReferenceDirectory = @"lib\KKS";
        internal const string ReleaseDirectory = @"bin\KKS\Release";
        internal const int MscorlibMajorVersion = 4;
        internal const string ClrVersionPrefix = "v4.";
#elif KK
        internal const string GameId = "KK";
        internal const string GameName = "Koikatsu";
        internal const string OtherGameId = "KKS";
        internal const string MainProcess = "Koikatu.exe";
        internal const string OtherMainProcess = "KoikatsuSunshine.exe";
        internal const string PluginAssemblyName = "KK_EyeMotion";
        internal const string OtherPluginAssemblyName = "KKS_EyeMotion";
        internal const string ApiAssemblyName = "KKAPI";
        internal const string OtherApiAssemblyName = "KKSAPI";
        internal const string ExtendedSaveAssemblyName = "ExtensibleSaveFormat";
        internal const string UnityRuntimeAssemblyName = "UnityEngine";
        internal const string ReferenceDirectory = "lib";
        internal const string ReleaseDirectory = @"bin\Release";
        internal const int MscorlibMajorVersion = 2;
        internal const string ClrVersionPrefix = "v2.";
#else
#error A KK or KKS test game target is required.
#endif

        internal static string FindProjectRoot()
        {
            DirectoryInfo candidate =
                new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (candidate != null)
            {
                if (File.Exists(Path.Combine(
                        candidate.FullName, "KK_EyeMotion.csproj")) &&
                    File.Exists(Path.Combine(
                        candidate.FullName, @"src\Plugin.cs")))
                {
                    return candidate.FullName;
                }

                candidate = candidate.Parent;
            }

            throw new DirectoryNotFoundException(
                "Run the tests from their build output inside the source repository.");
        }

        internal static ResolveEventHandler CreateReferenceResolver(
            string projectRoot)
        {
            string referenceRoot =
                Path.Combine(projectRoot, ReferenceDirectory);
            return delegate(object sender, ResolveEventArgs args)
            {
                string name = new AssemblyName(args.Name).Name;
                string path = Path.Combine(referenceRoot, name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
        }
    }
}
