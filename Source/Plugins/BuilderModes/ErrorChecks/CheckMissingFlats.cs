#region ================== Namespaces

using CodeImp.DoomBuilder.Map;
using System.Threading;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check missing flats", true, 40)]
	public class CheckMissingFlats : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 1000;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public CheckMissingFlats() 
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

			// Go for all the sectors
			foreach(Sector s in General.Map.Map.Sectors) 
			{
				// Check floor texture
				if(s.LongFloorTexture == MapSet.EmptyLongName) SubmitResult(new ResultMissingFlat(s, false));

				// Check ceiling texture
				if(s.LongCeilTexture == MapSet.EmptyLongName) SubmitResult(new ResultMissingFlat(s, true));

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
