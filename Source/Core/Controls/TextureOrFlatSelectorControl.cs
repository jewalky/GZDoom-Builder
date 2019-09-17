
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
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public class TextureOrFlatSelectorControl : ImageSelectorControl
	{
		bool isFlat;

		public bool IsFlat { get { return isFlat; } set { isFlat = value; } }

		// Setup
		public override void Initialize()
		{
			base.Initialize();
			
			// Fill autocomplete list
			name.AutoCompleteCustomSource.AddRange(General.Map.Data.TextureNames.ToArray());
			name.AutoCompleteCustomSource.AddRange(General.Map.Data.FlatNames.ToArray());
		}
		
		// This finds the image we need for the given texture name
		protected override Image FindImage(string imagename)
		{
			timer.Stop(); 
			
			// Check if name is a "none" texture
			if(string.IsNullOrEmpty(imagename)) 
			{
				DisplayImageSize(0, 0); 
				UpdateToggleImageNameButton(null); 
				return multipletextures ? Properties.Resources.ImageStack : null;
			} 
			else if(imagename == "-") 
			{
				DisplayImageSize(0, 0);
				UpdateToggleImageNameButton(null); 
				return null;
			}
			else
			{
				ImageData texture = General.Map.Data.GetTextureImage(imagename); 
				UpdateToggleImageNameButton(texture); 

				if(string.IsNullOrEmpty(texture.FilePathName) || texture is UnknownImage) DisplayImageSize(0, 0);
				else DisplayImageSize(texture.ScaledWidth, texture.ScaledHeight);

				if(!texture.IsPreviewLoaded) timer.Start();

				// Set the image
				return texture.GetPreview();
			}
		}

		// This gets ImageData by name...
		protected override ImageData GetImageData(string imagename)
		{
			return General.Map.Data.GetTextureImage(imagename);
		}

		// This browses for a texture
		protected override string BrowseImage(string imagename) 
		{
			// Browse for texture
			string result = TextureBrowserForm.Browse(this.ParentForm, imagename, true);
			this.IsFlat = IsNameForFlat(result);
			return result ?? imagename;
		}

		private bool IsNameForFlat(string name)
		{
			//Since the texture browser just gives us a name back, we need to check whether
			//we have a flat or texture. We could overhaul the texture browser to tell us
			//which is which instead...

			//Guess a flat first, If we don't have it, assume that it's a texture.
			if (General.Map.Data.FlatNames.Contains(name))
			{
				return true;
			}
			return false;
		}
	}
}
