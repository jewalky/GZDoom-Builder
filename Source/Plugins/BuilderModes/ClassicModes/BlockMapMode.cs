
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using System.Drawing;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[EditMode(DisplayName = "BlockMap Mode",
			  SwitchAction = "blockmapmode",
			  ButtonImage = "Blockmap.png",
			  ButtonOrder = 400,
			  ButtonGroup = "002_tools",
			  AllowCopyPaste = false,
			  UseByDefault = true,
			  SafeStartMode = true)]

	public sealed class BlockMapMode : BaseClassicMode
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private BlockMap<BlockEntry> blockmap;
		private BlockEntry highlighted;
		private int hx, hy;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public BlockMapMode()
		{
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Dispose base
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods
		
		#endregion

		#region ================== Events

		// Mode engages
		public override void OnEngage()
		{
			base.OnEngage();

			// Nothing highlighted
			highlighted = null;
			hx = -1;
			hy = -1;
			
			// Custom presentation to hide the grid
			CustomPresentation p = new CustomPresentation();
			p.AddLayer(new PresentLayer(RendererLayer.Background, BlendingMode.Mask, General.Settings.BackgroundAlpha));
			p.AddLayer(new PresentLayer(RendererLayer.Surface, BlendingMode.Mask));
			p.AddLayer(new PresentLayer(RendererLayer.Overlay, BlendingMode.Alpha, 1f, true));
			p.AddLayer(new PresentLayer(RendererLayer.Things, BlendingMode.Alpha, 1.0f));
			p.AddLayer(new PresentLayer(RendererLayer.Geometry, BlendingMode.Alpha, 1f, true));
			renderer.SetPresentation(p);
			
			// Make the blockmap
			RectangleF area = MapSet.CreateArea(General.Map.Map.Vertices);
			area = MapSet.IncreaseArea(area, General.Map.Map.Things);
			blockmap = new BlockMap<BlockEntry>(area);
			blockmap.AddLinedefsSet(General.Map.Map.Linedefs);
			blockmap.AddSectorsSet(General.Map.Map.Sectors);
			blockmap.AddThingsSet(General.Map.Map.Things);
		}

		// When disengaged
		public override void OnDisengage()
		{
			base.OnDisengage();

		}

		// This redraws the display
		public override void OnRedrawDisplay()
		{
			renderer.RedrawSurface();
			
			// Render lines and vertices
			if(renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				
				if(highlighted != null)
				{
					foreach(Linedef l in highlighted.Lines)
					{
						renderer.PlotLinedef(l, General.Colors.Highlight);
					}
				}

				renderer.PlotVerticesSet(General.Map.Map.Vertices);
				renderer.Finish();
			}

			// Render things
			if(renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, Presentation.THINGS_HIDDEN_ALPHA);
				renderer.RenderThingSet(General.Map.ThingsFilter.VisibleThings, 1.0f);

				if(highlighted != null)
				{
					foreach(Thing t in highlighted.Things)
					{
						renderer.RenderThing(t, General.Colors.Highlight, 1.0f);
					}
				}

				renderer.Finish();
			}

			// Render blocks
			if(renderer.StartOverlay(true))
			{
				for(int x = 0; x <= blockmap.Size.Width; x++)
				{
					Vector2D v1 = new Vector2D(blockmap.Range.X + (float)x * blockmap.BlockSize, blockmap.Range.Y);
					Vector2D v2 = new Vector2D(v1.x, blockmap.Range.Y + blockmap.Size.Height * blockmap.BlockSize);
					renderer.RenderLine(v1, v2, 2.0f, General.Colors.Selection, true);
				}
				
				for(int y = 0; y <= blockmap.Size.Height; y++)
				{
					Vector2D v1 = new Vector2D(blockmap.Range.X, blockmap.Range.Y + (float)y * blockmap.BlockSize);
					Vector2D v2 = new Vector2D(blockmap.Range.X + blockmap.Size.Width * blockmap.BlockSize, v1.y);
					renderer.RenderLine(v1, v2, 2.0f, General.Colors.Selection, true);
				}

				if((hx > -1) && (hy > -1))
				{
					Vector2D v1 = new Vector2D(blockmap.Range.X + (float)hx * blockmap.BlockSize, blockmap.Range.Y + (float)hy * blockmap.BlockSize);
					Vector2D v2 = new Vector2D(blockmap.Range.X + (float)(hx + 1) * blockmap.BlockSize, blockmap.Range.Y + (float)(hy + 1) * blockmap.BlockSize);
					renderer.RenderRectangle(new RectangleF(v1.x, v1.y, v2.x - v1.x, v2.y - v1.y), 2.0f, General.Colors.Highlight, true);
				}

				renderer.Finish();
			}
			
			renderer.Present();
		}

		// Mouse moves over display
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			BlockEntry newhighlight = null;
			hx = -1;
			hy = -1;
			
			// Determine highlighted block
			float nhx = (mousemappos.x - blockmap.Range.Left) / (float)blockmap.BlockSize;
			float nhy = (mousemappos.y - blockmap.Range.Top) / (float)blockmap.BlockSize;
			if((nhx < (float)blockmap.Size.Width) && (nhy < (float)blockmap.Size.Height) && (nhx >= 0.0f) && (nhy >= 0.0f))
			{
				newhighlight = blockmap.GetBlockAt(mousemappos);
				if(newhighlight != null)
				{
					hx = (int)nhx;
					hy = (int)nhy;
				}
			}
			
			if(newhighlight != highlighted)
			{
				highlighted = newhighlight;
				General.Interface.RedrawDisplay();
			}
		}

		#endregion
	}
}
