using System;
using UnityEngine;

namespace BlueprintCruncher.Configuration
{
    internal class TransformConfiguration
    {
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = new Quaternion();
        public Vector3 Scale = Vector3.one;
        public TransformConfiguration() { }
        public TransformConfiguration(Vector3 position, Quaternion rotation, Vector3? scale = null)
        {
            this.Position = Vector3Copy(position);
            this.Rotation = QuaternionCopy(rotation);
            if (scale == null) this.Scale = new Vector3(1,1,1);
            else this.Scale = Vector3Copy(scale);
        }
        public static Vector3 Vector3Copy(Vector3? v)
        {
            if (v == null) return Vector3.zero;
            return Vector3Copy((Vector3)v);
        }
        public static Vector3 Vector3Copy(Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Quaternion QuaternionCopy(Quaternion rotation)
        {
            return new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        }
    }
}
