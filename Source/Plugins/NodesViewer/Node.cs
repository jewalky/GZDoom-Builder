#region === Copyright (c) 2010 Pascal van der Heiden ===

using System.Drawing;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.Plugins.NodesViewer
{
	public struct Node
	{
		public Vector2D linestart;
		public Vector2D linedelta;
		public RectangleF rightbox;
		public RectangleF leftbox;
		public int rightchild;
		public int leftchild;
		public bool rightsubsector;
		public bool leftsubsector;
		public int parent;
	}
}
