#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using System.Threading;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check overlapping vertices", true, 500)]
	public class CheckOverlappingVertices : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 100;

		#endregion
		
		#region ================== Constructor / Destructor

		// Constructor
		public CheckOverlappingVertices() 
		{
			// Total progress is done when all lines are checked
			SetTotalProgress(General.Map.Map.Vertices.Count / PROGRESS_STEP);
		}
		
		#endregion
		
		#region ================== Methods
		
		// This runs the check
		public override void Run() 
		{
			BlockMap<BlockEntry> blockmap = BuilderPlug.Me.ErrorCheckForm.BlockMap;
			Dictionary<Vertex, List<Vertex>> doneverts = new Dictionary<Vertex, List<Vertex>>();
			int progress = 0;
			int stepprogress = 0;

			// Go for all the verts
			foreach(Vertex v in General.Map.Map.Vertices) 
			{
				BlockEntry block = blockmap.GetBlockAt(v.Position);

				// Go for all the linedefs that our vertex could overlap
				foreach(Linedef l in block.Lines) 
				{
					if(v == l.Start || v == l.End) continue;
					if((float)Math.Round(l.Line.GetDistanceToLine(v.Position, true), 3) == 0)
						SubmitResult(new ResultVertexOverlappingLine(v, l));
				}

				// Go for all the verts that our vertex could overlap
				foreach(Vertex bv in block.Vertices) 
				{
					if(bv == v || (doneverts.ContainsKey(v) && doneverts[v].Contains(bv)) || (doneverts.ContainsKey(bv) && doneverts[bv].Contains(v))) continue;
					if(bv.Position == v.Position) SubmitResult(new ResultVertexOverlappingVertex(v, bv));
					if(!doneverts.ContainsKey(v)) doneverts.Add(v, new List<Vertex>());
					doneverts[v].Add(bv);
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
