using GorillaKZ.Behaviours;
using System.Text;

namespace GorillaKZ.Models
{
	public class RunCollection
	{
		const int MaxNameLenght = 12;

		int offset = 1;
		Run[] runs;

		public RunCollection(int offset, Run[] runs)
		{
			this.offset = offset;
			this.runs = runs;
		}

		public string Render()
		{
			StringBuilder text = new StringBuilder();

			bool first = true;
			for (int i = 0; i < runs.Length; i++)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					text.AppendLine();
				}

				Run run = runs[i];

				int place = i + offset;

				if (run.Runner == GorillaKZManager.instance.Username) text.StartColor("00D700");
				else if (place == 1) text.StartColor("FFD700");
				else if (place == 2) text.StartColor("C0C0C0");
				else if (place == 3) text.StartColor("CD7F32");

				text.Append(place.ToString().PadLeft(2)).Append("  ");
				text.Append(run.Runner.PadRight(MaxNameLenght)).Append("  ");
				// TODO: This breaks with over 59 minutes
				text.Append(run.Time.ToString("mm\\:ss\\.fff"));

				if (place <= 3 || run.Runner == GorillaKZManager.instance.Username) text.EndColor();
			}

			return text.ToString();
		}
	}
}
