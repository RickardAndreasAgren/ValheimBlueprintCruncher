using BlueprintCruncher.Configuration;
using UnityEditor;
using UnityEngine;

namespace BlueprintCruncher
{
    internal static class BlueprintCruncher
    {
        internal static GameObject LoadBuildingFromBlueprint(string path, GameObject? selectedLocationAsset, LocationConfiguration config)
        {
            Blueprint blueprint = BlueprintImporter.GetBlueprint(path);

            Console.LogWarning($"{blueprint.Name} loaded.");
            Console.LogWarning($"Using editor selection of {(selectedLocationAsset != null ? selectedLocationAsset.name : string.Empty)}");
            if (blueprint == null || selectedLocationAsset == null) return new GameObject();

            string assetPath = BlueprintParser.LoadPath(selectedLocationAsset.name);

            var selectedLocation = selectedLocationAsset.GetComponent("Location");
            GameObject.DestroyImmediate(selectedLocation);

            Location updatedLocation = selectedLocationAsset.AddComponent<Location>();
            if(updatedLocation != null)
            {
                var updated = config.ApplyConfig(updatedLocation);
                EditorUtility.SetDirty(updatedLocation);
                EditorUtility.SetDirty(selectedLocationAsset);
            }

            GameObject container = BlueprintParser.PopulatePieces(blueprint, selectedLocationAsset, assetPath);

            var levelTerrain = BlueprintParser.UpsertLevelTerrain(container, blueprint.SnapPoints);

            GameObjectUtility.SetParentAndAlign(levelTerrain, selectedLocationAsset);
            GameObjectUtility.SetParentAndAlign(container, selectedLocationAsset);

            PrefabUtility.SaveAsPrefabAsset(selectedLocationAsset, assetPath);
            GameObject.DestroyImmediate(container);

            return selectedLocationAsset;
        }

        internal static void PrepareBuilding(string blueprintPath, LocationConfiguration? activeConfig, bool cleanObject)
        {
            var cutSlash = blueprintPath.Split('/');
            var last = cutSlash[cutSlash.Length - 1];
            cutSlash = last.Split('\\');
            last = cutSlash[cutSlash.Length - 1];
            string blueprint = last.Split('.')[0];
            var root = new GameObject(blueprint);
            Location updatedLocation = root.AddComponent<Location>();
            var locationConfig = activeConfig != null ? activeConfig.patchFrom(root) : new LocationConfiguration(root!);
            
            LoadBuildingFromBlueprint(blueprintPath, root, locationConfig);
            if(cleanObject) GameObject.DestroyImmediate(root);
        }
    }
}
