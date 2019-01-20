#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.VisualModes;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	#region ================== Structs

	// A struct, which contains information about visual sides connected to start and end of given visual side
	internal class SortedVisualSide
	{
		internal readonly BaseVisualGeometrySidedef Side;
		internal readonly Vector2D Start;
		internal readonly Vector2D End;
		internal Rectangle Bounds;
		internal Rectangle GlobalBounds;
		internal readonly Dictionary<SortedVisualSide, bool> NextSides;
		internal readonly Dictionary<SortedVisualSide, bool> PreviousSides;
		internal readonly int Index;
		internal int GroupIndex = -1;
		private static int index;

		//Initial texture coordinates
		private readonly float OffsetX;
		private readonly float OffsetY;
		private readonly float ControlSideOffsetX;
		private readonly float ControlSideOffsetY;
		private readonly float ScaleX;
		private readonly float ScaleY;

		internal SortedVisualSide(BaseVisualGeometrySidedef side)
		{
			Side = side;
			Bounds = BuilderModesTools.GetSidedefPartSize(side);
			Index = index++;

			if(side.Sidedef.Line.Front == side.Sidedef)
			{
				Start = side.Sidedef.Line.Start.Position;
				End = side.Sidedef.Line.End.Position;
			}
			else
			{
				Start = side.Sidedef.Line.End.Position;
				End = side.Sidedef.Line.Start.Position;
			}

			switch(side.GeometryType)
			{
				case VisualGeometryType.WALL_UPPER:
					OffsetX = UniFields.GetFloat(side.Sidedef.Fields, "offsetx_top");
					OffsetY = UniFields.GetFloat(side.Sidedef.Fields, "offsety_top");
					ScaleX = UniFields.GetFloat(side.Sidedef.Fields, "scalex_top", 1.0f);
					ScaleY = UniFields.GetFloat(side.Sidedef.Fields, "scaley_top", 1.0f);
					break;

				case VisualGeometryType.WALL_MIDDLE:
					OffsetX = UniFields.GetFloat(side.Sidedef.Fields, "offsetx_mid");
					OffsetY = UniFields.GetFloat(side.Sidedef.Fields, "offsety_mid");
					ScaleX = UniFields.GetFloat(side.Sidedef.Fields, "scalex_mid", 1.0f);
					ScaleY = UniFields.GetFloat(side.Sidedef.Fields, "scaley_mid", 1.0f);
					break;

				case VisualGeometryType.WALL_MIDDLE_3D:
					Sidedef cs = side.GetControlLinedef().Front;
					ControlSideOffsetX = cs.OffsetX + UniFields.GetFloat(cs.Fields, "offsetx_mid");
					OffsetX = UniFields.GetFloat(side.Sidedef.Fields, "offsetx_mid");
					ControlSideOffsetY = cs.OffsetY + UniFields.GetFloat(cs.Fields, "offsety_mid");
					OffsetY = UniFields.GetFloat(side.Sidedef.Fields, "offsety_mid");
					ScaleX = UniFields.GetFloat(cs.Fields, "scalex_mid", 1.0f);
					ScaleY = UniFields.GetFloat(cs.Fields, "scaley_mid", 1.0f);
					break;

				case VisualGeometryType.WALL_LOWER:
					OffsetX = UniFields.GetFloat(side.Sidedef.Fields, "offsetx_bottom");
					OffsetY = UniFields.GetFloat(side.Sidedef.Fields, "offsety_bottom");
					ScaleX = UniFields.GetFloat(side.Sidedef.Fields, "scalex_bottom", 1.0f);
					ScaleY = UniFields.GetFloat(side.Sidedef.Fields, "scaley_bottom", 1.0f);
					break;
			}

			NextSides = new Dictionary<SortedVisualSide, bool>();
			PreviousSides = new Dictionary<SortedVisualSide, bool>();
		}

		internal void OnTextureFit(FitTextureOptions options)
		{
			options.Bounds = Bounds;
			options.GlobalBounds = GlobalBounds;
			options.InitialOffsetX = OffsetX;
			options.InitialOffsetY = OffsetY;
			options.ControlSideOffsetX = ControlSideOffsetX;
			options.ControlSideOffsetY = ControlSideOffsetY;
			options.InitialScaleX = ScaleX;
			options.InitialScaleY = ScaleY;

			Side.OnTextureFit(options);
		}
	}

	#endregion

	internal static class BuilderModesTools
	{
		#region ================== Sidedef

		internal static Rectangle GetSidedefPartSize(BaseVisualGeometrySidedef side)
		{
			// We are interested in width, height and vertical position only
			float miny = float.MaxValue;
			float maxy = float.MinValue;

			foreach(WorldVertex v in side.Vertices)
			{
				if(v.z < miny) miny = v.z;
				else if(v.z > maxy) maxy = v.z;
			}

			return new Rectangle(0, (int)Math.Round(-maxy), Math.Max(1, (int)Math.Round(side.Sidedef.Line.Length)), (int)Math.Round(maxy - miny));
		}

		public static Rectangle GetSidedefPartSize(Sidedef side, VisualGeometryType type) 
		{
			Rectangle rect = new Rectangle(0, 0, Math.Max(1, (int)Math.Round(side.Line.Length)), 0);

			switch(type) 
			{
				case VisualGeometryType.WALL_LOWER:
					if(side.LowRequired())
					{
						rect.Y = -side.Other.Sector.FloorHeight;
						rect.Height = side.GetLowHeight();
					}
					break;

				case VisualGeometryType.WALL_UPPER:
					if(side.HighRequired()) 
					{
						rect.Y = -side.Sector.CeilHeight;
						rect.Height = side.GetHighHeight();
					} 
					break;

				case VisualGeometryType.WALL_MIDDLE:
					if(side.MiddleRequired())
					{
						rect.Y = -side.Sector.CeilHeight;
					}
					else if(side.Other.Sector != null) // Double-sided
					{
						rect.Y = -Math.Min(side.Sector.CeilHeight, side.Other.Sector.CeilHeight);
					}
					rect.Height = side.GetMiddleHeight();
					break;

				default:
					throw new NotImplementedException("GetSidedefPartSize: got unsupported geometry type: \"" + type + "\"");
			}

			return rect;
		}

		public static List<SortedVisualSide> SortVisualSides(IEnumerable<BaseVisualGeometrySidedef> tosort)
		{
			List<SortedVisualSide> result = new List<SortedVisualSide>();

			// Sort by texture
			Dictionary<long, List<BaseVisualGeometrySidedef>> sidesbytexture = new Dictionary<long, List<BaseVisualGeometrySidedef>>();
			foreach(BaseVisualGeometrySidedef side in tosort)
			{
				long texturelong;
				if(side is VisualLower) texturelong = side.Sidedef.LongLowTexture;
				else if(side is VisualUpper) texturelong = side.Sidedef.LongHighTexture;
				else if(side is VisualMiddle3D) texturelong = side.GetControlLinedef().Front.LongMiddleTexture;
				else texturelong = side.Sidedef.LongMiddleTexture;

				if(texturelong == MapSet.EmptyLongName) continue; //not interested...

				if(!sidesbytexture.ContainsKey(texturelong)) sidesbytexture.Add(texturelong, new List<BaseVisualGeometrySidedef>());
				sidesbytexture[texturelong].Add(side);
			}

			// Connect sides
			foreach(KeyValuePair<long, List<BaseVisualGeometrySidedef>> pair in sidesbytexture)
			{
				// Create strips
				Dictionary<int, List<SortedVisualSide>> strips = ConnectSides(pair.Value);

				// Calculate global bounds...
				foreach(List<SortedVisualSide> group in strips.Values) 
				{
					int minx = int.MaxValue;
					int maxx = int.MinValue;
					int miny = int.MaxValue;
					int maxy = int.MinValue;

					foreach(SortedVisualSide side in group) 
					{
						if(side.Bounds.X < minx) minx = side.Bounds.X;
						if(side.Bounds.X + side.Bounds.Width > maxx) maxx = side.Bounds.X + side.Bounds.Width;
						if(side.Bounds.Y < miny) miny = side.Bounds.Y;
						if(side.Bounds.Y + side.Bounds.Height > maxy) maxy = side.Bounds.Y + side.Bounds.Height;
					}

					Rectangle bounds = new Rectangle(minx, miny, maxx - minx, maxy - miny);

					// Normalize Y-offset
					int offsety = bounds.Y;
					bounds.Y = 0;

					// Apply changes
					foreach(SortedVisualSide side in group) 
					{
						side.Bounds.Y -= offsety;
						side.GlobalBounds = bounds;
					}

					// Add to result
					result.AddRange(group);
				}
			}

			return result;
		}

		// Connect sides, left to right and sort them into connected groups
		// NextSides - sides connected to the right (Start) vertex, 
		// PreviousSides - sides connected to the left (End) vertex
		private static Dictionary<int, List<SortedVisualSide>> ConnectSides(List<BaseVisualGeometrySidedef> allsides)
		{
			Dictionary<int, List<SortedVisualSide>> result = new Dictionary<int, List<SortedVisualSide>>();
			List<SortedVisualSide> sides = new List<SortedVisualSide>(allsides.Count);
			int groupindex = 0;

			foreach(BaseVisualGeometrySidedef side in allsides)
			{
				sides.Add(new SortedVisualSide(side));
			}

			foreach(SortedVisualSide curside in sides)
			{
				if(curside.GroupIndex == -1) curside.GroupIndex = groupindex++;
				
				// Find sides connected to the end of curside
				foreach(SortedVisualSide nextside in sides) 
				{
					if(curside.Index == nextside.Index) continue;
					if(nextside.Start == curside.End && nextside.End != curside.Start) 
					{
						// Add both ways
						if(!nextside.PreviousSides.ContainsKey(curside)) 
						{
							nextside.PreviousSides.Add(curside, false);
							nextside.GroupIndex = curside.GroupIndex;
						}
						if(!curside.NextSides.ContainsKey(nextside)) 
						{
							curside.NextSides.Add(nextside, false);
							nextside.GroupIndex = curside.GroupIndex;
						}
					}
				}

				// Find sides connected to the start of curside
				foreach(SortedVisualSide prevside in sides) 
				{
					if(curside.Index == prevside.Index) continue;
					if(prevside.End == curside.Start && prevside.Start != curside.End) 
					{
						// Add both ways
						if(!prevside.NextSides.ContainsKey(curside)) 
						{
							prevside.NextSides.Add(curside, false);
							prevside.GroupIndex = curside.GroupIndex;
						}
						if(!curside.PreviousSides.ContainsKey(prevside)) 
						{
							curside.PreviousSides.Add(prevside, false);
							prevside.GroupIndex = curside.GroupIndex;
						}
					}
				}

				// Add to collection
				if(!result.ContainsKey(curside.GroupIndex)) result.Add(curside.GroupIndex, new List<SortedVisualSide>());
				result[curside.GroupIndex].Add(curside);
			}

			// Try to find the left-most side
			foreach(KeyValuePair<int, List<SortedVisualSide>> pair in result) 
			{
				SortedVisualSide start = pair.Value[0];
				foreach(SortedVisualSide side in pair.Value) 
				{
					if(side.PreviousSides.Count == 0) {
						start = side;
						break;
					}
				}

				// Set horizontal offsets...
				ApplyHorizontalOffset(start, null, true, new Dictionary<int, bool>());
			}

			return result;
		}

		private static void ApplyHorizontalOffset(SortedVisualSide side, SortedVisualSide prevside, bool forward, Dictionary<int, bool> processed) 
		{
			// Set offset
			if(!processed.ContainsKey(side.Index))
			{
				if(prevside != null)
				{
					if(forward)
						side.Bounds.X = prevside.Bounds.X + (int)Math.Round(prevside.Side.Sidedef.Line.Length);
					else
						side.Bounds.X = prevside.Bounds.X - (int)Math.Round(side.Side.Sidedef.Line.Length);
				}

				processed.Add(side.Index, false);
			}

			// Repeat for NextSides
			foreach(KeyValuePair<SortedVisualSide, bool> pair in side.NextSides)
			{
				if(!processed.ContainsKey(pair.Key.Index))
					ApplyHorizontalOffset(pair.Key, side, true, processed);
			}

			// Repeat for PreviousSides
			foreach(KeyValuePair<SortedVisualSide, bool> pair in side.PreviousSides) 
			{
				if(!processed.ContainsKey(pair.Key.Index)) 
					ApplyHorizontalOffset(pair.Key, side, false, processed);
			}
		}

		#endregion

		#region ================== Things

		internal static float GetHigherThingZ(BaseVisualMode mode, SectorData sd, VisualThing thing)
		{
			Vector3D pos = thing.Thing.Position;
			float thingheight = thing.Thing.Height;
			bool absolute = thing.Info.AbsoluteZ;
			bool hangs = thing.Info.Hangs;
			
			if(absolute && hangs)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Sorry, can't have both 'absolute' and 'hangs' flags...");
				return pos.z;
			}

			// Get things, which bounding boxes intersect with target thing
			IEnumerable<Thing> intersectingthings = GetIntersectingThings(mode, thing.Thing);
			
			float fz = (absolute ? 0 : sd.Floor.plane.GetZ(pos));
			float cz = sd.Ceiling.plane.GetZ(pos);
			
			if(hangs)
			{
				// Transform to floor-aligned position
				Vector3D floorpos = new Vector3D(pos, (cz - fz) - pos.z - thingheight);
				float highertingz = GetNextHigherThingZ(mode, intersectingthings, floorpos.z, thingheight);
				float higherfloorz = float.MinValue;

				// Do it only when there are extrafloors
				if(sd.LightLevels.Count > 2)
				{
					// Unlike sd.ExtraFloors, these are sorted by height
					foreach(SectorLevel level in sd.LightLevels)
					{
						if(level.type == SectorLevelType.Light || level.type == SectorLevelType.Glow) continue; // Skip lights and glows
						float z = level.plane.GetZ(floorpos) - fz;
						if(level.type == SectorLevelType.Ceiling) z -= thingheight;
						if(z > floorpos.z)
						{
							higherfloorz = z;
							break;
						}
					}
				}

				if(higherfloorz != float.MinValue && highertingz != float.MaxValue)
				{
					// Transform back to ceiling-aligned position
					return cz - fz - Math.Max(Math.Min(higherfloorz, highertingz), 0) - thingheight; 
				}
				
				if(higherfloorz != float.MinValue)
				{
					// Transform back to ceiling-aligned position
					return Math.Max(cz - fz - higherfloorz - thingheight, 0); 
				}
				
				if(highertingz != float.MaxValue)
				{
					// Transform back to ceiling-aligned position
					return Math.Max(cz - fz - highertingz - thingheight, 0); 
				}

				return 0; // Align to real ceiling
			}
			else
			{
				float highertingz = GetNextHigherThingZ(mode, intersectingthings, (absolute ? pos.z - fz : pos.z), thingheight);
				float higherfloorz = float.MinValue;
				
				// Do it only when there are extrafloors
				if(sd.LightLevels.Count > 2)
				{
					// Unlike sd.ExtraFloors, these are sorted by height
					foreach(SectorLevel level in sd.LightLevels)
					{
						if(level.type == SectorLevelType.Light || level.type == SectorLevelType.Glow) continue; // Skip lights and glows
						float z = level.plane.GetZ(pos) - fz;
						if(level.type == SectorLevelType.Ceiling) z -= thingheight;
						if(z > pos.z)
						{
							higherfloorz = z;
							break;
						}
					}
				}

				float floorz = sd.Floor.plane.GetZ(pos);
				float ceilpos = cz - floorz - thingheight; // Ceiling-aligned relative target thing z
				
				if(higherfloorz != float.MinValue && highertingz != float.MaxValue) ceilpos = Math.Min(ceilpos, Math.Min(higherfloorz, highertingz));
				if(higherfloorz != float.MinValue) ceilpos = Math.Min(ceilpos, higherfloorz);
				if(highertingz != float.MaxValue) ceilpos = Math.Min(ceilpos, highertingz);
				
				return (absolute ? ceilpos + floorz : ceilpos); // Convert to absolute position if necessary
			}
		}

		internal static float GetLowerThingZ(BaseVisualMode mode, SectorData sd, VisualThing thing) 
		{
			Vector3D pos = thing.Thing.Position;
			float thingheight = thing.Thing.Height;
			bool absolute = thing.Info.AbsoluteZ;
			bool hangs = thing.Info.Hangs;
			
			if(absolute && hangs)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Sorry, can't have both 'absolute' and 'hangs' flags...");
				return pos.z;
			}

			// Get things, which bounding boxes intersect with target thing
			IEnumerable<Thing> intersectingthings = GetIntersectingThings(mode, thing.Thing);

			float fz = (absolute ? 0 : sd.Floor.plane.GetZ(pos));
			float cz = sd.Ceiling.plane.GetZ(pos);

			if(hangs) 
			{
				// Transform to floor-aligned position
				Vector3D floorpos = new Vector3D(pos, (cz - fz) - pos.z - thingheight);
				float lowertingz = GetNextLowerThingZ(mode, intersectingthings, floorpos.z, thingheight);
				float lowerfloorz = float.MaxValue;

				// Do it only when there are extrafloors
				if(sd.LightLevels.Count > 2)
				{
					// Unlike sd.ExtraFloors, these are sorted by height
					for(int i = sd.LightLevels.Count - 1; i > -1; i--)
					{
						SectorLevel level = sd.LightLevels[i];
						if(level.type == SectorLevelType.Light || level.type == SectorLevelType.Glow) continue; // Skip lights and glows
						float z = level.plane.GetZ(floorpos) - fz;
						if(level.type == SectorLevelType.Ceiling) z -= thingheight;
						if(z < floorpos.z)
						{
							lowerfloorz = z;
							break;
						}
					}
				}

				float floorz = cz - fz; // Floor height when counted from ceiling

				if(lowerfloorz != float.MaxValue && lowertingz != float.MinValue)
				{
					// Transform back to ceiling-aligned position
					return cz - fz - Math.Min(Math.Max(lowerfloorz, lowertingz), floorz) - thingheight;
				}

				if(lowerfloorz != float.MaxValue)
				{
					// Transform back to ceiling-aligned position
					return cz - fz - Math.Min(lowerfloorz, floorz) - thingheight;
				}

				if(lowertingz != float.MinValue)
				{
					// Transform back to ceiling-aligned position
					return cz - fz - Math.Min(lowertingz, floorz) - thingheight;
				}

				return floorz - thingheight; // Align to real floor
			} 
			else 
			{
				float lowertingz = GetNextLowerThingZ(mode, intersectingthings, (absolute ? pos.z - fz : pos.z), thingheight);
				float lowerfloorz = float.MaxValue;
				
				// Do it only when there are extrafloors
				if(sd.LightLevels.Count > 2)
				{
					// Unlike sd.ExtraFloors, these are sorted by height
					for(int i = sd.LightLevels.Count - 1; i > -1; i--)
					{
						SectorLevel level = sd.LightLevels[i];
						if(level.type == SectorLevelType.Light || level.type == SectorLevelType.Glow) continue; // Skip lights and glows
						float z = level.plane.GetZ(pos) - fz;
						if(level.type == SectorLevelType.Ceiling) z -= thingheight;
						if(z < pos.z)
						{
							lowerfloorz = z;
							break;
						}
					}
				}

				float floorz = sd.Floor.plane.GetZ(pos); // Floor-aligned relative target thing z
				float floorpos = 0;

				if(lowerfloorz != float.MaxValue && lowertingz != float.MinValue) floorpos = Math.Max(Math.Max(lowerfloorz, lowertingz), floorz);
				if(lowerfloorz != float.MaxValue) floorpos = Math.Max(lowerfloorz, floorz);
				if(lowertingz != float.MinValue) floorpos = Math.Max(lowertingz, floorz);

				return (absolute ? floorpos + floorz : floorpos); // Convert to absolute position if necessary
			}
		}

		//mxd. Gets thing z next higher to target thing z
		private static float GetNextHigherThingZ(BaseVisualMode mode, IEnumerable<Thing> things, float thingz, float thingheight)
		{
			float higherthingz = float.MaxValue;
			foreach(Thing t in things)
			{
				float neighbourz = GetAlignedThingZ(mode, t, thingheight);
				if(neighbourz > thingz && neighbourz < higherthingz) higherthingz = neighbourz;
			}
			return higherthingz;
		}

		//mxd. Gets thing z next lower to target thing z
		private static float GetNextLowerThingZ(BaseVisualMode mode, IEnumerable<Thing> things, float thingz, float thingheight)
		{
			float lowerthingz = float.MinValue;
			foreach(Thing t in things)
			{
				float neighbourz = GetAlignedThingZ(mode, t, thingheight);
				if(neighbourz < thingz && neighbourz > lowerthingz) lowerthingz = neighbourz;
			}

			return lowerthingz;
		}

		private static float GetAlignedThingZ(BaseVisualMode mode, Thing t, float targtthingheight)
		{
			ThingTypeInfo info = General.Map.Data.GetThingInfoEx(t.Type);
			if(info != null)
			{
				if(info.AbsoluteZ && info.Hangs) return t.Position.z; // Not sure what to do here...
				if(info.AbsoluteZ)
				{
					// Transform to floor-aligned position
					SectorData nsd = mode.GetSectorData(t.Sector);
					return t.Position.z - nsd.Floor.plane.GetZ(t.Position) + t.Height;
				}

				if(info.Hangs)
				{
					// Transform to floor-aligned position. Align top of target thing to the bottom of the hanging thing
					SectorData nsd = mode.GetSectorData(t.Sector);
					return (nsd.Ceiling.plane.GetZ(t.Position) - nsd.Floor.plane.GetZ(t.Position)) - t.Position.z - t.Height - targtthingheight;
				}
			}

			return t.Position.z + t.Height;
		}

		private static IEnumerable<Thing> GetIntersectingThings(VisualMode mode, Thing thing)
		{
			// Get nearby things
			List<Thing> neighbours = new List<Thing>();
			RectangleF bbox = new RectangleF(thing.Position.x - thing.Size, thing.Position.y - thing.Size, thing.Size * 2, thing.Size * 2);
			Point p1 = mode.BlockMap.GetBlockCoordinates(new Vector2D(bbox.Left, bbox.Top));
			Point p2 = mode.BlockMap.GetBlockCoordinates(new Vector2D(bbox.Right, bbox.Bottom));
			for(int x = p1.X; x <= p2.X; x++)
			{
				for(int y = p1.Y; y <= p2.Y; y++)
				{
					neighbours.AddRange(mode.BlockMap.GetBlock(new Point(x, y)).Things);
				}
			}

			// Collect things intersecting with target thing
			List<Thing> intersectingthings = new List<Thing>();
			
			foreach(Thing t in neighbours)
			{
				if(t != thing && t.Sector != null && bbox.IntersectsWith(new RectangleF(t.Position.x - t.Size, t.Position.y - t.Size, t.Size * 2, t.Size * 2)))
					intersectingthings.Add(t);
			}

			return intersectingthings;
		} 

		#endregion

		#region ================== Sectors

		// This gets sectors which surround given sectors
		internal static IEnumerable<Sector> GetSectorsAround(BaseVisualMode mode, IEnumerable<Sector> selected)
		{
			HashSet<int> processedsectors = new HashSet<int>();
			HashSet<Vertex> verts = new HashSet<Vertex>();
			List<Sector> result = new List<Sector>();

			foreach(Sector s in selected)
			{
				processedsectors.Add(s.Index);
				foreach(Sidedef side in s.Sidedefs)
				{
					if(!verts.Contains(side.Line.Start)) verts.Add(side.Line.Start);
					if(!verts.Contains(side.Line.End)) verts.Add(side.Line.End);
				}
			}

			foreach(Vertex v in verts)
			{
				foreach(Linedef l in v.Linedefs)
				{
					if(l.Front != null && l.Front.Sector != null && !processedsectors.Contains(l.Front.Sector.Index))
					{
						result.Add(l.Front.Sector);
						processedsectors.Add(l.Front.Sector.Index);

						// Add extrafloors as well
						SectorData sd = mode.GetSectorDataEx(l.Front.Sector);
						if(sd != null && sd.ExtraFloors.Count > 0)
						{
							foreach(Effect3DFloor effect in sd.ExtraFloors)
							{
								if(!processedsectors.Contains(effect.Linedef.Front.Sector.Index))
								{
									result.Add(effect.Linedef.Front.Sector);
									processedsectors.Add(effect.Linedef.Front.Sector.Index);
								}
							}
						}
					}
					if(l.Back != null && l.Back.Sector != null && !processedsectors.Contains(l.Back.Sector.Index))
					{
						result.Add(l.Back.Sector);
						processedsectors.Add(l.Back.Sector.Index);

						// Add extrafloors as well
						SectorData sd = mode.GetSectorDataEx(l.Back.Sector);
						if(sd != null && sd.ExtraFloors.Count > 0)
						{
							foreach(Effect3DFloor effect in sd.ExtraFloors)
							{
								if(!processedsectors.Contains(effect.Linedef.Front.Sector.Index))
								{
									result.Add(effect.Linedef.Front.Sector);
									processedsectors.Add(effect.Linedef.Front.Sector.Index);
								}
							}
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region ================== Texture Floodfill

		// This performs texture floodfill along all walls that match with the same texture
		// NOTE: This method uses the sidedefs marking to indicate which sides have been filled
		// When resetsidemarks is set to true, all sidedefs will first be marked false (not aligned).
		// Setting resetsidemarks to false is usefull to fill only within a specific selection
		// (set the marked property to true for the sidedefs outside the selection)
		public static void FloodfillTextures(BaseVisualMode mode, Sidedef start, HashSet<long> originaltextures, string filltexture, bool resetsidemarks)
		{
			Stack<Tools.SidedefFillJob> todo = new Stack<Tools.SidedefFillJob>(50);

			// Mark all sidedefs false (they will be marked true when the texture is aligned)
			if(resetsidemarks) General.Map.Map.ClearMarkedSidedefs(false);

			// Begin with first sidedef
			if(SidedefTextureMatch(mode, start, originaltextures))
				todo.Push(new Tools.SidedefFillJob { sidedef = start, forward = true });

			// Continue until nothing more to align
			while(todo.Count > 0)
			{
				// Get the align job to do
				Tools.SidedefFillJob j = todo.Pop();

				//mxd. Get visual parts
				if(mode.VisualSectorExists(j.sidedef.Sector))
				{
					VisualSidedefParts parts = ((BaseVisualSector)mode.GetVisualSector(j.sidedef.Sector)).GetSidedefParts(j.sidedef);
					
					// Apply texturing
					if((parts.upper != null && parts.upper.Triangles > 0) && originaltextures.Contains(j.sidedef.LongHighTexture))
						j.sidedef.SetTextureHigh(filltexture);
					if(((parts.middledouble != null && parts.middledouble.Triangles > 0) || (parts.middlesingle != null && parts.middlesingle.Triangles > 0)) && originaltextures.Contains(j.sidedef.LongMiddleTexture))
						j.sidedef.SetTextureMid(filltexture);
					if((parts.lower != null && parts.lower.Triangles > 0) && originaltextures.Contains(j.sidedef.LongLowTexture))
						j.sidedef.SetTextureLow(filltexture);
				}

				j.sidedef.Marked = true;

				if(j.forward)
				{
					// Add sidedefs forward (connected to the right vertex)
					Vertex v = j.sidedef.IsFront ? j.sidedef.Line.End : j.sidedef.Line.Start;
					AddSidedefsForFloodfill(mode, todo, v, true, originaltextures);

					// Add sidedefs backward (connected to the left vertex)
					v = j.sidedef.IsFront ? j.sidedef.Line.Start : j.sidedef.Line.End;
					AddSidedefsForFloodfill(mode, todo, v, false, originaltextures);
				}
				else
				{
					// Add sidedefs backward (connected to the left vertex)
					Vertex v = j.sidedef.IsFront ? j.sidedef.Line.Start : j.sidedef.Line.End;
					AddSidedefsForFloodfill(mode, todo, v, false, originaltextures);

					// Add sidedefs forward (connected to the right vertex)
					v = j.sidedef.IsFront ? j.sidedef.Line.End : j.sidedef.Line.Start;
					AddSidedefsForFloodfill(mode, todo, v, true, originaltextures);
				}
			}
		}

		// This adds the matching, unmarked sidedefs from a vertex for texture alignment
		private static void AddSidedefsForFloodfill(BaseVisualMode mode, Stack<Tools.SidedefFillJob> stack, Vertex v, bool forward, HashSet<long> texturelongnames)
		{
			foreach(Linedef ld in v.Linedefs)
			{
				Sidedef side1 = (forward ? ld.Front : ld.Back);
				Sidedef side2 = (forward ? ld.Back : ld.Front);

                // [ZZ] don't iterate the same linedef twice.
                //      
                if ((side1 != null && side1.Marked) ||
                    (side2 != null && side2.Marked)) continue;

                if ((ld.Start == v) && (side1 != null) && !side1.Marked)
				{
					if(SidedefTextureMatch(mode, side1, texturelongnames))
						stack.Push(new Tools.SidedefFillJob { forward = forward, sidedef = side1 });
				}
				else if((ld.End == v) && (side2 != null) && !side2.Marked)
				{
					if(SidedefTextureMatch(mode, side2, texturelongnames))
						stack.Push(new Tools.SidedefFillJob { forward = forward, sidedef = side2 });
				}
			}
		}

		#endregion

		#region ================== Texture Alignment

		//mxd. This checks if any of the sidedef texture match the given textures
		public static bool SidedefTextureMatch(BaseVisualMode mode, Sidedef sd, HashSet<long> texturelongnames)
		{
			if(!mode.VisualSectorExists(sd.Sector)) return false;
			VisualSidedefParts parts = ((BaseVisualSector)mode.GetVisualSector(sd.Sector)).GetSidedefParts(sd);

			return (texturelongnames.Contains(sd.LongHighTexture) && (parts.upper != null && parts.upper.Triangles > 0)) ||
				   (texturelongnames.Contains(sd.LongLowTexture) && (parts.lower != null && parts.lower.Triangles > 0)) ||
				   (texturelongnames.Contains(sd.LongMiddleTexture)
				   && ((parts.middledouble != null && parts.middledouble.Triangles > 0) || (parts.middlesingle != null && parts.middlesingle.Triangles > 0)));
		}

		#endregion
	}
}
