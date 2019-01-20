
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

using System.Collections.Generic;
using SlimDX.Direct3D9;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	internal struct SurfaceBufferSet
	{
		// The number of vertices per sector that this set is for
		public int numvertices;
		
		// These are the vertex buffers. They are hashed by an integer key which
		// is the number of vertices per sector geometry the buffer if meant for.
		public List<VertexBuffer> buffers;
		public List<int> buffersizes;

		// These are the entries that contain information for the contents of the buffers.
		public List<SurfaceEntry> entries;

		// These are the empty entries in buferrs that are available.
		public List<SurfaceEntry> holes;
	}
}
