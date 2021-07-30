using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;
using GorillaLocomotion;

namespace GorillaKZ
{
	public class ReplayManager : MonoBehaviour
	{
		enum DataCode : byte
		{
			Left,
			Right,
			Checkpoint,
			Teleport
		}

		List<byte> buffer = new List<byte>(170);

		DirectoryInfo gkzDirectory;
		DirectoryInfo replayDirectory;

		bool recording = false;
		float recordingTime;

		bool lastL = false;
		bool lastR = false;

		public void Awake()
		{
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

		public void Update()
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
					AddTouch(DataCode.Right, Player.Instance.leftHandTransform.position);
				}

				lastL = l;
				lastR = r;
			}
		}

		void AddTouch(DataCode code, Vector3 pos)
		{
			buffer.Add((byte)code);
			buffer.AddRange(BitConverter.GetBytes(recordingTime));
			buffer.AddRange(BitConverter.GetBytes(pos.x));
			buffer.AddRange(BitConverter.GetBytes(pos.y));
			buffer.AddRange(BitConverter.GetBytes(pos.z));
		}

		public void StartRecording()
		{
			ResetRecording();

			recording = true;
			recordingTime = 0;
		}

		public void ResetRecording()
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
