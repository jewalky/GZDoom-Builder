#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	internal struct Vector3DWrapper
	{
		#region ================== Variables

		public double x;
		public double y;
		public double z;

		#endregion

		#region ================== Constructors

		internal Vector3DWrapper(Vector3D v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}

		/// <summary>
		/// Creates a new `Vector3D` from x and y coordinates
		/// ```
		/// let v = new Vector3D(32, 64, 128);
		/// ```
		/// </summary>
		/// <param name="x">The x coordinate</param>
		/// <param name="y">The y coordinate</param>
		/// <param name="z">The z coordinate</param>
		public Vector3DWrapper(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Creates a new `Vector3D` from a point.
		/// ```
		/// let v = new Vector3D([ 32, 64, 128 ]);
		/// ```
		/// </summary>
		/// <param name="v">The vector to create the `Vector3D` from</param>
		public Vector3DWrapper(object v)
		{
			try
			{
				Vector3D v1 = (Vector3D)BuilderPlug.Me.GetVectorFromObject(v, true);

				x = v1.x;
				y = v1.y;
				z = v1.z;
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		#endregion

		#region ================== Internal

		internal Vector3D AsVector3D()
		{
			return new Vector3D(x, y, z);
		}

		#endregion

		#region ================== Statics

		/// <summary>
		/// Returns the dot product of two `Vector3D`s.
		/// </summary>
		/// <param name="a">First `Vector3D`</param>
		/// <param name="b">Second `Vector3D`</param>
		/// <returns>The dot product of the two vectors</returns>
		public static double dotProduct(Vector3DWrapper a, Vector3DWrapper b)
		{
			// Calculate and return the dot product
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		/// <summary>
		/// Returns the cross product of two `Vector3D`s.
		/// </summary>
		/// <param name="a">First `Vector3D`</param>
		/// <param name="b">Second `Vector3D`</param>
		/// <returns>Cross product of the two vectors as `Vector3D`</returns>
		public static Vector3DWrapper crossProduct(object a, object b)
		{
			try
			{
				Vector3D a1 = (Vector3D)BuilderPlug.Me.GetVectorFromObject(a, true);
				Vector3D b1 = (Vector3D)BuilderPlug.Me.GetVectorFromObject(b, true);

				return new Vector3DWrapper(a1.y * b1.x - a1.z * b1.y, a1.z * b1.x - a1.x * b1.z, a1.x * b1.y - a1.y * b1.x);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Reflects a `Vector3D` over a mirror `Vector3D`.
		/// </summary>
		/// <param name="v">`Vector3D` to reflect</param>
		/// <param name="m">Mirror `Vector3D`</param>
		/// <returns>The reflected vector as `Vector3D`</returns>
		public static Vector3DWrapper reflect(object v, object m)
		{
			try
			{
				Vector3D v1 = (Vector3D)BuilderPlug.Me.GetVectorFromObject(v, true);
				Vector3D m1 = (Vector3D)BuilderPlug.Me.GetVectorFromObject(m, true);

				return new Vector3DWrapper(Vector3D.Reflect(v1, m1));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns a reversed `Vector3D`.
		/// </summary>
		/// <param name="v">`Vector3D` to reverse</param>
		/// <returns>The reversed vector as `Vector3D`</returns>
		public static Vector3DWrapper reversed(object v)
		{
			try
			{
				Vector3D v1 = (Vector3D)BuilderPlug.Me.GetVectorFromObject(v, true);

				return new Vector3DWrapper(Vector3D.Reversed(v1));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Creates a `Vector3D` from an angle in radians
		/// </summary>
		/// <param name="angle">Angle on the x/y axes in radians</param>
		/// <returns>Vector as `Vector3D`</returns>
		public static Vector3DWrapper fromAngleXYRad(double angle)
		{
			return new Vector3DWrapper(Vector3D.FromAngleXY(angle));
		}

		/// <summary>
		/// Creates a `Vector3D` from an angle in radians,
		/// </summary>
		/// <param name="angle">Angle on the x/y axes in degrees</param>
		/// <returns>Vector as `Vector3D`</returns>
		public static Vector3DWrapper fromAngleXY(double angle)
		{
			return new Vector3DWrapper(Vector3D.FromAngleXY(Angle2D.DegToRad(angle)));
		}

		/// <summary>
		/// Creates a `Vector3D` from two angles in radians
		/// </summary>
		/// <param name="anglexy">Angle on the x/y axes in radians</param>
		/// <param name="anglez">Angle on the z axis in radians</param>
		/// <returns>Vector as `Vector3D`</returns>
		public static Vector3DWrapper fromAngleXYZRad(double anglexy, double anglez)
		{
			return new Vector3DWrapper(Vector3D.FromAngleXYZ(anglexy, anglez));
		}

		/// <summary>
		/// Creates a `Vector3D` from two angles in degrees
		/// </summary>
		/// <param name="anglexy">Angle on the x/y axes in radians</param>
		/// <param name="anglez">Angle on the z axis in radians</param>
		/// <returns>Vector as `Vector3D`</returns>
		public static Vector3DWrapper fromAngleXYZ(double anglexy, double anglez)
		{
			return new Vector3DWrapper(Vector3D.FromAngleXYZ(Angle2D.DegToRad(anglexy), Angle2D.DegToRad(anglez)));
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Returns the x/y angle of the `Vector3D` in radians.
		/// </summary>
		/// <returns>The x/y angle of the `Vector3D` in radians</returns>
		public double getAngleXYRad()
		{
			return new Vector3D(x, y, z).GetAngleXY();
		}

		/// <summary>
		/// Returns the angle of the `Vector3D` in degrees.
		/// </summary>
		/// <returns>The angle of the `Vector3D` in degrees</returns>
		public double getAngleXY()
		{
			return Angle2D.RadToDeg(new Vector3D(x, y, z).GetAngleXY());
		}

		/// <summary>
		/// Returns the z angle of the `Vector3D` in radians.
		/// </summary>
		/// <returns>The z angle of the `Vector3D` in radians</returns>
		public double getAngleZRad()
		{
			return new Vector3D(x, y, z).GetAngleZ();
		}

		/// <summary>
		/// Returns the z angle of the `Vector3D` in degrees.
		/// </summary>
		/// <returns>The z angle of the `Vector3D` in degrees</returns>
		public double getAngleZ()
		{
			return Angle2D.RadToDeg(new Vector3D(x, y, z).GetAngleZ());
		}

		/// <summary>
		/// Returns the length of the `Vector3D`.
		/// </summary>
		/// <returns>The length of the `Vector3D`</returns>
		public double getLength()
		{
			return new Vector3D(x, y, z).GetLength();
		}

		/// <summary>
		/// Returns the square length of the `Vector3D`.
		/// </summary>
		/// <returns>The square length of the `Vector3D`</returns>
		public double getLengthSq()
		{
			return new Vector3D(x, y, z).GetLengthSq();
		}

		/// <summary>
		/// Returns the normal of the `Vector3D`.
		/// </summary>
		/// <returns>The normal as `Vector3D`</returns>
		public Vector3DWrapper getNormal()
		{
			return new Vector3DWrapper(new Vector3D(x, y, z).GetNormal());
		}

		/// <summary>
		/// Return the scaled `Vector3D`.
		/// </summary>
		/// <param name="scale">Scale, where 1.0 is unscaled</param>
		/// <returns>The scaled `Vector3D`</returns>
		public Vector3DWrapper getScaled(double scale)
		{
			return new Vector3DWrapper(new Vector3D(x, y, z).GetScaled(scale));
		}

		/// <summary>
		/// Checks if the `Vector3D` is normalized or not.
		/// </summary>
		/// <returns>`true` if `Vector3D` is normalized, otherwise `false`</returns>
		public bool isNormalized()
		{
			return new Vector3D(x, y, z).IsNormalized();
		}

		/// <summary>
		/// Checks if the `Vector3D` is finite or not.
		/// </summary>
		/// <returns>`true` if `Vector3D` is finite, otherwise `false`</returns>
		public bool isFinite()
		{
			return new Vector3D(x, y, z).IsFinite();
		}

		public override string ToString()
		{
			return new Vector3D(x, y, z).ToString();
		}

		#endregion
	}
}