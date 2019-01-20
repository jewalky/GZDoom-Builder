
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

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public class SectorEffectInfo : INumberedTitle, IComparable<SectorEffectInfo>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Properties
		private int index;
		private string title;
		private bool isknown;
		private bool isgeneralized;
		
		#endregion

		#region ================== Properties

		public int Index { get { return index; } }
		public string Title { get { return title; } }
		public bool IsGeneralized { get { return isgeneralized; } }
		public bool IsKnown { get { return isknown; } }
		public bool IsNull { get { return (index == 0); } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal SectorEffectInfo(int index, string title, bool isknown, bool isgeneralized)
		{
			// Initialize
			this.index = index;
			this.title = title;
			this.isknown = isknown;
			this.isgeneralized = isgeneralized;
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This presents the item as string
		public override string ToString()
		{
			return index + " - " + title;
		}

		// This compares against another action info
		public int CompareTo(SectorEffectInfo other)
		{
			if(this.index < other.index) return -1;
			else if(this.index > other.index) return 1;
			else return 0;
		}

		#endregion
	}
}
