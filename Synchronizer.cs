using System.Collections.Generic;
using MyceliumNetworking;
using Steamworks;

/*
 * Player B is the host in the following cases
 *
 * Player A joins Player B's lobby
 * Player A requests Player B's data for given config GUID (Player B gets RequestSync and sends Player A ReceiveSync)
 * Player B sends Player A the data (Player A gets ReceiveSync)
 * | Alternatively |
 * Player B detects that Player A joined the lobby
 * Player B sends Player A the data (Player A gets ReceiveSync)
 *
 * Both ways should be implemented so a user or developer can request sync if needed, but the latter should be used for sync when someone joins
 * Situation A is player -> host | Situation B is player -> host -> player
 *
 * Currently there is only one case where a player requests sync, when a config is created while they're already in a lobby
 */

namespace ConfigSync
{
    public static class Synchronizer
    {
        public static List<Configuration> ConfigList = new List<Configuration>();

        internal static void OnPlayerEntered(CSteamID cSteamID)
        {
            if (!MyceliumNetwork.IsHost || MyceliumNetwork.LobbyHost == cSteamID)
                return;

            foreach (Configuration configuration in ConfigList)
            {
                ConfigStartup.RPCTargetRelay(nameof(ConfigStartup.ReceiveSync), cSteamID, configuration.ConfigGUID, configuration.ConfigType, configuration.CurrentValue);
            }
        }

        /// <summary>
        /// Once a player leaves a lobby we reset the CurrentValue back to InitialValue
        /// InitialValue can be set by SetValue if not the host, ContentValue cannot as it is the temporary config that mirrors the host's config
        /// Which means InitialValue is the absolute truth (the config the player wants to have, not the temporary config)
        /// </summary>
        internal static void OnLobbyLeft()
        {
            for (int i = 0; i < ConfigList.Count; i++)
            {
                Configuration config = ConfigList[i];
                config.UpdateValue(config.InitialValue);
            }
        }

        /// <summary>
        /// Syncs existing configs with the host if player, if host syncs configs with every player in the lobby
        /// </summary>
        /// <param name="configGUID"></param>
        /// <param name="value"></param>
        internal static void SyncConfig(string configGUID, object? value = null)
        {
            if (!MyceliumNetwork.InLobby)
            {
                ConfigStartup.Logger.LogWarning($"SyncConfig called but was not in lobby!");
                return;
            }

            Configuration? config = GetConfigOfGUID(configGUID);
            if (config == null) return;

            if (MyceliumNetwork.IsHost)
            {
                foreach (CSteamID cSteamID in MyceliumNetwork.Players)
                {
                    if (MyceliumNetwork.LobbyHost == cSteamID)
                        continue;

                    ConfigStartup.RPCTargetRelay(nameof(ConfigStartup.ReceiveSync), cSteamID, configGUID, config.ConfigType, value!);
                }
                return;
            }

            ConfigStartup.RPCTargetRelay(nameof(ConfigStartup.RequestSync), MyceliumNetwork.LobbyHost, configGUID);
        }

        /// <summary>
        /// Gets a created Configuration from the config list from its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns an existing Configuration matching the provided name</returns>
        public static Configuration? GetConfigOfName(string name)
        {
            return ConfigList.Find(match => match.ConfigName == name);
        }

        /// <summary>
        /// Gets a created Configuration from the config list from its GUID
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>Returns an existing Configuration matching the provided GUID</returns>
        public static Configuration? GetConfigOfGUID(string GUID)
        {
            return ConfigList.Find(match => match.ConfigGUID == GUID);
        }
    }
}
