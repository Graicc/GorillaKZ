// Adapted from https://github.com/Vadix88/MonkeMapLoader/blob/dev/MonkeMapLoader/Patches/PlayerTeleportPatch.cs
// Used with permission

using GorillaLocomotion;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace GorillaKZ.Patches
{
	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("Update", MethodType.Normal)]
	internal class PlayerTeleportPatch
	{
		private static bool _isTeleporting = false;
		private static Vector3 _teleportPosition;
		private static float _teleportRotation;

		internal static bool Prefix(Player __instance, ref Vector3 ___lastPosition, ref Vector3[] ___velocityHistory, ref Vector3 ___lastHeadPosition, ref Vector3 ___lastLeftHandPosition, ref Vector3 ___lastRightHandPosition, ref Vector3 ___currentVelocity, ref Vector3 ___denormalizedVelocityAverage)
		{
			if (_isTeleporting)
			{
				var playerRigidBody = __instance.GetComponent<Rigidbody>();
				if (playerRigidBody != null)
				{
					Vector3 correctedPosition = _teleportPosition - __instance.bodyCollider.transform.position + __instance.transform.position;

					playerRigidBody.velocity = Vector3.zero;
					playerRigidBody.isKinematic = true;
					__instance.transform.position = correctedPosition;
					Debug.Log(__instance.bodyCollider.transform.position);

					//__instance.transform.rotation = _teleportDestination.rotation;

					//__instance.transform.rotation = Quaternion.Euler(__instance.transform.rotation.eulerAngles.x, _teleportDestination.rotation.eulerAngles.y,
					//    __instance.transform.rotation.eulerAngles.z);
					__instance.Turn(_teleportRotation - __instance.headCollider.transform.rotation.eulerAngles.y);

					___lastPosition = correctedPosition;
					___velocityHistory = new Vector3[__instance.velocityHistorySize];

					___lastHeadPosition = __instance.headCollider.transform.position;
					var leftHandMethod = typeof(Player).GetMethod("CurrentLeftHandPosition",
						BindingFlags.NonPublic | BindingFlags.Instance);
					___lastLeftHandPosition = (Vector3)leftHandMethod.Invoke(__instance, new object[] { });

					var rightHandMethod = typeof(Player).GetMethod("CurrentRightHandPosition",
						BindingFlags.NonPublic | BindingFlags.Instance);
					___lastRightHandPosition = (Vector3)rightHandMethod.Invoke(__instance, new object[] { });
					___currentVelocity = Vector3.zero;
					___denormalizedVelocityAverage = Vector3.zero;
					playerRigidBody.isKinematic = false;
				}
				_isTeleporting = false;
				return false;
			}

			return true;
		}

		internal static void TeleportPlayer(Vector3 destinationPosition, float destinationRotation)
		{
			if (_isTeleporting)
				return;

			_teleportPosition = destinationPosition;
			_teleportRotation = destinationRotation;
			_isTeleporting = true;
		}
	}
}