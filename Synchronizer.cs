using System.Collections.Generic;
using System.Reflection;
using MyceliumNetworking;
using Sirenix.Serialization;

namespace ConfigSync
{
    public static class Synchronizer
    {
        // GUID, Value
        internal static Dictionary<string, object> configDeferDictionary = new Dictionary<string, object>();
        internal static List<Configuration> configList = new List<Configuration>();

        internal static void OnLobbyEntered()
        {
            // CLogger.LogDebug($"Lobby joined, temporary event list count: {TemporaryEventList.Count}");
            foreach (KeyValuePair<string, object> entry in configDeferDictionary)
            {
                CreateOrSyncConfig(entry.Key, entry.Value);
            }
            return;
        }

        internal static void OnLobbyDataUpdated(List<string> changedKeys)
        {
            if (MyceliumNetwork.IsHost) return;

            for (int i = 0; i < changedKeys.Count; i++)
            {
                CreateOrSyncConfig(changedKeys[i]);
            }
        }

        /// <summary>
        /// Creates a config if it is the host or syncs existing lobby configs if not
        /// </summary>
        /// <param name="configGUID"></param>
        /// <param name="value"></param>
        internal static void CreateOrSyncConfig(string configGUID, object? value = null)
        {
            Configuration config = GetConfigOfGUID(configGUID);
            if (config == null) return;

            if (MyceliumNetwork.IsHost)
            {
                // Create settings
                MyceliumNetwork.SetLobbyData(configGUID, value!);
                return;
            }
            // Sync settings
            ConfigStartup.Logger.LogDebug($"Config type: {config.ConfigType}");
            var method = typeof(MyceliumNetwork).GetMethod(nameof(MyceliumNetwork.GetLobbyData), BindingFlags.Static | BindingFlags.Public);
            var genericMethod = method.MakeGenericMethod(config.ConfigType);
            var result = genericMethod.Invoke(null, [configGUID]);
            ConfigStartup.Logger.LogDebug($"Got value {result}");

            if (result == null) return;

            config.UpdateValue(result);
        }

        /// <summary>
        /// Internal. Register lobby data using the GUID as a key and creates/syncs the config if in lobby, if not in lobby it gets deferred until a lobby is joined
        /// </summary>
        /// <param name="configGUID"></param>
        /// <param name="initialValue"></param>
        internal static void AddOrDeferConfig(string configGUID, object initialValue)
        {
            MyceliumNetwork.RegisterLobbyDataKey(configGUID);
            if (MyceliumNetwork.InLobby)
            {
                CreateOrSyncConfig(configGUID, initialValue);
                return;
            }
            // Defer

            configDeferDictionary.Add(configGUID, initialValue);
        }

        /// <summary>
        /// Gets a created Configuration from the config list from its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns an existing Configuration matching the provided name</returns>
        public static Configuration GetConfigOfName(string name)
        {
            // If this was lua I could do GetConfigFromVar(string variable) and do match[variable] but this is not lua i miss lua kinda a bit
            return configList.Find(match => match.ConfigName == name);
        }

        /// <summary>
        /// Gets a created Configuration from the config list from its GUID
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>Returns an existing Configuration matching the provided GUID</returns>
        public static Configuration GetConfigOfGUID(string GUID)
        {
            return configList.Find(match => match.ConfigGUID == GUID);
        }
    }
}
