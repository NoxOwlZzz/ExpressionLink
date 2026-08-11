namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal enum TargetSearchScope
    {
        HeadOnly,
        HeadFirst,
        Character
    }

    internal enum SourceEyeMode
    {
        Average,
        Left,
        Right
    }

    internal enum WeightRestoreMode
    {
        InitialValues,
        Zero
    }

    internal enum BindingState
    {
        Unbound,
        Searching,
        Bound,
        Ambiguous,
        Incompatible,
        Disabled
    }

    internal enum ResolveStatus
    {
        Retry,
        Bound,
        Ambiguous
    }

    internal enum ManualVisibilityMode
    {
        Original,
        Visible,
        Hidden
    }

    internal enum VisibilityResolutionStatus
    {
        Disabled,
        Ready,
        Missing,
        Ambiguous,
        Conflict,
        Invalid
    }
}
