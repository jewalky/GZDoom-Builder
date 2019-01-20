
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
	public partial class AngleForm : DelayedForm
	{
		#region ================== Variables

		//private bool setup;
		private int value;

		#endregion

		#region ================== Properties

		public int Value { get { return value; } }

		#endregion

		#region ================== Constructor

		// Constructor
		public AngleForm()
		{
			InitializeComponent();
		}
		
		#endregion

		#region ================== Events

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// Close
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			this.value = angle.Value;
			
			// Done
			DialogResult = DialogResult.OK;
			this.Close();
		}

		#endregion

		#region ================== Methods

		// Setup from EnumList
		public void Setup(int value)
		{
			//setup = true;
			this.value = value;
			angle.Value = value;
			//setup = false;
		}
		
		// This shows the dialog
		// Returns the flags or the same flags when cancelled
		public static int ShowDialog(IWin32Window owner, int value)
		{
			int result = value;
			AngleForm f = new AngleForm();
			f.Setup(value);
			if(f.ShowDialog(owner) == DialogResult.OK) result = f.Value;
			f.Dispose();
			return result;
		}

		#endregion
	}
}