using System;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal enum ExpressionLinkMode : byte
    {
        Binary = 0,
        FollowSource = 1
    }

    internal enum ExpressionTargetScope : byte
    {
        Any = 0,
        Head = 1,
        Hair = 2,
        Body = 3,
        Clothes = 4,
        Accessory = 5,
        Other = 6
    }

    internal sealed class ExpressionLinkDefinition
    {
        internal Guid Id { get; set; }

        internal string Name { get; set; }

        internal bool Enabled { get; set; }

        internal string Source { get; set; }

        internal ExpressionTargetScope Scope { get; set; }

        internal int SlotIndex { get; set; }

        internal string RendererPath { get; set; }

        internal int ComponentIndex { get; set; }

        internal string RendererHint { get; set; }

        internal string MeshHint { get; set; }

        internal string BlendshapeName { get; set; }

        internal ExpressionLinkMode Mode { get; set; }

        internal float Threshold { get; set; }

        internal float InputMin { get; set; }

        internal float InputMax { get; set; }

        internal float OutputMin { get; set; }

        internal float OutputMax { get; set; }

        internal float SmoothingSpeed { get; set; }

        internal int Priority { get; set; }

        internal static ExpressionLinkDefinition CreateDefault()
        {
            return new ExpressionLinkDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Expression Link",
                Enabled = true,
                Source = string.Empty,
                Scope = ExpressionTargetScope.Any,
                SlotIndex = -1,
                RendererPath = string.Empty,
                ComponentIndex = -1,
                RendererHint = string.Empty,
                MeshHint = string.Empty,
                BlendshapeName = string.Empty,
                Mode = ExpressionLinkMode.Binary,
                Threshold = 0.001f,
                InputMin = 0f,
                InputMax = 1f,
                OutputMin = 0f,
                OutputMax = 100f,
                SmoothingSpeed = 0f,
                Priority = 0
            };
        }

        internal ExpressionLinkDefinition Clone()
        {
            return new ExpressionLinkDefinition
            {
                Id = Id,
                Name = Name,
                Enabled = Enabled,
                Source = Source,
                Scope = Scope,
                SlotIndex = SlotIndex,
                RendererPath = RendererPath,
                ComponentIndex = ComponentIndex,
                RendererHint = RendererHint,
                MeshHint = MeshHint,
                BlendshapeName = BlendshapeName,
                Mode = Mode,
                Threshold = Threshold,
                InputMin = InputMin,
                InputMax = InputMax,
                OutputMin = OutputMin,
                OutputMax = OutputMax,
                SmoothingSpeed = SmoothingSpeed,
                Priority = Priority
            };
        }
    }
}
