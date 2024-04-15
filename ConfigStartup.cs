using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MyceliumNetworking;
using UnityEngine;

namespace ConfigSync
{
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, true)]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(MyceliumNetworking.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class ConfigStartup : BaseUnityPlugin
    {
        public static ConfigStartup Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }
        internal static ushort modID = (ushort)Hash128.Compute(MyPluginInfo.PLUGIN_GUID).GetHashCode();

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            MyceliumNetwork.RegisterNetworkObject(this, modID);
            MyceliumNetwork.LobbyEntered += Synchronizer.OnLobbyEntered;
            MyceliumNetwork.LobbyDataUpdated += Synchronizer.OnLobbyDataUpdated;

            Patch();
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
