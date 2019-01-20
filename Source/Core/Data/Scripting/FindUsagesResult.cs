using System.Text.RegularExpressions;

namespace CodeImp.DoomBuilder.Data.Scripting
{
	public class FindUsagesResult
	{
		private ScriptResource source;
		private string line;
		private int lineindex;
		private int matchstart;
		private int matchend;

		public ScriptResource Resource { get { return source; } }
		public string Line { get { return line; } }
		public int LineIndex { get { return lineindex; } }
		public int MatchStart { get { return matchstart; } }
		public int MatchEnd { get { return matchend; } }

		private FindUsagesResult() { }
		public FindUsagesResult(ScriptResource source, Match match, string line, int lineindex)
		{
			this.source = source;
			this.line = line;
			this.lineindex = lineindex;
			this.matchstart = match.Index;
			this.matchend = match.Index + match.Length;
		}
	}
}
