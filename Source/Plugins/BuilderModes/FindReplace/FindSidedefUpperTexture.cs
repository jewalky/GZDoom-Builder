#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Sidedef Texture (Upper)", BrowseButton = true)]
	internal class FindSidedefUpperTexture : BaseFindSidedef
	{
		#region ================== Properties

		public override Image BrowseImage { get { return Properties.Resources.List; } }
		public override string UsageHint { get { return "Supported wildcards:" + Environment.NewLine
					+ "* - zero or more characters" + Environment.NewLine
					+ "? - one character"; } }

		#endregion

		#region ================== Methods

		// This is called when the browse button is pressed
		public override string Browse(string initialvalue)
		{
			return General.Interface.BrowseTexture(BuilderPlug.Me.FindReplaceForm, initialvalue);
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
			ICollection<Sidedef> sidelist = withinselection ? General.Map.Map.GetSidedefsFromSelectedLinedefs(true) : General.Map.Map.Sidedefs;

			// Go for all sidedefs
			foreach(Sidedef sd in sidelist) 
			{
				if(set.IsMatch(sd.HighTexture) && (value != "-" || sd.HighRequired())) 
				{
					// Replace and add to list
					if(replace) sd.SetTextureHigh(replacewith);
					objs.Add(new FindReplaceObject(sd, "Sidedef " + sd.Index + " (" + (sd.IsFront ? "front" : "back") + ", high)" + (isregex ? " - " + sd.HighTexture : null)));
				}
			}
			
			// When replacing, make sure we keep track of used textures
			if(replace) General.Map.Data.UpdateUsedTextures();
			
			return objs.ToArray();
		}

		#endregion
	}
}
