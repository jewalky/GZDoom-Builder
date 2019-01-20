#region === Copyright (c) 2010 Pascal van der Heiden ===

using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectThingLineSlope : SectorEffect
	{
		// Thing used to create this effect
		// The thing is in the sector that must receive the slope and the
		// Thing's arg 0 indicates the linedef to start the slope at.
		private Thing thing;
		
		// Constructor
		public EffectThingLineSlope(SectorData data, Thing sourcething) : base(data)
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
			//mxd. Skip if arg0 is 0.
			if(thing.Args[0] == 0) return;
			
			ThingData td = data.Mode.GetThingData(thing);
			Thing t = thing;

			// Find the tagged line
			Linedef ld = null;
			foreach(Linedef l in General.Map.Map.Linedefs)
			{
				if(l.Tags.Contains(t.Args[0]))
				{
					ld = l;
					break;
				}
			}
			
			if(ld != null)
			{
				if(t.Type == 9500)
				{
					// Slope the floor from the linedef to thing
					t.DetermineSector(data.Mode.BlockMap);
					if(t.Sector != null)
					{
						Vector3D v3 = new Vector3D(t.Position.x, t.Position.y, t.Position.z + t.Sector.FloorHeight);
						if(ld.SideOfLine(t.Position) < 0.0f)
						{
							Vector3D v1 = new Vector3D(ld.Start.Position.x, ld.Start.Position.y, ld.Front.Sector.FloorHeight);
							Vector3D v2 = new Vector3D(ld.End.Position.x, ld.End.Position.y, ld.Front.Sector.FloorHeight);
							SectorData sd = data.Mode.GetSectorData(ld.Front.Sector);
							sd.AddUpdateSector(data.Sector, true);
							if(!sd.Updated) sd.Update();
							td.AddUpdateSector(ld.Front.Sector, true);
							sd.Floor.plane = new Plane(v1, v2, v3, true);
						}
						else
						{
							Vector3D v1 = new Vector3D(ld.Start.Position.x, ld.Start.Position.y, ld.Back.Sector.FloorHeight);
							Vector3D v2 = new Vector3D(ld.End.Position.x, ld.End.Position.y, ld.Back.Sector.FloorHeight);
							SectorData sd = data.Mode.GetSectorData(ld.Back.Sector);
							sd.AddUpdateSector(data.Sector, true);
							if(!sd.Updated) sd.Update();
							td.AddUpdateSector(ld.Back.Sector, true);
							sd.Floor.plane = new Plane(v2, v1, v3, true);
						}
					}
				}
				else if(t.Type == 9501)
				{
					// Slope the ceiling from the linedef to thing
					t.DetermineSector(data.Mode.BlockMap);
					if(t.Sector != null)
					{
						td.AddUpdateSector(t.Sector, true);
						Vector3D v3 = new Vector3D(t.Position.x, t.Position.y, t.Position.z + t.Sector.CeilHeight);
						if(ld.SideOfLine(t.Position) < 0.0f)
						{
							Vector3D v1 = new Vector3D(ld.Start.Position.x, ld.Start.Position.y, ld.Front.Sector.CeilHeight);
							Vector3D v2 = new Vector3D(ld.End.Position.x, ld.End.Position.y, ld.Front.Sector.CeilHeight);
							SectorData sd = data.Mode.GetSectorData(ld.Front.Sector);
							sd.AddUpdateSector(data.Sector, true);
							td.AddUpdateSector(ld.Front.Sector, true);
							if(!sd.Updated) sd.Update();
							sd.Ceiling.plane = new Plane(v1, v2, v3, false);
						}
						else
						{
							Vector3D v1 = new Vector3D(ld.Start.Position.x, ld.Start.Position.y, ld.Back.Sector.CeilHeight);
							Vector3D v2 = new Vector3D(ld.End.Position.x, ld.End.Position.y, ld.Back.Sector.CeilHeight);
							SectorData sd = data.Mode.GetSectorData(ld.Back.Sector);
							sd.AddUpdateSector(data.Sector, true);
							td.AddUpdateSector(ld.Back.Sector, true);
							if(!sd.Updated) sd.Update();
							sd.Ceiling.plane = new Plane(v2, v1, v3, false);
						}
					}
				}
			}
		}
	}
}
