#region === Copyright (c) 2010 Pascal van der Heiden ===

using System.Collections.Generic;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectThingVertexSlope : SectorEffect
	{
		// Thing used to create this effect
		// The thing is in the sector that must receive the slope and the
		// Thing's arg 0 indicates the linedef to start the slope at.
		private List<Thing> things;
		
		// Floor or ceiling?
		private bool slopefloor;
		
		// Constructor
		public EffectThingVertexSlope(SectorData data, List<Thing> sourcethings, bool floor) : base(data)
		{
			things = sourcethings;
			slopefloor = floor;
			
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
			// Create vertices in clockwise order
			Vector3D[] verts = new Vector3D[3];
			int index = 0;
			foreach(Sidedef sd in data.Sector.Sidedefs)
			{
				Vertex v = sd.IsFront ? sd.Line.End : sd.Line.Start;
				
				// Presume not sloped
				if(slopefloor)
					verts[index] = new Vector3D(v.Position.x, v.Position.y, data.Floor.plane.GetZ(v.Position));
				else
					verts[index] = new Vector3D(v.Position.x, v.Position.y, data.Ceiling.plane.GetZ(v.Position));

				//mxd. UDMF vertex offset overrides this effect
				if(General.Map.UDMF) 
				{
					if((slopefloor && !float.IsNaN(v.ZFloor)) || !float.IsNaN(v.ZCeiling)) 
					{
						index++;
						continue;
					}
				}
				
				// Find the thing at this position
				foreach(Thing t in things)
				{
					if((Vector2D)t.Position == v.Position)
					{
						ThingData td = data.Mode.GetThingData(t);
						td.AddUpdateSector(data.Sector, true);
						verts[index] = t.Position;
					}
				}
				
				index++;
			}
			
			// Make new plane
			if(slopefloor)
				data.Floor.plane = new Plane(verts[0], verts[1], verts[2], true);
			else
				data.Ceiling.plane = new Plane(verts[0], verts[2], verts[1], false);
		}
	}
}
