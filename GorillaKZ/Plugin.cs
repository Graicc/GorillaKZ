using BepInEx;

namespace GorillaKZ
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
	public class Plugin : BaseUnityPlugin
	{
		void Awake()
		{
			HarmonyPatches.ApplyHarmonyPatches();
		}
	}
}
