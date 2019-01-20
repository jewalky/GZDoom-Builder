
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

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultNoErrors : ErrorResult
	{
		#region ================== Variables

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 0; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ResultNoErrors()
		{
		}

		#endregion

		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) { }

		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return "No errors were found.";
		}
		
		#endregion
	}
}
