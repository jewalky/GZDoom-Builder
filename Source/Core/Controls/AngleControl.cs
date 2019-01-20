
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
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;
using System.Drawing.Drawing2D;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	/// <summary>
	/// Control which allows you to click on one of the buttons to select a rotation by 45 degrees.
	/// </summary>
	public partial class AngleControl : UserControl
	{
		#region ================== Constants

		private const float LINE_THICKNESS = 3f;

		#endregion
		
		#region ================== Events

		public event EventHandler ValueChanged;
		public event EventHandler ButtonClicked;

		#endregion

		#region ================== Variables

		// Buttons
		private RadioButton[] buttons;
		
		// Result
		private int angle;
		private bool settingangle;
		
		#endregion

		#region ================== Properties

		public int Value { get { return angle; } set { SetAngle(value, true); } }
		
		#endregion
		
		#region ================== Constructor / Disposer

		// Constructor
		public AngleControl()
		{
			// Initialize
			InitializeComponent();

			// Make array from buttons
			buttons = new RadioButton[8];
			buttons[0] = button0;
			buttons[1] = button1;
			buttons[2] = button2;
			buttons[3] = button3;
			buttons[4] = button4;
			buttons[5] = button5;
			buttons[6] = button6;
			buttons[7] = button7;
		}

		#endregion

		#region ================== Interface

		// Redraw the control
		private void AngleControl_Paint(object sender, PaintEventArgs e)
		{
			float rad = Angle2D.DegToRad(angle);
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.InterpolationMode = InterpolationMode.High;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.Clear(this.BackColor);
			Pen linepen = new Pen(SystemColors.ControlText, LINE_THICKNESS);
			PointF start = new PointF(this.Size.Width * 0.5f, this.Size.Height * 0.5f);
			float line_length = this.Size.Width * 0.26f;
			if((rad >= 0) && (rad < 360))
			{
				PointF end = new PointF(start.X + (float)Math.Sin(rad + Angle2D.PIHALF) * line_length,
										start.Y + (float)Math.Cos(rad + Angle2D.PIHALF) * line_length);
				e.Graphics.DrawLine(linepen, start, end);
			}
			else
			{
				e.Graphics.DrawLine(linepen, start, start);
			}
			linepen.Dispose(); //mxd
		}
		
		#endregion

		#region ================== Control

		// This sets an angle manually
		private void SetAngle(int newangle, bool changebuttons)
		{
			// Normalize and apply angle
			bool changed = (newangle != angle);
			angle = newangle;
			
			// Check if it matches an angle from the buttons
			if(changebuttons)
			{
				settingangle = true;
				for(int i = 0; i < 8; i++)
					buttons[i].Checked = (angle == i * 45);
				settingangle = false;
			}
			
			// Redraw
			this.Invalidate();
			
			// Raise event
			if((ValueChanged != null) && changed) ValueChanged(this, EventArgs.Empty);
		}
		
		// When checked state of a button changes
		private void button_CheckedChanged(object sender, EventArgs e)
		{
			if(!settingangle)
			{
				// Check if we can get the angle from one of the buttons
				for(int i = 0; i < 8; i++)
					if(buttons[i].Checked) SetAngle(i * 45, false);

				// Raise event
				if(ButtonClicked != null) ButtonClicked(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
