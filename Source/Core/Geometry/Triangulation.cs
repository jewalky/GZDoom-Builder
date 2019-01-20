
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
using CodeImp.DoomBuilder.Map;
using System.Collections.ObjectModel;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Geometry
{
	/// <summary>
	/// Responsible for creating sector polygons.
	/// Performs triangulation of sectors by using ear clipping.
	/// </summary>
	public sealed class Triangulation
	{
		#region ================== Delegates

		#if DEBUG
		
		// For debugging purpose only!
		// These are not called in a release build
		public delegate void ShowPolygon(LinkedList<EarClipVertex> p);
		public delegate void ShowEarClip(EarClipVertex[] found, LinkedList<EarClipVertex> remaining);
		public delegate void ShowRemaining(LinkedList<EarClipVertex> remaining);
		
		// For debugging purpose only!
		// These are not called in a release build
		public ShowPolygon OnShowPolygon;
		public ShowEarClip OnShowEarClip;
		public ShowRemaining OnShowRemaining;
		
		#endif

		#endregion

		#region ================== Constants

		#endregion

		#region ================== Variables

		// Number of vertices per island
		private ReadOnlyCollection<int> islandvertices;
		
		// Vertices that result from the triangulation, 3 per triangle.
		private ReadOnlyCollection<Vector2D> vertices;

		// These sidedefs match with the vertices. If a vertex is not the start
		// along a sidedef, this list contains a null entry for that vertex.
		private ReadOnlyCollection<Sidedef> sidedefs;
		
		// Temporary array for the sidedefs deserialization
		private int[] sidedefindices;
		
		#endregion
		
		#region ================== Properties
		
		public ReadOnlyCollection<int> IslandVertices { get { return islandvertices; } }
		public ReadOnlyCollection<Vector2D> Vertices { get { return vertices; } }
		public ReadOnlyCollection<Sidedef> Sidedefs { get { return sidedefs; } }
		
		#endregion
		
		#region ================== Constructor / Disposer

		// Constructor
		public static Triangulation Create(Sector sector)
		{
			Triangulation t = new Triangulation();
			t.Triangulate(sector);
			return t;
		}

		// Constructor
		public Triangulation()
		{
			islandvertices = Array.AsReadOnly(new int[0]);
			vertices = Array.AsReadOnly(new Vector2D[0]);
			sidedefs = Array.AsReadOnly(new Sidedef[0]);
		}

		// This performs the triangulation
		public void Triangulate(Sector s)
		{
			// Initialize
			List<int> islandslist = new List<int>();
			List<Vector2D> verticeslist = new List<Vector2D>();
			List<Sidedef> sidedefslist = new List<Sidedef>();

			// We have no destructor
			GC.SuppressFinalize(this);
			
			/*
			 * This process is divided into several steps:
			 * 
			 * 1) Tracing the sector lines to find clockwise outer polygons
			 * and counter-clockwise inner polygons. These are arranged in a
			 * polygon tree for the next step.
			 * 
			 * 2) Cutting the inner polygons to make a flat list of only
			 * outer polygons.
			 * 
			 * 3) Ear-clipping the polygons to create triangles.
			 * 
			 */

			// TRACING
			List<EarClipPolygon> polys = DoTrace(s);
			
			// CUTTING
			DoCutting(polys);
			
			// EAR-CLIPPING
			foreach(EarClipPolygon p in polys)
				islandslist.Add(DoEarClip(p, verticeslist, sidedefslist));

			// Make arrays
			islandvertices = Array.AsReadOnly(islandslist.ToArray());
			vertices = Array.AsReadOnly(verticeslist.ToArray());
			sidedefs = Array.AsReadOnly(sidedefslist.ToArray());
		}

		#endregion
		
		#region ================== Serialization
		
		// Serialize / deserialize
		internal void ReadWrite(IReadWriteStream s)
		{
			if(s.IsWriting)
			{
				s.wInt(islandvertices.Count);
				for(int i = 0; i < islandvertices.Count; i++) s.wInt(islandvertices[i]);
				
				s.wInt(vertices.Count);
				for(int i = 0; i < vertices.Count; i++) s.wVector2D(vertices[i]);
				
				s.wInt(sidedefs.Count);
				for(int i = 0; i < sidedefs.Count; i++)
				{
					if(sidedefs[i] != null)
						s.wInt(sidedefs[i].SerializedIndex);
					else
						s.wInt(-1);
				}
			}
			else
			{
				int c;
				
				s.rInt(out c);
				int[] islandverticeslist = new int[c];
				for(int i = 0; i < c; i++) s.rInt(out islandverticeslist[i]);
				islandvertices = Array.AsReadOnly(islandverticeslist);
				
				s.rInt(out c);
				Vector2D[] verticeslist = new Vector2D[c];
				for(int i = 0; i < c; i++) s.rVector2D(out verticeslist[i]);
				vertices = Array.AsReadOnly(verticeslist);
				
				s.rInt(out c);
				sidedefindices = new int[c];
				for(int i = 0; i < c; i++) s.rInt(out sidedefindices[i]);
			}
		}
		
		// After deserialization we need to find the actual sidedefs back
		internal void PostDeserialize(MapSet map)
		{
			// Find our sidedefs
			List<Sidedef> sides = new List<Sidedef>(sidedefindices.Length);
			for(int i = 0; i < sidedefindices.Length; i++)
			{
				if(sidedefindices[i] >= 0)
					sides.Add(map.SidedefIndices[sidedefindices[i]]);
				else
					sides.Add(null);
			}
			
			// We don't need this array any longer
			sidedefindices = null;
			
			// Keep readonly array
			sidedefs = Array.AsReadOnly(sides.ToArray());
		}
		
		
		#endregion
		
		#region ================== Tracing

		// This traces sector lines to create a polygon tree
		private static List<EarClipPolygon> DoTrace(Sector s)
		{
			Dictionary<Sidedef, bool> todosides = new Dictionary<Sidedef, bool>(s.Sidedefs.Count);
			Dictionary<Vertex, Vertex> ignores = new Dictionary<Vertex,Vertex>();
			List<EarClipPolygon> root = new List<EarClipPolygon>();

			// Fill the dictionary
			// The bool value is used to indicate lines which has been visited in the trace
			foreach(Sidedef sd in s.Sidedefs) todosides.Add(sd, false);
			
			// First remove all sides that refer to the same sector on both sides of the line
			RemoveDoubleSidedefReferences(todosides, s.Sidedefs);

			// Continue until all sidedefs have been processed
			while(todosides.Count > 0)
			{
				// Reset all visited indicators
				foreach(Sidedef sd in s.Sidedefs) if(todosides.ContainsKey(sd)) todosides[sd] = false;
				
				// Find the right-most vertex to start a trace with.
				// This guarantees that we start out with an outer polygon and we just
				// have to check if it is inside a previously found polygon.
				Vertex start = FindRightMostVertex(todosides, ignores);

				// No more possible start vertex found?
				// Then leave with what we have up till now.
				if(start == null) break;
				
				// Trace to find a polygon
				SidedefsTracePath path = DoTracePath(new SidedefsTracePath(), start, null, s, todosides);

				// If tracing is not possible (sector not closed?)
				// then add the start to the ignore list and try again later
				if(path == null)
				{
					// Ignore vertex as start
					ignores.Add(start, start);
				}
				else
				{
					// Remove the sides found in the path
					foreach(Sidedef sd in path) todosides.Remove(sd);

					// Create the polygon
					EarClipPolygon newpoly = path.MakePolygon();
					
					// Determine where this polygon goes in our tree
					foreach(EarClipPolygon p in root)
					{
						// Insert if it belongs as a child
						if(p.InsertChild(newpoly))
						{
							// Done
							newpoly = null;
							break;
						}
					}

					// Still not inserted in our tree?
					if(newpoly != null)
					{
						// Then add it at root level as outer polygon
						newpoly.Inner = false;
						root.Add(newpoly);
					}
				}
			}

			// Return result
			return root;
		}

		// This recursively traces a path
		// Returns the resulting TracePath when the search is complete
		// or returns null when no path found.
		private static SidedefsTracePath DoTracePath(SidedefsTracePath history, Vertex fromhere, Vertex findme, Sector sector, Dictionary<Sidedef, bool> sides)
		{
			// Found the vertex we are tracing to?
			if(fromhere == findme) return history;

			// On the first run, findme is null (otherwise the trace would end
			// immeditely when it starts) so set findme here on the first run.
			if(findme == null) findme = fromhere;

			// Make a list of sides referring to the same sector
			List<Sidedef> allsides = new List<Sidedef>(fromhere.Linedefs.Count * 2);
			foreach(Linedef l in fromhere.Linedefs)
			{
				// Should we go along the front or back side?
				// This is very important for clockwise polygon orientation!
				if(l.Start == fromhere)
				{
					// Front side of line connected to sector?
					if((l.Front != null) && (l.Front.Sector == sector))
					{
						// Visit here when not visited yet
						if(sides.ContainsKey(l.Front) && !sides[l.Front]) allsides.Add(l.Front);
					}
				}
				else
				{
					// Back side of line connected to sector?
					if((l.Back != null) && (l.Back.Sector == sector))
					{
						// Visit here when not visited yet
						if(sides.ContainsKey(l.Back) && !sides[l.Back]) allsides.Add(l.Back);
					}
				}
			}

			// Previous line available?
			if(history.Count > 0)
			{
				// This is done to ensure the tracing works along vertices that are shared by
				// more than 2 lines/sides of the same sector. We must continue tracing along
				// the first next smallest delta angle! This sorts the smallest delta angle to
				// the top of the list.
				SidedefAngleSorter sorter = new SidedefAngleSorter(history[history.Count - 1], fromhere);
				allsides.Sort(sorter);
			}
			
			// Go for all lines connected to this vertex
			foreach(Sidedef s in allsides)
			{
				// Mark sidedef as visited and move to next vertex
				sides[s] = true;
				SidedefsTracePath nextpath = new SidedefsTracePath(history, s);
				Vertex nextvertex = (s.Line.Start == fromhere ? s.Line.End : s.Line.Start);
				
				SidedefsTracePath result = DoTracePath(nextpath, nextvertex, findme, sector, sides);
				if(result != null) return result;
			}

			// Nothing found
			return null;
		}

		// This removes all sidedefs which has a sidedefs on the other side
		// of the same line that refers to the same sector. These are removed
		// because they are useless and make the triangulation inefficient.
		private static void RemoveDoubleSidedefReferences(Dictionary<Sidedef, bool> todosides, ICollection<Sidedef> sides)
		{
			// Go for all sides
			foreach(Sidedef sd in sides)
			{
				// Double sided?
				if(sd.Other != null)
				{
					// Referring to the same sector on both sides?
					if(sd.Sector == sd.Other.Sector)
					{
						// Remove this one
						todosides.Remove(sd);
					}
				}
			}
		}

		// This finds the right-most vertex to start tracing with
		private static Vertex FindRightMostVertex(Dictionary<Sidedef, bool> sides, Dictionary<Vertex, Vertex> ignores)
		{
			Vertex found = null;
			
			// Go for all sides to find the right-most side
			foreach(Sidedef sd in sides.Keys)
			{
				// First found?
				if((found == null) && !ignores.ContainsKey(sd.Line.Start)) found = sd.Line.Start;
				if((found == null) && !ignores.ContainsKey(sd.Line.End)) found = sd.Line.End;
				
				// Compare?
				if(found != null)
				{
					// Check if more to the right than the previous found
					if((sd.Line.Start.Position.x > found.Position.x) && !ignores.ContainsKey(sd.Line.Start)) found = sd.Line.Start;
					if((sd.Line.End.Position.x > found.Position.x) && !ignores.ContainsKey(sd.Line.End)) found = sd.Line.End;
				}
			}
			
			// Return result
			return found;
		}
		
		#endregion

		#region ================== Cutting

		// This cuts into outer polygons to solve inner polygons and make the polygon tree flat
		private void DoCutting(List<EarClipPolygon> polys)
		{
			Queue<EarClipPolygon> todo = new Queue<EarClipPolygon>(polys);
			
			// Begin processing outer polygons
			while(todo.Count > 0)
			{
				// Get outer polygon to process
				EarClipPolygon p = todo.Dequeue();

				// Any inner polygons to work with?
				if(p.Children.Count > 0)
				{
					// Go for all the children
					foreach(EarClipPolygon c in p.Children)
					{
						// The children of the children are outer polygons again,
						// so move them to the root and add for processing
						polys.AddRange(c.Children);
						foreach(EarClipPolygon sc in c.Children) todo.Enqueue(sc);

						// Remove from inner polygon
						c.Children.Clear();
					}

					// Now do some cutting on this polygon to merge the inner polygons
					MergeInnerPolys(p);
				}
			}
		}

		// This takes an outer polygon and a set of inner polygons to start cutting on
		private static void MergeInnerPolys(EarClipPolygon p)
		{
			LinkedList<EarClipPolygon> todo = new LinkedList<EarClipPolygon>(p.Children);

			// Continue until no more inner polygons to process
			while(todo.Count > 0)
			{
				// Find the inner polygon with the highest x vertex
				LinkedListNode<EarClipPolygon> found = null;
				LinkedListNode<EarClipVertex> foundstart = null;
				LinkedListNode<EarClipPolygon> ip = todo.First;
				while(ip != null)
				{
					LinkedListNode<EarClipVertex> start = FindRightMostVertex(ip.Value);
					if((foundstart == null) || (start.Value.Position.x > foundstart.Value.Position.x))
					{
						// Found a better start
						found = ip;
						foundstart = start;
					}
					
					// Next!
					ip = ip.Next;
				}
				
				// Remove from todo list
				todo.Remove(found);

				// Get cut start and end
				SplitOuterWithInner(foundstart, p);
			}
			
			// Remove the children, they should be merged in the polygon by now
			p.Children.Clear();
		}

		// This finds the right-most vertex in an inner polygon to use for cut startpoint.
		private static LinkedListNode<EarClipVertex> FindRightMostVertex(EarClipPolygon p)
		{
			LinkedListNode<EarClipVertex> found = p.First;
			LinkedListNode<EarClipVertex> v = found.Next;
			
			// Go for all vertices to find the on with the biggest x value
			while(v != null)
			{
				if(v.Value.Position.x > found.Value.Position.x) found = v;
				v = v.Next;
			}

			// Return result
			return found;
		}
		
		// This finds the cut coordinates and splits the other poly with inner vertices
		private static void SplitOuterWithInner(LinkedListNode<EarClipVertex> start, EarClipPolygon p)
		{
			LinkedListNode<EarClipVertex> insertbefore = null;
			float foundu = float.MaxValue;
			Vector2D foundpos = new Vector2D();

			// Create a line from start that goes beyond the right most vertex of p
			LinkedListNode<EarClipVertex> pr = FindRightMostVertex(p);
			float startx = start.Value.Position.x;
			float endx = pr.Value.Position.x + 10.0f;
			Line2D starttoright = new Line2D(start.Value.Position, new Vector2D(endx, start.Value.Position.y));
			
			// Calculate a small bonus (0.1 mappixel)
			float bonus = starttoright.GetNearestOnLine(new Vector2D(start.Value.Position.x + 0.1f, start.Value.Position.y));
			
			// Go for all lines in the outer polygon
			LinkedListNode<EarClipVertex> v1 = p.Last;
			LinkedListNode<EarClipVertex> v2 = p.First;
			while(v2 != null)
			{
				// Check if the line goes between startx and endx
				if((v1.Value.Position.x > startx || v2.Value.Position.x > startx) &&
				   (v1.Value.Position.x < endx || v2.Value.Position.x < endx))
				{
					// Find intersection
					Line2D pl = new Line2D(v1.Value.Position, v2.Value.Position);
					float u, ul;
					pl.GetIntersection(starttoright, out u, out ul);
					if(float.IsNaN(u))
					{
						// We have found a line that is perfectly horizontal
						// (parallel to the cut scan line) Check if the line
						// is overlapping the cut scan line.
						if(v1.Value.Position.y == start.Value.Position.y)
						{
							// This is an exceptional situation which causes a bit of a problem, because
							// this could be a previously made cut, which overlaps another line from the
							// same cut and we have to determine which of the two we will join with. If we
							// pick the wrong one, the polygon is no longer valid and triangulation will fail.
							
							// Calculate distance of each vertex in units
							u = starttoright.GetNearestOnLine(v1.Value.Position);
							ul = starttoright.GetNearestOnLine(v2.Value.Position);
							
							// Rule out vertices before the scan line
							if(u < 0.0f) u = float.MaxValue;
							if(ul < 0.0f) ul = float.MaxValue;
							
							float insert_u = Math.Min(u, ul);
							Vector2D inserpos = starttoright.GetCoordinatesAt(insert_u);
							
							// Check in which direction the line goes.
							if(v1.Value.Position.x > v2.Value.Position.x)
							{
								// The line goes from right to left (towards our start point)
								// so we must always insert our cut after this line.
								
								// If the next line goes up, we consider this a better candidate than
								// a horizontal line that goes from left to right (the other cut line)
								// so we give it a small bonus.
								LinkedListNode<EarClipVertex> v3 = v2.Next ?? v2.List.First;
								if(v3.Value.Position.y < v2.Value.Position.y)
									insert_u -= bonus;
								
								// Remember this when it is a closer match
								if(insert_u <= foundu)
								{
									insertbefore = v2.Next ?? v2.List.First;
									foundu = insert_u;
									foundpos = inserpos;
								}
							}
							else
							{
								// The line goes from left to right (away from our start point)
								// so we must always insert our cut before this line.
								
								// If the previous line goes down, we consider this a better candidate than
								// a horizontal line that goes from right to left (the other cut line)
								// so we give it a small bonus.
								LinkedListNode<EarClipVertex> v3 = v1.Previous ?? v1.List.Last;
								if(v3.Value.Position.y > v1.Value.Position.y)
									insert_u -= bonus;
								
								// Remember this when it is a closer match
								if(insert_u <= foundu)
								{
									insertbefore = v2;
									foundu = insert_u;
									foundpos = inserpos;
								}
							}
						}
					}
					// Found a closer match?
					else if((ul >= 0.0f) && (ul <= 1.0f) && (u > 0.0f) && (u <= foundu))
					{
						// Found a closer intersection
						insertbefore = v2;
						foundu = u;
						foundpos = starttoright.GetCoordinatesAt(u);
					}
				}
				
				// Next
				v1 = v2;
				v2 = v2.Next;
			}
			
			// Found anything?
			if(insertbefore != null)
			{
				Sidedef sd = (insertbefore.Previous == null) ? insertbefore.List.Last.Value.Sidedef : insertbefore.Previous.Value.Sidedef;
				
				// Find the position where we have to split the outer polygon
				EarClipVertex split = new EarClipVertex(foundpos, null);
				
				// Insert manual split vertices
				p.AddBefore(insertbefore, new EarClipVertex(split, sd));
				
				// Start inserting from the start (do I make sense this time?)
				v1 = start;
				do
				{
					// Insert inner polygon vertex
					p.AddBefore(insertbefore, new EarClipVertex(v1.Value));
					v1 = (v1.Next ?? v1.List.First);
				} while(v1 != start);
				
				// Insert manual split vertices
				p.AddBefore(insertbefore, new EarClipVertex(start.Value, sd));
				if(split.Position != insertbefore.Value.Position)
					p.AddBefore(insertbefore, new EarClipVertex(split, sd));
			}
		}

		#endregion

		#region ================== Ear Clipping

		// This clips a polygon and returns the triangles
		// The polygon may not have any holes or islands
		// See: http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
		private int DoEarClip(EarClipPolygon poly, List<Vector2D> verticeslist, List<Sidedef> sidedefslist)
		{
			LinkedList<EarClipVertex> verts = new LinkedList<EarClipVertex>();
			List<EarClipVertex> convexes = new List<EarClipVertex>(poly.Count);
			LinkedList<EarClipVertex> reflexes = new LinkedList<EarClipVertex>();
			LinkedList<EarClipVertex> eartips = new LinkedList<EarClipVertex>();
			LinkedListNode<EarClipVertex> n2;
			EarClipVertex[] t;
			int countvertices = 0;

			// Go for all vertices to fill list
			foreach(EarClipVertex vec in poly)
				vec.SetVertsLink(verts.AddLast(vec));

			// Remove any zero-length lines, these will give problems
			LinkedListNode<EarClipVertex> n1 = verts.First;
			do
			{
				// Continue until adjacent zero-length lines are removed
				n2 = n1.Next ?? verts.First;
				Vector2D d = n1.Value.Position - n2.Value.Position;
				while((Math.Abs(d.x) < 0.00001f) && (Math.Abs(d.y) < 0.00001f))
				{
					n2.Value.Remove();
					n2 = n1.Next ?? verts.First;
					if(n2 != null) d = n1.Value.Position - n2.Value.Position; else break;
				}

				// Next!
				n1 = n2;
			}
			while(n1 != verts.First);
			
			// Optimization: Vertices which have lines with the
			// same angle are useless. Remove them!
			n1 = verts.First;
			while(n1 != null)
			{
				// Get the next vertex
				n2 = n1.Next;
				
				// Get triangle for v
				t = GetTriangle(n1.Value);
				
				// Check if both lines have the same angle
				Line2D a = new Line2D(t[0].Position, t[1].Position);
				Line2D b = new Line2D(t[1].Position, t[2].Position);
				if(Math.Abs(Angle2D.Difference(a.GetAngle(), b.GetAngle())) < 0.00001f)
				{
					// Same angles, remove vertex
					n1.Value.Remove();
				}
				
				// Next!
				n1 = n2;
			}

			// Go for all vertices to determine reflex or convex
			foreach(EarClipVertex vv in verts)
			{
				// Add to reflex or convex list
				if(IsReflex(GetTriangle(vv))) vv.AddReflex(reflexes); else convexes.Add(vv);
			}

			// Go for all convex vertices to see if they are ear tips
			foreach(EarClipVertex cv in convexes)
			{
				// Add when this is a valid ear
				t = GetTriangle(cv);
				if(CheckValidEar(t, reflexes)) cv.AddEarTip(eartips);
			}

			#if DEBUG
			if(OnShowPolygon != null) OnShowPolygon(verts);
			#endif
			
			// Process ears until done
			while((eartips.Count > 0) && (verts.Count > 2))
			{
				// Get next ear
				EarClipVertex v = eartips.First.Value;
				t = GetTriangle(v);

				// Only save this triangle when it has an area
				if(TriangleHasArea(t))
				{
					// Add ear as triangle
					AddTriangleToList(t, verticeslist, sidedefslist, (verts.Count == 3));
					countvertices += 3;
				}
				
				// Remove this ear from all lists
				v.Remove();
				EarClipVertex v1 = t[0];
				EarClipVertex v2 = t[2];

				#if DEBUG
				if(TriangleHasArea(t))
				{
					if(OnShowEarClip != null) OnShowEarClip(t, verts);
				}
				#endif
				
				// Test first neighbour
				EarClipVertex[] t1 = GetTriangle(v1);
				//bool t1a = true;	//TriangleHasArea(t1);
				if(/*t1a && */IsReflex(t1))
				{
					// List as reflex if not listed yet
					if(!v1.IsReflex) v1.AddReflex(reflexes);
					v1.RemoveEarTip();
				}
				else
				{
					// Remove from reflexes
					v1.RemoveReflex();
				}
				
				// Test second neighbour
				EarClipVertex[] t2 = GetTriangle(v2);
				//bool t2a = true;	//TriangleHasArea(t2);
				if(/*t2a && */IsReflex(t2))
				{
					// List as reflex if not listed yet
					if(!v2.IsReflex) v2.AddReflex(reflexes);
					v2.RemoveEarTip();
				}
				else
				{
					// Remove from reflexes
					v2.RemoveReflex();
				}
				
				// Check if any neightbour have become a valid or invalid ear
				if(!v1.IsReflex && (/*!t1a || */CheckValidEar(t1, reflexes))) v1.AddEarTip(eartips); else v1.RemoveEarTip();
				if(!v2.IsReflex && (/*!t2a || */CheckValidEar(t2, reflexes))) v2.AddEarTip(eartips); else v2.RemoveEarTip();
			}

			#if DEBUG
			if(OnShowRemaining != null) OnShowRemaining(verts);
			#endif
			
			// Dispose remaining vertices
			foreach(EarClipVertex ecv in verts) ecv.Dispose();

			// Return the number of vertices in the result
			return countvertices;
		}

		// This checks if a given ear is a valid (no intersections from reflex vertices)
		private static bool CheckValidEar(EarClipVertex[] t, LinkedList<EarClipVertex> reflexes)
		{
			//mxd
			Vector2D pos0 = t[0].Position;
			Vector2D pos1 = t[1].Position;
			Vector2D pos2 = t[2].Position;
			Vector2D vpos;
			LinkedListNode<EarClipVertex> p;
			
			// Go for all reflex vertices
			foreach(EarClipVertex rv in reflexes)
			{
				// Not one of the triangle corners?
				if((rv.Position != pos0) && (rv.Position != pos1) && (rv.Position != pos2))
				{
					// Return false on intersection

					// This checks if a point is inside a triangle
					// When the point is on an edge of the triangle, it depends on the lines
					// adjacent to the point if it is considered inside or not
					// NOTE: vertices in t must be in clockwise order!

					// If the triangle has no area, there can never be a point inside
					if(TriangleHasArea(t))
					{
						//mxd
						pos0 = t[0].Position;
						pos1 = t[1].Position;
						pos2 = t[2].Position;
						p = rv.MainListNode;
						vpos = p.Value.Position;

						//mxd. Check bounds first...
						if( vpos.x < Math.Min(pos0.x, Math.Min(pos1.x, pos2.x)) ||
							vpos.x > Math.Max(pos0.x, Math.Max(pos1.x, pos2.x)) ||
							vpos.y < Math.Min(pos0.y, Math.Min(pos1.y, pos2.y)) ||
							vpos.y > Math.Max(pos0.y, Math.Max(pos1.y, pos2.y))) continue;

						float lineside01 = Line2D.GetSideOfLine(pos0, pos1, vpos);
						float lineside12 = Line2D.GetSideOfLine(pos1, pos2, vpos);
						float lineside20 = Line2D.GetSideOfLine(pos2, pos0, vpos);
						float u_on_line = 0.5f;

						// If point p is on the line of an edge, find out where on the edge segment p is.
						if(lineside01 == 0.0f)
							u_on_line = Line2D.GetNearestOnLine(pos0, pos1, vpos);
						else if(lineside12 == 0.0f)
							u_on_line = Line2D.GetNearestOnLine(pos1, pos2, vpos);
						else if(lineside20 == 0.0f)
							u_on_line = Line2D.GetNearestOnLine(pos2, pos0, vpos);

						// If any of the lineside results are 0 then that means the point p lies on that edge and we
						// need to test if the lines adjacent to the point p are in the triangle or not.
						// If the lines are intersecting the triangle, we also consider the point inside.
						if(lineside01 == 0.0f || lineside12 == 0.0f || lineside20 == 0.0f)
						{
							// When the point p is outside the edge segment, then it is not inside the triangle
							if(u_on_line < 0.0f || u_on_line > 1.0f) continue;

							// Point p is on an edge segment. We'll have to decide by it's lines if we call it inside or outside the triangle.
							LinkedListNode<EarClipVertex> p1 = p.Previous ?? p.List.Last;
							if(LineInsideTriangle(t, vpos, p1.Value.Position)) return false;

							LinkedListNode<EarClipVertex> p2 = p.Next ?? p.List.First;
							if(LineInsideTriangle(t, vpos, p2.Value.Position)) return false;

							continue;
						}

						if(lineside01 < 0.0f && lineside12 < 0.0f && lineside20 < 0.0f) return false;
					}
				}
			}

			// Valid ear!
			return true;
		}
		
		// This returns the 3-vertex array triangle for an ear
		private static EarClipVertex[] GetTriangle(EarClipVertex v)
		{
			return new []
			{
				(v.MainListNode.Previous == null) ? v.MainListNode.List.Last.Value : v.MainListNode.Previous.Value,
				v,
				(v.MainListNode.Next == null) ? v.MainListNode.List.First.Value : v.MainListNode.Next.Value
			};
		}
		
		// This checks if a vertex is reflex (corner > 180 deg) or convex (corner < 180 deg)
		private static bool IsReflex(EarClipVertex[] t)
		{
			// Return true when corner is > 180 deg
			return (Line2D.GetSideOfLine(t[0].Position, t[2].Position, t[1].Position) < 0.0f);
		}
		
		// This checks if a line is inside a triangle (touching the triangle is allowed)
		// NOTE: We already know p1 is on an edge segment of the triangle
		private static bool LineInsideTriangle(EarClipVertex[] t, Vector2D p1, Vector2D p2)
		{
			float s01 = Line2D.GetSideOfLine(t[0].Position, t[1].Position, p2);
			float s12 = Line2D.GetSideOfLine(t[1].Position, t[2].Position, p2);
			float s20 = Line2D.GetSideOfLine(t[2].Position, t[0].Position, p2);
			float p2_on_edge = 2.0f;		// somewhere outside the 0 .. 1 range
			float p1_on_same_edge = 2.0f;
			
			// Test if p2 is inside the triangle
			if((s01 < 0.0f) && (s12 < 0.0f) && (s20 < 0.0f))
			{
				// Line is inside triangle, because p2 is
				return true;
			}

			// Test if p2 is on an edge of the triangle and if it is we would
			// like to know where on the edge segment p2 is
			if(s01 == 0.0f)
			{
				p2_on_edge = Line2D.GetNearestOnLine(t[0].Position, t[1].Position, p2);
				p1_on_same_edge = Line2D.GetSideOfLine(t[0].Position, t[1].Position, p1);
			}
			else if(s12 == 0.0f)
			{
				p2_on_edge = Line2D.GetNearestOnLine(t[1].Position, t[2].Position, p2);
				p1_on_same_edge = Line2D.GetSideOfLine(t[1].Position, t[2].Position, p1);
			}
			else if(s20 == 0.0f)
			{
				p2_on_edge = Line2D.GetNearestOnLine(t[2].Position, t[0].Position, p2);
				p1_on_same_edge = Line2D.GetSideOfLine(t[2].Position, t[0].Position, p1);
			}

			// Is p2 actually on the edge segment?
			if((p2_on_edge >= 0.0f) && (p2_on_edge <= 1.0f))
			{
				// If p1 is on the same edge (or the unlimited line of that edge)
				// then the line is not inside this triangle.
				if(p1_on_same_edge == 0.0f)
					return false;
			}
			
			// Do a complete line-triangle intersection test
			// We already know p1 is not inside the triangle (possibly on an edge)
			Line2D p = new Line2D(p1, p2);
			Line2D t01 = new Line2D(t[0].Position, t[1].Position);
			Line2D t12 = new Line2D(t[1].Position, t[2].Position);
			Line2D t20 = new Line2D(t[2].Position, t[0].Position);
			float pu, pt;
			
			//mxd. Test intersections
			if(t01.GetIntersection(p, out pu, out pt)) return true;
			if(t12.GetIntersection(p, out pu, out pt)) return true;
			if(t20.GetIntersection(p, out pu, out pt)) return true;
			
			return false;
		}
		
		// This checks if the triangle has an area greater than 0
		private static bool TriangleHasArea(EarClipVertex[] t)
		{
			Vector2D tp0 = t[0].Position;
			Vector2D tp1 = t[1].Position;
			Vector2D tp2 = t[2].Position;

			return ((tp0.x * (tp1.y - tp2.y) +
					 tp1.x * (tp2.y - tp0.y) +
					 tp2.x * (tp0.y - tp1.y)) != 0.0f);
		}
		
		// This adds an array of vertices
		private static void AddTriangleToList(EarClipVertex[] triangle, List<Vector2D> verticeslist, List<Sidedef> sidedefslist, bool last)
		{
			//mxd
			EarClipVertex v0 = triangle[0];
			EarClipVertex v1 = triangle[1];
			EarClipVertex v2 = triangle[2];
			
			// Create triangle
			verticeslist.Add(v0.Position);
			sidedefslist.Add(v0.Sidedef);
			verticeslist.Add(v1.Position);
			sidedefslist.Add(v1.Sidedef);
			verticeslist.Add(v2.Position);
			sidedefslist.Add(!last ? null : v2.Sidedef);

			// Modify the first earclipvertex of this triangle, it no longer lies along a sidedef
			v0.Sidedef = null;
		}
		
		#endregion
	}
}
