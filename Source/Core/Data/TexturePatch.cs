
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

using System.IO;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.ZDoom;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public enum TexturePathRenderStyle
	{
		COPY,
		BLEND,
		ADD,
		SUBTRACT,
		REVERSE_SUBTRACT,
		MODULATE,
		COPY_ALPHA,
		COPY_NEW_ALPHA, //mxd
		OVERLAY, //mxd
	}

	public enum TexturePathBlendStyle //mxd
	{
		NONE,
		BLEND,
		TINT
	}
	
	internal struct TexturePatch
	{
		public readonly string LumpName;
		public readonly int X;
		public readonly int Y;
		public readonly bool FlipX;
		public readonly bool FlipY;
		public readonly bool HasLongName; //mxd
		public readonly int Rotate;
		public PixelColor BlendColor;
		public readonly float Alpha;
		public readonly TexturePathRenderStyle RenderStyle;
		public readonly TexturePathBlendStyle BlendStyle; //mxd
		public readonly bool Skip; //mxd
		
		// Constructor for simple patches
		public TexturePatch(string lumpname, int x, int y)
		{
			// Initialize
			this.LumpName = lumpname;
			this.X = x;
			this.Y = y < 0 ? 0 : y;
			this.FlipX = false;
			this.FlipY = false;
			this.Rotate = 0;
			this.BlendColor = new PixelColor(0, 0, 0, 0);
			this.Alpha = 1.0f;
			this.RenderStyle = TexturePathRenderStyle.COPY;
			this.BlendStyle = TexturePathBlendStyle.NONE;//mxd
			this.HasLongName = false; //mxd
			this.Skip = false; //mxd
		}

		//mxd. Constructor for hires patches
		public TexturePatch(PatchStructure patch) 
		{
			// Initialize
			this.LumpName = patch.Name.ToUpperInvariant();
			this.X = patch.OffsetX;
			this.Y = patch.OffsetY;
			this.FlipX = patch.FlipX;
			this.FlipY = patch.FlipY;
			this.Rotate = patch.Rotation;
			this.BlendColor = patch.BlendColor;
			this.Alpha = patch.Alpha;
			this.RenderStyle = patch.RenderStyle;
			this.BlendStyle = patch.BlendStyle;
			this.HasLongName = (Path.GetFileNameWithoutExtension(this.LumpName) != this.LumpName);
			this.Skip = patch.Skip;

			//mxd. Check data so we don't perform unneeded operations later on
			if(this.Alpha == 1.0f) 
			{
				switch(this.RenderStyle)
				{
					case TexturePathRenderStyle.BLEND:
					case TexturePathRenderStyle.COPY_ALPHA:
					case TexturePathRenderStyle.COPY_NEW_ALPHA:
					case TexturePathRenderStyle.OVERLAY:
						this.RenderStyle = TexturePathRenderStyle.COPY;
						break;
				}
			}

			//mxd. and get rid of render styles we don't support
			if(this.RenderStyle == TexturePathRenderStyle.OVERLAY) this.RenderStyle = TexturePathRenderStyle.COPY;
		}
	}
}
