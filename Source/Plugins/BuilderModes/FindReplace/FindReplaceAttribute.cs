
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

namespace CodeImp.DoomBuilder.BuilderModes
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	internal class FindReplaceAttribute : Attribute
	{
		#region ================== Variables

		private string displayname;
		private bool browsebutton;

		#endregion

		#region ================== Properties

		public string DisplayName { get { return displayname; } set { displayname = value; } }
		public bool BrowseButton { get { return browsebutton; } set { browsebutton = value; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public FindReplaceAttribute(string displayname)
		{
			// Initialize
			this.displayname = displayname;
		}

		#endregion

		#region ================== Methods

		// String representation
		public override string ToString()
		{
			return displayname;
		}

		#endregion
	}
}
