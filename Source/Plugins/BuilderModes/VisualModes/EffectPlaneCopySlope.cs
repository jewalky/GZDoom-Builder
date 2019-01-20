using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectPlaneCopySlope : SectorEffect
	{
		// Linedef that is used to create this effect
		private readonly Linedef linedef;
		private readonly bool front;

		public EffectPlaneCopySlope(SectorData data, Linedef sourcelinedef, bool front) : base(data) 
		{
			this.linedef = sourcelinedef;
			this.front = front;

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
			Sector sourcesector = null;
			SectorData sourcesectordata = null;
			bool updatesides = false;

			// Copy slopes from tagged sectors
			//check which arguments we must use
			int floorArg = (front ? 0 : 2);
			int ceilingArg = (front ? 1 : 3);

			//find sector to align floor to
			if(linedef.Args[floorArg] > 0) 
			{
				foreach(Sector s in General.Map.Map.Sectors) 
				{
					if(s.Tags.Contains(linedef.Args[floorArg]))
					{
						sourcesector = s;
						break;
					}
				}

				if(sourcesector != null) 
				{
					sourcesectordata = data.Mode.GetSectorData(sourcesector);
					if(!sourcesectordata.Updated) sourcesectordata.Update();

					data.Floor.plane = sourcesectordata.Floor.plane;
					sourcesectordata.AddUpdateSector(data.Sector, true);

					updatesides = true;
				}
			}

			if(linedef.Args[ceilingArg] > 0) 
			{
				//find sector to align ceiling to
				if(linedef.Args[ceilingArg] != linedef.Args[floorArg]) 
				{
					sourcesector = null;

					foreach(Sector s in General.Map.Map.Sectors) 
					{
						if(s.Tags.Contains(linedef.Args[ceilingArg]))
						{
							sourcesector = s;
							break;
						}
					}

					if(sourcesector != null) 
					{
						sourcesectordata = data.Mode.GetSectorData(sourcesector);
						if(!sourcesectordata.Updated) sourcesectordata.Update();

						data.Ceiling.plane = sourcesectordata.Ceiling.plane;
						sourcesectordata.AddUpdateSector(data.Sector, true);

						updatesides = true;
					}

				} 
				else if(sourcesector != null) //ceiling uses the same sector as floor 
				{ 
					data.Ceiling.plane = sourcesectordata.Ceiling.plane;
				}
			}

			//check the flags...
			bool copyFloor = false;
			bool copyCeiling = false;

			if(linedef.Args[4] > 0 && linedef.Args[4] != 3 && linedef.Args[4] != 12) 
			{
				if(front) 
				{
					copyFloor = (linedef.Args[4] & 2) == 2;
					copyCeiling = (linedef.Args[4] & 8) == 8;
				} 
				else 
				{
					copyFloor = (linedef.Args[4] & 1) == 1;
					copyCeiling = (linedef.Args[4] & 4) == 4;
				}
			}

			// Copy slope across the line
			if((copyFloor || copyCeiling) && linedef.Front != null && linedef.Back != null)
			{
				// Get appropriate source sector data
				sourcesectordata = data.Mode.GetSectorData(front ? linedef.Back.Sector : linedef.Front.Sector);
				if(!sourcesectordata.Updated) sourcesectordata.Update();

				//copy floor slope?
				if(copyFloor)
				{
					data.Floor.plane = sourcesectordata.Floor.plane;
					sourcesectordata.AddUpdateSector(data.Sector, true);
				}

				//copy ceiling slope?
				if(copyCeiling)
				{
					data.Ceiling.plane = sourcesectordata.Ceiling.plane;
					sourcesectordata.AddUpdateSector(data.Sector, true);
				}

				updatesides = true;
			}

			// Update outer sidedef geometry
			if(updatesides)
			{
				UpdateSectorSides(data.Sector);

				// Update sectors with PlaneCopySlope Effect...
				List<SectorData> toupdate = new List<SectorData>();
				foreach(Sector s in data.UpdateAlso.Keys)
				{
					SectorData osd = data.Mode.GetSectorDataEx(s);
					if(osd == null) continue;
					foreach(SectorEffect e in osd.Effects)
					{
						if(e is EffectPlaneCopySlope)
						{
							toupdate.Add(osd);
							break;
						}
					}
				}

				// Do it in 2 steps, because SectorData.Reset() may change SectorData.UpdateAlso collection...
				foreach(SectorData sd in toupdate)
				{
					// Update PlaneCopySlope Effect...
					sd.Reset(false);

					// Update outer sides...
					UpdateSectorSides(sd.Sector);
				}
			}
		}

		//mxd
		private void UpdateSectorSides(Sector s)
		{
			foreach(Sidedef side in s.Sidedefs)
			{
				if(side.Other != null && side.Other.Sector != null && data.Mode.VisualSectorExists(side.Other.Sector))
				{
					BaseVisualSector vs = (BaseVisualSector)data.Mode.GetVisualSector(side.Other.Sector);
					vs.GetSidedefParts(side.Other).SetupAllParts();
				}
			}
		}
	}
}
