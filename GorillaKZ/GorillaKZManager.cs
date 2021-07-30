using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VmodMonkeMapLoader;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Threading.Tasks;
using UnityEngine.XR;

namespace GorillaKZ
{
	public class GorillaKZManager : MonoBehaviour
	{
		public static string username;

		bool inGKZMap = false;

		Timer timer;
		bool running;
		public static bool ValidRun { get; private set; } = true;

		public static bool useCheckpoints = false;

		ReplayManager replayManager;

		string startName;
		string endName;

		InputDevice rightHand;
		InputDevice leftHand;

		bool lastRPrimary;
		bool lastLPrimary;
		bool lastLSecondary;

		public class GKZData
		{
			public bool checkpoints;
			public string start;
			public string end;
			public string leaderboard;
		}

		public void Awake()
		{
			Events.OnMapEnter += OnMapEnter;
			Events.OnMapChange += OnMapChange;
			replayManager = gameObject.AddComponent<ReplayManager>();
			CreateTimer();
			HideTimer();
		}

		public void Update()
		{
			if (rightHand == null || !rightHand.isValid)
			{
				rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
			}
			if (leftHand == null || !leftHand.isValid)
			{
				leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
			}

			if (!inGKZMap) return;

			if (useCheckpoints)
			{
				rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var rPrimary);
				leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out var lPrimary);
				leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out var lSecondary);

				if (rPrimary && !lastRPrimary)
				{
					CheckpointManager.LoadCheckpoint();
				}
				if (lPrimary && !lastLPrimary)
				{
					CheckpointManager.SaveCheckpoint();
				}
				if (lSecondary && !lastLSecondary)
				{
					CheckpointManager.DeleteCheckpoint();
				}

				lastRPrimary = rPrimary;
				lastLPrimary = lPrimary;
				lastLSecondary = lSecondary;
			}
		}

		void CreateTimer()
		{
			string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "gkz_timer");
			AssetBundle bundle = AssetBundle.LoadFromFile(path);
			GameObject timerGameObject = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>("Object"));

			Transform parentTransform = GameObject.Find("GorillaPlayer/TurnParent/Main Camera").transform;
			timerGameObject.transform.parent = parentTransform;
			timerGameObject.transform.localPosition = new Vector3(0, -0.03f, 0.3f);
			timerGameObject.transform.localEulerAngles = Vector3.zero;
			timerGameObject.transform.localScale = Vector3.one * 0.4f;

			timer = timerGameObject.AddComponent<Timer>();
		}

		void HideTimer()
		{
			timer.gameObject.SetActive(false);
		}

		void ShowTimer()
		{
			timer.gameObject.SetActive(true);
			timer.ResetTimer();
		}

		void OnMapEnter(bool enter)
		{
			if (enter)
			{
				var customData = Events.PackageInfo.Config.CustomData;
				if (customData.TryGetValue("gamemode", out var gamemode) && gamemode as string == "GorillaKZ")
				{
					inGKZMap = true;
					username = PlayerPrefs.GetString("playerName");

					ShowTimer();

					GKZData gkzData = (customData["gkz"] as Newtonsoft.Json.Linq.JObject).ToObject<GKZData>();

					useCheckpoints = gkzData.checkpoints;
					startName = gkzData.start;
					endName = gkzData.end;

					LeaderboardManager.CreateLeaderboard(gkzData.leaderboard);

					CheckpointManager.ResetCheckpoints();

					Task.Run(BackendInterface.OnJoinMap);
				}
				else
				{
					inGKZMap = false;
					HideTimer();
				}
			}
			else
			{
				LeaderboardManager.DestroyLeaderboard();
				BackendInterface.OnLeftMap();
				CheckpointManager.ResetCheckpoints();
				inGKZMap = false;
				HideTimer();
			}
		}

		void OnMapChange(bool enter)
		{
			if (!enter)
			{
				//LeaderboardManager.DestroyLeaderboard();
				//BackendInterface.OnLeftMap();
			}
		}

		void StartRun()
		{
			if (!running)
			{
				ValidRun = PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.IsVisible;
				timer.StartTimer();

				CheckpointManager.ResetCheckpoints();

				replayManager.StartRecording();

				running = true;
			}
		}

		void EndRun()
		{
			if (running)
			{
				running = false;

				float time = timer.StopTimer();
				string fileName = username + Events.Descriptor.MapName + time + DateTime.Now.ToString("-yyyy-dd-M-HH-mm-ss") + ".gtrec";
				System.IO.FileInfo file = replayManager.EndRecording(fileName);

				if (ValidRun) BackendInterface.SumbitRun(time, file);

				// Do this last so that the number of teleports is stil available
				CheckpointManager.ResetCheckpoints();
			}
		}

		void ResetRun()
		{
			running = false;
			timer.ResetTimer();
			replayManager.ResetRecording();
			CheckpointManager.ResetCheckpoints();
		}

		void OnTriggerEnter(Collider other)
		{
			if (!inGKZMap) return;

			if (other.gameObject.name == startName)
			{
				ResetRun();
			}
			else if (other.gameObject.name == endName)
			{
				EndRun();
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (!inGKZMap) return;

			if (other.gameObject.name == startName)
			{
				StartRun();
			}
		}
	}
}
