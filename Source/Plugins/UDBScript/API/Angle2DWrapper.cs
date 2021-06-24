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

namespace CodeImp.DoomBuilder.UDBScript.API
{
	internal struct Angle2DWrapper
	{
		/// <summary>
		/// Converts a Doom angle (where 0° is east) to a real world angle (where 0° is north).
		/// </summary>
		/// <param name="doomangle">Doom angle in degrees</param>
		/// <returns>Doom angle in degrees</returns>
		public static double doomToReal(int doomangle)
		{
			return normalized(doomangle + 90);
		}

		/// <summary>
		/// Converts a Doom angle (where 0° is east) to a real world angle (where 0° is north) in radians.
		/// </summary>
		/// <param name="doomangle">Doom angle in degrees</param>
		/// <returns>Doom angle in radians</returns>
		public static double doomToRealRad(int doomangle)
		{
			return Angle2D.DoomToReal(doomangle);
		}

		/// <summary>
		/// Converts a real world angle (where 0° is north) to a Doom angle (where 0° is east).
		/// </summary>
		/// <param name="realangle">Real world angle in degrees</param>
		/// <returns>Doom angle in degrees</returns>
		public static int realToDoom(double realangle)
		{
			return normalized((int)(realangle - 90));
		}

		/// <summary>
		/// Converts a real world  angle (where 0° is north) to a Doom angle (where 0° is east) in radians.
		/// </summary>
		/// <param name="realangle">Real world angle in radians</param>
		/// <returns>Doom angle in degrees</returns>
		public static int realToDoomRad(double realangle)
		{
			return Angle2D.RealToDoom(realangle);
		}

		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		/// <param name="rad">Angle in radians</param>
		/// <returns>Angle in degrees</returns>
		public static double radToDeg(double rad)
		{
			return Angle2D.RadToDeg(rad);
		}

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		/// <param name="deg">Angle in degrees</param>
		/// <returns>Angle in radians</returns>
		public static double DegToRad(double deg)
		{
			return Angle2D.DegToRad(deg);
		}

		/// <summary>
		/// Normalizes an angle in degrees so that it is bigger or equal to 0° and smaller than 360°.
		/// </summary>
		/// <param name="angle">Angle in degrees</param>
		/// <returns>Normalized angle in degrees</returns>
		public static int normalized(int angle)
		{
			while (angle < 0) angle += 360;
			while (angle >= 360) angle -= 360;
			return angle;
		}

		/// <summary>
		/// Normalizes an angle in radians so that it is bigger or equal to 0 and smaller than 2 Pi.
		/// </summary>
		/// <param name="angle">Angle in radians</param>
		/// <returns>Normalized angle in radians</returns>
		public static double normalizedRad(double angle)
		{
			return Angle2D.Normalized(angle);
		}

		/// <summary>
		/// Returns the angle between three positions.
		/// </summary>
		/// <param name="p1">First position</param>
		/// <param name="p2">Second position</param>
		/// <param name="p3">Third position</param>
		/// <returns>Angle in degrees</returns>
		public static double getAngle(object p1, object p2, object p3)
		{
			try
			{
				Vector2D v1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p1, false);
				Vector2D v2 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p2, false);
				Vector2D v3 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p3, false);

				return Angle2D.RadToDeg(Angle2D.GetAngle(v1, v2, v3));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the angle between three positions in radians.
		/// </summary>
		/// <param name="p1">First position</param>
		/// <param name="p2">Second position</param>
		/// <param name="p3">Third position</param>
		/// <returns>Angle in radians</returns>
		public static double getAngleRad(object p1, object p2, object p3)
		{
			try
			{
				Vector2D v1 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p1, false);
				Vector2D v2 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p2, false);
				Vector2D v3 = (Vector2D)BuilderPlug.Me.GetVectorFromObject(p3, false);

				return Angle2D.GetAngle(v1, v2, v3);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}
	}
}
