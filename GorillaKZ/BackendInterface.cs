using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using WebSocketSharp;
using UnityEngine;
using VmodMonkeMapLoader;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using Photon.Pun;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace GorillaKZ
{
	public static class BackendInterface
	{
#if DEBUG
		const string urlBase = @"localhost:5000";
#else
		const string urlBase = Secrets.baseUrl;
#endif

		static HttpClient client = new HttpClient();
		static WebSocket socket;

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

		public static void OnJoinMap()
		{
			// Make sure we don't leave any open connections
			if (socket != null && socket.IsAlive) socket.CloseAsync();

			socket = new WebSocket($"ws://{urlBase}/ws");
			socket.Connect();
			socket.OnMessage += (sender, e) =>
			{
				UpdateLeaderboard(e.Data);
			};
			socket.Send(MapName + "\n" + ID);

			Task.Run(SendPings);
		}

		public static void SendPings()
		{
			while (true)
			{
				if (socket == null || !socket.IsAlive) return;
				socket.Ping();
				Thread.Sleep(60 * 1000);
			}
		}

		public static void OnLeftMap()
		{
			if (socket != null && socket.IsAlive) socket.CloseAsync();
			socket = null;
		}

		public static void SumbitRun(float time, FileInfo replay)
		{
			if (!(PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.IsVisible)) return;

			string key = ComputeSha256Hash(MapName + ID + time.ToString() + CheckpointManager.teleports.ToString() + Secrets.BackendKey);
			Debug.Log($"Submitted run: runner:{GorillaKZManager.username} time:{time} map:{MapName} tp:{CheckpointManager.teleports} key:{key}");
			var body = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Map", MapName),
				new KeyValuePair<string, string>("UUID", ID),
				new KeyValuePair<string, string>("Name", GorillaKZManager.username),
				new KeyValuePair<string, string>("Time", time.ToString()),
				new KeyValuePair<string, string>("TP", CheckpointManager.teleports.ToString()),
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

		static void SubmitReplay(string guid, FileInfo replay)
		{
			MultipartFormDataContent form = new MultipartFormDataContent()
			{
				{ new ByteArrayContent(File.ReadAllBytes(replay.FullName)), "file", replay.Name }
			};
			client.PostAsync($"http://{urlBase}/submittime/{guid}", form);
		}

		public static string ComputeSha256Hash(string rawData)
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

		static void UpdateLeaderboard(string data)
		{
			var res = JsonConvert.DeserializeObject<DataType>(data);

			RunCollection topRuns = new RunCollection(1, res.TopRuns.Names.Select((x, i) => new Run(x, res.TopRuns.Times[i])).ToArray());
			RunCollection localRuns = new RunCollection(res.LocalRuns.Offset + 1, res.LocalRuns.Names.Select((x, i) => new Run(x, res.LocalRuns.Times[i])).ToArray());

			LeaderboardManager.UpdateLeaderboard(topRuns, localRuns);
		}
	}
}
