using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KKAPI;
using KKAPI.Chara;
using ModBoneImplantor;
using UnityEngine;

namespace AccessoryBoneBinder
{
    public sealed class AccessoryBoneBinderController : CharaCustomFunctionController
    {
        private readonly List<BoundAccessoryBone> _boundBones = new List<BoundAccessoryBone>();
        private Coroutine _scheduledRebind;

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (!maintainState)
                _boundBones.Clear();
            ScheduleRebind();
            base.OnReload(currentGameMode, maintainState);
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate)
        {
            ScheduleRebind();
            base.OnCoordinateBeingLoaded(coordinate);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnDestroy()
        {
            _boundBones.Clear();
            base.OnDestroy();
        }

        internal void ScheduleRebind()
        {
            if (_scheduledRebind != null)
                StopCoroutine(_scheduledRebind);
            _scheduledRebind = StartCoroutine(DelayedRebind());
        }

        private IEnumerator DelayedRebind()
        {
            yield return null;
            yield return null;
            _scheduledRebind = null;
            RebindAccessories();
        }

        private void RebindAccessories()
        {
            if (ChaControl == null) return;
            var accessories = ChaControl.objAccessory;
            if (accessories == null) return;

            CleanupDeadBindings(accessories);
            var bodyBones = BuildBodyBoneDictionary();
            if (bodyBones.Count == 0)
            {
                AccessoryBoneBinderPlugin.Log?.LogWarning($"No body bones found for {ChaControl.name}; accessory bones cannot be bound.");
                return;
            }

            for (int slot = 0; slot < accessories.Length; slot++)
            {
                var accessoryRoot = accessories[slot];
                if (accessoryRoot == null) continue;
                BindAccessory(slot, accessoryRoot, bodyBones);
            }
        }

        private void BindAccessory(int slot, GameObject accessoryRoot, Dictionary<string, Transform> bodyBones)
        {
            var implants = accessoryRoot.GetComponentsInChildren<BoneImplantProcess>(true);
            if (implants == null || implants.Length == 0) return;

            foreach (var implant in implants)
            {
                if (implant == null) continue;
                var source = implant.trfSrc;
                var destination = implant.trfDst;
                if (source == null || destination == null || source == destination)
                {
                    AccessoryBoneBinderPlugin.Log?.LogWarning($"Invalid BoneImplantProcess in slot {slot + 1}: trfSrc/trfDst is null or identical.");
                    continue;
                }

                string targetName = destination.name;
                if (!bodyBones.TryGetValue(targetName, out var bodyParent) || bodyParent == null)
                {
                    AccessoryBoneBinderPlugin.Log?.LogWarning($"Body bone '{targetName}' was not found for accessory slot {slot + 1}, source '{source.name}'.");
                    continue;
                }

                if (IsAlreadyBound(source, accessoryRoot, slot, bodyParent))
                    continue;

                _boundBones.Add(new BoundAccessoryBone
                {
                    Slot = slot,
                    AccessoryRoot = accessoryRoot,
                    SourceRoot = source,
                    OriginalParent = source.parent,
                    OriginalLocalPosition = source.localPosition,
                    OriginalLocalRotation = source.localRotation,
                    OriginalLocalScale = source.localScale,
                    BodyParent = bodyParent,
                    TargetBoneName = targetName
                });

                source.SetParent(bodyParent, false);
                AccessoryBoneBinderPlugin.Log?.LogInfo($"Bound accessory slot {slot + 1}: {source.name} -> {targetName}");
            }
        }

        private bool IsAlreadyBound(Transform source, GameObject accessoryRoot, int slot, Transform bodyParent)
        {
            foreach (var bound in _boundBones)
            {
                if (bound.SourceRoot == source)
                {
                    if (source.parent != bodyParent)
                    {
                        source.SetParent(bodyParent, false);
                        bound.BodyParent = bodyParent;
                    }
                    bound.AccessoryRoot = accessoryRoot;
                    bound.Slot = slot;
                    return true;
                }
            }

            return false;
        }

        private Dictionary<string, Transform> BuildBodyBoneDictionary()
        {
            var result = new Dictionary<string, Transform>();
            var root = ChaControl.objBodyBone != null ? ChaControl.objBodyBone.transform : ChaControl.transform;
            foreach (var bone in root.GetComponentsInChildren<Transform>(true))
            {
                if (bone == null || string.IsNullOrEmpty(bone.name) || result.ContainsKey(bone.name))
                    continue;
                result.Add(bone.name, bone);
            }
            return result;
        }

        private void CleanupDeadBindings(GameObject[] accessories)
        {
            for (int i = _boundBones.Count - 1; i >= 0; i--)
            {
                var bound = _boundBones[i];
                bool sourceAlive = bound.SourceRoot != null;
                bool slotStillSame = bound.Slot >= 0 &&
                                     bound.Slot < accessories.Length &&
                                     accessories[bound.Slot] == bound.AccessoryRoot &&
                                     bound.AccessoryRoot != null;

                if (sourceAlive && !slotStillSame)
                {
                    AccessoryBoneBinderPlugin.Log?.LogDebug($"Removing detached accessory bone {bound.SourceRoot.name} from old slot {bound.Slot + 1}.");
                    Destroy(bound.SourceRoot.gameObject);
                    sourceAlive = false;
                }

                if (!sourceAlive || !slotStillSame)
                    _boundBones.RemoveAt(i);
            }
        }

        private sealed class BoundAccessoryBone
        {
            public int Slot;
            public GameObject AccessoryRoot;
            public Transform SourceRoot;
            public Transform OriginalParent;
            public Vector3 OriginalLocalPosition;
            public Quaternion OriginalLocalRotation;
            public Vector3 OriginalLocalScale;
            public Transform BodyParent;
            public string TargetBoneName;
        }
    }
}
