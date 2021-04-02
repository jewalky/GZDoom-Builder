#region ================== Namespaces

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.ZDoom
{
	internal sealed class CvarInfoParser : ZDTextParser
	{
		#region ================== Variables

		private CvarsCollection cvars;

		#endregion

		#region ================== Properties

		internal override ScriptType ScriptType { get { return ScriptType.CVARINFO; } }

		public CvarsCollection Cvars { get { return cvars; } }

		#endregion

		#region ================== Constructor

		internal CvarInfoParser()
		{
			cvars = new CvarsCollection();
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
			HashSet<string> knowntypes = new HashSet<string> { "int", "float", "color", "bool", "string" };
			HashSet<string> flags = new HashSet<string> { "noarchive", "cheat", "latch" };
			while(SkipWhitespace(true))
			{
				string token = ReadToken().ToLowerInvariant();
				if(string.IsNullOrEmpty(token)) continue;

				//<scope> [noarchive] [cheat] [latch] <type> <name> [= <defaultvalue>];
				switch(token)
				{
					case "user":
					case "server":
					case "nosave":
						// read (skip) flags
						while(true)
						{
							string flagtoken;

							SkipWhitespace(true);
							flagtoken = ReadToken().ToLowerInvariant();

							if(!flags.Contains(flagtoken))
							{
								DataStream.Seek(-flagtoken.Length - 1, SeekOrigin.Current);
								break;
							}
						}

						// Type
						SkipWhitespace(true);
						string type = ReadToken().ToLowerInvariant();

						if(!knowntypes.Contains(type))
						{
							ReportError("Unknown cvar type");
							return false;
						}

						// Name
						SkipWhitespace(true);
						string name = ReadToken();

						if(string.IsNullOrEmpty(name))
						{
							ReportError("Expected cvar name");
							return false;
						}

						// Either "=" or ";"
						SkipWhitespace(true);
						token = ReadToken();

						switch(token)
						{
							case "=":
								SkipWhitespace(true);
								string value = ReadToken();

								if(string.IsNullOrEmpty(value))
								{
									ReportError("Expected \"" + name + "\" cvar value");
									return false;
								}

								// Add to collection
								if(!AddValue(name, type, value)) return false;

								// Next should be ";"
								if(!NextTokenIs(";")) return false;
								break;

							case ";":
								if(!AddValue(name, type, string.Empty)) return false;
								break;
						}

						break;

					default:
						ReportError("Unknown keyword");
						return false;
				}
			}

			return true;
		}

		private bool AddValue(string name, string type, string value)
		{
			switch(type)
			{
				case "int":
					int iv = 0;
					if(!string.IsNullOrEmpty(value) && !ReadSignedInt(value, ref iv))
					{
						ReportError("Cvar \"" + name + "\" has invalid integer value: \"" + value + "\"");
						return false;
					}
					if(!cvars.AddValue(name, iv))
					{
						ReportError("Cvar \"" + name + "\" is double defined");
						return false;
					}
					break;

				case "float":
					float fv = 0f;
					if(!string.IsNullOrEmpty(value) && !ReadSignedFloat(value, ref fv))
					{
						ReportError("Cvar \"" + name + "\" has invalid decimal value: \"" + value + "\"");
						return false;
					}
					if(!cvars.AddValue(name, fv))
					{
						ReportError("Cvar \"" + name + "\" is double defined");
						return false;
					}
					break;

				case "color":
					PixelColor cv = new PixelColor();
					if(!string.IsNullOrEmpty(value) && !GetColorFromString(value, out cv))
					{
						ReportError("Cvar \"" + name + "\" has invalid color value: \"" + value + "\"");
						return false;
					}
					if(!cvars.AddValue(name, cv))
					{
						ReportError("Cvar \"" + name + "\" is double defined");
						return false;
					}
					break;
				
				case "bool":
					bool bv = false;
					if(!string.IsNullOrEmpty(value))
					{
						string sv = value.ToLowerInvariant();
						if(sv != "true" && sv != "false")
						{
							ReportError("Cvar \"" + name + "\" has invalid boolean value: \"" + value + "\"");
							return false;
						}
						bv = (sv == "true");
					}
					if(!cvars.AddValue(name, bv))
					{
						ReportError("Cvar \"" + name + "\" is double defined");
						return false;
					}
					break;
				
				case "string":
					if(!cvars.AddValue(name, StripQuotes(value)))
					{
						ReportError("Cvar \"" + name + "\" is double defined");
						return false;
					}
					break;
			}

			return true;
		}

		#endregion
	}
}
