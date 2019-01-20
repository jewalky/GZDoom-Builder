using CodeImp.DoomBuilder.Map;
using System.Threading;

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check very short linedefs", false, 10)]
	public class CheckShortLinedefs : ErrorChecker
	{
		private const int PROGRESS_STEP = 1000;

		// Only possible in UDMF map format
		public override bool SkipCheck { get { return !General.Map.UDMF; } }
		
		// Constructor
		public CheckShortLinedefs() 
		{
			// Total progress is done when all linedefs are checked
			SetTotalProgress(General.Map.Map.Linedefs.Count / PROGRESS_STEP);
		}

		// This runs the check
		public override void Run() 
		{
			int progress = 0;
			int stepprogress = 0;

			// Go for all linedefs
			foreach(Linedef l in General.Map.Map.Linedefs) 
			{
				if(l.Length < 1.0f) SubmitResult(new ResultShortLinedef(l));

				// Handle thread interruption
				try { Thread.Sleep(0); } catch(ThreadInterruptedException) { return; }

				// We are making progress!
				if((++progress / PROGRESS_STEP) > stepprogress) 
				{
					stepprogress = (progress / PROGRESS_STEP);
					AddProgress(1);
				}
			}
		}
	}
}
