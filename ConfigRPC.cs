using MyceliumNetworking;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace ConfigSync
{
    public class ConfigRPC: MonoBehaviour
    {
        internal static uint modID = (uint)Hash128.Compute(MyPluginInfo.PLUGIN_GUID).GetHashCode();
        
        void Start()
        {
            MyceliumNetwork.RegisterNetworkObject(this, modID);
            MyceliumNetwork.LobbyLeft += Synchronizer.OnLobbyLeft;
            MyceliumNetwork.PlayerEntered += Synchronizer.OnPlayerEntered;

            DontDestroyOnLoad(this);
        }

        public delegate void SerializerDelegate(TypeSerializer serializer, object value);
        public static Dictionary<Type, SerializerDelegate> SerializerDictionary = new Dictionary<Type, SerializerDelegate>
        {
            { typeof(bool), (TypeSerializer serializer, object value) => serializer.WriteBool((bool)value) },

            { typeof(byte), (TypeSerializer serializer, object value) => serializer.WriteByte((byte)value) },
            //{ typeof(byte[]), (TypeSerializer serializer, object value) => serializer.WriteBytes((byte[])value) },

            { typeof(short), (TypeSerializer serializer, object value) => serializer.WriteShort((short)value) },
            { typeof(int), (TypeSerializer serializer, object value) => serializer.WriteInt((int)value) },
            { typeof(long), (TypeSerializer serializer, object value) => serializer.WriteLong((long)value) },

            { typeof(ushort), (TypeSerializer serializer, object value) => serializer.WriteUshort((ushort)value) },
            { typeof(uint), (TypeSerializer serializer, object value) => serializer.WriteUInt((uint)value) },
            { typeof(ulong), (TypeSerializer serializer, object value) => serializer.WriteUlong((ulong)value) },

            { typeof(float), (TypeSerializer serializer, object value) => serializer.WriteFloat((float)value) },
            { typeof(Vector3), (TypeSerializer serializer, object value) => serializer.WriteVector3((Vector3)value) },
            { typeof(Quaternion), (TypeSerializer serializer, object value) => serializer.WriteQuaternion((Quaternion)value) },

            { typeof(string), (TypeSerializer serializer, object value) => serializer.WriteString((string)value, Encoding.UTF8) },
        };

        public delegate object DeserializerDelegate(TypeDeserializer serializer, object? value);
        public static Dictionary<Type, DeserializerDelegate> DeserializerDictionary = new Dictionary<Type, DeserializerDelegate>
        {
            { typeof(bool), (TypeDeserializer serializer, object? value) => serializer.ReadBool() },

            { typeof(byte), (TypeDeserializer serializer, object? value) => serializer.ReadByte() },
            // Relatively easy to add support, but not right now!
            //{ typeof(byte[]), (TypeDeserializer serializer, object? value) => serializer.ReadBytes((int)value!, Allocator.Temp) }, // Remember to dispose the allocator later on

            { typeof(short), (TypeDeserializer serializer, object? value) => serializer.ReadShort() },
            { typeof(int), (TypeDeserializer serializer, object? value) => serializer.ReadInt() },
            { typeof(long), (TypeDeserializer serializer, object? value) => serializer.ReadLong() },

            { typeof(ushort), (TypeDeserializer serializer, object? value) => serializer.ReadUShort() },
            { typeof(uint), (TypeDeserializer serializer, object? value) => serializer.ReadUInt() },
            { typeof(ulong), (TypeDeserializer serializer, object? value) => serializer.ReadUlong() },

            { typeof(float), (TypeDeserializer serializer, object? value) => serializer.ReadFloat() },
            { typeof(Vector3), (TypeDeserializer serializer, object? value) => serializer.ReadVector3() },
            { typeof(Quaternion), (TypeDeserializer serializer, object? value) => serializer.ReadQuaternion() },

            { typeof(string), (TypeDeserializer serializer, object? value) => serializer.ReadString(Encoding.UTF8) },
        };

        // Relay for ReceiveSync
        public static void RPCTargetRelay(string methodName, CSteamID steamId, string configGUID, Type type, object value)
        {
            bool exists = SerializerDictionary.TryGetValue(type, out SerializerDelegate? serializerDelegate);

            if (exists)
            {
                TypeSerializer serializer = new TypeSerializer();

                serializerDelegate!.Invoke(serializer, value);

                byte[] byteArray = [];
                NativeArray<byte> buffer = serializer.buffer;
                ByteArrayConvertion.MoveToByteArray(ref buffer, ref byteArray);

                MyceliumNetwork.RPCTarget(modID, methodName, steamId, ReliableType.Reliable, configGUID, byteArray);
                serializer.Dispose();
                return;
            }

            Debug.LogError($"[ConfigSync] Type {type} is not currently serializable!");
        }

        public static void RPCTargetRelay(string methodName, CSteamID steamId, string configGUID) // Relay for RequestSync
        {
            MyceliumNetwork.RPCTarget(modID, methodName, steamId, ReliableType.Reliable, configGUID);
        }

        /// <summary>
        /// RPC from host to player synchronizing values
        /// </summary>
        /// <param name="configGUID"></param>
        /// <param name="byteArray"></param>
        [CustomRPC]
        public void ReceiveSync(string configGUID, byte[] byteArray)
        {
            if (MyceliumNetwork.IsHost)
            {
                Debug.LogError($"[ConfigSync] ReceiveSync was called to the host! This is not supposed to happen!");
                return;
            }

            Configuration? configuration = Synchronizer.GetConfigOfGUID(configGUID);
            if (configuration == null)
            {
                Debug.LogError($"[ConfigSync] Config of GUID '{configGUID}' does not exist!");
                return;
            }

            Type type = configuration.ConfigType;

            bool exists = DeserializerDictionary.TryGetValue(type, out DeserializerDelegate? deserializerDelegate);

            if (exists)
            {
                TypeDeserializer deserializer = new TypeDeserializer(byteArray, Allocator.Temp);

                object value = deserializerDelegate!.Invoke(deserializer, null);
                configuration.UpdateValue(value);

                deserializer.Dispose();
                return;
            }

            Debug.LogError($"[ConfigSync] Type '{type}' is not currently deserializable!");
        }

        /// <summary>
        /// RPC from player to host requesting sync
        /// </summary>
        /// <param name="configGUID"></param>
        /// <param name="byteArray"></param>
        [CustomRPC]
        public void RequestSync(string configGUID, RPCInfo info) // The player who receives is the host, we send the guid + value back to the sender
        {
            Configuration? configuration = Synchronizer.GetConfigOfGUID(configGUID);
            if (configuration == null)
            {
                Debug.LogError($"[ConfigSync] Config of GUID '{configGUID}' does not exist!");
                return;
            }

            RPCTargetRelay(nameof(ReceiveSync), info.SenderSteamID, configGUID, configuration.ConfigType, configuration.CurrentValue);
        }
    }
}
