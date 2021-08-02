using GorillaKZ.Patches;
using GorillaLocomotion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using VmodMonkeMapLoader;

namespace GorillaKZ.Behaviours
{
	public class CheckpointManager : MonoBehaviour
	{
		public static CheckpointManager instance;

		bool useCheckpoints;

		InputDevice rightHand;
		InputDevice leftHand;

		bool lastRPrimary;
		bool lastLPrimary;
		bool lastLSecondary;

		const float RaycastDistance = 1.0f;

		Stack<Vector3> checkpointsPos = new Stack<Vector3>();
		Stack<float> checkpointsRot = new Stack<float>();

		public int teleports = 0;

		void Awake()
		{
			if (instance != null)
			{
				Destroy(this);
			}
			else
			{
				instance = this;
			}

			GorillaKZManager.instance.OnGKZMapEnter += OnMapEnter;
			GorillaKZManager.instance.OnGKZMapLeave += ResetCheckpoints;

			GorillaKZManager.instance.OnStartRun += ResetCheckpoints;
			GorillaKZManager.instance.OnEndRun += ResetCheckpoints;
			GorillaKZManager.instance.OnResetRun += ResetCheckpoints;
		}

		void Update()
		{
			if (rightHand == null || !rightHand.isValid)
			{
				rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
			}
			if (leftHand == null || !leftHand.isValid)
			{
				leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
			}

			if (!GorillaKZManager.instance.InGKZMap) return;

			if (useCheckpoints)
			{
				rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var rPrimary);
				leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out var lPrimary);
				leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out var lSecondary);

				if (rPrimary && !lastRPrimary)
				{
					LoadCheckpoint();
				}
				if (lPrimary && !lastLPrimary)
				{
					SaveCheckpoint();
				}
				if (lSecondary && !lastLSecondary)
				{
					DeleteCheckpoint();
				}

				lastRPrimary = rPrimary;
				lastLPrimary = lPrimary;
				lastLSecondary = lSecondary;
			}
		}

		void OnMapEnter(object sender, GorillaKZManager.GKZData e)
		{
			useCheckpoints = e.checkpoints;
			ResetCheckpoints();
		}

		void LoadCheckpoint()
		{
			if (!useCheckpoints) return;

			if (checkpointsPos.Count > 0)
			{
				PlayerTeleportPatch.TeleportPlayer(checkpointsPos.Peek(), checkpointsRot.Peek());
				teleports++;
			}
		}

		void SaveCheckpoint()
		{
			if (!useCheckpoints) return;

			if (Physics.Raycast(Player.Instance.bodyCollider.transform.position, Vector3.down, 1.0f, 1 << 9))
			{
				Transform t = Player.Instance.bodyCollider.transform;
				checkpointsPos.Push(t.position);
				checkpointsRot.Push(t.eulerAngles.y);

				GorillaTagger.Instance.myVRRig.PlayTagSound(1);
			}
			else
			{
				GorillaTagger.Instance.myVRRig.PlayTagSound(0);
			}
		}

		void DeleteCheckpoint()
		{
			if (checkpointsPos.Count > 1)
			{
				checkpointsPos.Pop();
				checkpointsRot.Pop();
			}
		}

		void ResetCheckpoints(object sender, EventArgs e) => ResetCheckpoints();
		void ResetCheckpoints()
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
