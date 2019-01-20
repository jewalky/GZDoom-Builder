using System.Collections.Generic;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.GZBuilder.Data;

//mxd. Decorate parser used to create ScriptItems for use in script editor's navigator
//Should be able to parse actor definitions even from invalid DECORATE and should never fail parsing
namespace CodeImp.DoomBuilder.ZDoom.Scripting
{
	internal sealed class DecorateParserSE : ZDTextParser
	{
		internal override ScriptType ScriptType { get { return ScriptType.DECORATE; } }
		
		private readonly List<ScriptItem> actors;
		public List<ScriptItem> Actors { get { return actors; } }

		public DecorateParserSE() 
		{
			actors = new List<ScriptItem>();
		}

		public override bool Parse(TextResourceData data, bool clearerrors) 
		{
			//mxd. Already parsed?
			if(!base.AddTextResource(data))
			{
				if(clearerrors) ClearError();
				return true;
			}

			// Cannot process?
			if(!base.Parse(data, clearerrors)) return false;

			// Continue until at the end of the stream
			while(SkipWhitespace(true)) 
			{
                int startpos = (int)datastream.Position;
                string token = ReadToken();
				if(string.IsNullOrEmpty(token) || token.ToUpperInvariant() != "ACTOR") continue;

				SkipWhitespace(true);
				List<string> definition = new List<string>();
                definition.Add(token); // actor ... ..., not just class name

				do
				{
					token = ReadToken(false); // Don't skip newline
					if(string.IsNullOrEmpty(token) || token == "{" || token == "}") break;
					definition.Add(token);
				} while(SkipWhitespace(false)); // Don't skip newline

				string name = string.Join(" ", definition.ToArray());
				if(!string.IsNullOrEmpty(name)) actors.Add(new ScriptItem(name, startpos, false));
			}

			// Sort nodes
			actors.Sort(ScriptItem.SortByName);
			return true;
		}
	}
}
