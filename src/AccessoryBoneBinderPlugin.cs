using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;

namespace AccessoryBoneBinder
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("com.rclcircuit.bepinex.modboneimplantor")]
    public sealed class AccessoryBoneBinderPlugin : BaseUnityPlugin
    {
        public const string GUID = "tomtom.kks.accessorybonebinder";
        public const string Name = "KKS_AccessoryBoneBinder";
        public const string Version = "0.1.0.0";

        internal static ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            CharacterApi.RegisterExtraBehaviour<AccessoryBoneBinderController>(GUID);
            _harmony = new Harmony(GUID);
            _harmony.PatchAll(typeof(AccessoryHooks));
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
