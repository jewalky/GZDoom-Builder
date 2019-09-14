
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
using CodeImp.DoomBuilder.Geometry;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.VisualModes
{
	public sealed class VisualBlockMap
	{
		#region ================== Constants
		
		public const int BLOCK_SIZE_SHIFT = 7;
		public const int BLOCK_SIZE = 1 << BLOCK_SIZE_SHIFT;
		public const float BLOCK_RADIUS = BLOCK_SIZE * Angle2D.SQRT2;
		
		#endregion
		
		#region ================== Variables
		
		// Blocks
		private Dictionary<ulong, VisualBlockEntry> blockmap;
		
		// State
		private bool isdisposed;
		
		#endregion
		
		#region ================== Properties
		
		public bool IsDisposed { get { return isdisposed; } }
		
		#endregion
		
		#region ================== Constructor / Disposer
		
		// Constructor
		internal VisualBlockMap()
		{
			// Initialize
			blockmap = new Dictionary<ulong,VisualBlockEntry>();
		}
		
		// Disposer
		internal void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				blockmap = null;
				
				// Done
				isdisposed = true;
			}
		}
		
		#endregion
		
		#region ================== Methods
		
		// This returns the block coordinates
		public Point GetBlockCoordinates(Vector2D v)
		{
			return new Point((int)v.x >> BLOCK_SIZE_SHIFT,
							 (int)v.y >> BLOCK_SIZE_SHIFT);
		}

		// This returns the block center in world coordinates
		public Vector2D GetBlockCenter(Point p)
		{
			return new Vector2D((p.X << BLOCK_SIZE_SHIFT) + (BLOCK_SIZE >> 1),
								(p.Y << BLOCK_SIZE_SHIFT) + (BLOCK_SIZE >> 1));
		}

		// This returns the key for a block at the given coordinates
		// TODO: Could we just use the Point struct as key?
		private static ulong GetBlockKey(Point p)
		{
			return unchecked( ((ulong)(uint)p.X << 32) + (uint)p.Y );
		}
		
		// This returns the block with the given coordinates
		// Creates the block if it doesn't exist yet
		public VisualBlockEntry GetBlock(Point p)
		{
			ulong k = GetBlockKey(p);
			if(blockmap.ContainsKey(k))
			{
				return blockmap[k];
			}
			else
			{
				return (blockmap[k] = new VisualBlockEntry());
			}
		}
		
		// This clears the blockmap
		public void Clear()
		{
			blockmap = new Dictionary<ulong,VisualBlockEntry>();
		}
		
		// This returns a range of blocks in a square
		public List<VisualBlockEntry> GetSquareRange(RectangleF rect)
		{
			// Calculate block coordinates
			Point lt = GetBlockCoordinates(new Vector2D(rect.Left, rect.Top));
			Point rb = GetBlockCoordinates(new Vector2D(rect.Right, rect.Bottom));
			
			// Go through the range to make a list
			int entriescount = (rb.X - lt.X) * (rb.Y - lt.Y);
			List<VisualBlockEntry> entries = new List<VisualBlockEntry>(entriescount);
			for(int x = lt.X; x <= rb.X; x++)
			{
				for(int y = lt.Y; y <= rb.Y; y++)
				{
					entries.Add(GetBlock(new Point(x, y)));
				}
			}

			// Return list
			return entries;
		}

		// This returns a range of blocks in a frustum
		public List<VisualBlockEntry> GetFrustumRange(ProjectedFrustum2D frustum)
		{
			// Make square range from frustum circle
			// This will be the range in which we will test blocks
			Point lt = GetBlockCoordinates(frustum.Center - frustum.Radius);
			Point rb = GetBlockCoordinates(frustum.Center + frustum.Radius);

			// Constants we need
			float blockfrustumdistance2 = (frustum.Radius * frustum.Radius) + (BLOCK_RADIUS * BLOCK_RADIUS);
			
			// Go through the range to make a list
			int entriescount = (rb.X - lt.X) * (rb.Y - lt.Y);
			List<VisualBlockEntry> entries = new List<VisualBlockEntry>(entriescount);
			for(int x = lt.X; x <= rb.X; x++)
			{
				for(int y = lt.Y; y <= rb.Y; y++)
				{
					// First check if the block circle is intersecting the frustum circle
					Point block = new Point(x, y);
					Vector2D blockcenter = GetBlockCenter(block);
					if(Vector2D.DistanceSq(frustum.Center, blockcenter) < blockfrustumdistance2)
					{
						// Add the block if the block circle is inside the frustum
						if(frustum.IntersectCircle(blockcenter, BLOCK_RADIUS)) entries.Add(GetBlock(block));
					}
				}
			}
			
			// Return list
			return entries;
		}

		// This returns all blocks along the given line
		public List<VisualBlockEntry> GetLineBlocks(Vector2D v1, Vector2D v2)
		{
			// Estimate number of blocks we will go through and create list
			int entriescount = (int)(Vector2D.ManhattanDistance(v1, v2) * 2.0f) / BLOCK_SIZE;
			List<VisualBlockEntry> entries = new List<VisualBlockEntry>(entriescount);

			// Find start and end block
			Point pos = GetBlockCoordinates(v1);
			Point end = GetBlockCoordinates(v2);

			// Add this block
			entries.Add(GetBlock(pos));

			// Moving outside the block?
			if(pos != end)
			{
				// Calculate current block edges
				float cl = pos.X * BLOCK_SIZE;
				float cr = (pos.X + 1) * BLOCK_SIZE;
				float ct = pos.Y * BLOCK_SIZE;
				float cb = (pos.Y + 1) * BLOCK_SIZE;

				// Line directions
				int dirx = Math.Sign(v2.x - v1.x);
				int diry = Math.Sign(v2.y - v1.y);

				// Calculate offset and delta movement over x
				float posx, deltax;
				if(dirx >= 0)
				{
					posx = (cr - v1.x) / (v2.x - v1.x);
					deltax = BLOCK_SIZE / (v2.x - v1.x);
				}
				else
				{
					// Calculate offset and delta movement over x
					posx = (v1.x - cl) / (v1.x - v2.x);
					deltax = BLOCK_SIZE / (v1.x - v2.x);
				}

				// Calculate offset and delta movement over y
				float posy, deltay;
				if(diry >= 0)
				{
					posy = (cb - v1.y) / (v2.y - v1.y);
					deltay = BLOCK_SIZE / (v2.y - v1.y);
				}
				else
				{
					posy = (v1.y - ct) / (v1.y - v2.y);
					deltay = BLOCK_SIZE / (v1.y - v2.y);
				}

				// Continue while not reached the end
				while(pos != end)
				{
					// Check in which direction to move
					if(posx < posy)
					{
						// Move horizontally
						posx += deltax;
						if(pos.X != end.X) pos.X += dirx;
					}
					else
					{
						// Move vertically
						posy += deltay;
						if(pos.Y != end.Y) pos.Y += diry;
					}

					// Add lines to this block
					entries.Add(GetBlock(pos));
				}
			}

			// Return list
			return entries;
		}

		// This puts a thing in the blockmap
		public void AddThingsSet(ICollection<Thing> things)
		{
			foreach(Thing t in things) AddThing(t);
		}
		
		// This puts a thing in the blockmap
		public void AddThing(Thing t)
		{
			//mxd
			Point p1 = GetBlockCoordinates(new Vector2D(t.Position.x - t.RenderSize, t.Position.y - t.RenderSize));
			Point p2 = GetBlockCoordinates(new Vector2D(t.Position.x + t.RenderSize, t.Position.y + t.RenderSize));
			for(int x = p1.X; x <= p2.X; x++)
			{
				for(int y = p1.Y; y <= p2.Y; y++)
				{
					VisualBlockEntry block = GetBlock(new Point(x, y));
					block.Things.Add(t);
				}
			}
		}

		// This puts a secotr in the blockmap
		public void AddSectorsSet(ICollection<Sector> sectors)
		{
			foreach(Sector s in sectors) AddSector(s);
		}

		// This puts a sector in the blockmap
		public void AddSector(Sector s)
		{
			Point p1 = GetBlockCoordinates(new Vector2D(s.BBox.Left, s.BBox.Top));
			Point p2 = GetBlockCoordinates(new Vector2D(s.BBox.Right, s.BBox.Bottom));
			for(int x = p1.X; x <= p2.X; x++)
			{
				for(int y = p1.Y; y <= p2.Y; y++)
				{
					VisualBlockEntry block = GetBlock(new Point(x, y));
					block.Sectors.Add(s);
				}
			}
		}
		
		// This puts a whole set of linedefs in the blocks they cross
		public void AddLinedefsSet(ICollection<Linedef> lines)
		{
			foreach(Linedef l in lines) AddLinedef(l);
		}
		
		// This puts a single linedef in all blocks it crosses
		public void AddLinedef(Linedef line)
		{
			// Get coordinates
			Vector2D v1 = line.Start.Position;
			Vector2D v2 = line.End.Position;
			
			// Find start and end block
			Point pos = GetBlockCoordinates(v1);
			Point end = GetBlockCoordinates(v2);
			
			// Horizontal straight line?
			if(pos.Y == end.Y)
			{
				// Simple loop
				int dirx = Math.Sign(v2.x - v1.x);
				for(int x = pos.X; x != end.X; x += dirx)
				{
					GetBlock(new Point(x, pos.Y)).Lines.Add(line);
				}
				GetBlock(end).Lines.Add(line);
			}
			// Vertical straight line?
			else if(pos.X == end.X)
			{
				// Simple loop
				int diry = Math.Sign(v2.y - v1.y);
				for(int y = pos.Y; y != end.Y; y += diry)
				{
					GetBlock(new Point(pos.X, y)).Lines.Add(line);
				}
				GetBlock(end).Lines.Add(line);
			}
			else
			{
				// Add lines to this block
				GetBlock(pos).Lines.Add(line);
				
				// Moving outside the block?
				if(pos != end)
				{
					// Calculate current block edges
					float cl = pos.X * BLOCK_SIZE;
					float cr = (pos.X + 1) * BLOCK_SIZE;
					float ct = pos.Y * BLOCK_SIZE;
					float cb = (pos.Y + 1) * BLOCK_SIZE;
					
					// Line directions
					int dirx = Math.Sign(v2.x - v1.x);
					int diry = Math.Sign(v2.y - v1.y);
					
					// Calculate offset and delta movement over x
					float posx, deltax;
					if(dirx == 0)
					{
						posx = float.MaxValue;
						deltax = float.MaxValue;
					}
					else if(dirx > 0)
					{
						posx = (cr - v1.x) / (v2.x - v1.x);
						deltax = BLOCK_SIZE / (v2.x - v1.x);
					}
					else
					{
						// Calculate offset and delta movement over x
						posx = (v1.x - cl) / (v1.x - v2.x);
						deltax = BLOCK_SIZE / (v1.x - v2.x);
					}
					
					// Calculate offset and delta movement over y
					float posy, deltay;
					if(diry == 0)
					{
						posy = float.MaxValue;
						deltay = float.MaxValue;
					}
					else if(diry > 0)
					{
						posy = (cb - v1.y) / (v2.y - v1.y);
						deltay = BLOCK_SIZE / (v2.y - v1.y);
					}
					else
					{
						posy = (v1.y - ct) / (v1.y - v2.y);
						deltay = BLOCK_SIZE / (v1.y - v2.y);
					}
					
					// Continue while not reached the end
					while(pos != end)
					{
						// Check in which direction to move
						if(posx < posy)
						{
							// Move horizontally
							posx += deltax;
							if(pos.X != end.X) pos.X += dirx;
						}
						else
						{
							// Move vertically
							posy += deltay;
							if(pos.Y != end.Y) pos.Y += diry;
						}
						
						// Add lines to this block
						GetBlock(pos).Lines.Add(line);
					}
				}
			}
		}
		
		#endregion
	}
}
