using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class VertexData
	{
		#region ================== Variables

		// VisualMode
		private BaseVisualMode mode;

		// Vertex for which this data is
		private Vertex vertex;

		// Sectors that must be updated when this vertex is changed
		// The boolean value is the 'includeneighbours' of the UpdateSectorGeometry function which
		// indicates if the sidedefs of neighbouring sectors should also be rebuilt.
		private Dictionary<Sector, bool> updatesectors;

		#endregion

		#region ================== Properties

		public Vertex Vertex { get { return vertex; } }
		public BaseVisualMode Mode { get { return mode; } }
		public Dictionary<Sector, bool> UpdateAlso { get { return updatesectors; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public VertexData(BaseVisualMode mode, Vertex v)
		{
			// Initialize
			this.mode = mode;
			this.vertex = v;
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
