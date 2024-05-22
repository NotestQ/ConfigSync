using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using UnityEngine;
using Zorro.Core.Serizalization;

namespace ConfigSync
{
    public class TypeSerializer : BinarySerializer
    {
        public void WriteShort(short value)
        {
            NativeArray<short> nativeArray = new NativeArray<short>(1, Allocator.Temp, NativeArrayOptions.ClearMemory);
            nativeArray[0] = value;
            this.WriteBytes(nativeArray.Reinterpret<byte>(UnsafeUtility.SizeOf<short>()));
            nativeArray.Dispose();
        }

        public void WriteVector3(Vector3 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
        }

        public void WriteQuaternion(Quaternion value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }
    }
}
