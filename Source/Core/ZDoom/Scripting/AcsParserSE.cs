#region ================== Namespaces

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.GZBuilder.Data;

#endregion

//mxd. ACS parser used to create ScriptItems for use in script editor's navigator
namespace CodeImp.DoomBuilder.ZDoom.Scripting
{
	internal sealed class AcsParserSE : ZDTextParser
	{
		#region ================== Event Delegates

		internal delegate bool IncludeDelegate(AcsParserSE parser, string includefile, IncludeType includetype);
		internal IncludeDelegate OnInclude;

		#endregion

		#region ================== Variables

		private readonly Dictionary<string, HashSet<string>> includes; // <either "SCRIPTS" or Source library name, <List of files it #includes>>
		private HashSet<string> includestoskip;
		private string libraryname;

		private readonly List<ScriptItem> namedscripts;
		private readonly List<ScriptItem> numberedscripts;
		private readonly List<ScriptItem> functions;

		#endregion

		#region ================== Properties

		internal override ScriptType ScriptType { get { return ScriptType.ACS; } }

		internal List<ScriptItem> NamedScripts { get { return namedscripts; } }
		internal List<ScriptItem> NumberedScripts { get { return numberedscripts; } }
		internal List<ScriptItem> Functions { get { return functions; } }
		internal bool IsLibrary { get { return !string.IsNullOrEmpty(libraryname); } }
		internal string LibraryName { get { return libraryname; } }

		internal bool AddArgumentsToScriptNames;
		internal bool IsMapScriptsLump;
		internal bool IgnoreErrors;

		#endregion

		#region ================== Enums

		internal enum IncludeType
		{
			NONE,
			INCLUDE,
			LIBRARY
		}

		#endregion

		#region ================== Constructor

		internal AcsParserSE() 
		{
			namedscripts = new List<ScriptItem>();
			numberedscripts = new List<ScriptItem>();
			functions = new List<ScriptItem>();
			includes = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			includestoskip = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			specialtokens += "(,)";
		}

		#endregion

		#region ================== Parsing

		public override bool Parse(TextResourceData data, bool clearerrors) { return Parse(data, new HashSet<string>(), false, IncludeType.NONE, clearerrors); }
		public bool Parse(TextResourceData data, bool processincludes, IncludeType includetype, bool clearerrors) { return Parse(data, includestoskip, processincludes, includetype, clearerrors); }
		public bool Parse(TextResourceData data, HashSet<string> configincludes, bool processincludes, IncludeType includetype, bool clearerrors)
		{
			string source = data.Filename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			//INFO: files included or imported inside a library are not visible to the code outside it 
			//and must be included/imported separately

			// Includes tracking. "Regular" includes go to "SCRIPTS" group, library includes are tracked per-library
			string includecategory = (processincludes && includetype == IncludeType.LIBRARY ? source : "SCRIPTS");
			if(!includes.ContainsKey(includecategory)) includes.Add(includecategory, new HashSet<string>(StringComparer.OrdinalIgnoreCase));

			includestoskip = configincludes;
			int bracelevel = 0;

			// Already parsed?
			if(!base.AddTextResource(data))
			{
				if(clearerrors) ClearError();
				return true;
			}

			// Cannot process?
			if(!base.Parse(data, clearerrors)) return false;

			// Keep local data
			Stream localstream = datastream;
			string localsourcename = sourcename;
			int localsourcelumpindex = sourcelumpindex;
			BinaryReader localreader = datareader;
			DataLocation locallocation = datalocation;
			string localincludecategory = includecategory;

			// Continue until at the end of the stream
			while(SkipWhitespace(true)) 
			{
				string token = ReadToken().ToLowerInvariant();
				if(string.IsNullOrEmpty(token)) continue;

				// Ignore inner scope stuff
				if(token == "{") { bracelevel++; continue; }
				if(token == "}") { bracelevel--; continue; }
				if(bracelevel > 0) continue;

				switch(token)
				{
					case "script":
					{
						SkipWhitespace(true);
						int startpos = (int)datastream.Position;
						token = ReadToken();

						// Is this a named script?
						if(token.IndexOf('"') != -1) 
						{
							startpos += 1;
							string scriptname = StripQuotes(token);

							// Try to parse argument names
							List<KeyValuePair<string, string>> args = ParseArgs();
							List<string> argnames = new List<string>();
							foreach(KeyValuePair<string, string> group in args) argnames.Add(group.Value);

							// Make full name
							if(AddArgumentsToScriptNames) scriptname += " " + GetArgumentNames(args);

							// Add to collection
							namedscripts.Add(new ScriptItem(scriptname, argnames, startpos, includetype != IncludeType.NONE));
						}
						// Should be numbered script
						else
						{ 
							int n;
							if(int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out n)) 
							{
								// Try to parse argument names
								List<KeyValuePair<string, string>> args = ParseArgs();
								
								// Now find the opening brace
								do 
								{
									if(!SkipWhitespace(true)) break;
									token = ReadToken();
								} while(!string.IsNullOrEmpty(token) && token != "{");

								token = ReadLine();
								string name = "";
								bracelevel = 1;

								if(!string.IsNullOrEmpty(token))
								{
									int commentstart = token.IndexOf("//", StringComparison.Ordinal);
									if(commentstart != -1) // Found comment
									{ 
										commentstart += 2;
										name = token.Substring(commentstart, token.Length - commentstart).Trim();
									}
								}

								bool customname = (name.Length > 0);
								name = (customname ? name + " [Script " + n + "]" : "Script " + n);
								
								List<string> argnames = new List<string>();
								foreach(KeyValuePair<string, string> group in args) argnames.Add(group.Value);

								// Make full name
								if(AddArgumentsToScriptNames) name += " " + GetArgumentNames(args);

								// Add to collection
								numberedscripts.Add(new ScriptItem(n, name, argnames, startpos, includetype != IncludeType.NONE, customname));
							}
						}
					}
					break;

					case "function":
					{
						SkipWhitespace(true);
						string funcname = ReadToken(); // Read return type
						SkipWhitespace(true);
						int startpos = (int)datastream.Position;
						funcname += " " + ReadToken(); // Read function name

						// Try to parse argument names
						List<KeyValuePair<string, string>> args = ParseArgs();
						List<string> argnames = new List<string>();
						foreach(KeyValuePair<string, string> group in args) argnames.Add(group.Value);

						// Make full name
						if(AddArgumentsToScriptNames) funcname += GetArgumentNames(args);

						// Add to collection
						functions.Add(new ScriptItem(funcname, argnames, startpos, includetype != IncludeType.NONE));
					}
					break;

					case "#library":
						if(IsMapScriptsLump)
						{
							if(!IgnoreErrors) ReportError("SCRIPTS lump can't be compiled as library.");
							return IgnoreErrors;
						}
						
						SkipWhitespace(true);
						string libname = ReadToken(false); // Don't skip newline

						if(!libname.StartsWith("\"") || !libname.EndsWith("\""))
						{
							if(!IgnoreErrors) ReportError("#library name should be quoted.");
							return IgnoreErrors;
						}

						libname = StripQuotes(libname);

						if(string.IsNullOrEmpty(libname))
						{
							if(!IgnoreErrors) ReportError("Expected library name.");
							return IgnoreErrors;
						}

						// Store only when the script compiling was executed for is library
						if(includetype == IncludeType.NONE)
						{
							libraryname = libname;
							includetype = IncludeType.LIBRARY;
						}

						break;

					default:
						if(processincludes && (token == "#include" || token == "#import")) 
						{
							//INFO: ZDoom ACC include paths can be absolute ("d:\stuff\coollib.acs"), relative ("../coollib.acs") 
							//and can use forward and backward slashes ("acs\map01/script.acs")
							//also include paths must be quoted
							//long filenames are supported
							
							SkipWhitespace(true);
							string includelump = ReadToken(false); // Don't skip newline

							if(!includelump.StartsWith("\"") || !includelump.EndsWith("\""))
							{
								if(!IgnoreErrors) ReportError(token + " filename should be quoted.");
								return IgnoreErrors;
							}

							includelump = StripQuotes(includelump);

							if(string.IsNullOrEmpty(includelump))
							{
								if(!IgnoreErrors) ReportError("Expected file name to " + token + ".");
								return IgnoreErrors;
							}

							includelump = includelump.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

							// Compiler files? Track them, but don't parse them
							if(includestoskip.Contains(includelump))
							{
								// These can also be included several times...
								if(includes[includecategory].Contains(includelump))
								{
									if(!IgnoreErrors) ReportError("Already parsed \"" + includelump + "\". Check your " + token + " directives.");
									return IgnoreErrors;
								}

								// Add to collection
								includes[includecategory].Add(includelump);
								continue;
							}

							// Convert to a path we can use
							string includelumppath = GetRootedPath(source, includelump);

							// Rooting succeeded?
							if(this.HasError || string.IsNullOrEmpty(includelumppath))
								return IgnoreErrors;

							// Already parsed?
							if(includes[includecategory].Contains(includelumppath))
							{
								if(!IgnoreErrors) ReportError("Already parsed \"" + includelump + "\". Check your " + token + " directives.");
								return IgnoreErrors;
							}

							// Add to collection
							includes[includecategory].Add(includelumppath);

							// Callback to parse this file
							if(OnInclude != null)
							{
								IsMapScriptsLump = false;
								if(!OnInclude(this, includelumppath, (token == "#import" ? IncludeType.LIBRARY : IncludeType.INCLUDE)))
									return IgnoreErrors; // Bail out on errors
							}

							// Bail out on error
							if(this.HasError) return IgnoreErrors;

							// Set our buffers back to continue parsing
							datastream = localstream;
							datareader = localreader;
							sourcename = localsourcename;
							sourcelumpindex = localsourcelumpindex;
							datalocation = locallocation;
							includecategory = localincludecategory;
						}
						break;
				}
			}
			return true;
		}

		#endregion

		#region ================== Methods

		internal HashSet<string> GetIncludes()
		{
			HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach(KeyValuePair<string, HashSet<string>> group in includes)
			{
				foreach(string include in group.Value) result.Add(include);
			}

			result.ExceptWith(includestoskip); // Remove compiler includes
			return result;
		} 

		private List<KeyValuePair<string, string>> ParseArgs() //type, name
		{
			List<KeyValuePair<string, string>> argnames = new List<KeyValuePair<string, string>>();
			SkipWhitespace(true);
			string token = ReadToken();
			
			// Should be ENTER/OPEN etc. script type
			if(token != "(")
			{
				argnames.Add(new KeyValuePair<string, string>(token.ToUpperInvariant(), string.Empty));
				return argnames;
			}

			while(SkipWhitespace(true))
			{
				string argtype = ReadToken(); // should be type
				if(IsSpecialToken(argtype)) break;
				if(argtype.ToUpperInvariant() == "VOID")
				{
					argnames.Add(new KeyValuePair<string, string>("void", string.Empty));
					break;
				}

				SkipWhitespace(true);
				token = ReadToken(); // should be arg name
				argnames.Add(new KeyValuePair<string, string>(argtype, token));

				SkipWhitespace(true);
				token = ReadToken(); // should be comma or ")"
				if(token != ",") break;
			}

			return argnames;
		}

		private static string GetArgumentNames(List<KeyValuePair<string, string>> args)
		{
			// Make full name
			if(args.Count > 0)
			{
				List<string> argdescs = new List<string>(args.Count);
				foreach(KeyValuePair<string, string> group in args)
					argdescs.Add((group.Key + " " + group.Value).TrimEnd());

				return "(" + string.Join(", ", argdescs.ToArray()) + ")";
			}

			return "(void)";
		}

		#endregion
	}
}