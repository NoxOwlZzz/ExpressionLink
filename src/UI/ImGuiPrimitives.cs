using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ImGuiPrimitives
    {
        private static readonly GUILayoutOption[] NoLayoutOptions =
            new GUILayoutOption[0];

        internal static void BeginHorizontal()
        {
            GUILayout.BeginHorizontal(NoLayoutOptions);
        }

        internal static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        internal static void BeginVertical()
        {
            GUILayout.BeginVertical(NoLayoutOptions);
        }

        internal static void EndVertical()
        {
            GUILayout.EndVertical();
        }

        internal static void Space(float pixels)
        {
            GUILayout.Space(pixels);
        }
    }
}
