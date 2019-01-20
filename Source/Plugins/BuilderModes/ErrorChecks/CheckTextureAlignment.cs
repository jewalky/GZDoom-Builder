#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.VisualModes;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check texture alignment", false, 1000)]
	public class CheckTextureAlignment : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 100;

		#endregion

		#region ================== Variables

		// Now THAT'S what I call a collection! :)
		private Dictionary<VisualGeometryType, Dictionary<int, Dictionary<VisualGeometryType, Dictionary<int, bool>>>> donesides;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public CheckTextureAlignment()
		{
			// Total progress is done when all lines are checked
			SetTotalProgress(General.Map.Map.Linedefs.Count / PROGRESS_STEP);
		}
		
		#endregion
		
		#region ================== Methods
		
		// This runs the check
		public override void Run()
		{
			donesides = new Dictionary<VisualGeometryType, Dictionary<int, Dictionary<VisualGeometryType, Dictionary<int, bool>>>>(3);
			donesides.Add(VisualGeometryType.WALL_UPPER, new Dictionary<int, Dictionary<VisualGeometryType, Dictionary<int, bool>>>());
			donesides.Add(VisualGeometryType.WALL_MIDDLE, new Dictionary<int, Dictionary<VisualGeometryType, Dictionary<int, bool>>>());
			donesides.Add(VisualGeometryType.WALL_LOWER, new Dictionary<int, Dictionary<VisualGeometryType, Dictionary<int, bool>>>());
			
			int progress = 0;
			int stepprogress = 0;

			// Go for all the liendefs
			foreach(Linedef l in General.Map.Map.Linedefs) 
			{
				// Check if we need to align any part of the front sidedef
				if(l.Front != null) 
				{
					if(!donesides[VisualGeometryType.WALL_UPPER].ContainsKey(l.Front.Index))
						CheckTopAlignment(l.Front);
					if(!donesides[VisualGeometryType.WALL_MIDDLE].ContainsKey(l.Front.Index))
						CheckMiddleAlignment(l.Front);
					if(!donesides[VisualGeometryType.WALL_LOWER].ContainsKey(l.Front.Index))
						CheckBottomAlignment(l.Front);
				}

				// Check if we need to align any part of the back sidedef
				if(l.Back != null) 
				{
					if(!donesides[VisualGeometryType.WALL_UPPER].ContainsKey(l.Back.Index))
						CheckTopAlignment(l.Back);
					if(!donesides[VisualGeometryType.WALL_MIDDLE].ContainsKey(l.Back.Index))
						CheckMiddleAlignment(l.Back);
					if(!donesides[VisualGeometryType.WALL_LOWER].ContainsKey(l.Back.Index))
						CheckBottomAlignment(l.Back);
				}

				// Handle thread interruption
				try
				{
					Thread.Sleep(0);
				}
				catch(ThreadInterruptedException)
				{
					// Clear collection
					donesides.Clear();
					return;
				}

				// We are making progress!
				if((++progress / PROGRESS_STEP) > stepprogress) 
				{
					stepprogress = (progress / PROGRESS_STEP);
					AddProgress(1);
				}
			}

			// Clear collection
			donesides.Clear();
		}

		#endregion

		#region ================== Alignment checks

		private void CheckTopAlignment(Sidedef sidedef) 
		{
			if(!sidedef.HighRequired() || sidedef.LongHighTexture == MapSet.EmptyLongName) return;
			
			float scaleX = sidedef.Fields.GetValue("scalex_top", 1.0f);
			float scaleY = sidedef.Fields.GetValue("scaley_top", 1.0f);
			int localY = (int)Math.Round(sidedef.Fields.GetValue("offsety_top", 0.0f));

			int x = sidedef.OffsetX + (int)Math.Round(sidedef.Fields.GetValue("offsetx_top", 0.0f));
			int y = (int)Tools.GetSidedefTopOffsetY(sidedef, sidedef.OffsetY + localY, scaleY, false);
			CheckAlignment(sidedef, x, y, scaleX, scaleY, VisualGeometryType.WALL_UPPER, sidedef.HighTexture);
		}

		private void CheckMiddleAlignment(Sidedef sidedef) 
		{
			if(!sidedef.MiddleRequired() || sidedef.LongMiddleTexture == MapSet.EmptyLongName) return;
			
			float scaleX = sidedef.Fields.GetValue("scalex_mid", 1.0f);
			float scaleY = sidedef.Fields.GetValue("scaley_mid", 1.0f);
			int localY = (int)Math.Round(sidedef.Fields.GetValue("offsety_mid", 0.0f));

			int x = sidedef.OffsetX + (int)Math.Round(sidedef.Fields.GetValue("offsetx_mid", 0.0f));
			int y = (int)Tools.GetSidedefMiddleOffsetY(sidedef, sidedef.OffsetY + localY, scaleY, false);
			CheckAlignment(sidedef, x, y, scaleX, scaleY, VisualGeometryType.WALL_MIDDLE, sidedef.MiddleTexture);
		}

		private void CheckBottomAlignment(Sidedef sidedef) 
		{
			if(!sidedef.LowRequired() || sidedef.LongLowTexture == MapSet.EmptyLongName) return;

			float scaleX = sidedef.Fields.GetValue("scalex_bottom", 1.0f);
			float scaleY = sidedef.Fields.GetValue("scaley_bottom", 1.0f);
			int localY = (int)Math.Round(sidedef.Fields.GetValue("offsety_bottom", 0.0f));

			int x = sidedef.OffsetX + (int)Math.Round(sidedef.Fields.GetValue("offsetx_bottom", 0.0f));
			int y = (int)Tools.GetSidedefBottomOffsetY(sidedef, sidedef.OffsetY + localY, scaleY, false);
			CheckAlignment(sidedef, x, y, scaleX, scaleY, VisualGeometryType.WALL_LOWER, sidedef.LowTexture);
		}

		#endregion

		#region ================== Methods

		private void CheckAlignment(Sidedef sidedef, int offsetx, int offsety, float linescalex, float linescaley, VisualGeometryType parttype, string texturename) 
		{
			ImageData texture = General.Map.Data.GetTextureImage(texturename);
			if(!texture.IsImageLoaded) return;
			Rectangle partsize = BuilderModesTools.GetSidedefPartSize(sidedef, parttype);

			float scalex = ((General.Map.Config.ScaledTextureOffsets && !texture.WorldPanning) ? texture.Scale.x : 1.0f);
			float scaley = ((General.Map.Config.ScaledTextureOffsets && !texture.WorldPanning) ? texture.Scale.y : 1.0f);

			// Move offsets to proper range
			offsetx %= texture.Width;
			if(offsetx < 0) offsetx += texture.Width;
			offsety %= texture.Height;
			if(offsety < 0) offsety += texture.Height;

			// Check if current line is aligned to other sides
			ICollection<Linedef> lines = (sidedef.IsFront ? sidedef.Line.Start.Linedefs : sidedef.Line.End.Linedefs);
			Vertex v = sidedef.IsFront ? sidedef.Line.Start : sidedef.Line.End;

			foreach(Linedef line in lines) 
			{
				if(line.Index == sidedef.Line.Index) continue;

				Sidedef target = null;
				if(line.Front != null && line.End == v) target = line.Front;
				else if(line.Back != null && line.Start == v) target = line.Back;

				// No target?
				if(target == null) continue;

				// Get expected texture offsets
				VisualGeometryType targetparttype;
				int alignedY = GetExpectedOffsetY(sidedef, target, texturename, texture.Height, scaley, linescaley, partsize, out targetparttype);
				if(targetparttype == VisualGeometryType.UNKNOWN) continue;

				// Already added?
				if(donesides[parttype].ContainsKey(sidedef.Index) && donesides[parttype][sidedef.Index][targetparttype].ContainsKey(target.Index))
					continue;

				// Not aligned if scaley is not equal
				float targetscaley = GetSidedefValue(target, targetparttype, "scaley", 1.0f);
				if(targetscaley != linescaley) 
				{
					SubmitResult(new ResultTexturesMisaligned(sidedef, target, texturename));
				}

				float targetscalex = GetSidedefValue(target, targetparttype, "scalex", 1.0f);

				alignedY %= texture.Height;
				if(alignedY < 0) alignedY += texture.Height;

				int alignedX = (target.OffsetX + (int)GetSidedefValue(target, targetparttype, "offsetx", 0f) + (int)Math.Round(target.Line.Length / scalex * targetscalex)) % texture.Width;
				if(alignedX < 0) alignedX += texture.Width;

				// Submit result if target offsets don't match expected ones
				if(offsetx != alignedX || offsety != alignedY) 
				{
					SubmitResult(new ResultTexturesMisaligned(sidedef, target, texturename));
				}

				// Add to collection
				AddProcessedSides(sidedef, parttype, target, targetparttype);
			}

			// Check if other sides are aligned to current side
			lines = (sidedef.IsFront ? sidedef.Line.End.Linedefs : sidedef.Line.Start.Linedefs);
			v = (sidedef.IsFront ? sidedef.Line.End : sidedef.Line.Start);

			foreach(Linedef line in lines) 
			{
				if(line.Index == sidedef.Line.Index) continue;

				Sidedef target = null;
				if(line.Front != null && line.Start == v) target = line.Front;
				else if(line.Back != null && line.End == v) target = line.Back;

				// No target or laready processed?
				if(target == null) continue;

				// Get expected texture offsets
				VisualGeometryType targetparttype;
				int alignedY = GetExpectedOffsetY(sidedef, target, texturename, texture.Height, scaley, linescaley, partsize, out targetparttype);
				if(targetparttype == VisualGeometryType.UNKNOWN) continue;

				// Already added?
				if(donesides[parttype].ContainsKey(sidedef.Index) && donesides[parttype][sidedef.Index][targetparttype].ContainsKey(target.Index))
					continue;

				// Not aligned if scaley is not equal
				float targetscaley = GetSidedefValue(target, targetparttype, "scaley", 1.0f);
				if(targetscaley != linescaley)
				{
					SubmitResult(new ResultTexturesMisaligned(sidedef, target, texturename));
				}

				alignedY %= texture.Height;
				if(alignedY < 0) alignedY += texture.Height;

				int alignedX = (target.OffsetX + (int)GetSidedefValue(target, targetparttype, "offsetx", 0f) - (int)Math.Round(sidedef.Line.Length / scalex * linescalex)) % texture.Width;
				if(alignedX < 0) alignedX += texture.Width;

				// Submit result if target offsets don't match expected ones
				if(offsetx != alignedX || offsety != alignedY) 
				{
					SubmitResult(new ResultTexturesMisaligned(sidedef, target, texturename));
				}

				// Add to collection
				AddProcessedSides(sidedef, parttype, target, targetparttype);
			}
		}

		private static float GetSidedefValue(Sidedef target, VisualGeometryType targetparttype, string key, float defaultvalue) 
		{
			switch(targetparttype)
			{
				case VisualGeometryType.WALL_UPPER:
					return (float)Math.Round(UniFields.GetFloat(target.Fields, key + "_top", defaultvalue), 3);

				case VisualGeometryType.WALL_MIDDLE:
					return (float)Math.Round(UniFields.GetFloat(target.Fields, key + "_mid", defaultvalue), 3);

				case VisualGeometryType.WALL_LOWER:
					return (float)Math.Round(UniFields.GetFloat(target.Fields, key + "_bottom", defaultvalue), 3);
			}

			return 0;
		}

		private void AddProcessedSides(Sidedef s1, VisualGeometryType type1, Sidedef s2, VisualGeometryType type2)
		{
			// Add them both ways
			if(!donesides[type1].ContainsKey(s1.Index)) 
			{
				donesides[type1].Add(s1.Index, new Dictionary<VisualGeometryType, Dictionary<int, bool>>());
				donesides[type1][s1.Index].Add(VisualGeometryType.WALL_UPPER, new Dictionary<int, bool>());
				donesides[type1][s1.Index].Add(VisualGeometryType.WALL_MIDDLE, new Dictionary<int, bool>());
				donesides[type1][s1.Index].Add(VisualGeometryType.WALL_LOWER, new Dictionary<int, bool>());
			}
			donesides[type1][s1.Index][type2].Add(s2.Index, false);

			if(!donesides[type2].ContainsKey(s2.Index)) 
			{
				donesides[type2].Add(s2.Index, new Dictionary<VisualGeometryType, Dictionary<int, bool>>());
				donesides[type2][s2.Index].Add(VisualGeometryType.WALL_UPPER, new Dictionary<int, bool>());
				donesides[type2][s2.Index].Add(VisualGeometryType.WALL_MIDDLE, new Dictionary<int, bool>());
				donesides[type2][s2.Index].Add(VisualGeometryType.WALL_LOWER, new Dictionary<int, bool>());
			}
			donesides[type2][s2.Index][type1].Add(s1.Index, false);
		}

		private static int GetExpectedOffsetY(Sidedef source, Sidedef target, string texturename, int textureheight, float texturescaley, float linescaley, Rectangle partsize, out VisualGeometryType matchingparttype) 
		{
			if(target.MiddleTexture == texturename
					&& partsize.IntersectsWith(BuilderModesTools.GetSidedefPartSize(target, VisualGeometryType.WALL_MIDDLE)))
			{
				matchingparttype = VisualGeometryType.WALL_MIDDLE;
				int partheight = (int)Math.Round(((target.Sector.CeilHeight - source.Sector.CeilHeight) / texturescaley) * linescaley);
				return ((int)Tools.GetSidedefMiddleOffsetY(target, target.OffsetY + GetSidedefValue(target, matchingparttype, "offsety", 0f), linescaley, false) + partheight) % textureheight;
			}
			
			if(target.HighTexture == texturename
					&& partsize.IntersectsWith(BuilderModesTools.GetSidedefPartSize(target, VisualGeometryType.WALL_UPPER))) 
			{
				matchingparttype = VisualGeometryType.WALL_UPPER;
				int partheight = (int) Math.Round(((target.Sector.CeilHeight - source.Sector.CeilHeight) / texturescaley) * linescaley);
				return ((int)Tools.GetSidedefTopOffsetY(target, target.OffsetY + GetSidedefValue(target, matchingparttype, "offsety", 0f), linescaley, false) + partheight) % textureheight;
			}

			if(target.LowTexture == texturename
				  && partsize.IntersectsWith(BuilderModesTools.GetSidedefPartSize(target, VisualGeometryType.WALL_LOWER))) 
			{
				matchingparttype = VisualGeometryType.WALL_LOWER;
				int partheight = (int)Math.Round(((target.Sector.CeilHeight - source.Sector.CeilHeight) / texturescaley) * linescaley);
				return ((int)Tools.GetSidedefBottomOffsetY(target, target.OffsetY + GetSidedefValue(target, matchingparttype, "offsety", 0f), linescaley, false) + partheight) % textureheight;
			}

			matchingparttype = VisualGeometryType.UNKNOWN;
			return int.MinValue;
		}

		#endregion
	}
}
