using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Geometry
{
	/// <summary>
	/// mxd. Tools to work with curves.
	/// </summary>
	public static class CurveTools
	{
		//mxd. Ported from Cubic Bezier curve tools by Andy Woodruff (http://cartogrammar.com/source/CubicBezier.as)
		//"default" values: z = 0.5, angleFactor = 0.75; if targetSegmentLength <= 0, will return lines
		public static Curve CurveThroughPoints(List<Vector2D> points, float z, float angleFactor, int targetSegmentLength) 
		{
			Curve result = new Curve();

			// First calculate all the curve control points
			// None of this junk will do any good if there are only two points
			if(points.Count > 2 && targetSegmentLength > 0) 
			{
				List<List<Vector2D>> controlPts = new List<List<Vector2D>>();	// An array to store the two control points (of a cubic Bézier curve) for each point

				// Make sure z is between 0 and 1 (too messy otherwise)
				if(z <= 0) z = 0.1f;
				else if(z > 1) z = 1;

				// Make sure angleFactor is between 0 and 1
				if(angleFactor < 0) angleFactor = 0;
				else if(angleFactor > 1) angleFactor = 1;

				// Ordinarily, curve calculations will start with the second point and go through the second-to-last point
				int firstPt = 1;
				int lastPt = points.Count - 1;

				// Check if this is a closed line (the first and last points are the same)
				if(points[0].x == points[points.Count - 1].x && points[0].y == points[points.Count - 1].y) 
				{
					// Include first and last points in curve calculations
					firstPt = 0;
					lastPt = points.Count;
				} 
				else 
				{
					controlPts.Add(new List<Vector2D>()); //add a dummy entry
				}

				// Loop through all the points (except the first and last if not a closed line) to get curve control points for each.
				for(int i = firstPt; i < lastPt; i++) 
				{
					// The previous, current, and next points
					Vector2D p0 = (i - 1 < 0) ? points[points.Count - 2] : points[i - 1];	// If the first point (of a closed line), use the second-to-last point as the previous point
					Vector2D p1 = points[i];
					Vector2D p2 = (i + 1 == points.Count) ? points[1] : points[i + 1];		// If the last point (of a closed line), use the second point as the next point

					float a = Vector2D.Distance(p0, p1);	// Distance from previous point to current point
					if(a < 0.001) a = 0.001f;		        // Correct for near-zero distances, a cheap way to prevent division by zero
					float b = Vector2D.Distance(p1, p2);	// Distance from current point to next point
					if(b < 0.001) b = 0.001f;
					float c = Vector2D.Distance(p0, p2);	// Distance from previous point to next point
					if(c < 0.001) c = 0.001f;

					float cos = (b * b + a * a - c * c) / (2 * b * a);
					// Make sure above value is between -1 and 1 so that Math.acos will work
					if(cos < -1) cos = -1;
					else if(cos > 1) cos = 1;

					float C = (float)Math.Acos(cos); // Angle formed by the two sides of the triangle (described by the three points above) adjacent to the current point

					// Duplicate set of points. Start by giving previous and next points values RELATIVE to the current point.
					Vector2D aPt = new Vector2D(p0.x - p1.x, p0.y - p1.y);
					Vector2D bPt = new Vector2D(p1.x, p1.y);
					Vector2D cPt = new Vector2D(p2.x - p1.x, p2.y - p1.y);

					/*
					We'll be adding adding the vectors from the previous and next points to the current point,
					but we don't want differing magnitudes (i.e. line segment lengths) to affect the direction
					of the new vector. Therefore we make sure the segments we use, based on the duplicate points
					created above, are of equal length. The angle of the new vector will thus bisect angle C
					(defined above) and the perpendicular to this is nice for the line tangent to the curve.
					The curve control points will be along that tangent line.
					*/
					if(a > b) aPt = aPt.GetNormal() * b;	    // Scale the segment to aPt (bPt to aPt) to the size of b (bPt to cPt) if b is shorter.
					else if(b > a) cPt = cPt.GetNormal() * a;	// Scale the segment to cPt (bPt to cPt) to the size of a (aPt to bPt) if a is shorter.

					// Offset aPt and cPt by the current point to get them back to their absolute position.
					aPt += p1;
					cPt += p1;

					// Get the sum of the two vectors, which is perpendicular to the line along which our curve control points will lie.
					float ax = bPt.x - aPt.x;	// x component of the segment from previous to current point
					float ay = bPt.y - aPt.y;
					float bx = bPt.x - cPt.x;	// x component of the segment from next to current point
					float by = bPt.y - cPt.y;
					float rx = ax + bx;	// sum of x components
					float ry = ay + by;

					// Correct for three points in a line by finding the angle between just two of them
					if(rx == 0 && ry == 0) 
					{
						rx = -bx;	// Really not sure why this seems to have to be negative
						ry = by;
					}

					// Switch rx and ry when y or x difference is 0. This seems to prevent the angle from being perpendicular to what it should be.
					if(ay == 0 && by == 0) 
					{
						rx = 0;
						ry = 1;
					} 
					else if(ax == 0 && bx == 0) 
					{
						rx = 1;
						ry = 0;
					}

					//float r = (float)Math.Sqrt(rx * rx + ry * ry);	// length of the summed vector - not being used, but there it is anyway
					float theta = (float)Math.Atan2(ry, rx);	// angle of the new vector

					float controlDist = Math.Min(a, b) * z;	// Distance of curve control points from current point: a fraction the length of the shorter adjacent triangle side
					float controlScaleFactor = C / Angle2D.PI;	// Scale the distance based on the acuteness of the angle. Prevents big loops around long, sharp-angled triangles.
					controlDist *= ((1 - angleFactor) + angleFactor * controlScaleFactor);	// Mess with this for some fine-tuning
					float controlAngle = theta + Angle2D.PIHALF;	// The angle from the current point to control points: the new vector angle plus 90 degrees (tangent to the curve).

					Vector2D controlPoint2 = new Vector2D(controlDist, 0);
					Vector2D controlPoint1 = new Vector2D(controlDist, 0);
					controlPoint2 = controlPoint2.GetRotated(controlAngle);
					controlPoint1 = controlPoint1.GetRotated(controlAngle + Angle2D.PI);

					// Offset control points to put them in the correct absolute position
					controlPoint1 += p1;
					controlPoint2 += p1;

					/*
					Haven't quite worked out how this happens, but some control points will be reversed.
					In this case controlPoint2 will be farther from the next point than controlPoint1 is.
					Check for that and switch them if it's true.
					*/
					if(Vector2D.Distance(controlPoint2, p2) > Vector2D.Distance(controlPoint1, p2))
						controlPts.Add(new List<Vector2D> { controlPoint2, controlPoint1 });
					else
						controlPts.Add(new List<Vector2D> { controlPoint1, controlPoint2 });
				}

				// If this isn't a closed line, draw a regular quadratic Bézier curve from the first to second points, using the first control point of the second point
				if(firstPt == 1) 
				{
					float length = (points[1] - points[0]).GetLength();
					int numSteps = Math.Max(1, (int)Math.Round(length / targetSegmentLength));
					CurveSegment segment = new CurveSegment();
					segment.Start = points[0];
					segment.CPMid = controlPts[1][0];
					segment.End = points[1];
					CreateQuadraticCurve(segment, numSteps);

					result.Segments.Add(segment);
				}

				// Loop through points to draw cubic Bézier curves through the penultimate point, or through the last point if the line is closed.
				for(int i = firstPt; i < lastPt - 1; i++) 
				{
					float length = (points[i + 1] - points[i]).GetLength();
					int numSteps = Math.Max(1, (int)Math.Round(length / targetSegmentLength));

					CurveSegment segment = new CurveSegment();
					segment.CPStart = controlPts[i][1];
					segment.CPEnd = controlPts[i + 1][0];
					segment.Start = points[i];
					segment.End = points[i + 1];
					CreateCubicCurve(segment, numSteps);

					result.Segments.Add(segment);
				}

				// If this isn't a closed line, curve to the last point using the second control point of the penultimate point.
				if(lastPt == points.Count - 1) 
				{
					float length = (points[lastPt] - points[lastPt - 1]).GetLength();
					int numSteps = Math.Max(1, (int)Math.Round(length / targetSegmentLength));

					CurveSegment segment = new CurveSegment();
					segment.Start = points[lastPt - 1];
					segment.CPMid = controlPts[lastPt - 1][1];
					segment.End = points[lastPt];
					CreateQuadraticCurve(segment, numSteps);

					result.Segments.Add(segment);
				}

				// create lines
			} 
			else if(points.Count >= 2) 
			{
				for(int i = 0; i < points.Count - 1; i++) 
				{
					CurveSegment segment = new CurveSegment();
					segment.Start = points[i];
					segment.End = points[i + 1];
					segment.Points = new[] { segment.Start, segment.End };
					result.Segments.Add(segment);
				}
			}

			result.UpdateShape();
			return result;
		}

		public static void CreateQuadraticCurve(CurveSegment segment, int steps) 
		{
			segment.CurveType = CurveSegmentType.QUADRATIC;
			segment.Points = GetQuadraticCurve(segment.Start, segment.CPMid, segment.End, steps);
		}

		//this returns array of Vector2D to draw 3-point bezier curve
		public static Vector2D[] GetQuadraticCurve(Vector2D p1, Vector2D p2, Vector2D p3, int steps) 
		{
			if(steps < 0) return null;

			int totalSteps = steps + 1;
			Vector2D[] points = new Vector2D[totalSteps];
			float step = 1f / steps;
			float curStep = 0f;

			for(int i = 0; i < totalSteps; i++) 
			{
				points[i] = GetPointOnQuadraticCurve(p1, p2, p3, curStep);
				curStep += step;
			}

			return points;
		}

		public static void CreateCubicCurve(CurveSegment segment, int steps) 
		{
			segment.CurveType = CurveSegmentType.CUBIC;
			segment.Points = GetCubicCurve(segment.Start, segment.End, segment.CPStart, segment.CPEnd, steps);
		}

		//this returns array of Vector2D to draw 4-point bezier curve
		public static Vector2D[] GetCubicCurve(Vector2D p1, Vector2D p2, Vector2D cp1, Vector2D cp2, int steps) 
		{
			if(steps < 0) return null;

			int totalSteps = steps + 1;
			Vector2D[] points = new Vector2D[totalSteps];
			float step = 1f / steps;
			float curStep = 0f;

			for(int i = 0; i < totalSteps; i++) 
			{
				points[i] = GetPointOnCubicCurve(p1, p2, cp1, cp2, curStep);
				curStep += step;
			}
			return points;
		}

		public static Vector2D GetPointOnCurve(CurveSegment segment, float delta) 
		{
			if(segment.CurveType == CurveSegmentType.QUADRATIC)
				return GetPointOnQuadraticCurve(segment.Start, segment.CPMid, segment.End, delta);

			if(segment.CurveType == CurveSegmentType.CUBIC)
				return GetPointOnCubicCurve(segment.Start, segment.End, segment.CPStart, segment.CPEnd, delta);

			if(segment.CurveType == CurveSegmentType.LINE)
				return GetPointOnLine(segment.Start, segment.End, delta);

			throw new Exception("GetPointOnCurve: got unknown curve type: " + segment.CurveType);
		}

		public static Vector2D GetPointOnQuadraticCurve(Vector2D p1, Vector2D p2, Vector2D p3, float delta) 
		{
			float invDelta = 1f - delta;

			float m1 = invDelta * invDelta;
			float m2 = 2 * invDelta * delta;
			float m3 = delta * delta;

			float px = (m1 * p1.x + m2 * p2.x + m3 * p3.x);
			float py = (m1 * p1.y + m2 * p2.y + m3 * p3.y);

			return new Vector2D(px, py);
		}

		public static Vector2D GetPointOnCubicCurve(Vector2D p1, Vector2D p2, Vector2D cp1, Vector2D cp2, float delta) 
		{
			float invDelta = 1f - delta;

			float m1 = invDelta * invDelta * invDelta;
			float m2 = 3 * delta * invDelta * invDelta;
			float m3 = 3 * delta * delta * invDelta;
			float m4 = delta * delta * delta;

			float px = (m1 * p1.x + m2 * cp1.x + m3 * cp2.x + m4 * p2.x);
			float py = (m1 * p1.y + m2 * cp1.y + m3 * cp2.y + m4 * p2.y);

			return new Vector2D(px, py);
		}

		//it's basically 2-point bezier curve
		public static Vector2D GetPointOnLine(Vector2D p1, Vector2D p2, float delta) 
		{
			return new Vector2D((int)((1f - delta) * p1.x + delta * p2.x), (int)((1f - delta) * p1.y + delta * p2.y));
		}
	}

	public class Curve
	{
		public List<CurveSegment> Segments;
		public List<Vector2D> Shape;

		public Curve() 
		{
			Segments = new List<CurveSegment>();
		}

		public void UpdateShape() 
		{
			Shape = new List<Vector2D>();
			foreach(CurveSegment segment in Segments) 
			{
				foreach(Vector2D point in segment.Points) 
				{
					if(Shape.Count == 0 || point != Shape[Shape.Count - 1])
						Shape.Add(point);
				}
			}
		}
	}

	public class CurveSegment
	{
		public Vector2D[] Points;
		public Vector2D Start;
		public Vector2D End;
		public Vector2D CPStart;
		public Vector2D CPMid;
		public Vector2D CPEnd;
		public CurveSegmentType CurveType;
	}

	public enum CurveSegmentType
	{
		LINE,
		QUADRATIC,
		CUBIC,
	}
}
