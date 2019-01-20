using System;
using CodeImp.DoomBuilder.Rendering;

namespace CodeImp.DoomBuilder.Geometry 
{
	public class Line3D 
	{
		// Coordinates
		public Vector3D Start;
		public Vector3D End;
		public PixelColor Color;
		public readonly bool RenderArrowhead;

		// Changed by Renderer2D.RenderArrows()
		internal Vector2D Start2D;
		internal Vector2D End2D;
		internal bool SkipRendering;

		// Constructors
		public Line3D(Vector3D start, Vector3D end) 
		{
			this.Start = start;
			this.End = end;
			this.Start2D = start;
			this.End2D = end;
			this.Color = General.Colors.InfoLine;
			this.RenderArrowhead = true;
		}

		public Line3D(Vector3D start, Vector3D end, bool renderArrowhead)
		{
			this.Start = start;
			this.End = end;
			this.Start2D = start;
			this.End2D = end;
			this.Color = General.Colors.InfoLine;
			this.RenderArrowhead = renderArrowhead;
		}

		public Line3D(Vector3D start, Vector3D end, PixelColor color) 
		{
			this.Start = start;
			this.End = end;
			this.Start2D = start;
			this.End2D = end;
			this.Color = color;
			this.RenderArrowhead = true;
		}

		public Line3D(Vector3D start, Vector3D end, PixelColor color, bool renderArrowhead)
		{
			this.Start = start;
			this.End = end;
			this.Start2D = start;
			this.End2D = end;
			this.Color = color;
			this.RenderArrowhead = renderArrowhead;
		}

		public Vector3D GetDelta() { return End - Start; }

		// This calculates the angle
		public float GetAngle() 
		{
			// Calculate and return the angle
			Vector2D d = GetDelta();
			return -(float)Math.Atan2(-d.y, d.x) + Angle2D.PIHALF;
		}
	}
}
