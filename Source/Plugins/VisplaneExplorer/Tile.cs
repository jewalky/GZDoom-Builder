#region === Copyright (c) 2010 Pascal van der Heiden ===

using System;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.Plugins.VisplaneExplorer
{
	// This is a 64x64 tile in map space which holds the point data results.
	internal class Tile
	{
		// Constants
		public const int TILE_SIZE = 64;
		public static readonly int[] STATS_COMPRESSOR = new[] { 1, 2, 1, 160 };
		public static readonly int[] STATS_LIMITS = new[] { 128, 256, 32, 320 * 64 };
		public const uint POINT_MAXRANGE = 254;
		public const uint POINT_OVERFLOW = 0xFEFEFEFE;
		public const uint POINT_VOID = 0xFFFFFFFF;
		public const byte POINT_OVERFLOW_B = 0xFE;
		public const byte POINT_VOID_B = 0xFF;
		
		// Members
		private Point position;
		private uint[][] points;
		private int nextindex;
		
		// Properties
		public Point Position { get { return position; } }
		public bool IsComplete { get { return nextindex == (TILE_SIZE * TILE_SIZE); } }

		// Constructor
		public Tile(Point lefttoppos)
		{
			// Make the jagged array
			// I use a jagged array because, allegedly, it performs better than a multidimensional array.
			points = new uint[TILE_SIZE][];
			for(int y = 0; y < TILE_SIZE; y++)
				points[y] = new uint[TILE_SIZE];
			
			position = lefttoppos;
		}

		// This receives a processed point
		public void StorePointData(PointData pd)
		{
			uint t;
			switch(pd.result)
			{
				case PointResult.OK:
					uint vp = (uint)Math.Min((pd.visplanes + (STATS_COMPRESSOR[(int)ViewStats.Visplanes] - 1)) / STATS_COMPRESSOR[(int)ViewStats.Visplanes], POINT_MAXRANGE);
					uint ds = (uint)Math.Min((pd.drawsegs + (STATS_COMPRESSOR[(int)ViewStats.Drawsegs] - 1)) / STATS_COMPRESSOR[(int)ViewStats.Drawsegs], POINT_MAXRANGE);
					uint ss = (uint)Math.Min((pd.solidsegs + (STATS_COMPRESSOR[(int)ViewStats.Solidsegs] - 1)) / STATS_COMPRESSOR[(int)ViewStats.Solidsegs], POINT_MAXRANGE);
					uint op = (uint)Math.Min((pd.openings + (STATS_COMPRESSOR[(int)ViewStats.Openings] - 1)) / STATS_COMPRESSOR[(int)ViewStats.Openings], POINT_MAXRANGE);
					t = MakePointValue(vp, ds, ss, op);
					break;

				case PointResult.BadZ:
					t = MakePointValue(1, 0, 0, 0);
					break;
					
				case PointResult.Void:
					t = POINT_VOID;
					break;

				case PointResult.Overflow:
				default:
					t = POINT_OVERFLOW;
					break;
			}

			FillPoints(ref pd.point, t);
		}

		// This fills points with the given tile data over the specified granularity
		private void FillPoints(ref TilePoint p, uint t)
		{
			int xs = p.x - position.X;
			int ys = p.y - position.Y;
			int xe = xs + p.granularity;
			int ye = ys + p.granularity;
			for(int x = xs; x < xe; x++)
				for(int y = ys; y < ye; y++)
					points[y][x] = t;
		}

		// This composes point values
		private static uint MakePointValue(uint vp, uint ds, uint ss, uint op)
		{
			unchecked
			{
				return vp + (ds << 8) + (ss << 16) + (op << 24);
			}
		}

		// This returns a point value
		public byte GetPointByte(int x, int y, int stat)
		{
			unchecked
			{
				uint v = points[y][x];
				return (byte)((v >> (stat * 8)) & 0xFF);
			}
		}
		
		// This returns a point value
		public int GetPointValue(int x, int y, int stat)
		{
			byte b = GetPointByte(x, y, stat);
			return b * STATS_COMPRESSOR[stat];
		}

		// This returns the next point to process
		public TilePoint GetNextPoint()
		{
			TilePoint p = PointByIndex(nextindex++);
			p.x += position.X;
			p.y += position.Y;
			return p;
		}

		// Returns a position by index
		private static TilePoint PointByIndex(int index)
		{
			#if DEBUG
			if(index > (TILE_SIZE * TILE_SIZE))
				throw new IndexOutOfRangeException();
			#endif

			TilePoint p;
			
			// Would be nicer if this could be done without branching or looping...
			if(index == 0) p.granularity = 64;
			else if(index < 4) p.granularity = 32;
			else if(index < 16) p.granularity = 16;
			else if(index < 64) p.granularity = 8;
			else if(index < 256) p.granularity = 4;
			else if(index < 1024) p.granularity = 2;
			else p.granularity = 1;
			
			// this is a "butterfly" style sequence, which begins like:
			//    ( 0  0)  (32 32)  ( 0 32)  (32  0)
			//    (16 16)  (48 48)  (16 48)  (48 16)
			//    ( 0 16)  (32 48)  ( 0 48)  (32 16)
			//    (16  0)  (48 32)  (16 32)  (48  0)
			//    ( 8  8)  (40 40)  ( 8 40)  (40  8)
			//    etc....

			p.x = (index & 1) << 5;
			p.y = (((index >> 1) ^ index) & 1) << 5;

			index >>= 2;
			p.x += (index & 1) << 4;
			p.y += (((index >> 1) ^ index) & 1) << 4;

			index >>= 2;
			p.x |= (index & 1) << 3;
			p.y |= (((index >> 1) ^ index) & 1) << 3;

			index >>= 2;
			p.x |= (index & 1) << 2;
			p.y |= (((index >> 1) ^ index) & 1) << 2;

			index >>= 2;
			p.x |= (index & 1) << 1;
			p.y |= (((index >> 1) ^ index) & 1) << 1;

			index >>= 2;
			p.x |= index & 1;
			p.y |= ((index >> 1) ^ index) & 1;

			return p;
		}
	}
}
