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
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	class VertexWrapper : MapElementWrapper
	{
		#region ================== Variables

		Vertex vertex;

		#endregion

		#region ================== Properties

		internal Vertex Vertex { get { return vertex; } }

		/// <summary>
		/// The vertex index. Read-only.
		/// </summary>
		public int index
		{
			get
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the index property can not be accessed.");

				return vertex.Index;
			}
		}

		/// <summary>
		/// Position of the vertex. It's an object with `x` and `y` properties. 
		/// The `x` and `y` accept numbers:
		/// ```
		/// v.position.x = 32;
		/// v.position.y = 64;
		/// ```
		/// It's also possible to set all fields immediately by assigning either a `Vector2D`, or an array of numbers:
		/// ```
		/// v.position = new Vector2D(32, 64);
		/// v.position = [ 32, 64 ];
		/// ```
		/// </summary>
		public object position
		{
			get
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the position property can not be accessed.");

				dynamic eo = new ExpandoObject();
				IDictionary<string, object> o = eo as IDictionary<string, object>;

				// Create x, y, and z properties
				o.Add("x", vertex.Position.x);
				o.Add("y", vertex.Position.y);

				// Create event that gets called when a property is changed. This moves the thing to the given position
				((INotifyPropertyChanged)eo).PropertyChanged += new PropertyChangedEventHandler((sender, ea) => {
					string[] allowedproperties = new string[] { "x", "y" };

					// Give us easier access to the variables
					var pcea = ea as PropertyChangedEventArgs;
					IDictionary<string, object> so = sender as IDictionary<string, object>;

					// Make sure the changed property is a valid one
					if (!allowedproperties.Contains(pcea.PropertyName))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Invalid property '" + pcea.PropertyName + "' given. Only x and y are allowed.");

					if (!(so[pcea.PropertyName] is double))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Position values must be a number.");

					double x = (double)so["x"];
					double y = (double)so["y"];

					vertex.Move(new Vector2D(x, y));

					General.Map.Map.Update();
				});

				return eo;
			}
			set
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the position property can not be accessed.");

				if (value is Vector2D)
					vertex.Move((Vector2D)value);
				else if (value.GetType().IsArray)
				{
					object[] vals = (object[])value;

					// Make sure all values in the array are doubles
					foreach (object v in vals)
						if (!(v is double))
							throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Values in position array must be numbers.");

					if (vals.Length == 2)
						vertex.Move(new Vector2D((double)vals[0], (double)vals[1]));
					else
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Position array must contain 2 values.");
				}
				else
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Position values must be a Vector2D, or an array of numbers.");
			}
		}

		/// <summary>
		/// If the vertex is selected or not
		/// </summary>
		public bool selected
		{
			get
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the selected property can not be accessed.");

				return vertex.Selected;
			}
			set
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the selected property can not be accessed.");

				vertex.Selected = value;
			}
		}

		/// <summary>
		/// If the vertex is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).
		/// </summary>
		public bool marked
		{
			get
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the marked property can not be accessed.");

				return vertex.Marked;
			}
			set
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the marked property can not be accessed.");

				vertex.Marked = value;
			}
		}

		/// <summary>
		/// The ceiling z position of the vertex. Only available in UDMF. Only available for supported game configurations.
		/// </summary>
		public double ceilingZ
		{
			get
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the ceilingZ property can not be accessed.");

				return vertex.ZCeiling;
			}
			set
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the ceilingZ property can not be accessed.");

				vertex.ZCeiling = value;
			}
		}

		/// <summary>
		/// The floor z position of the vertex. Only available in UDMF. Only available for supported game configurations.
		/// </summary>
		public double floorZ
		{
			get
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the floorZ property can not be accessed.");

				return vertex.ZFloor;
			}
			set
			{
				if (vertex.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the floorZ property can not be accessed.");

				vertex.ZFloor = value;
			}
		}

		#endregion

		#region ================== Constructors

		internal VertexWrapper(Vertex vertex) : base(vertex)
		{
			this.vertex = vertex;
		}

		#endregion

		#region ================== Update

		internal override void AfterFieldsUpdate()
		{
		}

		#endregion

		#region ================== Methods

		public override string ToString()
		{
			return vertex.ToString();
		}

		/// <summary>
		/// Gets all linedefs that are connected to this vertex.
		/// </summary>
		/// <returns>Array of linedefs</returns>
		public LinedefWrapper[] getLinedefs()
		{
			if (vertex.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the getLinedefs method can not be accessed.");

			List<LinedefWrapper> linedefs = new List<LinedefWrapper>(vertex.Linedefs.Count);

			foreach (Linedef ld in vertex.Linedefs)
				if (!ld.IsDisposed)
					linedefs.Add(new LinedefWrapper(ld));

			return linedefs.ToArray();
		}

		/// <summary>
		/// Copies the properties from this vertex to another.
		/// </summary>
		/// <param name="v">the vertex to copy the properties to</param>
		public void copyPropertiesTo(VertexWrapper v)
		{
			if (vertex.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the copyPropertiesTo method can not be accessed.");

			vertex.CopyPropertiesTo(v.vertex);
		}

		/// <summary>
		/// Gets the squared distance between this vertex and the given point.
		/// The point can be either a `Vector2D` or an array of numbers.
		/// ```
		/// v.distanceToSq(new Vector2D(32, 64));
		/// v.distanceToSq([ 32, 64 ]);
		/// ```
		/// </summary>
		/// <param name="pos">Point to calculate the squared distance to.</param>
		/// <returns>Squared distance to `pos`</returns>
		public double distanceToSq(object pos)
		{
			if (vertex.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the distanceToSq method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)GetVectorFromObject(pos, false);
				return vertex.DistanceToSq(v);
			}
			catch(CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the distance between this vertex and the given point.
		/// The point can be either a `Vector2D` or an array of numbers.
		/// ```
		/// v.distanceTo(new Vector2D(32, 64));
		/// v.distanceTo([ 32, 64 ]);
		/// ```
		/// </summary>
		/// <param name="pos">Point to calculate the distance to.</param>
		/// <returns>Distance to `pos`</returns>
		public double distanceTo(object pos)
		{
			if (vertex.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the distanceTo method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)GetVectorFromObject(pos, false);
				return vertex.DistanceTo(v);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Returns the linedef that is connected to this vertex that is closest to the given point.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public LinedefWrapper nearestLinedef(object pos)
		{
			if (vertex.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the nearestLinedef method can not be accessed.");

			try
			{
				Vector2D v = (Vector2D)GetVectorFromObject(pos, false);
				return new LinedefWrapper(vertex.NearestLinedef(v));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Snaps the vertex's position to the map format's accuracy
		/// </summary>
		public void snapToAccuracy()
		{
			if (vertex.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the snapToAccuracy method can not be accessed.");

			vertex.SnapToAccuracy();
		}

		/// <summary>
		/// Snaps the vertex's position to the grid.
		/// </summary>
		public void snapToGrid()
		{
			if (vertex.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the snapToGrid method can not be accessed.");

			vertex.SnapToGrid();
		}

		/// <summary>
		/// Joins this vertex with another vertex, deleting this vertex and keeping the other.
		/// </summary>
		/// <param name="other">`Vertex` to join with</param>
		public void join(VertexWrapper other)
		{
			if (vertex.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Vertex is disposed, the join method can not be accessed.");

			vertex.Join(other.vertex);
		}

		/// <summary>
		/// Deletes the vertex. Note that this can result in unclosed sectors.
		/// </summary>
		public void delete()
		{
			if (vertex.IsDisposed)
				return;

			vertex.Dispose();
		}

		#endregion
	}
}
