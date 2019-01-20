
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

using System;
using System.Drawing;
using SlimDX;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	public struct PixelColor
	{
		#region ================== Constants
		
		public const float BYTE_TO_FLOAT = 0.00392156862745098f;

		//mxd. Some color constants, full alpha
		public const int INT_BLACK = -16777216;
		public const int INT_WHITE = -1;

		//mxd. Some color constants, no alpha
		public const int INT_WHITE_NO_ALPHA = 16777215;
		
		#endregion

		#region ================== Statics

		public static readonly PixelColor Transparent = new PixelColor(0, 0, 0, 0);
		
		#endregion

		#region ================== Variables

		// Members
		public byte b;
		public byte g;
		public byte r;
		public byte a;

		#endregion

		#region ================== Constructors

		// Constructor
		public PixelColor(byte a, byte r, byte g, byte b)
		{
			// Initialize
			this.a = a;
			this.r = r;
			this.g = g;
			this.b = b;
		}

		// Constructor
		public PixelColor(PixelColor p, byte a)
		{
			// Initialize
			this.a = a;
			this.r = p.r;
			this.g = p.g;
			this.b = p.b;
		}

		#endregion

		#region ================== Methods
		
		// Construct from color
		public static PixelColor FromColor(Color c)
		{
			return new PixelColor(c.A, c.R, c.G, c.B);
		}
		
		// Construct from int
		public static PixelColor FromInt(int c)
		{
			return FromColor(Color.FromArgb(c));
		}
		
		// Return the inverse color
		public PixelColor Inverse()
		{
			return new PixelColor((byte)(255 - a), (byte)(255 - r), (byte)(255 - g), (byte)(255 - b));
		}
		
		// Return the inverse color, but keep same alpha
		public PixelColor InverseKeepAlpha()
		{
			return new PixelColor(a, (byte)(255 - r), (byte)(255 - g), (byte)(255 - b));
		}
		
		// To int
		public int ToInt()
		{
			return Color.FromArgb(a, r, g, b).ToArgb();
		}

		// To Color
		public Color ToColor()
		{
			return Color.FromArgb(a, r, g, b);
		}

		//To ColorRef (alpha-less). mxd. Changed function name to more descriptive one...
		public int ToInversedColorRef()
		{
			return (r + (b << 16) + (g << 8));
		}
		
		// To ColorValue
		public Color4 ToColorValue()
		{
			return new Color4(a * BYTE_TO_FLOAT,
							  r * BYTE_TO_FLOAT,
							  g * BYTE_TO_FLOAT,
							  b * BYTE_TO_FLOAT);
		}

		// To ColorValue
		public Color4 ToColorValue(float withalpha)
		{
			return new Color4(withalpha,
							  r * BYTE_TO_FLOAT,
							  g * BYTE_TO_FLOAT,
							  b * BYTE_TO_FLOAT);
		}
		
		// This returns a new PixelColor with adjusted alpha
		public PixelColor WithAlpha(byte a)
		{
			return new PixelColor(this, a);
		}
		
		// This blends two colors with respect to alpha
		public PixelColor Blend(PixelColor a, PixelColor b)
		{
			PixelColor c = new PixelColor();

			float ba = a.a * BYTE_TO_FLOAT;
			c.r = (byte)(a.r * (1f - ba) + b.r * ba);
			c.g = (byte)(a.g * (1f - ba) + b.g * ba);
			c.b = (byte)(a.b * (1f - ba) + b.b * ba);
			c.a = (byte)(a.a * (1f - ba) + ba);
			
			return c;
		}

		//mxd. This adds two colors
		public static PixelColor Add(PixelColor a, PixelColor b)
		{
			return new PixelColor
			{
				a = (byte)(Math.Min(a.a + b.a, 255)), 
				r = (byte)(Math.Min(a.r + b.r, 255)), 
				g = (byte)(Math.Min(a.g + b.g, 255)), 
				b = (byte)(Math.Min(a.b + b.b, 255))
			};
		}

		//mxd. This subtracts two colors
		public static PixelColor Subtract(PixelColor a, PixelColor b)
		{
			return new PixelColor
			{
				a = (byte)(Math.Max(a.a , b.a)), // Not sure about that...
				r = (byte)(Math.Max(a.r - b.r, 0)),
				g = (byte)(Math.Max(a.g - b.g, 0)),
				b = (byte)(Math.Max(a.b - b.b, 0))
			};
		}
		
		// This modulates two colors
		public static PixelColor Modulate(PixelColor a, PixelColor b)
		{
			float aa = a.a * BYTE_TO_FLOAT;
			float ar = a.r * BYTE_TO_FLOAT;
			float ag = a.g * BYTE_TO_FLOAT;
			float ab = a.b * BYTE_TO_FLOAT;
			float ba = b.a * BYTE_TO_FLOAT;
			float br = b.r * BYTE_TO_FLOAT;
			float bg = b.g * BYTE_TO_FLOAT;
			float bb = b.b * BYTE_TO_FLOAT;
			
			return new PixelColor
			{
				a = (byte)((aa * ba) * 255.0f), 
				r = (byte)((ar * br) * 255.0f), 
				g = (byte)((ag * bg) * 255.0f), 
				b = (byte)((ab * bb) * 255.0f)
			};
		}

		//mxd. Handy while debugging
		public override string ToString()
		{
			return "[A=" + a + ", R=" + r + ", G=" + g + ", B=" + b + "]";
		}

		//mxd
		public bool Equals(PixelColor other)
		{
			return (r == other.r && g == other.g && b == other.b && a == other.a);
		}
		
		#endregion
	}
}
