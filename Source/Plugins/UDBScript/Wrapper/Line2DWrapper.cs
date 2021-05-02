using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Geometry;

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	internal struct Line2DWrapper
	{
		#region ================== Variables

		public Vector2DWrapper v1;
		public Vector2DWrapper v2;

		#endregion

		#region ================== Constructors

		internal Line2DWrapper(Line2D line)
		{
			v1 = new Vector2DWrapper(line.v1);
			v2 = new Vector2DWrapper(line.v2);
		}

		/// <summary>
		/// Creates a new `Line2D` from two points
		/// ```
		/// let line1 = new Line2D(new Vector2D(32, 64), new Vector2D(96, 128));
		/// let line2 = new Line2D([ 32, 64 ], [ 96, 128 ]);
		/// ```
		/// </summary>
		/// <param name="v1">Vector of first point</param>
		/// <param name="v2">Vector of second point</param>
		public Line2DWrapper(object v1, object v2)
		{
			try
			{
				this.v1 = new Vector2DWrapper((Vector2D)BuilderPlug.Me.GetVectorFromObject(v1, false));
				this.v2 = new Vector2DWrapper((Vector2D)BuilderPlug.Me.GetVectorFromObject(v1, false));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		#endregion

		#region ================== Internals

		internal Line2D AsLine2D()
		{
			return new Line2D(v1.x, v1.y, v2.x, v2.y);
		}

		#endregion

		#region ================== Statics

		/// <summary>
		/// Checks if two lines intersect. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="line1">First line</param>
		/// <param name="line2">Second line</param>
		/// <param name="bounded">`true` to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>`true` if the lines intersect, `false` if they do not</returns>
		public static bool areIntersecting(Line2DWrapper line1, Line2DWrapper line2, bool bounded=true)
		{
			double u_ray;
			return Line2D.GetIntersection(line1.v1.AsVector2D(), line1.v2.AsVector2D(), line2.v1.x, line2.v1.y, line2.v2.x, line2.v2.y, out u_ray, bounded);
		}

		/// <summary>
		/// Checks if two lines defined by their start and end points intersect. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="a1">First point of first line</param>
		/// <param name="a2">Second point of first line</param>
		/// <param name="b1">First point of second line</param>
		/// <param name="b2">Second point of second line</param>
		/// <param name="bounded">`true` (default) to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>`true` if the lines intersect, `false` if they do not</returns>
		public static bool areIntersecting(object a1, object a2, object b1, object b2, bool bounded=true)
		{
			try
			{
				Vector2D v1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a1, false);
				Vector2D v2 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a2, false);
				Vector2D v3 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b1, false);
				Vector2D v4 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b2, false);
				double u_ray;

				return Line2D.GetIntersection(v1, v2, v3.x, v3.y, v4.x, v4.y, out u_ray, bounded);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the intersection point of two lines as `Vector2D`. If the lines do not intersect the `x`
		/// and `y` properties of the `Vector2D` are `NaN`. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="a1">First point of first line</param>
		/// <param name="a2">Second point of first line</param>
		/// <param name="b1">First point of second line</param>
		/// <param name="b2">Second point of second line</param>
		/// <param name="bounded">`true` (default) to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>The intersection point as `Vector2D`</returns>
		public static Vector2DWrapper getIntersectionPoint(object a1, object a2, object b1, object b2, bool bounded = true)
		{
			try
			{
				Vector2D v1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a1, false);
				Vector2D v2 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a2, false);
				Vector2D v3 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b1, false);
				Vector2D v4 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b2, false);

				return new Vector2DWrapper(Line2D.GetIntersectionPoint(new Line2D(v1, v2), new Line2D(v3, v4), bounded));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns which the of the line defined by its start and end point a given point is on
		/// </summary>
		/// <param name="v1">First point of the line</param>
		/// <param name="v2">Second point of the line</param>
		/// <param name="p">Point to check</param>
		/// <returns>`&lt; 0` if `p` is on the front (right) side, `&gt; 0` if `p` is on the back (left) side, `== 0` if `p` in on the line</returns>
		public static double getSideOfLine(object v1, object v2, object p)
		{
			try
			{
				Vector2D v11 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v1, false);
				Vector2D v21 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v2, false);
				Vector2D p1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p, false);

				return Line2D.GetSideOfLine(v11, v21, p1);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the shortest distance from point `p` to the line defined by its start and end points. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="v1">First point of the line</param>
		/// <param name="v2">Second point of the line</param>
		/// <param name="p">Point to get the distance to</param>
		/// <param name="bounded">`true` (default) to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>The shortest distance to the line</returns>
		public static double getDistanceToLine(object v1, object v2, object p, bool bounded=true)
		{
			try
			{
				Vector2D v11 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v1, false);
				Vector2D v21 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v2, false);
				Vector2D p1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p, false);

				return Line2D.GetDistanceToLine(v11, v21, p1, bounded);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the shortest square distance from point `p` to the line defined by its start and end points. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="v1">First point of the line</param>
		/// <param name="v2">Second point of the line</param>
		/// <param name="p">Point to get the distance to</param>
		/// <param name="bounded">`true` (default) to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>The shortest square distance to the line</returns>
		public static double getDistanceToLineSq(object v1, object v2, object p, bool bounded = true)
		{
			try
			{
				Vector2D v11 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v1, false);
				Vector2D v21 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v2, false);
				Vector2D p1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p, false);

				return Line2D.GetDistanceToLineSq(v11, v21, p1, bounded);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the offset coordinate on the line nearest to the given point. `0.0` being on the first point, `1.0` being on the second point, and `u = 0.5` being in
		/// the middle between the points
		/// </summary>
		/// <param name="v1">First point of the line</param>
		/// <param name="v2">Second point of the line</param>
		/// <param name="p">Point to get the nearest offset coordinate from</param>
		/// <returns>The offset value relative to the first point of the line.</returns>
		public static double getNearestOnLine(object v1, object v2, object p)
		{
			try
			{
				Vector2D v11 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v1, false);
				Vector2D v21 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v2, false);
				Vector2D p1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p, false);

				return Line2D.GetNearestOnLine(v11, v21, p1);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the coordinate on a line defined by its start and end points as `Vector2D`
		/// </summary>
		/// <param name="v1">First point of the line</param>
		/// <param name="v2">Second point of the line</param>
		/// <param name="u">Offset coordinate relative to the first point of the line</param>
		/// <returns>Point on the line as `Vector2D`</returns>
		public static Vector2DWrapper getCoordinatesAt(object v1, object v2, double u)
		{
			try
			{
				Vector2D v11 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v1, false);
				Vector2D v21 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v2, false);

				return new Vector2DWrapper(Line2D.GetCoordinatesAt(v11, v21, u));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Returns the coordinates on the line, where `u` is the position between the first and second point,
		/// `u = 0.0` being on the first point, `u = 1.0` being on the second point, and `u = 0.5` being in
		/// the middle between the points
		/// </summary>
		/// <param name="u">Position on the line, between 0.0 and 1.0</param>
		/// <returns>Position on the line as `Vector2D`</returns>
		public Vector2DWrapper getCoordinatesAt(double u)
		{
			return new Vector2DWrapper(new Line2D(v1.x, v1.y, v2.x, v2.y).GetCoordinatesAt(u));
		}

		/// <summary>
		/// Returns the length of the vector
		/// </summary>
		/// <returns>Length of the vector</returns>
		public double getLength()
		{
			return Line2D.GetLength(v2.x - v1.x, v2.y - v1.y);
		}

		/// <summary>
		/// Returns the angle of the vector in radians
		/// </summary>
		/// <returns>Angle of vector in radians</returns>
		public double getAngleRad()
		{
			return new Line2D(v1.AsVector2D(), v2.AsVector2D()).GetAngle();
		}
		
		/// <summary>
		/// Return the angle of the vector in degrees
		/// </summary>
		/// <returns>Angle of the vector in degrees</returns>
		public double getAngle()
		{
			return Angle2D.RadToDeg(new Line2D(v1.AsVector2D(), v2.AsVector2D()).GetAngle());
		}

		/// <summary>
		/// Returns the perpendicular line of this line as `Vector2D`
		/// </summary>
		/// <returns>Perpendicular line of this line as `Vector2D`</returns>
		public Vector2DWrapper getPerpendiculat()
		{
			return new Vector2DWrapper(new Line2D(v1.x, v1.y, v2.x, v2.y).GetPerpendicular());
		}

		/// <summary>
		/// Checks if the given line intersects this line. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="ray">`Line2D` to check against</param>
		/// <param name="bounded">`true` (default) to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>`true` if lines intersect, `false` if they do not intersect</returns>
		public bool isIntersecting(Line2DWrapper ray, bool bounded=true)
		{
			double u_ray;
			return AsLine2D().GetIntersection(ray.v1.x, ray.v1.y, ray.v2.x, ray.v2.y, out u_ray, bounded);
		}

		/// <summary>
		/// Checks if the given line intersects this line. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="a1">First point of the line to check against</param>
		/// <param name="a2">Second point of the line to check against</param>
		/// <param name="bounded">`true` (default) to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>`true` if the lines intersect, `false` if they do not</returns>
		public bool isIntersecting(object a1, object a2, bool bounded = true)
		{
			try
			{
				Vector2D v3 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a1, false);
				Vector2D v4 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a2, false);
				double u_ray;

				return AsLine2D().GetIntersection(v3.x, v3.y, v4.x, v4.y, out u_ray, bounded);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the intersection point of of the given line defined by its start and end points with this line as `Vector2D`. If the lines do not intersect the `x`
		/// and `y` properties of the `Vector2D` are `NaN`. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="a1">First point of first line</param>
		/// <param name="a2">Second point of first line</param>
		/// <param name="bounded">`true` (default) to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>The intersection point as `Vector2D`</returns>
		public Vector2DWrapper getIntersectionPoint(object a1, object a2, bool bounded = true)
		{
			try
			{
				Vector2D v3 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a1, false);
				Vector2D v4 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a2, false);
				Line2D line = AsLine2D();
				double u_ray;
				line.GetIntersection(v3.x, v3.y, v4.x, v4.y, out u_ray, bounded);

				return new Vector2DWrapper(line.GetCoordinatesAt(u_ray));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the intersection point of of the given line with this line as `Vector2D`. If the lines do not intersect the `x`
		/// and `y` properties of the `Vector2D` are `NaN`. If `bounded` is set to `true` (default) the finite length of the lines
		/// is used, otherwise the infinite length of the lines is used
		/// </summary>
		/// <param name="ray">Other line to get the intersection point from</param>
		/// <param name="bounded">`true` (default) to use finite length of lines, `false` to use infinite length of lines</param>
		/// <returns>The intersection point as `Vector2D`</returns>
		public Vector2DWrapper getIntersectionPoint(Line2DWrapper ray, bool bounded=true)
		{
			Line2D thisline = AsLine2D();
			Line2D otherline = ray.AsLine2D();
			double u_ray;
			thisline.GetIntersection(otherline, out u_ray, bounded);

			return new Vector2DWrapper(thisline.GetCoordinatesAt(u_ray));
		}

		/// <summary>
		/// Returns which the of the line defined by its start and end point a given point is on
		/// </summary>
		/// <param name="p">Point to check</param>
		/// <returns>`&lt; 0` if `p` is on the front (right) side, `&gt; 0` if `p` is on the back (left) side, `== 0` if `p` in on the line</returns>
		public double getSideOfLine(object p)
		{
			try
			{
				Vector2D p1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p, false);

				return AsLine2D().GetSideOfLine(p1);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		public override string ToString()
		{
			return AsLine2D().ToString();
		}

		#endregion
	}
}
