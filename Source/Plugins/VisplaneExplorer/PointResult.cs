#region === Copyright (c) 2010 Pascal van der Heiden ===

#endregion

namespace CodeImp.DoomBuilder.Plugins.VisplaneExplorer
{
	internal enum PointResult : int
	{
		OK = 0,
		BadZ = -1,
		Void = -2,
		Overflow = -3,
	}
}
