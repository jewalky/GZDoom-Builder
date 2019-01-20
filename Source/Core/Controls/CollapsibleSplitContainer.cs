#region ================== Copyright (c) 2015 MaxED

// Parts of the code are based on "Collapsible Splitter control in C#" by Furty
// http://www.codeproject.com/Articles/3025/Collapsible-Splitter-control-in-C

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	[Designer("System.Windows.Forms.Design.SplitContainerDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class CollapsibleSplitContainer : SplitContainer, ISupportInitialize
	{
		#region ================== Private Properties

		// Declare and define some base properties
		private bool hot;
		private bool collapsed;
		private readonly Color hotcolor = CalculateColor(SystemColors.Highlight, SystemColors.Window, 70);
		private Rectangle bounds;
		private readonly Dictionary<int, int> scaled;

		// Storesome settings
		private int storedpanel1minsize;
		private int storedpanel2minsize;
		private int storedsplitterdistance;

		#endregion

		#region ================== Public Properties

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsCollapsed 
		{
			get { return collapsed; }
			set { collapsed = value; ToggleSplitter(); }
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SplitPosition
		{
			get { return GetSplitPosition(); }
			set { storedsplitterdistance = value; if(!IsCollapsed) ToggleSplitter(); }
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		private new int SplitterWidth 
		{
			get { return base.SplitterWidth; }
			set { base.SplitterWidth = value; }
		}
		
		#endregion

		#region ================== Constructor

		public CollapsibleSplitContainer()
		{
			// Register mouse events
			this.Click += OnClick;
			this.Resize += OnResize;
			this.MouseLeave += OnMouseLeave;
			this.MouseMove += OnMouseMove;
			this.MouseUp += OnMouseUp;

			//mxd. Set drawing style
			const ControlStyles cs = ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer;
			this.SetStyle(cs, true);
			
			object[] args = new object[] { cs, true };
			MethodInfo objMethodInfo = typeof(Control).GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
			objMethodInfo.Invoke(this.Panel1, args);
			objMethodInfo.Invoke(this.Panel2, args);

			// Force the width to 8px so that everything always draws correctly
			this.SplitterWidth = 8;

			//mxd. Create some scaled coordinates...
			int[] coords = new[] { 1, 2, 3, 4, 6, 8, 9, 14, 115 };
			scaled = new Dictionary<int, int>(coords.Length);
			foreach(int i in coords) scaled[i] = (int)Math.Round(i * MainForm.DPIScaler.Width);
		}

		#endregion

		#region ================== Event Handlers

		protected override void OnMouseDown(MouseEventArgs e) 
		{
			// if the hider control isn't hot, let the base resize action occur
			if(!this.hot && this.Panel1.Visible && this.Panel2.Visible) base.OnMouseDown(e);
		}

		private void OnResize(object sender, EventArgs e) 
		{
			this.Invalidate();
		}

		private void OnMouseMove(object sender, MouseEventArgs e) 
		{
			// check to see if the mouse cursor position is within the bounds of our control
			if(bounds.Contains(e.Location)) 
			{
				if(!this.hot) 
				{
					this.hot = true;
					this.Cursor = Cursors.Hand;
					this.Invalidate();
				}
			} 
			else 
			{
				if(this.hot) 
				{
					this.hot = false;
					this.Invalidate();
				}

				this.Cursor = Cursors.Default;
			}
		}

		private void OnMouseLeave(object sender, EventArgs e) 
		{
			// ensure that the hot state is removed
			this.hot = false;
			this.Cursor = Cursors.Default; //mxd
			this.Invalidate();
		}

		// User may've moved the splitter...
		private void OnMouseUp(object sender, MouseEventArgs mouseEventArgs) 
		{
			if(!collapsed) storedsplitterdistance = GetSplitPosition();
		}

		private void OnClick(object sender, EventArgs e) 
		{
			if(FixedPanel != FixedPanel.None && hot)
			{
				collapsed = !collapsed;
				ToggleSplitter();
				this.Invalidate();
			}
		}

		#endregion

		#region ================== Paint

		protected override void OnPaint(PaintEventArgs e) 
		{
			base.OnPaint(e);
			if(FixedPanel == FixedPanel.None) return;
			
			// find the rectangle for the splitter and paint it
			Rectangle r = this.SplitterRectangle;
			using(SolidBrush brushbg = new SolidBrush(this.BackColor))
			{
				e.Graphics.FillRectangle(brushbg, r);
			}

			Pen pendark = new Pen(SystemColors.ControlDark);
			SolidBrush brushlightlight = new SolidBrush(SystemColors.ControlLightLight);
			SolidBrush brushdark = new SolidBrush(SystemColors.ControlDark);
			SolidBrush brushdarkdark = new SolidBrush(SystemColors.ControlDarkDark);

			// Check the docking style and create the control rectangle accordingly
			if(this.Orientation == Orientation.Vertical) 
			{
				// create a new rectangle in the vertical center of the splitter for our collapse control button
				bounds = new Rectangle(r.X, r.Y + ((r.Height - scaled[115]) / 2), scaled[8], scaled[115]);

				// draw the background color for our control image
				using(SolidBrush bg = new SolidBrush(hot ? hotcolor : this.BackColor))
				{
					e.Graphics.FillRectangle(bg, new Rectangle(bounds.X + scaled[1], bounds.Y, scaled[6], scaled[115]));
				}

				// draw the top & bottom lines for our control image
				e.Graphics.DrawLine(pendark, bounds.X + scaled[1], bounds.Y, bounds.X + bounds.Width - scaled[2], bounds.Y);
				e.Graphics.DrawLine(pendark, bounds.X + scaled[1], bounds.Y + bounds.Height, bounds.X + bounds.Width - scaled[2], bounds.Y + bounds.Height);

				if(this.Enabled) 
				{
					// draw the arrows for our control image
					// the ArrowPointArray is a point array that defines an arrow shaped polygon
					e.Graphics.FillPolygon(brushdarkdark, ArrowPointArray(bounds.X + scaled[2], bounds.Y + scaled[3]));
					e.Graphics.FillPolygon(brushdarkdark, ArrowPointArray(bounds.X + scaled[2], bounds.Y + bounds.Height - scaled[9]));
				}

				// draw the dots for our control image using a loop
				int x = bounds.X + scaled[3];
				int y = bounds.Y + scaled[14];

				for(int i = 0; i < 30; i++) 
				{
					// light dot
					e.Graphics.FillRectangle(brushlightlight, x, y + scaled[1] + (i * scaled[3]), scaled[2], scaled[2]);
					// dark dot
					e.Graphics.FillRectangle(brushdark, x - scaled[1], y + (i * scaled[3]), scaled[2], scaled[2]);
					i++;
					// light dot
					e.Graphics.FillRectangle(brushlightlight, x + scaled[2], y + scaled[1] + (i * scaled[3]), scaled[2], scaled[2]);
					// dark dot
					e.Graphics.FillRectangle(brushdark, x + scaled[1], y + (i * scaled[3]), scaled[2], scaled[2]);
				}
			} 
			else // Should be Orientation.Horizontal
			{
				// create a new rectangle in the horizontal center of the splitter for our collapse control button
				bounds = new Rectangle(r.X + ((r.Width - scaled[115]) / 2), r.Y, scaled[115], scaled[8]);

				// draw the background color for our control image
				using(SolidBrush bg = new SolidBrush(hot ? hotcolor : this.BackColor))
				{
					e.Graphics.FillRectangle(bg, new Rectangle(bounds.X, bounds.Y + scaled[1], scaled[115], scaled[6]));
				}

				// draw the left & right lines for our control image
				e.Graphics.DrawLine(pendark, bounds.X, bounds.Y + scaled[1], bounds.X, bounds.Y + bounds.Height - scaled[2]);
				e.Graphics.DrawLine(pendark, bounds.X + bounds.Width, bounds.Y + scaled[1], bounds.X + bounds.Width, bounds.Y + bounds.Height - scaled[2]);

				if(this.Enabled) 
				{
					// draw the arrows for our control image
					// the ArrowPointArray is a point array that defines an arrow shaped polygon
					e.Graphics.FillPolygon(brushdarkdark, ArrowPointArray(bounds.X + scaled[3], bounds.Y + scaled[2]));
					e.Graphics.FillPolygon(brushdarkdark, ArrowPointArray(bounds.X + bounds.Width - scaled[9], bounds.Y + scaled[2]));
				}

				// draw the dots for our control image using a loop
				int x = bounds.X + scaled[14];
				int y = bounds.Y + scaled[3];

				for(int i = 0; i < 30; i++) 
				{
					// light dot
					e.Graphics.FillRectangle(brushlightlight, x + scaled[1] + (i * scaled[3]), y, scaled[2], scaled[2]);
					// dark dot
					e.Graphics.FillRectangle(brushdark, x + (i * scaled[3]), y - scaled[1], scaled[2], scaled[2]);
					i++;
					// light dot
					e.Graphics.FillRectangle(brushlightlight, x + scaled[1] + (i * scaled[3]), y + scaled[2], scaled[2], scaled[2]);
					// dark dot
					e.Graphics.FillRectangle(brushdark, x + (i * scaled[3]), y + scaled[1], scaled[2], scaled[2]);
				}
			}

			//mxd. Dispose brushes
			pendark.Dispose();
			brushlightlight.Dispose();
			brushdark.Dispose();
			brushdarkdark.Dispose();
		}

		#endregion

		#region ================== Helper methods

		private void ToggleSplitter() 
		{
			//mxd. Toggle visibility
			switch(FixedPanel)
			{
				case FixedPanel.Panel1:
					Panel1.Visible = !collapsed;
					if(collapsed)
					{
						storedsplitterdistance = SplitterDistance;
						storedpanel1minsize = Panel1MinSize;
						Panel1MinSize = 0;
						SplitterDistance = 0;
					}
					else
					{
						Panel1MinSize = storedpanel1minsize;
						SplitterDistance = Math.Min(this.Width, storedsplitterdistance);
					}
					break;

				case FixedPanel.Panel2:
					Panel2.Visible = !collapsed;
					if(collapsed)
					{
						storedpanel2minsize = Panel2MinSize;
						Panel2MinSize = 0;
					}
					else
					{
						Panel2MinSize = storedpanel2minsize;
					}

					if(Orientation == Orientation.Vertical)
					{
						if(collapsed) storedsplitterdistance = this.Width - SplitterDistance;
						SplitterDistance = (collapsed ? this.Width : Math.Max(0, this.Width - storedsplitterdistance));
					}
					else
					{
						if(collapsed) storedsplitterdistance = this.Height - SplitterDistance;
						SplitterDistance = (collapsed ? this.Height : Math.Max(0, this.Height - storedsplitterdistance));
					}
					break;
			}
		}

		private int GetSplitPosition() 
		{
			switch(FixedPanel) 
			{
				case FixedPanel.Panel1:
					return (Panel1.Visible ? SplitterDistance : storedsplitterdistance);

				case FixedPanel.Panel2:
					if(Panel2.Visible)
					{
						if(Orientation == Orientation.Vertical)
							return Math.Max(0, this.Width - SplitterDistance);
						else
							return Math.Max(0, this.Height - SplitterDistance);
					}
					else
					{
						return storedsplitterdistance;
					}
			}

			return SplitterDistance;
		}

		// This creates a point array to draw a arrow-like polygon
		private Point[] ArrowPointArray(int x, int y) 
		{
			Point[] points = new Point[3];

			// Right or left arrows
			if(Orientation == Orientation.Vertical)
			{
				if((FixedPanel == FixedPanel.Panel2 && Panel2.Visible) || (FixedPanel == FixedPanel.Panel1 && !Panel1.Visible)) // Right arrow
				{
					points[0] = new Point(x, y);
					points[1] = new Point(x + scaled[3], y + scaled[3]);
					points[2] = new Point(x, y + scaled[6]);
				}
				else // Left arrow
				{
					points[0] = new Point(x + scaled[3], y);
					points[1] = new Point(x, y + scaled[3]);
					points[2] = new Point(x + scaled[3], y + scaled[6]);
				}
			}
			else // Up or down arrows
			{
				if((FixedPanel == FixedPanel.Panel2 && Panel2.Visible) || (FixedPanel == FixedPanel.Panel1 && !Panel1.Visible)) // Down arrow
				{
					points[0] = new Point(x, y);
					points[1] = new Point(x + scaled[6], y);
					points[2] = new Point(x + scaled[3], y + scaled[3]);
				} 
				else // Up arrow
				{
					points[0] = new Point(x + scaled[3], y);
					points[1] = new Point(x + scaled[6], y + scaled[4]);
					points[2] = new Point(x, y + scaled[4]);
				}
			}

			return points;
		}

		// this method was borrowed from the RichUI Control library by Sajith M
		private static Color CalculateColor(Color front, Color back, int alpha) 
		{
			// solid color obtained as a result of alpha-blending
			Color frontColor = Color.FromArgb(255, front);
			Color backColor = Color.FromArgb(255, back);

			float frontRed = frontColor.R;
			float frontGreen = frontColor.G;
			float frontBlue = frontColor.B;
			float backRed = backColor.R;
			float backGreen = backColor.G;
			float backBlue = backColor.B;

			float fRed = frontRed * alpha / 255 + backRed * ((float)(255 - alpha) / 255);
			float fGreen = frontGreen * alpha / 255 + backGreen * ((float)(255 - alpha) / 255);
			float fBlue = frontBlue * alpha / 255 + backBlue * ((float)(255 - alpha) / 255);

			return Color.FromArgb(255, (byte)fRed, (byte)fGreen, (byte)fBlue);
		}

		// ISupportInitialize methods. Not needed for .Net 4 and higher
		//public void BeginInit() { }
		//public void EndInit() { }

		#endregion
	}
}
