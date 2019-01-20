#region === Copyright (c) 2010 Pascal van der Heiden ===

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal abstract class SectorEffect
	{
		protected readonly SectorData data;
		
		// Constructor
		protected SectorEffect(SectorData data)
		{
			this.data = data;
		}

		// This makes sure we are updated with the source linedef information
		public abstract void Update();
	}
}
