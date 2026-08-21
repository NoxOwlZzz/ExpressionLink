using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class CharacterRendererRecord
    {
        internal CharacterRendererRecord(
            SkinnedMeshRenderer renderer,
            Mesh mesh,
            string relativePath,
            int componentIndex,
            ExpressionTargetScope scope,
            int slotIndex)
        {
            Renderer = renderer;
            Mesh = mesh;
            RelativePath = relativePath ?? string.Empty;
            ComponentIndex = componentIndex;
            Scope = scope;
            SlotIndex = slotIndex;
            RendererInstanceId = renderer == null ? 0 : renderer.GetInstanceID();
            MeshInstanceId = mesh == null ? 0 : mesh.GetInstanceID();
            RendererName = renderer == null ? string.Empty : renderer.name;
            MeshName = mesh == null ? string.Empty : mesh.name;
        }

        internal SkinnedMeshRenderer Renderer { get; private set; }

        internal Mesh Mesh { get; private set; }

        internal string RelativePath { get; private set; }

        internal int ComponentIndex { get; private set; }

        internal ExpressionTargetScope Scope { get; private set; }

        internal int SlotIndex { get; private set; }

        internal int RendererInstanceId { get; private set; }

        internal int MeshInstanceId { get; private set; }

        internal string RendererName { get; private set; }

        internal string MeshName { get; private set; }

        internal bool IsValid
        {
            get
            {
                return Renderer != null &&
                    Mesh != null &&
                    Renderer.sharedMesh == Mesh;
            }
        }

        internal int FindBlendshape(string name)
        {
            return Mesh == null || string.IsNullOrEmpty(name)
                ? -1
                : Mesh.GetBlendShapeIndex(name);
        }
    }

    internal sealed class CharacterRendererCatalog
    {
        private CharacterRendererRecord[] _records =
            new CharacterRendererRecord[0];

        internal ChaControl Owner { get; private set; }

        internal int OwnerInstanceId { get; private set; }

        internal int Count
        {
            get { return _records.Length; }
        }

        internal CharacterRendererRecord GetRecord(int index)
        {
            return index >= 0 && index < _records.Length
                ? _records[index]
                : null;
        }

        internal void Rebuild(ChaControl owner)
        {
            Owner = owner;
            OwnerInstanceId = owner == null ? 0 : owner.GetInstanceID();
            if (owner == null)
            {
                _records = new CharacterRendererRecord[0];
                return;
            }

            SkinnedMeshRenderer[] discovered;
            try
            {
                discovered =
                    owner.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            }
            catch (Exception)
            {
                _records = new CharacterRendererRecord[0];
                return;
            }

            List<CharacterRendererRecord> records =
                new List<CharacterRendererRecord>(discovered.Length);
            Dictionary<int, bool> seen = new Dictionary<int, bool>();
            for (int i = 0; i < discovered.Length; i++)
            {
                SkinnedMeshRenderer renderer = discovered[i];
                if (renderer == null ||
                    renderer.GetComponentInParent<ChaControl>() != owner)
                {
                    continue;
                }

                int instanceId = renderer.GetInstanceID();
                if (seen.ContainsKey(instanceId))
                {
                    continue;
                }

                seen.Add(instanceId, true);
                ExpressionTargetScope scope;
                int slotIndex;
                Classify(owner, renderer.transform, out scope, out slotIndex);
                records.Add(new CharacterRendererRecord(
                    renderer,
                    renderer.sharedMesh,
                    RendererPathUtility.GetRelativePath(
                        owner.transform,
                        renderer.transform),
                    GetComponentIndex(renderer),
                    scope,
                    slotIndex));
            }

            _records = records.ToArray();
        }

        private static int GetComponentIndex(SkinnedMeshRenderer renderer)
        {
            if (renderer == null || renderer.gameObject == null)
            {
                return -1;
            }

            SkinnedMeshRenderer[] components =
                renderer.gameObject.GetComponents<SkinnedMeshRenderer>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == renderer)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void Classify(
            ChaControl owner,
            Transform target,
            out ExpressionTargetScope scope,
            out int slotIndex)
        {
            slotIndex = -1;

            slotIndex = RendererPathUtility.FindContainingRoot(
                target,
                owner.objHair);
            if (slotIndex >= 0)
            {
                scope = ExpressionTargetScope.Hair;
                return;
            }

            slotIndex = RendererPathUtility.FindContainingRoot(
                target,
                owner.objClothes);
            if (slotIndex < 0)
            {
                slotIndex = RendererPathUtility.FindContainingRoot(
                    target,
                    owner.objParts);
            }

            if (slotIndex >= 0)
            {
                scope = ExpressionTargetScope.Clothes;
                return;
            }

            slotIndex = RendererPathUtility.FindContainingRoot(
                target,
                owner.objAccessory);
            if (slotIndex >= 0)
            {
                scope = ExpressionTargetScope.Accessory;
                return;
            }

            slotIndex = -1;
            if (RendererPathUtility.IsUnder(target, owner.objHead) ||
                RendererPathUtility.IsUnder(target, owner.objHeadBone))
            {
                scope = ExpressionTargetScope.Head;
                return;
            }

            if (RendererPathUtility.IsUnder(target, owner.objBody))
            {
                scope = ExpressionTargetScope.Body;
                return;
            }

            scope = ExpressionTargetScope.Other;
        }
    }
}
