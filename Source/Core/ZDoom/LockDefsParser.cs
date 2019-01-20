using System.Collections.Generic;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Rendering;

namespace CodeImp.DoomBuilder.ZDoom
{
	internal sealed class LockDefsParser :ZDTextParser
	{
		internal override ScriptType ScriptType { get { return ScriptType.LOCKDEFS; } }

		private Dictionary<int, string> locks;
		private Dictionary<int, PixelColor> mapcolors;

		public Dictionary<int, PixelColor> MapColors { get { return mapcolors; } }

		public LockDefsParser()
		{
			locks = new Dictionary<int, string>();
			mapcolors = new Dictionary<int, PixelColor>();
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
			int locknum = -1;
			string locktitle = string.Empty;
			string game = string.Empty;
			int bracelevel = 0;
			long lockstartpos = -1;
			PixelColor mapcolor = new PixelColor();

			while(SkipWhitespace(true))
			{
				string token = ReadToken().ToLowerInvariant();
				if(string.IsNullOrEmpty(token)) continue;

				switch(token)
				{
					case "clearlocks":
						if(bracelevel == 0)
						{
							locks.Clear();
						}
						else
						{
							ReportError("Unexpected \"CLEARLOCKS\" keyword");
							return false;
						}
						break;

					// LOCK locknumber [game]
					case "lock":
						SkipWhitespace(false);
						if(!ReadSignedInt(ref locknum))
						{
							ReportError("Expected lock number");
							return false;
						}

						//wiki: The locknumber must be in the 1-to-255 range
						if(locknum < 1 || locknum > 255)
						{
							ReportError("The locknumber must be in the [1 .. 255] range, but is " + locknum);
							return false;
						}

						// Store position
						lockstartpos = datastream.Position;

						SkipWhitespace(true);
						token = ReadToken().ToLowerInvariant();
						if(!string.IsNullOrEmpty(token))
						{
							if(token == "{")
							{
								bracelevel++;
							}
							//Should be game
							else if(!GameType.GameTypes.Contains(token))
							{
								LogWarning("Lock " + locknum + " is defined for unknown game \"" + token + "\"");
							}
							else
							{
								game = token;
							}
						}
						break;

					case "$title":
						SkipWhitespace(false);
						locktitle = StripQuotes(ReadToken(false));
						break;

					// Mapcolor r g b
					case "mapcolor":
						int r = 0;
						int g = 0;
						int b = 0;

						SkipWhitespace(false);
						if(!ReadSignedInt(ref r))
						{
							ReportError("Expected Mapcolor Red value");
							return false;
						}
						if(r < 0 || r > 255)
						{
							ReportError("Mapcolor Red value must be in [0 .. 255] range, but is " + r);
							return false;
						}

						SkipWhitespace(false);
						if(!ReadSignedInt(ref g))
						{
							ReportError("Expected Mapcolor Green value");
							return false;
						}
						if(g < 0 || g > 255)
						{
							ReportError("Mapcolor Green value must be in [0 .. 255] range, but is " + g);
							return false;
						}

						SkipWhitespace(false);
						if(!ReadSignedInt(ref b))
						{
							ReportError("Expected Mapcolor Blue value");
							return false;
						}
						if(b < 0 || b > 255)
						{
							ReportError("Mapcolor Blue value must be in [0 .. 255] range, but is " + b);
							return false;
						}

						mapcolor = new PixelColor(255, (byte)r, (byte)g, (byte)b);
						break;

					case "{":
						bracelevel++;
						break;

					case "}":
						if(--bracelevel > 0) continue;

						// Add to collection?
						if(locknum > 0 && (string.IsNullOrEmpty(game) || General.Map.Config.BaseGame == game))
						{
							// No custom title given?
							if(string.IsNullOrEmpty(locktitle))
							{
								locktitle = "Lock " + locknum;
							}

							// Lock already defined?
							if(locks.ContainsKey(locknum))
							{
								// Do some stream poition hacking to make the warning point to the correct line
								long curpos = datastream.Position;
								if(lockstartpos != -1) datastream.Position = lockstartpos;
								LogWarning("Lock " + locknum + " is double-defined as \"" + locks[locknum] + "\" and \"" + locktitle + "\"");
								datastream.Position = curpos;
							}

							// Add to collections
							locks[locknum] = locktitle;
							if(mapcolor.a == 255) mapcolors[locknum] = mapcolor;
						}

						// Reset values
						locknum = -1;
						locktitle = string.Empty;
						game = string.Empty;
						mapcolor = new PixelColor();
						lockstartpos = -1;
						break;
				}
			}


			return true;
		}

		public EnumList GetLockDefs()
		{
			EnumList result = new EnumList(locks.Count);

			foreach(KeyValuePair<int, string> group in locks)
				result.Add(new EnumItem(group.Key.ToString(), group.Value));

			return result;
		}
	}
}
