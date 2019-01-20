
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

#endregion

namespace CodeImp.DoomBuilder.Geometry
{
	public class ProjectedFrustum2D
	{
		#region ================== Variables
		
		// Frustum settings
		private readonly float near;
		private readonly float far;
		private readonly float fov;
		private readonly Vector2D pos;
		private readonly float xyangle;
		private readonly float zangle;
		
		// Frustum lines
		private readonly Line2D[] lines;

		// Circle
		private readonly Vector2D center;
		private readonly float radius;
		
		#endregion

		#region ================== Properties

		public float Near { get { return near; } }
		public float Far { get { return far; } }
		public float Fov { get { return fov; } }
		public Vector2D Position { get { return pos; } }
		public float XYAngle { get { return xyangle; } }
		public float ZAngle { get { return zangle; } }
		public Line2D[] Lines { get { return lines; } }
		public Vector2D Center { get { return center; } }
		public float Radius { get { return radius; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ProjectedFrustum2D(Vector2D pos, float xyangle, float zangle, float near, float far, float fov)
		{
			Vector2D[] forwards = new Vector2D[4];
			Vector2D[] downwards = new Vector2D[4];
			Vector2D[] corners = new Vector2D[4];
			
			// Initialize
			this.pos = pos;
			this.xyangle = xyangle;
			this.zangle = zangle;
			this.near = near;
			this.far = far;
			this.fov = fov;
			
			// Make the corners for a forward frustum
			// The corners are in this order: Left-Far, Right-Far, Left-Near, Right-Near
			float fovhalf = fov * 0.5f;
			float fovhalfcos = (float)Math.Cos(fovhalf);
			float farsidelength = far / fovhalfcos;
			float nearsidelength = near / fovhalfcos;
			forwards[0] = pos + Vector2D.FromAngle(xyangle - fovhalf, farsidelength);
			forwards[1] = pos + Vector2D.FromAngle(xyangle + fovhalf, farsidelength);
			forwards[2] = pos + Vector2D.FromAngle(xyangle - fovhalf, nearsidelength);
			forwards[3] = pos + Vector2D.FromAngle(xyangle + fovhalf, nearsidelength);
			
			// Make the corners for a downward frustum
			// The corners are in the same order as above
			//float farradius = far * (float)Math.Tan(fovhalf) * Angle2D.SQRT2;
			float farradius = far * 0.5f * Angle2D.SQRT2;
			downwards[0] = pos + Vector2D.FromAngle(xyangle - Angle2D.PI * 0.25f, farradius);
			downwards[1] = pos + Vector2D.FromAngle(xyangle + Angle2D.PI * 0.25f, farradius);
			downwards[2] = pos + Vector2D.FromAngle(xyangle - Angle2D.PI * 0.75f, farradius);
			downwards[3] = pos + Vector2D.FromAngle(xyangle + Angle2D.PI * 0.75f, farradius);
			
			// Interpolate between the two to make the final corners depending on the z angle
			float d = Math.Abs((float)Math.Sin(zangle));
			corners[0] = forwards[0] * (1.0f - d) + downwards[0] * d;
			corners[1] = forwards[1] * (1.0f - d) + downwards[1] * d;
			corners[2] = forwards[2] * (1.0f - d) + downwards[2] * d;
			corners[3] = forwards[3] * (1.0f - d) + downwards[3] * d;
			
			// Make the frustum lines
			// Note that the lines all have their right side inside the frustum!
			lines = new Line2D[4];
			lines[0] = new Line2D(corners[2], corners[0]);
			lines[1] = new Line2D(corners[1], corners[3]);
			lines[2] = new Line2D(corners[3], corners[2]);
			lines[3] = new Line2D(corners[0], corners[1]);

			// Calculate the circle center
			center = (corners[0] + corners[1] + corners[2] + corners[3]) * 0.25f;
			
			// Calculate the radius from the center to the farthest corner
			float radius2 = 0.0f;
			for(int i = 0; i < corners.Length; i++)
			{
				float distance2 = Vector2D.DistanceSq(center, corners[i]);
				if(distance2 > radius2) radius2 = distance2;
			}
			radius = (float)Math.Sqrt(radius2);
		}

		#endregion

		#region ================== Methods

		// This checks if a specified circle is intersecting the frustum
		// NOTE: This checks only against the actual frustum and does not use the frustum circle!
		public bool IntersectCircle(Vector2D circlecenter, float circleradius)
		{
			// Go for all frustum lines
			for(int i = 0; i < lines.Length; i++)
			{
				// Check on which side the circle center lies
				if(lines[i].GetSideOfLine(circlecenter) < 0)
				{
					// Center is outside the frustum
					// If the circle is not overlapping, it is not intersecting.
					if(lines[i].GetDistanceToLineSq(circlecenter, false) > (circleradius * circleradius)) return false;
				}
			}

			// Intersecting!
			return true;
		}

		#endregion
	}
}
