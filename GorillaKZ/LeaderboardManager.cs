using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VmodMonkeMapLoader;

namespace GorillaKZ
{
	public static class LeaderboardManager
	{
		static AssetBundle leaderboardBundle;
		static GameObject leaderboard;
		static Text topTimesText;
		static Text localTimesText;

		public static void CreateLeaderboard(string parentName)
		{
			if (leaderboardBundle == null)
			{
				string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "leaderboard");
				leaderboardBundle = AssetBundle.LoadFromFile(path);
			}

			leaderboard = UnityEngine.Object.Instantiate(leaderboardBundle.LoadAsset<GameObject>("Object"));

			Transform parentTransform = GameObject.Find(parentName).transform;
			leaderboard.transform.parent = parentTransform;
			leaderboard.transform.localPosition = Vector3.zero;
			leaderboard.transform.localEulerAngles = Vector3.zero;

			topTimesText = leaderboard.transform.Find("Canvas/Global").GetComponent<Text>();
			localTimesText = leaderboard.transform.Find("Canvas/Local").GetComponent<Text>();

			StringBuilder topSB = new StringBuilder().AppendLine(Events.Descriptor.MapName.ToUpper());
			topSB.AppendLine("GETTING TIMES...");
			topTimesText.text = topSB.ToString();
			localTimesText.text = "";
		}

		// Pretty sure this is useless, since it should be destroyed when the map is
		public static void DestroyLeaderboard()
		{
			if (leaderboard != null) UnityEngine.Object.Destroy(leaderboard);
		}

		public static void UpdateLeaderboard(RunCollection top, RunCollection local)
		{
			StringBuilder topSB = new StringBuilder().AppendLine(Events.Descriptor.MapName.ToUpper());
			topSB.Append(top.Render());
			topTimesText.text = topSB.ToString();

			StringBuilder localSB = new StringBuilder().AppendLine();
			localSB.AppendLine(local.Render());
			localTimesText.text = localSB.ToString();
		}
	}
}
