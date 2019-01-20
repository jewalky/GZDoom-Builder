
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
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Geometry
{
	public struct Line2D
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Coordinates
		public Vector2D v1;
		public Vector2D v2;
		
		#endregion
		
		#region ================== Constructors
		
		// Constructor
		public Line2D(Vector2D v1, Vector2D v2)
		{
			this.v1 = v1;
			this.v2 = v2;
		}
		
		// Constructor
		public Line2D(Vector2D v1, float x2, float y2)
		{
			this.v1 = v1;
			this.v2 = new Vector2D(x2, y2);
		}

		// Constructor
		public Line2D(float x1, float y1, Vector2D v2)
		{
			this.v1 = new Vector2D(x1, y1);
			this.v2 = v2;
		}
		
		// Constructor
		public Line2D(float x1, float y1, float x2, float y2)
		{
			this.v1 = new Vector2D(x1, y1);
			this.v2 = new Vector2D(x2, y2);
		}

		//mxd. Constructor
		public Line2D(Linedef line)
		{
			this.v1 = line.Start.Position;
			this.v2 = line.End.Position;
		}
		
		#endregion

		#region ================== Statics

		// This calculates the length
		public static float GetLength(float dx, float dy)
		{
			// Calculate and return the length
			return (float)Math.Sqrt(GetLengthSq(dx, dy));
		}

		// This calculates the square of the length
		public static float GetLengthSq(float dx, float dy)
		{
			// Calculate and return the length
			return dx * dx + dy * dy;
		}

		// This calculates the normal of a line
		public static Vector2D GetNormal(float dx, float dy)
		{
			return new Vector2D(dx, dy).GetNormal();
		}

		//mxd. This tests if given lines intersects
		public static bool GetIntersection(Line2D line1, Line2D line2) 
		{
			return GetIntersection(line1.v1, line1.v2, line2.v1.x, line2.v1.y, line2.v2.x, line2.v2.y);
		}

		// This tests if the line intersects with the given line coordinates
		public static bool GetIntersection(Vector2D v1, Vector2D v2, float x3, float y3, float x4, float y4)
		{
			float u_ray, u_line;
			return GetIntersection(v1, v2, x3, y3, x4, y4, out u_ray, out u_line);
		}

		// This tests if the line intersects with the given line coordinates
		public static bool GetIntersection(Vector2D v1, Vector2D v2, float x3, float y3, float x4, float y4, out float u_ray)
		{
			float u_line;
			return GetIntersection(v1, v2, x3, y3, x4, y4, out u_ray, out u_line, true);
		}

		//mxd. This tests if the line intersects with the given line coordinates
		public static bool GetIntersection(Vector2D v1, Vector2D v2, float x3, float y3, float x4, float y4, out float u_ray, bool bounded)
		{
			float u_line;
			return GetIntersection(v1, v2, x3, y3, x4, y4, out u_ray, out u_line, bounded);
		}

		//mxd. Gets intersection point between given lines
		public static Vector2D GetIntersectionPoint(Line2D line1, Line2D line2, bool bounded)
		{
			float u_ray, u_line;
			if(GetIntersection(line1.v1, line1.v2, line2.v1.x, line2.v1.y, line2.v2.x, line2.v2.y, out u_ray, out u_line, bounded))
				return GetCoordinatesAt(line2.v1, line2.v2, u_ray);

			// No dice...
			return new Vector2D(float.NaN, float.NaN);
		}

		// This tests if the line intersects with the given line coordinates
		public static bool GetIntersection(Vector2D v1, Vector2D v2, float x3, float y3, float x4, float y4, out float u_ray, out float u_line)
		{
			return GetIntersection(v1, v2, x3, y3, x4, y4, out u_ray, out u_line, true);
		}

		// This tests if the line intersects with the given line coordinates
		public static bool GetIntersection(Vector2D v1, Vector2D v2, float x3, float y3, float x4, float y4, out float u_ray, out float u_line, bool bounded)
		{
			// Calculate divider
			float div = (y4 - y3) * (v2.x - v1.x) - (x4 - x3) * (v2.y - v1.y);

			// Can this be tested?
			if(div != 0.0f)
			{
				// Calculate the intersection distance from the line
				u_line = ((x4 - x3) * (v1.y - y3) - (y4 - y3) * (v1.x - x3)) / div;

				// Calculate the intersection distance from the ray
				u_ray = ((v2.x - v1.x) * (v1.y - y3) - (v2.y - v1.y) * (v1.x - x3)) / div;

				// Return if intersecting
				if(bounded && (u_ray < 0.0f || u_ray > 1.0f || u_line < 0.0f || u_line > 1.0f)) return false; //mxd
				return true;
			}

			// Unable to detect intersection
			u_line = float.NaN;
			u_ray = float.NaN;
			return false;
		}

		// This tests on which side of the line the given coordinates are
		// returns < 0 for front (right) side, > 0 for back (left) side and 0 if on the line
		public static float GetSideOfLine(Vector2D v1, Vector2D v2, Vector2D p)
		{
			// Calculate and return side information
			return (p.y - v1.y) * (v2.x - v1.x) - (p.x - v1.x) * (v2.y - v1.y);
		}

		// This returns the shortest distance from given coordinates to line
		public static float GetDistanceToLine(Vector2D v1, Vector2D v2, Vector2D p, bool bounded)
		{
			return (float)Math.Sqrt(GetDistanceToLineSq(v1, v2, p, bounded));
		}

		// This returns the shortest distance from given coordinates to line
		public static float GetDistanceToLineSq(Vector2D v1, Vector2D v2, Vector2D p, bool bounded)
		{
			// Calculate intersection offset
			float u = ((p.x - v1.x) * (v2.x - v1.x) + (p.y - v1.y) * (v2.y - v1.y)) / GetLengthSq(v2.x - v1.x, v2.y - v1.y);

			if(bounded)
			{
				/*
				// Limit intersection offset to the line
				float lbound = 1f / GetLength(v2.x - v1.x, v2.y - v1.y);
				float ubound = 1f - lbound;
				if(u < lbound) u = lbound;
				if(u > ubound) u = ubound;
				*/
				if(u < 0f) u = 0f; else if(u > 1f) u = 1f;
			}

			// Calculate intersection point
			Vector2D i = v1 + u * (v2 - v1);

			// Return distance between intersection and point
			// which is the shortest distance to the line
			float ldx = p.x - i.x;
			float ldy = p.y - i.y;
			return ldx * ldx + ldy * ldy;
		}

		// This returns the offset coordinates on the line nearest to the given coordinates
		public static float GetNearestOnLine(Vector2D v1, Vector2D v2, Vector2D p)
		{
			// Calculate and return intersection offset
			return ((p.x - v1.x) * (v2.x - v1.x) + (p.y - v1.y) * (v2.y - v1.y)) / GetLengthSq(v2.x - v1.x, v2.y - v1.y);
		}

		// This returns the coordinates at a specific position on the line
		public static Vector2D GetCoordinatesAt(Vector2D v1, Vector2D v2, float u)
		{
			// Calculate and return intersection offset
			return new Vector2D(v1.x + u * (v2.x - v1.x), v1.y + u * (v2.y - v1.y));
		}

		// Cohen-Sutherland algorithm
		public static Line2D ClipToRectangle(Line2D line, RectangleF rect, out bool intersects)
		{
			int flags1 = MapSet.GetCSFieldBits(line.v1, rect);
			int flags2 = MapSet.GetCSFieldBits(line.v2, rect);
			intersects = false;

			Line2D result = line;
			while (true)
			{
				if (flags1 == 0 && flags2 == 0)
				{
					// Line is fully inside the box
					intersects = true;
					return result;
				}
				
				if ((flags1 & flags2) != 0)
				{
					// Both points are in the same outer area
					intersects = false;
					return new Line2D();
				}

				float x, y;
				int outFlags = flags1 != 0 ? flags1 : flags2;
				if ((outFlags & 0x1) > 0) // Top
				{
					x = result.v1.x + (result.v2.x - result.v1.x) * (rect.Top - result.v1.y) / (result.v2.y - result.v1.y);
					y = rect.Top;
				}
				else if ((outFlags & 0x2) > 0) // Bottom
				{
					x = result.v1.x + (result.v2.x - result.v1.x) * (rect.Bottom - result.v1.y) / (result.v2.y - result.v1.y);
					y = rect.Bottom;
				}
				else if ((outFlags & 0x4) > 0) // Left
				{
					y = result.v1.y + (result.v2.y - result.v1.y) * (rect.Left - result.v1.x) / (result.v2.x - result.v1.x);
					x = rect.Left;
				}
				else if ((outFlags & 0x8) > 0) // Right
				{
					y = result.v1.y + (result.v2.y - result.v1.y) * (rect.Right - result.v1.x) / (result.v2.x - result.v1.x);
					x = rect.Right;
				} 
				else
				{
					intersects = true;
					return result;
				}

				if (outFlags == flags1)
				{
					result.v1 = new Vector2D(x, y);
					flags1 = MapSet.GetCSFieldBits(result.v1, rect);
				}
				else
				{
					result.v2 = new Vector2D(x, y);
					flags2 = MapSet.GetCSFieldBits(result.v2, rect);
				}
			}
		}
		
		#endregion

		#region ================== Methods

		// This returns the perpendicular vector by simply making a normal
		public Vector2D GetPerpendicular()
		{
			Vector2D d = GetDelta();
			return new Vector2D(-d.y, d.x);
		}

		// This calculates the angle
		public float GetAngle()
		{
			// Calculate and return the angle
			Vector2D d = GetDelta();
			return -(float)Math.Atan2(-d.y, d.x) + Angle2D.PIHALF;
		}
		
		public Vector2D GetDelta() { return v2 - v1; }
		
		public float GetLength() { return Line2D.GetLength(v2.x - v1.x, v2.y - v1.y); }
		public float GetLengthSq() { return Line2D.GetLengthSq(v2.x - v1.x, v2.y - v1.y); }

		// Output
		public override string ToString()
		{
			return "(" + v1 + ") - (" + v2 + ")";
		}
		
		public bool GetIntersection(float x3, float y3, float x4, float y4)
		{
			return Line2D.GetIntersection(v1, v2, x3, y3, x4, y4);
		}

		public bool GetIntersection(float x3, float y3, float x4, float y4, out float u_ray)
		{
			return Line2D.GetIntersection(v1, v2, x3, y3, x4, y4, out u_ray, true);
		}

		public bool GetIntersection(float x3, float y3, float x4, float y4, out float u_ray, bool bounded)
		{
			return Line2D.GetIntersection(v1, v2, x3, y3, x4, y4, out u_ray, bounded);
		}

		public bool GetIntersection(float x3, float y3, float x4, float y4, out float u_ray, out float u_line)
		{
			return Line2D.GetIntersection(v1, v2, x3, y3, x4, y4, out u_ray, out u_line);
		}

		public bool GetIntersection(Line2D ray)
		{
			return Line2D.GetIntersection(v1, v2, ray.v1.x, ray.v1.y, ray.v2.x, ray.v2.y);
		}

		public bool GetIntersection(Line2D ray, out float u_ray)
		{
			return Line2D.GetIntersection(v1, v2, ray.v1.x, ray.v1.y, ray.v2.x, ray.v2.y, out u_ray, true);
		}

		public bool GetIntersection(Line2D ray, out float u_ray, bool bounded)
		{
			return Line2D.GetIntersection(v1, v2, ray.v1.x, ray.v1.y, ray.v2.x, ray.v2.y, out u_ray, bounded);
		}

		public bool GetIntersection(Line2D ray, out float u_ray, out float u_line)
		{
			return Line2D.GetIntersection(v1, v2, ray.v1.x, ray.v1.y, ray.v2.x, ray.v2.y, out u_ray, out u_line);
		}

		public float GetSideOfLine(Vector2D p)
		{
			return Line2D.GetSideOfLine(v1, v2, p);
		}

		public float GetDistanceToLine(Vector2D p, bool bounded)
		{
			return Line2D.GetDistanceToLine(v1, v2, p, bounded);
		}

		public float GetDistanceToLineSq(Vector2D p, bool bounded)
		{
			return Line2D.GetDistanceToLineSq(v1, v2, p, bounded);
		}

		public float GetNearestOnLine(Vector2D p)
		{
			return Line2D.GetNearestOnLine(v1, v2, p);
		}

		public Vector2D GetCoordinatesAt(float u)
		{
			return Line2D.GetCoordinatesAt(v1, v2, u);
		}

		public Line2D GetTransformed(float offsetx, float offsety, float scalex, float scaley)
		{
			return new Line2D(v1.GetTransformed(offsetx, offsety, scalex, scaley),
				v2.GetTransformed(offsetx, offsety, scalex, scaley));
		}

		// Inverse Transform
		public Line2D GetInvTransformed(float invoffsetx, float invoffsety, float invscalex, float invscaley)
		{
			return new Line2D(v1.GetInvTransformed(invoffsetx, invoffsety, invscalex, invscaley), 
				v2.GetInvTransformed(invoffsetx, invoffsety, invscalex, invscaley));
		}
		
		#endregion
	}
}
