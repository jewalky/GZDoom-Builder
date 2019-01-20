using System;
using System.Drawing;

namespace CodeImp.DoomBuilder.ColorPicker
{
	public static class ColorHandler 
	{
		// Handle conversions between RGB and HSV    
		// (and Color types, as well).
		public struct RGB 
		{
			// All values are between 0 and 255.
			public int Red;
			public int Green;
			public int Blue;

			public RGB(int R, int G, int B) 
			{
				Red = R;
				Green = G;
				Blue = B;
			}
		}

		public struct HSV 
		{
			// All values are between 0 and 255.
			public int Hue;
			public int Saturation;
			public int value;

			public HSV(int H, int S, int V) 
			{
				Hue = H;
				Saturation = S;
				value = V;
			}

			public override string ToString() 
			{
				return String.Format("({0}, {1}, {2})", Hue, Saturation, value);
			}
		}

		public static RGB HSVtoRGB(int H, int S, int V) 
		{
			// H, S, and V must all be between 0 and 255.
			return HSVtoRGB(new HSV(H, S, V));
		}

		public static Color HSVtoColor(HSV hsv) 
		{
			RGB RGB = HSVtoRGB(hsv);
			return Color.FromArgb(RGB.Red, RGB.Green, RGB.Blue);
		}

		public static Color HSVtoColor(int H, int S, int V) 
		{
			return HSVtoColor(new HSV(H, S, V));
		}

		public static Color RGBtoColor(RGB rgb) 
		{
			return Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
		}

		public static RGB HSVtoRGB(HSV HSV) 
		{
			// HSV contains values scaled as in the color wheel:
			// that is, all from 0 to 255. 

			// for( this code to work, HSV.Hue needs
			// to be scaled from 0 to 360 (it//s the angle of the selected
			// point within the circle). HSV.Saturation and HSV.value must be 
			// scaled to be between 0 and 1.

			float r = 0;
			float g = 0;
			float b = 0;

			// Scale Hue to be between 0 and 360. Saturation
			// and value scale to be between 0 and 1.
			float h = ((float)HSV.Hue / 255 * 360) % 360;
			float s = (float)HSV.Saturation / 255;
			float v = (float)HSV.value / 255;

			if(s == 0) 
			{
				// If s is 0, all colors are the same.
				// This is some flavor of gray.
				r = v;
				g = v;
				b = v;
			} 
			else 
			{
				// The color wheel consists of 6 sectors.
				// Figure out which sector you//re in.
				float sectorPos = h / 60;
				int sectorNumber = (int)(Math.Floor(sectorPos));

				// get the fractional part of the sector.
				// That is, how many degrees into the sector
				// are you?
				float fractionalSector = sectorPos - sectorNumber;

				// Calculate values for the three axes
				// of the color. 
				float p = v * (1 - s);
				float q = v * (1 - (s * fractionalSector));
				float t = v * (1 - (s * (1 - fractionalSector)));

				// Assign the fractional colors to r, g, and b
				// based on the sector the angle is in.
				switch(sectorNumber) 
				{
					case 0:
						r = v;
						g = t;
						b = p;
						break;

					case 1:
						r = q;
						g = v;
						b = p;
						break;

					case 2:
						r = p;
						g = v;
						b = t;
						break;

					case 3:
						r = p;
						g = q;
						b = v;
						break;

					case 4:
						r = t;
						g = p;
						b = v;
						break;

					case 5:
						r = v;
						g = p;
						b = q;
						break;
				}
			}
			// return an RGB structure, with values scaled
			// to be between 0 and 255.
			return new RGB((int)(r * 255), (int)(g * 255), (int)(b * 255));
		}

		public static HSV RGBtoHSV(RGB RGB) 
		{
			// In this function, R, G, and B values must be scaled 
			// to be between 0 and 1.
			// HSV.Hue will be a value between 0 and 360, and 
			// HSV.Saturation and value are between 0 and 1.
			// The code must scale these to be between 0 and 255 for
			// the purposes of this application.

			float r = (float)RGB.Red / 255;
			float g = (float)RGB.Green / 255;
			float b = (float)RGB.Blue / 255;

			float h, s, v;

			float min = Math.Min(Math.Min(r, g), b);
			float max = Math.Max(Math.Max(r, g), b);
			v = max;
			float delta = max - min;
			if(max == 0 || delta == 0) 
			{
				// R, G, and B must be 0, or all the same. In this case, S is 0, and H is undefined.
				// Using H = 0 is as good as any...
				s = 0;
				h = 0;
			} 
			else 
			{
				s = delta / max;
				if(r == max) 
				{
					// Between Yellow and Magenta
					h = (g - b) / delta;
				} 
				else if(g == max) 
				{
					// Between Cyan and Yellow
					h = 2 + (b - r) / delta;
				} 
				else 
				{
					// Between Magenta and Cyan
					h = 4 + (r - g) / delta;
				}

			}
			// Scale h to be between 0 and 360. 
			// This may require adding 360, if the value is negative.
			h *= 60;
			if(h < 0) h += 360;

			// Scale to the requirements of this application. All values are between 0 and 255.
			return new HSV((int)(h / 360 * 255), (int)(s * 255), (int)(v * 255));
		}
	}
}
