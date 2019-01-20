#region === Copyright (c) 2010 Pascal van der Heiden ===

using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.Plugins.NodesViewer
{
	public struct Subsector
	{
		public int numsegs;
		public int firstseg;

		public Vector2D[] points;
		public FlatVertex[] vertices;
	}
}
