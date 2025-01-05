using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#pragma warning disable IDE0046
namespace BlueprintCruncher
{
    internal class BlueprintImporter
    {
        internal static IEnumerable<string> LoadFiles(string folder, IEnumerable<string> bps)
        {
            if (Directory.Exists(folder))
            {
                var blueprints = Directory.EnumerateFiles(folder, "*.blueprint", SearchOption.AllDirectories);
                // var vbuilds = Directory.EnumerateFiles(folder, "*.vbuild", SearchOption.AllDirectories);
                return bps.Concat(blueprints); //.Concat(vbuilds);
            }
            return bps;
        }
        internal static Blueprint GetBlueprint(string filepath) {
            var rows = File.ReadAllLines(filepath);
            var extension = Path.GetExtension(filepath);
            Blueprint bp = new() { Name = "Importing" };

            if (extension == ".vbuild") return GetBuildShare(bp, rows, true);
            if (extension == ".blueprint") return GetPlanBuild(bp, rows, true);
            throw new InvalidOperationException("Unknown file format.");
        }
        internal static Blueprint GetPlanBuild(Blueprint bp, string[] rows, bool loadData)
        {
            var piece = true;
            foreach (var row in rows)
            {
                if (row.StartsWith("#name:", StringComparison.OrdinalIgnoreCase))
                    bp.Name = row.Split(':')[1];
                else if (row.StartsWith("#description:", StringComparison.OrdinalIgnoreCase))
                    bp.Description = row.Split(':')[1];
                else if (row.StartsWith("#coordinates:", StringComparison.OrdinalIgnoreCase))
                    bp.Coordinates = VectorXZY(row.Split(':')[1]);
                else if (row.StartsWith("#rotation:", StringComparison.OrdinalIgnoreCase))
                    bp.Rotation = VectorXZY(row.Split(':')[1]);
                else if (row.StartsWith("#snappoints", StringComparison.OrdinalIgnoreCase))
                    piece = false;
                else if (row.StartsWith("#pieces", StringComparison.OrdinalIgnoreCase))
                    piece = true;
                else if (row.StartsWith("#", StringComparison.Ordinal))
                    continue;
                else if (piece)
                    bp.Objects.Add(GetPlanBuildObject(row, loadData));
                else
                    bp.SnapPoints.Add(GetPlanBuildSnapPoint(row));
            }
            return bp;
        }

        internal static BlueprintObject GetPlanBuildObject(string row, bool loadData)
        {
            if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
            var split = row.Split(';');
            var name = split[0];
            var posX = InvariantFloat(split, 2);
            var posY = InvariantFloat(split, 3);
            var posZ = InvariantFloat(split, 4);
            var rotX = InvariantFloat(split, 5);
            var rotY = InvariantFloat(split, 6);
            var rotZ = InvariantFloat(split, 7);
            var rotW = InvariantFloat(split, 8);
            var info = split.Length > 9 ? split[9] : "";
            var scaleX = InvariantFloat(split, 10, 1f);
            var scaleY = InvariantFloat(split, 11, 1f);
            var scaleZ = InvariantFloat(split, 12, 1f);
            var data = loadData && split.Length > 13 ? split[13] : "";
            var chance = InvariantFloat(split, 14, 1f);
            return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), new(scaleX, scaleY, scaleZ), info, data, chance);
        }
        internal static Vector3 GetPlanBuildSnapPoint(string row)
        {
            if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
            var split = row.Split(';');
            var x = InvariantFloat(split, 0);
            var y = InvariantFloat(split, 1);
            var z = InvariantFloat(split, 2);
            return new Vector3(x, y, z);
        }
        internal static Blueprint GetBuildShare(Blueprint bp, string[] rows, bool loadData)
        {
            bp.Objects = rows.Select(r => GetBuildShareObject(r, loadData)).ToList();
            return bp;
        }
        private static BlueprintObject GetBuildShareObject(string row, bool loadData)
        {
            if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
            var split = row.Split(' ');
            var name = split[0];
            var rotX = InvariantFloat(split, 1);
            var rotY = InvariantFloat(split, 2);
            var rotZ = InvariantFloat(split, 3);
            var rotW = InvariantFloat(split, 4);
            var posX = InvariantFloat(split, 5);
            var posY = InvariantFloat(split, 6);
            var posZ = InvariantFloat(split, 7);
            var data = loadData && split.Length > 8 ? split[8] : "";
            var chance = InvariantFloat(split, 9, 1f);
            return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), Vector3.one, "", data, chance);
        }
        internal static float InvariantFloat(string[] row, int index, float defaultValue = 0f)
        {
            if (index >= row.Length) return defaultValue;
            var s = row[index];
            if (string.IsNullOrEmpty(s)) return defaultValue;
            return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
        }


        public static Vector3 VectorXZY(string arg)
        {
            return VectorXZY(Split(arg), 0, Vector3.zero);
        }

        public static Vector3 VectorXZY(string[] args)
        {
            return VectorXZY(args, 0, Vector3.zero);
        }

        public static Vector3 VectorXZY(string[] args, Vector3 defaultValue)
        {
            return VectorXZY(args, 0, defaultValue);
        }

        public static Vector3 VectorXZY(string[] args, int index)
        {
            return VectorXZY(args, index, Vector3.zero);
        }

        public static Vector3 VectorXZY(string[] args, int index, Vector3 defaultValue)
        {
            Vector3 zero = Vector3.zero;
            zero.x = Float(args, index, defaultValue.x);
            zero.y = Float(args, index + 2, defaultValue.y);
            zero.z = Float(args, index + 1, defaultValue.z);
            return zero;
        }
        public static string[] Split(string arg, char separator = ',')
        {
            return (from s in arg.Split(new char[1] { separator })
                    select s.Trim() into s
                    where s != ""
                    select s).ToArray();
        }

        public static float Float(string arg, float defaultValue = 0f)
        {
            if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return defaultValue;
            }

            return result;
        }
        public static float Float(string[] args, int index, float defaultValue = 0f)
        {
            if (args.Length <= index)
            {
                return defaultValue;
            }

            return Float(args[index], defaultValue);
        }
    }
}