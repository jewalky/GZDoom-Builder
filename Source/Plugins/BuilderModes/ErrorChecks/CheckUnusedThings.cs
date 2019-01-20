#region ================== Namespaces

using System.Collections.Generic;
using System.Threading;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check unused things", true, 50)]
	public class CheckUnusedThings : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 10;

		#endregion

		#region ================== Constructor / Destructor

		public CheckUnusedThings()	
		{
			// Total progress is done when all things are checked
			SetTotalProgress(General.Map.Map.Things.Count / PROGRESS_STEP);
		}

		#endregion

		#region ================== Methods

		// This runs the check
		public override void Run() 
		{
			int progress = 0;
			int stepprogress = 0;

			// Go for all things
			foreach(Thing t in General.Map.Map.Things) 
			{
				// Gather enabled flags
				HashSet<string> activeflags = new HashSet<string>();
				foreach(KeyValuePair<string, bool> group in t.GetFlags())
				{
					if(group.Value) activeflags.Add(group.Key);
				}

				// Check em
				List<string> warnings = ThingFlagsCompare.CheckFlags(activeflags);
				if(warnings.Count > 0)
				{
					// Got missing flags
					SubmitResult(new ResultUnusedThing(t, string.Join(" ", warnings.ToArray())));
				}

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

		#endregion
	}
}
