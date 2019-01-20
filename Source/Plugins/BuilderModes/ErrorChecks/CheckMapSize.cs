using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using System.Threading;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
	[ErrorChecker("Check map size", true, 50)]
	public class CheckMapSize : ErrorChecker
	{
		private const int PROGRESS_STEP = 1000;

		public override bool SkipCheck { get { return General.Map.Config.SafeBoundary <= 0; } }

		// Constructor
		public CheckMapSize() 
		{
			// Total progress is done when all vertices are checked
			SetTotalProgress(General.Map.Map.Vertices.Count / PROGRESS_STEP);
		}

		// This runs the check
		public override void Run() 
		{
			int progress = 0;
			int stepprogress = 0;

			float minx = int.MaxValue;
			float maxx = int.MinValue;
			float miny = int.MaxValue;
			float maxy = int.MinValue;

			// Go for all vertices
			foreach(Vertex v in General.Map.Map.Vertices) 
			{
				if(v.Position.x < minx) minx = v.Position.x;
				if(v.Position.x > maxx) maxx = v.Position.x;
				if(v.Position.y < miny) miny = v.Position.y;
				if(v.Position.y > maxy) maxy = v.Position.y;

				// Handle thread interruption
				try { Thread.Sleep(0); } catch(ThreadInterruptedException) { return; }

				// We are making progress!
				if((++progress / PROGRESS_STEP) > stepprogress) 
				{
					stepprogress = (progress / PROGRESS_STEP);
					AddProgress(1);
				}
			}

			// Map elements should not be more than 32767 mu apart
			if(maxx - minx > General.Map.Config.SafeBoundary || maxy - miny > General.Map.Config.SafeBoundary)
			{
				SubmitResult(new ResultMapTooBig(new Vector2D(minx, miny), new Vector2D(maxx, maxy)));
			}
		}
	}
}
