using System.Collections.Generic;
using UnityEngine;

namespace BlueprintCruncher
{
    public class BlueprintObject
    {
        public string Prefab;
        public Vector3 Pos;
        public Quaternion Rot;
        public string Data;
        public Vector3 Scale;
        public float Chance;
        public string ExtraInfo;
        public BlueprintObject(string name, Vector3 pos, Quaternion rot, Vector3 scale, string info, string data, float chance)
        {
            Prefab = name;
            Pos = pos;
            Rot = rot.normalized;
            Data = data;
            Scale = scale;
            Chance = chance;
            ExtraInfo = info;
        }
    }
    public class Blueprint
    {
        public Blueprint() { }
        public string Name = "";
        public string Description = "";
        public string Creator = "";
        public Vector3 Coordinates = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
        public List<BlueprintObject> Objects = new ();
        public List<Vector3> SnapPoints = new ();
        public float Radius = 0f;
    }
}