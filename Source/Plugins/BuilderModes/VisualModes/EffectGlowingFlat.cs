using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.Rendering;

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectGlowingFlat
	{
		private readonly SectorData data;

		// Level planes
		private SectorLevel ceillevel;
		private SectorLevel floorlevel;
		
		// Constructor
		public EffectGlowingFlat(SectorData sourcedata)
		{
			data = sourcedata;
		}

		public void Update()
		{
			// Create ceiling glow effect?
			data.CeilingGlow = GetGlowData(false);
			if(data.CeilingGlow != null)
			{
				// Create ceiling level?
				if(ceillevel == null)
				{
					ceillevel = new SectorLevel(data.Ceiling) { type = SectorLevelType.Glow, disablelighting = true };
					data.AddSectorLevel(ceillevel);
				}

				// Update ceiling level
				ceillevel.brightnessbelow = -1; // We need this plane for clipping only,
				ceillevel.color = 0;            // so we need to reset all shading and coloring
				ceillevel.plane = data.Ceiling.plane;
				ceillevel.plane.Offset -= data.CeilingGlow.Height;
				data.CeilingGlowPlane = ceillevel.plane;
			}

			// Create floor glow effect?
			data.FloorGlow = GetGlowData(true);
			if(data.FloorGlow != null)
			{
				// Create floor level?
				if(floorlevel == null)
				{
					floorlevel = new SectorLevel(data.Floor) { type = SectorLevelType.Glow, disablelighting = true };
					data.AddSectorLevel(floorlevel);
				}

				// Update floor level
				floorlevel.plane = data.Floor.plane.GetInverted();
				floorlevel.plane.Offset += data.FloorGlow.Height;

				if(floorlevel.plane.Offset < data.Ceiling.plane.Offset)
				{
					floorlevel.brightnessbelow = -1; // We need this plane for clipping only,
					floorlevel.color = 0;            // so we need to reset all shading and coloring
					floorlevel.colorbelow = new PixelColor(0, 0, 0, 0);
				}
				else
				{
					// If glow plane is above real ceiling, apply ceiling colouring
					floorlevel.brightnessbelow = data.Ceiling.brightnessbelow;
					floorlevel.color = data.Ceiling.color;
					floorlevel.colorbelow = data.Ceiling.colorbelow;
				}

				data.FloorGlowPlane = floorlevel.plane;
			}
		}

		private GlowingFlatData GetGlowData(bool floor)
		{
			// Check UDMF glow properties
			if(General.Map.UDMF)
			{
				int glowcolor = data.Sector.Fields.GetValue((floor ? "floorglowcolor" : "ceilingglowcolor"), 0);

				// Glow is explicidly disabled?
				if(glowcolor == -1) return null;

				// Avoid black glows
				if(glowcolor > 0)
				{
					float glowheight = data.Sector.Fields.GetValue((floor ? "floorglowheight" : "ceilingglowheight"), 0f);
					if(glowheight > 0f)
					{
						// Create glow data
						PixelColor c = PixelColor.FromInt(glowcolor);
						return new GlowingFlatData
						{
							Color = c,
							Height = glowheight,
							Brightness = (c.r + c.g + c.b) / 3,
						};
					}
				}
			}

			// Use GLDEFS glow if available
			long texture = (floor ? data.Sector.LongFloorTexture : data.Sector.LongCeilTexture);
			return (General.Map.Data.GlowingFlats.ContainsKey(texture) ? General.Map.Data.GlowingFlats[texture] : null);
		}
	}
}
