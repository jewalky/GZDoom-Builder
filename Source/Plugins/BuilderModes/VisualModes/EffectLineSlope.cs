#region === Copyright (c) 2010 Pascal van der Heiden ===

using System.Collections.Generic;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectLineSlope : SectorEffect
	{
		// Linedef that is used to create this effect
		private readonly Linedef l;
		private Plane storedfloor; //mxd. SectorData recreates floor/ceiling planes before updating effects
		private Plane storedceiling; //mxd
		
		// Constructor
		public EffectLineSlope(SectorData data, Linedef sourcelinedef) : base(data)
		{
			l = sourcelinedef;
			
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
			if(l.Front == null || l.Back == null) return; //mxd
			
			// Find the vertex furthest from the line
			Vertex foundv = null;
			float founddist = -1.0f;
			foreach(Sidedef sd in data.Sector.Sidedefs)
			{
				Vertex v = sd.IsFront ? sd.Line.Start : sd.Line.End;
				float d = l.DistanceToSq(v.Position, false);
				if(d > founddist)
				{
					foundv = v;
					founddist = d;
				}
			}

			if(foundv == null) return; //mxd
			bool updatesides = false;
			
			// Align floor with back of line
			if((l.Args[0] == 1) && (l.Front.Sector == data.Sector))
			{
				Vector3D v1 = new Vector3D(l.Start.Position.x, l.Start.Position.y, l.Back.Sector.FloorHeight);
				Vector3D v2 = new Vector3D(l.End.Position.x, l.End.Position.y, l.Back.Sector.FloorHeight);
				Vector3D v3 = new Vector3D(foundv.Position.x, foundv.Position.y, data.Sector.FloorHeight);
				data.Floor.plane = (l.SideOfLine(v3) < 0.0f ? new Plane(v1, v2, v3, true) : new Plane(v2, v1, v3, true));

				//mxd. Update only when actually changed
				if(storedfloor != data.Floor.plane)
				{
					storedfloor = data.Floor.plane;
					updatesides = true;
				}
			}
			// Align floor with front of line
			else if((l.Args[0] == 2) && (l.Back.Sector == data.Sector))
			{
				Vector3D v1 = new Vector3D(l.Start.Position.x, l.Start.Position.y, l.Front.Sector.FloorHeight);
				Vector3D v2 = new Vector3D(l.End.Position.x, l.End.Position.y, l.Front.Sector.FloorHeight);
				Vector3D v3 = new Vector3D(foundv.Position.x, foundv.Position.y, data.Sector.FloorHeight);
				data.Floor.plane = (l.SideOfLine(v3) < 0.0f ? new Plane(v1, v2, v3, true) : new Plane(v2, v1, v3, true));

				//mxd. Update only when actually changed
				if(storedfloor != data.Floor.plane)
				{
					storedfloor = data.Floor.plane;
					updatesides = true;
				}
			}
			
			// Align ceiling with back of line
			if((l.Args[1] == 1) && (l.Front.Sector == data.Sector))
			{
				Vector3D v1 = new Vector3D(l.Start.Position.x, l.Start.Position.y, l.Back.Sector.CeilHeight);
				Vector3D v2 = new Vector3D(l.End.Position.x, l.End.Position.y, l.Back.Sector.CeilHeight);
				Vector3D v3 = new Vector3D(foundv.Position.x, foundv.Position.y, data.Sector.CeilHeight);
				data.Ceiling.plane = (l.SideOfLine(v3) > 0.0f ? new Plane(v1, v2, v3, false) : new Plane(v2, v1, v3, false));

				//mxd. Update only when actually changed
				if(storedceiling != data.Ceiling.plane)
				{
					storedceiling = data.Ceiling.plane;
					updatesides = true;
				}
			}
			// Align ceiling with front of line
			else if((l.Args[1] == 2) && (l.Back.Sector == data.Sector))
			{
				Vector3D v1 = new Vector3D(l.Start.Position.x, l.Start.Position.y, l.Front.Sector.CeilHeight);
				Vector3D v2 = new Vector3D(l.End.Position.x, l.End.Position.y, l.Front.Sector.CeilHeight);
				Vector3D v3 = new Vector3D(foundv.Position.x, foundv.Position.y, data.Sector.CeilHeight);
				data.Ceiling.plane = (l.SideOfLine(v3) > 0.0f ? new Plane(v1, v2, v3, false) : new Plane(v2, v1, v3, false));

				//mxd. Update only when actually changed
				if(storedceiling != data.Ceiling.plane)
				{
					storedceiling = data.Ceiling.plane;
					updatesides = true;
				}
			}

			//mxd. Update outer sidedef geometry
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

				// Update all things in the sector
				foreach(Thing t in General.Map.Map.Things)
				{
					if(t.Sector == data.Sector)
					{
						if(data.Mode.VisualThingExists(t))
						{
							// Update thing
							BaseVisualThing vt = (BaseVisualThing)data.Mode.GetVisualThing(t);
							vt.Changed = true;
						}
					}
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
