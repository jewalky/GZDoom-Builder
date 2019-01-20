#region === Copyright (c) 2010 Pascal van der Heiden ===

using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using System;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectThingSlope : SectorEffect
	{
		// Thing used to create this effect
		// The thing is in the sector that must receive the slope
		private Thing thing;

		// Constructor
		public EffectThingSlope(SectorData data, Thing sourcething) : base(data)
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
			ThingData td = data.Mode.GetThingData(thing);
			Thing t = thing;

			// Floor slope thing
			if(t.Type == 9502)
			{
				t.DetermineSector(data.Mode.BlockMap);
				if(t.Sector != null)
				{
					//mxd. Vertex zheight overrides this effect
					if(General.Map.UDMF && t.Sector.Sidedefs.Count == 3) 
					{
						foreach(Sidedef side in t.Sector.Sidedefs) 
						{
							if(!float.IsNaN(side.Line.Start.ZFloor) || !float.IsNaN(side.Line.End.ZFloor)) 
								return;
						}
					}

					float angle = Angle2D.DoomToReal((int)Angle2D.RadToDeg(t.Angle));
					float vangle = Angle2D.DegToRad(General.Clamp(t.Args[0], 0, 180)); //mxd. Don't underestimate user stupidity (or curiosity)!
					Vector2D point = new Vector2D(t.Position.x + (float)Math.Cos(angle) * (float)Math.Sin(vangle), t.Position.y + (float)Math.Sin(angle) * (float)Math.Sin(vangle));
					Vector2D perpendicular = new Line2D(t.Position, point).GetPerpendicular();

					Vector3D v1 = new Vector3D(t.Position.x, t.Position.y, t.Position.z + t.Sector.FloorHeight);

					Vector3D v2 = new Vector3D(
						point.x + perpendicular.x,
						point.y + perpendicular.y,
						t.Position.z + t.Sector.FloorHeight + (float)Math.Cos(vangle)
					);

					Vector3D v3 = new Vector3D(
						point.x - perpendicular.x,
						point.y - perpendicular.y,
						t.Position.z + t.Sector.FloorHeight + (float)Math.Cos(vangle)
					);

					SectorData sd = data.Mode.GetSectorData(t.Sector);
					sd.AddUpdateSector(data.Sector, true);
					if(!sd.Updated) sd.Update();
					td.AddUpdateSector(t.Sector, true);
					sd.Floor.plane = new Plane(v1, v2, v3, true);
				}
			}
			// Ceiling slope thing
			else if(t.Type == 9503)
			{
				t.DetermineSector(data.Mode.BlockMap);
				if(t.Sector != null)
				{
					//mxd. Vertex zheight overrides this effect
					if(General.Map.UDMF && t.Sector.Sidedefs.Count == 3) 
					{
						foreach(Sidedef side in t.Sector.Sidedefs) 
						{
							if(!float.IsNaN(side.Line.Start.ZCeiling) || !float.IsNaN(side.Line.End.ZCeiling))
								return;
						}
					}
					
					float angle = Angle2D.DoomToReal((int)Angle2D.RadToDeg(t.Angle));
					float vangle = Angle2D.DegToRad(General.Clamp(t.Args[0], 0, 180)); //mxd. Don't underestimate user stupidity (or curiosity)!
					Vector2D point = new Vector2D(t.Position.x + (float)Math.Cos(angle) * (float)Math.Sin(vangle), t.Position.y + (float)Math.Sin(angle) * (float)Math.Sin(vangle));
					Vector2D perpendicular = new Line2D(t.Position, point).GetPerpendicular();

					Vector3D v1 = new Vector3D(t.Position.x, t.Position.y, t.Position.z + t.Sector.CeilHeight);

					Vector3D v2 = new Vector3D(
						point.x + perpendicular.x,
						point.y + perpendicular.y,
						t.Position.z + t.Sector.CeilHeight + (float)Math.Cos(vangle)
					);

					Vector3D v3 = new Vector3D(
						point.x - perpendicular.x,
						point.y - perpendicular.y,
						t.Position.z + t.Sector.CeilHeight + (float)Math.Cos(vangle)
					);

					SectorData sd = data.Mode.GetSectorData(t.Sector);
					sd.AddUpdateSector(data.Sector, true);
					if(!sd.Updated) sd.Update();
					td.AddUpdateSector(t.Sector, true);
					sd.Ceiling.plane = new Plane(v1, v2, v3, false);
				}
			}

		}
	}
}
