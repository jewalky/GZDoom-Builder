using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Config
{
	public class SectorEffectData
	{
		public int Effect;
		public HashSet<int> GeneralizedBits;

		public SectorEffectData()
		{
			GeneralizedBits = new HashSet<int>();
		}
	}
}
