
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
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	internal sealed class ColormapImage : ImageData
	{
		#region ================== Constructor / Disposer

		// Constructor
		public ColormapImage(string name)
		{
			// Initialize
			SetName(name);
			virtualname = "[Colormaps]/" + this.name; //mxd

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

			lock(this)
			{
				// Get the lump data stream
				Stream lumpdata = General.Map.Data.GetColormapData(Name);
				if(lumpdata != null)
				{
					// Copy lump data to memory
					lumpdata.Seek(0, SeekOrigin.Begin);
					byte[] membytes = new byte[(int)lumpdata.Length];
					lumpdata.Read(membytes, 0, (int)lumpdata.Length);
					MemoryStream mem = new MemoryStream(membytes);
					mem.Seek(0, SeekOrigin.Begin);

					// Get a reader for the data
					IImageReader reader = ImageDataFormat.GetImageReader(mem, ImageDataFormat.DOOMCOLORMAP, General.Map.Data.Palette);
					if(reader is UnknownImageReader)
					{
						// Data is in an unknown format!
						General.ErrorLogger.Add(ErrorType.Error, "Colormap lump \"" + Name + "\" data format could not be read. Does this lump contain valid colormap data at all?");
						bitmap = null;
					}
					else
					{
						// Read data as bitmap
						mem.Seek(0, SeekOrigin.Begin);
						if(bitmap != null) bitmap.Dispose();
						bitmap = reader.ReadAsBitmap(mem);
					}

					// Done
					mem.Dispose();

					if(bitmap != null)
					{
						// Get width and height from image and set the scale
						width = bitmap.Size.Width;
						height = bitmap.Size.Height;
						scale.x = General.Map.Config.DefaultFlatScale;
						scale.y = General.Map.Config.DefaultFlatScale;
					}
					else
					{
						loadfailed = true;
					}
				}
				else
				{
					// Missing a patch lump!
					General.ErrorLogger.Add(ErrorType.Error, "Missing colormap lump \"" + Name + "\". Did you forget to include required resources?");
					loadfailed = true;
				}

				// Pass on to base
				base.LocalLoadImage();
			}
		}

		#endregion
	}
}
