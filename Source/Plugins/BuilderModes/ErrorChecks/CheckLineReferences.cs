
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

using CodeImp.DoomBuilder.Map;
using System.Threading;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check line references", true, 50)]
	public class CheckLineReferences : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 1000;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public CheckLineReferences()
		{
			// Total progress is done when all lines are checked
			SetTotalProgress(General.Map.Map.Linedefs.Count / PROGRESS_STEP);
		}
		
		#endregion
		
		#region ================== Methods
		
		// This runs the check
		public override void Run()
		{
			int progress = 0;
			int stepprogress = 0;
			
			// Go for all the liendefs
			foreach(Linedef l in General.Map.Map.Linedefs)
			{
				// Line has a back side but no front side?
				if((l.Back != null) && (l.Front == null))
				{
					SubmitResult(new ResultLineMissingFront(l));
				}
				// Line has no sides at all?
				else if((l.Back == null) && (l.Front == null))
				{
					SubmitResult(new ResultLineMissingSides(l));
				}
				// Line is marked double-sided, but has only one side?
				else if(l.IsFlagSet(General.Map.Config.DoubleSidedFlag) && (l.Back == null))
				{
					SubmitResult(new ResultLineNotDoubleSided(l));
				}
				// Line is marked single-sided, and has two sides?
				else if(!l.IsFlagSet(General.Map.Config.DoubleSidedFlag) && (l.Back != null))
				{
					SubmitResult(new ResultLineNotSingleSided(l));
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
