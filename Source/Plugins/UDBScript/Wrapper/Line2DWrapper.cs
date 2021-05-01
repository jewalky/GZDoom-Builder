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

		#region ================== Statics

		#endregion

		#region ================== Methods

		public Vector2DWrapper getCoordinatesAt(double u)
		{
			return new Vector2DWrapper(new Line2D(v1.x, v1.y, v2.x, v2.y).GetCoordinatesAt(u));
		}

		public double getLength()
		{
			return Line2D.GetLength(v2.x - v1.x, v2.y - v1.y);
		}

		public double getAngleRad()
		{
			return new Line2D(v1.AsVector2D(), v2.AsVector2D()).GetAngle();
		}
		
		public double getAngle()
		{
			return Angle2D.RadToDeg(new Line2D(v1.AsVector2D(), v2.AsVector2D()).GetAngle());
		}

		#endregion
	}
}
