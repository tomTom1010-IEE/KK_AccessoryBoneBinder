using BepInEx.Bootstrap;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KKABMX.Core;
using UnityEngine;

namespace AccessoryBoneBinder
{
    internal static class AbmxBridge
    {
        private const string AbmxGuid = "KKABMX.Core";
        private static readonly PropertyInfo BoneTransformProperty = typeof(BoneModifier).GetProperty(nameof(BoneModifier.BoneTransform), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly PropertyInfo BoneLocationProperty = typeof(BoneModifier).GetProperty(nameof(BoneModifier.BoneLocation), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static bool RequestRefreshIfAvailable(ChaControl chaControl, IDictionary<string, Transform> reboundBones)
        {
            if (chaControl == null || !Chainloader.PluginInfos.ContainsKey(AbmxGuid))
                return false;

            return RequestRefresh(chaControl, reboundBones);
        }

        private static bool RequestRefresh(ChaControl chaControl, IDictionary<string, Transform> reboundBones)
        {
            var abmx = chaControl.GetComponent<KKABMX.Core.BoneController>();
            if (abmx == null)
                return false;

            var targets = reboundBones?
                .Where(x => !string.IsNullOrEmpty(x.Key) && x.Value != null)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Last().Value);
            if (targets == null || targets.Count == 0)
                return false;

            foreach (var target in targets)
            {
                var boneName = target.Key;
                var sourceTransform = target.Value;
                var modifiers = abmx.GetAllModifiers().Where(x => x.BoneName == boneName).ToList();
                var bodyModifier = abmx.GetOrAddModifier(boneName, BoneLocation.BodyTop);
                var sourceModifier = modifiers.FirstOrDefault(x => !ReferenceEquals(x, bodyModifier) && !x.IsEmpty());

                if (sourceModifier != null)
                {
                    bodyModifier.CoordinateModifiers = sourceModifier.CoordinateModifiers.Select(x => x.Clone()).ToArray();
                    ClearModifierData(sourceModifier);
                }

                ForceAssignBodyModifier(bodyModifier, sourceTransform);

                foreach (var modifier in modifiers)
                {
                    modifier.ClearBaseline();
                    if (!ReferenceEquals(modifier, bodyModifier) && modifier.BoneLocation >= BoneLocation.Accessory)
                        ClearModifierData(modifier);
                }

                AccessoryBoneBinderPlugin.Log?.LogDebug($"Prepared ABMX modifier '{boneName}' as BodyTop after accessory bone binding.");
            }

            abmx.BoneSearcher?.ClearCache(false);
            abmx.NeedsFullRefresh = true;
            return true;
        }

        private static void ForceAssignBodyModifier(BoneModifier modifier, Transform transform)
        {
            BoneLocationProperty?.SetValue(modifier, BoneLocation.BodyTop, null);
            BoneTransformProperty?.SetValue(modifier, transform, null);
            modifier.ClearBaseline();
        }

        private static void ClearModifierData(BoneModifier modifier)
        {
            if (modifier?.CoordinateModifiers == null)
                return;

            modifier.CoordinateModifiers = modifier.CoordinateModifiers.Select(_ => new BoneModifierData()).ToArray();
            modifier.ClearBaseline();
        }
    }
}
