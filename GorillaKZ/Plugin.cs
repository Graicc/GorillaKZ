using BepInEx;
using GorillaKZ.Behaviours;
using GorillaLocomotion;

namespace GorillaKZ
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
	public class Plugin : BaseUnityPlugin
	{
		void Awake()
		{
			HarmonyPatches.ApplyHarmonyPatches();
			Player.Instance.gameObject.AddComponent<GorillaKZManager>();
		}
	}
}
