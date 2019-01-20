
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
	public sealed class LinedefTracePath : List<Linedef>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public LinedefTracePath()
		{
			// Initialize
		}

		// Constructor
		public LinedefTracePath(IEnumerable<Linedef> lines) : base(lines)
		{
			// Initialize
		}

		// Constructor
		public LinedefTracePath(ICollection<LinedefSide> lines) : base(lines.Count)
		{
			// Initialize
			foreach(LinedefSide ls in lines) base.Add(ls.Line);
		}

		// Constructor
		public LinedefTracePath(LinedefTracePath p, Linedef add) : base(p)
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
				return (base[0].Start == base[base.Count - 1].Start) ||
					   (base[0].Start == base[base.Count - 1].End) ||
					   (base[0].End == base[base.Count - 1].Start) ||
					   (base[0].End == base[base.Count - 1].End);
			}
			else
			{
				// Not closed
				return false;
			}
		}
		
		// This makes a polygon from the path
		public EarClipPolygon MakePolygon(bool startfront)
		{
			EarClipPolygon p = new EarClipPolygon();
			bool forward = startfront;
			
			// Any sides at all?
			if(base.Count > 0)
			{
				if(forward)
					p.AddLast(new EarClipVertex(base[0].Start.Position, base[0].Front));
				else
					p.AddLast(new EarClipVertex(base[0].End.Position, base[0].Back));
				
				// Add all lines, but the first
				for(int i = 1; i < base.Count; i++)
				{
					// Traverse direction changes?
					if((base[i - 1].Start == base[i].Start) ||
					   (base[i - 1].End == base[i].End))
						forward = !forward;

					// Add next vertex
					if(forward)
						p.AddLast(new EarClipVertex(base[i].Start.Position, base[i].Front));
					else
						p.AddLast(new EarClipVertex(base[i].End.Position, base[i].Back));
				}
			}
			
			return p;
		}

		#endregion
	}
}
