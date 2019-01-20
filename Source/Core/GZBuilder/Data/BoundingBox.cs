using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Rendering;

namespace CodeImp.DoomBuilder.GZBuilder.Data
{
	public struct BoundingBoxSizes
	{
		public int MinX;
		public int MaxX;
		public int MinY;
		public int MaxY;
		public int MinZ;
		public int MaxZ;

		//we need some reference here
		public BoundingBoxSizes(WorldVertex v) 
		{
			MinX = MaxX = (int)v.x;
			MinY = MaxY = (int)v.y;
			MinZ = MaxZ = (int)v.z;
		}
	}

	public static class BoundingBoxTools
	{
		public static Vector3D[] CalculateBoundingPlane(BoundingBoxSizes bbs) 
		{
			//mxd. looks like I need only these 2 points, so...
			//center
			Vector3D v0 = new Vector3D(bbs.MinX + (bbs.MaxX - bbs.MinX) / 2, bbs.MinY + (bbs.MaxY - bbs.MinY) / 2, bbs.MinZ + (bbs.MaxZ - bbs.MinZ) / 2);
			Vector3D v1 = new Vector3D(bbs.MinX, bbs.MinY, bbs.MinZ);
			return new[] { v0, v1 };
		}

		public static void UpdateBoundingBoxSizes(ref BoundingBoxSizes bbs, WorldVertex v) 
		{
			if(v.x < bbs.MinX) bbs.MinX = (int)v.x;
			else if(v.x > bbs.MaxX) bbs.MaxX = (int)v.x;

			if(v.z < bbs.MinZ) bbs.MinZ = (int)v.z;
			else if(v.z > bbs.MaxZ) bbs.MaxZ = (int)v.z;

			if(v.y < bbs.MinY) bbs.MinY = (int)v.y;
			else if(v.y > bbs.MaxY) bbs.MaxY = (int)v.y;
		}
	}
}
