
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

namespace CodeImp.DoomBuilder.Geometry
{
	public sealed class SidedefsTracePath : List<Sidedef>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public SidedefsTracePath()
		{
			// Initialize
		}

		// Constructor
		public SidedefsTracePath(SidedefsTracePath p, Sidedef add) : base(p)
		{
			// Initialize
			base.Add(add);
		}

		#endregion

		#region ================== Methods

		// This checks if the polygon is closed
		public bool CheckIsClosed()
		{
			// There must be at least 2 sidedefs
			if(base.Count > 1)
			{
				// The end sidedef must share a vertex with the first
				return (base[0].Line.Start == base[base.Count - 1].Line.Start) ||
					   (base[0].Line.Start == base[base.Count - 1].Line.End) ||
					   (base[0].Line.End == base[base.Count - 1].Line.Start) ||
					   (base[0].Line.End == base[base.Count - 1].Line.End);
			}
			else
			{
				// Not closed
				return false;
			}
		}
		
		// This makes a polygon from the path
		public EarClipPolygon MakePolygon()
		{
			EarClipPolygon p = new EarClipPolygon();
			
			// Any sides at all?
			if(base.Count > 0)
			{
				// Add all sides
				for(int i = 0; i < base.Count; i++)
				{
					// On front or back?
					if(base[i].IsFront)
						p.AddLast(new EarClipVertex(base[i].Line.End.Position, base[i]));
					else
						p.AddLast(new EarClipVertex(base[i].Line.Start.Position, base[i]));
				}
			}
			
			return p;
		}

		#endregion
	}
}
