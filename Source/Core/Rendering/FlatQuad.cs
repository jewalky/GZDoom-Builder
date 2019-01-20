
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
using System.Drawing;
using SlimDX.Direct3D9;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	// FlatQuad
	internal class FlatQuad
	{
		#region ================== Variables

		// Vertices
		private FlatVertex[] vertices;
		private PrimitiveType type;
		private int numvertices;
		
		#endregion

		#region ================== Properties

		public FlatVertex[] Vertices { get { return vertices; } }
		public PrimitiveType Type { get { return type; } }
		
		#endregion

		#region ================== Constructors

		// Constructor
		public FlatQuad(PrimitiveType type, float left, float top, float right, float bottom)
		{
			// Initialize
			Initialize(type);
			
			// Set coordinates
			switch(type)
			{
				case PrimitiveType.TriangleList:
					SetTriangleListCoordinates(left, top, right, bottom, 0f, 0f, 1f, 1f);
					break;
				case PrimitiveType.TriangleStrip:
					SetTriangleStripCoordinates(left, top, right, bottom, 0f, 0f, 1f, 1f);
					break;
			}
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Constructor
		public FlatQuad(PrimitiveType type, float left, float top, float right, float bottom, float twidth, float theight)
		{
			// Initialize
			Initialize(type);

			// Determine texture size dividers
			float twd = 1f / twidth;
			float thd = 1f / theight;
			
			// Set coordinates
			switch(type)
			{
				case PrimitiveType.TriangleList:
					SetTriangleListCoordinates(left, top, right, bottom, twd, thd, 1f - twd, 1f - thd);
					break;
				case PrimitiveType.TriangleStrip:
					SetTriangleStripCoordinates(left, top, right, bottom, twd, thd, 1f - twd, 1f - thd);
					break;
			}
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Constructor
		public FlatQuad(PrimitiveType type, RectangleF pos, float tl, float tt, float tr, float tb)
		{
			// Initialize
			Initialize(type);

			// Set coordinates
			switch(type)
			{
				case PrimitiveType.TriangleList:
					SetTriangleListCoordinates(pos.Left, pos.Top, pos.Right, pos.Bottom, tl, tt, tr, tb);
					break;
				case PrimitiveType.TriangleStrip:
					SetTriangleStripCoordinates(pos.Left, pos.Top, pos.Right, pos.Bottom, tl, tt, tr, tb);
					break;
			}

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Constructor
		public FlatQuad(PrimitiveType type, float left, float top, float right, float bottom, float tl, float tt, float tr, float tb)
		{
			// Initialize
			Initialize(type);
			
			// Set coordinates
			switch(type)
			{
				case PrimitiveType.TriangleList:
					SetTriangleListCoordinates(left, top, right, bottom, tl, tt, tr, tb);
					break;
				case PrimitiveType.TriangleStrip:
					SetTriangleStripCoordinates(left, top, right, bottom, tl, tt, tr, tb);
					break;
			}

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This sets the color on all vertices
		public void SetColors(int color)
		{
			// Go for all vertices to set the color
			for(int i = 0; i < numvertices; i++)
				vertices[i].c = color;
		}

		// This sets the color on all vertices
		public void SetColors(int clt, int crt, int clb, int crb)
		{
			// Determine polygon type
			switch(type)
			{
				case PrimitiveType.TriangleList:
					vertices[0].c = clt;
					vertices[1].c = crt;
					vertices[2].c = clb;
					vertices[3].c = clb;
					vertices[4].c = crt;
					vertices[5].c = crb;
					break;
				case PrimitiveType.TriangleStrip:
					vertices[0].c = clt;
					vertices[1].c = crt;
					vertices[2].c = clb;
					vertices[3].c = crb;
					break;
			}
		}

		// This applies coordinates for TriangleList type
		private void SetTriangleListCoordinates(float vl, float vt, float vr, float vb,
												float tl, float tt, float tr, float tb)
		{
			// Setup coordinates
			vertices[0].x = vl;
			vertices[0].y = vt;
			vertices[1].x = vr;
			vertices[1].y = vt;
			vertices[2].x = vl;
			vertices[2].y = vb;
			vertices[3].x = vl;
			vertices[3].y = vb;
			vertices[4].x = vr;
			vertices[4].y = vt;
			vertices[5].x = vr;
			vertices[5].y = vb;
			
			// Set texture coordinates
			vertices[0].u = tl;
			vertices[0].v = tt;
			vertices[1].u = tr;
			vertices[1].v = tt;
			vertices[2].u = tl;
			vertices[2].v = tb;
			vertices[3].u = tl;
			vertices[3].v = tb;
			vertices[4].u = tr;
			vertices[4].v = tt;
			vertices[5].u = tr;
			vertices[5].v = tb;
		}

		// This applies coordinates for TriangleStrip type
		private void SetTriangleStripCoordinates(float vl, float vt, float vr, float vb,
												 float tl, float tt, float tr, float tb)
		{
			// Setup coordinates
			vertices[0].x = vl;
			vertices[0].y = vt;
			vertices[1].x = vr;
			vertices[1].y = vt;
			vertices[2].x = vl;
			vertices[2].y = vb;
			vertices[3].x = vr;
			vertices[3].y = vb;

			// Set texture coordinates
			vertices[0].u = tl;
			vertices[0].v = tt;
			vertices[1].u = tr;
			vertices[1].v = tt;
			vertices[2].u = tl;
			vertices[2].v = tb;
			vertices[3].u = tr;
			vertices[3].v = tb;
		}
		
		// This initializes vertices to default values
		private void Initialize(PrimitiveType type)
		{
			// Determine primitive type
			this.type = type;

			// Determine number of vertices
			switch(type)
			{
				case PrimitiveType.TriangleList: numvertices = 6; break;
				case PrimitiveType.TriangleStrip: numvertices = 4; break;
				default: throw new NotSupportedException("Unsupported PrimitiveType");
			}
			
			// Make the array
			vertices = new FlatVertex[numvertices];

			// Go for all vertices
			for(int i = 0; i < numvertices; i++)
			{
				// Initialize to defaults
				vertices[i].c = -1;
			}
		}

		#endregion
		
		#region ================== Rendering

		// This renders the quad
		public void Render(D3DDevice device)
		{
			// Render the quad
			device.Device.DrawUserPrimitives(type, 0, 2, vertices);
		}
		
		#endregion
	}
}
