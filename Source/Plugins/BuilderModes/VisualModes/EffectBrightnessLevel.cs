#region === Copyright (c) 2010 Pascal van der Heiden ===

using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectBrightnessLevel : SectorEffect
	{
		// Linedef that is used to create this effect
		// The sector can be found by linedef.Front.Sector
		private readonly Linedef linedef;
		
		// Level planes
		private SectorLevel toplevel;
		private SectorLevel bottomlevel; //mxd
		
		// Constructor
		public EffectBrightnessLevel(SectorData data, Linedef sourcelinedef) : base(data)
		{
			linedef = sourcelinedef;

			// New effect added: This sector needs an update!
			if(data.Mode.VisualSectorExists(data.Sector))
			{
				BaseVisualSector vs = (BaseVisualSector)data.Mode.GetVisualSector(data.Sector);
				vs.UpdateSectorGeometry(false);
			}
		}
		
		// This makes sure we are updated with the source linedef information
		public override void Update()
		{
			SectorData sd = data.Mode.GetSectorData(linedef.Front.Sector);
			if(!sd.Updated) sd.Update();
			sd.AddUpdateSector(data.Sector, false);

			// Create top level?
			if(toplevel == null)
			{
				toplevel = new SectorLevel(sd.Ceiling);
				data.AddSectorLevel(toplevel);
			}

			// Update top level
			sd.Ceiling.CopyProperties(toplevel);
			toplevel.lighttype = (LightLevelType)General.Clamp(linedef.Args[1], 0, 2); //mxd
			toplevel.type = SectorLevelType.Light;

			//mxd. Create bottom level?
			if(toplevel.lighttype == LightLevelType.TYPE1)
			{
				// Create bottom level? Skip this step if there's a different light level between toplevel and bottomlevel
				if(bottomlevel == null)
				{
					bottomlevel = new SectorLevel(data.Ceiling);
					data.AddSectorLevel(bottomlevel);
				}

				// Update bottom level
				data.Ceiling.CopyProperties(bottomlevel);
				bottomlevel.type = SectorLevelType.Light;
				bottomlevel.lighttype = LightLevelType.TYPE1_BOTTOM;
				bottomlevel.plane = sd.Floor.plane.GetInverted();
			}
		}
	}
}
