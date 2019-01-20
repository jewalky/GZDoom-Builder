
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
using System.Collections.ObjectModel;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Sector Flat (Any)", BrowseButton = true)]
	internal class FindSectorFlat : BaseFindSector
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		public override Image BrowseImage { get { return Properties.Resources.List_Images; } }
		public override string UsageHint { get { return "Supported wildcards:" + Environment.NewLine
					+ "* - zero or more characters" + Environment.NewLine
					+ "? - one character"; } }
		
		#endregion

		#region ================== Constructor / Destructor

		#endregion

		#region ================== Methods

		// This is called when the browse button is pressed
		public override string Browse(string initialvalue)
		{
			return General.Interface.BrowseFlat(BuilderPlug.Me.FindReplaceForm, initialvalue);
		}


		// This is called to perform a search (and replace)
		// Returns a list of items to show in the results list
		// replacewith is null when not replacing
		public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
		{
			List<FindReplaceObject> objs = new List<FindReplaceObject>();

			// Interpret the replacement
			if(replace && (string.IsNullOrEmpty(replacewith) || replacewith.Length > General.Map.Config.MaxTextureNameLength)) 
			{
				MessageBox.Show("Invalid replace value for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return objs.ToArray();
			}
			
			// Interpret the find
			bool isregex = (value.IndexOf('*') != -1 || value.IndexOf('?') != -1); //mxd
			MatchingTextureSet set = new MatchingTextureSet(new Collection<string> { value.Trim() }); //mxd

			// Where to search?
			ICollection<Sector> list = withinselection ? General.Map.Map.GetSelectedSectors(true) : General.Map.Map.Sectors;

			// Go for all sectors
			foreach(Sector s in list)
			{
				// Flat matches?
				if(set.IsMatch(s.CeilTexture)) 
				{
					// Replace and add to list
					if(replace) s.SetCeilTexture(replacewith);
					objs.Add(new FindReplaceObject(s, "Sector " + s.Index + " (ceiling)" + (isregex ? " - " + s.CeilTexture : null)));
				}

				if(set.IsMatch(s.FloorTexture)) 
				{
					// Replace and add to list
					if(replace) s.SetFloorTexture(replacewith);
					objs.Add(new FindReplaceObject(s, "Sector " + s.Index + " (floor)" + (isregex ? " - " + s.FloorTexture : null)));
				}
			}
			
			// When replacing, make sure we keep track of used textures
			if(replace) 
			{
				General.Map.Data.UpdateUsedTextures();
				General.Map.Map.Update(); //mxd. And don't forget to update the view itself
				General.Map.IsChanged = true;
			}
			
			return objs.ToArray();
		}

		#endregion
	}
}
