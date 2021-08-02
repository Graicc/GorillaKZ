using GorillaKZ.Models;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VmodMonkeMapLoader;

namespace GorillaKZ.Behaviours
{
	public class LeaderboardManager : MonoBehaviour
	{
		public static LeaderboardManager instance;

		AssetBundle leaderboardBundle;
		GameObject leaderboard;
		Text topTimesText;
		Text localTimesText;

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

			GorillaKZManager.instance.OnGKZMapEnter += CreateLeaderboard;
			GorillaKZManager.instance.OnGKZMapLeave += DestroyLeaderboard;
		}

		void CreateLeaderboard(object sender, GorillaKZManager.GKZData e)
		{
			if (leaderboardBundle == null)
			{
				string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "leaderboard");
				leaderboardBundle = AssetBundle.LoadFromFile(path);
			}

			leaderboard = Instantiate(leaderboardBundle.LoadAsset<GameObject>("Object"));

			Transform parentTransform = GameObject.Find(e.leaderboard).transform;
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
		void DestroyLeaderboard(object sender, EventArgs e)
		{
			if (leaderboard != null) Destroy(leaderboard);
		}

		public void UpdateLeaderboard(RunCollection top, RunCollection local)
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
