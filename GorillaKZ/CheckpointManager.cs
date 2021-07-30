using GorillaKZ.Patches;
using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VmodMonkeMapLoader;

namespace GorillaKZ
{
	public static class CheckpointManager
	{
		const float RaycastDistance = 1.0f;

		static Stack<Vector3> checkpointsPos = new Stack<Vector3>();
		static Stack<float> checkpointsRot = new Stack<float>();

		public static int teleports = 0;

		public static void LoadCheckpoint()
		{
			if (!GorillaKZManager.useCheckpoints) return;

			if (checkpointsPos.Count > 0)
			{
				PlayerTeleportPatch.TeleportPlayer(checkpointsPos.Peek(), checkpointsRot.Peek());
				teleports++;
			}
		}

		public static void SaveCheckpoint()
		{
			if (!GorillaKZManager.useCheckpoints) return;

			if (Physics.Raycast(Player.Instance.bodyCollider.transform.position, Vector3.down, 1.0f, 1 << 9))
			{
				Transform t = Player.Instance.bodyCollider.transform;
				var pos = t.position;
				var rot = t.eulerAngles.y;
				checkpointsPos.Push(pos);
				checkpointsRot.Push(rot);

				GorillaTagger.Instance.myVRRig.PlayTagSound(1);
			}
			else
			{
				GorillaTagger.Instance.myVRRig.PlayTagSound(0);
			}
		}

		public static void DeleteCheckpoint()
		{
			if (checkpointsPos.Count > 1)
			{
				checkpointsPos.Pop();
				checkpointsRot.Pop();
			}
		}

		public static void RemoveCheckpoints()
		{
			while (checkpointsPos.Count > 1)
			{
				checkpointsPos.Pop();
				checkpointsRot.Pop();
			}
		}

		public static void ResetCheckpoints()
		{
			checkpointsPos = new Stack<Vector3>();
			checkpointsRot = new Stack<float>();
			teleports = 0;
			if (Events.Descriptor?.SpawnPoints != null)
			{
				var firstTeleport = Events.Descriptor.SpawnPoints[0];
				checkpointsPos.Push(firstTeleport.position);
				checkpointsRot.Push(firstTeleport.rotation.eulerAngles.y);
			}
		}
	}
}
