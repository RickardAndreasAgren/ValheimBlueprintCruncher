using System.IO;
using UnityEngine;
using UnityEditor;
using BlueprintCruncher.Configuration;
using static Heightmap;
using static ZoneSystem;
using System.Linq;
using System.Collections.Generic;
using System;
using BlueprintCruncher.Constants;

namespace BlueprintCruncher
{
    public class BlueprintCruncherGUIEditor : EditorWindow
    {
        string importPath = "";
        bool folderMode = false;
        bool configMode = false;
        bool attemptImport = false;
        bool getFile = false;
        bool clearObject = false;
        Firemode firemode = Firemode.Default;
        LocationConfiguration config = new();
        GameObject? selectedLocationPrefab;

        public bool[] boolOptions = new bool[] { true, false };

        [MenuItem("Window/BlueprintCruncher")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(BlueprintCruncherGUIEditor));
        }
        private void OnGUI()
        {
            GUILayout.Label("Blueprint Importer", EditorStyles.boldLabel);

            selectedLocationPrefab = (GameObject)Selection.activeObject;

            folderMode = GUILayout.Toggle(folderMode, "Import folder mode");
            configMode = GUILayout.Toggle(configMode, "Configure location and buildings");
            if (folderMode) configMode = false;
            if (configMode)
            {
                folderMode = false;
                importPath = string.Empty;
            }

            if(!configMode)
            {
                importPath = EditorGUILayout.TextField("Path", importPath);
                var buttonText = folderMode ? new GUIContent("Select blueprint folder") : new GUIContent("Select blueprint");
                getFile = GUILayout.Button(buttonText);
            }

            if (getFile)
            {
                getFile = false;
                if (folderMode) importPath = EditorUtility.OpenFolderPanel("Get a folder with .blueprint files", importPath, "");
                else importPath = EditorUtility.OpenFilePanel("Get a .blueprint", "", "blueprint,vbuild");
            }


            DrawConfig();

            if(configMode)
            {
                attemptImport = GUILayout.Button(new GUIContent("Apply configuration"));
                if (attemptImport)
                {
                    attemptImport = false;
                    ConfigurePrefab(selectedLocationPrefab, config);
                }
            } else
            {
                attemptImport = GUILayout.Button(new GUIContent("Import"));
                if (attemptImport && importPath.Length != 0)
                {
                    attemptImport = false;
                    if (!folderMode && File.Exists(importPath))
                    {
                        Import(selectedLocationPrefab, config);
                    }
                    else ImportFolder(config);
                }
            }
            
        }

        private void DrawConfig()
        {
            if (folderMode)
            {
                bool clearObject = false;
                Rect rco = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
                GUILayout.Label("Clean up object after save");
                PopupBool(boolOptions, config.ClearArea, out clearObject);
                this.clearObject = clearObject;
                EditorGUILayout.EndHorizontal();
            }

            Firemode chosenFiremode = firemode;
            Rect rf = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Fire mode");
            PopupFiremode(Enum.GetNames(typeof(Firemode)).Select(b => b.ToString()).ToList<string>(), (Firemode)firemode, out chosenFiremode);
            this.firemode = chosenFiremode;
            BlueprintParser.firemode = firemode;
            EditorGUILayout.EndHorizontal();

            Biome chosenBiome = Biome.None;
            Rect r = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Biome");
            PopupBiome(Enum.GetNames(typeof(Biome)).Select(b => b.ToString()).ToList<string>(), (Heightmap.Biome)config.Biome, out chosenBiome);
            config.Biome = (int)chosenBiome;
            EditorGUILayout.EndHorizontal();

            bool chosenClearArea = false;
            Rect r2 = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Clear Area");
            PopupBool(boolOptions, config.ClearArea, out chosenClearArea);
            config.ClearArea = chosenClearArea;
            EditorGUILayout.EndHorizontal();

            Rect r3 = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Exterior Radius");
            config.ExteriorRadius = EditorGUILayout.FloatField(config.ExteriorRadius);
            EditorGUILayout.EndHorizontal();

            Rect r4 = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Discovery Label");
            config.FoundLabel = EditorGUILayout.TextField("", config.FoundLabel);
            EditorGUILayout.EndHorizontal();

            Rect r5 = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Level increase override");
            config.LevelIncreaseOverride = EditorGUILayout.FloatField(config.LevelIncreaseOverride);
            EditorGUILayout.EndHorizontal();

            Rect r6 = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Level increase override minimum");
            config.MinLevelOverride = EditorGUILayout.IntField(config.MinLevelOverride);
            EditorGUILayout.EndHorizontal();

            Rect r7 = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Level increase override maximum");
            config.MaxLevelOverride = EditorGUILayout.IntField(config.MaxLevelOverride);
            EditorGUILayout.EndHorizontal();

            bool chosenNobuildOverride = false;
            Rect r8 = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("No Build Location");
            PopupBool(boolOptions, config.NoBuildLocation, out chosenNobuildOverride);
            config.NoBuildLocation = chosenNobuildOverride;
            EditorGUILayout.EndHorizontal();

            Rect r9 = (Rect)EditorGUILayout.BeginHorizontal(GUIStyle.none);
            config.NoBuildRadiusOverride = EditorGUILayout.FloatField("Nobuild radius override", config.NoBuildRadiusOverride );
            EditorGUILayout.EndHorizontal();
        }
        private void PopupBool(bool[] options, bool value, out bool result)
        {
            int currentIndex = options.ToList().IndexOf(value);
            string[] stringOptions = options.Select(option => option ? "True" : "False").ToArray();
            int chosenIndex = EditorGUILayout.Popup(currentIndex, stringOptions);
            currentIndex = chosenIndex;
            result = options[chosenIndex];
        }
        private void PopupBiome(List<string> options, Biome value, out Biome result)
        {
            int currentIndex = options.IndexOf(value.ToString());
            string[] stringOptions = options.Select(option => option.ToString()).ToArray();
            int chosenIndex = EditorGUILayout.Popup(currentIndex, stringOptions);
            currentIndex = chosenIndex;
            result = (Biome)Enum.Parse(typeof(Biome), options[chosenIndex]);
        }
        private void PopupFiremode(List<string> options, Firemode value, out Firemode result)
        {
            int currentIndex = options.IndexOf(value.ToString());
            string[] stringOptions = options.Select(option => option.ToString()).ToArray();
            int chosenIndex = EditorGUILayout.Popup(currentIndex, stringOptions);
            currentIndex = chosenIndex;
            result = (Firemode)Enum.Parse(typeof(Firemode), options[chosenIndex]);
        }
        private void Import(GameObject? prefabRoot, LocationConfiguration? activeConfig = null)
        {
            Location? locationComponent = null;
            if (prefabRoot == null || !prefabRoot.TryGetComponent<Location>(out locationComponent))
            {
                EditorUtility.DisplayDialog("Select a location prefab", "Importing requires a selected location prefab", "OK");
            }
            var locationConfig = activeConfig != null ? activeConfig.patchFrom(prefabRoot) : new LocationConfiguration(prefabRoot!);
            BlueprintCruncher.LoadBuildingFromBlueprint(importPath, prefabRoot, locationConfig);
        }
        private void ImportFolder(LocationConfiguration? activeConfig = null)
        {
            List<string> blueprints = BlueprintImporter.LoadFiles(importPath, new List<string>()).ToList();

            foreach(var blueprintPath in blueprints)
            {
                BlueprintCruncher.PrepareBuilding(blueprintPath, activeConfig, clearObject);
            }
        }
        private void ConfigurePrefab(GameObject prefabRoot, LocationConfiguration? activeConfig = null)
        {
            Location? locationComponent = null;
            if (prefabRoot == null || !prefabRoot.TryGetComponent<Location>(out locationComponent))
            {
                EditorUtility.DisplayDialog("Select a location prefab", "Importing requires a selected location prefab", "OK");
            }
            string assetPath = AssetDatabase.GetAssetPath(prefabRoot);
            var locationConfig = activeConfig != null ? activeConfig.patchFrom(prefabRoot) : new LocationConfiguration(prefabRoot!);
            ConfigCruncher.ApplyConfiguration(prefabRoot, locationConfig);
        }
    }
}
