#region ================== Namespaces

using System;
using CodeImp.DoomBuilder.VisualModes;
using SlimDX.Direct3D9;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	internal sealed class VisualVertexHandle : IDisposable, ID3DResource
	{
		#region ================== Variables

		private VertexBuffer upper;
		private VertexBuffer lower;
		private bool isdisposed;

		#endregion

		#region ================== Properties

		public VertexBuffer Upper { get { return upper; } }
		public VertexBuffer Lower { get { return lower; } }

		#endregion

		#region ================== Constructor / Disposer

		public VisualVertexHandle() 
		{
			// Create geometry
			ReloadResource();

			// Register as resource
			General.Map.Graphics.RegisterResource(this);
		}

		public void Dispose() 
		{
			// Not already disposed?
			if(!isdisposed) 
			{
				if(upper != null) upper.Dispose();
				upper = null;
				if(lower != null) lower.Dispose();
				lower = null;

				// Unregister resource
				General.Map.Graphics.UnregisterResource(this);

				// Done
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Methods

		// This is called resets when the device is reset
		// (when resized or display adapter was changed)
		public void ReloadResource() 
		{
			float radius = VisualVertex.DEFAULT_SIZE * General.Settings.GZVertexScale3D;

			WorldVertex c = new WorldVertex();
			WorldVertex v0 = new WorldVertex(-radius, -radius, -radius);
			WorldVertex v1 = new WorldVertex(-radius, radius, -radius);
			WorldVertex v2 = new WorldVertex(radius, radius, -radius);
			WorldVertex v3 = new WorldVertex(radius, -radius, -radius);

			WorldVertex v4 = new WorldVertex(-radius, -radius, radius);
			WorldVertex v5 = new WorldVertex(-radius, radius, radius);
			WorldVertex v6 = new WorldVertex(radius, radius, radius);
			WorldVertex v7 = new WorldVertex(radius, -radius, radius);

			WorldVertex[] vu = new []{ c, v0,
									   c, v1,
									   c, v2,
									   c, v3,

									   v0, v1,
									   v1, v2,
									   v2, v3,
									   v3, v0 };

			upper = new VertexBuffer(General.Map.Graphics.Device, WorldVertex.Stride * vu.Length, Usage.WriteOnly | Usage.Dynamic, VertexFormat.None, Pool.Default);
			upper.Lock(0, WorldVertex.Stride * vu.Length, LockFlags.None).WriteRange(vu);
			upper.Unlock();

			WorldVertex[] vl = new[]{ c, v4,
									  c, v5,
									  c, v6,
									  c, v7,

									  v4, v5, 
									  v5, v6,
									  v6, v7,
									  v7, v4 };

			lower = new VertexBuffer(General.Map.Graphics.Device, WorldVertex.Stride * vl.Length, Usage.WriteOnly | Usage.Dynamic, VertexFormat.None, Pool.Default);
			lower.Lock(0, WorldVertex.Stride * vl.Length, LockFlags.None).WriteRange(vl);
			lower.Unlock();
		}

		// This is called before a device is reset
		// (when resized or display adapter was changed)
		public void UnloadResource() 
		{
			// Trash geometry buffers
			if(upper != null) upper.Dispose();
			upper = null;
			if(lower != null) lower.Dispose();
			lower = null;
		}

		#endregion
	}
}