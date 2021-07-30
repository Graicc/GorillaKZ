using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace GorillaKZ.Patches
{

    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
	[HarmonyPatch("Awake", MethodType.Normal)]
	internal class PlayerStartPatch
	{
		private static void Postfix(GorillaLocomotion.Player __instance)
		{
			__instance.gameObject.AddComponent<GorillaKZManager>();
		}
	}
}
