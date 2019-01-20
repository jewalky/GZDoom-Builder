
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
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Compilers;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	internal class NodebuilderInfo : IComparable<NodebuilderInfo>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private readonly string name;
		private readonly string title;
		private readonly CompilerInfo compiler;
		private readonly string parameters;
		private readonly bool specialoutputfile;
		
		#endregion

		#region ================== Properties

		public string Name { get { return name; } }
		public string Title { get { return title; } }
		public CompilerInfo Compiler { get { return compiler; } }
		public string Parameters { get { return parameters; } }
		public bool HasSpecialOutputFile { get { return specialoutputfile; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public NodebuilderInfo(string filename, string name, Configuration cfg)
		{
			General.WriteLogLine("Registered nodebuilder configuration \"" + name + "\" from \"" + filename + "\"");
			
			// Initialize
			this.name = name;
			this.compiler = null;
			this.title = cfg.ReadSetting("nodebuilders." + name + ".title", "<untitled configuration>");
			string compilername = cfg.ReadSetting("nodebuilders." + name + ".compiler", "");
			this.parameters = cfg.ReadSetting("nodebuilders." + name + ".parameters", "");
			
			// Check for special output filename
			this.specialoutputfile = this.parameters.Contains("%FO");
			
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

		// Constructor for "none" nodebuilder
		public NodebuilderInfo()
		{
			// Initialize
			this.name = "";
			this.compiler = null;
			this.title = "[do not build nodes]";
			this.parameters = "";
			this.specialoutputfile = false;
		}
		
		#endregion

		#region ================== Methods

		// This compares it to other ConfigurationInfo objects
		public int CompareTo(NodebuilderInfo other)
		{
			// Compare
			return String.Compare(name, other.name, StringComparison.Ordinal);
		}
		
		// String representation
		public override string ToString()
		{
			return title;
		}
		
		// This runs the nodebuilder
		public Compiler CreateCompiler()
		{
			return compiler.Create();
		}
		
		#endregion
	}
}
