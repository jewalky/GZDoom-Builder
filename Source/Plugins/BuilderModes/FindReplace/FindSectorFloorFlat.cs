#region ================== Namespaces

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Sector Flat (Floor)", BrowseButton = true)]
	internal class FindSectorFloorFlat : FindSectorFlat
	{
		#region ================== Methods

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
				// Floor flat matches?
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
