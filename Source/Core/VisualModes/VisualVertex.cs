using System;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using SlimDX;

namespace CodeImp.DoomBuilder.VisualModes
{
	public class VisualVertexPair
	{
		private readonly VisualVertex floorvert;
		private readonly VisualVertex ceilvert;

		public VisualVertex[] Vertices { get { return new[] { floorvert, ceilvert }; } }
		public VisualVertex FloorVertex { get { return floorvert; } }
		public VisualVertex CeilingVertex { get { return ceilvert; } }
		public bool Changed { set { floorvert.Changed = value; ceilvert.Changed = value; } }

		public VisualVertexPair(VisualVertex floorvert, VisualVertex ceilvert) 
		{
			if(floorvert.CeilingVertex == ceilvert.CeilingVertex)
				throw new Exception("VisualVertexPair: both verts have the same alignment! We cannot tolerate this!");

			this.floorvert = floorvert;
			this.ceilvert = ceilvert;
		}

		public void Update() 
		{
			if(floorvert.Changed) floorvert.Update();
			if(ceilvert.Changed) ceilvert.Update();
		}

		public void Deselect() 
		{
			floorvert.Selected = false;
			ceilvert.Selected = false;
		}
	}

	public abstract class VisualVertex : IVisualPickable
	{
		//Constants
		public const float DEFAULT_SIZE = 6.0f;
		
		//Variables
		protected readonly Vertex vertex;
		private Matrix position;
		protected bool selected;
		protected bool changed;
		protected readonly bool ceilingVertex;
		protected bool haveOffset;

		//Properties
		internal Matrix Position { get { return position; } }
		public Vertex Vertex { get { return vertex; } }
		public bool Selected { get { return selected; } set { selected = value; } }
		public bool Changed { get { return changed; } set { changed |= value; } }
		public bool CeilingVertex { get { return ceilingVertex; } }
		public bool HaveHeightOffset { get { return haveOffset; } }

		protected VisualVertex(Vertex v, bool ceilingVertex) 
		{
			vertex = v;
			position = Matrix.Identity;
			this.ceilingVertex = ceilingVertex;
		}

		public void SetPosition(Vector3D pos) 
		{
			position = Matrix.Translation(pos.x, pos.y, pos.z);
		}

		public virtual void Update() { }

		/// <summary>
		/// This is called when the thing must be tested for line intersection. This should reject
		/// as fast as possible to rule out all geometry that certainly does not touch the line.
		/// </summary>
		public virtual bool PickFastReject(Vector3D from, Vector3D to, Vector3D dir) 
		{
			return false;
		}

		/// <summary>
		/// This is called when the thing must be tested for line intersection. This should perform
		/// accurate hit detection and set u_ray to the position on the ray where this hits the geometry.
		/// </summary>
		public virtual bool PickAccurate(Vector3D from, Vector3D to, Vector3D dir, ref float u_ray) 
		{
			return false;
		}
	}
}
