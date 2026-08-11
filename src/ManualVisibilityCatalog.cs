namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal static class ManualVisibilityCatalog
    {
        internal const int BlendshapeCount = 10;
        internal const int RendererCount = 4;

        internal static readonly string[] BlendshapeDisplayNames =
        {
            "Highlight 01",
            "Highlight 02",
            "Inner iris",
            "Outer iris",
            "Pupil",
            "Sclera",
            "Normal eyes",
            "Expression 01 (fused)",
            "Expression 02 (fused)",
            "Expression 03 (fused)"
        };

        internal static readonly string[] BlendshapeConfigKeys =
        {
            "Highlight01Blendshape",
            "Highlight02Blendshape",
            "IrisInnerBlendshape",
            "IrisOuterBlendshape",
            "PupilBlendshape",
            "ScleraBlendshape",
            "NormalEyesBlendshape",
            "Expression01Blendshape",
            "Expression02Blendshape",
            "Expression03Blendshape"
        };

        internal static readonly string[] DefaultBlendshapeNames =
        {
            "eye_motion.f00_hide_highlight01",
            "eye_motion.f00_hide_highlight02",
            "eye_motion.f00_hide_irisinner",
            "eye_motion.f00_hide_irisouter",
            "eye_motion.f00_hide_pupil",
            "eye_motion.f00_hide_sclera",
            "eye_motion.f00_hide_normaleyes",
            "eye_motion.f00_hide_expression01",
            "eye_motion.f00_hide_expression02",
            "eye_motion.f00_hide_expression03"
        };

        internal static readonly string[] RendererDisplayNames =
        {
            "ExpressionMesh 01",
            "ExpressionMesh 02",
            "ExpressionMesh 03",
            "ExpressionMesh 04"
        };

        internal static readonly string[] RendererConfigKeys =
        {
            "ExpressionMesh01Target",
            "ExpressionMesh02Target",
            "ExpressionMesh03Target",
            "ExpressionMesh04Target"
        };

        internal static readonly string[] DefaultRendererTargets =
        {
            "ExpressionMesh_01",
            "ExpressionMesh_02",
            "ExpressionMesh_03",
            "ExpressionMesh_04"
        };
    }
}
