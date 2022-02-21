using GorillaKZ.Models;
using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VmodMonkeMapLoader;
using WebSocketSharp;

namespace GorillaKZ.Behaviours
{
	public class BackendInterface : MonoBehaviour
	{
		public static BackendInterface instance;

#if DEBUG
		const string urlBase = @"localhost:5000";
#else
		const string urlBase = Secrets.baseUrl;
#endif

		static HttpClient client = new HttpClient();
		static WebSocket socket = new WebSocket($"ws://{urlBase}/ws");

		static string ID
		{
			get
			{
				return PhotonNetwork.LocalPlayer.UserId;
			}
		}

		static string MapName
		{
			get
			{
				return Events.Descriptor.MapName;
			}
		}

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

			GorillaKZManager.instance.OnGKZMapEnter += OnJoinMap;
			GorillaKZManager.instance.OnGKZMapLeave += OnLeftMap;

			socket.OnMessage += (sender, e) =>
			{
				UpdateLeaderboard(e.Data);
			};

			Task.Run(SendPings);
		}

		void OnJoinMap(object sender, GorillaKZManager.GKZData e) => Task.Run(OnJoinMap);
		void OnJoinMap()
		{
			socket.Close();

			socket.Connect();
			socket.Send(MapName + "\n" + ID);
		}

		void SendPings()
		{
			while (true)
			{
				if (socket != null && socket.IsAlive)
				{
					socket.Ping();
				}
				Thread.Sleep(45 * 1000);
			}
		}

		void OnLeftMap(object sender, EventArgs e)
		{
			socket.CloseAsync();
		}

		internal void SumbitRun(RunTime time, FileInfo replay)
		{
			if (!(GorillaKZManager.IsValidRoom())) return;

			string key = ComputeSha256Hash(MapName + ID + time.ToString() + CheckpointManager.instance.teleports.ToString() + Secrets.BackendKey);
			Debug.Log($"Submitted run: runner:{GorillaKZManager.instance.Username} time:{time} map:{MapName} tp:{CheckpointManager.instance.teleports} key:{key}");
			var body = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Map", MapName),
				new KeyValuePair<string, string>("UUID", ID),
				new KeyValuePair<string, string>("Name", GorillaKZManager.instance.Username),
				new KeyValuePair<string, string>("Time", time.ToString()),
				new KeyValuePair<string, string>("TP", CheckpointManager.instance.teleports.ToString()),
				new KeyValuePair<string, string>("Key", key)
			};

			Task.Run(() =>
			{
				string reply = client.PostAsync($"http://{urlBase}/submittime", new FormUrlEncodedContent(body)).Result.Content.ReadAsStringAsync().Result;
				if (reply != "")
				{
					SubmitReplay(reply, replay);
				}
			});
		}

		void SubmitReplay(string guid, FileInfo replay)
		{
			MultipartFormDataContent form = new MultipartFormDataContent()
			{
				{ new ByteArrayContent(File.ReadAllBytes(replay.FullName)), "file", replay.Name }
			};
			client.PostAsync($"http://{urlBase}/submittime/{guid}", form);
		}

		string ComputeSha256Hash(string rawData)
		{
			// Create a SHA256   
			using (SHA256 sha256Hash = SHA256.Create())
			{
				// ComputeHash - returns byte array  
				byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

				// Convert byte array to a string   
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("x2"));
				}
				return builder.ToString();
			}
		}

		public class DataType
		{
			public TopRuns TopRuns { get; set; }
			public LocalRuns LocalRuns { get; set; }
		}

		public class TopRuns
		{
			public string[] Names { get; set; }
			public float[] Times { get; set; }
		}

		public class LocalRuns
		{
			public int Offset { get; set; }
			public string[] Names { get; set; }
			public float[] Times { get; set; }
		}

		void UpdateLeaderboard(string data)
		{
			var res = JsonConvert.DeserializeObject<DataType>(data);

			RunCollection topRuns = new RunCollection(1, res.TopRuns.Names.Select((x, i) => new Run(x, res.TopRuns.Times[i])).ToArray());
			RunCollection localRuns = new RunCollection(res.LocalRuns.Offset + 1, res.LocalRuns.Names.Select((x, i) => new Run(x, res.LocalRuns.Times[i])).ToArray());

			LeaderboardManager.instance.UpdateLeaderboard(topRuns, localRuns);
		}
	}
}
