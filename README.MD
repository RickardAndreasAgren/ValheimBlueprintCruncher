
==DESCRIPTION==
Unity plugin for Valheim to import a blueprints building to a Location prefab,
create prefabs for all blueprints in a folder or update an existing buildings
configuration. Based on .blueprint format. I made this to be able to construct
buildings in "creative" mode inside the game, rather than building in Unity, and
then add additional elements like spawners, and randomizers in Unity.

==SETUP==
Follow https://valheim-modding.github.io/Jotunn/guides/overview.html to set up your
development environment. Open the BlueprintCruncher.csproj file and adjust filepaths in
GamePath, BepInExPath, CorlibPath, PublicizedAssembliesPath, UnityPath, VSStudioPath
and <Analyzer * /> elements.
Build project, and ensure that BlueprintCruncherWindow is copied to the .dll output.
(Temp/bin/Debug | Temp/bin/Release)
In your Unity project, under Assets/Plugins make BlueprintCruncher folder and copy
.dll & .csproj from the output into it.
You will now have a menu choice under the Window menu called BlueprintCruncher.

Use a mod like InfinityHammer (Requires: Server Devcommands, World Edit Commands. 
  See https://github.com/JereKuusela/valheim-infinity_hammer/blob/main/README.md )


==FEATURES==
-Import a .blueprint to provide a building to selected Location prefab root (Opened prefab)
-Import all .blueprint files from a folder to create new Location prefabs
-Configuration
--Set location properties
--Use Firemode to change behaviour of loaded light prefabs
--Automagic translation of wood and stone chests to their biome defaults with loot table

==OPTIONS==
-File/Folder path to get .blueprint/folder with .blueprint files
-Clean up temporary objects generated to build new assets
-Firemode
--Default makes light pieces behave like normal built versions, starting with 2 fuel
--Dead empties fuel and prevents refilling
--Infinite removes refilling and toggles on infinite burning
-Biome sets the location biome. Will dictate automatic chest translation
-Clear Area removes vegetation
-Exterior radius sets the location size
-Discovery Label provides pop-up text when discovering a location
-Level increase overrides are for tweaking spawns
-No Build Location locks out buiding near the Location.
-No Build Radius Override specifies area of building prevention