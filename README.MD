
==DESCRIPTION==<br/>
Unity plugin for Valheim to import a blueprints building to a Location prefab,<br/>
create prefabs for all blueprints in a folder or update an existing buildings<br/>
configuration. Based on .blueprint format. I made this to be able to construct<br/>
buildings in "creative" mode inside the game, rather than building in Unity, and<br/>
then add additional elements like spawners, and randomizers in Unity.<br/>
<br/>
==SETUP==<br/>
Follow https://valheim-modding.github.io/Jotunn/guides/overview.html to set up your<br/>
development environment. Open the BlueprintCruncher.csproj file and adjust filepaths in<br/>
GamePath, BepInExPath, CorlibPath, PublicizedAssembliesPath, UnityPath, VSStudioPath
and <Analyzer * /> elements.<br/>
Build project, and ensure that BlueprintCruncherWindow is copied to the .dll output.<br/>
(Temp/bin/Debug | Temp/bin/Release)<br/>
In your Unity project, under Assets/Plugins make BlueprintCruncher folder and copy
.dll & .csproj from the output into it.<br/>
You will now have a menu choice under the Window menu called BlueprintCruncher.<br/>
<br/>
Enable console for Valheim and use a mod like InfinityHammer (Requires: Server Devcommands,
World Edit Commands. See https://github.com/JereKuusela/valheim-infinity_hammer/blob/main/README.md )<br/>
  Reference commands:<br/>
  hammer connect (while aiming at your building)<br/>
  hammer_save <filename> (save currently held ghost into .blueprint in BepInEx\config\PlanBuild)<br/>
<br/>

==FEATURES==<br/>
-Import a .blueprint to provide a building to selected Location prefab root (Opened prefab)<br/>
-Import all .blueprint files from a folder to create new Location prefabs<br/>
-Configuration<br/>
--Set location properties<br/>
--Use Firemode to change behaviour of loaded light prefabs<br/>
--Automagic translation of wood and stone chests to their biome defaults with loot table<br/>
<br/>
==OPTIONS==<br/>
-File/Folder path to get .blueprint/folder with .blueprint files<br/>
-Clean up temporary objects generated to build new assets<br/>
-Firemode<br/>
--Default makes light pieces behave like normal built versions, starting with 2 fuel<br/>
--Dead empties fuel and prevents refilling<br/>
--Infinite removes refilling and toggles on infinite burning<br/>
-Biome sets the location biome. Will dictate automatic chest translation<br/>
-Clear Area removes vegetation<br/>
-Exterior radius sets the location size<br/>
-Discovery Label provides pop-up text when discovering a location<br/>
-Level increase overrides are for tweaking spawns<br/>
-No Build Location locks out buiding near the Location.<br/>
-No Build Radius Override specifies area of building prevention<br/>