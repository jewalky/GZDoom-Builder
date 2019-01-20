#region === Copyright (c) 2010 Pascal van der Heiden ===

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class ThingData
	{
		#region ================== Variables
		
		// VisualMode
		private BaseVisualMode mode;
		
		// Thing for which this data is
		private Thing thing;
		
		// Sectors that must be updated when this thing is changed
		// The boolean value is the 'includeneighbours' of the UpdateSectorGeometry function which
		// indicates if the sidedefs of neighbouring sectors should also be rebuilt.
		private Dictionary<Sector, bool> updatesectors;
		
		#endregion
		
		#region ================== Properties
		
		public Thing Thing { get { return thing; } }
		public BaseVisualMode Mode { get { return mode; } }
		public Dictionary<Sector, bool> UpdateAlso { get { return updatesectors; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ThingData(BaseVisualMode mode, Thing t)
		{
			// Initialize
			this.mode = mode;
			this.thing = t;
			this.updatesectors = new Dictionary<Sector, bool>(2);
		}
		
		#endregion
		
		#region ================== Public Methods
		
		// This adds a sector for updating
		public void AddUpdateSector(Sector s, bool includeneighbours)
		{
			updatesectors[s] = includeneighbours;
		}
		
		#endregion
	}
}
