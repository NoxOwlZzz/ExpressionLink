using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class ExpressionLinkTargetBinding
    {
        private const float WeightTolerance = 0.0001f;

        private float _originalWeight;
        private float _lastWritten;
        private bool _owned;

        internal ExpressionLinkTargetBinding(
            CharacterRendererRecord record,
            int blendshapeIndex,
            string blendshapeName)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            Renderer = record.Renderer;
            Mesh = record.Mesh;
            RendererInstanceId = record.RendererInstanceId;
            MeshInstanceId = record.MeshInstanceId;
            RelativePath = record.RelativePath;
            ComponentIndex = record.ComponentIndex;
            BlendshapeIndex = blendshapeIndex;
            BlendshapeName = blendshapeName ?? string.Empty;
            _originalWeight = IsValid()
                ? Renderer.GetBlendShapeWeight(BlendshapeIndex)
                : 0f;
        }

        internal SkinnedMeshRenderer Renderer { get; private set; }

        internal Mesh Mesh { get; private set; }

        internal int RendererInstanceId { get; private set; }

        internal int MeshInstanceId { get; private set; }

        internal string RelativePath { get; private set; }

        internal int ComponentIndex { get; private set; }

        internal int BlendshapeIndex { get; private set; }

        internal string BlendshapeName { get; private set; }

        internal bool Owned
        {
            get { return _owned; }
        }

        internal float OriginalWeight
        {
            get { return _originalWeight; }
        }

        internal float LastWritten
        {
            get { return _lastWritten; }
        }

        internal bool LastRestoreSkipped { get; private set; }

        internal bool IsValid()
        {
            return Renderer != null &&
                Mesh != null &&
                Renderer.sharedMesh == Mesh &&
                BlendshapeIndex >= 0 &&
                BlendshapeIndex < Mesh.blendShapeCount;
        }

        internal bool TryGetCurrentWeight(out float weight, out string error)
        {
            weight = 0f;
            error = string.Empty;
            if (!IsValid())
            {
                error = "The renderer, mesh, or blendshape is no longer valid.";
                return false;
            }

            try
            {
                weight = Renderer.GetBlendShapeWeight(BlendshapeIndex);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        internal bool Apply(float desiredWeight, out string error)
        {
            error = string.Empty;
            if (float.IsNaN(desiredWeight) || float.IsInfinity(desiredWeight))
            {
                error = "The requested blendshape weight is not finite.";
                return false;
            }

            float current;
            if (!TryGetCurrentWeight(out current, out error))
            {
                return false;
            }

            try
            {
                if (!_owned)
                {
                    _originalWeight = current;
                }

                // Reading even when the target is stable is intentional. It
                // lets this owner repair a value changed by another writer on
                // the next LateUpdate without performing redundant writes.
                if (!NearlyEqual(current, desiredWeight) ||
                    float.IsNaN(current) ||
                    float.IsInfinity(current))
                {
                    Renderer.SetBlendShapeWeight(
                        BlendshapeIndex,
                        desiredWeight);
                }

                _lastWritten = desiredWeight;
                _owned = true;
                LastRestoreSkipped = false;
                return true;
            }
            catch (Exception exception)
            {
                error = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        internal bool Restore(out string error)
        {
            error = string.Empty;
            LastRestoreSkipped = false;
            if (!_owned)
            {
                return true;
            }

            float current;
            if (!TryGetCurrentWeight(out current, out error))
            {
                return false;
            }

            try
            {
                if (NearlyEqual(current, _lastWritten))
                {
                    if (!NearlyEqual(current, _originalWeight) ||
                        float.IsNaN(current) ||
                        float.IsInfinity(current))
                    {
                        Renderer.SetBlendShapeWeight(
                            BlendshapeIndex,
                            _originalWeight);
                    }
                }
                else
                {
                    // Another owner changed this channel after our last write.
                    // Relinquish it without overwriting that external value.
                    LastRestoreSkipped = true;
                }

                _owned = false;
                _lastWritten = 0f;
                return true;
            }
            catch (Exception exception)
            {
                error = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private static bool NearlyEqual(float left, float right)
        {
            return Math.Abs(left - right) <= WeightTolerance;
        }
    }
}
