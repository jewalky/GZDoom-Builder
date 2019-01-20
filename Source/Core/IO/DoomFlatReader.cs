
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
using System.IO;
using System.Drawing;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Rendering;
using System.Drawing.Imaging;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	internal unsafe class DoomFlatReader : IImageReader
	{
		#region ================== Variables

		// Palette to use
		private readonly Playpal palette;

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public DoomFlatReader(Playpal palette)
		{
			// Initialize
			this.palette = palette;

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This validates the data as doom flat
		public bool Validate(Stream stream)
		{
			// Check if the flat is square
			float sqrlength = (float)Math.Sqrt(stream.Length);
			if(sqrlength == (float)Math.Truncate(sqrlength))
			{
				// Success when not 0
				return ((int)sqrlength > 0);
			}
			// Valid if the data is more than 4096
			return stream.Length > 4096;
		}

		// This creates a Bitmap from the given data
		// Returns null on failure
		public Bitmap ReadAsBitmap(Stream stream, out int offsetx, out int offsety)
		{
			offsetx = int.MinValue;
			offsety = int.MinValue;
			return ReadAsBitmap(stream);
		}
		
		// This creates a Bitmap from the given data
		// Returns null on failure
		public Bitmap ReadAsBitmap(Stream stream)
		{
			int width, height;
			Bitmap bmp;

			// Read pixel data
			PixelColor[] pixeldata = ReadAsPixelData(stream, out width, out height);
			if(pixeldata != null)
			{
				try
				{
					// Create bitmap and lock pixels
					bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
					BitmapData bitmapdata = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
					PixelColor* targetdata = (PixelColor*)bitmapdata.Scan0.ToPointer();

					//mxd. Copy the pixels
					int size = pixeldata.Length - 1;
					for(PixelColor* cp = targetdata + size; cp >= targetdata; cp--)
						*cp = pixeldata[size--];

					// Done
					bmp.UnlockBits(bitmapdata);
				}
				catch(Exception e)
				{
					// Unable to make bitmap
					General.ErrorLogger.Add(ErrorType.Error, "Unable to make Doom flat data. " + e.GetType().Name + ": " + e.Message);
					return null;
				}
			}
			else
			{
				// Failed loading picture
				bmp = null;
			}

			// Return result
			return bmp;
		}

		// This draws the picture to the given pixel color data
		// Throws exception on failure
		public void DrawToPixelData(Stream stream, PixelColor* target, int targetwidth, int targetheight, int x, int y)
		{
			// Get bitmap
			Bitmap bmp = ReadAsBitmap(stream);
			int width = bmp.Size.Width;
			int height = bmp.Size.Height;

			// Lock bitmap pixels
			BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			PixelColor* pixels = (PixelColor*)bmpdata.Scan0.ToPointer();

			// Go for all pixels in the original image
			for(int ox = 0; ox < width; ox++)
			{
				for(int oy = 0; oy < height; oy++)
				{
					// Copy this pixel?
					if(pixels[oy * width + ox].a > 0.5f)
					{
						// Calculate target pixel and copy when within bounds
						int tx = x + ox;
						int ty = y + oy;
						if((tx >= 0) && (tx < targetwidth) && (ty >= 0) && (ty < targetheight))
							target[ty * targetwidth + tx] = pixels[oy * width + ox];
					}
				}
			}

			// Done
			bmp.UnlockBits(bmpdata);
			bmp.Dispose();
		}

		// This creates pixel color data from the given data
		// Returns null on failure
		private PixelColor[] ReadAsPixelData(Stream stream, out int width, out int height)
		{
			// Check if the flat is square
			float sqrlength = (float)Math.Sqrt(stream.Length);
			if(sqrlength == (float)Math.Truncate(sqrlength))
			{
				// Calculate image size
				width = (int)sqrlength;
				height = (int)sqrlength;
			}
			// Check if the data is more than 4096
			else if(stream.Length > 4096)
			{
				// Image will be 64x64
				width = 64;
				height = 64;
			}
			else
			{
				// Invalid
				width = 0;
				height = 0;
				return null;
			}

			#if !DEBUG
			try
			{
			#endif
			
			// Valid width and height?
			if((width <= 0) || (height <= 0)) return null;

			// Allocate memory
			PixelColor[] pixeldata = new PixelColor[width * height];

			// Read flat bytes from stream
			byte[] bytes = new byte[width * height];
			stream.Read(bytes, 0, width * height);

			// Convert bytes with palette
			for(uint i = 0; i < width * height; i++) pixeldata[i] = palette[bytes[i]];

			// Return pointer
			return pixeldata;
			
			#if !DEBUG
			}
			catch(Exception)
			{
				// Return nothing
				return null;
			}
			#endif
		}
		
		#endregion

	}
}
