using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class RendererPathUtility
    {
        internal static string GetRelativePath(
            Transform root,
            Transform target)
        {
            if (root == null || target == null)
            {
                return string.Empty;
            }

            if (root == target)
            {
                return ".";
            }

            Stack<string> segments = new Stack<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                segments.Push(current.name);
                current = current.parent;
            }

            return current == root
                ? string.Join("/", segments.ToArray())
                : string.Empty;
        }

        internal static string NormalizeRelativePath(
            Transform root,
            string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string normalized = path.Trim().Replace('\\', '/').Trim('/');
            if (string.Equals(normalized, ".", StringComparison.Ordinal))
            {
                return ".";
            }

            while (normalized.IndexOf("//", StringComparison.Ordinal) >= 0)
            {
                normalized = normalized.Replace("//", "/");
            }

            if (root != null)
            {
                string rootName = root.name ?? string.Empty;
                if (string.Equals(normalized, rootName, StringComparison.Ordinal))
                {
                    return ".";
                }

                string prefix = rootName + "/";
                if (normalized.StartsWith(prefix, StringComparison.Ordinal))
                {
                    normalized = normalized.Substring(prefix.Length);
                }
            }

            return normalized;
        }

        internal static bool IsUnder(Transform target, GameObject root)
        {
            if (target == null || root == null)
            {
                return false;
            }

            Transform expected = root.transform;
            Transform current = target;
            while (current != null)
            {
                if (current == expected)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        internal static int FindContainingRoot(
            Transform target,
            GameObject[] roots)
        {
            if (target == null || roots == null)
            {
                return -1;
            }

            for (int i = 0; i < roots.Length; i++)
            {
                if (IsUnder(target, roots[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
