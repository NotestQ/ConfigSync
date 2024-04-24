# ConfigSync

## Dependencies
This depends on [Mycelium](https://github.com/RugbugRedfern/Mycelium-Networking-For-Content-Warning)!  

## How do I use this
Download the [latest dll](https://github.com/NotestQ/ConfigSync/releases/latest) from the releases tab or clone and build the repo, then add it as a reference to the project. After adding it as a reference you can add it as a dependency:  
```cs
[BepInDependency(ConfigSync.MyPluginInfo.PLUGIN_GUID)] // Make sure to specify if it's a soft or a hard dependency! BepInEx sets dependencies to hard by default.
public class YourMod : BaseUnityPlugin { // ...
```  

And for in-depth documentation, check out the [documentation](https://github.com/NotestQ/ConfigSync/wiki/ConfigSync-Documentation) or one of the demos! Demos available are the [BepInEx config branch](https://github.com/NotestQ/ConfigSync-Demo/tree/master) and the [ContentSettings config branch](https://github.com/NotestQ/ConfigSync-Demo/tree/feat_ContentSettingsCompatibility)

## Does this add something?
By itself, no — this is an API for mod developers to _temporarily_ sync settings/configurations from the host to other players when in a lobby.

## It doesn't work
If the mod is throwing an error use [the github issues page](https://github.com/NotestQ/ConfigSync/issues) and copy-paste the error in there, with a description of what is happening and what you expected to happen if applicable. Or just ping me at the Content Warning Modding Discord server!

## Credits

Big thanks to [Day A.K.A. www.day.dream](https://thunderstore.io/c/content-warning/p/www_Day_Dream/) for helping me test the library and helping me with reflection!
