
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

using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Actions
{
	internal struct KeyControl
	{
		#region ================== Variables

		public int key;
		public string name;

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public KeyControl(Keys key, string name)
		{
			// Initialize
			this.key = (int)key;
			this.name = name;
		}

		// Constructor
		public KeyControl(SpecialKeys key, string name)
		{
			// Initialize
			this.key = (int)key;
			this.name = name;
		}

		// Constructor
		public KeyControl(int key, string name)
		{
			// Initialize
			this.key = key;
			this.name = name;
		}

		#endregion

		#region ================== Methods

		// Returns name
		public override string ToString()
		{
			return name;
		}
		
		#endregion
	}
}
