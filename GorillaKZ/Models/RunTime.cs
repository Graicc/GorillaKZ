using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GorillaKZ.Models
{
	public class RunTime
	{
		private readonly float time;

		public RunTime(float time)
		{
			this.time = time;
		}

		public override string ToString()
		{
			return time.ToString(CultureInfo.InvariantCulture);
		}

		public static implicit operator float(RunTime t) => t.time;
		public static implicit operator RunTime(float t) => new RunTime(t);
	}
}
