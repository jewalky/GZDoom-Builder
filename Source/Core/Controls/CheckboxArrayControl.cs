
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public partial class CheckboxArrayControl : UserControl
	{
		// Events
		public event EventHandler OnValueChanged; //mxd

		// Variables
		private List<CheckBox> checkboxes;
		private int columns;
		private int spacingY = 1; //mxd
		
		// Properties
		public List<CheckBox> Checkboxes { get { return checkboxes; } }
		public int Columns { get { return columns; } set { columns = value; } }
		public int VerticalSpacing { get { return spacingY; } set { spacingY = value; } } //mxd
		
		// Constructor
		public CheckboxArrayControl()
		{
			// Initialize
			InitializeComponent();

			// Setup
			checkboxes = new List<CheckBox>();
		}

		// This adds a checkbox
		public CheckBox Add(string text, object tag)
		{
			// Make new checkbox
			CheckBox c = new CheckBox();
			c.AutoSize = true;
			//c.FlatStyle = FlatStyle.System;
			c.UseVisualStyleBackColor = true;
			c.Text = text;
			c.Tag = tag;
			c.CheckStateChanged += checkbox_OnCheckStateChanged; //mxd

			// Add to list
			this.Controls.Add(c);
			checkboxes.Add(c);

			// Return checkbox
			return c;
		}

		//mxd
		public int GetWidth() 
		{
			if(columns < 1 || checkboxes.Count < 1) return 0;
			int maxwidth = 0;
			foreach(CheckBox cb in checkboxes)
			{
				if(cb.Width > maxwidth) maxwidth = cb.Width;
			}

			return maxwidth * columns;
		}

		//mxd
		public int GetHeight() 
		{
			if(columns < 1 || checkboxes.Count < 1)	return 0;
			int col = (int)Math.Ceiling(checkboxes.Count / (float)columns);
			return col * checkboxes[0].Height + (col * spacingY + spacingY);
		}

		// This positions the checkboxes
		public void PositionCheckboxes()
		{
			int boxheight = 0;
			int row = 0;
			int col = 0;
			
			// Checks
			if(columns < 1 || checkboxes.Count < 1) return;
			
			// Calculate column width
			int columnwidth = this.ClientSize.Width / columns;
			
			// Check what the biggest checkbox height is
			foreach(CheckBox c in checkboxes) if(c.Height > boxheight) boxheight = c.Height;

			// Check what the preferred column length is
			int columnlength = 1 + (int)Math.Floor((this.ClientSize.Height - boxheight) / (float)(boxheight + spacingY));
			
			// When not all items fit with the preferred column length
			// we have to extend the column length to make it fit
			if((int)Math.Ceiling(checkboxes.Count / (float)columnlength) > columns)
			{
				// Make a column length which works for all items
				columnlength = (int)Math.Ceiling(checkboxes.Count / (float)columns);
			}

			// Go for all items
			foreach(CheckBox c in checkboxes)
			{
				// Position checkbox
				c.Location = new Point(col * columnwidth, row * boxheight + (row - 1) * spacingY + spacingY);

				// Next position
				if(++row == columnlength)
				{
					row = 0;
					col++;
				}
			}
		}

		//mxd
		public void Sort()
		{
			checkboxes.Sort(CheckboxesComparison);
		}

		//mxd
		private static int CheckboxesComparison(CheckBox cb1, CheckBox cb2)
		{
			return String.Compare(cb1.Text, cb2.Text, StringComparison.Ordinal);
		}

		// When layout must change
		private void CheckboxArrayControl_Layout(object sender, LayoutEventArgs e)
		{
			PositionCheckboxes();
		}

		private void CheckboxArrayControl_Paint(object sender, PaintEventArgs e)
		{
			if(this.DesignMode)
			{
				using(Pen p = new Pen(SystemColors.ControlDark, 1) { DashStyle = DashStyle.Dash })
				{
					e.Graphics.DrawRectangle(p, 0, 0, this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);
				}
			}
		}

		//mxd
		private void checkbox_OnCheckStateChanged(object sender, EventArgs eventArgs) 
		{
			if(OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
		}
	}
}
