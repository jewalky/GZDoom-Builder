
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

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public class BitmapImage : ImageData
	{
		#region ================== Variables

		// Image source
		private Bitmap img;

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public BitmapImage(Bitmap img, string name)
		{
			// Initialize
			this.img = img;
			this.AllowUnload = false;
			SetName(name);

			// Get width and height from image
			width = img.Size.Width;
			height = img.Size.Height;
			scale.x = 1.0f;
			scale.y = 1.0f;

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This loads the image
		protected override void LocalLoadImage()
		{
			lock(this)
			{
				// No failure checking here. I anything fails here, it is not the user's fault,
				// because the resources this loads are in the assembly.

				// Get resource from memory
				bitmap = img;

				// Pass on to base
				base.LocalLoadImage();
			}
		}

		#endregion
	}
}
