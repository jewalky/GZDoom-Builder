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
	[FindReplace("Any UDMF Field", BrowseButton = false)]
	internal class FindAnyUDMFField : BaseFindUDMFField
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

			List<MapElement> list = new List<MapElement>();

			if(withinselection)
			{
				list.AddRange(General.Map.Map.GetSelectedSectors(true));
				list.AddRange(General.Map.Map.GetSelectedLinedefs(true));

				foreach (Linedef ld in General.Map.Map.GetSelectedLinedefs(true))
				{
					if (ld.Front != null && !ld.Front.IsDisposed)
						list.Add(ld.Front);

					if (ld.Back != null && !ld.Back.IsDisposed)
						list.Add(ld.Back);
				}

				list.AddRange(General.Map.Map.GetSelectedThings(true));
				list.AddRange(General.Map.Map.GetSelectedVertices(true));
			}
			else
			{
				list.AddRange(General.Map.Map.Sectors);
				list.AddRange(General.Map.Map.Linedefs);
				list.AddRange(General.Map.Map.Sidedefs);
				list.AddRange(General.Map.Map.Things);
				list.AddRange(General.Map.Map.Vertices);
			}

			return GetObjects(value, list);
		}

		#endregion
	}
}
