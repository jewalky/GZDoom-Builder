using System.Threading;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.BuilderModes 
{
	[ErrorChecker("Check unknown things", true, 50)]
	public class CheckUnknownThings : ErrorChecker 
	{
		
		private const int PROGRESS_STEP = 1000;

		// Constructor
		public CheckUnknownThings()	
		{
			// Total progress is done when all things are checked
			SetTotalProgress(General.Map.Map.Things.Count / PROGRESS_STEP);
		}

		// This runs the check
		public override void Run() 
		{
			int progress = 0;
			int stepprogress = 0;

			// Go for all things
			foreach(Thing t in General.Map.Map.Things) 
			{
				if(General.Map.Data.GetThingInfoEx(t.Type) == null) SubmitResult(new ResultUnknownThing(t));

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
