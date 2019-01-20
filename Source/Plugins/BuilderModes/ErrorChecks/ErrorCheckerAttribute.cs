
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
	public class ErrorCheckerAttribute : Attribute
	{
		#region ================== Variables
		
		private string displayname;
		private bool defaultchecked;
		private int cost;
		
		#endregion
		
		#region ================== Properties
		
		public string DisplayName { get { return displayname; } set { displayname = value; } }
		public bool DefaultChecked { get { return defaultchecked; } set { defaultchecked = value; } }
		public int Cost { get { return cost; } set { cost = value; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ErrorCheckerAttribute(string displayname, bool defaultchecked, int cost)
		{
			// Initialize
			this.displayname = displayname;
			this.defaultchecked = defaultchecked;
			this.cost = cost;
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
