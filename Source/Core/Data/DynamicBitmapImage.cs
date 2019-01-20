
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

using System.Drawing;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public class DynamicBitmapImage : BitmapImage, ID3DResource
	{
		#region ================== Variables

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public DynamicBitmapImage(Bitmap img, string name) : base(img, name)
		{
			// Initialize
			this.UseColorCorrection = false;
			this.dynamictexture = true;

			// This resource is volatile
			General.Map.Graphics.RegisterResource(this);
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				General.Map.Graphics.UnregisterResource(this);

				// Done
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		// Unload the resource because Direct3D says so
		public void UnloadResource()
		{
			ReleaseTexture();
		}

		// Reload the resource
		public void ReloadResource()
		{
			CreateTexture();
		}

		#endregion
	}
}
