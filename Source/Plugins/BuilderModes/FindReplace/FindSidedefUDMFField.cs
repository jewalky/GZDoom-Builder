#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * Copyright (c) 2021 Boris Iwanski
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
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Sidedef UDMF Field", BrowseButton = false)]
	internal class FindSidedefUDMFField : BaseFindUDMFField
	{
		#region ================== Methods

		public override bool CanReplace()
		{
			return false;
		}

		public override bool DetermineVisiblity()
		{
			return General.Map.UDMF;
		}

		public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
		{
			if (string.IsNullOrWhiteSpace(value))
				return new FindReplaceObject[] { };

			ICollection<MapElement> list;

			if(withinselection)
			{
				// Get all sidedefs of the selected linedefs
				list = new List<MapElement>();

				foreach(Linedef ld in General.Map.Map.GetSelectedLinedefs(true))
				{
					if (ld.Front != null && !ld.Front.IsDisposed)
						list.Add(ld.Front);

					if (ld.Back != null && !ld.Back.IsDisposed)
						list.Add(ld.Back);
				}
			}
			else
			{
				list = (ICollection<MapElement>)General.Map.Map.Sidedefs;
			}

			return GetObjects(value, list);
		}

		#endregion
	}
}
