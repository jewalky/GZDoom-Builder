#region Namespaces

//Downloaded from
//Visual C# Kicks - http://vckicks.110mb.com
//The Code Project - http://www.codeproject.com

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public partial class AngleControlEx : UserControl
	{
		#region Variables

		private int angle;
		private int angleoffset;

		private Rectangle drawRegion;
		private const int drawOffset = 2;
		private const int markScaler = 5;
		private Point origin;

		private bool doomangleclamping;

		//UI colors
		private readonly Color fillColor = SystemColors.Window;
		private readonly Color fillInactiveColor = SystemColors.Control;
		private readonly Color outlineColor = SystemColors.WindowFrame;
		private readonly Color outlineInactiveColor = SystemColors.ControlDarkDark;
		private readonly Color needleColor = SystemColors.ControlText;
		private readonly Color needleInactiveColor = SystemColors.ControlDarkDark;
		private readonly Color marksColor = SystemColors.ActiveBorder;
		private readonly Color marksInactiveColor = SystemColors.ControlDark;

		#endregion

		#region Properties

		public event EventHandler AngleChanged;

		public int Angle { get { return (angle == NO_ANGLE ? NO_ANGLE : angle - angleoffset); } set {
			if(LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
				angle = (value == NO_ANGLE ? NO_ANGLE : value + angleoffset);
				this.Refresh();
			}
		} }
		public int AngleOffset { get { return angleoffset; } set { angleoffset = value; this.Refresh(); } }
		public bool DoomAngleClamping { get { return doomangleclamping; } set { doomangleclamping = value; } }
		public const int NO_ANGLE = int.MinValue;

		#endregion

		public AngleControlEx() 
		{
			InitializeComponent();
			this.DoubleBuffered = true;
		}

		#region Methods

		private void SetDrawRegion() 
		{
			drawRegion = new Rectangle(0, 0, this.Width, this.Height);
			drawRegion.X += 2;
			drawRegion.Y += 2;
			drawRegion.Width -= 4;
			drawRegion.Height -= 4;

			origin = new Point(drawRegion.Width / 2 + drawOffset, drawRegion.Height / 2 + drawOffset);

			this.Refresh();
		}

		private static PointF DegreesToXY(float degrees, float radius, Point origin) 
		{
			PointF xy = new PointF();
			float radians = degrees * Angle2D.PI / 180.0f;

			xy.X = (float)Math.Cos(radians) * radius + origin.X;
			xy.Y = (float)Math.Sin(-radians) * radius + origin.Y;

			return xy;
		}

		private static int XYToDegrees(Point xy, Point origin) 
		{
			float xDiff = xy.X - origin.X;
			float yDiff = xy.Y - origin.Y;
			return ((int)Math.Round(Math.Atan2(-yDiff, xDiff) * 180.0 / Angle2D.PI) + 360) % 360;
		}

		#endregion

		#region Events

		private void AngleSelector_Load(object sender, EventArgs e) 
		{
			SetDrawRegion();
		}

		private void AngleSelector_SizeChanged(object sender, EventArgs e) 
		{
			this.Height = this.Width; // Keep it there and keep it square!
			SetDrawRegion();
		}

		protected override void OnPaint(PaintEventArgs e) 
		{
			Graphics g = e.Graphics;

			Pen outline;
			Pen needle;
			Pen marks;
			SolidBrush fill;
			Brush center;

			if(this.Enabled) 
			{
				outline = new Pen(outlineColor, 2.0f);
				fill = new SolidBrush(fillColor);
				needle = new Pen(needleColor);
				center = new SolidBrush(needleColor);
				marks = new Pen(marksColor);
			} 
			else 
			{
				outline = new Pen(outlineInactiveColor, 2.0f);
				fill = new SolidBrush(fillInactiveColor);
				needle = new Pen(needleInactiveColor);
				center = new SolidBrush(needleInactiveColor);
				marks = new Pen(marksInactiveColor);
			}

			Rectangle originSquare = new Rectangle(origin.X - 1, origin.Y - 1, 3, 3);

			//Draw circle
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.DrawEllipse(outline, drawRegion);
			g.FillEllipse(fill, drawRegion);

			// Draw angle marks
			int offset = this.Height / markScaler;
			for(int i = 0; i < 360; i += 45) 
			{
				PointF p1 = DegreesToXY(i, origin.X - 6, origin);
				PointF p2 = DegreesToXY(i, origin.X - offset, origin);
				g.DrawLine(marks, p1, p2);
			}

			// Draw needle
			if(angle != NO_ANGLE) 
			{
				PointF anglePoint = DegreesToXY(angle, origin.X - 4, origin);
				g.DrawLine(needle, origin, anglePoint);
			}

			g.SmoothingMode = SmoothingMode.HighSpeed; //Make the square edges sharp
			g.FillRectangle(center, originSquare);

			//mxd. Dispose brushes
			fill.Dispose();
			center.Dispose();
			outline.Dispose();
			marks.Dispose();
			needle.Dispose();

			base.OnPaint(e);
		}

		private void AngleSelector_MouseDown(object sender, MouseEventArgs e) 
		{
			int thisAngle = XYToDegrees(new Point(e.X, e.Y), origin);

			if(e.Button == MouseButtons.Left) 
			{
				thisAngle = (int)Math.Round(thisAngle / 45f) * 45;
				if(thisAngle == 360) thisAngle = 0;
			}

			if(thisAngle != angle) 
			{
				angle = thisAngle;
				if(!this.DesignMode && AngleChanged != null) AngleChanged(this, EventArgs.Empty); //Raise event
				this.Refresh();
			}
		}

		private void AngleSelector_MouseMove(object sender, MouseEventArgs e) 
		{
			if(e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) 
			{
				int thisAngle = XYToDegrees(new Point(e.X, e.Y), origin);

				if(e.Button == MouseButtons.Left || doomangleclamping) 
				{
					thisAngle = (int)Math.Round(thisAngle / 45f) * 45;
					if(thisAngle == 360) thisAngle = 0;
				}

				if(thisAngle != angle) 
				{
					angle = thisAngle;
					if(!this.DesignMode && AngleChanged != null) AngleChanged(this, EventArgs.Empty); //Raise event
					this.Refresh();
				}
			}
		}

		#endregion
	}
}
