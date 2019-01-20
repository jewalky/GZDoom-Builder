
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
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class LineLengthLabel : CustomTextLabel, IDisposable
	{
		#region ================== Constants

		private const string VALUE_FORMAT = "0";

		#endregion

		#region ================== Variables

		protected Vector2D start;
		protected Vector2D end;

		//mxd. Display options
		private bool showangle;
		private bool offsetposition;
		
		#endregion

		#region ================== Properties

		//mxd. Display options
		public bool ShowAngle { get { return showangle; } set { showangle = value; UpdateText(); } }
		public bool OffsetPosition { get { return offsetposition; } set { offsetposition = value; Move(start, end); } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public LineLengthLabel()
		{
			this.showangle = true;
			this.offsetposition = true;
			
			// Initialize
			Initialize();
		}

		//mxd. Constructor
		public LineLengthLabel(bool showangle, bool offsetposition)
		{
			this.showangle = showangle;
			this.offsetposition = offsetposition;

			// Initialize
			Initialize();
		}

		// Constructor
		public LineLengthLabel(Vector2D start, Vector2D end)
		{
			this.showangle = true; //mxd
			this.offsetposition = true; //mxd
			
			// Initialize
			Initialize();
			Move(start, end);
		}

		//mxd. Constructor
		public LineLengthLabel(Vector2D start, Vector2D end, bool showangle, bool offsetposition)
		{
			this.showangle = showangle;
			this.offsetposition = offsetposition;
			
			// Initialize
			Initialize();
			Move(start, end);
		}

		// Initialization
		private void Initialize()
		{
			label = new TextLabel
					{
						AlignX = TextAlignmentX.Center, 
						AlignY = TextAlignmentY.Middle, 
						Color = General.Colors.Highlight, 
						BackColor = General.Colors.Background.WithAlpha(128), 
						TransformCoords = true,
					};
		}
		
		// Disposer
		public void Dispose()
		{
			label.Dispose();
		}

		#endregion
		
		#region ================== Methods

		// This updates the text
		protected virtual void UpdateText()
		{
			Vector2D delta = end - start;

			// Update label text
			float length = delta.GetLength();
			if(showangle)
			{
				int displayangle = (int)Math.Round(Angle2D.RadToDeg(delta.GetAngle()));
				label.Text = "L:" + length.ToString(VALUE_FORMAT) + "; A:" + displayangle;
			}
			else
			{
				label.Text = length.ToString(VALUE_FORMAT);
			}
		}

		//mxd. This moves the label so it stays on screen and offsets it vertically so it doesn't overlap the line
		public virtual void Move(Vector2D start, Vector2D end)
		{
			// Store before making any adjustments to start/end...
			this.start = start;
			this.end = end;

			// Update text label
			UpdateText();

			// Check if start/end point is on screen...
			Vector2D lt = General.Map.Renderer2D.DisplayToMap(new Vector2D(0.0f, General.Interface.Display.Size.Height));
			Vector2D rb = General.Map.Renderer2D.DisplayToMap(new Vector2D(General.Interface.Display.Size.Width, 0.0f));
			RectangleF viewport = new RectangleF(lt.x, lt.y, rb.x - lt.x, rb.y - lt.y);
			bool startvisible = viewport.Contains(start.x, start.y);
			bool endvisible = viewport.Contains(end.x, end.y);

			// Do this only when one point is visible, an the other isn't 
			if((!startvisible && endvisible) || (startvisible && !endvisible))
			{
				Line2D drawnline = new Line2D(start, end);
				Line2D[] viewportsides = new[] {
					new Line2D(lt, rb.x, lt.y), // top
					new Line2D(lt.x, rb.y, rb.x, rb.y), // bottom
					new Line2D(lt, lt.x, rb.y), // left
					new Line2D(rb.x, lt.y, rb.x, rb.y), // right
				};

				foreach(Line2D side in viewportsides)
				{
					// Modify the start point so it stays on screen
					float u;
					if(!startvisible && side.GetIntersection(drawnline, out u))
					{
						start = drawnline.GetCoordinatesAt(u);
						break;
					}

					// Modify the end point so it stays on screen
					if(!endvisible && side.GetIntersection(drawnline, out u))
					{
						end = drawnline.GetCoordinatesAt(u);
						break;
					}
				}
			}

			// Update label position
			if(offsetposition)
			{
				Vector2D perpendicular = (end - start).GetPerpendicular();
				float angle = perpendicular.GetAngle();
				SizeF textsize = General.Interface.MeasureString(label.Text, label.Font);
				float offset = textsize.Width * Math.Abs((float)Math.Sin(angle)) + textsize.Height * Math.Abs((float)Math.Cos(angle));
				perpendicular = perpendicular.GetNormal().GetScaled(offset / 2.0f / General.Map.Renderer2D.Scale);
				start += perpendicular;
				end += perpendicular;
			}

			// Apply changes
			Vector2D delta = end - start;
			label.Location = new Vector2D(start.x + delta.x * 0.5f, start.y + delta.y * 0.5f);
		}
		
		#endregion
	}
}
