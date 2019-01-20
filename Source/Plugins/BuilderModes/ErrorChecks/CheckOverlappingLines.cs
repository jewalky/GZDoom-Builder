
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
using System.Threading;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check overlapping lines", true, 500)]
	public class CheckOverlappingLines : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 100;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public CheckOverlappingLines()
		{
			// Total progress is done when all lines are checked
			SetTotalProgress(General.Map.Map.Linedefs.Count / PROGRESS_STEP);
		}
		
		#endregion
		
		#region ================== Methods
		
		// This runs the check
		public override void Run()
		{
			Dictionary<Linedef, Linedef> donelines = new Dictionary<Linedef, Linedef>();
			BlockMap<BlockEntry> blockmap = BuilderPlug.Me.ErrorCheckForm.BlockMap;
			int progress = 0;
			int stepprogress = 0;

			// Go for all the liendefs
			foreach(Linedef l in General.Map.Map.Linedefs)
			{
				// Check if not already done
				if(!donelines.ContainsKey(l))
				{
					// And go for all the linedefs that could overlap
					List<BlockEntry> blocks = blockmap.GetLineBlocks(l.Start.Position, l.End.Position);
					Dictionary<Linedef, Linedef> doneblocklines = new Dictionary<Linedef, Linedef>(blocks.Count * 3);
					foreach(BlockEntry b in blocks)
					{
						foreach(Linedef d in b.Lines)
						{
							// Not the same line and not already checked
							if(!object.ReferenceEquals(l, d) && !doneblocklines.ContainsKey(d))
							{
								float lu, du;
								
								//mxd. This can also happen. I suppose. Some people manage to do this. I dunno how, but they do...
								if((l.Start.Position == d.Start.Position && l.End.Position == d.End.Position)
									|| (l.Start.Position == d.End.Position && l.End.Position == d.Start.Position)) 
								{
									SubmitResult(new ResultLineOverlapping(l, d));
									donelines[d] = d;
								} 
								else if(l.Line.GetIntersection(d.Line, out du, out lu)) 
								{
									// Check if the lines touch. Note that I don't include 0.0 and 1.0 here because
									// the lines may be touching at the ends when sharing the same vertex.
									if(General.Map.FormatInterface.VertexDecimals > 0) //mxd
									{
										lu = (float)Math.Round(lu, General.Map.FormatInterface.VertexDecimals);
										du = (float)Math.Round(du, General.Map.FormatInterface.VertexDecimals);
									}
									
									if((lu > 0.0f) && (lu < 1.0f) && (du > 0.0f) && (du < 1.0f))
									{
										// Check if not the same sector on all sides
										Sector samesector = null;
										if(l.Front != null) samesector = l.Front.Sector;
										else if(l.Back != null) samesector = l.Back.Sector;
										else if(d.Front != null) samesector = d.Front.Sector;
										else if(d.Back != null) samesector = d.Back.Sector;
										
										if((l.Front == null) || (l.Front.Sector != samesector)) samesector = null;
										else if((l.Back == null) || (l.Back.Sector != samesector)) samesector = null;
										else if((d.Front == null) || (d.Front.Sector != samesector)) samesector = null;
										else if((d.Back == null) || (d.Back.Sector != samesector)) samesector = null;

										if(samesector == null)
										{
											SubmitResult(new ResultLineOverlapping(l, d));
											donelines[d] = d;
										}
									}
								}
								
								// Checked
								doneblocklines.Add(d, d);
							}
						}
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
		
		#endregion
	}
}
