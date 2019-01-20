
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
/*
#region ================== Namespaces

using System;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	internal sealed class ColorSetting : IEquatable<ColorSetting>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private string name;
		private PixelColor color;

		#endregion

		#region ================== Properties

		public Color Color { get { return Color.FromArgb(color.ToInt()); } set { color = PixelColor.FromColor(value); } }
		public PixelColor PixelColor { get { return color; } set { color = value; } }
		public string Name { get { return name; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public ColorSetting(string name, PixelColor color)
		{
			// Initialize
			this.name = name;
			this.color = color;
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This returns a PixelColor with adjusted alpha
		public PixelColor WithAlpha(byte a)
		{
			return new PixelColor(color, a);
		}
		
		// Equal?
		public bool Equals(ColorSetting other)
		{
			return this.name == other.name;
		}

		// To PixelColor
		public static implicit operator PixelColor(ColorSetting c)
		{
			return c.color;
		}

		// To Color
		public static implicit operator Color(ColorSetting c)
		{
			return Color.FromArgb(c.color.ToInt());
		}
		
		#endregion
	}
}
*/