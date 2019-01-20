
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
	public class LinedefActivateInfo : IComparable<LinedefActivateInfo>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Properties
		private int intkey;
		private string key;
		private string title;
		
		#endregion

		#region ================== Properties

		public int Index { get { return intkey; } }
		public string Key { get { return key; } }
		public string Title { get { return title; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal LinedefActivateInfo(string key, string title)
		{
			// Initialize
			this.key = key;
			this.title = title;
			
			// Try parsing key as int for comparison
			if(!int.TryParse(key, out intkey)) intkey = 0;
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This presents the item as string
		public override string ToString()
		{
			return title;
		}

		// This compares against another activate info
		public int CompareTo(LinedefActivateInfo other)
		{
			if(this.intkey < other.intkey) return -1;
			else if(this.intkey > other.intkey) return 1;
			else return 0;
		}
		
		#endregion
	}
}
