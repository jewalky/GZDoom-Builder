
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
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Config;
using System.Threading;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check stuck things", true, 1000)]
	public class CheckStuckThings : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 10;
		private const float ALLOWED_STUCK_DISTANCE = 6.0f;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public CheckStuckThings()
		{
			// Total progress is done when all things are checked
			SetTotalProgress(General.Map.Map.Things.Count / PROGRESS_STEP);
		}
		
		#endregion
		
		#region ================== Methods
		
		// This runs the check
		public override void Run()
		{
			BlockMap<BlockEntry> blockmap = BuilderPlug.Me.ErrorCheckForm.BlockMap;
			int progress = 0;
			int stepprogress = 0;
			float maxradius = 0;
			Dictionary<int, HashSet<int>> processedthingpairs = new Dictionary<int, HashSet<int>>(); //mxd

			foreach(ThingTypeInfo tti in General.Map.Data.ThingTypes)
			{
				if(tti.Radius > maxradius) maxradius = tti.Radius;
			}

			// Go for all the things
			foreach(Thing t in General.Map.Map.Things)
			{
				ThingTypeInfo info = General.Map.Data.GetThingInfo(t.Type);
				bool stuck = false;

				// Check this thing for getting stuck?
				if( (info.ErrorCheck == ThingTypeInfo.THING_ERROR_INSIDE_STUCK) &&
					(info.Blocking > ThingTypeInfo.THING_BLOCKING_NONE))
				{
					// Make square coordinates from thing
					float blockingsize = t.Size - ALLOWED_STUCK_DISTANCE;
					Vector2D lt = new Vector2D(t.Position.x - blockingsize, t.Position.y - blockingsize);
					Vector2D rb = new Vector2D(t.Position.x + blockingsize, t.Position.y + blockingsize);
					Vector2D bmlt = new Vector2D(t.Position.x - maxradius, t.Position.y - maxradius);
					Vector2D bmrb = new Vector2D(t.Position.x + maxradius, t.Position.y + maxradius);

					// Go for all the lines to see if this thing is stuck
					List<BlockEntry> blocks = blockmap.GetSquareRange(new RectangleF(bmlt.x, bmlt.y, (bmrb.x - bmlt.x), (bmrb.y - bmlt.y)));
					Dictionary<Linedef, Linedef> doneblocklines = new Dictionary<Linedef, Linedef>(blocks.Count * 3);

					foreach(BlockEntry b in blocks)
					{
						foreach(Linedef l in b.Lines)
						{
							// Only test when sinlge-sided, two-sided + impassable and not already checked
							if(((l.Back == null) || l.IsFlagSet(General.Map.Config.ImpassableFlag)) && !doneblocklines.ContainsKey(l))
							{
								// Test if line ends are inside the thing
								if(PointInRect(lt, rb, l.Start.Position) || PointInRect(lt, rb, l.End.Position))
								{
									// Thing stuck in line!
									stuck = true;
									SubmitResult(new ResultStuckThingInLine(t, l));
								}
								// Test if the line intersects the square
								else if(Line2D.GetIntersection(l.Start.Position, l.End.Position, lt.x, lt.y, rb.x, lt.y) ||
										Line2D.GetIntersection(l.Start.Position, l.End.Position, rb.x, lt.y, rb.x, rb.y) ||
										Line2D.GetIntersection(l.Start.Position, l.End.Position, rb.x, rb.y, lt.x, rb.y) ||
										Line2D.GetIntersection(l.Start.Position, l.End.Position, lt.x, rb.y, lt.x, lt.y))
								{
									// Thing stuck in line!
									stuck = true;
									SubmitResult(new ResultStuckThingInLine(t, l));
								}
								
								// Checked
								doneblocklines.Add(l, l);
							}
						}

						// Check if thing is stuck in other things
						if(info.Blocking != ThingTypeInfo.THING_BLOCKING_NONE) 
						{
							foreach(Thing ot in b.Things)
							{
								// Don't compare the thing with itself
								if(t.Index == ot.Index) continue;

								// mxd. Don't compare already processed stuff
								if(processedthingpairs.ContainsKey(t.Index) && processedthingpairs[t.Index].Contains(ot.Index)) continue;

								// Only check of items that can block
								if(General.Map.Data.GetThingInfo(ot.Type).Blocking == ThingTypeInfo.THING_BLOCKING_NONE) continue;

								// need to compare the flags
								if(FlagsOverlap(t, ot) && ThingsOverlap(t, ot))
								{
									stuck = true;
									SubmitResult(new ResultStuckThingInThing(t, ot));
								}

								//mxd. Prepare collections
								if(!processedthingpairs.ContainsKey(t.Index)) processedthingpairs.Add(t.Index, new HashSet<int>());
								if(!processedthingpairs.ContainsKey(ot.Index)) processedthingpairs.Add(ot.Index, new HashSet<int>());

								//mxd. Add both ways
								processedthingpairs[t.Index].Add(ot.Index);
								processedthingpairs[ot.Index].Add(t.Index);
							}
						}
					}
				}

				// Check this thing for being outside the map?
				if(!stuck && info.ErrorCheck >= ThingTypeInfo.THING_ERROR_INSIDE) 
				{
					// Get the nearest line to see if the thing is outside the map
					bool outside;
					Linedef l = General.Map.Map.NearestLinedef(t.Position);
					if(l.SideOfLine(t.Position) <= 0) 
					{
						outside = (l.Front == null);
					} 
					else 
					{
						outside = (l.Back == null);
					}

					// Outside the map?
					if(outside) 
					{
						// Make result
						SubmitResult(new ResultThingOutside(t));
					}
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
		
		// Point in rect?
		private static bool PointInRect(Vector2D lt, Vector2D rb, Vector2D p)
		{
			return (p.x >= lt.x) && (p.x <= rb.x) && (p.y <= lt.y) && (p.y >= rb.y);
		}

		// Checks if two things overlap
		private static bool ThingsOverlap(Thing t1, Thing t2)
		{
			Vector3D p1 = t1.Position;
			Vector3D p2 = t2.Position;
			ThingTypeInfo t1info = General.Map.Data.GetThingInfo(t1.Type);
			ThingTypeInfo t2info = General.Map.Data.GetThingInfo(t2.Type);
	
			// simple bounding box collision detection
			if(		p1.x + t1.Size - ALLOWED_STUCK_DISTANCE < p2.x - t2.Size + ALLOWED_STUCK_DISTANCE ||
					p1.x - t1.Size + ALLOWED_STUCK_DISTANCE > p2.x + t2.Size - ALLOWED_STUCK_DISTANCE ||
					p1.y - t1.Size + ALLOWED_STUCK_DISTANCE > p2.y + t2.Size - ALLOWED_STUCK_DISTANCE ||
					p1.y + t1.Size - ALLOWED_STUCK_DISTANCE < p2.y - t2.Size + ALLOWED_STUCK_DISTANCE)
				return false;

			// if either thing blocks full height there's no need to check the z-axis
			if(t1info.Blocking == ThingTypeInfo.THING_BLOCKING_FULL || t2info.Blocking == ThingTypeInfo.THING_BLOCKING_FULL)
				return true;

			// check z-axis
			if(p1.z > p2.z + t2info.Height || p1.z + t1info.Height < p2.z)
				return false;

			return true;
		}
		
		// Checks if the flags of two things overlap (i.e. if they show up at the same time)
		private static bool FlagsOverlap(Thing t1, Thing t2) 
		{
			if(General.Map.Config.ThingFlagsCompare.Count < 1) return true; //mxd. Otherwise, no things will ever overlap when ThingFlagsCompare is empty
			Dictionary<string, ThingFlagsCompareResult> results = new Dictionary<string, ThingFlagsCompareResult>(General.Map.Config.ThingFlagsCompare.Count);

			// Go through all flags in all groups and check if they overlap
			foreach(ThingFlagsCompareGroup group in General.Map.Config.ThingFlagsCompare.Values)
				results[group.Name] = group.Compare(t1, t2);

			// Process Required/IgnoredGroups
			foreach(ThingFlagsCompareResult result in results.Values)
			{
				// Group matters only when it contains overlapping flags
				if(result.Result == 1)
				{
					// Ignore this group when RequiredGroup flags don't overlap
					foreach(string requiredgroup in result.RequiredGroups)
					{
						if(results[requiredgroup].Result != 1)
						{
							result.Result = 0;
							break;
						}
					}

					// Ignore other groups when this one's flags overlap
					if(result.Result == 1)
					{
						foreach(string ignoredgroup in result.IgnoredGroups)
							results[ignoredgroup].Result = 0;
					}
				}
			}

			// Count overlapping groups
			int overlappinggroupscount = 0;
			int totalgroupscount = results.Values.Count;
			foreach(ThingFlagsCompareResult result in results.Values)
			{
				switch(result.Result)
				{
					case  1: overlappinggroupscount++; break; // Group flags overlap
					case  0: totalgroupscount--; break; // Ignored group should be ignored
					case -1: return false; // Group flags don't overlap
					default: throw new NotImplementedException("Unknown thing flags comparison result");
				}
			}

			// All groups have to overlap for the things to show up at the same time
			return (totalgroupscount > 0 && overlappinggroupscount == totalgroupscount);
		}
		
		#endregion
	}
}
