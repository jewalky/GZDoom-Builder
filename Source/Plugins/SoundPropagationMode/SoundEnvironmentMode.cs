
#region ================== Copyright (c) 2007 Pascal vd Heiden, 2014 Boris Iwanski

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * Copyright (c) 2014 Boris Iwanski
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
using System.ComponentModel;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.SoundPropagationMode
{
	[EditMode(DisplayName = "Sound Environment Mode",
			  SwitchAction = "soundenvironmentmode",		// Action name used to switch to this mode
			  ButtonImage = "ZDoomSoundEnvironment.png",	// Image resource name for the button
			  ButtonOrder = int.MinValue + 502,	// Position of the button (lower is more to the left)
			  ButtonGroup = "000_editing",
			  UseByDefault = true,
			  SafeStartMode = false,
			  SupportedMapFormats = new[] { "UniversalMapSetIO" }, //mxd
			  Volatile = false)]

	public class SoundEnvironmentMode : ClassicMode
	{
		#region ================== Variables

		// Highlighted item
		private Sector highlighted;
		private SoundEnvironment highlightedsoundenvironment;
		private SoundEnvironment oldhighlightedsoundenvironment; //mxd
		private Linedef highlightedline; //mxd
		private Thing highlightedthing; //mxd

		// Interface
		private static SoundEnvironmentPanel panel;
		private Docker docker;

		private BackgroundWorker worker;

		#endregion

		#region ================== Properties

		public override object HighlightedObject { get { return highlighted; } }
		internal const string ZoneBoundaryFlag = "zoneboundary"; //mxd
		internal static SoundEnvironmentPanel SoundEnvironmentPanel { get { return panel; } } //mxd

		#endregion

		#region ================== Constructor / Disposer

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

		// This highlights a new item
		private void Highlight(Sector s)
		{
			// Set new highlight
			highlighted = s;
			highlightedsoundenvironment = null;

			if(highlighted != null)
			{
				foreach(SoundEnvironment se in BuilderPlug.Me.SoundEnvironments)
				{
					if(se.Sectors.Contains(highlighted))
					{
						highlightedsoundenvironment = se;
						break;
					}
				}
			}

			panel.HighlightSoundEnvironment(highlightedsoundenvironment);
		}

		private void UpdateData() 
		{
			BuilderPlug.Me.DataIsDirty = false;

			panel.SoundEnvironments.Nodes.Clear();

			// Only update if map has changed or the sound environments were never updated at all (i.e. first time engaging this mode)
			if((General.Map.IsChanged || !BuilderPlug.Me.SoundEnvironmentIsUpdated) && !worker.IsBusy) 
			{
				General.Interface.DisplayStatus(StatusType.Busy, "Updating sound environments");
				worker.RunWorkerAsync();
			} 
			else if(!worker.IsBusy) 
			{
				panel.BeginUpdate(); //mxd
				
				foreach(SoundEnvironment se in BuilderPlug.Me.SoundEnvironments)
					panel.AddSoundEnvironment(se);

				panel.EndUpdate(); //mxd
			}
		}

		#endregion

		#region ================== Events

		public override void OnHelp()
		{
			General.ShowHelp("gzdb/features/classic_modes/mode_soundenvironment.html");
		}

		// Cancel mode
		public override void OnCancel()
		{
			base.OnCancel();

			// Return to this mode
			General.Editing.ChangeMode(new SoundEnvironmentMode());
		}

		// Mode engages
		public override void OnEngage()
		{
			base.OnEngage();

			General.Interface.AddButton(BuilderPlug.Me.MenusForm.ColorConfiguration);

			panel = new SoundEnvironmentPanel();
			panel.OnShowWarningsOnlyChanged += PanelOnOnShowWarningsOnlyChanged;
			docker = new Docker("soundenvironments", "Sound Environments", panel);
			General.Interface.AddDocker(docker, true);
			General.Interface.SelectDocker(docker);

			worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;

			worker.DoWork += BuilderPlug.Me.UpdateSoundEnvironments;
			worker.ProgressChanged += worker_ProgressChanged;
			worker.RunWorkerCompleted += worker_RunWorkerCompleted;

			UpdateData();

			CustomPresentation presentation = new CustomPresentation();
			presentation.AddLayer(new PresentLayer(RendererLayer.Background, BlendingMode.Mask, General.Settings.BackgroundAlpha));
			presentation.AddLayer(new PresentLayer(RendererLayer.Grid, BlendingMode.Mask));
			presentation.AddLayer(new PresentLayer(RendererLayer.Overlay, BlendingMode.Alpha, 1f, true));
			presentation.AddLayer(new PresentLayer(RendererLayer.Things, BlendingMode.Alpha, 1.0f));
			presentation.AddLayer(new PresentLayer(RendererLayer.Geometry, BlendingMode.Alpha, 1f, true));
			renderer.SetPresentation(presentation);
		}

		//mxd. If a linedef is highlighted, toggle the sound blocking flag, if a sound environment is clicked, select it in the tree view
		protected override void OnSelectEnd() 
		{
			if(highlightedline != null)
			{
				// Make undo
				General.Map.UndoRedo.CreateUndo("Toggle Sound Zone Boundary");

				// Toggle flag
				highlightedline.SetFlag(ZoneBoundaryFlag, !highlightedline.IsFlagSet(ZoneBoundaryFlag));

				// Update
				UpdateData();
				General.Interface.RedrawDisplay();
			}
			else //mxd
			{
				panel.SelectSoundEnvironment(highlightedsoundenvironment);
			}
		}

		//mxd. Show Reverb selector dialog or add a new sound environment thing
		protected override void OnEditEnd()
		{
			if(highlightedthing != null)
			{
				ReverbsPickerForm form = new ReverbsPickerForm(highlightedthing);
				if(form.ShowDialog((Form) General.Interface) == DialogResult.OK)
				{
					// Make undo
					General.Map.UndoRedo.CreateUndo("Change Sound Environment Settings");

					// Apply changes
					form.ApplyTo(highlightedthing);

					// Update
					UpdateData();
					General.Interface.RedrawDisplay();
				}
			}
			else
			{
				InsertThing();
			}
		}

		//mxd
		public override void OnUndoEnd()
		{
			base.OnUndoEnd();

			// Update
			UpdateData();
			General.Interface.RedrawDisplay();
		}

		//mxd
		public override void OnRedoEnd()
		{
			base.OnRedoEnd();

			// Update
			UpdateData();
			General.Interface.RedrawDisplay();
		}

		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			panel.HighlightSoundEnvironment(highlightedsoundenvironment); //mxd. Expand highlighted node in the treeview
			General.Interface.DisplayStatus(StatusType.Ready, "Finished updating sound environments");
			General.Interface.RedrawDisplay(); //mxd
		}

		private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			SoundEnvironment se = e.UserState as SoundEnvironment;
			General.Interface.DisplayStatus(StatusType.Busy, "Updating sound environments (" + e.ProgressPercentage + "%)");
			panel.AddSoundEnvironment(se);
			General.Interface.RedrawDisplay();
		}

		// Mode disengages
		public override void OnDisengage()
		{
			base.OnDisengage();

			worker.CancelAsync();
			worker.Dispose();
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.ColorConfiguration);
			General.Interface.RemoveDocker(docker);
			panel.Dispose(); //mxd
			panel = null; //mxd

			// Hide highlight info
			General.Interface.HideInfo();
		}

		// This redraws the display
		public override void OnRedrawDisplay()
		{
			if(BuilderPlug.Me.DataIsDirty) UpdateData();

			// Render lines and vertices
			if(renderer.StartPlotter(true))
			{
				// Plot lines by hand, so that no coloring (line specials, 3D floors etc.) distracts from
				// the sound environments. Also don't draw the line's normal. They are not needed here anyway
				// and can make it harder to see the sound environment colors
				foreach(Linedef ld in General.Map.Map.Linedefs)
				{
					PixelColor c;
					
					if(ld.IsFlagSet(General.Map.Config.ImpassableFlag))
						c = General.Colors.Linedefs;
					else
						c = General.Colors.Linedefs.WithAlpha(General.Settings.DoubleSidedAlphaByte);

					renderer.PlotLine(ld.Start.Position, ld.End.Position, c, BuilderPlug.LINE_LENGTH_SCALER);
				}

				// Since there will usually be way less blocking linedefs than total linedefs, it's presumably
				// faster to draw them on their own instead of checking if each linedef is in BlockingLinedefs
				lock(BuilderPlug.Me.BlockingLinedefs)
				{
					foreach(Linedef ld in BuilderPlug.Me.BlockingLinedefs)
					{
						renderer.PlotLine(ld.Start.Position, ld.End.Position, BuilderPlug.Me.BlockSoundColor, BuilderPlug.LINE_LENGTH_SCALER);
					}
				}

				//mxd. Render highlighted line
				if(highlightedline != null) 
				{
					renderer.PlotLine(highlightedline.Start.Position, highlightedline.End.Position, General.Colors.Highlight, BuilderPlug.LINE_LENGTH_SCALER);
				}

				renderer.Finish();
			}

			// Render things
			if(renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, General.Settings.HiddenThingsAlpha);
				renderer.RenderThingSet(General.Map.ThingsFilter.VisibleThings, General.Settings.InactiveThingsAlpha);

				lock(BuilderPlug.Me.SoundEnvironments)
				{
					foreach(SoundEnvironment se in BuilderPlug.Me.SoundEnvironments)
					{
						if(se.Things.Count == 0) continue;
						if(se == highlightedsoundenvironment)
						{
							foreach(Thing t in se.Things)
								renderer.RenderThing(t, General.Colors.Highlight, General.Settings.ActiveThingsAlpha);
						}
						else
						{
							renderer.RenderThingSet(se.Things, General.Settings.ActiveThingsAlpha);
						}
					}
				}

				//mxd. Render highlighted thing
				if(highlightedthing != null) 
					renderer.RenderThing(highlightedthing, General.Colors.Selection, General.Settings.ActiveThingsAlpha);

				renderer.Finish();
			}

			// Render overlay geometry (sectors)
			if(BuilderPlug.Me.OverlayGeometry != null)
			{
				lock(BuilderPlug.Me.OverlayGeometry)
				{
					if(BuilderPlug.Me.OverlayGeometry.Length > 0 && renderer.StartOverlay(true))
					{
						renderer.RenderGeometry(BuilderPlug.Me.OverlayGeometry, null, true);
						
						//mxd. Render highlighted sound environment
						if(General.Settings.UseHighlight && highlightedsoundenvironment != null)
							renderer.RenderHighlight(highlightedsoundenvironment.SectorsGeometry, General.Colors.Highlight.WithAlpha(128).ToInt());

						renderer.Finish();
					}
				}
			}

			renderer.Present();
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			// Not holding any buttons?
			if(e.Button == MouseButtons.None)
			{
				General.Interface.SetCursor(Cursors.Default);

				// Find the nearest linedef within highlight range
				Linedef l = General.Map.Map.NearestLinedef(mousemappos);
				if(l != null)
				{
					// Check on which side of the linedef the mouse is
					float side = l.SideOfLine(mousemappos);
					if(side > 0)
					{
						// Is there a sidedef here?
						if(l.Back != null)
						{
							// Highlight if not the same
							if(l.Back.Sector != highlighted) Highlight(l.Back.Sector);
						}
						else if(highlighted != null)
						{
							// Highlight nothing
							Highlight(null);
						}
					}
					else
					{
						// Is there a sidedef here?
						if(l.Front != null)
						{
							// Highlight if not the same
							if(l.Front.Sector != highlighted) Highlight(l.Front.Sector);
						}
						else if(highlighted != null)
						{
							// Highlight nothing
							Highlight(null);
						}
					}
				}
				else if(highlighted != null)
				{
					// Highlight nothing
					Highlight(null);
				}

				//mxd. Find the nearest linedef within default highlight range
				l = General.Map.Map.NearestLinedefRange(mousemappos, 20 / renderer.Scale);
				//mxd. We are not interested in single-sided lines, unless they have zoneboundary flag...
				if(l != null && ((l.Front == null || l.Back == null) && !l.IsFlagSet(ZoneBoundaryFlag))) l = null;

				//mxd. Set as highlighted
				bool redrawrequired = false;
				if(highlightedline != l) 
				{
					highlightedline = l;
					redrawrequired = true;
				}

				//mxd. Highlighted environment changed?
				if(oldhighlightedsoundenvironment != highlightedsoundenvironment)
				{
					oldhighlightedsoundenvironment = highlightedsoundenvironment;
					redrawrequired = true;
				}

				//mxd. Find the nearest thing within default highlight range
				if(highlightedline == null && highlightedsoundenvironment != null)
				{
					Thing t = MapSet.NearestThingSquareRange(highlightedsoundenvironment.Things, mousemappos, 10 / renderer.Scale);
					if(highlightedthing != t)
					{
						highlightedthing = t;
						redrawrequired = true;
					}
				} 
				else if(highlightedthing != null)
				{
					highlightedthing = null;
					redrawrequired = true;
				}

				//mxd
				if(redrawrequired)
				{
					// Show highlight info
					if(highlightedline != null && !highlightedline.IsDisposed)
						General.Interface.ShowLinedefInfo(highlightedline);
					else if(highlighted != null && !highlighted.IsDisposed)
						General.Interface.ShowSectorInfo(highlighted);
					else
						General.Interface.HideInfo();

					// Redraw display
					General.Interface.RedrawDisplay();
				}
			}
		}

		// Mouse leaves
		public override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			// Highlight nothing
			Highlight(null);
		}

		//mxd
		private void PanelOnOnShowWarningsOnlyChanged(object sender, EventArgs eventArgs)
		{
			UpdateData();
		}

		#endregion

		#region ================== Actions

		[BeginAction("soundpropagationcolorconfiguration")]
		public void ConfigureColors() 
		{
			ColorConfiguration cc = new ColorConfiguration();
			if(cc.ShowDialog((Form)General.Interface) == DialogResult.OK)
			{
				General.Interface.RedrawDisplay();
			}
		}

		// This creates a new thing at the mouse position
		[BeginAction("insertitem", BaseAction = true)]
		public void InsertThing()
		{
			// Mouse in window?
			if(mouseinside) 
			{
				// Check the boundaries
				if(mousemappos.x < General.Map.Config.LeftBoundary || mousemappos.x > General.Map.Config.RightBoundary ||
				   mousemappos.y > General.Map.Config.TopBoundary || mousemappos.y < General.Map.Config.BottomBoundary)
				{
					General.Interface.DisplayStatus(StatusType.Warning, "Failed to insert thing: outside of map boundaries.");
					return;
				}
				
				// Create undo
				General.Map.UndoRedo.CreateUndo("Insert sound environment thing");
				
				// Create thing
				Thing t = General.Map.Map.CreateThing();
				if(t == null)
				{
					General.Map.UndoRedo.WithdrawUndo();
					return;
				}

				General.Settings.ApplyDefaultThingSettings(t);
				t.Type = BuilderPlug.SOUND_ENVIROMNEMT_THING_TYPE;
				t.Move(mousemappos);
				t.UpdateConfiguration();

				// Update things filter so that it includes this thing
				General.Map.ThingsFilter.Update();

				// Snap to grid enabled?
				if(General.Interface.SnapToGrid)
				{
					// Snap to grid
					t.SnapToGrid();
				}
				else
				{
					// Snap to map format accuracy
					t.SnapToAccuracy();
				}

				// Add to current sound environment
				if(highlightedsoundenvironment != null) 
				{
					lock(highlightedsoundenvironment) 
					{
						highlightedsoundenvironment.Things.Add(t);
					}
				}

				// Edit the thing
				highlightedthing = t;
				General.Interface.RedrawDisplay(); // Redraw screen
				OnEditEnd();

				General.Interface.DisplayStatus(StatusType.Action, "Inserted a new sound environment thing.");

				// Update things filter
				General.Map.ThingsFilter.Update();

				// Update sound environments
				UpdateData();

				// Redraw screen again
				General.Interface.RedrawDisplay();
			}
		}

		[BeginAction("deleteitem", BaseAction = true)]
		public void DeleteItem() 
		{
			// Anything to do?
			if(highlightedthing == null) return;

			// Make undo
			General.Map.UndoRedo.CreateUndo("Delete sound environment thing");
			General.Interface.DisplayStatus(StatusType.Action, "Deleted a sound environment thing.");

			// Remove from current sound environment
			if(highlightedsoundenvironment != null && highlightedsoundenvironment.Things.Contains(highlightedthing))
			{
				lock(highlightedsoundenvironment)
				{
					highlightedsoundenvironment.Things.Remove(highlightedthing);
				}
			}

			// Dispose highlighted thing
			General.Map.Map.BeginAddRemove();
			highlightedthing.Dispose();
			highlightedthing = null;
			General.Map.Map.EndAddRemove();

			// Update cache values
			General.Map.IsChanged = true;
			General.Map.ThingsFilter.Update();

			// Update sound environments
			UpdateData();

			// Invoke a new mousemove so that the highlighted item updates
			highlightedsoundenvironment = null;
			OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, (int)mousepos.x, (int)mousepos.y, 0));

			// Redraw screen
			General.Interface.RedrawDisplay();
		}

		#endregion
	}
}
