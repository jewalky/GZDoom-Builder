
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
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[EditMode(DisplayName = "Make Sectors Mode",
			  SwitchAction = "makesectormode",
			  ButtonImage = "NewSector2.png",	// Image resource name for the button
			  ButtonOrder = int.MinValue + 302,	// Position of the button (lower is more to the left)
			  ButtonGroup = "000_editing",
			  UseByDefault = true)]

	public class MakeSectorMode : BaseClassicMode
	{
		#region ================== Constants

		private const float FLASH_DURATION = 300.0f;

		#endregion

		#region ================== Variables

		// Nearest sidedef
		private LinedefSide editside;
		private LinedefSide nearestside;
		private List<LinedefSide> allsides;
		private List<Linedef> alllines;
		
		// Flash polygon
		private FlatVertex[] flashpolygon;
		private float flashintensity;
		private long flashstarttime;
		
		// Interface
		new private bool editpressed;

		//mxd. Used in overlay rendering
		private Dictionary<Sector, Sector> associates;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public MakeSectorMode()
		{
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				nearestside = null;
				allsides = null;
				
				// Dispose base
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		// This draws the geometry
		private void DrawGeometry()
		{
			associates = new Dictionary<Sector, Sector>();
			
			// Render lines and vertices
			if(renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				
				// Render sector indication
				if(allsides != null)
				{
					foreach(LinedefSide sd in allsides)
					{
						if(sd.Front)
						{
							if(sd.Line.Front != null)
							{
								if(!associates.ContainsKey(sd.Line.Front.Sector))
								{
									renderer.PlotSector(sd.Line.Front.Sector, General.Colors.Indication);
									associates[sd.Line.Front.Sector] = sd.Line.Front.Sector;
								}
							}
						}
						else
						{
							if(sd.Line.Back != null)
							{
								if(!associates.ContainsKey(sd.Line.Back.Sector))
								{
									renderer.PlotSector(sd.Line.Back.Sector, General.Colors.Indication);
									associates[sd.Line.Back.Sector] = sd.Line.Back.Sector;
								}
							}
						}
					}
				}
				
				// Render highlight
				if(alllines != null)
				{
					foreach(Linedef l in alllines) renderer.PlotLinedef(l, General.Colors.Highlight);
				}

				renderer.PlotVerticesSet(General.Map.Map.Vertices);
				renderer.Finish();
			}
		}

		// This draws the overlay
		private void DrawOverlay()
		{
			// Redraw overlay
			if(renderer.StartOverlay(true))
			{
				if((flashpolygon != null) && (flashintensity > 0.0f))
				{
					renderer.RenderGeometry(flashpolygon, null, true);
				} 
				else if(General.Settings.UseHighlight) //mxd
				{
					int color = General.Colors.Indication.WithAlpha(64).ToInt();
					foreach(Sector s in associates.Keys)
					{
						renderer.RenderHighlight(s.FlatVertices, color);
					}
				}

				renderer.Finish();
			}
		}

		// This highlights a new region
		private void Highlight(bool buttonspressed)
		{
			// Mouse inside?
			if(mouseinside)
			{
				// Highlighting from a new sidedef?
				Linedef nl = General.Map.Map.NearestLinedef(mousemappos);
				if(nl != null)
				{
					bool front = (nl.SideOfLine(mousemappos) <= 0.0f); //mxd
					LinedefSide newnearest = new LinedefSide(nl, front);
					if(newnearest != nearestside)
					{
						// Only change when buttons are not pressed
						if(!buttonspressed || (editside == newnearest))
						{
							// Find new sector
							nearestside = newnearest;
							allsides = Tools.FindPotentialSectorAt(nl, front); //mxd
							if(allsides != null)
							{
								alllines = new List<Linedef>(allsides.Count);
								foreach(LinedefSide sd in allsides) alllines.Add(sd.Line);
							}
							else
							{
								alllines = null;
							}
						}
						else
						{
							// Don't highlight this one
							nearestside = null;
							allsides = null;
							alllines = null;
						}

						// Redraw overlay
						DrawGeometry();
						DrawOverlay(); //mxd
						renderer.Present();
					}
				}
			}
			else
			{
				// No valid region
				nearestside = null;
				allsides = null;
				alllines = null;

				// Redraw overlay
				DrawGeometry();
				renderer.Present();
			}
		}

		// This makes this highlighted potential sector
		private Sector MakeSector()
		{
			General.Interface.SetCursor(Cursors.WaitCursor);
			General.Settings.FindDefaultDrawSettings();
			General.Map.UndoRedo.CreateUndo("Make Sector");
			
			// Mark the lines we are going to use for this sector
			General.Map.Map.ClearAllMarks(true);
			foreach(LinedefSide ls in allsides) ls.Line.Marked = false;
			List<Linedef> oldlines = General.Map.Map.GetMarkedLinedefs(true);
			
			// Make the sector
			Sector s = Tools.MakeSector(allsides, oldlines, false);
			if(s != null)
			{
				// Now we go for all the lines along the sector to
				// see if they only have a back side. In that case we want
				// to flip the linedef to that it only has a front side.
				foreach(Sidedef sd in s.Sidedefs)
				{
					if((sd.Line.Front == null) && (sd.Line.Back != null))
					{
						// Flip linedef
						sd.Line.FlipVertices();
						sd.Line.FlipSidedefs();
					}
				}

				General.Map.Data.UpdateUsedTextures();
				General.Interface.SetCursor(Cursors.Default);
				return s;
			}
			else
			{
				General.Map.UndoRedo.WithdrawUndo();
				return null;
			}
		}
		
		#endregion
		
		#region ================== Events

		public override void OnHelp()
		{
			General.ShowHelp("e_makesectors.html");
		}

		// When the mapset changes (undo/redo)
		public override void OnMapSetChangeBegin()
		{
			// No valid region
			nearestside = null;
			allsides = null;
			alllines = null;
			
			base.OnMapSetChangeBegin();
		}

		// Cancel mode
		public override void OnCancel()
		{
			base.OnCancel();
			
			// Return to base mode
			General.Editing.ChangeMode(new SectorsMode());
		}

		// Mode engages
		public override void OnEngage()
		{
			base.OnEngage();

			// Make customized presentation
			CustomPresentation p = new CustomPresentation();
			p.AddLayer(new PresentLayer(RendererLayer.Background, BlendingMode.Mask, General.Settings.BackgroundAlpha));
			p.AddLayer(new PresentLayer(RendererLayer.Surface, BlendingMode.Mask));
			p.AddLayer(new PresentLayer(RendererLayer.Grid, BlendingMode.Mask));
			p.AddLayer(new PresentLayer(RendererLayer.Overlay, BlendingMode.Alpha, 1f, true));
			p.AddLayer(new PresentLayer(RendererLayer.Things, BlendingMode.Alpha, General.Settings.InactiveThingsAlpha, false));
			p.AddLayer(new PresentLayer(RendererLayer.Geometry, BlendingMode.Alpha, 1f, true));
			renderer.SetPresentation(p);
			General.Map.Map.SelectionType = SelectionType.All;
		}

		// Mode disengages
		public override void OnDisengage()
		{
			base.OnDisengage();

			// Check which mode we are switching to
			if(General.Editing.NewMode is VerticesMode || General.Editing.NewMode is LinedefsMode)
			{
				// Clear selected sectors
				General.Map.Map.ClearSelectedSectors();
			}

			// Hide highlight info
			General.Interface.HideInfo();

			// Stop processing
			General.Interface.DisableProcessing();
		}

		// This redraws the display
		public override void OnRedrawDisplay()
		{
			renderer.RedrawSurface();
			
			// Render lines and vertices
			DrawGeometry();

			// Render things
			if(renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, General.Settings.HiddenThingsAlpha);
				renderer.RenderThingSet(General.Map.ThingsFilter.VisibleThings, General.Settings.ActiveThingsAlpha);
				renderer.Finish();
			}

			// Render overlay
			DrawOverlay();
			renderer.Present();
		}

		// Start select
		protected override void OnSelectBegin()
		{
			// Select pressed in this mode
			selectpressed = true;
			editside = nearestside;
			base.OnEditBegin();
		}
		
		// Done selecting
		protected override void OnSelectEnd()
		{
			// Select was pressed in this mode?
			if(selectpressed && (editside == nearestside) && (nearestside != null))
			{
				// Possible to make a sector?
				if(allsides != null)
				{
					// Make the sector
					Sector s = MakeSector();
					if(s != null)
					{
						// Quickly flash this sector to indicate it was created
						General.Map.IsChanged = true;
						General.Map.Map.Update();
						General.Interface.RedrawDisplay();
						flashpolygon = new FlatVertex[s.FlatVertices.Length];
						s.FlatVertices.CopyTo(flashpolygon, 0);
						flashintensity = 1.0f;
						flashstarttime = Clock.CurrentTime;
						General.Interface.EnableProcessing();
					}
					
					// Redraw overlay
					DrawGeometry();
					DrawOverlay();
					renderer.Present();
				}
			}

			base.OnSelectEnd();
		}
		
		// Start editing
		protected override void OnEditBegin()
		{
			// Edit pressed in this mode
			editpressed = true;
			editside = nearestside;
			base.OnEditBegin();
		}

		// Done editing
		protected override void OnEditEnd()
		{
			// Edit was pressed in this mode?
			if(editpressed && (editside == nearestside) && (nearestside != null))
			{
				// Possible to make a sector?
				if(allsides != null)
				{
					// Make the sector
					Sector s = MakeSector();
					if(s != null)
					{
						General.Map.Map.Update();

						// Edit the sector
						List<Sector> secs = new List<Sector>(); secs.Add(s);
						if(General.Interface.ShowEditSectors(secs) == DialogResult.OK)
						{
							// Quickly flash this sector to indicate it was created
							General.Map.IsChanged = true;
							General.Map.Map.Update();
							flashpolygon = new FlatVertex[s.FlatVertices.Length];
							s.FlatVertices.CopyTo(flashpolygon, 0);
							flashintensity = 1.0f;
							flashstarttime = Clock.CurrentTime;
							General.Interface.EnableProcessing();
						}
						else
						{
							// Undo
							General.Map.UndoRedo.WithdrawUndo();
						}
					}
					
					// Redraw overlay
					DrawGeometry();
					DrawOverlay();
					renderer.Present();
				}
			}
			
			editpressed = false;
			base.OnEditEnd();
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if(panning) return; //mxd. Skip all this jazz while panning

			// Highlight the region
			Highlight(e.Button != MouseButtons.None);
		}

		// Mouse leaves
		public override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			// Highlight nothing
			Highlight(false);
		}
		
		// Something is undone
		public override void OnUndoEnd()
		{
			base.OnUndoEnd();

			editside = null;
			nearestside = null;
			if(alllines != null)
				alllines.Clear();
			if(allsides != null)
				allsides.Clear();
		}

		// Something is redone
		public override void OnRedoEnd()
		{
			base.OnRedoEnd();

			editside = null;
			nearestside = null;
			if(alllines != null)
				alllines.Clear();
			if(allsides != null)
				allsides.Clear();
		}

		// Processing
		public override void OnProcess(long deltatime)
		{
			base.OnProcess(deltatime);

			// Process flash
			if(flashpolygon != null)
			{
				// Determine the intensity of the flash by time elapsed
				flashintensity = 1f - ((Clock.CurrentTime - flashstarttime) / FLASH_DURATION);
				if(flashintensity > 0.0f)
				{
					// Update vertices in polygon
					PixelColor pc = new PixelColor((byte)(flashintensity * 255.0f), 255, 255, 255);
					int intcolor = pc.ToInt();
					for(int i = 0; i < flashpolygon.Length; i++) flashpolygon[i].c = intcolor;
				}
				else
				{
					// End of flash, trash the polygon
					flashpolygon = null;
					flashintensity = 0.0f;
					General.Interface.DisableProcessing();
				}

				// Redraw overlay
				DrawOverlay();
				renderer.Present();
			}
		}
		
		#endregion
	}
}