
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Drawing;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;

#endregion

namespace CodeImp.DoomBuilder.StairSectorBuilderMode
{
	//[EditMode(DisplayName = "Stair Sector Builder",
	//          Volatile = true)]

	[EditMode(DisplayName = "Stair Sector Builder Mode",
			  SwitchAction = "stairsectorbuildermode",		// Action name used to switch to this mode
			  ButtonImage = "StairIcon.png",	// Image resource name for the button
			  ButtonOrder = int.MinValue + 500,	// Position of the button (lower is more to the left)
			  ButtonGroup = "000_editing",
			  UseByDefault = true,
			  SafeStartMode = false,
              Volatile = true )]

	public sealed class StairSectorBuilderMode : ClassicMode
	{
		#region ================== Constants

		private const float LINE_THICKNESS = 0.6f;
		private const float CONTROLPOINT_SIZE = 10.0f;
		private const int INNER_SPLINE = 0;
		private const int OUTER_SPLINE = 1;

		#endregion

		#region ================== Variables

		private StairSectorBuilderForm stairsectorbuilderform;
        private List<StairInfo> stairsectors;
		// private List<Vector2D> controlpoints;
		// private List<Vector2D> tangents;
		private List<CatmullRomSplineGroup> catmullromsplinegroups;
		private int numcontrolpoints;
		private ControlpointPointer selectedcontrolpoint;
		private bool linesconnected;
		private List<ConnectedLinedefs> connectedlinedefs;
		private bool baseheightset;
		private int oldflipping = -1;
		private List<List<Vector2D>> exactsplines;

		#endregion

		#region ================== Structures

		private struct CatmullRomSplineData
		{
			public Line2D line;
			public List<Vector2D> controlpoints;
			public List<Vector2D> tangents;
		}

		private struct CatmullRomSplineGroup
		{
			public CatmullRomSplineData[] splines;
			public Linedef sourcelinedef;
		}

		private struct ControlpointPointer
		{
			public int crsg;
			public int crsd;
			public int cp;
		}

		private struct ConnectedLinedefs
		{
			public bool closed;
			public int sector;
			public List<Linedef> linedefs;
			public Linedef firstlinedef;
		}

        private struct StairInfo
        {
            public int floorheight;
            public int ceilingheight;
            public List<List<Vector2D>> sectors;
        }


		#endregion

		#region ================== Properties

		// Just keep the base mode button checked
		public override string EditModeButtonName { get { return General.Editing.PreviousStableMode.Name; } }

		#endregion

		#region ================== Constructor / Disposer

		#endregion

		#region ================== Methods

		private void InitializeCatmullRomSplines()
		{
			numcontrolpoints = stairsectorbuilderform.NumControlPoints;

			if(General.Map.Map.GetSelectedLinedefs(true).Count <= 0) return;

			if(catmullromsplinegroups == null)
				catmullromsplinegroups = new List<CatmullRomSplineGroup>();
			else
				catmullromsplinegroups.Clear();

			List<Linedef> sourceld = new List<Linedef>(General.Map.Map.GetSelectedLinedefs(true));
			for(int l1 = 0; l1 < sourceld.Count - 1; l1++)
			{
				int l2 = l1 + 1;

				Vector2D s1 = stairsectorbuilderform.Flipping == 1 ? sourceld[l1].End.Position : sourceld[l1].Start.Position;
				Vector2D e1 = stairsectorbuilderform.Flipping == 1 ? sourceld[l1].Start.Position : sourceld[l1].End.Position;
				Vector2D s2 = stairsectorbuilderform.Flipping == 2 ? sourceld[l2].End.Position : sourceld[l2].Start.Position;
				Vector2D e2 = stairsectorbuilderform.Flipping == 2 ? sourceld[l2].Start.Position : sourceld[l2].End.Position;

				//innerline = new Line2D(sourceld[l2].Start.Position, sourceld[l1].Start.Position);
				//outerline = new Line2D(sourceld[l1].End.Position, sourceld[l2].End.Position);

				Line2D innerline = new Line2D(s2, s1);
				Line2D outerline = new Line2D(e1, e2);

				CatmullRomSplineData innerspline = new CatmullRomSplineData();
				innerspline.controlpoints = new List<Vector2D>();

				innerspline.line = innerline;
				innerspline.controlpoints.Add(new Vector2D(innerline.v1));

				for(int k = 1; k <= numcontrolpoints - 2; k++)
					innerspline.controlpoints.Add(new Vector2D(innerline.GetCoordinatesAt(1.0f / (numcontrolpoints - 1) * k)));

				innerspline.controlpoints.Add(new Vector2D(innerline.v2));

				ComputeTangents(ref innerspline);

				CatmullRomSplineData outerspline = new CatmullRomSplineData();
				outerspline.controlpoints = new List<Vector2D>();

				outerspline.line = outerline;
				outerspline.controlpoints.Add(new Vector2D(outerline.v1));

				for(int k = 1; k <= numcontrolpoints - 2; k++)
					outerspline.controlpoints.Add(new Vector2D(outerline.GetCoordinatesAt(1.0f / (numcontrolpoints - 1) * k)));

				outerspline.controlpoints.Add(new Vector2D(outerline.v2));

				ComputeTangents(ref outerspline);

				CatmullRomSplineGroup splinegroup = new CatmullRomSplineGroup();
				splinegroup.splines = new CatmullRomSplineData[2];
				splinegroup.splines[INNER_SPLINE] = innerspline;
				splinegroup.splines[OUTER_SPLINE] = outerspline;
				splinegroup.sourcelinedef = sourceld[l1];

				catmullromsplinegroups.Add(splinegroup);
			}
		}

		// Modifies the number of control points on the splines
		public void ModifyControlpointNumber(int num)
		{
			if(catmullromsplinegroups == null) return;

			for(int crsg=0; crsg < catmullromsplinegroups.Count;crsg++) {
				for(int s = 0; s < 2; s++)
				{
					List<Vector2D> verts = GenerateCatmullRom(catmullromsplinegroups[crsg].splines[s], num-1);

					catmullromsplinegroups[crsg].splines[s].controlpoints.Clear();

					foreach(Vector2D v in verts)
					{
						catmullromsplinegroups[crsg].splines[s].controlpoints.Add(v);
					}

					// catmullromsplinegroups[crsg].splines[s].controlpoints.Add(catmullromsplinegroups[crsg].splines[s].line.v1);

					// New tangents of the control points have to be calculated
					ComputeTangents(ref catmullromsplinegroups[crsg].splines[s]);
				}
			}
		}

		// Create the sector data depending on the selected tab
		public void UpdateVertexData()
		{
			// Straight stair
			if(stairsectorbuilderform.Tabs.SelectedIndex == 0)
			{
				if(General.Map.Map.SelectedSectorsCount == 0)
				{
					stairsectors = CreateStraightStairSectorsFromLines(new List<Linedef>(General.Map.Map.GetSelectedLinedefs(true)));
				}
				else
				{
					stairsectors = CreateStraightStairSectorsFromSectors(new List<Sector>(General.Map.Map.GetSelectedSectors(true)));
				}
			}
			// Curved stair
			else if(stairsectorbuilderform.Tabs.SelectedIndex == 1)
			{
				stairsectors = CreateCurvedStairSectors();
			}
			// Catmull Rom stair
			else if(stairsectorbuilderform.Tabs.SelectedIndex == 2)
			{
				if(oldflipping != stairsectorbuilderform.Flipping)
				{
					oldflipping = stairsectorbuilderform.Flipping;
					InitializeCatmullRomSplines();
				}

				exactsplines = new List<List<Vector2D>>();
				stairsectors = CreateCatmullRomStairSectors();
			}

			if(stairsectors.Count > 0)
			{
				stairsectorbuilderform.OriginalCeilingBase = stairsectors[0].ceilingheight;
				stairsectorbuilderform.OriginalFloorBase = stairsectors[0].floorheight;
			}
		}

		/*private bool CheckConnectedLines()
		{
			//List<Linedef> sourceld = new List<Linedef>();

			//if(General.Map.Map.GetSelectedLinedefs(true).Count <= 0 && General.Map.Map.SelectedSectorsCount <= 0)
			//{
			//    return false;
			//}

			// connectedlinedefs = new List<ConnectedLinedefs>();

			//foreach(Linedef ld in General.Map.Map.GetSelectedLinedefs(true))
			//{
			//    sourceld.Add(ld);
			//}

			// GetConnectedLines(ref sourceld, -1);

			return true;
		}*/

		private void GetConnectedLines(ref List<Linedef> sourceld, int sector)
		{
			List<Linedef> connectedld = new List<Linedef>();
			Linedef currentld = null;

			connectedlinedefs = new List<ConnectedLinedefs>();

			while(sourceld.Count > 0)
			{
				// List is empty
				if(connectedld.Count == 0)
				{
					connectedld.Add(sourceld[0]);
					currentld = sourceld[0];
					sourceld.RemoveAt(0);
				}

				// Find all connected linedefs starting from the start of the current linedef
				while(currentld != null)
				{
					bool found = false;

					foreach(Linedef ld in sourceld)
					{
						// if(currentld.Start.Linedefs.Contains(ld))
						if(ld.Start == currentld.Start || ld.End == currentld.Start)
						{
							// add the connected linedef to the beginning of the list
							connectedld.Insert(0, ld);
							currentld = ld;
							sourceld.Remove(ld);
							found = true;
							break;
						}
					}

					if(found == false) currentld = null;
				}

				currentld = connectedld[connectedld.Count - 1];

				// Find all connected linedefs starting from the end of the current linedef
				while(currentld != null)
				{
					bool found = false;

					foreach(Linedef ld in sourceld)
					{
						// if(currentld.End.Linedefs.Contains(ld))
						if(ld.Start == currentld.End || ld.End == currentld.End)
						{
							// add the connected linedef to the end of the list
							connectedld.Add(ld);
							currentld = ld;
							sourceld.Remove(ld);
							found = true;
							break;
						}
					}

					if(found == false) currentld = null;
				}

				ConnectedLinedefs cld = new ConnectedLinedefs();
				cld.linedefs = new List<Linedef>();

				cld.sector = sector;

				foreach(Linedef ld in connectedld)
				{
					cld.linedefs.Add(ld);
				}

				if(General.Map.Map.SelectedLinedefsCount > 0)
				{
					foreach(Linedef ld in General.Map.Map.GetSelectedLinedefs(true))
					{
						if(cld.linedefs.Contains(ld))
						{
							cld.firstlinedef = ld;
							break;
						}
					}

					// if(connectedvertices[0] == cld.firstlinedef.End.Position && connectedvertices[connectedvertices.Count-1] == cld.firstlinedef.Start.Position) {
				}
				else
				{
					cld.closed = true;
					cld.firstlinedef = cld.linedefs[cld.linedefs.Count-1];
				}

				connectedlinedefs.Add(cld);
				connectedld.Clear();
			}
		}

		// Computes the tangents of the control points of a Catmull Rom spline
		private void ComputeTangents(ref CatmullRomSplineData spline)
		{
			float f = 2.0f;
			int i;

			if(spline.tangents == null)
				spline.tangents = new List<Vector2D>();
			else
				spline.tangents.Clear();

			// Special case for the first control point
			// spline.tangents.Add(new Vector2D((spline.controlpoints[1] - spline.controlpoints[0]) / f));
			spline.tangents.Add(new Vector2D(spline.controlpoints[1] - spline.controlpoints[0]));

			for(i = 1; i < spline.controlpoints.Count - 1; i++)
			{
				spline.tangents.Add(new Vector2D((spline.controlpoints[i + 1] - spline.controlpoints[i - 1]) / f));
			}

			// Special case for the last control point
			//spline.tangents.Add(new Vector2D((spline.controlpoints[i] - spline.controlpoints[i - 1]) / f));
			spline.tangents.Add(new Vector2D(spline.controlpoints[i] - spline.controlpoints[i - 1]));
		}

		// Creates sectors for a curved stair
        private List<StairInfo> CreateCurvedStairSectors()
		{
			List<List<Vector2D>> secs = new List<List<Vector2D>>();
	        int innervertexmultiplier = stairsectorbuilderform.InnerVertexMultiplier;
			int outervertexmultiplier = stairsectorbuilderform.OuterVertexMultiplier;
	        int numsteps = (int)stairsectorbuilderform.NumberOfSectors;
			List<Linedef> sourceld = new List<Linedef>();
            List<StairInfo> stairinfo = new List<StairInfo>();
            StairInfo si;
            List<List<Vector2D>> sisecs = new List<List<Vector2D>>();

			secs.Clear();

			if(General.Map.Map.GetSelectedLinedefs(true).Count <= 0)
			{
				return null;
			}

			// Add all selected linedefs to a source linedef list
			foreach(Linedef ld in General.Map.Map.GetSelectedLinedefs(true))
			{
				sourceld.Add(ld);
			}

			// Go through all source linedefs
			for(int l1 = 0; l1 < sourceld.Count - 1; l1++)
			{
				// A curve will always be made in the order the lines were selected
				int l2 = l1 + 1;
				float distance = 128;
				bool fixedcurve = true;

				Vector2D s1 = stairsectorbuilderform.Flipping == 1 ? sourceld[l1].End.Position : sourceld[l1].Start.Position;
				Vector2D e1 = stairsectorbuilderform.Flipping == 1 ? sourceld[l1].Start.Position : sourceld[l1].End.Position;
				Vector2D s2 = stairsectorbuilderform.Flipping == 2 ? sourceld[l2].End.Position : sourceld[l2].Start.Position;
				Vector2D e2 = stairsectorbuilderform.Flipping == 2 ? sourceld[l2].Start.Position : sourceld[l2].End.Position;

				Line2D innerline, outerline;
				bool clockwise;
				if(Vector2D.Distance(s1, s2) < Vector2D.Distance(e1, e2))
				{
					clockwise = true;
					innerline = new Line2D(s2, s1);
					outerline = new Line2D(e1, e2);
				}
				else // Counter clockwise
				{
					clockwise = false;
					innerline = new Line2D(e1, e2);
					outerline = new Line2D(s2, s1);
				}

				// Compute the angle of the inner and outer line
				float innerangle, outerangle;
				if(sourceld[l1].Angle == sourceld[l2].Angle && stairsectorbuilderform.Flipping != 1 && stairsectorbuilderform.Flipping != 2)
				{
					innerangle = 1;
					outerangle = 1;
					distance = 0;
					fixedcurve = false;
				}
				else
				{
					if(clockwise)
					{
						innerangle = outerangle = sourceld[l1].Angle - sourceld[l2].Angle;
						if(innerangle < 0.0f) innerangle += Angle2D.PI2;
						if(outerangle < 0.0f) outerangle += Angle2D.PI2;
					}
					else
					{
						innerangle = outerangle = sourceld[l2].Angle - sourceld[l1].Angle;
						if(innerangle < 0.0f) innerangle += Angle2D.PI2;
						if(outerangle < 0.0f) outerangle += Angle2D.PI2;
					}

					if(stairsectorbuilderform.Flipping != 0)
					{
						if(sourceld[l1].Angle == sourceld[l2].Angle)
						{
							if(stairsectorbuilderform.Flipping == 1)
							{
								innerangle = Math.Abs(innerangle - Angle2D.PI);
								outerangle = Math.Abs(outerangle - Angle2D.PI);
							}
							else if(stairsectorbuilderform.Flipping == 2)
							{
								innerangle -= Angle2D.PI;
								outerangle -= Angle2D.PI;
							}
						}
						else
						{
							innerangle = Math.Abs(innerangle - Angle2D.PI2);
							outerangle = Math.Abs(outerangle - Angle2D.PI2);
						}
					}
				}

				// Generate the vertices on the curve, and add the start and end vertices of the inner line to the list
				List<Vector2D> innervertices = GenerateCurve(innerline, numsteps * innervertexmultiplier - 1, innerangle, false, distance, fixedcurve);
				innervertices.Insert(0, innerline.v1);
				innervertices.Add(innerline.v2);

				// Generate the vertices on the curve, and add the start and end vertices of the outer line to the list
				List<Vector2D> outervertices = GenerateCurve(outerline, numsteps * outervertexmultiplier - 1, outerangle, true, distance, fixedcurve);
				outervertices.Insert(0, outerline.v1);
				outervertices.Add(outerline.v2);

                // If the vertices were created in counter-clockwise order turn them into clockwise order
                if(!clockwise)
                {
	                List<Vector2D> tmpvertices = innervertices;
                    int tmpmultiplier = innervertexmultiplier;

                    innervertices = outervertices;
                    outervertices = tmpvertices;

                    innervertexmultiplier = outervertexmultiplier;
                    outervertexmultiplier = tmpmultiplier;
                }

				// Create the sectors from the created vertices
			    for(int i = 0; i < numsteps; i++)
			    {
					List<Vector2D> newsec = new List<Vector2D>();

					// Depending on the outer and innter vertex multipliers more
					// vertices from the curve have to be added to the new sector
                    for(int k = 0; k <= outervertexmultiplier; k++)
                    {
                        newsec.Add(outervertices[i * outervertexmultiplier + k]);
                    }

					// Depending on the outer and innter vertex multipliers more
					// vertices from the curve have to be added to the new sector
                    for(int k = 0; k <= innervertexmultiplier; k++)
                    {
                        newsec.Add(innervertices[(numsteps - 1 - i) * innervertexmultiplier + k]);
                    }

					// Don't forget the add the first vertex again, to actually have
					// a whole sector
                    newsec.Add(outervertices[i * outervertexmultiplier]);

					sisecs.Add(newsec);
			    }
			}

			GetSetTextureAndHeightInfo(sourceld[0], out si);

            si.sectors = sisecs;

            stairinfo.Add(si);

			stairsectorbuilderform.StepMultiplier = sourceld.Count - 1;

			return stairinfo;
		}

		/*private List<Vector2D> GenerateCurve(Line2D line, int vertices, float angle, bool backwards)
		{
			return GenerateCurve(line, vertices, angle, backwards, 128, true);
		}*/

		// This generates the vertices to split the line with, from start to end
		private static List<Vector2D> GenerateCurve(Line2D line, int vertices, float angle, bool backwards, float distance, bool fixedcurve)
		{

			// Make list
			List<Vector2D> points = new List<Vector2D>(vertices);

			//Added by Anders Åstrand 2008-05-18
			//The formulas used are taken from http://mathworld.wolfram.com/CircularSegment.html
			//c and theta are known (length of line and angle parameter). d, R and h are
			//calculated from those two
			//If the curve is not supposed to be a circular segment it's simply deformed to fit
			//the value set for distance.

			//The vertices are generated to be evenly distributed (by angle) along the curve
			//and lastly they are rotated and moved to fit with the original line

			//calculate some identities of a circle segment (refer to the graph in the url above)
			double c = line.GetLength();
			double theta = angle;

			double d = (c / Math.Tan(theta / 2)) / 2;
			double R = d / Math.Cos(theta / 2);
			double h = R - d;

			double yDeform = fixedcurve ? 1 : distance / h;
			if(backwards) yDeform = -yDeform;

			for(int v = 1; v <= vertices; v++)
			{
				//calculate the angle for this vertex
				//the curve starts at PI/2 - theta/2 and is segmented into vertices+1 segments
				//this assumes the line is horisontal and on y = 0, the point is rotated and moved later

				double a = (Math.PI - theta) / 2 + v * (theta / (vertices + 1));

				//calculate the coordinates of the point, and distort the y coordinate
				//using the deform factor calculated above
				double x = Math.Cos(a) * R;
				double y = (Math.Sin(a) * R - d) * yDeform;

				//rotate and transform to fit original line
				Vector2D vertex = new Vector2D((float)x, (float)y).GetRotated(line.GetAngle() + Angle2D.PIHALF);
				vertex = vertex.GetTransformed(line.GetCoordinatesAt(0.5f).x, line.GetCoordinatesAt(0.5f).y, 1, 1);

				points.Add(vertex);
			}

			// Done
			return points;
		}


		// Creates stair sectors from two Catmull Rom splines
        private List<StairInfo> CreateCatmullRomStairSectors()
		{
			List<List<Vector2D>> secs = new List<List<Vector2D>>();
	        int innervertexmultiplier = stairsectorbuilderform.InnerVertexMultiplier;
			int outervertexmultiplier = stairsectorbuilderform.OuterVertexMultiplier;
			int numsteps = (int)stairsectorbuilderform.NumberOfSectors;
			List<List<Vector2D>> sisecs = new List<List<Vector2D>>();

            List<StairInfo> stairinfo = new List<StairInfo>();
            StairInfo si;

			secs.Clear();

			foreach(CatmullRomSplineGroup crsg in catmullromsplinegroups)
			{
				// Generate the vertices for both the inner and outer spline
				List<Vector2D> innervertices = GenerateCatmullRom(crsg.splines[INNER_SPLINE], numsteps * innervertexmultiplier);
				List<Vector2D> outervertices = GenerateCatmullRom(crsg.splines[OUTER_SPLINE], numsteps * outervertexmultiplier);

				// Create the data to build complete sectors from the splines
				for(int i = 0; i < numsteps; i++)
				{
					List<Vector2D> newsec = new List<Vector2D>();

					// Depending on the outer and innter vertex multipliers more
					// vertices from the splines have to be added to the new sector
					for(int k = 0; k <= outervertexmultiplier; k++)
					{
						newsec.Add(outervertices[i * outervertexmultiplier + k]);
					}

					for(int k = 0; k <= innervertexmultiplier; k++)
					{
						newsec.Add(innervertices[(numsteps - 1 - i) * innervertexmultiplier + k]);
					}

					// Don't forget the add the first vertex again, to actually have
					// a whole sector
					newsec.Add(outervertices[i * outervertexmultiplier]);

					sisecs.Add(newsec);
				}
			}

			GetSetTextureAndHeightInfo(catmullromsplinegroups[0].sourcelinedef, out si);

			si.sectors = sisecs;

			stairinfo.Add(si);

			return stairinfo;
		}

		private List<StairInfo> CreateStraightStairSectorsFromSectors(List<Sector> selectedsectors)
		{
			List<StairInfo> stairinfo = new List<StairInfo>();

			foreach(Sector s in selectedsectors)
			{
				List<Linedef> linedefs = new List<Linedef>();
				List<Linedef> fliplinedefs = new List<Linedef>();

				foreach(Sidedef sd in s.Sidedefs)
				{
					if(sd.Line.Front.Sector.Index != s.Index)
					{
						fliplinedefs.Add(sd.Line);
						sd.Line.FlipVertices();
						sd.Line.FlipSidedefs();
					}

					linedefs.Add(sd.Line);
				}

				stairinfo.AddRange(CreateStraightStairSectorsFromLines(linedefs));

				foreach(Linedef ld in fliplinedefs)
				{
					ld.FlipSidedefs();
					ld.FlipVertices();
				}
			}

			return stairinfo;
		}

		private List<StairInfo> CreateStraightStairSectorsFromLines(List<Linedef> selectedlinedefs)
		{
            List<StairInfo> stairinfo = new List<StairInfo>();
			uint numsteps = stairsectorbuilderform.NumberOfSectors;
			uint depth = stairsectorbuilderform.SectorDepth;
            int spacing = stairsectorbuilderform.Spacing;
			List<Linedef> sourceld = new List<Linedef>(selectedlinedefs);
            StairInfo si;

			GetConnectedLines(ref selectedlinedefs, -1);

            // Build an independend set of steps from each selected line
			if(!stairsectorbuilderform.SingleSectors.Checked)
			{
				// Go through each selected line
				for(int k = 0; k < sourceld.Count; k++)
				{
                    List<List<Vector2D>> sisecs = new List<List<Vector2D>>();

                    // Get the direction to build the stair to. "Flip" the vector depending on the selected build direction modifier
                    Vector2D direction = sourceld[k].Line.GetPerpendicular().GetNormal() * (stairsectorbuilderform.SideFront ? -1 : 1);

					for(int i = 0; i < numsteps; i++)
					{
						//List<DrawnVertex> vertices = new List<DrawnVertex>();
						List<Vector2D> newsec = new List<Vector2D>();

                        // Compute the position of the vertices that form the line opposite the source line
                        Vector2D v1 = sourceld[k].Start.Position + direction * depth * i;
                        Vector2D v2 = sourceld[k].End.Position + direction * depth * i;
						Vector2D v3 = v1 + direction * depth;
						Vector2D v4 = v2 + direction * depth;

                        // Apply spacing to each vertex
                        if(spacing > 0)
                        {
                            v1 += (direction * spacing) * i;
                            v2 += (direction * spacing) * i;
                            v3 += (direction * spacing) * i;
                            v4 += (direction * spacing) * i;
                        }

                        // v1 is added twice so it will actually draw a complete sector
						newsec.Add(v1);
						newsec.Add(v3);
						newsec.Add(v4);
						newsec.Add(v2);
						newsec.Add(v1);

						sisecs.Add(newsec);
					}

					GetSetTextureAndHeightInfo(sourceld[k], out si);

                    si.sectors = sisecs;

                    stairinfo.Add(si);
				}
			}
			else if(stairsectorbuilderform.SingleSectors.Checked)
			{
				Vector2D direction = new Vector2D();
				bool closed = false;

				foreach(ConnectedLinedefs cld in connectedlinedefs)
				{
					List<Vector2D> connectedvertices = new List<Vector2D>();
					List<Vector2D> connecteddirections = new List<Vector2D>();
					List<double> connectedlength = new List<double>();
                    List<List<Vector2D>> sisecs = new List<List<Vector2D>>();

					Vector2D globaldirection = Vector2D.FromAngle(cld.firstlinedef.Angle + Angle2D.DegToRad(stairsectorbuilderform.SideFront ? -90.0f : 90.0f));

					for(int i = 0; i < cld.linedefs.Count; i++)
					{
						if(cld.sector >= 0)
						{
							if(cld.linedefs[i].Front.Sector.Index == cld.sector)
							{
								if(!connectedvertices.Contains(cld.linedefs[i].End.Position))
									connectedvertices.Add(cld.linedefs[i].End.Position);

								if(!connectedvertices.Contains(cld.linedefs[i].Start.Position))
									connectedvertices.Add(cld.linedefs[i].Start.Position);
							}
							else
							{
								if(!connectedvertices.Contains(cld.linedefs[i].Start.Position))
									connectedvertices.Add(cld.linedefs[i].Start.Position);

								if(!connectedvertices.Contains(cld.linedefs[i].End.Position))
									connectedvertices.Add(cld.linedefs[i].End.Position);
							}
						}
						else
						{
							if(!connectedvertices.Contains(cld.linedefs[i].Start.Position))
								connectedvertices.Add(cld.linedefs[i].Start.Position);

							if(!connectedvertices.Contains(cld.linedefs[i].End.Position))
								connectedvertices.Add(cld.linedefs[i].End.Position);
						}
					}

					if(cld.sector >= 0)
					{
						closed = true;
					}
					else
					{
						if(connectedvertices[0] == cld.firstlinedef.End.Position && connectedvertices[connectedvertices.Count - 1] == cld.firstlinedef.Start.Position)
							closed = true;
					}
				
					for(int i = 0; i < connectedvertices.Count; i++)
					{
						if(i == 0 && closed == false)
						{
							connecteddirections.Add(Vector2D.FromAngle(Vector2D.GetAngle(connectedvertices[0], connectedvertices[1]) - Angle2D.DegToRad(stairsectorbuilderform.SideFront ? -90.0f : 90.0f)));
							connectedlength.Add(depth);
						}
						else if(i == connectedvertices.Count - 1 && closed == false)
						{
							connecteddirections.Add(Vector2D.FromAngle(Vector2D.GetAngle(connectedvertices[i], connectedvertices[i - 1]) + Angle2D.DegToRad(stairsectorbuilderform.SideFront ? -90.0f : 90.0f)));
							connectedlength.Add(depth);
						}
						else
						{
							Vector2D v1;
							Vector2D v2;

							if(closed && i == 0)
							{
								v1 = new Line2D(connectedvertices[1], connectedvertices[0]).GetPerpendicular();
								v2 = new Line2D(connectedvertices[0], connectedvertices[connectedvertices.Count - 1]).GetPerpendicular();
							}
							else if(closed && i == connectedvertices.Count - 1)
							{
								v1 = new Line2D(connectedvertices[0], connectedvertices[i]).GetPerpendicular();
								v2 = new Line2D(connectedvertices[i], connectedvertices[i - 1]).GetPerpendicular();
							}
							else
							{
								v1 = new Line2D(connectedvertices[i + 1], connectedvertices[i]).GetPerpendicular();
								v2 = new Line2D(connectedvertices[i], connectedvertices[i - 1]).GetPerpendicular();
							}

							double a = (v1.GetNormal() + v2.GetNormal()).GetAngle();
							double b = a - v1.GetNormal().GetAngle();

							double opl = Math.Tan(b) * depth;
							double distance = Math.Sqrt(depth * depth + opl * opl);
							Vector2D v3 = Vector2D.FromAngle((float)a);

							connecteddirections.Add(Vector2D.FromAngle(v3.GetAngle() + Angle2D.DegToRad(stairsectorbuilderform.SideFront ? 0.0f : 180.0f)));
							connectedlength.Add(distance);
						}
					}

					for(int i = 0; i < numsteps; i++)
					{
						List<Vector2D> newsec = new List<Vector2D>();
						float length;

						if(closed == false)
						{
							for(int k = 0; k < connectedvertices.Count; k++)
							{
								if(!stairsectorbuilderform.SingleDirection.Checked)
								{
									direction = connecteddirections[k];
									length = (float)connectedlength[k];
								}
								else
								{
									direction = globaldirection;
									length = depth;
								}

                                newsec.Add(connectedvertices[k] + direction * length * i + (direction * spacing) * i);
							}
						}

						for(int k = connectedvertices.Count - 1; k >= 0; k--)
						{
							if(!stairsectorbuilderform.SingleDirection.Checked)
							{
								direction = connecteddirections[k];
								length = (float)connectedlength[k];
							}
							else
							{
								direction = globaldirection;
								length = depth;
							}

                            newsec.Add(connectedvertices[k] + direction * length * (i + 1) + (direction * spacing) * i);
						}

						if(closed == false)
                            newsec.Add(connectedvertices[0] + direction * depth * i + (direction * spacing) * i);
						else
							newsec.Add(newsec[0]);

						// If the steps are drawn on the back side the vertices are in counter-clockwise
						// order, so reverse their order
						if(!stairsectorbuilderform.SideFront) newsec.Reverse();

						sisecs.Add(newsec);
					}

					GetSetTextureAndHeightInfo(cld.firstlinedef, out si);

                    si.sectors = sisecs;

                    stairinfo.Add(si);
				}
			}

			stairsectorbuilderform.StepMultiplier = 1;

			return stairinfo;
		}

		// Generats the Catmull Rom spline from the given control points and their tangents.
		// Returns a list of Vector2D.
		// The line between two control points is a "section". There is always one section
		// less than there are control points.
		// A "hop" is the distance between two to-be-created vertices. The distance is not
		// in map units, but a relative indication where on the line between two control
		// points the vertex will be.
		// First it is calculated in which section the vertex will be in, then the relative
		// position on the line between the control points of this section is calculated.
		// This value will be from 0.0 to 0.99999...
		// A workaround has to be done for the last vertex, which will inevitably be exactly
		// on the last control point, thus computing the wrong section and relative position
		private List<Vector2D> GenerateCatmullRom(CatmullRomSplineData crsd, int numverts)
		{
			List<Vector2D> vertices = new List<Vector2D>();
			//int sections = crsd.controlpoints.Count - 1;
			//double hop = (double)sections / numverts;
			float distance = 0.0f;
			List<float> cpdistance = new List<float>();

			// Compute the length of the whole spline and the length of the parts on the
			// spline between the control points
			for(int i = 0; i < crsd.controlpoints.Count-1; i++)
			{
				int h = (int)Vector2D.Distance(crsd.controlpoints[i], crsd.controlpoints[i + 1]);
				float dist = 0;
				List<Vector2D> dc = new List<Vector2D>();

				Vector2D p0 = crsd.controlpoints[i];
				Vector2D p1 = crsd.controlpoints[i + 1];
				Vector2D t0 = crsd.tangents[i];
				Vector2D t1 = crsd.tangents[i + 1];

				for(int k = 0; k <= h; k++)
				{
					dc.Add(Tools.HermiteSpline(p0, t0, p1, t1, (float)k / h));
				}

				for(int k = 0; k < h; k++)
				{
					dist += Vector2D.Distance(dc[k], dc[k + 1]);
				}

				distance += dist;

				cpdistance.Add(dist);

				exactsplines.Add(dc);
			}

			float unithop = distance / numverts;

			for(int i = 0; i <= numverts; i++)
			{
				int s = 0;
				float u;
				float distancefromstart = i * unithop;
				float max = 0.0f;

				// Find out what section the vertex is in
				while(max < distancefromstart)
				{
					max += cpdistance[s];
					if(max < distancefromstart) s++;

					// Work around for rounding errors
					if(s > cpdistance.Count-1)
					{
						s = cpdistance.Count - 1;
						max = distancefromstart;
					}
				}

				// Compute the u component with special cases for the first
				// and last vertex
				if(distancefromstart == 0)
					u = 0.0f;
				else if(distancefromstart == distance)
					u = 1.0f;
				else
					u = 1.0f - ((max - distancefromstart) / cpdistance[s]);

				Vector2D p0 = crsd.controlpoints[s];
				Vector2D p1 = crsd.controlpoints[s + 1];
				Vector2D t0 = crsd.tangents[s];
				Vector2D t1 = crsd.tangents[s + 1];
				vertices.Add(Tools.HermiteSpline(p0, t0, p1, t1, u));
			}

			// OLD CODE
			//for(int x = 0; x <= numverts; x++)
			//{
			//    // Compute the section the vertex will be in
			//    int s = (int)(x * hop);
			//    double u;

			//    // Workaround for the last vertex
			//    if((x * hop) - s < 0.00001f && s == sections)
			//    {
			//        u = 1.0f;
			//        s--;
			//    }
			//    else
			//    {
			//        //u = (summe - x * unithop) / Vector2D.Distance(crsd.controlpoints[cs], crsd.controlpoints[cs+1]);
			//        u = (x * hop) - s;
			//    }

			//    if(u > 1.0f) u = 1.0f;

			//    Vector2D p0 = crsd.controlpoints[s];
			//    Vector2D p1 = crsd.controlpoints[s + 1];
			//    Vector2D t0 = crsd.tangents[s];
			//    Vector2D t1 = crsd.tangents[s + 1];
			//    vertices.Add(Tools.HermiteSpline(p0, t0, p1, t1, (float)u));
			//}

			return vertices;
		}

		// Turns a position into a DrawnVertex and returns it
		private DrawnVertex SectorVertex(float x, float y)
		{
			DrawnVertex v = new DrawnVertex();

			v.stitch = true;
			v.stitchline = true;
			v.pos = new Vector2D((float)Math.Round(x, General.Map.FormatInterface.VertexDecimals), (float)Math.Round(y, General.Map.FormatInterface.VertexDecimals));

			return v;
		}

		private DrawnVertex SectorVertex(Vector2D v)
		{
			return SectorVertex(v.x, v.y);
		}

		// Remember info for floor and ceiling height. The info is retrieved
		// from either front or back of the source line, depending in what
		// direction the it should build
		// Also set the upper/lower textures to use if they aren't already
		private void GetSetTextureAndHeightInfo(Linedef ld, out StairInfo siout)
		{
			StairInfo si = new StairInfo();
			Sidedef primary;
			Sidedef secondary;

			GetSetBaseHeights(ld);

			if(stairsectorbuilderform.SideFront)
			{
				if(ld.Front == null)
				{
					primary = ld.Back;
					secondary = ld.Front;
				}
				else
				{
					primary = ld.Front;
					secondary = ld.Back;
				}
			}
			else
			{
				if(ld.Back == null)
				{
					primary = ld.Front;
					secondary = ld.Back;
				}
				else
				{
					primary = ld.Back;
					secondary = ld.Front;
				}
			}

			si.ceilingheight = primary.Sector.CeilHeight;
			si.floorheight = primary.Sector.FloorHeight;

			if(stairsectorbuilderform.UpperTextureTexture == "")
				stairsectorbuilderform.UpperTextureTexture = (secondary == null) ? primary.MiddleTexture : primary.HighTexture;

			if(stairsectorbuilderform.LowerTextureTexture == "")
				stairsectorbuilderform.LowerTextureTexture = (secondary == null) ? primary.MiddleTexture : primary.LowTexture;

			siout = si;
		}

		private void GetSetBaseHeights(Linedef ld)
		{
			if(baseheightset) return;

			baseheightset = true;

			if(stairsectorbuilderform.SideFront)
			{
				if(ld.Back == null)
				{
					stairsectorbuilderform.CeilingBase = ld.Front.Sector.CeilHeight;
					stairsectorbuilderform.FloorBase = ld.Front.Sector.FloorHeight;
				}
				else
				{
					stairsectorbuilderform.CeilingBase = ld.Back.Sector.CeilHeight;
					stairsectorbuilderform.FloorBase = ld.Back.Sector.FloorHeight;
				}
			}
			else
			{
				stairsectorbuilderform.CeilingBase = ld.Front.Sector.CeilHeight;
				stairsectorbuilderform.FloorBase = ld.Front.Sector.FloorHeight;
			}
		}

		#endregion

		#region ================== Events

		public override void OnHelp()
		{
			General.ShowHelp("e_curvelinedefs.html");
		}

		// Cancelled
		public override void OnCancel()
		{
			// Cancel base class
			base.OnCancel();

			// stairsectorbuilderform.Close();

			// Return to base mode
			General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
		}

		// Mode engages
		public override void OnEngage()
		{
			base.OnEngage();

            // If no lines are selected nothing can be done, so exit this mode immediately
            if(General.Map.Map.SelectedLinedefsCount == 0 && General.Map.Map.SelectedSectorsCount == 0)
            {
                General.Interface.DisplayStatus(StatusType.Warning, "No lines or sectors selected.");
                General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
                return;
            }

			renderer.SetPresentation(Presentation.Standard);

			linesconnected = true; //CheckConnectedLines();

			LoadPrefabs();

            // stairsectorbuilderform = BuilderPlug.Me.StairSectorBuilderForm;

            stairsectorbuilderform = new StairSectorBuilderForm();

			InitializeCatmullRomSplines();

			selectedcontrolpoint.cp = -1;
			selectedcontrolpoint.crsd = -1;
			selectedcontrolpoint.crsg = -1;

			stairsectorbuilderform.SingleSectors.Enabled = linesconnected;
			stairsectorbuilderform.DistinctBaseHeights.Checked = (General.Map.Map.SelectedLinedefsCount != 1);


			// Show toolbox window
            // stairsectorbuilderform.Show();
            stairsectorbuilderform.Show((Form)General.Interface);
		}

		// Disenagaging
		public override void OnDisengage()
		{
			base.OnDisengage();

			SavePrefabs();

			// Sort of work around so that the DB2 window won't lose focus
			General.Interface.Focus();

			if(stairsectorbuilderform != null)
			    stairsectorbuilderform.Hide();
		}

		// This applies the curves and returns to the base mode
		public override void OnAccept()
		{
			string uppertexture = stairsectorbuilderform.UpperTextureTexture;
			string lowertexture = stairsectorbuilderform.LowerTextureTexture;
			string middletexture = stairsectorbuilderform.MiddleTextureTexture;
			string olddefaulttexture = "-";
			List<Vertex> newvertices = new List<Vertex>();
			List<Linedef> newlinedefs = new List<Linedef>();
			List<Sector> allnewsectors = new List<Sector>();

			if(stairsectors == null) return;

			// Create undo
			General.Map.UndoRedo.CreateUndo("Build stair sectors");

            // Get the default settings and remember the default texture
            General.Settings.FindDefaultDrawSettings();

			if(stairsectorbuilderform.MiddleTexture)
			{
				olddefaulttexture = General.Map.Options.DefaultWallTexture;
				General.Map.Options.DefaultWallTexture = "-";
			}

    		// This creates the geometry. Uses exactly the same data used
			// for the preview
			try
			{
				foreach(StairInfo si in stairsectors)
				{
					int stepcounter = 1;

					foreach(List<Vector2D> lv in si.sectors)
					{
						List<Sector> oldsectors = new List<Sector>(General.Map.Map.Sectors);
						//List<Sector> newsectors = new List<Sector>();
						List<DrawnVertex> vertices = new List<DrawnVertex>();

						for(int i = 0; i < lv.Count; i++)
						{
							vertices.Add(SectorVertex(lv[i]));
						}

						// Draw the new sector
						if(!Tools.DrawLines(vertices)) throw new Exception("Failed drawing lines");

						// Update newly created sector(s) with the given textures and heights
						foreach(Sector s in General.Map.Map.Sectors)
						{
							if(!oldsectors.Contains(s))
							{
								allnewsectors.Add(s);

								// Apply floor heights if necessary
								if(stairsectorbuilderform.FloorHeight)
								{
									// Should each stair use individual base height or the global one?
									if(stairsectorbuilderform.DistinctBaseHeights.Checked && stairsectorbuilderform.StairType == 0)
										s.FloorHeight = si.floorheight + stairsectorbuilderform.FloorHeightModification * stepcounter;
									else
										s.FloorHeight = stairsectorbuilderform.FloorBase + stairsectorbuilderform.FloorHeightModification * stepcounter;
								}

								// Apply ceiling heights if necessary
								if(stairsectorbuilderform.CeilingHeight)
								{
									// Should each stair use individual base height or the global one?
									if(stairsectorbuilderform.DistinctBaseHeights.Checked && stairsectorbuilderform.StairType == 0)
										s.CeilHeight = si.ceilingheight + stairsectorbuilderform.CeilingHeightModification * stepcounter;
									else
										s.CeilHeight = stairsectorbuilderform.CeilingBase + stairsectorbuilderform.CeilingHeightModification * stepcounter;
								}

								// Apply floor and ceiling textures if necessary
								if(stairsectorbuilderform.FloorFlat)
									s.SetFloorTexture(stairsectorbuilderform.FloorFlatTexture);

								if(stairsectorbuilderform.CeilingFlat)
									s.SetCeilTexture(stairsectorbuilderform.CeilingFlatTexture);

								// Fix missing textures
								foreach(Sidedef sd in s.Sidedefs)
								{
									if(!newlinedefs.Contains(sd.Line)) newlinedefs.Add(sd.Line);

									if(stairsectorbuilderform.UpperTexture)
									{
										if(sd.Line.Back != null && sd.Line.Back.HighRequired()) sd.Line.Back.SetTextureHigh(uppertexture);
										if(sd.Line.Front != null && sd.Line.Front.HighRequired()) sd.Line.Front.SetTextureHigh(uppertexture);
										if(stairsectorbuilderform.UpperUnpegged) sd.Line.SetFlag(General.Map.Config.UpperUnpeggedFlag, true);
									}

									if(stairsectorbuilderform.LowerTexture)
									{
										if(sd.Line.Front != null && sd.Line.Front.LowRequired()) sd.Line.Front.SetTextureLow(lowertexture);
										if(sd.Line.Back != null && sd.Line.Back.LowRequired()) sd.Line.Back.SetTextureLow(lowertexture);
										if(stairsectorbuilderform.LowerUnpegged) sd.Line.SetFlag(General.Map.Config.LowerUnpeggedFlag, true);
									}

									if(stairsectorbuilderform.MiddleTexture)
									{
										if(sd.Line.Front != null && sd.Line.Front.MiddleRequired()) sd.Line.Front.SetTextureMid(middletexture);
										if(sd.Line.Back != null && sd.Line.Back.MiddleRequired()) sd.Line.Back.SetTextureMid(middletexture);
									}
								}
							}
						}

						stepcounter++;
					}
				}

				if(stairsectorbuilderform.MiddleTexture)
					General.Map.Options.DefaultWallTexture = olddefaulttexture;

				// Snap to map format accuracy
				General.Map.Map.SnapAllToAccuracy();

				// Clean the map up. Merge all vertices that share the same position and
				// remove looped linedefs
				foreach(Sector s in allnewsectors)
				{
					if(s.Sidedefs == null) continue;

					foreach(Sidedef sd in s.Sidedefs)
					{
						if(!newvertices.Contains(sd.Line.Start)) newvertices.Add(sd.Line.Start);
						if(!newvertices.Contains(sd.Line.End)) newvertices.Add(sd.Line.End);
					}
				}

				General.Map.Map.BeginAddRemove();
				MapSet.JoinVertices(newvertices, newvertices, false, MapSet.STITCH_DISTANCE);
				General.Map.Map.EndAddRemove();

				MapSet.RemoveLoopedLinedefs(newlinedefs);

				// Update textures
				General.Map.Data.UpdateUsedTextures();

				// Update caches
				General.Map.Map.Update();
				General.Map.IsChanged = true;
			}
			catch(Exception e)
			{
				General.Interface.DisplayStatus(StatusType.Warning, e.ToString());
				General.Map.UndoRedo.WithdrawUndo();
			}

			// Return to base mode
			General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
		}

		// Redrawing display
		public override void OnRedrawDisplay()
		{
			if(!stairsectorbuilderform.FullyLoaded) return;

			base.OnRedrawDisplay();

			renderer.RedrawSurface();

			// Render lines
			if(renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);
				renderer.Finish();
			}

			// Render things
			if(renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.Map.Things, General.Settings.ActiveThingsAlpha);
				renderer.Finish();
			}

			// Render overlay
			if(renderer.StartOverlay(true))
			{
				if(stairsectorbuilderform.Tabs.SelectedIndex == 2 && numcontrolpoints != stairsectorbuilderform.NumControlPoints)
				{
					ModifyControlpointNumber(stairsectorbuilderform.NumControlPoints);
					numcontrolpoints = stairsectorbuilderform.NumControlPoints;
				}

                // Create sector info based on the settings...
				UpdateVertexData();

				// Render the control points if the Catmull Rom spline tab is selected
				if(stairsectorbuilderform.Tabs.SelectedIndex == 2)
				{
					foreach(List<Vector2D> lv in exactsplines)
					{
						for(int i = 0; i < lv.Count - 1; i++)
						{
							renderer.RenderLine(lv[i], lv[i + 1], LINE_THICKNESS, new PixelColor(255, 128, 128, 128), true);
						}
					}

					foreach(CatmullRomSplineGroup crsg in catmullromsplinegroups)
					{
						CatmullRomSplineData[] crsd = new CatmullRomSplineData[2] { crsg.splines[INNER_SPLINE], crsg.splines[OUTER_SPLINE] };

						foreach(CatmullRomSplineData c in crsd)
						{
							for(int i = 0; i < c.controlpoints.Count; i++)
							{
								RectangleF rect = new RectangleF(c.controlpoints[i].x - CONTROLPOINT_SIZE / 2, c.controlpoints[i].y - CONTROLPOINT_SIZE / 2, 10.0f, 10.0f);

								if(i == 0) renderer.RenderRectangle(rect, 2, new PixelColor(128, 255, 0, 0), true);
								else if(i == c.controlpoints.Count -1) renderer.RenderRectangle(rect, 2, new PixelColor(128, 0, 255, 0), true);
								else renderer.RenderRectangle(rect, 2, General.Colors.Indication, true);
								//renderer.RenderLine(c.controlpoints[i], c.controlpoints[i] + c.tangents[i], LINE_THICKNESS, new PixelColor(255, 128, 128, 128), true);
							}
						}
					}
				}

				// ... and draw the preview
				foreach(StairInfo si in stairsectors)
				{
					foreach(List<Vector2D> lv in si.sectors)
					{
						for(int i = 0; i < lv.Count - 1; i++)
						{
							renderer.RenderLine(lv[i], lv[i + 1], LINE_THICKNESS, General.Colors.Highlight, true);

							RectangleF rect = new RectangleF(lv[i].x - CONTROLPOINT_SIZE / 2, lv[i].y - CONTROLPOINT_SIZE / 2, 10.0f, 10.0f);
							renderer.RenderRectangle(rect, 2, new PixelColor(128, 255, 0, 0), true);
						}
					}
				}
			
				renderer.Finish();
			}

			renderer.Present();
		}

		protected override void OnSelectBegin()
		{
			base.OnSelectBegin();

			if(stairsectorbuilderform.Tabs.SelectedIndex != 2) return;

			// Go through all Catmull Rom Spline Groups
			for(int crsg = 0; crsg < catmullromsplinegroups.Count; crsg++)
			{
				// Check inner and outer splines
				for(int crsd = 0; crsd < 2; crsd++)
				{
					// Check all control points except the first and last, they belong to the linedefs
					// and may not be moved
					for(int cp = 1; cp < catmullromsplinegroups[crsg].splines[crsd].controlpoints.Count-1; cp++)
					{
						Vector2D p = catmullromsplinegroups[crsg].splines[crsd].controlpoints[cp];

						// If the mouse is inside a control point, set the info about that control point
						// in the selectedcontrolpoint struct
						if(mousemappos.x >= p.x - CONTROLPOINT_SIZE / 2 && mousemappos.x <= p.x + CONTROLPOINT_SIZE / 2 && mousemappos.y >= p.y - CONTROLPOINT_SIZE / 2 && mousemappos.y <= p.y + CONTROLPOINT_SIZE / 2)
						{
							selectedcontrolpoint.cp = cp;
							selectedcontrolpoint.crsd = crsd;
							selectedcontrolpoint.crsg = crsg;
						}
					}
				}
			}
		}

		// When selected button is released
		protected override void OnSelectEnd()
		{
			base.OnSelectEnd();

			// No control point is selected
			selectedcontrolpoint.cp = -1;
			selectedcontrolpoint.crsd = -1;
			selectedcontrolpoint.crsg = -1;

			// Redraw
			General.Map.Map.Update();
			General.Interface.RedrawDisplay();
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			// If a control point is selected move it around
			if(selectedcontrolpoint.cp != -1)
			{
				if(General.Interface.ShiftState ^ General.Interface.SnapToGrid)
				{
					catmullromsplinegroups[selectedcontrolpoint.crsg].splines[selectedcontrolpoint.crsd].controlpoints[selectedcontrolpoint.cp] = new Vector2D(General.Map.Grid.SnappedToGrid(mousemappos).x, General.Map.Grid.SnappedToGrid(mousemappos).y);
				}
				else
				{
					catmullromsplinegroups[selectedcontrolpoint.crsg].splines[selectedcontrolpoint.crsd].controlpoints[selectedcontrolpoint.cp] = new Vector2D(mousemappos.x, mousemappos.y);
				}

				// New tangents of the control points have to be calculated
				ComputeTangents(ref catmullromsplinegroups[selectedcontrolpoint.crsg].splines[selectedcontrolpoint.crsd]);

				General.Interface.RedrawDisplay();
			}
		}

		public static void SavePrefabs()
		{
			ListDictionary prefabdata = new ListDictionary();
			int counter = 1;

			foreach(BuilderPlug.Prefab p in BuilderPlug.Me.Prefabs)
			{
				ListDictionary data = new ListDictionary();
				data.Add("name", p.name);
				data.Add("numberofsectors", p.numberofsectors);
				data.Add("outervertexmultiplier", p.outervertexmultiplier);
				data.Add("innervertexmultiplier", p.innervertexmultiplier);
				data.Add("stairtype", p.stairtype);
				data.Add("sectordepth", p.sectordepth);
				data.Add("spacing", p.spacing);
				data.Add("frontside", p.frontside);
				data.Add("singlesectors", p.singlesectors);
				data.Add("singledirection", p.singledirection);
				data.Add("distinctbaseheights", p.distinctbaseheights);
				data.Add("flipping", p.flipping);
				data.Add("numberofcontrolpoints", p.numberofcontrolpoints);
				data.Add("applyfloormod", p.applyfloormod);
				data.Add("floormod", p.floormod);
				//data.Add("floorbase", p.floorbase);
				data.Add("applyceilingmod", p.applyceilingmod);
				data.Add("ceilingmod", p.ceilingmod);
				//data.Add("ceilingbase", p.ceilingbase);
				data.Add("applyfloortexture", p.applyfloortexture);
				data.Add("floortexture", p.floortexture);
				data.Add("applyceilingtexture", p.applyceilingtexture);
				data.Add("ceilingtexture", p.ceilingtexture);
				data.Add("applyuppertexture", p.applyuppertexture);
				data.Add("uppertexture", p.uppertexture);
				data.Add("upperunpegged", p.upperunpegged);
				data.Add("applymiddletexture", p.applymiddletexture);
				data.Add("middletexture", p.middletexture);
				data.Add("applylowertexture", p.applylowertexture);
				data.Add("lowertexture", p.lowertexture);
				data.Add("lowerunpegged", p.lowerunpegged);

				prefabdata.Add("prefab" + counter, data);

				counter++;
			}

			General.Map.Options.WritePluginSetting("prefabs", prefabdata);
		}

		private void LoadPrefabs()
		{
			// load the light info from the .dbs
			ListDictionary prefabdata = (ListDictionary)General.Map.Options.ReadPluginSetting("prefabs", new ListDictionary());

			BuilderPlug.Me.Prefabs.Clear();

			foreach(DictionaryEntry prefabentry in prefabdata)
			{
				BuilderPlug.Prefab p = new BuilderPlug.Prefab();

				foreach(DictionaryEntry entry in (ListDictionary)prefabentry.Value)
				{
					if((string)entry.Key == "name") p.name = (string)entry.Value;
					if((string)entry.Key == "numberofsectors") p.numberofsectors = (int)entry.Value;
					if((string)entry.Key == "outervertexmultiplier") p.outervertexmultiplier = (int)entry.Value;
					if((string)entry.Key == "innervertexmultiplier") p.innervertexmultiplier = (int)entry.Value;
					if((string)entry.Key == "stairtype") p.stairtype = (int)entry.Value;
					if((string)entry.Key == "sectordepth") p.sectordepth = (int)entry.Value;
					if((string)entry.Key == "spacing") p.spacing = (int)entry.Value;
					if((string)entry.Key == "frontside") p.frontside = (bool)entry.Value;
					if((string)entry.Key == "singlesectors") p.singlesectors = (bool)entry.Value;
					if((string)entry.Key == "singledirection") p.singledirection = (bool)entry.Value;
					if((string)entry.Key == "distinctbaseheights") p.distinctbaseheights = (bool)entry.Value;
					if((string)entry.Key == "flipping") p.flipping = (int)entry.Value;
					if((string)entry.Key == "numberofcontrolpoints") p.numberofcontrolpoints = (int)entry.Value;
					if((string)entry.Key == "applyfloormod") p.applyfloormod = (bool)entry.Value;
					if((string)entry.Key == "floormod") p.floormod = (int)entry.Value;
					//if((string)entry.Key == "floorbase") p.floorbase = (int)entry.Value;
					if((string)entry.Key == "applyceilingmod") p.applyceilingmod = (bool)entry.Value;
					if((string)entry.Key == "ceilingmod") p.ceilingmod = (int)entry.Value;
					//if((string)entry.Key == "ceilingbase") p.ceilingbase = (int)entry.Value;
					if((string)entry.Key == "applyfloortexture") p.applyfloortexture = (bool)entry.Value;
					if((string)entry.Key == "floortexture") p.floortexture = (string)entry.Value;
					if((string)entry.Key == "applyceilingtexture") p.applyceilingtexture = (bool)entry.Value;
					if((string)entry.Key == "ceilingtexture") p.ceilingtexture = (string)entry.Value;
					if((string)entry.Key == "applyuppertexture") p.applyuppertexture = (bool)entry.Value;
					if((string)entry.Key == "upperunpegged") p.upperunpegged = (bool)entry.Value;
					if((string)entry.Key == "uppertexture") p.uppertexture = (string)entry.Value;
					if((string)entry.Key == "applymiddletexture") p.applymiddletexture = (bool)entry.Value;
					if((string)entry.Key == "middletexture") p.middletexture = (string)entry.Value;
					if((string)entry.Key == "applylowertexture") p.applylowertexture = (bool)entry.Value;
					if((string)entry.Key == "lowertexture") p.lowertexture = (string)entry.Value;
					if((string)entry.Key == "lowerunpegged") p.lowerunpegged = (bool)entry.Value;
				}

				BuilderPlug.Me.Prefabs.Add(p);
			}
		}

		#endregion
	}
}
