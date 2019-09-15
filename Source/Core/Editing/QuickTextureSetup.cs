
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
using System.Collections.Generic;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Data;
using System.Collections;
using System.Linq;

#endregion

namespace CodeImp.DoomBuilder.Editing
{
	public class QuickTextureSetup : IDisposable
	{

		#region ================== Variables

		// List that represents the palette for quick texture access
		private IDictionary<int, TextureOrFlatName> quicktextures = new Dictionary<int, TextureOrFlatName>();

		// These are texture names considered blank.
		private readonly string[] blankTextureNames = new string[] { null, "-", "" };

		// Disposing
		private bool isdisposed;

		#endregion

		#region ================== Properties

		public IDictionary<int, TextureOrFlatName> QuickTextures { get { return quicktextures; } set { quicktextures = value; } }
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal QuickTextureSetup()
		{
			// Initialize

			// Register actions
			General.Actions.BindMethods(this);
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public void Dispose()
		{
			if(!isdisposed)
			{
				// Unregister actions
				General.Actions.UnbindMethods(this);

				// Done
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Methods

		public TextureOrFlatName GetQuickTexture(int slot)
		{
			if (IsSlotActive(slot))
			{
				return QuickTextures[slot];
			}
			return null;
		}

		public void UpdateQuickTexture(int slot, TextureOrFlatName textureOrFlat)
		{
			QuickTextures[slot] = textureOrFlat;
		}

		public bool IsSlotActive(int slot)
		{
			if (QuickTextures.ContainsKey(slot) && !blankTextureNames.Contains(QuickTextures[slot].LumpName))
			{
				return true;
			}
			return false;
		}

		internal void WriteToConfig(Configuration cfg, string path)
		{
			for (int slot = 0; slot < 10; slot++)
			{
				if (QuickTextures.ContainsKey(slot))
				{
					cfg.WriteSetting(path + ".slot" + slot + ".name", QuickTextures[slot].LumpName);
					cfg.WriteSetting(path + ".slot" + slot + ".isflat", QuickTextures[slot].IsFlat);
				}
			}
		}

		// Read settings from configuration
		internal void ReadFromConfig(Configuration cfg, string path)
		{
			string name = "";
			bool isFlat = false;
			QuickTextures = new Dictionary<int, TextureOrFlatName>();
			for (int i = 0; i < 10; i++)
			{
				IDictionary textureOrFlatSetting = cfg.ReadSetting(path + ".slot" + i, new Hashtable());
				if (textureOrFlatSetting.Contains("name") && textureOrFlatSetting.Contains("isflat"))
				{
					name = (string)textureOrFlatSetting["name"];
					isFlat = (bool)textureOrFlatSetting["isflat"];
					QuickTextures[i] = new TextureOrFlatName(name, isFlat);
				}
			}
		}
		#endregion

		#region ================== Actions

		#endregion
	}
}
