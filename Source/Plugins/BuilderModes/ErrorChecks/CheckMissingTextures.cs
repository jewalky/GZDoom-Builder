
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
	[ErrorChecker("Check missing textures", true, 80)]
	public class CheckMissingTextures : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 1000;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public CheckMissingTextures()
		{
			// Total progress is done when all lines are checked
			SetTotalProgress(General.Map.Map.Sidedefs.Count / PROGRESS_STEP);
		}

		#endregion

		#region ================== Methods

		// This runs the check
		public override void Run()
		{
			int progress = 0;
			int stepprogress = 0;

			// Go for all the sidedefs
			foreach(Sidedef sd in General.Map.Map.Sidedefs)
			{
				// Check upper texture. Also make sure not to return a false
				// positive if the sector on the other side has the ceiling
				// set to be sky
				if(sd.HighRequired() && sd.HighTexture == "-")
				{
					if(sd.Line.Action == 181 && sd.Line.Args[1] > 0) continue; //mxd. Ceiling slopes doesn't require upper texture
					if(sd.Other != null && sd.Other.Sector.CeilTexture != General.Map.Config.SkyFlatName)
					{
						SubmitResult(new ResultMissingTexture(sd, SidedefPart.Upper));
					}
				}

				// Check middle texture
				if(sd.MiddleRequired() && sd.MiddleTexture == "-")
				{
					SubmitResult(new ResultMissingTexture(sd, SidedefPart.Middle));
				}

				// Check lower texture. Also make sure not to return a false
				// positive if the sector on the other side has the floor
				// set to be sky
				if(sd.LowRequired() && sd.LowTexture == "-")
				{
					if(sd.Line.Action == 181 && sd.Line.Args[0] > 0) continue; //mxd. Floor slopes doesn't require lower texture
					if(sd.Other != null && sd.Other.Sector.FloorTexture != General.Map.Config.SkyFlatName)
					{
						SubmitResult(new ResultMissingTexture(sd, SidedefPart.Lower));
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
