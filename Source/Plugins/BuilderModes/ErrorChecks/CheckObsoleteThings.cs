#region ================== Namespaces

using System.Threading;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check obsolete things", true, 50)]
	public class CheckObsoleteThings : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 10;

		#endregion

		#region ================== Properties

		// Only possible when the game configuration supports the use of decorate
		public override bool SkipCheck { get { return string.IsNullOrEmpty(General.Map.Config.DecorateGames); } }

		#endregion

		#region ================== Constructor / Destructor

		public CheckObsoleteThings()	
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
				ThingTypeInfo info = General.Map.Data.GetThingInfoEx(t.Type);
				if(info != null && info.IsObsolete)
				{
					SubmitResult(new ResultObsoleteThing(t, info.ObsoleteMessage));
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
