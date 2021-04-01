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
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	internal class MapWrapper
	{
		#region ================== Variables

		private MapSet map;

		#endregion

		#region ================== Properties

		/// <summary>
		/// `true` if the map is in Doom format, `false` if it isn't. Read-only.
		/// </summary>
		public bool isDoom { get { return General.Map.DOOM; } }

		/// <summary>
		/// `true` if the map is in Hexen format, `false` if it isn't. Read-only.
		/// </summary>
		public bool isHexen { get { return General.Map.HEXEN; } }

		/// <summary>
		/// `true` if the map is in UDMF, `false` if it isn't. Read-only.
		/// </summary>
		public bool isUDMF { get { return General.Map.UDMF; } }

		#endregion

		#region ================== Constructors

		public MapWrapper()
		{
			map = General.Map.Map;
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Returns an array of all things in the map
		/// </summary>
		/// <returns></returns>
		public ThingWrapper[] getThings()
		{
			List<ThingWrapper> things = new List<ThingWrapper>(General.Map.Map.Things.Count);

			foreach (Thing t in General.Map.Map.Things)
				if(!t.IsDisposed)
					things.Add(new ThingWrapper(t));

			return things.ToArray();
		}

		/// <summary>
		/// Returns an array of all sectors in the map
		/// </summary>
		/// <returns>`Array` of `Sector`s</returns>
		public SectorWrapper[] getSectors()
		{
			List<SectorWrapper> sectors = new List<SectorWrapper>(General.Map.Map.Sectors.Count);

			foreach (Sector s in General.Map.Map.Sectors)
				if (!s.IsDisposed)
					sectors.Add(new SectorWrapper(s));

			return sectors.ToArray();
		}

		/// <summary>
		/// Returns an array of all sidedefs in the map
		/// </summary>
		/// <returns>`Array` of `Linedef`s</returns>
		public SidedefWrapper[] getSidedefs()
		{
			List<SidedefWrapper> sidedefs = new List<SidedefWrapper>(General.Map.Map.Sidedefs.Count);

			foreach (Sidedef sd in General.Map.Map.Sidedefs)
				if (!sd.IsDisposed)
					sidedefs.Add(new SidedefWrapper(sd));

			return sidedefs.ToArray();
		}

		/// <summary>
		/// Returns an array of all linedefs in the map
		/// </summary>
		/// <returns>`Array` of `Sidedef`s</returns>
		public LinedefWrapper[] getLinedefs()
		{
			List<LinedefWrapper> linedefs = new List<LinedefWrapper>(General.Map.Map.Linedefs.Count);

			foreach (Linedef ld in General.Map.Map.Linedefs)
				if (!ld.IsDisposed)
					linedefs.Add(new LinedefWrapper(ld));

			return linedefs.ToArray();
		}

		/// <summary>
		/// Creates a new `Vertex` at the given position. The position can be a `Vector2D` or an `Array` of two numbers.
		/// ```
		/// var v1 = Map.createVertex(new Vector2D(32, 64));
		/// var v2 = Map.createVertex([ 32, 64 ]);
		/// ```
		/// </summary>
		/// <param name="pos">Position where the `Vertex` should be created at</param>
		/// <returns>The created `Vertex`</returns>
		public VertexWrapper createVertex(object pos)
		{
			try
			{
				Vector2D v = (Vector2D)MapElementWrapper.GetVectorFromObject(pos, false);
				Vertex newvertex = General.Map.Map.CreateVertex(v);

				if(newvertex == null)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Failed to create new vertex");

				return new VertexWrapper(newvertex);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Creates a new `Thing` at the given position. The position can be a `Vector2D` or an `Array` of two numbers. A thing type can be supplied optionally.
		/// ```
		/// var t1 = Map.createThing(new Vector2D(32, 64));
		/// var t2 = Map.createThing([ 32, 64 ]);
		/// var t3= Map.createThing(new Vector2D(32, 64), 3001); // Create an Imp
		/// var t4 = Map.createThing([ 32, 64 ], 3001); // Create an Imp
		/// ```
		/// </summary>
		/// <param name="pos">Position where the `Thing` should be created at</param>
		/// <param name="type">Thing type (optional)</param>
		/// <returns></returns>
		public ThingWrapper createThing(object pos, int type=0)
		{
			try
			{
				if(type < 0)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Thing type can not be negative.");

				object v = MapElementWrapper.GetVectorFromObject(pos, true);
				Thing t = General.Map.Map.CreateThing();

				if(t == null)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Failed to create new thing.");

				General.Settings.ApplyDefaultThingSettings(t);

				if (type > 0)
					t.Type = type;

				if(v is Vector2D)
					t.Move((Vector2D)v);
				else if(v is Vector3D)
					t.Move((Vector3D)v);

				t.UpdateConfiguration();

				return new ThingWrapper(t);
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		public void beginAddRemove()
		{
			map.BeginAddRemove();
		}

		public void endAddRemove()
		{
			map.EndAddRemove();
		}

		#endregion
	}
}
