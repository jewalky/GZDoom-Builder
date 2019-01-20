using System;
using System.Drawing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Rendering;

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class HintLabel : LineLengthLabel
	{
		public HintLabel() : base(false, false) { }
		public HintLabel(PixelColor textcolor) : base(false, false)
		{
			label.Color = textcolor;
		}

		// We don't want any changes here
		protected override void UpdateText() { }

		public override void Move(Vector2D start, Vector2D end)
		{
			// Store before making any adjustments to start/end...
			this.start = start;
			this.end = end;

			// Check if start/end point is on screen...
			Vector2D lt = General.Map.Renderer2D.DisplayToMap(new Vector2D(0.0f, General.Interface.Display.Size.Height));
			Vector2D rb = General.Map.Renderer2D.DisplayToMap(new Vector2D(General.Interface.Display.Size.Width, 0.0f));
			RectangleF viewport = new RectangleF(lt.x, lt.y, rb.x - lt.x, rb.y - lt.y);
			bool startvisible = viewport.Contains(start.x, start.y);
			bool endvisible = viewport.Contains(end.x, end.y);

			// Get visile area
			if(!startvisible || !endvisible)
			{
				float minx = Math.Min(start.x, end.x);
				float maxx = Math.Max(start.x, end.x);
				float miny = Math.Min(start.y, end.y);
				float maxy = Math.Max(start.y, end.y);
				RectangleF labelarea = new RectangleF(minx, miny, maxx - minx, maxy - miny);
				labelarea.Intersect(viewport);

				if(!labelarea.IsEmpty)
				{
					label.Location = new Vector2D(labelarea.X + labelarea.Width * 0.5f, labelarea.Y + labelarea.Height * 0.5f);
					return;
				}
			}

			Vector2D delta = end - start;
			label.Location = new Vector2D(start.x + delta.x * 0.5f, start.y + delta.y * 0.5f);
		}
	}
}
