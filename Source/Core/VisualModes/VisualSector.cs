
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using SlimDX.Direct3D9;
using SlimDX;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.VisualModes
{
	public class VisualSector : ID3DResource, IDisposable
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Geometry
		private readonly List<VisualGeometry> fixedgeometry;
		private readonly List<VisualGeometry> allgeometry;
		private readonly Dictionary<Sidedef, List<VisualGeometry>> sidedefgeometry;
		private VertexBuffer geobuffer;
		private bool updategeo;
		
		// Original sector
		private readonly Sector sector;
		
		// Disposing
		private bool isdisposed;

		#endregion

		#region ================== Properties

		internal List<VisualGeometry> FixedGeometry { get { return fixedgeometry; } }
		internal List<VisualGeometry> AllGeometry { get { return allgeometry; } }
		internal VertexBuffer GeometryBuffer { get { return geobuffer; } }
		internal bool NeedsUpdateGeo { get { return updategeo; } set { updategeo |= value; } }
		
		public bool IsDisposed { get { return isdisposed; } }
		public Sector Sector { get { return sector; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public VisualSector(Sector s)
		{
			// Initialize
			this.sector = s;
			allgeometry = new List<VisualGeometry>();
			fixedgeometry = new List<VisualGeometry>();
			sidedefgeometry = new Dictionary<Sidedef, List<VisualGeometry>>();
			this.sector.UpdateFogColor(); //mxd

			// Register as resource
			General.Map.Graphics.RegisterResource(this);
		}

		// Disposer
		public virtual void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				if(geobuffer != null) geobuffer.Dispose();
				geobuffer = null;

				// Unregister resource
				General.Map.Graphics.UnregisterResource(this);
				
				// Done
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Methods

		// This is called before a device is reset
		// (when resized or display adapter was changed)
		public virtual void UnloadResource()
		{
			// Trash geometry buffer
			if(geobuffer != null) geobuffer.Dispose();
			geobuffer = null;
			updategeo = true;
		}

		// This is called resets when the device is reset
		// (when resized or display adapter was changed)
		public virtual void ReloadResource()
		{
			// Make new geometry
			//Update();
		}

		//mxd. Added to allow to properly update visual geometry from plugins
		public virtual void UpdateSectorData() { }
		public virtual void UpdateSectorGeometry(bool includeneighbours) { }
		
		// This updates the visual sector
		public void Update()
		{
			int numverts = 0;
			int v = 0;
			
			// Trash geometry buffer
			if(geobuffer != null) geobuffer.Dispose();
			geobuffer = null;
			
			// Count the number of vertices there are
			foreach(VisualGeometry g in allgeometry) if(g.Vertices != null) numverts += g.Vertices.Length;
			
			// Any vertics?
			if(numverts > 0)
			{
				// Make a new buffer
				geobuffer = new VertexBuffer(General.Map.Graphics.Device, WorldVertex.Stride * numverts,
											 Usage.WriteOnly | Usage.Dynamic, VertexFormat.None, Pool.Default);

				// Fill the buffer
				DataStream bufferstream = geobuffer.Lock(0, WorldVertex.Stride * numverts, LockFlags.Discard);
				foreach(VisualGeometry g in allgeometry)
				{
					if((g.Vertices != null) && (g.Vertices.Length > 0))
					{
						bufferstream.WriteRange(g.Vertices);
						g.VertexOffset = v;
						v += g.Vertices.Length;
					}
				}
				geobuffer.Unlock();
				bufferstream.Dispose();
			}

			this.sector.UpdateFogColor(); //mxd

            // Done
            updategeo = false;
		}

		/// <summary>
		/// This adds geometry for this sector. If the geometry inherits from VisualSidedef then it
		/// will be added to the SidedefGeometry, otherwise it will be added as FixedGeometry.
		/// </summary>
		public void AddGeometry(VisualGeometry geo)
		{
			updategeo = true;
			allgeometry.Add(geo);
			if(geo.Sidedef != null)
			{
				if(!sidedefgeometry.ContainsKey(geo.Sidedef))
					sidedefgeometry[geo.Sidedef] = new List<VisualGeometry>(3);
				sidedefgeometry[geo.Sidedef].Add(geo);
			}
			else
			{
				fixedgeometry.Add(geo);
			}
		}

		/// <summary>
		/// This removes all geometry.
		/// </summary>
		public void ClearGeometry()
		{
			allgeometry.Clear();
			fixedgeometry.Clear();
			sidedefgeometry.Clear();
			updategeo = true;
		}

		// This gets the geometry list for the specified sidedef
		public List<VisualGeometry> GetSidedefGeometry(Sidedef sd)
		{
			if(sidedefgeometry.ContainsKey(sd)) return sidedefgeometry[sd];
			return new List<VisualGeometry>();
		}
		
		#endregion
	}
}
