using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ExpressionLinkProfileJsonCodec
    {
        internal const int MaximumProfileBytes = 512 * 1024;

        private const int MaximumProfileNameBytes = 256;
        private const int MaximumAuthorBytes = 256;
        private const int MaximumCreatedWithBytes = 64;
        private const int MaximumSupportedGames = 8;
        private const int MaximumGameIdBytes = 32;

        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        internal static bool TrySerialize(
            ExpressionLinkProfile profile,
            out string json,
            out string error)
        {
            json = string.Empty;
            error = string.Empty;

            if (profile == null)
            {
                error = "The expression-link profile is null.";
                return false;
            }

            if (profile.Id == Guid.Empty)
            {
                error = "The profile ID is empty.";
                return false;
            }

            string name;
            string author;
            string createdWith;
            string[] supportedGames;
            if (!TryNormalizeMetadata(
                    profile.Name,
                    profile.Author,
                    profile.CreatedWith,
                    profile.SupportedGames,
                    out name,
                    out author,
                    out createdWith,
                    out supportedGames,
                    out error))
            {
                return false;
            }

            ExpressionLinkDefinition[] links;
            if (!ExpressionLinkValidator.TryNormalizeList(
                    profile.Links,
                    out links,
                    out error))
            {
                return false;
            }

            ExpressionLinkProfileDocument document =
                new ExpressionLinkProfileDocument
                {
                    format = ExpressionLinkProfile.CurrentFormat,
                    version = ExpressionLinkProfile.CurrentVersion,
                    id = profile.Id.ToString("N"),
                    name = name,
                    author = author,
                    supportedGames = supportedGames,
                    createdWith = createdWith,
                    links = ToDocuments(links)
                };

            try
            {
                json = JsonUtility.ToJson(document, true);
            }
            catch (ArgumentException exception)
            {
                error = "The profile could not be serialized: " +
                    exception.Message;
                return false;
            }

            int byteCount;
            if (!TryGetByteCount(json, out byteCount, out error))
            {
                json = string.Empty;
                return false;
            }

            if (byteCount > MaximumProfileBytes)
            {
                json = string.Empty;
                error = "The serialized profile exceeds the 512 KiB limit.";
                return false;
            }

            return true;
        }

        internal static bool TryDeserialize(
            string json,
            out ExpressionLinkProfile profile,
            out string error)
        {
            profile = null;
            error = string.Empty;

            if (string.IsNullOrEmpty(json))
            {
                error = "The profile file is empty.";
                return false;
            }

            int byteCount;
            if (!TryGetByteCount(json, out byteCount, out error))
            {
                return false;
            }

            if (byteCount > MaximumProfileBytes)
            {
                error = "The profile exceeds the 512 KiB limit.";
                return false;
            }

            string trimmed = json.Trim();
            if (trimmed.Length < 2 ||
                trimmed[0] != '{' ||
                trimmed[trimmed.Length - 1] != '}')
            {
                error = "The profile is not a JSON object.";
                return false;
            }

            ExpressionLinkProfileDocument document;
            try
            {
                document = JsonUtility.FromJson<ExpressionLinkProfileDocument>(
                    trimmed);
            }
            catch (ArgumentException exception)
            {
                error = "The profile JSON is invalid: " + exception.Message;
                return false;
            }

            if (document == null)
            {
                error = "The profile JSON has no document.";
                return false;
            }

            if (!string.Equals(
                    document.format,
                    ExpressionLinkProfile.CurrentFormat,
                    StringComparison.Ordinal))
            {
                error = "The profile format is not supported.";
                return false;
            }

            if (document.version != ExpressionLinkProfile.CurrentVersion)
            {
                error = "Profile version " + document.version +
                    " is not supported.";
                return false;
            }

            Guid profileId;
            if (!TryParseGuidN(document.id, out profileId) ||
                profileId == Guid.Empty)
            {
                error = "The profile ID is invalid.";
                return false;
            }

            string name;
            string author;
            string createdWith;
            string[] supportedGames;
            if (!TryNormalizeMetadata(
                    document.name,
                    document.author,
                    document.createdWith,
                    document.supportedGames,
                    out name,
                    out author,
                    out createdWith,
                    out supportedGames,
                    out error))
            {
                return false;
            }

            if (document.links == null)
            {
                error = "The profile links array is missing.";
                return false;
            }

            if (document.links.Length > ExpressionLinkLimits.MaximumLinks)
            {
                error = "The profile contains more than " +
                    ExpressionLinkLimits.MaximumLinks + " links.";
                return false;
            }

            ExpressionLinkDefinition[] candidates =
                new ExpressionLinkDefinition[document.links.Length];
            for (int i = 0; i < document.links.Length; i++)
            {
                ExpressionLinkProfileLinkDocument item = document.links[i];
                if (item == null)
                {
                    error = "Profile link " + i + " is null.";
                    return false;
                }

                Guid linkId;
                if (!TryParseGuidN(item.id, out linkId) ||
                    linkId == Guid.Empty)
                {
                    error = "Profile link " + i + " has an invalid ID.";
                    return false;
                }

                candidates[i] = FromDocument(item, linkId);
            }

            ExpressionLinkDefinition[] links;
            string validationError;
            if (!ExpressionLinkValidator.TryNormalizeList(
                    candidates,
                    out links,
                    out validationError))
            {
                error = "The profile links are invalid: " + validationError;
                return false;
            }

            profile = new ExpressionLinkProfile(
                profileId,
                name,
                author,
                supportedGames,
                createdWith,
                links);
            return true;
        }

        private static ExpressionLinkProfileLinkDocument[] ToDocuments(
            ExpressionLinkDefinition[] links)
        {
            ExpressionLinkProfileLinkDocument[] documents =
                new ExpressionLinkProfileLinkDocument[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                ExpressionLinkDefinition link = links[i];
                documents[i] = new ExpressionLinkProfileLinkDocument
                {
                    id = link.Id.ToString("N"),
                    name = link.Name,
                    enabled = link.Enabled,
                    source = link.Source,
                    targetScope = (int)link.Scope,
                    slotIndex = link.SlotIndex,
                    rendererPath = link.RendererPath,
                    componentIndex = link.ComponentIndex,
                    rendererNameHint = link.RendererHint,
                    meshNameHint = link.MeshHint,
                    blendshapeName = link.BlendshapeName,
                    mode = (int)link.Mode,
                    threshold = link.Threshold,
                    inputMin = link.InputMin,
                    inputMax = link.InputMax,
                    outputMin = link.OutputMin,
                    outputMax = link.OutputMax,
                    smoothingSpeed = link.SmoothingSpeed,
                    priority = link.Priority
                };
            }

            return documents;
        }

        private static ExpressionLinkDefinition FromDocument(
            ExpressionLinkProfileLinkDocument item,
            Guid id)
        {
            return new ExpressionLinkDefinition
            {
                Id = id,
                Name = item.name,
                Enabled = item.enabled,
                Source = item.source,
                Scope = (ExpressionTargetScope)item.targetScope,
                SlotIndex = item.slotIndex,
                RendererPath = item.rendererPath,
                ComponentIndex = item.componentIndex,
                RendererHint = item.rendererNameHint,
                MeshHint = item.meshNameHint,
                BlendshapeName = item.blendshapeName,
                Mode = (ExpressionLinkMode)item.mode,
                Threshold = item.threshold,
                InputMin = item.inputMin,
                InputMax = item.inputMax,
                OutputMin = item.outputMin,
                OutputMax = item.outputMax,
                SmoothingSpeed = item.smoothingSpeed,
                Priority = item.priority
            };
        }

        private static bool TryNormalizeMetadata(
            string inputName,
            string inputAuthor,
            string inputCreatedWith,
            string[] inputSupportedGames,
            out string name,
            out string author,
            out string createdWith,
            out string[] supportedGames,
            out string error)
        {
            name = (inputName ?? string.Empty).Trim();
            author = (inputAuthor ?? string.Empty).Trim();
            createdWith = (inputCreatedWith ?? string.Empty).Trim();
            supportedGames = new string[0];
            error = string.Empty;

            if (name.Length == 0)
            {
                error = "The profile name is empty.";
                return false;
            }

            if (createdWith.Length == 0)
            {
                error = "The profile creator version is empty.";
                return false;
            }

            if (!HasValidByteLength(
                    name,
                    MaximumProfileNameBytes,
                    "profile name",
                    out error) ||
                !HasValidByteLength(
                    author,
                    MaximumAuthorBytes,
                    "profile author",
                    out error) ||
                !HasValidByteLength(
                    createdWith,
                    MaximumCreatedWithBytes,
                    "creator version",
                    out error))
            {
                return false;
            }

            if (inputSupportedGames == null ||
                inputSupportedGames.Length == 0 ||
                inputSupportedGames.Length > MaximumSupportedGames)
            {
                error = "The supported-games list is missing or too large.";
                return false;
            }

            supportedGames = new string[inputSupportedGames.Length];
            HashSet<string> seen =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool supportsCurrentGame = false;
            for (int i = 0; i < inputSupportedGames.Length; i++)
            {
                string gameId =
                    (inputSupportedGames[i] ?? string.Empty).Trim();
                if (gameId.Length == 0 ||
                    !HasValidByteLength(
                        gameId,
                        MaximumGameIdBytes,
                        "game ID",
                        out error))
                {
                    return false;
                }

                if (!seen.Add(gameId))
                {
                    error = "The supported-games list contains duplicates.";
                    return false;
                }

                if (string.Equals(
                        gameId,
                        GameCompatibility.GameId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    gameId = GameCompatibility.GameId;
                    supportsCurrentGame = true;
                }

                supportedGames[i] = gameId;
            }

            if (!supportsCurrentGame)
            {
                error = "This profile does not declare " + GameCompatibility.GameName + " support.";
                return false;
            }

            return true;
        }

        private static bool HasValidByteLength(
            string value,
            int maximumBytes,
            string fieldName,
            out string error)
        {
            int byteCount;
            if (!TryGetByteCount(value, out byteCount, out error))
            {
                error = "The " + fieldName + " contains invalid text.";
                return false;
            }

            if (byteCount > maximumBytes)
            {
                error = "The " + fieldName + " is too long.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool TryGetByteCount(
            string value,
            out int byteCount,
            out string error)
        {
            byteCount = 0;
            error = string.Empty;
            try
            {
                byteCount = StrictUtf8.GetByteCount(value ?? string.Empty);
                return true;
            }
            catch (EncoderFallbackException)
            {
                error = "The profile contains invalid UTF-16 text.";
                return false;
            }
        }

        private static bool TryParseGuidN(string value, out Guid result)
        {
            result = Guid.Empty;
            if (value == null || value.Length != 32)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                bool hexadecimal =
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f') ||
                    (character >= 'A' && character <= 'F');
                if (!hexadecimal)
                {
                    return false;
                }
            }

            try
            {
                result = new Guid(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}
