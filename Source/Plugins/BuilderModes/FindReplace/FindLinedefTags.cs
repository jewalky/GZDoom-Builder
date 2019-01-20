
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
using System.Linq;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Linedef Tag", BrowseButton = false)]
	internal class FindLinedefTags : BaseFindLinedef
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Destructor

		#endregion

		#region ================== Methods

		// This is called to test if the item should be displayed
		public override bool DetermineVisiblity()
		{
			return General.Map.FormatInterface.HasLinedefTag;
		}

		// This is called to perform a search (and replace)
		// Returns a list of items to show in the results list
		// replacewith is null when not replacing
		public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
		{
			List<FindReplaceObject> objs = new List<FindReplaceObject>();

			// Interpret the replacement
			int replacetag = 0;
			if(replace)
			{
				// If it cannot be interpreted, set replacewith to null (not replacing at all)
				if(!int.TryParse(replacewith, out replacetag)) replacewith = null;
				if(replacetag < General.Map.FormatInterface.MinTag) replacewith = null;
				if(replacetag > General.Map.FormatInterface.MaxTag) replacewith = null;
				if(replacewith == null)
				{
					MessageBox.Show("Invalid replace value for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return objs.ToArray();
				}
			}

			// Interpret the number given
			int tag;
			if(int.TryParse(value, out tag))
			{
				// Where to search?
				ICollection<Linedef> list = withinselection ? General.Map.Map.GetSelectedLinedefs(true) : General.Map.Map.Linedefs;

				// Go for all linedefs
				foreach(Linedef l in list)
				{
					// Tag matches?
					int index = l.Tags.IndexOf(tag);
					if(index != -1)
					{
						// Replace
						if(replace)
						{
							//mxd. Make a copy of tags, otherwise BeforePropsChange will be triggered after tag changes
							List<int> tags = new List<int>(l.Tags);
							tags[index] = replacetag;
							l.Tags = tags.Distinct().ToList(); // We don't want duplicates
						}

						// Add to list
						LinedefActionInfo info = General.Map.Config.GetLinedefActionInfo(l.Action);
						if(!info.IsNull)
							objs.Add(new FindReplaceObject(l, "Linedef " + l.Index + " (" + info.Title + ")"));
						else
							objs.Add(new FindReplaceObject(l, "Linedef " + l.Index));
					}
				}
			}

			return objs.ToArray();
		}

		#endregion
	}
}
