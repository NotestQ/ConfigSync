using BepInEx.Logging;
using MyceliumNetworking;
using System;

namespace ConfigSync
{
    public class Configuration
    {
        public Configuration(string name, string GUID, object initialValue) {
            ConfigName = name;
            ConfigGUID = GUID;
            CurrentValue = initialValue;
            ConfigType = initialValue.GetType();

            Synchronizer.configList.Add(this);
            Synchronizer.AddOrDeferConfig(ConfigGUID, initialValue);
        }

        /// <summary>
        /// Updates the value and invokes the ConfigChanged event making it possible to handle it in your own way
        /// </summary>
        /// <param name="value"></param>
        internal void UpdateValue(object value)
        {
            ConfigStartup.Logger.LogWarning($"Value updated internally, new value: {value}, old value: {CurrentValue}");
            CurrentValue = value;
            ConfigChanged?.Invoke(CurrentValue);
        }

        /// <summary>
        /// Sets the current value and if we are the host we propagate the config
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(object value)
        {
            CurrentValue = value;
            if (MyceliumNetwork.InLobby && MyceliumNetwork.IsHost)
            {
                Synchronizer.CreateOrSyncConfig(ConfigGUID, CurrentValue);
            }
        }

        /// <summary>
        /// Called when the config's value is changed by Mycelium
        /// </summary>
        public event Action<object>? ConfigChanged;

        public string ConfigName;
        public string ConfigGUID;
        public object CurrentValue;
        public Type ConfigType;
    }
}
