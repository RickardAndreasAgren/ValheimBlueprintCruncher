using BlueprintCruncher.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace BlueprintCruncher
{
    internal static class ConfigCruncher
    {
        internal static GameObject ApplyConfiguration(GameObject? selectedLocationAsset, LocationConfiguration config)
        {
            if (selectedLocationAsset == null) return new GameObject();

            string assetPath = BlueprintParser.LoadPath(selectedLocationAsset.name);

            GameObject editAsset = PrefabUtility.LoadPrefabContents(assetPath);

            var selectedLocation = editAsset.GetComponent("Location");
            GameObject.DestroyImmediate(selectedLocation);

            Location updatedLocation = editAsset.AddComponent<Location>();
            if(updatedLocation != null)
            {
                var updated = config.ApplyConfig(updatedLocation);
                EditorUtility.SetDirty(updatedLocation);
                EditorUtility.SetDirty(editAsset);
            }

            GameObject buildingContainer = new GameObject("dummy");
            foreach(Transform childTransform in editAsset.transform)
            {
                if(childTransform.name.ToLower() == "building")
                {
                    GameObject.DestroyImmediate(buildingContainer);
                    buildingContainer = GameObject.Instantiate(childTransform.gameObject);
                    buildingContainer.name.ToLower().Replace("(clone)", "");
                }
            }
            if (buildingContainer != null)
            {
                UpdateFireplaces(buildingContainer);
                GameObjectUtility.SetParentAndAlign(buildingContainer, editAsset);
            }
            var levelTerrain = BlueprintParser.UpsertLevelTerrain(editAsset, null);

            GameObjectUtility.SetParentAndAlign(levelTerrain, editAsset);
            GameObject.DestroyImmediate(selectedLocationAsset);
            PrefabUtility.SaveAsPrefabAsset(editAsset, assetPath);

            return editAsset;
        }

        private static void UpdateFireplaces(GameObject selectedLocationAsset)
        {
            List<Fireplace> fireplaces = selectedLocationAsset.GetComponentsInChildren<Fireplace>().ToList();

            foreach (Fireplace fireplace in fireplaces)
            {
                BlueprintParser.SetFireplaceMode(fireplace.gameObject);
            }
        }
    }
}
