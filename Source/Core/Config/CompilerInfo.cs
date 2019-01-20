
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
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Compilers;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public sealed class CompilerInfo
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		private readonly string filename;
		private readonly string name;
		private readonly string programfile;
		private readonly string programinterface;
		private readonly string path;
		private readonly HashSet<string> files;
		
		#endregion
		
		#region ================== Properties

		public string FileName { get { return filename; } }
		public string Name { get { return name; } }
		public string Path { get { return path; } }
		public string ProgramFile { get { return programfile; } }
		public string ProgramInterface { get { return programinterface; } }
		public HashSet<string> Files { get { return files; } }
		
		#endregion
		
		#region ================== Constructor / Disposer
		
		// Constructor
		internal CompilerInfo(string filename, string name, string path, Configuration cfg)
		{
			General.WriteLogLine("Registered compiler configuration \"" + name + "\" from \"" + filename + "\"");
			
			// Initialize
			this.filename = filename;
			this.path = path;
			this.name = name;
			this.files = new HashSet<string>(StringComparer.OrdinalIgnoreCase); //mxd. List -> HashSet
			
			// Read program file and interface
			this.programfile = cfg.ReadSetting("compilers." + name + ".program", "");
			this.programinterface = cfg.ReadSetting("compilers." + name + ".interface", "");
			
			// Make list of files required
			IDictionary cfgfiles = cfg.ReadSetting("compilers." + name, new Hashtable());
			foreach(DictionaryEntry de in cfgfiles)
			{
				if(de.Key.ToString() != "interface" && de.Key.ToString() != "program")
				{
					//mxd
					string include = de.Value.ToString().Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
					if(files.Contains(include))
						General.ErrorLogger.Add(ErrorType.Warning, "Include file \"" + de.Value + "\" is double defined in \"" + name + "\" compiler configuration");
					else
						files.Add(include);
				}
			}
		}
		
		#endregion
		
		#region ================== Methods
		
		// This creates the actual compiler interface
		internal Compiler Create()
		{
			return Compiler.Create(this);
		}
		
		#endregion
	}
}
