#region === Copyright (c) 2010 Pascal van der Heiden ===

using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.Plugins.VisplaneExplorer
{
	internal class Palette
	{
		// Members
		private readonly int[] colors;

		// Properties
		public int[] Colors { get { return colors; } }

		// Constructor
		public Palette(Bitmap bmp)
		{
			// Initialize
			colors = new int[bmp.Size.Width];
			for(int x = 0; x < bmp.Size.Width; x++)
				colors[x] = bmp.GetPixel(x, 0).ToArgb();
		}

		// This overrides a color
		public void SetColor(int index, int color)
		{
			colors[index] = color;
		}
	}
}
