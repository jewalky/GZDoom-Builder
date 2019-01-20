
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
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public partial class ColorControl : UserControl
	{
		public event EventHandler ColorChanged;

		// Properties
		public string Label { get { return label.Text; } set { label.Text = value; } }
		public PixelColor Color { get { return PixelColor.FromColor(panel.BackColor); } set { panel.BackColor = System.Drawing.Color.FromArgb(value.ToInt()); } }

		// Constructor
		public ColorControl()
		{
			// Initialize
			InitializeComponent();
		}

		//mxd. Panel clicked
		private void panel_Click(object sender, EventArgs e)
		{
			// Show color dialog
			dialog.Color = panel.BackColor;
			if(dialog.ShowDialog(this.ParentForm) == DialogResult.OK)
			{
				// Apply new color
				panel.BackColor = dialog.Color;

				// Dispatch Event
				if(ColorChanged != null) ColorChanged(this, EventArgs.Empty);
			}
		}
	}
}
