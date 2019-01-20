
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

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.VisualModes
{
	public sealed class VisualBlockEntry
	{
		#region ================== Variables
		
		// Members
		private List<Linedef> lines;
		private List<Thing> things;
		private List<Sector> sectors;
		
		#endregion
		
		#region ================== Properties
		
		public List<Linedef> Lines { get { return lines; } }
		public List<Thing> Things { get { return things; } }
		public List<Sector> Sectors { get { return sectors; } }
		
		#endregion
		
		#region ================== Constructor
		
		// Constructor for empty block
		internal VisualBlockEntry()
		{
			lines = new List<Linedef>(2);
			things = new List<Thing>(2);
			sectors = new List<Sector>(2);
		}
		
		#endregion
	}
}
