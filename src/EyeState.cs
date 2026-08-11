namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal struct EyeState
    {
        public bool LookAvailable;
        public bool BlinkAvailable;

        public float LeftHorizontal;
        public float RightHorizontal;
        public float LeftHorizontalCommon;
        public float RightHorizontalCommon;
        public float HorizontalSourceRaw;
        public float Vertical;
        public float FinalHorizontal;
        public float FinalVertical;

        public float BlinkOpenRate;
        public float CurrentOpen;
        public float EffectiveMaximumOpen;
        public float Closure;

        public int LastSampleFrame;
        public int LastApplicationFrame;

        public void Reset()
        {
            LookAvailable = false;
            BlinkAvailable = false;
            LeftHorizontal = 0f;
            RightHorizontal = 0f;
            LeftHorizontalCommon = 0f;
            RightHorizontalCommon = 0f;
            HorizontalSourceRaw = 0f;
            Vertical = 0f;
            FinalHorizontal = 0f;
            FinalVertical = 0f;
            BlinkOpenRate = 1f;
            CurrentOpen = 1f;
            EffectiveMaximumOpen = 1f;
            Closure = 0f;
            LastSampleFrame = -1;
            LastApplicationFrame = -1;
        }
    }

    internal struct MappedWeights
    {
        public float PositiveX;
        public float NegativeX;
        public float PositiveY;
        public float NegativeY;
        public float Blink;

        public void Reset()
        {
            PositiveX = 0f;
            NegativeX = 0f;
            PositiveY = 0f;
            NegativeY = 0f;
            Blink = 0f;
        }
    }
}
