
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
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	public partial class ThingBrowserForm : DelayedForm
	{
		// Variables
		private int selectedtype;
		
		// Properties
		public int SelectedType { get { return selectedtype; } }
		
		// Constructor
		public ThingBrowserForm(int type)
		{
			InitializeComponent();

			// Setup list
			thingslist.Setup();

			// Select given type
			thingslist.SelectType(type);
		}

		// This browses for a thing type
		// Returns the new thing type or the same thing type when cancelled
		public static int BrowseThing(IWin32Window owner, int type)
		{
			ThingBrowserForm f = new ThingBrowserForm(type);
			if(f.ShowDialog(owner) == DialogResult.OK) type = f.SelectedType;
			f.Dispose();
			return type;
		}
		
		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			// Get the result
			selectedtype = thingslist.GetResult(selectedtype);

			// Done
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// Leave
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// Double-clicked an item
		private void thingslist_OnTypeDoubleClicked()
		{
			// OK
			apply_Click(this, EventArgs.Empty);
		}

		//mxd
		private void ThingBrowserForm_Shown(object sender, EventArgs e)
		{
			thingslist.FocusTextbox();
		}
	}
}