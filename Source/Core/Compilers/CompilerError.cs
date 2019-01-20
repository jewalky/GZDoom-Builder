
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

#endregion

namespace CodeImp.DoomBuilder.Compilers
{
	public struct CompilerError
	{
		// Constants
		public const int NO_LINE_NUMBER = -1;
		
		// Members
		public string description;
		public string filename;
		public int linenumber;
		
		// Constructor
		public CompilerError(string description)
		{
			this.description = description;
			this.filename = "";
			this.linenumber = NO_LINE_NUMBER;
		}
		
		// Constructor
		public CompilerError(string description, string filename)
		{
			this.description = description;
			this.filename = filename;
			this.linenumber = NO_LINE_NUMBER;
		}
		
		// Constructor
		public CompilerError(string description, string filename, int linenumber)
		{
			this.description = description;
			this.filename = filename;
			this.linenumber = linenumber;
		}

		//mxd
		public bool Equals(CompilerError other)
		{
			return description == other.description && filename == other.filename && linenumber == other.linenumber;
		}
	}
}

