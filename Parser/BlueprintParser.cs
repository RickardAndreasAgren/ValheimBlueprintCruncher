using BlueprintCruncher.Configuration;
using BlueprintCruncher.Constants;
using BlueprintCruncher.Extensions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace BlueprintCruncher
{
    internal static class BlueprintParser
    {
        static List<string> debugs = new();
        public static Firemode firemode { get; set; } = Firemode.Default;
        static Dictionary<string, GameObject> cachePieces = new();
        static Dictionary<string, string> prefabCorrector = new() {
            { "stone_arch", "stone_arch_0"},
        };
        static Dictionary<string, string> treasureDefaultsWood = new()
        {
            { Heightmap.Biome.Meadows.ToString(), "TreasureChest_meadows"},
            { Heightmap.Biome.Plains.ToString(), "TreasureChest_heath"},
            { Heightmap.Biome.BlackForest.ToString(), "TreasureChest_blackforest"},
            { Heightmap.Biome.Mountain.ToString(), "TreasureChest_mountains"},
            { Heightmap.Biome.Swamp.ToString(), "TreasureChest_swamp"}
        };
        static Dictionary<string, string> treasureDefaultsStone = new()
        {
            { Heightmap.Biome.Plains.ToString(), "TreasureChest_plains_stone"},
            { Heightmap.Biome.BlackForest.ToString(), "TreasureChest_trollcave"},
            { Heightmap.Biome.Mountain.ToString(), "TreasureChest_mountaincave"},
            { Heightmap.Biome.Swamp.ToString(), "TreasureChest_sunkencrypt"},
            { Heightmap.Biome.Mistlands.ToString(), "TreasureChest_dvergr_loose_stone"},
            { Heightmap.Biome.AshLands.ToString(), "TreasureChest_ashland_stone"}
        };
        public static GameObject CreateBuildingContainer(string name)
        {
            var newContainer = new GameObject(name);
            EnsureTransform(newContainer);

            return newContainer;
        }
        private static TransformConfiguration _defaultConfig = new ();
        private static void EnsureTransform(GameObject g, TransformConfiguration? values = null)
        {
            if (values == null) values = _defaultConfig;

            g.transform.position = new Vector3(values.Position.x, values.Position.y, values.Position.z);
            g.transform.rotation = new Quaternion(values.Rotation.x, values.Rotation.y, values.Rotation.z, values.Rotation.w);
            g.transform.localScale = new Vector3(values.Scale.x, values.Scale.y, values.Scale.z);
        }
        private static void EnsureLocalTransform(GameObject g, TransformConfiguration? values = null)
        {
            if (values == null) values = _defaultConfig;

            g.transform.localPosition = new Vector3(values.Position.x, values.Position.y, values.Position.z);
            g.transform.localRotation = new Quaternion(values.Rotation.x, values.Rotation.y, values.Rotation.z, values.Rotation.w);
            g.transform.localScale = new Vector3(values.Scale.x, values.Scale.y, values.Scale.z);
        }

        internal static GameObject PopulatePieces(Blueprint blueprint, GameObject selectedLocationObject, string assetPath)
        {
            if (blueprint == null) throw new Exception("No blueprint");
            if (selectedLocationObject == null) throw new Exception("No root");
            GameObject container = BlueprintParser.CreateBuildingContainer("Building");
            foreach (BlueprintObject bpPart in blueprint.Objects)
            {
                GameObject rootObject = LoadPrefab(bpPart, selectedLocationObject, container);
                if (rootObject == null) continue;

                var transforms = new TransformConfiguration(bpPart.Pos, bpPart.Rot, bpPart.Scale);

                GameObjectUtility.SetParentAndAlign(rootObject, container);
                GameObjectUtility.EnsureUniqueNameForSibling(rootObject);
                EnsureLocalTransform(rootObject, transforms);
            }
            PrefabUtility.SaveAsPrefabAsset(selectedLocationObject, assetPath);
            ReleasePrefabs();
            return container;
        }

        private static void ReleasePrefabs()
        {
            foreach(var keyString in cachePieces.Keys)
            {
                if(PrefabUtility.IsPartOfAnyPrefab(cachePieces[keyString])) PrefabUtility.UnloadPrefabContents(cachePieces[keyString]);
            }
            cachePieces = new Dictionary<string, GameObject>();
        }

        private static GameObject LoadPrefab(BlueprintObject bpPart, GameObject? rootLocationObject, GameObject buildingContainer)
        {
            GameObject? newPiece;
            try
            {
                bpPart.Prefab = ValidateBlueprintPiece(bpPart);
                if (!cachePieces.ContainsKey(bpPart.Prefab))
                { 
                    newPiece = PrefabUtility.LoadPrefabContents(LoadPath(bpPart.Prefab));
                    newPiece = SetFireplaceMode(newPiece)!;
                    if (rootLocationObject != null && newPiece.name.Contains("chest")) newPiece = ConvertChest(newPiece, rootLocationObject, buildingContainer);
                    cachePieces.Add(bpPart.Prefab, newPiece);
                }
                newPiece = GameObject.Instantiate(cachePieces[bpPart.Prefab]);
                newPiece.name = newPiece.name.ToLower().Replace("(clone)", "");
                EditorUtility.SetDirty(newPiece);

                return newPiece;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }

            return new GameObject("Wut");
        }

        private static GameObject LoadPrefab(string prefabName)
        {
            GameObject? newPiece;
            if (!cachePieces.ContainsKey(prefabName))
            {
                newPiece = PrefabUtility.LoadPrefabContents(LoadPath(prefabName));
                newPiece.name = newPiece.name.ToLower().Replace("(clone)", "");
                cachePieces.Add(prefabName, newPiece);
            }
            newPiece = GameObject.Instantiate(cachePieces[prefabName]);
            newPiece.name = newPiece.name.ToLower().Replace("(clone)", "");

            return newPiece;
        }

        private static GameObject ConvertChest(GameObject newPiece, GameObject locationRoot, GameObject buildingContainer)
        {
            Location selectedLocation = (Location)locationRoot.GetComponent("Location");
            string shrunkName = newPiece.name.ToLower();
            GameObject? updatedPiece = null;
            TransformConfiguration placement = new();
            string biomeKey = selectedLocation.m_biome.ToString();
            placement = new TransformConfiguration(newPiece.transform.position, newPiece.transform.rotation);

            if (shrunkName.Contains("wood") && treasureDefaultsWood.ContainsKey(selectedLocation.m_biome.ToString()))
            {
                updatedPiece = LoadPrefab(treasureDefaultsWood[biomeKey]);
            } else if (shrunkName.Contains("stone") && treasureDefaultsStone.ContainsKey(selectedLocation.m_biome.ToString()))
            {
                updatedPiece = LoadPrefab(treasureDefaultsStone[biomeKey]);
            }

            if (updatedPiece != null)
            {
                return updatedPiece;
            }
            return new GameObject("Fail");
        }

        public static GameObject SetFireplaceMode(GameObject newPiece)
        {
            Component fireComponent;
            if (newPiece.TryGetComponent(typeof(Fireplace), out fireComponent))
            {
                Fireplace asF = (Fireplace)newPiece.GetComponent(typeof(Fireplace));

                Fireplace updatedFireplace = (Fireplace)newPiece.AddComponent(typeof(Fireplace));
                updatedFireplace.GetCopyOf<Fireplace>(asF);
                if(firemode == Firemode.Infinite)
                {
                    updatedFireplace.m_canRefill = false;
                    updatedFireplace.m_infiniteFuel = true;
                    updatedFireplace.m_startFuel = float.Parse("0");
                } else if(firemode == Firemode.Dead)
                {
                    updatedFireplace.m_startFuel = float.Parse("0");
                    updatedFireplace.m_canRefill = false;
                } else if(firemode == Firemode.Default)
                {
                    updatedFireplace.m_canRefill = true;
                    updatedFireplace.m_infiniteFuel = false;
                    updatedFireplace.m_startFuel = float.Parse("2");
                }

                GameObject.DestroyImmediate(asF);
                EditorUtility.SetDirty(newPiece);
            }
            return newPiece;
        }

        private static string ValidateBlueprintPiece(BlueprintObject bpPart)
        {
            if(prefabCorrector.ContainsKey(bpPart.Prefab))
            {
                return prefabCorrector[bpPart.Prefab];
            }
            return bpPart.Prefab;
        }

        public static string LoadPath(string name)
        {
            return $"Assets/PrefabInstance/{name}.prefab";
        }

        internal static GameObject UpsertLevelTerrain(GameObject container, List<Vector3>? snapList)
        {
            GameObject levelTerrain = new GameObject("LevelTerrain");
            EnsureLocalTransform(levelTerrain);
            TerrainModifier? modifier = null;
            if (snapList != null)
            {
                modifier = levelTerrain.AddComponent<TerrainModifier>();
                var snappoints = Snapping.GetSnapPoints(container);
                float flatRadius = CalculateRadius(snapList);
                modifier.m_smoothRadius = flatRadius;
                modifier.m_paintRadius = flatRadius * (float)0.75;
                modifier.m_paintType = TerrainModifier.PaintType.Cultivate;
            } else
            {
                Component outTerrain;
                if (container.TryGetComponent(typeof(TerrainModifier), out outTerrain))
                {
                    modifier = (TerrainModifier)outTerrain;

                    TerrainModifier updatedTerrain = (TerrainModifier)container.AddComponent(typeof(TerrainModifier));
                    updatedTerrain.GetCopyOf<TerrainModifier>(modifier);

                    GameObject.DestroyImmediate(modifier);
                    EditorUtility.SetDirty(container);
                    modifier = updatedTerrain;
                }
            }
            modifier!.m_spawnAtMaxLevelDepth = true;
            modifier!.m_smooth = true;
            modifier!.m_level = false;

            return levelTerrain;
        }

        private static float CalculateRadius(List<Vector3> snapList)
        {
            float xMax = 0;
            float xMin = 0;
            float zMax = 0;
            float zMin = 0;
            float xLength = 0;
            float zLength = 0;

            foreach (Vector3 snapPoint in snapList)
            {
                xMax = xMax < snapPoint.x ? snapPoint.x : xMax;
                xMin = xMin > snapPoint.x ? snapPoint.x : xMin;
                zMax = zMax < snapPoint.z ? snapPoint.z : zMax;
                zMin = zMin > snapPoint.x ? snapPoint.x : zMin;
            }

            xLength = xMax - xMin;
            zLength = zMax - zMin;
            float useLength = xLength > zLength ? xLength : zLength;

            return useLength / (float)Math.Sqrt(Math.PI);
        }
    }
}
