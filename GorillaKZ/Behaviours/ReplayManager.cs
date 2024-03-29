﻿using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using static System.BitConverter;

namespace GorillaKZ.Behaviours
{
	public class ReplayManager : MonoBehaviour
	{
		public static ReplayManager instance;

		enum DataCode : byte
		{
			Left,
			Right,
			Checkpoint,
			Teleport
		}

		static readonly byte[] MagicBytes = { 0x47, 0x54 };
		const ushort Version = 1;
		const byte Platform = 0;

		List<byte> buffer = new List<byte>(170);

		DirectoryInfo gkzDirectory;
		DirectoryInfo replayDirectory;

		bool recording = false;
		float recordingTime;

		bool lastL = false;
		bool lastR = false;

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

			GorillaKZManager.instance.OnStartRun += StartRecording;
			GorillaKZManager.instance.OnResetRun += ResetRecording;

			int maxReplayCount = 100;

			gkzDirectory = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GorillaKZ"));
			replayDirectory = gkzDirectory.CreateSubdirectory("Replays");
			var replays = replayDirectory.GetFiles();
			if (replays.Length > maxReplayCount)
			{
				replays.OrderBy(x => x.LastWriteTime).Take(replays.Length - maxReplayCount).ToList().ForEach(x =>
				{
					Debug.Log("Deleting old replay: " + x.Name);
					x.Delete();
				});
			}
		}

		void Update()
		{
			if (recording)
			{
				recordingTime += Time.deltaTime;
				bool l = Player.Instance.wasLeftHandTouching;
				bool r = Player.Instance.wasRightHandTouching;

				if (l && !lastL)
				{
					AddTouch(DataCode.Left, Player.Instance.leftHandTransform.position);
				}
				if (r && !lastR)
				{
					AddTouch(DataCode.Right, Player.Instance.rightHandTransform.position);
				}

				lastL = l;
				lastR = r;
			}
		}

		byte[] GetStringBytes(string s)
		{
			var bytes = Encoding.ASCII.GetBytes(s);
			var finalBytes = new byte[bytes.Length + 1];
			bytes.CopyTo(finalBytes, 0);
			return finalBytes;
		}

		void AddTouch(DataCode code, Vector3 pos)
		{
			buffer.Add((byte)code);
			buffer.AddRange(GetBytes(recordingTime));
			buffer.AddRange(GetBytes(pos.x));
			buffer.AddRange(GetBytes(pos.y));
			buffer.AddRange(GetBytes(pos.z));
		}

		void StartRecording(object sender, EventArgs e) => StartRecording();
		void StartRecording()
		{
			ResetRecording();

			recording = true;
			recordingTime = 0;

			foreach(var b in MagicBytes)
			{
				buffer.Add(b);
			}

			buffer.AddRange(GetBytes(Version));

			buffer.Add(Platform);

			buffer.AddRange(GetStringBytes($"GKZv{PluginInfo.Version}"));

			buffer.AddRange(GetStringBytes(VmodMonkeMapLoader.Events.MapName));

			var mods = BepInEx.Bootstrap.Chainloader.PluginInfos.Select(x => x.Value.Metadata.GUID);
			buffer.AddRange(GetStringBytes(string.Join(",", mods)));
		}

		void ResetRecording(object sender, EventArgs e) => ResetRecording();
		void ResetRecording()
		{
			recording = false;
			buffer = new List<byte>(170);
		}

		public FileInfo EndRecording(string fileName)
		{
			using (FileStream fs = File.Create(Path.Combine(replayDirectory.ToString(), fileName), buffer.Count))
			{
				fs.Write(buffer.ToArray(), 0, buffer.Count);
			}

			ResetRecording();

			return new FileInfo(Path.Combine(replayDirectory.ToString(), fileName));
		}
	}
}
