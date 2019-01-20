
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
	[TypeHandler(UniversalType.ThingTag, "Thing Tag", true)]
	internal class ThingTagHandler : SectorTagHandler
	{
		#region ================== Setup (mxd)

		protected override EnumList CreateEnumList() 
		{
			// Collect tags
			List<int> tags = new List<int>();
			EnumList taglist = new EnumList();

			if(General.Map.Map != null)
			{
				foreach(Thing t in General.Map.Map.Things)
				{
					if(t.Tag == 0 || tags.Contains(t.Tag)) continue;

					// Check target class?
					if(arginfo.TargetClasses.Count > 0)
					{
						ThingTypeInfo info = General.Map.Data.GetThingInfoEx(t.Type);
						if(info != null && !arginfo.TargetClasses.Contains(info.ClassName))
							continue;
					}

					tags.Add(t.Tag);
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
