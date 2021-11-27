using GorillaKZ.Models;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaKZ.Behaviours
{
	public class Timer : MonoBehaviour
	{
		public static Timer instance;

		Text text;

		bool running = false;
		DateTime startTime = new DateTime();

		Color normalColor = new Color(1, 1, 1);
		Color invalidColor = new Color(1, 0, 0);

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

			GorillaKZManager.instance.OnGKZMapEnter += ShowTimer;
			GorillaKZManager.instance.OnGKZMapLeave += HideTimer;

			GorillaKZManager.instance.OnStartRun += StartTimer;
			GorillaKZManager.instance.OnResetRun += ResetTimer;

			text = gameObject.GetComponentInChildren<Text>();

			HideTimer();
		}

		void Update()
		{
			if (GorillaKZManager.instance.ValidRun && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.IsVisible)
			{
				text.color = normalColor;
			}
			else
			{
				text.color = invalidColor;
			}

			if (running)
			{
				text.text = (DateTime.Now - startTime).ToString("m\\:ss\\.fff");
			}
		}

		void ShowTimer(object sender, EventArgs e) => ShowTimer();
		void ShowTimer()
		{
			text.enabled = true;
			ResetTimer();
		}

		void HideTimer(object sender, EventArgs e) => HideTimer();
		void HideTimer()
		{
			text.enabled = false;
		}

		void StartTimer(object sender, EventArgs e) => StartTimer();
		void StartTimer()
		{
			running = true;
			startTime = DateTime.Now;
		}

		void ResetTimer(object sender, EventArgs e) => ResetTimer();
		void ResetTimer()
		{
			running = false;
			text.text = "0:00.000";
		}

		public RunTime StopTimer()
		{
			running = false;

			TimeSpan diff = DateTime.Now - startTime;
			text.text = diff.ToString("m\\:ss\\.fff");

			startTime = DateTime.Now;

			return (float)diff.TotalSeconds;
		}
	}
}

