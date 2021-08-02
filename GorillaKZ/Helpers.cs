using System.Text;

namespace GorillaKZ
{
	public static class Helpers
	{
		public static StringBuilder StartColor(this StringBuilder stringBuilder, string color)
		{
			return stringBuilder.Append($"<color=#{color}>");
		}

		public static StringBuilder EndColor(this StringBuilder stringBuilder)
		{
			return stringBuilder.Append($"</color>");
		}
	}
}
