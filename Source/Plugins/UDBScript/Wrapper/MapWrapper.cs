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
using CodeImp.DoomBuilder.Editing;
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

		public Vector2D mousePosition { get { return ((ClassicMode)General.Editing.Mode).MouseMapPos; } }

		#endregion

		#region ================== Constructors

		public MapWrapper()
		{
			map = General.Map.Map;
		}

		#endregion

		#region ================== Methods

		public Vector2D snappedToGrid(object pos)
		{
			try
			{
				return General.Map.Grid.SnappedToGrid((Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

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
		/// Stitches marked geometry with non-marked geometry.
		/// </summary>
		/// <param name="mergemode">Mode to merge by</param>
		/// <returns>`true` if successful, `false` if failed</returns>
		public bool stitchGeometry(MergeGeometryMode mergemode = MergeGeometryMode.CLASSIC)
		{
			return General.Map.Map.StitchGeometry(mergemode);
		}

		/// <summary>
		/// Snaps all vertices and things to the map format accuracy. Call this to ensure the vertices and things are at valid coordinates.
		/// </summary>
		/// <param name="usepreciseposition"></param>
		public void snapAllToAccuracy(bool usepreciseposition = true)
		{
			General.Map.Map.SnapAllToAccuracy(usepreciseposition);
		}

		/// <summary>
		/// Gets a new tag.
		/// </summary>
		/// <param name="usedtags">List of additional tags to skip</param>
		/// <returns>The new tag</returns>
		public int getNewTag(int[] usedtags = null)
		{
			if (usedtags == null)
				return General.Map.Map.GetNewTag();
			else
				return General.Map.Map.GetNewTag(usedtags.ToList());
		}

		/// <summary>
		/// Gets multiple new tags.
		/// </summary>
		/// <param name="count">Number of tags to get</param>
		/// <returns>`Array` of the new tags</returns>
		public int[] getMultipleNewTags(int count)
		{
			return General.Map.Map.GetMultipleNewTags(count).ToArray();
		}

		/// <summary>
		/// Gets the linedef that's nearest to the specified position.
		/// </summary>
		/// <param name="pos">Position to check against</param>
		/// <param name="maxrange">Maximum range (optional)</param>
		/// <returns>Nearest linedef</returns>
		public LinedefWrapper nearestLinedef(object pos, double maxrange = double.NaN)
		{
			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);

				if (double.IsNaN(maxrange))
					return new LinedefWrapper(General.Map.Map.NearestLinedef(v));
				else
					return new LinedefWrapper(General.Map.Map.NearestLinedefRange(v, maxrange));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the thing that's nearest to the specified position.
		/// </summary>
		/// <param name="pos">Position to check against</param>
		/// <param name="maxrange">Maximum range (optional)</param>
		/// <returns>Nearest linedef</returns>
		public ThingWrapper nearestThing(object pos, double maxrange = double.NaN)
		{
			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);

				if (double.IsNaN(maxrange))
					return new ThingWrapper(General.Map.Map.NearestThingSquareRange(v, double.MaxValue));
				else
					return new ThingWrapper(General.Map.Map.NearestThingSquareRange(v, maxrange));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the vertex that's nearest to the specified position.
		/// </summary>
		/// <param name="pos">Position to check against</param>
		/// <param name="maxrange">Maximum range (optional)</param>
		/// <returns>Nearest linedef</returns>
		public VertexWrapper nearestVertex(object pos, double maxrange = double.NaN)
		{
			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);

				if (double.IsNaN(maxrange))
					return new VertexWrapper(General.Map.Map.NearestVertexSquareRange(v, double.MaxValue));
				else
					return new VertexWrapper(General.Map.Map.NearestVertexSquareRange(v, maxrange));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Gets the sidedef that's nearest to the specified position.
		/// </summary>
		/// <param name="pos">Position to check against</param>
		/// <param name="maxrange">Maximum range (optional)</param>
		/// <returns>Nearest linedef</returns>
		public SidedefWrapper nearestSidedef(object pos)
		{
			try
			{
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);

				return new SidedefWrapper(MapSet.NearestSidedef(General.Map.Map.Sidedefs, v));
			}
			catch (CantConvertToVectorException e)
			{
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
			}
		}

		/// <summary>
		/// Draws lines. Data has to be an `Array` of `Array` of numbers, `Vector2D`s, `Vector3D`s, or objects with x and y properties.
		/// Note that the first and last element have to be at the same positions to make a complete drawing.
		/// ```
		/// Map.DrawLines([
		///		new Vector2D(64, 0),
		///		new Vector2D(128, 0),
		///		new Vector2D(128, 64),
		///		new Vector2D(64, 64),
		///		new Vector2D(64, 0)
		///	]);
		///	
		/// Map.DrawLines([
		///		[ 0, 0 ],
		///		[ 64, 0 ],
		///		[ 64, 64 ],
		///		[ 0, 64 ],
		///		[ 0, 0 ]
		/// ]);
		/// ```
		/// </summary>
		/// <param name="data">`Array` of positions</param>
		/// <returns>`true` is drawing was successful, `false` if it wasn't</returns>
		public bool drawLines(object data)
		{
			if (!data.GetType().IsArray)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Data must be supplied as an array");

			List<DrawnVertex> vertices = new List<DrawnVertex>();

			foreach(object item in (object[])data)
			{
				try
				{
					Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(item, false);
					DrawnVertex dv = new DrawnVertex();
					dv.pos = v;
					dv.stitch = dv.stitchline = true;
					vertices.Add(dv);
				}
				catch (CantConvertToVectorException e)
				{
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(e.Message);
				}
			}

			if(vertices.Count < 2)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Array must have at least 2 values");

			bool success = Tools.DrawLines(vertices);

			// Snap to map format accuracy
			General.Map.Map.SnapAllToAccuracy();

			// Update map
			General.Map.Map.Update();

			// Update textures
			General.Map.Data.UpdateUsedTextures();

			return success;
		}

		#endregion

		#region ================== Marks

		/// <summary>
		/// Sets the `marked` property of all map elements. Can be passed `true` to mark all map elements.
		/// </summary>
		/// <param name="mark">`false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`</param>
		public void clearAllMarks(bool mark=false)
		{
			General.Map.Map.ClearAllMarks(mark);
		}

		/// <summary>
		/// Sets the `marked` property of all vertices. Can be passed `true` to mark all vertices.
		/// </summary>
		/// <param name="mark">`false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`</param>
		public void clearMarkedVertices(bool mark=false)
		{
			General.Map.Map.ClearMarkedVertices(mark);
		}

		/// <summary>
		/// Sets the `marked` property of all things. Can be passed `true` to mark all things.
		/// </summary>
		/// <param name="mark">`false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`</param>
		public void clearMarkedThings(bool mark=false)
		{
			General.Map.Map.ClearMarkedThings(mark);
		}

		/// <summary>
		/// Sets the `marked` property of all linedefs. Can be passed `true` to mark all linedefs.
		/// </summary>
		/// <param name="mark">`false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`</param>
		public void clearMarkeLinedefs(bool mark=false)
		{
			General.Map.Map.ClearMarkedLinedefs(mark);
		}

		/// <summary>
		/// Sets the `marked` property of all sidedefs. Can be passed `true` to mark all sidedefs.
		/// </summary>
		/// <param name="mark">`false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`</param>
		public void clearMarkeSidedefs(bool mark = false)
		{
			General.Map.Map.ClearMarkedSidedefs(mark);
		}

		/// <summary>
		/// Sets the `marked` property of all sectors. Can be passed `true` to mark all sectors.
		/// </summary>
		/// <param name="mark">`false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`</param>
		public void clearMarkeSectors(bool mark = false)
		{
			General.Map.Map.ClearMarkedSectors(mark);
		}

		/// <summary>
		/// Inverts all marks of all map elements.
		/// </summary>
		public void invertAllMarks()
		{
			General.Map.Map.InvertAllMarks();
		}

		/// <summary>
		/// Inverts the `marked` property of all vertices.
		/// </summary>
		public void invertMarkedVertices()
		{
			General.Map.Map.InvertMarkedVertices();
		}

		/// <summary>
		/// Inverts the `marked` property of all things.
		/// </summary>
		public void invertMarkedThings()
		{
			General.Map.Map.InvertMarkedThings();
		}

		/// <summary>
		/// Inverts the `marked` property of all linedefs.
		/// </summary>
		public void invertMarkedLinedefs()
		{
			General.Map.Map.InvertMarkedLinedefs();
		}

		/// <summary>
		/// Inverts the `marked` property of all sidedefs.
		/// </summary>
		public void invertMarkedSidedefs()
		{
			General.Map.Map.InvertMarkedSidedefs();
		}

		/// <summary>
		/// Inverts the `marked` property of all sectors.
		/// </summary>
		public void invertMarkedSectors()
		{
			General.Map.Map.InvertMarkedSectors();
		}

		/// <summary>
		/// Gets all marked (default) or unmarked vertices.
		/// </summary>
		/// <param name="mark">`true` to get all marked vertices (default), `false` to get all unmarked vertices</param>
		/// <returns></returns>
		public VertexWrapper[] getMarkedVertices(bool mark=true)
		{
			List<VertexWrapper> vertices = new List<VertexWrapper>();

			foreach (Vertex v in General.Map.Map.Vertices)
				if(v.Marked == mark)
					vertices.Add(new VertexWrapper(v));

			return vertices.ToArray();
		}

		/// <summary>
		/// Gets all marked (default) or unmarked things.
		/// </summary>
		/// <param name="mark">`true` to get all marked things (default), `false` to get all unmarked things</param>
		/// <returns></returns>
		public ThingWrapper[] getMarkedThings(bool mark = true)
		{
			List<ThingWrapper> things = new List<ThingWrapper>();

			foreach (Thing t in General.Map.Map.Things)
				if (t.Marked == mark)
					things.Add(new ThingWrapper(t));

			return things.ToArray();
		}

		/// <summary>
		/// Gets all marked (default) or unmarked linedefs.
		/// </summary>
		/// <param name="mark">`true` to get all marked linedefs (default), `false` to get all unmarked linedefs</param>
		/// <returns></returns>
		public LinedefWrapper[] getMarkedLinedefs(bool mark = true)
		{
			List<LinedefWrapper> linedefs = new List<LinedefWrapper>();

			foreach (Linedef ld in General.Map.Map.Linedefs)
				if (ld.Marked == mark)
					linedefs.Add(new LinedefWrapper(ld));

			return linedefs.ToArray();
		}

		/// <summary>
		/// Gets all marked (default) or unmarked sidedefs.
		/// </summary>
		/// <param name="mark">`true` to get all marked sidedefs (default), `false` to get all unmarked sidedefs</param>
		/// <returns></returns>
		public SidedefWrapper[] getMarkedSidedef(bool mark = true)
		{
			List<SidedefWrapper> sidedefs = new List<SidedefWrapper>();

			foreach (Sidedef sd in General.Map.Map.Sidedefs)
				if (sd.Marked == mark)
					sidedefs.Add(new SidedefWrapper(sd));

			return sidedefs.ToArray();
		}

		/// <summary>
		/// Gets all marked (default) or unmarked sectors.
		/// </summary>
		/// <param name="mark">`true` to get all marked sectors (default), `false` to get all unmarked sectors</param>
		/// <returns></returns>
		public SectorWrapper[] getMarkedSectors(bool mark = true)
		{
			List<SectorWrapper> sectors = new List<SectorWrapper>();

			foreach (Sector s in General.Map.Map.Sectors)
				if (s.Marked == mark)
					sectors.Add(new SectorWrapper(s));

			return sectors.ToArray();
		}

		/// <summary>
		/// Marks (default) or unmarks all selected vertices.
		/// </summary>
		/// <param name="mark">`true` to mark all selected vertices (default), `false` to unmark</param>
		public void markSelectedVertices(bool mark=true)
		{
			General.Map.Map.MarkSelectedVertices(true, mark);
		}

		/// <summary>
		/// Marks (default) or unmarks all selected linedefs.
		/// </summary>
		/// <param name="mark">`true` to mark all selected linedefs (default), `false` to unmark</param>
		public void markSelectedLinedefs(bool mark = true)
		{
			General.Map.Map.MarkSelectedLinedefs(true, mark);
		}

		/// <summary>
		/// Marks (default) or unmarks all selected sectors.
		/// </summary>
		/// <param name="mark">`true` to mark all selected sectors (default), `false` to unmark</param>
		public void markSelectedSectors(bool mark = true)
		{
			General.Map.Map.MarkSelectedSectors(true, mark);
		}

		/// <summary>
		/// Marks (default) or unmarks all selected things.
		/// </summary>
		/// <param name="mark">`true` to mark all selected things (default), `false` to unmark</param>
		public void markSelectedThings(bool mark = true)
		{
			General.Map.Map.MarkSelectedThings(true, mark);
		}

		#endregion

		#region ================== Selected

		/// <summary>
		/// Gets all selected (default) or unselected vertices.
		/// </summary>
		/// <param name="selected">`true` to get all selected vertices, `false` to get all unselected ones</param>
		/// <returns></returns>
		public VertexWrapper[] getSelectedVertices(bool selected=true)
		{
			List<VertexWrapper> vertices = new List<VertexWrapper>(General.Map.Map.SelectedVerticessCount);

			foreach (Vertex v in General.Map.Map.Vertices)
				if (v.Selected == selected)
					vertices.Add(new VertexWrapper(v));

			return vertices.ToArray();
		}

		/// <summary>
		/// Gets all selected (default) or unselected things.
		/// </summary>
		/// <param name="selected">`true` to get all selected things, `false` to get all unselected ones</param>
		/// <returns></returns>
		public ThingWrapper[] getSelectedThings(bool selected = true)
		{
			List<ThingWrapper> things = new List<ThingWrapper>(General.Map.Map.SelectedThingsCount);

			foreach (Thing t in General.Map.Map.Things)
				if (t.Selected == selected)
					things.Add(new ThingWrapper(t));

			return things.ToArray();
		}

		/// <summary>
		/// Gets all selected (default) or unselected sectors.
		/// </summary>
		/// <param name="selected">`true` to get all selected sectors, `false` to get all unselected ones</param>
		/// <returns></returns>
		public SectorWrapper[] getSelectedSectors(bool selected = true)
		{
			List<SectorWrapper> sectors = new List<SectorWrapper>(General.Map.Map.SelectedSectorsCount);

			foreach (Sector s in General.Map.Map.Sectors)
				if (s.Selected == selected)
					sectors.Add(new SectorWrapper(s));

			return sectors.ToArray();
		}

		/// <summary>
		/// Gets all selected (default) or unselected linedefs.
		/// </summary>
		/// <param name="selected">`true` to get all selected linedefs, `false` to get all unselected ones</param>
		/// <returns></returns>
		public LinedefWrapper[] getSelectedLinedefs(bool selected = true)
		{
			List<LinedefWrapper> linedefs = new List<LinedefWrapper>(General.Map.Map.SelectedLinedefsCount);

			foreach (Linedef ld in General.Map.Map.Linedefs)
				if (ld.Selected == selected)
					linedefs.Add(new LinedefWrapper(ld));

			return linedefs.ToArray();
		}

		/// <summary>
		/// Gets all sidedefs from the selected linedefs
		/// </summary>
		/// <param name="selected">`true` to get all sidedefs of all selected linedefs, `false` to get all sidedefs of all unselected linedefs</param>
		/// <returns></returns>
		public SidedefWrapper[] getSidedefsFromSelectedLinedefs(bool selected = true)
		{
			List<SidedefWrapper> sidedefs = new List<SidedefWrapper>(General.Map.Map.SelectedLinedefsCount);

			foreach (Sidedef sd in General.Map.Map.GetSidedefsFromSelectedLinedefs(selected))
				sidedefs.Add(new SidedefWrapper(sd));

			return sidedefs.ToArray();
		}

		/// <summary>
		/// Clears all selected map elements.
		/// </summary>
		public void clearAllSelected()
		{
			General.Map.Map.ClearAllSelected();
		}

		/// <summary>
		/// Clears all selected vertices.
		/// </summary>
		public void clearSelectedVertices()
		{
			General.Map.Map.ClearSelectedVertices();
		}

		/// <summary>
		/// Clears all selected things.
		/// </summary>
		public void clearSelectedThings()
		{
			General.Map.Map.ClearSelectedThings();
		}

		/// <summary>
		/// Clears all selected sectors.
		/// </summary>
		public void clearSelectedSectors()
		{
			General.Map.Map.ClearSelectedSectors();
		}

		#endregion

		#region ================== Creation

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
				Vector2D v = (Vector2D)BuilderPlug.Me.GetVectorFromObject(pos, false);
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

				object v = BuilderPlug.Me.GetVectorFromObject(pos, true);
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

		/// <summary>
		/// Using this before making big changes to the map can improve performance. Use `endAddRemove` when you're finished.
		/// </summary>
		public void beginAddRemove()
		{
			map.BeginAddRemove();
		}

		/// <summary>
		/// Use after making big changes to them. Must be called after `beginAddRemove`.
		/// </summary>
		public void endAddRemove()
		{
			map.EndAddRemove();
		}

		#endregion
	}
}
