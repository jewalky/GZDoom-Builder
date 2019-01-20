
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

using System;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder
{
	public class StepsList : List<int>
	{
		// This returns a step higher
		public int GetNextHigher(int level)
		{
			int low = 0;
			int high = base.Count - 1;

			while(low < high)
			{
				int mid = (int)Math.Floor((low + high) * 0.5f);
				int l = base[mid];

				if(l <= level)
					low = mid + 1;
				else
					high = mid;
			}
			
			return base[high];
		}
		
		// This returns a step lower
		public int GetNextLower(int level)
		{
			int low = 0;
			int high = base.Count - 1;

			while(low < high)
			{
				int mid = (int)Math.Ceiling((low + high) * 0.5f);
				int l = base[mid];

				if(l >= level)
					high = mid - 1;
				else
					low = mid;
			}

			return base[low];
		}

		//mxd. This returns a step higher, or lowest step if level is already the highest possible value
		public int GetNextHigherWrap(int level, bool wraparound) 
		{
			int result = GetNextHigher(level);
			if(!wraparound) return result;
			return (result == level ? this[0] : result);
		}

		//mxd. This returns a step lower, or highest step if level is already the lowest possible value
		public int GetNextLowerWrap(int level, bool wraparound) 
		{
			int result = GetNextLower(level);
			if(!wraparound) return result;
			return (result == level ? this[this.Count - 1] : result);
		}

		//mxd. This returns a step higher for UDMF relative light range (-255..255)
		public int GetNextHigher(int level, bool absolute) 
		{
			if(absolute || level >= 0) return GetNextHigher(level);
			return -GetNextLower(Math.Abs(level));
		}

		//mxd. This returns a step lower for UDMF relative light range (-255..255)
		public int GetNextLower(int level, bool absolute) 
		{
			if(absolute || level > 0) return GetNextLower(level);
			return -GetNextHigher(Math.Abs(level));
		}

		// This returns the nearest step
		public int GetNearest(int level)
		{
			int low = 0;
			int high = base.Count - 1;

			while(low < high)
			{
				int mid = (int)Math.Floor((low + high) * 0.5f);
				int l = base[mid];

				if(l <= level)
					low = mid + 1;
				else
					high = mid;
			}

			// Find which one is nearest
			low = (high > 0) ? (high - 1) : 0;
			int dlow = level - base[low];
			int dhigh = base[high] - level;
			return (dlow < dhigh) ? base[low] : base[high];
		}
	}
}
