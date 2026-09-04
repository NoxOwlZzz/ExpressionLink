using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class BlendshapeBinding
    {
        internal readonly SkinnedMeshRenderer Renderer;
        internal readonly Mesh Mesh;
        internal readonly int RendererInstanceId;
        internal readonly int MeshInstanceId;
        internal readonly string RelativePath;

        internal readonly int PositiveXIndex;
        internal readonly int NegativeXIndex;
        internal readonly int PositiveYIndex;
        internal readonly int NegativeYIndex;
        internal readonly int BlinkIndex;
        internal readonly int[] EyeCustomizationIndices;

        internal readonly float PositiveXInitial;
        internal readonly float NegativeXInitial;
        internal readonly float PositiveYInitial;
        internal readonly float NegativeYInitial;
        internal readonly float BlinkInitial;
        internal readonly bool ManagesBlink;
        internal readonly bool ManagesEyeCustomization;

        private ManagedWeightState _positiveXState;
        private ManagedWeightState _negativeXState;
        private ManagedWeightState _positiveYState;
        private ManagedWeightState _negativeYState;
        private ManagedWeightState _blinkState;
        private readonly float[] _eyeCustomizationInitial;
        private readonly float[] _eyeCustomizationDesired;
        private readonly ManagedWeightState[] _eyeCustomizationStates;
        private readonly bool[] _eyeCustomizationOwned;
        private ExpressionControlEyeSource _eyeAdjustmentSource;
        private int _lastEyeCustomizationRevision = -1;
        private bool _hasEyeCustomizationSource;
        private float _lastIrisY;
        private float _lastIrisSize;

        internal BlendshapeBinding(RendererCandidate candidate)
        {
            Renderer = candidate.Renderer;
            Mesh = candidate.Mesh;
            RendererInstanceId = Renderer.GetInstanceID();
            MeshInstanceId = Mesh.GetInstanceID();
            RelativePath = candidate.RelativePath;

            PositiveXIndex = candidate.PositiveXIndex;
            NegativeXIndex = candidate.NegativeXIndex;
            PositiveYIndex = candidate.PositiveYIndex;
            NegativeYIndex = candidate.NegativeYIndex;
            BlinkIndex = candidate.BlinkIndex;
            EyeCustomizationIndices =
                new int[EyeCustomizationCatalog.ChannelCount];
            Array.Copy(
                candidate.EyeCustomizationIndices,
                EyeCustomizationIndices,
                EyeCustomizationIndices.Length);
            ManagesBlink =
                PluginConfig.BlinkEnabled.Value &&
                EyeStateSampler.BlinkSamplingAvailable &&
                BlinkIndex >= 0;
            for (int i = 0; i < EyeCustomizationIndices.Length; i++)
            {
                if (EyeCustomizationIndices[i] >= 0)
                {
                    ManagesEyeCustomization = true;
                    break;
                }
            }

            PositiveXInitial = ReadInitial(PositiveXIndex);
            NegativeXInitial = ReadInitial(NegativeXIndex);
            PositiveYInitial = ReadInitial(PositiveYIndex);
            NegativeYInitial = ReadInitial(NegativeYIndex);
            BlinkInitial = ReadInitial(BlinkIndex);
            _eyeCustomizationInitial =
                new float[EyeCustomizationIndices.Length];
            _eyeCustomizationDesired =
                new float[EyeCustomizationIndices.Length];
            _eyeCustomizationStates =
                new ManagedWeightState[EyeCustomizationIndices.Length];
            _eyeCustomizationOwned =
                new bool[EyeCustomizationIndices.Length];
            for (int i = 0; i < EyeCustomizationIndices.Length; i++)
            {
                float initial = ReadInitial(EyeCustomizationIndices[i]);
                _eyeCustomizationInitial[i] = initial;
                _eyeCustomizationDesired[i] = initial;
                _eyeCustomizationStates[i].Commit(initial);
            }

            _positiveXState.Commit(PositiveXInitial);
            _negativeXState.Commit(NegativeXInitial);
            _positiveYState.Commit(PositiveYInitial);
            _negativeYState.Commit(NegativeYInitial);
            _blinkState.Commit(BlinkInitial);
        }

        internal bool IsValid()
        {
            return Renderer != null &&
                   Mesh != null &&
                   Renderer.sharedMesh == Mesh;
        }

        internal bool ApplyAlreadyValidated(
            ref MappedWeights weights,
            bool applyBlink,
            out string error)
        {
            error = string.Empty;
            try
            {
                ApplyManaged(
                    PositiveXIndex,
                    weights.PositiveX,
                    ref _positiveXState);
                ApplyManaged(
                    NegativeXIndex,
                    weights.NegativeX,
                    ref _negativeXState);
                ApplyManaged(
                    PositiveYIndex,
                    weights.PositiveY,
                    ref _positiveYState);
                ApplyManaged(
                    NegativeYIndex,
                    weights.NegativeY,
                    ref _negativeYState);
                if (ManagesBlink && applyBlink)
                {
                    ApplyManaged(
                        BlinkIndex,
                        weights.Blink,
                        ref _blinkState);
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        internal bool ApplyEyeCustomizationAlreadyValidated(
            ChaControl owner,
            int frame,
            out string error)
        {
            error = string.Empty;
            if (!ManagesEyeCustomization ||
                (!HasEnabledEyeCustomizationChannel() &&
                 !HasOwnedEyeCustomizationChannel()))
            {
                return true;
            }

            try
            {
                float irisY = 0f;
                float irisSize = 0f;
                if (_eyeAdjustmentSource == null && owner != null)
                {
                    _eyeAdjustmentSource =
                        ExpressionControlEyeSource.Resolve(owner);
                }

                bool sourceAvailable =
                    _eyeAdjustmentSource != null &&
                    _eyeAdjustmentSource.TrySample(
                        out irisY,
                        out irisSize);
                if (!sourceAvailable)
                {
                    RestoreOwnedEyeCustomizationChannels();
                    _hasEyeCustomizationSource = false;
                    _lastIrisY = 0f;
                    _lastIrisSize = 0f;
                    return true;
                }

                irisY = SanitizeAdjustmentSource(
                    irisY,
                    EyeCustomizationMapper.IrisYMaximum);
                irisSize = SanitizeAdjustmentSource(
                    irisSize,
                    EyeCustomizationMapper.IrisSizeMaximum);

                bool sourceChanged = !_hasEyeCustomizationSource ||
                    !NearlyEqual(irisY, _lastIrisY) ||
                    !NearlyEqual(irisSize, _lastIrisSize);
                bool configurationChanged =
                    _lastEyeCustomizationRevision != PluginConfig.Revision;
                bool verifyStableValues =
                    ((frame + RendererInstanceId) & 63) == 0;
                if (!sourceChanged && !configurationChanged &&
                    !verifyStableValues)
                {
                    return true;
                }

                if (sourceChanged || configurationChanged)
                {
                    CalculateEyeCustomizationWeights(
                        irisY,
                        irisSize);
                    _lastIrisY = irisY;
                    _lastIrisSize = irisSize;
                    _hasEyeCustomizationSource = true;
                    _lastEyeCustomizationRevision = PluginConfig.Revision;
                }

                for (int i = 0; i < EyeCustomizationIndices.Length; i++)
                {
                    if (IsEyeCustomizationChannelEnabled(i))
                    {
                        ApplyManaged(
                            EyeCustomizationIndices[i],
                            _eyeCustomizationDesired[i],
                            ref _eyeCustomizationStates[i]);
                        if (EyeCustomizationIndices[i] >= 0)
                        {
                            _eyeCustomizationOwned[i] = true;
                        }
                    }
                    else if (_eyeCustomizationOwned[i])
                    {
                        SetIfDifferent(
                            EyeCustomizationIndices[i],
                            _eyeCustomizationInitial[i]);
                        _eyeCustomizationStates[i].Commit(
                            _eyeCustomizationInitial[i]);
                        _eyeCustomizationOwned[i] = false;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        internal bool IsManagedIndex(int index)
        {
            if (index < 0)
            {
                return false;
            }

            if (index == PositiveXIndex || index == NegativeXIndex ||
                index == PositiveYIndex || index == NegativeYIndex ||
                (ManagesBlink && index == BlinkIndex))
            {
                return true;
            }

            for (int i = 0; i < EyeCustomizationIndices.Length; i++)
            {
                if (EyeCustomizationIndices[i] == index)
                {
                    return true;
                }
            }

            return false;
        }

        internal int EyeCustomizationShapeCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < EyeCustomizationIndices.Length; i++)
                {
                    if (EyeCustomizationIndices[i] >= 0)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        internal string EyeAdjustmentSourceStatus
        {
            get
            {
                return _eyeAdjustmentSource == null
                    ? "Not sampled"
                    : _eyeAdjustmentSource.Status;
            }
        }

        internal float LastIrisY
        {
            get { return _lastIrisY; }
        }

        internal float LastIrisSize
        {
            get { return _lastIrisSize; }
        }

        internal bool RestoreBlink(out string error)
        {
            error = string.Empty;
            if (!ManagesBlink)
            {
                return true;
            }

            if (!IsValid())
            {
                error = "Blink restore skipped because the original renderer/mesh pair no longer exists.";
                return false;
            }

            try
            {
                SetIfDifferent(BlinkIndex, BlinkInitial);
                _blinkState.Reset();
                return true;
            }
            catch (Exception exception)
            {
                error = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        internal bool Restore(WeightRestoreMode mode, out string error)
        {
            error = string.Empty;
            if (!IsValid())
            {
                error = "Restore skipped because the original renderer/mesh pair no longer exists.";
                return false;
            }

            try
            {
                bool zero = mode == WeightRestoreMode.Zero;
                SetIfDifferent(
                    PositiveXIndex,
                    zero ? 0f : PositiveXInitial);
                SetIfDifferent(
                    NegativeXIndex,
                    zero ? 0f : NegativeXInitial);
                SetIfDifferent(
                    PositiveYIndex,
                    zero ? 0f : PositiveYInitial);
                SetIfDifferent(
                    NegativeYIndex,
                    zero ? 0f : NegativeYInitial);
                if (ManagesBlink)
                {
                    SetIfDifferent(BlinkIndex, zero ? 0f : BlinkInitial);
                }

                for (int i = 0; i < EyeCustomizationIndices.Length; i++)
                {
                    if (_eyeCustomizationOwned[i])
                    {
                        SetIfDifferent(
                            EyeCustomizationIndices[i],
                            zero ? 0f : _eyeCustomizationInitial[i]);
                    }
                }

                ResetManagedStates();

                return true;
            }
            catch (Exception exception)
            {
                error = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private float ReadInitial(int index)
        {
            return index < 0 ? 0f : Renderer.GetBlendShapeWeight(index);
        }

        private void Set(int index, float value)
        {
            if (index >= 0)
            {
                Renderer.SetBlendShapeWeight(index, value);
            }
        }

        private void ApplyManaged(
            int index,
            float desiredValue,
            ref ManagedWeightState state)
        {
            if (index < 0)
            {
                return;
            }

            // Skip the renderer read when the requested target differs from
            // managed state. Stable targets are sampled each LateUpdate so an
            // external write is reapplied on the next update.
            if (state.TargetChanged(desiredValue))
            {
                Set(index, desiredValue);
                state.Commit(desiredValue);
                return;
            }

            float currentValue = Renderer.GetBlendShapeWeight(index);
            if (state.CurrentNeedsCorrection(currentValue, desiredValue))
            {
                Set(index, desiredValue);
                state.Commit(desiredValue);
                return;
            }

            state.Commit(currentValue);
        }

        private void CalculateEyeCustomizationWeights(
            float irisY,
            float irisSize)
        {
            if (PluginConfig.EyeAdjustmentEnabled.Value)
            {
                _eyeCustomizationDesired[EyeCustomizationCatalog.IrisY] =
                    EyeCustomizationMapper.MapIrisY(
                        irisY,
                        PluginConfig.IrisYMaxWeight.Value);
                _eyeCustomizationDesired[EyeCustomizationCatalog.IrisSize] =
                    EyeCustomizationMapper.MapIrisSize(
                        irisSize,
                        PluginConfig.IrisSizeMaxWeight.Value);
            }
            else
            {
                for (int i = 0; i < _eyeCustomizationDesired.Length; i++)
                {
                    _eyeCustomizationDesired[i] =
                        _eyeCustomizationInitial[i];
                }
            }
        }

        private bool HasEnabledEyeCustomizationChannel()
        {
            for (int i = 0; i < EyeCustomizationIndices.Length; i++)
            {
                if (EyeCustomizationIndices[i] >= 0 &&
                    IsEyeCustomizationChannelEnabled(i))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasOwnedEyeCustomizationChannel()
        {
            for (int i = 0; i < _eyeCustomizationOwned.Length; i++)
            {
                if (_eyeCustomizationOwned[i])
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsEyeCustomizationChannelEnabled(int index)
        {
            return index >= 0 &&
                   index < EyeCustomizationCatalog.ChannelCount &&
                   PluginConfig.EyeAdjustmentEnabled.Value;
        }

        private void RestoreOwnedEyeCustomizationChannels()
        {
            for (int i = 0; i < _eyeCustomizationOwned.Length; i++)
            {
                if (!_eyeCustomizationOwned[i])
                {
                    continue;
                }

                SetIfDifferent(
                    EyeCustomizationIndices[i],
                    _eyeCustomizationInitial[i]);
                _eyeCustomizationStates[i].Commit(
                    _eyeCustomizationInitial[i]);
                _eyeCustomizationOwned[i] = false;
            }
        }

        private static bool NearlyEqual(float left, float right)
        {
            return Math.Abs(left - right) <= 0.00001f;
        }

        private static float SanitizeAdjustmentSource(
            float value,
            float maximum)
        {
            if (!DirectionalMapper.IsFinite(value) || value <= 0f)
            {
                return 0f;
            }

            return value > maximum ? maximum : value;
        }

        private void SetIfDifferent(int index, float desiredValue)
        {
            if (index < 0)
            {
                return;
            }

            float currentValue = Renderer.GetBlendShapeWeight(index);
            if (currentValue != desiredValue ||
                !DirectionalMapper.IsFinite(currentValue))
            {
                Set(index, desiredValue);
            }
        }

        private void ResetManagedStates()
        {
            _positiveXState.Reset();
            _negativeXState.Reset();
            _positiveYState.Reset();
            _negativeYState.Reset();
            _blinkState.Reset();
            for (int i = 0; i < _eyeCustomizationStates.Length; i++)
            {
                _eyeCustomizationStates[i].Reset();
                _eyeCustomizationOwned[i] = false;
            }

            _hasEyeCustomizationSource = false;
            _lastEyeCustomizationRevision = -1;
        }
    }
}
