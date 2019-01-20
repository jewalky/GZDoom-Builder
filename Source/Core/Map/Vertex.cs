
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
using System.Collections.Generic;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Map
{
	public sealed class Vertex : SelectableElement
	{
		#region ================== Constants
		
		public const int BUFFERVERTICES = 1;
		public const int RENDERPRIMITIVES = 1;
		
		#endregion

		#region ================== Variables

		// Map
		private MapSet map;
		
		// List items
		private LinkedListNode<Vertex> selecteditem;

		// Position
		private Vector2D pos;

		//mxd. Height
		private float zfloor;
		private float zceiling;

		// References
		private LinkedList<Linedef> linedefs;
		
		// Cloning
		private Vertex clone;
		private int serializedindex;
		
		#endregion

		#region ================== Properties

		public MapSet Map { get { return map; } }
		public ICollection<Linedef> Linedefs { get { return linedefs; } }
		public Vector2D Position { get { return pos; } }
		internal Vertex Clone { get { return clone; } set { clone = value; } }
		internal int SerializedIndex { get { return serializedindex; } set { serializedindex = value; } }
		public float ZCeiling {	//mxd
			get { return zceiling; }
			set {
				if(zceiling != value) 
				{
					BeforeFieldsChange();
					zceiling = value;
				}
			}
		}
		public float ZFloor { //mxd
			get { return zfloor; }
			set {
				if(zfloor != value) 
				{
					BeforeFieldsChange();
					zfloor = value;
				}
			}
		}

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal Vertex(MapSet map, int listindex, Vector2D pos)
		{
            // [ZZ] Check coordinates
            //      Something in GZDB creates vertices with NaN coordinates. This needs to be found.
            if (float.IsNaN(pos.x) ||
                float.IsNaN(pos.y))
            {
                throw new Exception("Tried to create a vertex at coordinates NaN,NaN");
            }

            // Initialize
            this.elementtype = MapElementType.VERTEX; //mxd
			this.map = map;
			this.linedefs = new LinkedList<Linedef>();
			this.listindex = listindex;
			this.pos = pos;
			this.zceiling = float.NaN; //mxd
			this.zfloor = float.NaN; //mxd
			
			if(map == General.Map.Map)
				General.Map.UndoRedo.RecAddVertex(this);
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Already set isdisposed so that changes can be prohibited
				isdisposed = true;

				if(map.AutoRemove)
				{
					// Dispose the lines that are attached to this vertex
					// because a linedef cannot exist without 2 vertices.
					foreach(Linedef ld in linedefs) ld.Dispose();
				}
				else
				{
					// Detach from linedefs
					foreach(Linedef ld in linedefs) ld.DetachVertexP(this);
				}
				
				if(map == General.Map.Map)
					General.Map.UndoRedo.RecRemVertex(this);
				
				// Remove from main list
				map.RemoveVertex(listindex);

				// Clean up
				linedefs = null;
				map = null;

				//mxd. Restore isdisposed so base classes can do their disposal job
				isdisposed = false;

				// Dispose base
				base.Dispose();
			}
		}

		#endregion

		#region ================== Management

		// Call this before changing properties
		protected override void BeforePropsChange()
		{
			if(map == General.Map.Map)
				General.Map.UndoRedo.RecPrpVertex(this);
		}

		// This attaches a linedef and returns the listitem
		internal LinkedListNode<Linedef> AttachLinedefP(Linedef l)
		{
			return linedefs.AddLast(l);
		}

		// This detaches a linedef
		internal void DetachLinedefP(LinkedListNode<Linedef> l)
		{
			// Not disposing?
			if(!isdisposed)
			{
				// Remove linedef
				linedefs.Remove(l);

				// No more linedefs left?
				if((linedefs.Count == 0) && map.AutoRemove)
				{
					// This vertex is now useless, dispose it
					this.Dispose();
				}
			}
		}

		// Serialize / deserialize
		new internal void ReadWrite(IReadWriteStream s)
		{
			if(!s.IsWriting) BeforePropsChange();
			
			base.ReadWrite(s);
			
			s.rwVector2D(ref pos);
			s.rwFloat(ref zceiling); //mxd
			s.rwFloat(ref zfloor); //mxd
			
			if(s.IsWriting)
			{
				// Let all lines know they need an update
				foreach(Linedef l in linedefs) l.NeedUpdate();
			}
		}

		// Selected
		protected override void DoSelect()
		{
			base.DoSelect();
			selecteditem = map.SelectedVertices.AddLast(this);
		}

		// Deselect
		protected override void DoUnselect()
		{
			base.DoUnselect();
			if(selecteditem.List != null) selecteditem.List.Remove(selecteditem);
			selecteditem = null;
		}

		#endregion
		
		#region ================== Methods

		// This copies all properties to another thing
		public void CopyPropertiesTo(Vertex v)
		{
			v.BeforePropsChange();
			
			// Copy properties
			v.pos = pos;
			v.zceiling = zceiling; //mxd
			v.zfloor = zfloor; //mxd
			base.CopyPropertiesTo(v);
		}
		
		// This returns the distance from given coordinates
		public float DistanceToSq(Vector2D p)
		{
			return (p.x - pos.x) * (p.x - pos.x) + (p.y - pos.y) * (p.y - pos.y);
		}
		
		// This returns the distance from given coordinates
		public float DistanceTo(Vector2D p)
		{
			return Vector2D.Distance(p, pos);
		}

		// This finds the line closest to the specified position
		public Linedef NearestLinedef(Vector2D pos) { return MapSet.NearestLinedef(linedefs, pos); }

		// This moves the vertex
		public void Move(Vector2D newpos)
		{
			// Do we actually move?
			if(newpos != pos)
			{
				BeforePropsChange();
				
				// Change position
				pos = newpos;

				#if DEBUG
				if(float.IsNaN(pos.x) || float.IsNaN(pos.y) ||
				   float.IsInfinity(pos.x) || float.IsInfinity(pos.y))
				{
					General.Fail("Invalid vertex position! The given vertex coordinates cannot be NaN or Infinite.");
				}
				#endif

				// Let all lines know they need an update
				foreach(Linedef l in linedefs) l.NeedUpdate();
				General.Map.IsChanged = true;
			}
		}

		// This snaps the vertex to the map format accuracy
		public void SnapToAccuracy()
		{
			SnapToAccuracy(true);
		}

		// This snaps the vertex to the map format accuracy
		public void SnapToAccuracy(bool usepreciseposition)
		{
			// Round the coordinates
			Vector2D newpos = new Vector2D((float)Math.Round(pos.x, (usepreciseposition ? General.Map.FormatInterface.VertexDecimals : 0)),
										   (float)Math.Round(pos.y, (usepreciseposition ? General.Map.FormatInterface.VertexDecimals : 0)));
			this.Move(newpos);
		}

		// This snaps the vertex to the grid
		public void SnapToGrid()
		{
			// Calculate nearest grid coordinates
			this.Move(General.Map.Grid.SnappedToGrid(pos));
		}
		
		// This joins another vertex
		// Which means this vertex is removed and the other is kept!
		public void Join(Vertex other)
		{
			// If either of the two vertices was selected, keep the other selected
			if(this.Selected) other.Selected = true;
			if(this.marked) other.marked = true;

			// Any linedefs to move?
			if(linedefs.Count > 0)
			{
				// Detach all linedefs and attach them to the other
				// This will automatically dispose this vertex
				while(linedefs != null)
				{
					// Move the line to the other vertex
					if(linedefs.First.Value.Start == this)
						linedefs.First.Value.SetStartVertex(other);
					else
						linedefs.First.Value.SetEndVertex(other);
				}
			}
			else
			{
				// No lines attached
				// Dispose manually
				this.Dispose();
			}

			General.Map.IsChanged = true;
		}

		// String representation
		public override string ToString()
		{
#if DEBUG
			return "Vertex " + Index + " (" + pos + (marked ? "; marked" : "") + ")";
#else
			return "Vertex (" + pos + ")";
#endif
		}

		#endregion
	}
}
