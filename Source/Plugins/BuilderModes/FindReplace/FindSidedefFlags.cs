#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Sidedef Flags", BrowseButton = true)]
	internal class FindSidedefFlags : BaseFindSidedef
	{
		#region ================== Properties

		public override Image BrowseImage { get { return Properties.Resources.List; } }

		#endregion

		#region ================== Methods

		// This is called to test if the item should be displayed
		public override bool DetermineVisiblity() 
		{
			return General.Map.Config.SidedefFlags.Count > 0;
		}

		// This is called when the browse button is pressed
		public override string Browse(string initialvalue) 
		{
			return FlagsForm.ShowDialog(Form.ActiveForm, initialvalue, General.Map.Config.SidedefFlags);
		}

		// This is called to perform a search (and replace)
		// Returns a list of items to show in the results list
		// replacewith is null when not replacing
		public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection) 
		{
			List<FindReplaceObject> objs = new List<FindReplaceObject>();

			// Where to search?
			ICollection<Sidedef> list = withinselection ? General.Map.Map.GetSidedefsFromSelectedLinedefs(true) : General.Map.Map.Sidedefs;

			// Find what? (mxd)
			Dictionary<string, bool> findflagslist = new Dictionary<string, bool>();
			foreach(string flag in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) 
			{
				string f = flag.Trim();
				bool setflag = true;
				if(f.StartsWith("!"))
				{
					setflag = false;
					f = f.Substring(1, f.Length - 1);
				}

				if(General.Map.Config.SidedefFlags.ContainsKey(f)) findflagslist.Add(f, setflag);
			}
			if(findflagslist.Count == 0) 
			{
				MessageBox.Show("Invalid value for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return objs.ToArray();
			}

			// Replace with what? (mxd)
			Dictionary<string, bool> replaceflagslist = new Dictionary<string, bool>();
			if(replace) 
			{
				string[] replaceflags = replacewith.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach(string flag in replaceflags) 
				{
					string f = flag.Trim();
					bool setflag = true;
					if(f.StartsWith("!"))
					{
						setflag = false;
						f = f.Substring(1, f.Length - 1);
					}

					if(!General.Map.Config.SidedefFlags.ContainsKey(f))
					{
						MessageBox.Show("Invalid replace value \"" + f + "\" for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return objs.ToArray();
					}
					replaceflagslist.Add(f, setflag);
				}
			}

			// Go for all linedefs
			foreach(Sidedef sd in list) 
			{
				bool match = true;

				// Parse the value string...
				foreach(KeyValuePair<string, bool> group in findflagslist) 
				{
					// ...and check if the flag doesn't match
					if(group.Value != sd.IsFlagSet(group.Key))
					{
						match = false;
						break;
					}
				}

				// Flags matches?
				if(match) 
				{
					string side = sd.IsFront ? "front" : "back";
					
					// Replace flags (mxd)
					if(replace) 
					{
						// Set new flags
						foreach(KeyValuePair<string, bool> group in replaceflagslist) sd.SetFlag(group.Key, group.Value);
					}

					// Add to list
					objs.Add(new FindReplaceObject(sd, "Sidedef " + sd.Index + " (" + side + ")"));
				}
			}

			return objs.ToArray();
		}

		#endregion
	}
}
