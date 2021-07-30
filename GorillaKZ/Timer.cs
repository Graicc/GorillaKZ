using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaKZ
{
	public class Timer : MonoBehaviour
	{
		Text text;

		bool running = false;
		DateTime startTime = new DateTime();

		Color normalColor = new Color(1, 1, 1);
		Color invalidColor = new Color(1, 0, 0);

		public void Awake()
		{

			text = gameObject.GetComponentInChildren<Text>();
			//text = gameObject.AddComponent<Text>();
			//text.text = "0:00.000";
		}

		public void Update()
		{
			if (GorillaKZManager.ValidRun && (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.IsVisible))
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

		public void StartTimer()
		{
			running = true;
			startTime = DateTime.Now;
		}

		public float StopTimer()
		{
			running = false;


			TimeSpan diff = (DateTime.Now - startTime);
			text.text = diff.ToString("m\\:ss\\.fff");

			startTime = DateTime.Now;

			return (float)diff.TotalSeconds;
		}

		public void ResetTimer()
		{
			running = false;
			text.text = "0:00.000";
		}
	}
}

