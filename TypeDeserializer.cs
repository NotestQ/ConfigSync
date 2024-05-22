using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using UnityEngine;
using Zorro.Core.Serizalization;

namespace ConfigSync
{
    public class TypeDeserializer(byte[] buffer, Allocator allocator) : BinaryDeserializer(buffer, allocator)
    {
        public short ReadShort()
        {
            NativeSlice<byte> nativeSlice = new NativeSlice<byte>(buffer, position, 2);
            short result = nativeSlice.SliceConvert<short>()[0];
            position += 2;
            return result;
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }
    }
}
