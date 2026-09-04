using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkProfileStore
    {
        private const string ProfileExtension = ".json";
        private const int MaximumSanitizedNameLength = 96;

        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);
        private static readonly UTF8Encoding Utf8WithoutBom =
            new UTF8Encoding(false);

        private readonly string _rootPath;
        private readonly string _rootPrefix;

        internal ExpressionLinkProfileStore()
            : this(Path.Combine(
                Paths.ConfigPath,
                Path.Combine("KK_ExpressionLink", "Profiles")))
        {
        }

        internal ExpressionLinkProfileStore(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentException(
                    "The profile root path is empty.",
                    "rootPath");
            }

            _rootPath = Path.GetFullPath(rootPath);
            _rootPrefix = _rootPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
        }

        internal bool TrySave(
            string requestedName,
            ExpressionLinkProfile profile,
            out string savedName,
            out string error)
        {
            savedName = string.Empty;
            error = string.Empty;

            string path;
            if (!TryResolveProfilePath(
                    requestedName,
                    out savedName,
                    out path,
                    out error))
            {
                return false;
            }

            string json;
            if (!ExpressionLinkProfileJsonCodec.TrySerialize(
                    profile,
                    out json,
                    out error))
            {
                savedName = string.Empty;
                return false;
            }

            string temporaryPath = path + "." +
                Guid.NewGuid().ToString("N") + ".tmp";
            if (!IsPathInsideRoot(temporaryPath))
            {
                savedName = string.Empty;
                error = "The temporary profile path escaped the profile folder.";
                return false;
            }

            try
            {
                Directory.CreateDirectory(_rootPath);
                WriteTemporaryFile(temporaryPath, json);

                if (File.Exists(path))
                {
                    File.Replace(temporaryPath, path, null);
                }
                else
                {
                    File.Move(temporaryPath, path);
                }

                return true;
            }
            catch (UnauthorizedAccessException exception)
            {
                savedName = string.Empty;
                error = "The profile could not be saved: " + exception.Message;
                return false;
            }
            catch (IOException exception)
            {
                savedName = string.Empty;
                error = "The profile could not be saved: " + exception.Message;
                return false;
            }
            catch (NotSupportedException exception)
            {
                savedName = string.Empty;
                error = "The profile could not be saved: " + exception.Message;
                return false;
            }
            finally
            {
                TryDeleteTemporaryFile(temporaryPath);
            }
        }

        internal bool TryLoad(
            string requestedName,
            out ExpressionLinkProfile profile,
            out string error)
        {
            profile = null;
            error = string.Empty;

            string normalizedName;
            string path;
            if (!TryResolveProfilePath(
                    requestedName,
                    out normalizedName,
                    out path,
                    out error))
            {
                return false;
            }

            if (!File.Exists(path))
            {
                error = "Profile '" + normalizedName + "' was not found.";
                return false;
            }

            string json;
            if (!TryReadProfileFile(path, out json, out error))
            {
                return false;
            }

            return ExpressionLinkProfileJsonCodec.TryDeserialize(
                json,
                out profile,
                out error);
        }

        internal string[] ListNames()
        {
            string[] names;
            string error;
            if (!TryListNames(out names, out error))
            {
                throw new IOException(error);
            }

            return names;
        }

        internal bool TryListNames(
            out string[] names,
            out string error)
        {
            names = new string[0];
            error = string.Empty;

            try
            {
                if (!Directory.Exists(_rootPath))
                {
                    return true;
                }

                string[] paths = Directory.GetFiles(
                    _rootPath,
                    "*" + ProfileExtension,
                    SearchOption.TopDirectoryOnly);
                List<string> result = new List<string>(paths.Length);
                for (int i = 0; i < paths.Length; i++)
                {
                    string fullPath = Path.GetFullPath(paths[i]);
                    if (!IsPathInsideRoot(fullPath) ||
                        !string.Equals(
                            Path.GetExtension(fullPath),
                            ProfileExtension,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string name = Path.GetFileNameWithoutExtension(fullPath);
                    if (!string.IsNullOrEmpty(name))
                    {
                        result.Add(name);
                    }
                }

                names = result.ToArray();
                Array.Sort(names, StringComparer.OrdinalIgnoreCase);
                return true;
            }
            catch (UnauthorizedAccessException exception)
            {
                error = "Profiles could not be listed: " + exception.Message;
                return false;
            }
            catch (IOException exception)
            {
                error = "Profiles could not be listed: " + exception.Message;
                return false;
            }
            catch (ArgumentException exception)
            {
                error = "Profiles could not be listed: " + exception.Message;
                return false;
            }
            catch (NotSupportedException exception)
            {
                error = "Profiles could not be listed: " + exception.Message;
                return false;
            }
        }

        internal bool TryResolveProfilePath(
            string requestedName,
            out string sanitizedName,
            out string path,
            out string error)
        {
            sanitizedName = SanitizeName(requestedName);
            path = string.Empty;
            error = string.Empty;

            if (sanitizedName.Length == 0)
            {
                error = "The profile name does not contain a valid file name.";
                return false;
            }

            try
            {
                path = Path.GetFullPath(Path.Combine(
                    _rootPath,
                    sanitizedName + ProfileExtension));
            }
            catch (ArgumentException exception)
            {
                error = "The profile path is invalid: " + exception.Message;
                return false;
            }
            catch (NotSupportedException exception)
            {
                error = "The profile path is invalid: " + exception.Message;
                return false;
            }
            catch (PathTooLongException exception)
            {
                error = "The profile path is too long: " + exception.Message;
                return false;
            }

            if (!IsPathInsideRoot(path))
            {
                path = string.Empty;
                error = "The profile path escaped the profile folder.";
                return false;
            }

            return true;
        }

        internal static string SanitizeName(string requestedName)
        {
            string name = (requestedName ?? string.Empty).Trim();
            if (name.EndsWith(
                    ProfileExtension,
                    StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(
                    0,
                    name.Length - ProfileExtension.Length);
            }

            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            StringBuilder builder = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                char character = name[i];
                bool invalid = char.IsControl(character) ||
                    character == Path.DirectorySeparatorChar ||
                    character == Path.AltDirectorySeparatorChar ||
                    Array.IndexOf(invalidCharacters, character) >= 0;
                builder.Append(invalid ? '_' : character);
            }

            name = builder.ToString().Trim(' ', '.');
            while (name.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                name = name.Replace("..", "_");
            }

            if (name.Length > MaximumSanitizedNameLength)
            {
                name = name.Substring(0, MaximumSanitizedNameLength)
                    .TrimEnd(' ', '.');
            }

            if (IsReservedWindowsName(name))
            {
                name = "_" + name;
            }

            return name;
        }

        private static void WriteTemporaryFile(
            string path,
            string contents)
        {
            using (FileStream stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                4096,
                false))
            using (StreamWriter writer = new StreamWriter(
                stream,
                Utf8WithoutBom))
            {
                writer.Write(contents);
                writer.Flush();
                stream.Flush();
            }
        }

        private static bool TryReadProfileFile(
            string path,
            out string json,
            out string error)
        {
            json = string.Empty;
            error = string.Empty;

            try
            {
                byte[] bytes;
                using (FileStream stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {
                    if (stream.Length >
                        ExpressionLinkProfileJsonCodec.MaximumProfileBytes)
                    {
                        error = "The profile exceeds the 512 KiB limit.";
                        return false;
                    }

                    bytes = new byte[(int)stream.Length];
                    int offset = 0;
                    while (offset < bytes.Length)
                    {
                        int read = stream.Read(
                            bytes,
                            offset,
                            bytes.Length - offset);
                        if (read == 0)
                        {
                            error = "The profile ended while it was being read.";
                            return false;
                        }

                        offset += read;
                    }

                    if (stream.ReadByte() >= 0)
                    {
                        error = "The profile changed while it was being read.";
                        return false;
                    }
                }

                int start = bytes.Length >= 3 &&
                    bytes[0] == 0xEF &&
                    bytes[1] == 0xBB &&
                    bytes[2] == 0xBF
                    ? 3
                    : 0;
                json = StrictUtf8.GetString(
                    bytes,
                    start,
                    bytes.Length - start);
                return true;
            }
            catch (DecoderFallbackException)
            {
                error = "The profile is not valid UTF-8.";
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                error = "The profile could not be read: " + exception.Message;
                return false;
            }
            catch (IOException exception)
            {
                error = "The profile could not be read: " + exception.Message;
                return false;
            }
            catch (NotSupportedException exception)
            {
                error = "The profile could not be read: " + exception.Message;
                return false;
            }
        }

        private bool IsPathInsideRoot(string path)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return false;
            }

            return fullPath.StartsWith(
                _rootPrefix,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReservedWindowsName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            string stem = name;
            int dot = stem.IndexOf('.');
            if (dot >= 0)
            {
                stem = stem.Substring(0, dot);
            }

            stem = stem.ToUpperInvariant();
            if (stem == "CON" || stem == "PRN" || stem == "AUX" ||
                stem == "NUL")
            {
                return true;
            }

            if (stem.Length == 4 &&
                stem[3] >= '1' &&
                stem[3] <= '9')
            {
                string prefix = stem.Substring(0, 3);
                return prefix == "COM" || prefix == "LPT";
            }

            return false;
        }

        private static void TryDeleteTemporaryFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
