using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkProfile
    {
        internal const string CurrentFormat = "kk-expression-link-profile";
        internal const int CurrentVersion = 1;
        internal const string CurrentPluginVersion = "0.5.1";
        internal const string KoikatsuGameId = "KK";

        private readonly Guid _id;
        private readonly string _name;
        private readonly string _author;
        private readonly string[] _supportedGames;
        private readonly string _createdWith;
        private readonly ExpressionLinkDefinition[] _links;

        internal ExpressionLinkProfile(
            Guid id,
            string name,
            string author,
            string[] supportedGames,
            string createdWith,
            ExpressionLinkDefinition[] links)
        {
            _id = id;
            _name = name ?? string.Empty;
            _author = author ?? string.Empty;
            _supportedGames = CopyStrings(supportedGames);
            _createdWith = createdWith ?? string.Empty;
            _links = CopyLinks(links, false);
        }

        internal Guid Id
        {
            get { return _id; }
        }

        internal string Name
        {
            get { return _name; }
        }

        internal string Author
        {
            get { return _author; }
        }

        internal string[] SupportedGames
        {
            get { return CopyStrings(_supportedGames); }
        }

        internal string CreatedWith
        {
            get { return _createdWith; }
        }

        internal ExpressionLinkDefinition[] Links
        {
            get { return CopyLinks(_links, false); }
        }

        internal static ExpressionLinkProfile Create(
            string name,
            string author,
            ExpressionLinkDefinition[] links)
        {
            return new ExpressionLinkProfile(
                Guid.NewGuid(),
                name,
                author,
                new[] { KoikatsuGameId },
                CurrentPluginVersion,
                links);
        }

        internal ExpressionLinkDefinition[] CloneForCharacter()
        {
            return CopyLinks(_links, true);
        }

        private static string[] CopyStrings(string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return new string[0];
            }

            string[] copy = new string[values.Length];
            Array.Copy(values, copy, values.Length);
            return copy;
        }

        private static ExpressionLinkDefinition[] CopyLinks(
            ExpressionLinkDefinition[] links,
            bool assignNewIds)
        {
            if (links == null || links.Length == 0)
            {
                return new ExpressionLinkDefinition[0];
            }

            ExpressionLinkDefinition[] copy = new ExpressionLinkDefinition[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                ExpressionLinkDefinition source = links[i];
                if (source == null)
                {
                    copy[i] = null;
                    continue;
                }

                ExpressionLinkDefinition clone = source.Clone();
                if (assignNewIds)
                {
                    clone.Id = Guid.NewGuid();
                }

                copy[i] = clone;
            }

            return copy;
        }
    }

    /// <summary>
    /// JSON wire model for a reusable Expression Link profile. The public field
    /// names are part of the versioned profile format.
    /// </summary>
    [Serializable]
    public sealed class ExpressionLinkProfileDocument
    {
        public string format;
        public int version;
        public string id;
        public string name;
        public string author;
        public string[] supportedGames;
        public string createdWith;
        public ExpressionLinkProfileLinkDocument[] links;
    }

    /// <summary>
    /// JSON wire model for one link in a reusable profile. The public field
    /// names are part of the versioned profile format.
    /// </summary>
    [Serializable]
    public sealed class ExpressionLinkProfileLinkDocument
    {
        public string id;
        public string name;
        public bool enabled;
        public string source;
        public int targetScope;
        public int slotIndex;
        public string rendererPath;
        public int componentIndex;
        public string rendererNameHint;
        public string meshNameHint;
        public string blendshapeName;
        public int mode;
        public float threshold;
        public float inputMin;
        public float inputMax;
        public float outputMin;
        public float outputMax;
        public float smoothingSpeed;
        public int priority;
    }
}
