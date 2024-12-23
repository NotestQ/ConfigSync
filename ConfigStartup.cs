using MyceliumNetworking;
using System.Collections.Generic;
using System;
using System.Text;
using Unity.Collections;
using UnityEngine;
using Steamworks;


namespace ConfigSync
{
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, true)]
    public class ConfigStartup
    {
        static ConfigStartup()
        {
            var go = new GameObject("MyceliumNetworkingTest Persistent");
            go.AddComponent<ConfigRPC>();
            go.hideFlags = HideFlags.HideAndDontSave;

            Debug.Log($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }
    }
}
