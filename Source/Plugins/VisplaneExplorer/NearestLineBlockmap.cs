#region === Copyright (c) 2010 Pascal van der Heiden ===

using System.Collections.Generic;
using System.Drawing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Plugins.VisplaneExplorer
{
	internal sealed class NearestLineBlockmap : BlockMap<BlockEntry>
	{
		// Constructor
		public NearestLineBlockmap(RectangleF range) : base(range)
		{
			List<Linedef> singlesided = new List<Linedef>(General.Map.Map.Linedefs.Count);
			foreach(Linedef ld in General.Map.Map.Linedefs)
				if(ld.Back == null) singlesided.Add(ld);
			AddLinedefsSet(singlesided);

			// Blocks that do not have any linedefs in them must get the nearest line in them!
			for(int x = 0; x < size.Width; x++)
			{
				for(int y = 0; y < size.Height; y++)
				{
					BlockEntry be = blockmap[x, y];
					if(be.Lines.Count == 0)
					{
						Vector2D bc = GetBlockCenter(new Point(x, y));
						Linedef ld = MapSet.NearestLinedef(singlesided, bc);
						be.Lines.Add(ld);
					}
				}
			}
		}
	}
}
