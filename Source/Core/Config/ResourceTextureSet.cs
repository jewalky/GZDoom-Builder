
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

using System.Collections.Generic;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	internal sealed class ResourceTextureSet : TextureSet, IFilledTextureSet
	{
		#region ================== Constants
		
		#endregion

		#region ================== Variables

		// Matching textures and flats
		private Dictionary<long, ImageData> textures;
		private Dictionary<long, ImageData> flats;
		private DataLocation location;

		#endregion

		#region ================== Properties
		
		public ICollection<ImageData> Textures { get { return textures.Values; } }
		public ICollection<ImageData> Flats { get { return flats.Values; } }
		public DataLocation Location { get { return location; } }
		
		#endregion

		#region ================== Constructor / Destructor

		// New texture set constructor
		public ResourceTextureSet(string name, DataLocation location)
		{
			this.name = name;
			this.location = location;
			this.textures = new Dictionary<long, ImageData>();
			this.flats = new Dictionary<long, ImageData>();
		}
		
		#endregion

		#region ================== Methods
		
		// Add a texture
		internal void AddTexture(ImageData image)
		{
			//mxd. Wad duplicates are checked by WadReader
			if(location.type != DataLocation.RESOURCE_WAD && textures.ContainsKey(image.LongName) && !image.HasPatchWithSameName)
			{
				if(image is CameraTextureImage)
					General.ErrorLogger.Add(ErrorType.Warning, "Texture \"" + image.Name + "\" is overridden by CameraTexture with the same name in resource \"" + this.Location.GetDisplayName() + "\".");
				else
					General.ErrorLogger.Add(ErrorType.Warning, "Texture \"" + image.Name + "\" is double defined in resource \"" + this.Location.GetDisplayName() + "\".");
			}
			textures[image.LongName] = image;
		}

		// Add a flat
		internal void AddFlat(ImageData image)
		{
			//mxd. Wad duplicates are checked by WadReader
			if(location.type != DataLocation.RESOURCE_WAD && flats.ContainsKey(image.LongName) && (!General.Map.Config.MixTexturesFlats || !image.HasPatchWithSameName))
			{
				if(image is CameraTextureImage)
					General.ErrorLogger.Add(ErrorType.Warning, "Flat \"" + image.Name + "\" is overridden by CameraTexture with the same name in resource \"" + this.Location.GetDisplayName() + "\".");
				else
					General.ErrorLogger.Add(ErrorType.Warning, "Flat \"" + image.Name + "\" is double defined in resource \"" + this.Location.GetDisplayName() + "\".");
			}
				
			flats[image.LongName] = image;
		}

		// Check if this set has a texture
		internal bool TextureExists(ImageData image)
		{
			return textures.ContainsKey(image.LongName);
		}

		// Check if this set has a flat
		internal bool FlatExists(ImageData image)
		{
			return flats.ContainsKey(image.LongName);
		}

		// Mix the textures and flats
		internal void MixTexturesAndFlats()
		{
			// Make a copy of the flats only
			Dictionary<long, ImageData> flatsonly = new Dictionary<long, ImageData>(flats);

			// Add textures to flats
			foreach(KeyValuePair<long, ImageData> t in textures) 
			{
				if(!flats.ContainsKey(t.Key)) flats.Add(t.Key, t.Value);
			}

			// Add flats to textures
			foreach(KeyValuePair<long, ImageData> f in flatsonly) 
			{
				if(!textures.ContainsKey(f.Key)) textures.Add(f.Key, f.Value);
			}
		}
		
		#endregion
	}
}
