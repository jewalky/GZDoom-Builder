
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
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Linedef Flags", BrowseButton = true)]
	internal class FindLinedefFlags : BaseFindLinedef
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		public override Image BrowseImage { get { return Properties.Resources.List; } }

		#endregion

		#region ================== Constructor / Destructor

		#endregion

		#region ================== Methods

		// This is called when the browse button is pressed
		public override string Browse(string initialvalue)
		{
			//mxd. Combine regular and activation flags
			Dictionary<string, string> flags = new Dictionary<string, string>(General.Map.Config.LinedefFlags);
			foreach(LinedefActivateInfo ai in General.Map.Config.LinedefActivates) flags.Add(ai.Key, ai.Title);

			return FlagsForm.ShowDialog(Form.ActiveForm, initialvalue, flags);
		}

		// This is called to perform a search (and replace)
		// Returns a list of items to show in the results list
		// replacewith is null when not replacing
		public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
		{
			List<FindReplaceObject> objs = new List<FindReplaceObject>();

			// Where to search?
			ICollection<Linedef> list = withinselection ? General.Map.Map.GetSelectedLinedefs(true) : General.Map.Map.Linedefs;

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

				if(General.Map.Config.LinedefFlags.ContainsKey(f)) findflagslist.Add(f, setflag);
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

					if(!General.Map.Config.LinedefFlags.ContainsKey(f))
					{
						MessageBox.Show("Invalid replace value \"" + f + "\" for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return objs.ToArray();
					}
					replaceflagslist.Add(f, setflag);
				}
			}

			// Go for all linedefs
			foreach(Linedef l in list)
			{
				bool match = true;

				// Parse the value string...
				foreach(KeyValuePair<string, bool> group in findflagslist)
				{
					// ...and check if the flag doesn't match
					if(group.Value != l.IsFlagSet(group.Key))
					{
						match = false;
						break;
					}
				}

				// Flags matches?
				if(match)
				{
					// Replace flags (mxd)
					if(replace) 
					{
						// Set new flags
						foreach(KeyValuePair<string, bool> group in replaceflagslist) l.SetFlag(group.Key, group.Value);
					}
					
					// Add to list
					LinedefActionInfo info = General.Map.Config.GetLinedefActionInfo(l.Action);
					if(!info.IsNull)
						objs.Add(new FindReplaceObject(l, "Linedef " + l.Index + " (" + info.Title + ")"));
					else
						objs.Add(new FindReplaceObject(l, "Linedef " + l.Index));
				}
			}

			return objs.ToArray();
		}

		#endregion
	}
}
