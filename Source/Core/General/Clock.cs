
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

using SlimDX;

namespace CodeImp.DoomBuilder
{
	public static class Clock
	{
		// This queries the system for the current time
		public static long CurrentTime { get { return Configuration.Timer.ElapsedMilliseconds; } }

		//mxd. Timer needs to be reset from time to time (like, every 2 days of continuously running the editor) to prevent float precision degradation.
		internal static void Reset()
		{
			Configuration.Timer.Reset();
			Configuration.Timer.Start();
		}
	}
}
