
#region ================== Copyright (c) 2009 Boris Iwanski

/*
 * Copyright (c) 2009 Boris Iwanski
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

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using System.Threading;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check invalid sectors", true, 300)]
	public class CheckClosedSectors : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 40;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public CheckClosedSectors()
		{
			// Total progress is done when all sectors are checked
			SetTotalProgress(General.Map.Map.Sectors.Count / PROGRESS_STEP);
		}
		
		#endregion
		
		#region ================== Methods
		
		// This runs the check
		public override void Run()
		{
			int progress = 0;
			int stepprogress = 0;
			
			// This is a simple yet effective way to check if a sector is closed.
			// Each sidedef that belongs to a sector is checked out. The lines vertices
			// are used as the key in a Dictionary. While going through all the
			// sidedefs the values associated to the keys are set with a bitwise OR.
			//
			// The idea for this method was taken from the way DEU checks for closed
			// sectors
			//
			// The starting vertex of the side gets its 1 bit set, the ending  vertex
			// of the side gets its 2 bit set.
			// The resulting dictionary will have 1, 2 or 3 as values (unlike the DEU
			// version 0 will not appear here, since we only look at sides that belong
			// to the current sector).
			// Values 1 and 2 mean that a line they belong to has no side belonging to
			// the current sector, thereby making the sector unclosed.
			// A value of 3 usually means all lines belonging to that vertex have at least
			// one side referencing the current sector. There are (rare) cases where this
			// is not true, so a second check is done for all "opposite" vertices of the
			// vertices with values 1 and 2: if the opposite vertex has a value of 3, but
			// the line between the two vertices has no side referencing the current sector,
			// then this falsely good vertex is added to the holes, too

			// Go for all the sectors
			foreach(Sector s in General.Map.Map.Sectors)
			{
				List<Vertex> foundholes = new List<Vertex>();
				Dictionary<Vertex, int> vertices = new Dictionary<Vertex, int>();

				// look at each side that belongs to the current sector
				foreach(Sidedef sd in s.Sidedefs)
				{
					// Add the lines starting vertex to the Dictionary if it's not yet present
					if(!vertices.ContainsKey(sd.Line.Start))
					{
						vertices.Add(sd.Line.Start, 0);
					}

					// Add the lines ending vertex to the Dictionary if it's not yet present
					if(!vertices.ContainsKey(sd.Line.End))
					{
						vertices.Add(sd.Line.End, 0);
					}

					// enable the 1 bit of the start vertex and the 2 bit of the end vertex of the side
					// This is from the sides point of view, where the side is always the "right" of line,
					// so start and end are flipped depending if the current side is the front of the
					// line or not
					if(sd.IsFront) 
					{
						vertices[sd.Line.Start] |= 1;
						vertices[sd.Line.End] |= 2;
					}
					else
					{
						vertices[sd.Line.End] |= 1;
						vertices[sd.Line.Start] |= 2;
					}
				}

				// look at all the vertices
				foreach(KeyValuePair<Vertex, int> kvp in vertices)
				{
					// only look at bad vertices
					if(kvp.Value != 3)
					{
						// add the current vertex to the holes list
						if(!foundholes.Contains(kvp.Key))
						{
							foundholes.Add(kvp.Key);
						}

						// this checks if any vertices slipped past the first test
						// look at all lines that share the current vertex
						foreach(Linedef cl in kvp.Key.Linedefs)
						{
							// the line does not has its front or back side referencing the current sector
							if(!((cl.Front != null && cl.Front.Sector == s) || (cl.Back != null && cl.Back.Sector == s)))
							{
								// if the opposite vertex of our current vertex is marked good something must be wrong,
								// since the above check tells us that the line bewtween the vertices has no side referencing
								// the current sector. So add the falsely marked vertex to the holes list
								if(vertices.ContainsKey(cl.Start) && vertices[cl.Start] == 3 && !foundholes.Contains(cl.Start))
								{
									foundholes.Add(cl.Start);
								}

								if(vertices.ContainsKey(cl.End) && vertices[cl.End] == 3 && !foundholes.Contains(cl.End))
								{
									foundholes.Add(cl.End);
								}
							}
						}
					}
				}

				// Add report when holes have been found
				if(foundholes.Count > 0)
				{
					SubmitResult(new ResultSectorUnclosed(s, foundholes));
				}
				//mxd. Add report when sector has less than 3 sidedefs
				else if(s.Sidedefs.Count < 3 || s.BBox.IsEmpty)
				{
					SubmitResult(new ResultSectorInvalid(s));
				}
				//mxd. Add report when sector has less than 3 linedefs
				else
				{
					HashSet<Linedef> lines = new HashSet<Linedef>();
					foreach(Sidedef side in s.Sidedefs)
						if(side.Line != null && !lines.Contains(side.Line)) lines.Add(side.Line);

					if(lines.Count < 3) SubmitResult(new ResultSectorInvalid(s));
				}

				// Handle thread interruption
				try { Thread.Sleep(0); }
				catch(ThreadInterruptedException) { return; }

				// We are making progress!
				if((++progress / PROGRESS_STEP) > stepprogress)
				{
					stepprogress = (progress / PROGRESS_STEP);
					AddProgress(1);
				}
			}
		}
		
		#endregion
	}
}
