
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
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.LinedefTag, "Linedef Tag", true)]
	internal class LinedefTagHandler : SectorTagHandler
	{
		#region ================== Setup (mxd)

		protected override EnumList CreateEnumList()
		{
			// Collect tags
			List<int> tags = new List<int>();
			HashSet<int> tagshash = new HashSet<int>();
			EnumList taglist = new EnumList();

			if(General.Map.Map != null)
			{
				foreach(Linedef l in General.Map.Map.Linedefs)
				{
					if(l.Tag == 0 || tagshash.IsSupersetOf(l.Tags)) continue;
					tags.AddRange(l.Tags);
					foreach(int i in l.Tags) tagshash.Add(i);
				}

				// Now sort them in descending order
				tags.Sort((a, b) => -1 * a.CompareTo(b));

				// Create enum items
				foreach(int tag in tags)
				{
					if(General.Map.Options.TagLabels.ContainsKey(tag)) // Tag labels
						taglist.Add(new EnumItem(tag.ToString(), General.Map.Options.TagLabels[tag]));
					else
						taglist.Add(new EnumItem(tag.ToString(), tag.ToString()));
				}
			}

			return taglist;
		}

		#endregion
	}
}
