using MyceliumNetworking;
using System;

namespace ConfigSync
{
    public class Configuration
    {
        /// <summary>
        /// Creates a new configuration with the value provided.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="GUID"></param>
        /// <param name="initialValue"></param>
        public Configuration(string name, string GUID, object initialValue) {
            ConfigName = name;
            ConfigGUID = GUID;
            InitialValue = initialValue;
            CurrentValue = initialValue;
            ConfigType = initialValue.GetType();

            ConfigStartup.Logger.LogDebug($"Config created by end user, initial value: {initialValue}");

            Synchronizer.ConfigList.Add(this);

            if (MyceliumNetwork.InLobby)
            {
                Synchronizer.SyncConfig(ConfigGUID, CurrentValue);
            }
        }

        /// <summary>
        /// Updates the current value internally and invokes the ConfigChanged event making it possible for the end user to
        /// handle updates in a custom manner
        /// </summary>
        /// <param name="value"></param>
        internal void UpdateValue(object value)
        {
            ConfigStartup.Logger.LogDebug($"Current value updated internally, new value: {value}, old value: {CurrentValue}");
            CurrentValue = value;
            ConfigChanged?.Invoke(CurrentValue);
        }

        /// <summary>
        /// Sets the current and initial value, if player is the host we propagate the change
        /// Current value will not be set if player is in a lobby and player is not the host
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(object value)
        {
            InitialValue = value;
            if (MyceliumNetwork.InLobby && !MyceliumNetwork.IsHost)
            {
                ConfigStartup.Logger.LogDebug($"Current value tried to be set by end user, but is in lobby without being a host, value: {value}");
                return;
            }

            ConfigStartup.Logger.LogDebug($"Current value set by end user, value: {value}");
            CurrentValue = value;
            if (MyceliumNetwork.InLobby && MyceliumNetwork.IsHost)
            {
                Synchronizer.SyncConfig(ConfigGUID, CurrentValue);
            }
        }

        /// <summary>
        /// Called when the config's value is changed internally by UpdateValue
        /// </summary>
        public event Action<object>? ConfigChanged;

        public string ConfigName;
        public string ConfigGUID;
        public object CurrentValue;
        public object InitialValue;
        public Type ConfigType;
    }
}

