using System;
using BepInEx;
using UnityEngine;
using GorillaLocomotion;
using HarmonyLib;

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
