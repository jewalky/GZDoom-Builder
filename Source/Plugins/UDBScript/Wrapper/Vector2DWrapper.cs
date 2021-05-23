using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Geometry;

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	internal struct Vector2DWrapper
	{
		#region ================== Variables

		public double x;
		public double y;

		#endregion

		#region ================== Constructors

		internal Vector2DWrapper(Vector2D v)
		{
			x = v.x;
			y = v.y;
		}

		/// <summary>
		/// Creates a new `Vector2D` from x and y coordinates
		/// ```
		/// let v = new Vector2D(32, 64);
		/// ```
		/// </summary>
		/// <param name="x">The x coordinate</param>
		/// <param name="y">The y coordinate</param>
		public Vector2DWrapper(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		/// <summary>
		/// Creates a new `Vector2D` from a point.
		/// ```
		/// let v = new Vector2D([ 32, 64 ]);
		/// ```
		/// </summary>
		/// <param name="v">The vector to create the `Vector2D` from</param>
		public Vector2DWrapper(object v)
		{
			try
			{
				Vector2D v1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v, false);

				x = v1.x;
				y = v1.y;
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		#endregion

		#region ================== Internal

		internal Vector2D AsVector2D()
		{
			return new Vector2D(x, y);
		}

		#endregion

		#region ================== Statics

		/// <summary>
		/// Returns the dot product of two `Vector2D`s.
		/// </summary>
		/// <param name="a">First `Vector2D`</param>
		/// <param name="b">Second `Vector2D`</param>
		/// <returns>The dot product of the two vectors</returns>
		public static double dotProduct(Vector2DWrapper a, Vector2DWrapper b)
		{
			// Calculate and return the dot product
			return a.x * b.x + a.y * b.y;
		}

		/// <summary>
		/// Returns the cross product of two `Vector2D`s.
		/// </summary>
		/// <param name="a">First `Vector2D`</param>
		/// <param name="b">Second `Vector2D`</param>
		/// <returns>Cross product of the two vectors as `Vector2D`</returns>
		public static Vector2DWrapper crossProduct(object a, object b)
		{
			try
			{
				Vector2D a1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a, false);
				Vector2D b1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b, false);

				return new Vector2DWrapper(a1.y * b1.x, a1.x * b1.y);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Reflects a `Vector2D` over a mirror `Vector2D`.
		/// </summary>
		/// <param name="v">`Vector2D` to reflect</param>
		/// <param name="m">Mirror `Vector2D`</param>
		/// <returns>The reflected vector as `Vector2D`</returns>
		public static Vector2DWrapper reflect(object v, object m)
		{
			try
			{
				Vector2D v1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v, false);
				Vector2D m1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(m, false);

				Vector2D mv = Vector2D.Reflect(v1, m1);

				return new Vector2DWrapper(mv.x, mv.y);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns a reversed `Vector2D`.
		/// </summary>
		/// <param name="v">`Vector2D` to reverse</param>
		/// <returns>The reversed vector as `Vector2D`</returns>
		public static Vector2DWrapper reversed(object v)
		{
			try
			{
				Vector2D v1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(v, false);

				return new Vector2DWrapper(Vector2D.Reversed(v1));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Creates a `Vector2D` from an angle in radians,
		/// </summary>
		/// <param name="angle">Angle in radians</param>
		/// <returns>Vector as `Vector2D`</returns>
		public static Vector2DWrapper fromAngleRad(double angle)
		{
			return new Vector2DWrapper(Vector2D.FromAngle(angle));
		}

		/// <summary>
		/// Creates a `Vector2D` from an angle in degrees,
		/// </summary>
		/// <param name="angle">Angle in degrees</param>
		/// <returns>Vector as `Vector2D`</returns>
		public static Vector2DWrapper fromAngle(double angle)
		{
			return new Vector2DWrapper(Vector2D.FromAngle(Angle2D.DegToRad(angle)));
		}

		/// <summary>
		/// Returns the angle between two `Vector2D`s in radians
		/// </summary>
		/// <param name="a">First `Vector2D`</param>
		/// <param name="b">Second `Vector2D`</param>
		/// <returns>Angle in radians</returns>
		public static double getAngleRad(object a, object b)
		{
			try
			{
				Vector2D a1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a, false);
				Vector2D b1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b, false);

				return Vector2D.GetAngle(a1, b1);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the angle between two `Vector2D`s in radians.
		/// </summary>
		/// <param name="a">First `Vector2D`</param>
		/// <param name="b">Second `Vector2D`</param>
		/// <returns>Angle in radians</returns>
		public static double getAngle(object a, object b)
		{
			try
			{
				Vector2D a1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a, false);
				Vector2D b1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b, false);

				return Angle2D.RadToDeg(Vector2D.GetAngle(a1, b1));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the square distance between two `Vector2D`s.
		/// </summary>
		/// <param name="a">First `Vector2D`</param>
		/// <param name="b">Second `Vector2D`</param>
		/// <returns>The squared distance</returns>
		public static double getDistanceSq(object a, object b)
		{
			try
			{
				Vector2D a1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a, false);
				Vector2D b1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b, false);

				return Vector2D.DistanceSq(a1, b1);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the distance between two `Vector2D`s.
		/// </summary>
		/// <param name="a">First `Vector2D`</param>
		/// <param name="b">Second `Vector2D`</param>
		/// <returns>The distance</returns>
		public static double getDistance(object a, object b)
		{
			try
			{
				Vector2D a1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(a, false);
				Vector2D b1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(b, false);

				return Vector2D.Distance(a1, b1);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Returns the perpendicular to the `Vector2D`.
		/// </summary>
		/// <returns>The perpendicular as `Vector2D`</returns>
		public Vector2DWrapper getPerpendicular()
		{
			return new Vector2DWrapper(-y, x);
		}

		/// <summary>
		/// Returns a `Vector2D` with the sign of all components.
		/// </summary>
		/// <returns>A `Vector2D` with the sign of all components</returns>
		public Vector2DWrapper getSign()
		{
			return new Vector2DWrapper(new Vector2D(x, y).GetSign());
		}

		/// <summary>
		/// Returns the angle of the `Vector2D` in radians.
		/// </summary>
		/// <returns>The angle of the `Vector2D` in radians</returns>
		public double getAngleRad()
		{
			return new Vector2D(x, y).GetAngle();
		}

		/// <summary>
		/// Returns the angle of the `Vector2D` in degree.
		/// </summary>
		/// <returns>The angle of the `Vector2D` in degree</returns>
		public double getAngle()
		{
			return Angle2D.RadToDeg(new Vector2D(x, y).GetAngle());
		}

		/// <summary>
		/// Returns the length of the `Vector2D`.
		/// </summary>
		/// <returns>The length of the `Vector2D`</returns>
		public double getLength()
		{
			return new Vector2D(x, y).GetLength();
		}

		/// <summary>
		/// Returns the square length of the `Vector2D`.
		/// </summary>
		/// <returns>The square length of the `Vector2D`</returns>
		public double getLengthSq()
		{
			return new Vector2D(x, y).GetLengthSq();
		}

		/// <summary>
		/// Returns the normal of the `Vector2D`.
		/// </summary>
		/// <returns>The normal as `Vector2D`</returns>
		public Vector2DWrapper getNormal()
		{
			return new Vector2DWrapper(new Vector2D(x, y).GetNormal());
		}

		/// <summary>
		/// Returns the transformed vector as `Vector2D`.
		/// </summary>
		/// <param name="offsetx">X offset</param>
		/// <param name="offsety">Y offset</param>
		/// <param name="scalex">X scale</param>
		/// <param name="scaley">Y scale</param>
		/// <returns>The transformed vector as `Vector2D`</returns>
		public Vector2DWrapper getTransformed(double offsetx, double offsety, double scalex, double scaley)
		{
			return new Vector2DWrapper(new Vector2D(x, y).GetTransformed(offsetx, offsety, scalex, scaley));
		}

		/// <summary>
		/// Returns the inverse transformed vector as `Vector2D`.
		/// </summary>
		/// <param name="invoffsetx">X offset</param>
		/// <param name="invoffsety">Y offset</param>
		/// <param name="invscalex">X scale</param>
		/// <param name="invscaley">Y scale</param>
		/// <returns>The inverse transformed vector as `Vector2D`</returns>
		public Vector2DWrapper getInverseTransformed(double invoffsetx, double invoffsety, double invscalex, double invscaley)
		{
			return new Vector2DWrapper(new Vector2D(x, y).GetInvTransformed(invoffsetx, invoffsety, invscalex, invscaley));
		}

		/// <summary>
		/// Returns the rotated vector as `Vector2D`.
		/// </summary>
		/// <param name="theta">Angle in degree to rotate by</param>
		/// <returns>The rotated `Vector2D`</returns>
		public Vector2DWrapper getRotated(double theta)
		{
			return new Vector2DWrapper(new Vector2D(x, y).GetRotated(Angle2D.DegToRad(theta)));
		}

		/// <summary>
		/// Returns the rotated vector as `Vector2D`.
		/// </summary>
		/// <param name="theta">Angle in radians to rotate by</param>
		/// <returns>The rotated `Vector2D`</returns>
		public Vector2DWrapper getRotatedRad(double theta)
		{
			return new Vector2DWrapper(new Vector2D(x, y).GetRotated(theta));
		}

		public override string ToString()
		{
			return new Vector2D(x, y).ToString();
		}

		#endregion
	}
}
