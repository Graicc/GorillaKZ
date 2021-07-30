using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaKZ
{
	public class Run
	{
		public string Runner { get; }
		public TimeSpan Time { get; }

		public Run(string runner, float time)
		{
			Runner = runner;
			Time = TimeSpan.FromSeconds(time);
		}
	}
}
