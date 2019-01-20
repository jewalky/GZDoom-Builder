
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CodeImp.DoomBuilder.IO;
using ScintillaNET;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	//mxd
	public enum ScriptType
	{
		UNKNOWN,
		ACS,
		MODELDEF,
		DECORATE,
		GLDEFS,
		SNDSEQ,
		MAPINFO,
		VOXELDEF,
		TEXTURES,
		ANIMDEFS,
		REVERBS,
		TERRAIN,
		X11R6RGB,
		CVARINFO,
		SNDINFO,
		LOCKDEFS,
		MENUDEF,
		SBARINFO,
		USDF,
		GAMEINFO,
		KEYCONF,
		FONTDEFS,
        ZSCRIPT,
	}
	
	internal class ScriptConfiguration : IComparable<ScriptConfiguration>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables
		
		// Compiler settings
		private readonly CompilerInfo compiler;
		private readonly string parameters;
		private readonly string resultlump;
		
		// Editor settings
		private readonly string description;
		private readonly int codepage;
		private readonly string extrawordchars; //mxd. Extra characters to be threated as part of a word by Scintilla
		private readonly string[] extensions;
		private readonly bool casesensitive;
		private readonly int insertcase;
		private readonly Lexer lexer;
		private readonly string keywordhelp;
		private readonly string functionopen;
		private readonly string functionclose;
		private readonly string codeblockopen; //mxd
		private readonly string codeblockclose; //mxd
		private readonly string arrayopen; //mxd
		private readonly string arrayclose; //mxd
		private readonly string argumentdelimiter;
		private readonly string terminator;
		private readonly ScriptType scripttype; //mxd
		
		// Collections
		private readonly Dictionary<string, string> keywords;
		private readonly Dictionary<string, string> lowerkeywords;
		private readonly List<string> keywordkeyssorted; //mxd
		private readonly List<string> constants;
		private readonly Dictionary<string, string> lowerconstants;
		private readonly List<string> properties; //mxd
		private readonly Dictionary<string, string> lowerproperties; //mxd
		private readonly Dictionary<string, string[]> snippets; //mxd
		private readonly HashSet<string> snippetkeyssorted; //mxd
		private readonly HashSet<char> braces; //mxd
		
		#endregion

		#region ================== Properties

		// Compiler settings
		public CompilerInfo Compiler { get { return compiler; } }
		public string Parameters { get { return parameters; } }
		public string ResultLump { get { return resultlump; } }
		
		// Editor settings
		public string Description { get { return description; } }
		public int CodePage { get { return codepage; } }
		public string[] Extensions { get { return extensions; } }
		public bool CaseSensitive { get { return casesensitive; } }
		public int InsertCase { get { return insertcase; } }
		public Lexer Lexer { get { return lexer; } }
		public string KeywordHelp { get { return keywordhelp; } }
		public string FunctionOpen { get { return functionopen; } }
		public string FunctionClose { get { return functionclose; } }
		public string CodeBlockOpen { get { return codeblockopen; } } //mxd
		public string CodeBlockClose { get { return codeblockclose; } } //mxd
		public string ArrayOpen { get { return arrayopen; } } //mxd
		public string ArrayClose { get { return arrayclose; } } //mxd
		public string ArgumentDelimiter { get { return argumentdelimiter; } }
		public string Terminator { get { return terminator; } }
		public string ExtraWordCharacters { get { return extrawordchars; } } //mxd
		public ScriptType ScriptType { get { return scripttype; } } //mxd

		// Collections
		public ICollection<string> Keywords { get { return keywordkeyssorted; } }
		public ICollection<string> Properties { get { return properties; } } //mxd
		public ICollection<string> Constants { get { return constants; } }
		public ICollection<string> Snippets { get { return snippetkeyssorted; } } //mxd
		public HashSet<char> BraceChars { get { return braces; } } //mxd
		
		#endregion

		#region ================== Constructor / Disposer
		
		// This creates the default script configuration
		// that is used for documents of unknown type
		internal ScriptConfiguration()
		{
			// Initialize
			this.keywords = new Dictionary<string, string>(StringComparer.Ordinal);
			this.constants = new List<string>();
			this.properties = new List<string>(); //mxd
			this.lowerkeywords = new Dictionary<string, string>(StringComparer.Ordinal);
			this.lowerconstants = new Dictionary<string, string>(StringComparer.Ordinal);
			this.lowerproperties = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
			this.keywordkeyssorted = new List<string>(); //mxd
			this.snippets = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase); //mxd
			this.snippetkeyssorted = new HashSet<string>(); //mxd
			this.braces = new HashSet<char>(); //mxd

			// Settings
			lexer = Lexer.Null;
			casesensitive = false;
			codepage = 65001;
			parameters = "";
			resultlump = "";
			insertcase = 0;
			keywordhelp = "";
			functionopen = "";
			functionclose = "";
			codeblockopen = ""; //mxd
			codeblockclose = ""; //mxd
			arrayopen = ""; //mxd
			arrayclose = ""; //mxd
			argumentdelimiter = "";
			terminator = "";
			description = "Plain text";
			scripttype = ScriptType.UNKNOWN; //mxd
			extrawordchars = ""; //mxd
			extensions = new[] { "txt" };
		}
		
		// Constructor
		internal ScriptConfiguration(Configuration cfg)
		{
			// Initialize
			this.keywords = new Dictionary<string, string>(StringComparer.Ordinal);
			this.constants = new List<string>();
			this.properties = new List<string>(); //mxd
			this.lowerkeywords = new Dictionary<string, string>(StringComparer.Ordinal);
			this.lowerconstants = new Dictionary<string, string>(StringComparer.Ordinal);
			this.lowerproperties = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
			this.keywordkeyssorted = new List<string>(); //mxd
			this.snippets = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase); //mxd
			this.snippetkeyssorted = new HashSet<string>(); //mxd
			this.braces = new HashSet<char>(); //mxd
			
			// Read settings
			description = cfg.ReadSetting("description", "Untitled script");
			codepage = cfg.ReadSetting("codepage", 0);
			string extensionsstring = cfg.ReadSetting("extensions", "");
			string compilername = cfg.ReadSetting("compiler", "");
			parameters = cfg.ReadSetting("parameters", "");
			resultlump = cfg.ReadSetting("resultlump", "");
			casesensitive = cfg.ReadSetting("casesensitive", true);
			insertcase = cfg.ReadSetting("insertcase", 0);
			lexer = (Lexer)cfg.ReadSetting("lexer", (int)Lexer.Container);
			keywordhelp = cfg.ReadSetting("keywordhelp", "");
			functionopen = cfg.ReadSetting("functionopen", "");
			functionclose = cfg.ReadSetting("functionclose", "");
			codeblockopen = cfg.ReadSetting("codeblockopen", ""); //mxd
			codeblockclose = cfg.ReadSetting("codeblockclose", ""); //mxd
			arrayopen = cfg.ReadSetting("arrayopen", ""); //mxd
			arrayclose = cfg.ReadSetting("arrayclose", ""); //mxd
			argumentdelimiter = cfg.ReadSetting("argumentdelimiter", "");
			terminator = cfg.ReadSetting("terminator", "");
			extrawordchars = cfg.ReadSetting("extrawordchars", ""); //mxd

            //mxd. Get script type...
			string scripttypestr = cfg.ReadSetting("scripttype", string.Empty);
			if(!string.IsNullOrEmpty(scripttypestr))
			{
				List<string> typenames = new List<string>(Enum.GetNames(typeof(ScriptType)));
				int pos = typenames.IndexOf(scripttypestr.ToUpperInvariant());
				if(pos == -1)
				{
					scripttype = ScriptType.UNKNOWN;
					General.ErrorLogger.Add(ErrorType.Warning, "Unknown script type \"" + scripttypestr.ToUpperInvariant() + "\" in \"" + description + "\" script configuration.");
				}
				else
				{
					scripttype = (ScriptType)pos;
				}
			}
			else
			{
				scripttype = ScriptType.UNKNOWN;
			}

            //mxd. Make braces array
            if (!string.IsNullOrEmpty(functionopen)) braces.Add(functionopen[0]);
			if(!string.IsNullOrEmpty(functionclose)) braces.Add(functionclose[0]);
			if(!string.IsNullOrEmpty(codeblockopen)) braces.Add(codeblockopen[0]);
			if(!string.IsNullOrEmpty(codeblockclose)) braces.Add(codeblockclose[0]);
			if(!string.IsNullOrEmpty(arrayopen)) braces.Add(arrayopen[0]);
			if(!string.IsNullOrEmpty(arrayclose)) braces.Add(arrayclose[0]);
			
			// Make extensions array
			extensions = extensionsstring.Split(',');
			for(int i = 0; i < extensions.Length; i++) extensions[i] = extensions[i].Trim();
			
			// Load keywords
			IDictionary dic = cfg.ReadSetting("keywords", new Hashtable());
			foreach(DictionaryEntry de in dic)
			{
				string keyword = de.Key.ToString();
				if(keywords.ContainsKey(keyword)) //mxd
				{
					General.ErrorLogger.Add(ErrorType.Warning, "Keyword \"" + keyword + "\" is double defined in \"" + description + "\" script configuration.");
					continue;
				}

				keywords[keyword] = de.Value.ToString();
				lowerkeywords[keyword.ToLowerInvariant()] = keyword;
				keywordkeyssorted.Add(keyword); //mxd
			}

			//mxd. Sort keywords lookup
			keywordkeyssorted.Sort();

			//mxd. Load properties
			dic = cfg.ReadSetting("properties", new Hashtable());
			foreach(DictionaryEntry de in dic)
			{
				string property = de.Key.ToString();
				if(lowerproperties.ContainsValue(property)) //mxd
				{
					General.ErrorLogger.Add(ErrorType.Warning, "Property \"" + property + "\" is double defined in \"" + description + "\" script configuration.");
					continue;
				}

				properties.Add(property);
				lowerproperties[property.ToLowerInvariant()] = property;
			}

			//mxd
			properties.Sort();
			
			// Load constants
			dic = cfg.ReadSetting("constants", new Hashtable());
			foreach(DictionaryEntry de in dic)
			{
				string constant = de.Key.ToString();
				if(lowerconstants.ContainsValue(constant)) //mxd
				{
					General.ErrorLogger.Add(ErrorType.Warning, "Constant \"" + constant + "\" is double defined in \"" + description + "\" script configuration.");
					continue;
				}

				constants.Add(constant);
				lowerconstants[constant.ToLowerInvariant()] = constant;
			}

			//mxd
			constants.Sort();

			//mxd. Load Snippets
			string snippetsdir = cfg.ReadSetting("snippetsdir", "");
			if(!string.IsNullOrEmpty(snippetsdir))
			{
				string snippetspath = Path.Combine(General.SnippetsPath, snippetsdir);
				if(Directory.Exists(snippetspath))
				{
					string[] files = Directory.GetFiles(snippetspath, "*.txt", SearchOption.TopDirectoryOnly);
					List<string> sortedkeys = new List<string>();

					foreach(string file in files)
					{
						string name = Path.GetFileNameWithoutExtension(file);
						if(string.IsNullOrEmpty(name))
						{
							General.ErrorLogger.Add(ErrorType.Warning, "Failed to load snippet \"" + file + "\" for \"" + description + "\" script configuration.");
						}
						else
						{
							if(name.Contains(" ")) name = name.Replace(' ', '_');
							string[] lines = File.ReadAllLines(file);
							if(lines.Length > 0)
							{
								snippets.Add(name, lines);
								sortedkeys.Add(name);
							}
							else
							{
								General.ErrorLogger.Add(ErrorType.Warning, "Failed to load snippet \"" + file + "\" for \"" + description + "\" script configuration: file is empty!");
							}
						}
					}

					//mxd. Sort snippets lookup
					sortedkeys.Sort();
					snippetkeyssorted = new HashSet<string>(sortedkeys, StringComparer.OrdinalIgnoreCase);
				}
			}
			
			// Compiler specified?
			if(compilername.Length > 0)
			{
				// Find compiler
				foreach(CompilerInfo c in General.Compilers)
				{
					// Compiler name matches?
					if(c.Name == compilername)
					{
						// Apply compiler
						this.compiler = c;
						break;
					}
				}
				
				// No compiler found?
				if(this.compiler == null) throw new Exception("Compiler \"" + compilername + "\" is not defined");
			}
		}
		
		#endregion

		#region ================== Methods
		
		// This returns the correct case for a keyword
		// Returns the same keyword as the input when it cannot be found
		public string GetKeywordCase(string keyword)
		{
			if(lowerkeywords.ContainsKey(keyword.ToLowerInvariant()))
				return lowerkeywords[keyword.ToLowerInvariant()];
			else
				return keyword;
		}

		// This returns the correct case for a constant
		// Returns the same constant as the input when it cannot be found
		public string GetConstantCase(string constant)
		{
			if(lowerconstants.ContainsKey(constant.ToLowerInvariant()))
				return lowerconstants[constant.ToLowerInvariant()];
			else
				return constant;
		}
		
		// This returns true when the given word is a keyword
		public bool IsKeyword(string keyword)
		{
			return lowerkeywords.ContainsKey(keyword.ToLowerInvariant());
		}

		// This returns true when the given word is a contant
		public bool IsConstant(string constant)
		{
			return lowerconstants.ContainsKey(constant.ToLowerInvariant());
		}
		
		// This returns the function definition for a keyword
		// Returns null when no function definition exists
		// NOTE: The keyword parameter is case-sensitive!
		public string GetFunctionDefinition(string keyword)
		{
			if(keywords.ContainsKey(keyword))
				return keywords[keyword];
			else
				return null;
		}
		
		//mxd
		public string[] GetSnippet(string name)
		{
			return (snippetkeyssorted.Contains(name) ? snippets[name] : null);
		}

		// This sorts by description
		public int CompareTo(ScriptConfiguration other)
		{
			return string.Compare(this.description, other.description, true);
		}

		//mxd
		public override string ToString() 
		{
			return description;
		}
		
		#endregion
	}
}
