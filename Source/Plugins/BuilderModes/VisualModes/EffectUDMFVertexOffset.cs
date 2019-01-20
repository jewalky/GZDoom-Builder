using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Geometry;

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectUDMFVertexOffset : SectorEffect
	{
		public EffectUDMFVertexOffset(SectorData data) : base(data) 
		{
			// New effect added: This sector needs an update!
			if(data.Mode.VisualSectorExists(data.Sector)) 
			{
				BaseVisualSector vs = (BaseVisualSector)data.Mode.GetVisualSector(data.Sector);
				vs.UpdateSectorGeometry(true);
			}
		}

		public override void Update() 
		{
			// Create vertices in clockwise order
			Vector3D[] floorVerts = new Vector3D[3];
			Vector3D[] ceilingVerts = new Vector3D[3];
			bool floorChanged = false;
			bool ceilingChanged = false;
			int index = 0;

			//check vertices
			foreach(Sidedef sd in data.Sector.Sidedefs)	
			{
				Vertex v = sd.IsFront ? sd.Line.End : sd.Line.Start;
				
				//create "normal" vertices
				floorVerts[index] = new Vector3D(v.Position);
				ceilingVerts[index] = new Vector3D(v.Position);

				//check ceiling
				if(!float.IsNaN(v.ZCeiling)) 
				{
					//vertex offset is absolute
					ceilingVerts[index].z = v.ZCeiling;
					ceilingChanged = true;
				} 
				else 
				{
					ceilingVerts[index].z = data.Ceiling.plane.GetZ(v.Position);
				}

				//and floor
				if(!float.IsNaN(v.ZFloor)) 
				{
					//vertex offset is absolute
					floorVerts[index].z = v.ZFloor;
					floorChanged = true;
				} 
				else 
				{
					floorVerts[index].z = data.Floor.plane.GetZ(v.Position);
				}

				VertexData vd = data.Mode.GetVertexData(v);

				foreach(Linedef line in v.Linedefs) 
				{
					if(line.Front != null && line.Front.Sector != null)
						vd.AddUpdateSector(line.Front.Sector, false);
					if(line.Back != null && line.Back.Sector != null)
						vd.AddUpdateSector(line.Back.Sector, false);
				}

				data.Mode.UpdateVertexHandle(v);
				index++;
			}

			//apply changes
			if(ceilingChanged)
				data.Ceiling.plane = new Plane(ceilingVerts[0], ceilingVerts[2], ceilingVerts[1], false);

			if(floorChanged)
				data.Floor.plane = new Plane(floorVerts[0], floorVerts[1], floorVerts[2], true);
		}
	}
}
