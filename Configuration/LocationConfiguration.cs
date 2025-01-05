using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintCruncher.Configuration
{
    /// <summary>
    ///     Configuration class for location components
    ///     Jotunn-based
    /// </summary>
    public class LocationConfiguration
    {
        /// <summary>
        ///     Biome to spawn in, multiple Biomes can be allowed with />
        /// </summary>
        public int Biome { get; set; }

        /// <summary>
        ///     Radius of the location. Terrain delta is calculated within this circle.
        /// </summary>
        public float ExteriorRadius { get; set; } = 10f;
        /// <summary>
        ///     Enable to activate interior handling
        /// </summary>
        public bool HasInterior { get; set; } = false;

        /// <summary>
        ///     Radius of the interior attached to the location
        /// </summary>
        public float InteriorRadius { get; set; } = 20;

        /// <summary>
        ///     Environment string used by the interior
        /// </summary>
        public string InteriorEnvironment { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public GameObject? InteriorPrefab { get; set; } = null;
        /// <summary>
        /// Use custom overriding transform for interior
        /// </summary>
        public bool InteriorTransformCustom { get; set; } = false;
        /// <summary>
        /// Overriding transform for interior
        /// </summary>
        public Transform? InteriorTransform { get; set; } = null;

        /// <summary>
        ///     Enable to forbid Vegetation from spawning inside the circle defined by ExteriorRadius
        /// </summary>
        public bool ClearArea { get; set; } = true;
        /// <summary>
        ///     UI pop-up label on discovery
        /// </summary>
        public string FoundLabel { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public List<int> BlockSpawnGroups { get; set; } = new();
        /// <summary>
        /// 
        /// </summary>
        public float LevelIncreaseOverride { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int MinLevelOverride { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int MaxLevelOverride { get; set; }
        /// <summary>
        /// DungeonGenerator reference
        /// </summary>
        public DungeonGenerator? DungeonGeneratorObject { get; set; } = null;
        /// <summary>
        /// Disallow player building inside NoBuildRadius
        /// </summary>
        public bool NoBuildLocation { get; set; } = false;
        /// <summary>
        /// Enforced no build radius
        /// </summary>
        public float NoBuildRadiusOverride { get; set; }
        /// <summary>
        ///     Create a new <see cref="LocationConfig"/>
        /// </summary>
        public LocationConfiguration() { }

        /// <summary>
        ///     Create a configured location component from existing prefabs locationcomponent
        /// </summary>
        /// <param name="zoneLocation">ZoneLocation to copy</param>
        public LocationConfiguration(GameObject locationPrefab)
        {
            var locationComponent = locationPrefab.GetComponent<Location>();
            if (locationComponent != null)
            {
                Biome = (int)locationComponent.m_biome;
                ClearArea = locationComponent.m_clearArea;
                ExteriorRadius = locationComponent.m_exteriorRadius;
                FoundLabel = locationComponent.m_discoverLabel;
                BlockSpawnGroups = locationComponent.m_blockSpawnGroups;
                LevelIncreaseOverride = locationComponent.m_enemyLevelUpOverride;
                MinLevelOverride = locationComponent.m_enemyMinLevelOverride;
                MaxLevelOverride = locationComponent.m_enemyMaxLevelOverride;
                HasInterior = locationComponent.m_hasInterior;
                InteriorRadius = locationComponent.m_interiorRadius;
                InteriorEnvironment = locationComponent.m_interiorEnvironment;
                InteriorPrefab = locationComponent.m_interiorPrefab;
                InteriorTransformCustom = locationComponent.m_useCustomInteriorTransform;
                InteriorTransform = locationComponent.m_interiorTransform;
                DungeonGeneratorObject = locationComponent.m_generator;
                NoBuildLocation = locationComponent.m_noBuild;
                NoBuildRadiusOverride = locationComponent.m_noBuildRadiusOverride;
            } else
            {
                FoundLabel = "";
                BlockSpawnGroups = new();
            }
        }

        /// <summary>
        ///     Converts the configuration to a valid Location component for argument object
        /// </summary>
        /// <returns></returns>
        public Location ApplyConfig(Location locationComponent, string name = "")
        {
            if (name != string.Empty) locationComponent.name = name;

            locationComponent.m_biome = (Heightmap.Biome)Biome;
            locationComponent.m_clearArea = ClearArea;
            locationComponent.m_exteriorRadius = ExteriorRadius;
            locationComponent.m_discoverLabel = FoundLabel;
            locationComponent.m_blockSpawnGroups = BlockSpawnGroups;
            locationComponent.m_enemyLevelUpOverride = LevelIncreaseOverride;
            locationComponent.m_enemyMinLevelOverride = MinLevelOverride;
            locationComponent.m_enemyMaxLevelOverride = MaxLevelOverride;
            locationComponent.m_hasInterior = HasInterior;
            locationComponent.m_interiorRadius = InteriorRadius;
            locationComponent.m_interiorEnvironment = InteriorEnvironment;
            locationComponent.m_interiorPrefab = InteriorPrefab;
            locationComponent.m_useCustomInteriorTransform = InteriorTransformCustom;
            locationComponent.m_interiorTransform = InteriorTransform;
            locationComponent.m_generator = DungeonGeneratorObject;
            locationComponent.m_noBuild = NoBuildLocation;
            locationComponent.m_noBuildRadiusOverride = NoBuildRadiusOverride;

            return locationComponent;
        }

        internal LocationConfiguration patchFrom(GameObject? prefabRoot)
        {
            var locationComponent = prefabRoot!.GetComponent<Location>();

            // TODO delta-check against a LocationConfigurationDefaults
            // TODO patch defaults with locationcomponent values

            return this;
        }
    }
}
