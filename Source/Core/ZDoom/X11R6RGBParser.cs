#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.ZDoom
{
	internal sealed class X11R6RGBParser : ZDTextParser
	{
		#region ================== Variables

		private readonly Dictionary<string, PixelColor> knowncolors;

		#endregion

		#region ================== Properties

		internal override ScriptType ScriptType { get { return ScriptType.X11R6RGB; } }

		public Dictionary<string, PixelColor> KnownColors { get { return knowncolors; } }

		#endregion

		#region ================== Constructor

		internal X11R6RGBParser()
		{
			knowncolors = new Dictionary<string, PixelColor>(StringComparer.InvariantCultureIgnoreCase);
		}

		#endregion

		#region ================== Parsing

		public override bool Parse(TextResourceData data, bool clearerrors)
		{
			// Already parsed?
			if(!base.AddTextResource(data))
			{
				if(clearerrors) ClearError();
				return true;
			}

			// Cannot process?
			if(!base.Parse(data, clearerrors)) return false;

			// Continue until at the end of the stream
			char[] space = {' ', '\t'};
			while(SkipWhitespace(true))
			{
				string line = ReadLine();
				if(string.IsNullOrEmpty(line) || line.StartsWith("!")) continue; // Skip comments

				// "R G B Name with spaces"
				string[] parts = line.Split(space, StringSplitOptions.RemoveEmptyEntries);
				
				if(parts.Length < 4)
				{
					ReportError("Incorrect X11R6RGB color assignment");
					return false;
				}

				// Parse colors
				byte r = 0, g = 0, b = 0;
				if(!ReadByte(parts[0], ref r)) { ReportError("Expected red color value in [0 .. 255] range");   return false; }
				if(!ReadByte(parts[1], ref g)) { ReportError("Expected green color value in [0 .. 255] range"); return false; }
				if(!ReadByte(parts[2], ref b)) { ReportError("Expected blue color value in [0 .. 255] range");  return false; }

				// Assemble name
				string colorname = string.Join("", parts, 3, parts.Length - 3);
                colorname = colorname.ToLowerInvariant(); // [ZZ] just to make sure, even though it's OrdinalIgnoreCase

				// Add to collection
				knowncolors[colorname] = new PixelColor(255, r, g, b);
			}

			return true;
		}

		#endregion
	}
}
