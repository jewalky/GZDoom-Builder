#region ================== Copyright (c) 2021 Derek MacDonald

/*
 * Copyright (c) 2021 Derek MacDonald
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

using CodeImp.DoomBuilder.IO;
using System;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public class StaticLimits
	{
		#region ================== Constants

		private const uint DEFAULT_MAX_VISPLANES = 128;
		private const uint DEFAULT_MAX_VISPLANES_LIMIT = DEFAULT_MAX_VISPLANES * 2;
		private const uint MAX_DRAWSEGS = 256;
		private const uint MAX_SOLIDSEGS = 32;
		private const uint MAX_OPENINGS = 320 * 64;

		#endregion

		#region ================== Variables

		private uint visplanes;
		private static uint maxvisplaneslimit;

		#endregion

		#region ================== Properties

		public uint Visplanes { get { return visplanes; } }
		public uint Drawsegs { get { return MAX_DRAWSEGS; } }
		public uint Solidsegs { get { return MAX_SOLIDSEGS; } }
		public uint Openings { get { return MAX_OPENINGS; } }

		#endregion

		// Constructor
		internal StaticLimits(Configuration cfg)
		{
			visplanes = (uint)cfg.ReadSetting("staticlimits.visplanes", DEFAULT_MAX_VISPLANES);
			maxvisplaneslimit = visplanes * 2;
		}

		#region ================== Methods

		// This interpolates the supported visplane count to default range 1-255
		// where 128 is the configured static limit.
		public byte InterpolateVisplanes(byte value)
		{
			if (visplanes == DEFAULT_MAX_VISPLANES) return value;

			double v = DEFAULT_MAX_VISPLANES_LIMIT * value / maxvisplaneslimit;
			return (byte)Math.Ceiling(v);
		}

		#endregion
	}
}
