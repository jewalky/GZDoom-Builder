
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
using System.Drawing.Imaging;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	internal sealed unsafe class ColorImage : ImageData
	{
		#region ================== Variables

		private PixelColor color;

		#endregion
		
		#region ================== Constructor / Disposer

		// Constructor
		public ColorImage(PixelColor color, int width, int height)
		{
			// Initialize
			this.width = width;
			this.height = height;
			this.color = color;
			SetName(color.ToColorValue().ToString());

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This loads the image
		protected override void LocalLoadImage()
		{
			// Leave when already loaded
			if(this.IsImageLoaded) return;
			if((width == 0) || (height == 0)) return;

			lock(this)
			{
				// Create bitmap
				try
				{
					if(bitmap != null) bitmap.Dispose();
					bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
					BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
					PixelColor* pixels = (PixelColor*)bitmapdata.Scan0.ToPointer();
					for(int i = 0; i < (width * height); i++)
					{
						*pixels = color;
						pixels++;
					}
					bitmap.UnlockBits(bitmapdata);
				}
				catch(Exception e)
				{
					// Unable to make bitmap
					General.ErrorLogger.Add(ErrorType.Error, "Unable to create color image '" + this.Name + "'. " + e.GetType().Name + ": " + e.Message);
					loadfailed = true;
				}

				// Dispose bitmap if load failed
				if(loadfailed && (bitmap != null))
				{
					bitmap.Dispose();
					bitmap = null;
				}
				
				// Pass on to base
				base.LocalLoadImage();
			}
		}

		#endregion
	}
}
*/