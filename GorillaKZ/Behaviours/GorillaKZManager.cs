using GorillaKZ.Models;
using Photon.Pun;
using System;
using UnityEngine;
using VmodMonkeMapLoader;

namespace GorillaKZ.Behaviours
{
	public class GorillaKZManager : MonoBehaviour
	{
		public static GorillaKZManager instance;

		public string Username { get; private set; }
		public bool InGKZMap { get; private set; } = false;
		public bool ValidRun { get; private set; }

		bool running;

		string startName;
		string endName;

		public class GKZData : EventArgs
		{
			public bool checkpoints;
			public string start;
			public string end;
			public string leaderboard;
		}

		internal EventHandler<GKZData> OnGKZMapEnter;
		internal EventHandler OnGKZMapLeave;

		internal EventHandler OnStartRun;
		internal EventHandler OnEndRun;
		internal EventHandler OnResetRun;

		void Awake()
		{
			if (instance != null)
			{
				Destroy(this);
				return;
			}
			else
			{
				instance = this;
			}

			Events.OnMapEnter += OnMapEnter;

			CreateTimer();

			gameObject.AddComponent<CheckpointManager>();
			gameObject.AddComponent<LeaderboardManager>();
			gameObject.AddComponent<BackendInterface>();
			gameObject.AddComponent<ReplayManager>();
		}

		void CreateTimer()
		{
			string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "gkz_timer");
			AssetBundle bundle = AssetBundle.LoadFromFile(path);
			GameObject timerGameObject = Instantiate(bundle.LoadAsset<GameObject>("Object"));

			Transform parentTransform = GameObject.Find("GorillaPlayer/TurnParent/Main Camera").transform;
			timerGameObject.transform.parent = parentTransform;
			timerGameObject.transform.localPosition = new Vector3(0, -0.03f, 0.3f);
			timerGameObject.transform.localEulerAngles = Vector3.zero;
			timerGameObject.transform.localScale = Vector3.one * 0.4f;

			timerGameObject.AddComponent<Timer>();
		}

		void OnMapEnter(bool enter)
		{
			if (enter)
			{
				var customData = Events.PackageInfo.Config.CustomData;
				if (customData == null) return;

				if (customData.TryGetValue("gamemode", out var gamemode) && gamemode as string == "GorillaKZ")
				{
					InGKZMap = true;
					Username = PlayerPrefs.GetString("playerName");

					GKZData gkzData = (customData["gkz"] as Newtonsoft.Json.Linq.JObject).ToObject<GKZData>();

					startName = gkzData.start;
					endName = gkzData.end;

					OnGKZMapEnter?.Invoke(this, gkzData);
				}
				else
				{
					InGKZMap = false;
				}
			}
			else
			{
				InGKZMap = false;
				OnGKZMapLeave?.Invoke(this, EventArgs.Empty);
			}
		}

		void StartRun()
		{
			if (!running)
			{
				ValidRun = IsValidRoom();
				OnStartRun?.Invoke(this, EventArgs.Empty);

				running = true;
			}
		}

		void EndRun()
		{
			if (running)
			{
				running = false;
				RunTime time = Timer.instance.StopTimer();

				string fileName = Username + Events.Descriptor.MapName + time + DateTime.Now.ToString("-yyyy-dd-M-HH-mm-ss") + ".gtrec";
				System.IO.FileInfo file = ReplayManager.instance.EndRecording(fileName);

				if (ValidRun) BackendInterface.instance.SumbitRun(time, file);

				OnEndRun?.Invoke(this, EventArgs.Empty);
			}
		}

		void ResetRun()
		{
			running = false;
			OnResetRun?.Invoke(this, EventArgs.Empty);
		}

		void OnTriggerEnter(Collider other)
		{
			if (!InGKZMap) return;

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
			if (!InGKZMap) return;

			if (other.gameObject.name == startName)
			{
				StartRun();
			}
		}

		public static bool IsValidRoom()
		{
			if (!PhotonNetwork.InRoom) return false;

			if (PhotonNetwork.CurrentRoom.IsVisible)
			{
				LeaderboardManager.instance.ShowMessage("Join a public room\nto start a run!", true);
				return false;
			}

			if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out var gamemodeObject)
				&& (gamemodeObject as string)?.Contains("MODDED") ?? true)
			{
				LeaderboardManager.instance.ShowMessage("Join a vanilla gamemode\nto start a run!", true);
				return false;
			}

			return true;
		}
	}
}
