using System.Text;
using System.Text.RegularExpressions;

namespace AutoDocs
{
    public class MarkdownConverter
    {
        public List<Func<string, string>> Converters {get; set;} = new() {
            (s) => {
                if (s.StartsWith("# "))
                {
                    return $"__**{s.Substring(2)}**__";
                }
                return s;
            },
            (s) => {
                if (s.StartsWith("## "))
                {
                    return $"**{s.Substring(3)}**";
                }
                return s;
            },
            (s) => {
                Regex regex = new System.Text.RegularExpressions.Regex("<img.*?>");
                return regex.Replace(s, "");
            },
            (s) => {
                Regex regex = new Regex(@"\[(.*?)\]\((.*?)\)");
                return regex.Replace(s, "$1 (<$2>)");
            },
            (s) => {
                Regex regex = new Regex(@"<!--\s*DISC-ONLY\s*(.*?)\s*-->");
                return regex.Replace(s, "$1");
            },
        };

        public string Convert(string markdown)
        {
            StringBuilder sb = new StringBuilder();
            markdown.ReplaceLineEndings().Split(Environment.NewLine).ToList().ForEach(line =>
            {
                if (String.IsNullOrEmpty(line))
                {
                    sb.AppendLine();
                }
                else
                {
                    Converters.ForEach(converter =>
                    {
                        line = converter(line);
                    });

                    if (!String.IsNullOrEmpty(line))
                    {
                        sb.AppendLine(line);
                    }
                }
            });

            return sb.ToString();
        }
    }
}
