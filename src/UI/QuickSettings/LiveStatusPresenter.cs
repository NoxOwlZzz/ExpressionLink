using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class LiveStatusSnapshot
    {
        internal static readonly LiveStatusSnapshot NoCharacter =
            new LiveStatusSnapshot(
                "Character: none",
                "State: no controller",
                string.Empty,
                "ExpressionControl: no character",
                string.Empty,
                string.Empty);

        internal LiveStatusSnapshot(
            string selectedCharacterLine,
            string motionStateLine,
            string weightsLine,
            string irisStateLine,
            string highlightStateLine,
            string cardDataLine)
        {
            SelectedCharacterLine = selectedCharacterLine;
            MotionStateLine = motionStateLine;
            WeightsLine = weightsLine;
            IrisStateLine = irisStateLine;
            HighlightStateLine = highlightStateLine;
            CardDataLine = cardDataLine;
        }

        internal string SelectedCharacterLine { get; private set; }

        internal string MotionStateLine { get; private set; }

        internal string WeightsLine { get; private set; }

        internal string IrisStateLine { get; private set; }

        internal string HighlightStateLine { get; private set; }

        internal string CardDataLine { get; private set; }
    }

    internal sealed class LiveStatusPresenter
    {
        private const int MaximumCharacterNameLength = 36;

        private int _cachedFrame = -1;
        private int _cachedControllerInstanceId;
        private LiveStatusSnapshot _snapshot = LiveStatusSnapshot.NoCharacter;

        internal LiveStatusSnapshot GetSnapshot(
            EyeMotionCharacterController controller)
        {
            return GetSnapshot(controller, Time.frameCount);
        }

        internal LiveStatusSnapshot GetSnapshot(
            EyeMotionCharacterController controller,
            int frame)
        {
            int instanceId = controller == null ? 0 : controller.GetInstanceID();
            if (_cachedFrame == frame &&
                _cachedControllerInstanceId == instanceId)
            {
                return _snapshot;
            }

            _cachedFrame = frame;
            _cachedControllerInstanceId = instanceId;
            _snapshot = controller == null
                ? LiveStatusSnapshot.NoCharacter
                : BuildSnapshot(controller);
            return _snapshot;
        }

        internal void Invalidate()
        {
            _cachedFrame = -1;
            _cachedControllerInstanceId = 0;
        }

        private static LiveStatusSnapshot BuildSnapshot(
            EyeMotionCharacterController controller)
        {
            EyeState state = controller.State;
            MappedWeights weights = controller.AppliedWeights;
            string irisStateLine;
            BlendshapeBinding binding = controller.Binding;
            if (binding == null)
            {
                irisStateLine = "ExpressionControl: not bound";
            }
            else
            {
                irisStateLine =
                    "ExpressionControl: " +
                    binding.EyeAdjustmentSourceStatus +
                    " | Shapes " + binding.EyeCustomizationShapeCount +
                    "/" + EyeCustomizationCatalog.ChannelCount +
                    string.Format(
                        " | IrisY {0:F3} Size {1:F3}",
                        binding.LastIrisY,
                        binding.LastIrisSize);
            }

            return new LiveStatusSnapshot(
                "Character: " + GetDisplayCharacterName(controller),
                string.Format(
                    "State: {0} | X/Y: {1:F3} / {2:F3} | Raw X: {3:F3}",
                    controller.CurrentBindingState,
                    state.FinalHorizontal,
                    state.FinalVertical,
                    state.HorizontalSourceRaw),
                string.Format(
                    "Weights: X+ {0:F1} X- {1:F1} | Y+ {2:F1} Y- {3:F1} | B {4:F1}",
                    weights.PositiveX,
                    weights.NegativeX,
                    weights.PositiveY,
                    weights.NegativeY,
                    weights.Blink),
                irisStateLine,
                "Base highlight: " +
                (controller.BaseGameHighlightHidden ? "erased" : "visible") +
                " | Sync override: " +
                (controller.HighlightSyncEffective ? "active" : "inactive"),
                "Card data: " + controller.CardPersistenceStatus);
        }

        private static string GetDisplayCharacterName(
            EyeMotionCharacterController controller)
        {
            string name = (controller.GetCharacterName() ?? string.Empty)
                .Trim();
            if (name.Length == 0)
            {
                return "Unnamed";
            }

            if (name.Length <= MaximumCharacterNameLength)
            {
                return name;
            }

            return name.Substring(
                0,
                MaximumCharacterNameLength - 3) + "...";
        }
    }
}
