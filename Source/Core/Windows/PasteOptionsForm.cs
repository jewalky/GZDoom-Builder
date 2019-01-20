
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
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class PasteOptionsForm : DelayedForm
	{
		#region ================== Variables
		
		private PasteOptions options;
		
		#endregion
		
		#region ================== Properties
		
		public PasteOptions Options { get { return options; } }
		
		#endregion
		
		#region ================== Constructor
		
		// Constructor
		public PasteOptionsForm()
		{
			InitializeComponent();
			
			// Get defaults
			options = General.Settings.PasteOptions.Copy();
			pasteoptions.Setup(options);
		}
		
		#endregion
		
		#region ================== Events
		
		// Paste clicked
		private void paste_Click(object sender, EventArgs e)
		{
			options = pasteoptions.GetOptions();
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
		
		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
		
		#endregion
	}
}
