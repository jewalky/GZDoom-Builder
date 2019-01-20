
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
using System.Reflection;
using System.Drawing;
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public class ResourceImage : ImageData
	{
		#region ================== Variables

		// Image source
		private readonly Assembly assembly;
		private readonly string resourcename;

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public ResourceImage(string resourcename)
		{
			// Initialize
			this.assembly = Assembly.GetCallingAssembly();
			this.resourcename = resourcename;
			this.AllowUnload = false;
			SetName(resourcename);

			// Temporarily load resource from memory
			Stream bitmapdata = assembly.GetManifestResourceStream(resourcename);
			Bitmap bmp = (Bitmap)Image.FromStream(bitmapdata);

			// Get width and height from image
			width = bmp.Size.Width;
			height = bmp.Size.Height;
			scale.x = 1.0f;
			scale.y = 1.0f;
			
			// Done
			bmp.Dispose();
			bitmapdata.Dispose();

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This loads the image
		protected override void LocalLoadImage()
		{
			if(IsImageLoaded) return; //mxd. ResourceImages can't be unloaded, so no need to reload them.
			
			lock(this)
			{
				// No failure checking here. If anything fails here, it is not the user's fault,
				// because the resources this loads are in the assembly.
				
				// Get resource from memory
				Stream bitmapdata = assembly.GetManifestResourceStream(resourcename);
				if(bitmap != null) bitmap.Dispose();
				bitmap = (Bitmap)Image.FromStream(bitmapdata);
				bitmapdata.Dispose();
				
				// Pass on to base
				base.LocalLoadImage();
			}
		}

		//mxd
		public override Image GetPreview() 
		{
			return base.GetBitmap();
		}
		
		#endregion
	}
}
