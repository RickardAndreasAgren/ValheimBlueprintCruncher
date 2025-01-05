using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlueprintCruncher
{
    public static class SnappingMode
    {
        public const string All = "All";
        public const string Corners = "Corners";
        public const string Edges = "Edges";
        public const string Off = "Off";
    }
    public static class Snapping
    {
        public static bool IsSnapPoint(GameObject obj) => obj && obj.CompareTag("snappoint");
        public static bool IsSnapPoint(Transform tr) => IsSnapPoint(tr.gameObject);
        public static string ConfigurationSnapping = SnappingMode.Corners;
        public static List<GameObject> GetChildren(GameObject obj)
        {
            List<GameObject> children = new();
            foreach (Transform tr in obj.transform)
            {
                if (IsSnapPoint(tr)) continue;
                children.Add(tr.gameObject);
            }
            return children;
        }
        public static List<GameObject> GetSnapPoints(GameObject obj)
        {
            List<GameObject> snapPoints = new();
            foreach (Transform tr in obj.transform)
            {
                if (IsSnapPoint(tr)) snapPoints.Add(tr.gameObject);
            }
            return snapPoints;
        }

        public static void CreateSnapPoint(GameObject parent, Vector3 pos, string name)
        {
            GameObject snapObj = new()
            {
                name = name,
                layer = LayerMask.NameToLayer("piece"),
                tag = "snappoint",
            };
            snapObj.SetActive(false);
            snapObj.transform.parent = parent.transform;
            snapObj.transform.localPosition = pos;
            snapObj.transform.localRotation = Quaternion.identity;
        }
        public static void CreateSnapPoints(GameObject obj, List<Vector3> points)
        {
            int counter = 0;
            foreach (var point in points)
                CreateSnapPoint(obj, point, $"Snap {++counter}");
        }

        public static void RegenerateSnapPoints(GameObject obj)
        {
            RemoveSnapPoints(obj);
            GenerateSnapPoints(obj);
        }
        public static void RemoveSnapPoints(GameObject obj)
        {
            foreach (Transform tr in obj.transform)
            {
                if (IsSnapPoint(tr)) Object.Destroy(tr.gameObject);
            }
        }
        public static void GenerateSnapPoints(GameObject obj) => CreateSnapPoints(obj, GenerateSnapPoints(GetChildren(obj)));
        public static List<Vector3> GenerateSnapPoints(List<GameObject> objects)
        {
            if (objects.Count == 0) return new();
            if (ConfigurationSnapping == SnappingMode.Off)
            {
                List<Vector3> snapPoints = new();
                GetSnapPoints(objects[0], snapPoints);
                return snapPoints;
            }
            var points = FindOuterPoints(SearchSnapPoints(objects));
            if (points.Count == 0) return points;
            if (ConfigurationSnapping == SnappingMode.Corners) return CornerSnap(points);
            return points;
        }

        private static List<Vector3> SearchSnapPoints(List<GameObject> objects)
        {
            List<Vector3> snapPoints = new();
            foreach (var child in objects)
            {
                if (IsSnapPoint(child)) continue;
                GetSnapPoints(child, snapPoints);
            }
            return snapPoints;
        }
        private static void GetSnapPoints(GameObject obj, List<Vector3> snapPoints)
        {
            for (int c = 0; c < obj.transform.childCount; c++)
            {
                var snap = obj.transform.GetChild(c);
                if (IsSnapPoint(snap))
                {
                    var pos = obj.transform.localPosition + obj.transform.localRotation * snap.localPosition;
                    snapPoints.Add(pos);
                }
            }
        }

        private static List<Vector3> FindOuterPoints(List<Vector3> snapPoints)
        {
            if (snapPoints.Count == 0) return new();
            float left = float.MaxValue;
            float right = float.MinValue;
            float front = float.MaxValue;
            float back = float.MinValue;
            float top = float.MinValue;
            float bottom = float.MaxValue;
            List<Vector3> lefts = new();
            List<Vector3> rights = new();
            List<Vector3> fronts = new();
            List<Vector3> backs = new();
            List<Vector3> tops = new();
            List<Vector3> bottoms = new();
            foreach (var pos in snapPoints)
            {
                if (Approx(pos.x, left))
                {
                    lefts.Add(pos);
                }
                else if (pos.x < left)
                {
                    left = pos.x;
                    lefts = [pos];
                }

                if (Approx(pos.x, right))
                {
                    rights.Add(pos);
                }
                else if (pos.x > right)
                {
                    right = pos.x;
                    rights = [pos];
                }

                if (Approx(pos.z, front))
                {
                    fronts.Add(pos);
                }
                else if (pos.z < front)
                {
                    front = pos.z;
                    fronts = [pos];
                }

                if (Approx(pos.z, back))
                {
                    backs.Add(pos);
                }
                else if (pos.z > back)
                {
                    back = pos.z;
                    backs = [pos];
                }

                if (Approx(pos.y, top))
                {
                    tops.Add(pos);
                }
                else if (pos.y > top)
                {
                    top = pos.y;
                    tops = [pos];
                }

                if (Approx(pos.y, bottom))
                {
                    bottoms.Add(pos);
                }
                else if (pos.y < bottom)
                {
                    bottom = pos.y;
                    bottoms = [pos];
                }
            }
            return UniquePoints([.. lefts, .. rights, .. fronts, .. backs, .. tops, .. bottoms]);
        }

        private static List<Vector3> CornerSnap(List<Vector3> snapPoints)
        {
            var center = FindCenter(snapPoints);
            var points = snapPoints.Select(p => System.Tuple.Create(p - center, p)).ToList();
            var corner1 = points.OrderBy(p => p.Item1.x + p.Item1.y + p.Item1.z).First().Item2;
            var corner2 = points.OrderBy(p => p.Item1.x + p.Item1.y - p.Item1.z).First().Item2;
            var corner3 = points.OrderBy(p => p.Item1.x - p.Item1.y + p.Item1.z).First().Item2;
            var corner4 = points.OrderBy(p => p.Item1.x - p.Item1.y - p.Item1.z).First().Item2;
            var corner5 = points.OrderBy(p => -p.Item1.x + p.Item1.y + p.Item1.z).First().Item2;
            var corner6 = points.OrderBy(p => -p.Item1.x + p.Item1.y - p.Item1.z).First().Item2;
            var corner7 = points.OrderBy(p => -p.Item1.x - p.Item1.y + p.Item1.z).First().Item2;
            var corner8 = points.OrderBy(p => -p.Item1.x - p.Item1.y - p.Item1.z).First().Item2;
            return UniquePoints([corner1, corner2, corner3, corner4, corner5, corner6, corner7, corner8]);
        }

        private static List<Vector3> UniquePoints(List<Vector3> points)
        {
            // Very inefficient but not many snap points.
            List<Vector3> unique = new();
            foreach (var pos in points)
            {
                if (unique.Any(p => Approx(p.x, pos.x) && Approx(p.y, pos.y) && Approx(p.z, pos.z))) continue;
                unique.Add(pos);
            }
            return unique;
        }

        private static Vector3 FindCenter(List<Vector3> snapPoints)
        {
            Vector3 center = Vector3.zero;
            foreach (var pos in snapPoints)
                center += pos;
            center /= snapPoints.Count;
            return snapPoints.OrderBy(p => (p - center).sqrMagnitude).First();
        }
        private static bool Approx(float a, float b)
        {
            return Mathf.Abs(a - b) < 0.001f;
        }
    }
}
