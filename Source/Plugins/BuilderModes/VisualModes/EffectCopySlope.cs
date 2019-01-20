#region === Copyright (c) 2010 Pascal van der Heiden ===

using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectCopySlope : SectorEffect
	{
		// Thing used to create this effect
		// The thing is in the sector that must receive the slope and the
		// Thing's arg 0 indicates the sector to copy the slope from.
		private Thing thing;
		
		// Constructor
		public EffectCopySlope(SectorData data, Thing sourcething) : base(data)
		{
			thing = sourcething;
			
			// New effect added: This sector needs an update!
			if(data.Mode.VisualSectorExists(data.Sector))
			{
				BaseVisualSector vs = (BaseVisualSector)data.Mode.GetVisualSector(data.Sector);
				vs.UpdateSectorGeometry(true);
			}
		}
		
		// This makes sure we are updated with the source linedef information
		public override void Update()
		{
			// Find tagged sector
			Sector sourcesector = null;
			foreach(Sector s in General.Map.Map.Sectors)
			{
				if(s.Tags.Contains(thing.Args[0]))
				{
					sourcesector = s;
					break;
				}
			}

			if(sourcesector != null)
			{
				SectorData sourcesectordata = data.Mode.GetSectorData(sourcesector);
				if(!sourcesectordata.Updated) sourcesectordata.Update();

				switch(thing.Type)
				{
					case 9510:
						data.Floor.plane = sourcesectordata.Floor.plane;
						break;
					case 9511:
						data.Ceiling.plane = sourcesectordata.Ceiling.plane;
						break;
				}
				
				sourcesectordata.AddUpdateSector(data.Sector, true);
			}
		}
	}
}
