
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
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.BuilderModes.Interface;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[EditMode(DisplayName = "Sectors Mode",
			  SwitchAction = "sectorsmode",		// Action name used to switch to this mode
			  ButtonImage = "SectorsMode.png",	// Image resource name for the button
			  ButtonOrder = int.MinValue + 200,	// Position of the button (lower is more to the left)
			  ButtonGroup = "000_editing",
			  UseByDefault = true,
			  SafeStartMode = true)]

	public class SectorsMode : BaseClassicMode
	{
		#region ================== Constants

		private const int MAX_SECTOR_LABELS = 256; //mxd

		#endregion

		#region ================== Variables

		// Highlighted item
		private Sector highlighted;
		private readonly Association highlightasso = new Association();

		// Interface
		new private bool editpressed;
		private bool selectionfromhighlight; //mxd

		// Labels
		private Dictionary<Sector, TextLabel[]> labels;

		//mxd. Effects
		private readonly Dictionary<int, string[]> effects;
		
		//mxd. Cached overlays stuff
		private FlatVertex[] overlayGeometry;
		private Dictionary<Sector, string[]> selectedEffectLabels;
		private Dictionary<Sector, string[]> unselectedEffectLabels; 
		
		#endregion

		#region ================== Properties

		public override object HighlightedObject { get { return highlighted; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public SectorsMode()
		{
			//mxd
			effects = new Dictionary<int, string[]>();
			foreach(SectorEffectInfo info in General.Map.Config.SortedSectorEffects) 
			{
				string name = info.Index + ": " + info.Title;
				effects.Add(info.Index, new[] { name, "E" + info.Index });
			}
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Dispose old labels
				foreach(TextLabel[] lbl in labels.Values)
					foreach(TextLabel l in lbl) l.Dispose();

				// Dispose base
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		// This makes a CRC for the selection
		/*public int CreateSelectionCRC()
		{
			CRC crc = new CRC();
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			crc.Add(orderedselection.Count);
			foreach(Sector s in orderedselection)
			{
				crc.Add(s.FixedIndex);
			}
			return (int)(crc.Value & 0xFFFFFFFF);
		}*/

		//mxd. This makes a CRC for given selection
		private static int CreateSelectionCRC(ICollection<Sector> selection) 
		{
			CRC crc = new CRC();
			crc.Add(selection.Count);
			foreach(Sector s in selection) crc.Add(s.FixedIndex);
			return (int)(crc.Value & 0xFFFFFFFF);
		}

		// This sets up new labels
		private void SetupLabels()
		{
			if(labels != null)
			{
				// Dispose old labels
				foreach(TextLabel[] lbl in labels.Values)
					foreach(TextLabel l in lbl) l.Dispose();
			}

			// Make text labels for sectors
			PixelColor c = (General.Settings.UseHighlight ? General.Colors.Highlight : General.Colors.Selection); //mxd
			labels = new Dictionary<Sector, TextLabel[]>(General.Map.Map.Sectors.Count);
			foreach(Sector s in General.Map.Map.Sectors)
			{
				// Setup labels
				TextLabel[] labelarray = new TextLabel[s.Labels.Count];
				for(int i = 0; i < s.Labels.Count; i++)
				{
					labelarray[i] = new TextLabel();
					labelarray[i].TransformCoords = true;
					labelarray[i].Location = s.Labels[i].position;
					labelarray[i].AlignX = TextAlignmentX.Center;
					labelarray[i].AlignY = TextAlignmentY.Middle;
					labelarray[i].Color = c;
					labelarray[i].BackColor = General.Colors.Background.WithAlpha(128);
				}
				labels.Add(s, labelarray);
			}
		}

		// This updates the overlay
		private void UpdateOverlay()
		{
			if(renderer.StartOverlay(true))
			{
				// Go for all selected sectors
				ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
				
				//mxd. Render selected sectors
				if(General.Settings.UseHighlight) 
				{
					renderer.RenderHighlight(overlayGeometry, General.Colors.Selection.WithAlpha(64).ToInt());
				}

				//mxd. Render highlighted sector
				if(General.Settings.UseHighlight && highlighted != null) 
				{
					renderer.RenderHighlight(highlighted.FlatVertices, General.Colors.Highlight.WithAlpha(64).ToInt());
				}

				//mxd. Render comments
				if(General.Map.UDMF && General.Settings.RenderComments)
				{
					foreach(Sector s in General.Map.Map.Sectors) RenderComment(s);
				}

				if(BuilderPlug.Me.ViewSelectionNumbers && orderedselection.Count < MAX_SECTOR_LABELS) 
				{
					List<ITextLabel> torender = new List<ITextLabel>(orderedselection.Count);
					foreach(Sector s in orderedselection) 
					{
						//mxd. Self-referencing (and probably some other) sectors don't have labels...
						if(labels[s].Length == 0) continue;
						
						// Render labels
						TextLabel[] labelarray = labels[s];
						float requiredsize = (labelarray[0].TextSize.Height / 2) / renderer.Scale;
						for(int i = 0; i < s.Labels.Count; i++) 
						{
							// Render only when enough space for the label to see
							if(!string.IsNullOrEmpty(labelarray[i].Text) && requiredsize < s.Labels[i].radius)
								torender.Add(labelarray[i]);
						}
					}
					renderer.RenderText(torender);
				}

				//mxd. Render effect labels
				if(BuilderPlug.Me.ViewSelectionEffects) 
				{
					if(!BuilderPlug.Me.ViewSelectionNumbers) RenderEffectLabels(selectedEffectLabels);
					RenderEffectLabels(unselectedEffectLabels);
				}
				
				renderer.Finish();
			}
		}

		//mxd
		private void RenderEffectLabels(Dictionary<Sector, string[]> labelsGroup) 
		{
			List<ITextLabel> torender = new List<ITextLabel>(labelsGroup.Count);
			foreach(KeyValuePair<Sector, string[]> group in labelsGroup) 
			{
				// Pick which text variant to use
				TextLabel[] labelarray = labels[group.Key];
				for(int i = 0; i < group.Key.Labels.Count; i++) 
				{
					TextLabel l = labelarray[i];
					l.Color = General.Colors.InfoLine;

					// Render only when enough space for the label to see
					float requiredsize = (General.Interface.MeasureString(group.Value[0], l.Font).Width / 2) / renderer.Scale;
					if(requiredsize > group.Key.Labels[i].radius) 
					{
						requiredsize = (General.Interface.MeasureString(group.Value[1], l.Font).Width / 2) / renderer.Scale;
						if(requiredsize > group.Key.Labels[i].radius)
							l.Text = (requiredsize > group.Key.Labels[i].radius * 4 ? string.Empty : "+");
						else
							l.Text = group.Value[1];
					} 
					else 
					{
						l.Text = group.Value[0];
					}

					if(!string.IsNullOrEmpty(l.Text)) torender.Add(l);
				}
			}

			// Render labels
			renderer.RenderText(torender);
		}

		//mxd
		private string[] GetEffectText(Sector s) 
		{
			string tagstr = string.Empty;
			string tagstrshort = string.Empty;
			string effectstr = string.Empty;
			string effectstrshort = string.Empty;

			// Make effect text
			if(s.Effect != 0)
			{
				if(effects.ContainsKey(s.Effect))
					effectstr = effects[s.Effect][0];
				else
					effectstr = s.Effect + " - " + General.Map.Config.GetGeneralizedSectorEffectName(s.Effect);
				effectstrshort = "E" + s.Effect;
			}

			// Make tag text
			if(s.Tag != 0)
			{
				if(s.Tags.Count > 1)
				{
					string[] stags = new string[s.Tags.Count];
					for(int i = 0; i < s.Tags.Count; i++) stags[i] = s.Tags[i].ToString();
					tagstr = "Tags " + string.Join(", ", stags);
					tagstrshort = "T" + string.Join(",", stags);
				}
				else
				{
					tagstr = "Tag " + s.Tag;
					tagstrshort = "T" + s.Tag;
				}
			}

			// Combine them
			string[] result = new[] { string.Empty, string.Empty };
			if(s.Effect != 0 && s.Tag != 0)
			{
				result[0] = tagstr + Environment.NewLine + effectstr;
				result[1] = tagstrshort + Environment.NewLine + effectstrshort;
			}
			else if(s.Effect != 0)
			{
				result[0] = effectstr;
				result[1] = effectstrshort;
			} 
			else if(s.Tag != 0)
			{
				result[0] = tagstr;
				result[1] = tagstrshort;
			}

			return result;
		}

		//mxd
		private void UpdateOverlaySurfaces() 
		{
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			List<FlatVertex> vertsList = new List<FlatVertex>();
			
			// Go for all selected sectors
			foreach(Sector s in orderedselection) vertsList.AddRange(s.FlatVertices);
			overlayGeometry = vertsList.ToArray();
		}

		//mxd
		private void UpdateEffectLabels() 
		{
			selectedEffectLabels = new Dictionary<Sector, string[]>();
			unselectedEffectLabels = new Dictionary<Sector, string[]>();

			//update effect labels for selected sectors
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			foreach(Sector s in orderedselection) 
			{
				string[] labelText = GetEffectText(s);
				if(!string.IsNullOrEmpty(labelText[0])) 
					selectedEffectLabels.Add(s, labelText);
			}

			//update effect labels for unselected sectors
			orderedselection = General.Map.Map.GetSelectedSectors(false);
			foreach(Sector s in orderedselection) 
			{
				string[] labelText = GetEffectText(s);
				if(!string.IsNullOrEmpty(labelText[0]))
					unselectedEffectLabels.Add(s, labelText);
			}
		}
		
		// Support function for joining and merging sectors
		private void JoinMergeSectors(bool removelines)
		{
			// Remove lines in betwen joining sectors?
			if(removelines)
			{
				// Go for all selected linedefs
				List<Linedef> selectedlines = new List<Linedef>(General.Map.Map.GetSelectedLinedefs(true));
				foreach(Linedef ld in selectedlines)
				{
					// Front and back side?
					if((ld.Front != null) && (ld.Back != null))
					{
						// Both a selected sector, but not the same?
						if(ld.Front.Sector.Selected && ld.Back.Sector.Selected &&
						   (ld.Front.Sector != ld.Back.Sector))
						{
							// Remove this line
							ld.Dispose();
						}
					}
				}
			}

			// Find the first sector that is not disposed
			List<Sector> orderedselection = new List<Sector>(General.Map.Map.GetSelectedSectors(true));
			Sector first = null;
			foreach(Sector s in orderedselection)
				if(!s.IsDisposed) { first = s; break; }
			
			// Join all selected sectors with the first
			for(int i = 0; i < orderedselection.Count; i++)
				if((orderedselection[i] != first) && !orderedselection[i].IsDisposed)
					orderedselection[i].Join(first);

			// Clear selection
			General.Map.Map.ClearAllSelected();
			
			// Update
			General.Map.Map.Update();
			
			// Make text labels for sectors
			SetupLabels();
			UpdateSelectedLabels();
		}

		// This highlights a new item
		private void Highlight(Sector s)
		{
			// Often we can get away by simply undrawing the previous
			// highlight and drawing the new highlight. But if associations
			// are or were drawn we need to redraw the entire display.

			// Previous association highlights something?
			bool completeredraw = (highlighted != null) && (highlighted.Tag != 0);

			// Set highlight association
			if(s != null && s.Tag != 0) 
			{
				Vector2D center = (s.Labels.Count > 0 ? s.Labels[0].position : new Vector2D(s.BBox.X + s.BBox.Width / 2, s.BBox.Y + s.BBox.Height / 2));
				highlightasso.Set(center, s.Tags, UniversalType.SectorTag);
			} 
			else 
			{
				highlightasso.Set(new Vector2D(), 0, 0);
			}

			// New association highlights something?
			if((s != null) && (s.Tag != 0)) completeredraw = true;

			// Change label color
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				TextLabel[] labelarray = labels[highlighted];
				PixelColor c = (General.Settings.UseHighlight ? General.Colors.Highlight : General.Colors.Selection);
				foreach(TextLabel l in labelarray) l.Color = c;
			}
			
			// Change label color
			if((s != null) && !s.IsDisposed)
			{
				TextLabel[] labelarray = labels[s];
				PixelColor c = (General.Settings.UseHighlight ? General.Colors.Selection : General.Colors.Highlight);
				foreach(TextLabel l in labelarray) l.Color = c;
			}
			
			// If we're changing associations, then we
			// need to redraw the entire display
			if(completeredraw)
			{
				// Set new highlight and redraw completely
				highlighted = s;
				General.Interface.RedrawDisplay();
			}
			else
			{
				// Update display
				if(renderer.StartPlotter(false))
				{
					// Undraw previous highlight
					if((highlighted != null) && !highlighted.IsDisposed)
						renderer.PlotSector(highlighted);

					// Set new highlight
					highlighted = s;

					// Render highlighted item
					if((highlighted != null) && !highlighted.IsDisposed)
						renderer.PlotSector(highlighted, General.Colors.Highlight);
	
					// Done
					renderer.Finish();
				}
				
				UpdateOverlay();
				renderer.Present();
			}

			// Show highlight info
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				General.Interface.ShowSectorInfo(highlighted);
			}
			else
			{
				General.Interface.Display.HideToolTip(); //mxd
				General.Interface.HideInfo();
			}
		}

		// This selectes or deselects a sector
		private void SelectSector(Sector s, bool selectstate, bool update)
		{
			bool selectionchanged = false;

			if(!s.IsDisposed)
			{
				// Select the sector?
				if(selectstate && !s.Selected)
				{
					s.Selected = true;
					selectionchanged = true;
					
					// Setup labels
					if(update) 
					{ 
						//mxd
						string selectedCount = General.Map.Map.SelectedSectorsCount.ToString();
						PixelColor c = (General.Settings.UseHighlight ? General.Colors.Highlight : General.Colors.Selection);
						TextLabel[] labelarray = labels[s];
						foreach(TextLabel l in labelarray) 
						{
							l.Text = selectedCount;
							l.Color = c;
						}

						UpdateEffectLabels();
					}
				}
				// Deselect the sector?
				else if(!selectstate && s.Selected)
				{
					s.Selected = false;
					selectionchanged = true;

					// Clear labels
					if(update) 
					{
						TextLabel[] labelarray = labels[s];
						foreach(TextLabel l in labelarray) l.Text = "";

						// Update all other labels
						UpdateSelectedLabels();
					}
				}

				// Selection changed?
				if(selectionchanged)
				{
					// Make update lines selection
					foreach(Sidedef sd in s.Sidedefs)
					{
						bool front, back;
						if(sd.Line.Front != null) front = sd.Line.Front.Sector.Selected; else front = false;
						if(sd.Line.Back != null) back = sd.Line.Back.Sector.Selected; else back = false;
						sd.Line.Selected = front | back;
					}

					//mxd. Also (de)select things?
					if(General.Interface.AltState ^ BuilderPlug.Me.SyncronizeThingEdit)
					{
						foreach(Thing t in General.Map.Map.Things) 
							if(t.Sector == s && t.Selected != s.Selected) t.Selected = s.Selected;
					}

					if(update) 
					{
						UpdateOverlay();
						renderer.Present();
					}
				}
			}
		}

		// This updates labels from the selected sectors
		private void UpdateSelectedLabels()
		{
			// Don't show lables for selected-from-highlight item
			if(selectionfromhighlight) return;
			
			// Go for all labels in all selected sectors
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			PixelColor c = (General.Settings.UseHighlight ? General.Colors.Highlight : General.Colors.Selection); //mxd
			int index = 0;
			foreach(Sector s in orderedselection)
			{
				TextLabel[] labelarray = labels[s];
				foreach(TextLabel l in labelarray)
				{
					// Make sure the text and color are right
					int labelnum = index + 1;
					l.Text = labelnum.ToString();
					l.Color = c;
				}
				index++;
			}

			//mxd
			UpdateEffectLabels();
		}

		//mxd
		private bool IsInSelectionRect(Sector s, List<Line2D> selectionOutline) 
		{
			if(selectionrect.Contains(s.BBox)) return true;

			if(BuilderPlug.Me.MarqueSelectTouching && s.BBox.IntersectsWith(selectionrect)) 
			{
				//check endpoints
				foreach(Sidedef side in s.Sidedefs) 
				{
					if((selectionrect.Contains(side.Line.Start.Position.x, side.Line.Start.Position.y)
						|| selectionrect.Contains(side.Line.End.Position.x, side.Line.End.Position.y))) 
						return true;
				}

				//check line intersections
				foreach(Sidedef side in s.Sidedefs) 
				{
					foreach(Line2D line in selectionOutline) 
					{
						if(Line2D.GetIntersection(side.Line.Line, line)) return true;
					}
				}
			}

			return false;
		}

		//mxd. Gets map elements inside of selectionoutline and sorts them by distance to targetpoint
		private List<Sector> GetOrderedSelection(Vector2D targetpoint, List<Line2D> selectionoutline)
		{
			// Gather affected sectors
			List<Sector> result = new List<Sector>();
			foreach(Sector s in General.Map.Map.Sectors)
			{
				if(IsInSelectionRect(s, selectionoutline)) result.Add(s);
			}

			if(result.Count == 0) return result;

			// Sort by distance to targetpoint
			result.Sort(delegate(Sector s1, Sector s2)
			{
				if(s1 == s2) return 0;

				// Get closest distance from s1 to selectstart
				float closest1 = float.MaxValue;
				foreach(Sidedef side in s1.Sidedefs)
				{
					Vector2D pos = (side.IsFront ? side.Line.Start : side.Line.End).Position;
					float curdistance = Vector2D.DistanceSq(pos, targetpoint);
					if(curdistance < closest1) closest1 = curdistance;
				}

				// Get closest distance from s2 to selectstart
				float closest2 = float.MaxValue;
				foreach(Sidedef side in s2.Sidedefs)
				{
					Vector2D pos = (side.IsFront ? side.Line.Start : side.Line.End).Position;
					float curdistance = Vector2D.DistanceSq(pos, targetpoint);
					if(curdistance < closest2) closest2 = curdistance;
				}

				// Return closer one
				return (int)(closest1 - closest2);
			});

			return result;
		}

		//mxd
		public override void SelectMapElement(SelectableElement element)
		{
			Sector sector = element as Sector;
			if(sector != null) 
			{
				SelectSector(sector, true, true);

				// Update overlay
				UpdateOverlaySurfaces();
				UpdateSelectionInfo();
			}
		}

		#endregion
		
		#region ================== Events

		public override void OnHelp()
		{
			General.ShowHelp("e_sectors.html");
		}

		// Cancel mode
		public override void OnCancel()
		{
			base.OnCancel();

			// Return to this mode
			General.Editing.ChangeMode(new SectorsMode());
		}

		// Mode engages
		public override void OnEngage()
		{
			base.OnEngage();
			renderer.SetPresentation(Presentation.Standard);

			// Add toolbar buttons
			General.Interface.BeginToolbarUpdate(); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.CopyProperties);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.PasteProperties);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.PastePropertiesOptions); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SeparatorCopyPaste);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.ViewSelectionNumbers);
			BuilderPlug.Me.MenusForm.ViewSelectionEffects.Text = "View Tags and Effects"; //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.ViewSelectionEffects);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SeparatorSectors1);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeDoor); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SeparatorSectors2); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientBrightness);
			if(General.Map.UDMF) General.Interface.AddButton(BuilderPlug.Me.MenusForm.GradientModeMenu); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.GradientInterpolationMenu); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientFloors);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientCeilings);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SeparatorSectors3); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MarqueSelectTouching); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SyncronizeThingEditButton); //mxd
			if(General.Map.UDMF) General.Interface.AddButton(BuilderPlug.Me.MenusForm.TextureOffsetLock, ToolbarSection.Geometry); //mxd
			General.Interface.EndToolbarUpdate(); //mxd
			
			// Convert geometry selection to sectors only
			General.Map.Map.ConvertSelection(SelectionType.Sectors);

			//mxd. Update the tooltip
			BuilderPlug.Me.MenusForm.SyncronizeThingEditButton.ToolTipText = "Synchronized Things Editing" + Environment.NewLine + BuilderPlug.Me.MenusForm.SyncronizeThingEditSectorsItem.ToolTipText;

			//mxd. Determine thing sectors. Cause SyncronizeThingEdit requires that
			foreach(Thing t in General.Map.Map.Things) t.DetermineSector();

			//mxd. Select things as well?
			if(BuilderPlug.Me.SyncronizeThingEdit)
			{
				ICollection<Sector> sectors = General.Map.Map.GetSelectedSectors(true);
				if(sectors.Count > 0)
				{
					foreach(Thing t in General.Map.Map.Things)
					{
						if(!t.Selected && t.Sector != null && sectors.Contains(t.Sector))
							t.Selected = true;
					}
				}
			}

			// Make text labels for sectors
			SetupLabels();
			
			// Update
			UpdateSelectedLabels();
			UpdateOverlaySurfaces();//mxd
			UpdateSelectionInfo(); //mxd
		}
		
		// Mode disengages
		public override void OnDisengage()
		{
			base.OnDisengage();

			// Remove toolbar buttons
			General.Interface.BeginToolbarUpdate(); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.CopyProperties);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.PasteProperties);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.PastePropertiesOptions); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SeparatorCopyPaste);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.ViewSelectionNumbers);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.ViewSelectionEffects);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SeparatorSectors1);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeDoor); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SeparatorSectors2); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientBrightness);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.GradientModeMenu); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.GradientInterpolationMenu); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientFloors);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientCeilings);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SeparatorSectors3); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MarqueSelectTouching); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SyncronizeThingEditButton); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.TextureOffsetLock); //mxd
			General.Interface.EndToolbarUpdate(); //mxd
			
			// Keep only sectors selected
			General.Map.Map.ClearSelectedLinedefs();
			
			// Going to EditSelectionMode?
			EditSelectionMode mode = General.Editing.NewMode as EditSelectionMode;
			if(mode != null)
			{
				// Not pasting anything?
				if(!mode.Pasting)
				{
					// No selection made? But we have a highlight!
					if((General.Map.Map.GetSelectedSectors(true).Count == 0) && (highlighted != null))
					{
						// Make the highlight the selection
						SelectSector(highlighted, true, false);
						UpdateSelectedLabels(); //mxd
					}
				}
			}
			
			// Hide highlight info and tooltip
			General.Interface.HideInfo();
			General.Interface.Display.HideToolTip(); //mxd
		}

		// This redraws the display
		public override void OnRedrawDisplay()
		{
			renderer.RedrawSurface();
			List<Line3D> eventlines = new List<Line3D>(); //mxd
			
			// Render lines and vertices
			if(renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					renderer.PlotSector(highlighted, General.Colors.Highlight);
					BuilderPlug.PlotReverseAssociations(renderer, highlightasso, eventlines);
				}
				renderer.Finish();
			}

			// Render things
			if(renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, General.Settings.HiddenThingsAlpha);
				renderer.RenderThingSet(General.Map.ThingsFilter.VisibleThings, General.Settings.ActiveThingsAlpha);
				renderer.Finish();
			}

			// Render overlay
			UpdateOverlay();

			// Render selection
			if(renderer.StartOverlay(false)) 
			{
				if(highlighted != null && !highlighted.IsDisposed) BuilderPlug.RenderReverseAssociations(renderer, highlightasso, eventlines); //mxd
				if(selecting) RenderMultiSelection();
				renderer.RenderArrows(eventlines); //mxd
				renderer.Finish();
			}
			
			renderer.Present();
		}

		// Selection
		protected override void OnSelectBegin()
		{
			// Item highlighted?
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				// Update display
				if(renderer.StartPlotter(false))
				{
					// Redraw highlight to show selection
					renderer.PlotSector(highlighted);
					renderer.Finish();
					renderer.Present();
				}
			}

			base.OnSelectBegin();
		}

		// End selection
		protected override void OnSelectEnd()
		{
			// Not stopping from multiselection?
			if(!selecting)
			{
				// Item highlighted?
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					//mxd. Flip selection
					SelectSector(highlighted, !highlighted.Selected, true);
					
					// Update display
					if(renderer.StartPlotter(false))
					{
						// Render highlighted item
						renderer.PlotSector(highlighted, General.Colors.Highlight);
						renderer.Finish();
						renderer.Present();
					}

					// Update overlay
					TextLabel[] labelarray = labels[highlighted];
					PixelColor c = (General.Settings.UseHighlight ? General.Colors.Selection : General.Colors.Highlight);
					foreach(TextLabel l in labelarray) l.Color = c;
					UpdateOverlaySurfaces(); //mxd
					UpdateOverlay();
					renderer.Present();

					//mxd. Thing selection state may've changed
					if(General.Interface.AltState ^ BuilderPlug.Me.SyncronizeThingEdit) General.Interface.RedrawDisplay();
				}
				else if(BuilderPlug.Me.AutoClearSelection) //mxd
				{
					if(General.Map.Map.SelectedSectorsCount > 0)
					{
						General.Map.Map.ClearSelectedLinedefs();
						General.Map.Map.ClearSelectedSectors();
						UpdateOverlaySurfaces(); //mxd
					}

					//mxd
					if(BuilderPlug.Me.SyncronizeThingEdit && General.Map.Map.SelectedThingsCount > 0)
					{
						General.Map.Map.ClearSelectedThings();
					}
					
					General.Interface.RedrawDisplay();
				}

				UpdateSelectionInfo(); //mxd
			}

			base.OnSelectEnd();
		}

		// Start editing
		protected override void OnEditBegin()
		{
			// Item highlighted?
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				// Edit pressed in this mode
				editpressed = true;

				// Highlighted item not selected?
				if(!highlighted.Selected && (BuilderPlug.Me.AutoClearSelection || (General.Map.Map.SelectedSectorsCount == 0)))
				{
					// Make this the only selection
					selectionfromhighlight = true; //mxd
					General.Map.Map.ClearSelectedSectors();
					General.Map.Map.ClearSelectedLinedefs();
					SelectSector(highlighted, true, false);
					UpdateSelectedLabels(); //mxd
					UpdateOverlaySurfaces(); //mxd
					UpdateSelectionInfo(); //mxd
					General.Interface.RedrawDisplay();
				}

				// Update display
				if(renderer.StartPlotter(false))
				{
					// Redraw highlight to show selection
					renderer.PlotSector(highlighted);
					renderer.Finish();
					renderer.Present();
				}
			}
			else if(!selecting && BuilderPlug.Me.AutoDrawOnEdit) //mxd. We don't want to draw while multiselecting
			{
				// Start drawing mode
				DrawGeometryMode drawmode = new DrawGeometryMode();
				bool snaptogrid = General.Interface.ShiftState ^ General.Interface.SnapToGrid;
				bool snaptonearest = General.Interface.CtrlState ^ General.Interface.AutoMerge;
				DrawnVertex v = DrawGeometryMode.GetCurrentPosition(mousemappos, snaptonearest, snaptogrid, false, false, renderer, new List<DrawnVertex>());
				
				if(drawmode.DrawPointAt(v))
					General.Editing.ChangeMode(drawmode);
				else
					General.Interface.DisplayStatus(StatusType.Warning, "Failed to draw point: outside of map boundaries.");
			}
			
			base.OnEditBegin();
		}

		// Done editing
		protected override void OnEditEnd()
		{
			// Edit pressed in this mode?
			if(editpressed)
			{
				// Anything selected?
				ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
				if(selected.Count > 0)
				{
					if(General.Interface.IsActiveWindow)
					{
						//mxd. Show realtime vertex edit dialog
						General.Interface.OnEditFormValuesChanged += sectorEditForm_OnValuesChanged;
						DialogResult result = General.Interface.ShowEditSectors(selected);
						General.Interface.OnEditFormValuesChanged -= sectorEditForm_OnValuesChanged;

						General.Map.Renderer2D.UpdateExtraFloorFlag(); //mxd

						// When a single sector was selected, deselect it now
						if(selected.Count == 1 && selectionfromhighlight) 
						{
							General.Map.Map.ClearSelectedSectors();
							General.Map.Map.ClearSelectedLinedefs();

							//mxd. Also deselect things?
							if(BuilderPlug.Me.SyncronizeThingEdit)
							{
								Sector s = General.GetByIndex(selected, 0);
								foreach(Thing t in General.Map.Map.Things)
									if(t.Sector == s && t.Selected) t.Selected = false;
							}

							UpdateEffectLabels(); //mxd
						} 
						else if(result == DialogResult.Cancel) //mxd. Restore selection...
						{ 
							foreach(Sector s in selected) SelectSector(s, true, false);
							UpdateSelectedLabels(); //mxd
						}

						UpdateOverlaySurfaces(); //mxd
						General.Interface.RedrawDisplay();
					}
				}

				UpdateSelectionInfo(); //mxd
			}

			editpressed = false;
			selectionfromhighlight = false; //mxd
			base.OnEditEnd();
		}

		//mxd
		private void sectorEditForm_OnValuesChanged(object sender, EventArgs e) 
		{
			// Update entire display
			General.Map.Map.Update();
			General.Interface.RedrawDisplay();
		}
		
		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if(panning) return; //mxd. Skip all this jazz while panning

			//mxd
			if(selectpressed && !editpressed && !selecting) 
			{
				// Check if moved enough pixels for multiselect
				Vector2D delta = mousedownpos - mousepos;
				if((Math.Abs(delta.x) > MULTISELECT_START_MOVE_PIXELS) ||
				   (Math.Abs(delta.y) > MULTISELECT_START_MOVE_PIXELS)) 
				{
					// Start multiselecting
					StartMultiSelection();
				}
			}
			else if(paintselectpressed && !editpressed && !selecting) //mxd. Drag-select
			{
				// Find the nearest linedef within highlight range
				Linedef l = General.Map.Map.NearestLinedefRange(mousemappos, BuilderPlug.Me.HighlightRange / renderer.Scale);
				Sector s = null;

				if(l != null) 
				{
					// Check on which side of the linedef the mouse is
					float side = l.SideOfLine(mousemappos);
					if(side > 0) 
					{
						// Is there a sidedef here?
						if(l.Back != null) s = l.Back.Sector;
					} 
					else 
					{
						// Is there a sidedef here?
						if(l.Front != null) s = l.Front.Sector;
					}

					if(s != null) 
					{
						if(s != highlighted) 
						{
							//toggle selected state
							highlighted = s;
							if(General.Interface.ShiftState ^ BuilderPlug.Me.AdditiveSelect)
								SelectSector(highlighted, true, true);
							else if(General.Interface.CtrlState)
								SelectSector(highlighted, false, true);
							else
								SelectSector(highlighted, !highlighted.Selected, true);

							// Update entire display
							UpdateOverlaySurfaces();//mxd
							General.Interface.RedrawDisplay();
						}
					} 
					else if(highlighted != null) 
					{
						Highlight(null);

						// Update entire display
						General.Interface.RedrawDisplay();
					}

					UpdateSelectionInfo(); //mxd
				}
			} 
			else if(e.Button == MouseButtons.None) // Not holding any buttons?
			{
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
						else
						{
							// Highlight nothing
							if(highlighted != null) Highlight(null);
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
						else
						{
							// Highlight nothing
							if(highlighted != null) Highlight(null);
						}
					}
				}
				else
				{
					// Highlight nothing
					if(highlighted != null) Highlight(null);
				}

				//mxd. Show tooltip?
				if(General.Map.UDMF && General.Settings.RenderComments && mouselastpos != mousepos && highlighted != null && !highlighted.IsDisposed && highlighted.Fields.ContainsKey("comment"))
				{
					string comment = highlighted.Fields.GetValue("comment", string.Empty);
					if(comment.Length > 2)
					{
						string type = comment.Substring(0, 3);
						int index = Array.IndexOf(CommentType.Types, type);
						if(index > 0) comment = comment.TrimStart(type.ToCharArray());
					}
					General.Interface.Display.ShowToolTip("Comment:", comment, (int)(mousepos.x + 32 * MainForm.DPIScaler.Width), (int)(mousepos.y + 8 * MainForm.DPIScaler.Height));
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
		protected override void OnPaintSelectBegin() 
		{
			if(highlighted != null) 
			{
				if(General.Interface.ShiftState ^ BuilderPlug.Me.AdditiveSelect)
					SelectSector(highlighted, true, true);
				else if(General.Interface.CtrlState)
					SelectSector(highlighted, false, true);
				else
					SelectSector(highlighted, !highlighted.Selected, true);

				// Update entire display
				UpdateOverlaySurfaces();//mxd
				General.Interface.RedrawDisplay();
			}

			base.OnPaintSelectBegin();
		}

		// Mouse wants to drag
		protected override void OnDragStart(MouseEventArgs e)
		{
			base.OnDragStart(e);

			// Edit button used?
			if(General.Actions.CheckActionActive(null, "classicedit"))
			{
				// Anything highlighted?
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					// Highlighted item not selected?
					if(!highlighted.Selected)
					{
						// Select only this sector for dragging
						General.Map.Map.ClearSelectedSectors();
						SelectSector(highlighted, true, true);
						UpdateOverlaySurfaces(); //mxd
					}

					// Start dragging the selection
					if(!BuilderPlug.Me.DontMoveGeometryOutsideMapBoundary || CanDrag()) //mxd
						General.Editing.ChangeMode(new DragSectorsMode(mousedownmappos));
				}
			}
		}

		//mxd. Check if any selected sector is outside of map boundary
		private bool CanDrag() 
		{
			ICollection<Sector> selectedsectors = General.Map.Map.GetSelectedSectors(true);
			int unaffectedCount = 0;

			foreach(Sector s in selectedsectors) 
			{
				// Make sure the sector is inside the map boundary
				foreach(Sidedef sd in s.Sidedefs) 
				{
					if(sd.Line.Start.Position.x < General.Map.Config.LeftBoundary || sd.Line.Start.Position.x > General.Map.Config.RightBoundary
						|| sd.Line.Start.Position.y > General.Map.Config.TopBoundary || sd.Line.Start.Position.y < General.Map.Config.BottomBoundary
						|| sd.Line.End.Position.x < General.Map.Config.LeftBoundary || sd.Line.End.Position.x > General.Map.Config.RightBoundary
						|| sd.Line.End.Position.y > General.Map.Config.TopBoundary || sd.Line.End.Position.y < General.Map.Config.BottomBoundary) 
					{
						SelectSector(s, false, false);
						unaffectedCount++;
						break;
					}
				}
			}

			if(unaffectedCount == selectedsectors.Count) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Unable to drag selection: " + (selectedsectors.Count == 1 ? "selected sector is" : "all of selected sectors are") + " outside of map boundary!");
				General.Interface.RedrawDisplay();
				return false;
			}

			if(unaffectedCount > 0)
				General.Interface.DisplayStatus(StatusType.Warning, unaffectedCount + " of selected sectors " + (unaffectedCount == 1 ? "is" : "are") + " outside of map boundary!");

			UpdateSelectedLabels(); //mxd
			return true;
		}

		//mxd. This is called wheh selection ends
		protected override void OnEndMultiSelection()
		{
			bool selectionvolume = ((Math.Abs(base.selectionrect.Width) > 0.1f) && (Math.Abs(base.selectionrect.Height) > 0.1f));

			if(selectionvolume)
			{
				List<Line2D> selectionoutline = new List<Line2D>
				{
					new Line2D(selectionrect.Left, selectionrect.Top, selectionrect.Right, selectionrect.Top),
					new Line2D(selectionrect.Right, selectionrect.Top, selectionrect.Right, selectionrect.Bottom),
					new Line2D(selectionrect.Left, selectionrect.Bottom, selectionrect.Right, selectionrect.Bottom),
					new Line2D(selectionrect.Left, selectionrect.Bottom, selectionrect.Left, selectionrect.Top)
				};
				
				// (De)select sectors
				switch(marqueSelectionMode) 
				{
					case MarqueSelectionMode.SELECT:
						// Get ordered selection
						List<Sector> selectresult = GetOrderedSelection(base.selectstart, selectionoutline);

						// First deselect everything...
						foreach(Sector s in General.Map.Map.Sectors) SelectSector(s, false, false);
						
						// Then select sectors in correct order
						foreach(Sector s in selectresult) SelectSector(s, true, false);
						break;

					case MarqueSelectionMode.ADD:
						// Get ordered selection
						List<Sector> addresult = GetOrderedSelection(selectstart, selectionoutline);

						// First deselect everything inside of selection...
						foreach(Sector s in addresult) SelectSector(s, false, false);

						// Then reselect in correct order
						foreach(Sector s in addresult) SelectSector(s, true, false);
						break;

					case MarqueSelectionMode.SUBTRACT:
						// Selection order doesn't matter here
						foreach(Sector s in General.Map.Map.Sectors) 
						{
							if(!s.Selected) continue;
							if(IsInSelectionRect(s, selectionoutline))
								SelectSector(s, false, false);
						}
						break;

					// Should be Intersect selection mode
					default: 
						// Selection order doesn't matter here
						foreach(Sector s in General.Map.Map.Sectors) 
						{
							if(!s.Selected) continue;
							if(!IsInSelectionRect(s, selectionoutline))
								SelectSector(s, false, false);
						}
						break;
				}

				// Make sure all linedefs reflect selected sectors
				foreach(Sidedef sd in General.Map.Map.Sidedefs)
					sd.Line.Selected = sd.Sector.Selected || (sd.Other != null && sd.Other.Sector.Selected);

				//mxd. Clear labels for unselected sectors
				if(marqueSelectionMode != MarqueSelectionMode.ADD) 
				{
					ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(false);
					foreach(Sector s in orderedselection)
					{
						TextLabel[] labelarray = labels[s];
						foreach(TextLabel l in labelarray) l.Text = "";
					}
				}

				UpdateSelectedLabels(); //mxd
				UpdateSelectionInfo(); //mxd
				UpdateOverlaySurfaces(); //mxd
			}
			
			base.OnEndMultiSelection();
			if(renderer.StartOverlay(true)) renderer.Finish();
			General.Interface.RedrawDisplay();
		}

		// This is called when the selection is updated
		protected override void OnUpdateMultiSelection()
		{
			base.OnUpdateMultiSelection();

			// Render selection
			if(renderer.StartOverlay(true))
			{
				RenderMultiSelection();
				renderer.Finish();
				renderer.Present();
			}
		}

		// When copying
		public override bool OnCopyBegin()
		{
			// No selection made? But we have a highlight!
			if((General.Map.Map.GetSelectedSectors(true).Count == 0) && (highlighted != null))
			{
				// Make the highlight the selection
				SelectSector(highlighted, true, false);

				//mxd. Actually, we want it marked, not selected
				bool result = base.OnCopyBegin();
				SelectSector(highlighted, false, false);
				return result;
			}

			return base.OnCopyBegin();
		}

		// When undo is used
		public override bool OnUndoBegin()
		{
			// Clear ordered selection
			General.Map.Map.ClearAllSelected();

			return base.OnUndoBegin();
		}

		// When undo is performed
		public override void OnUndoEnd()
		{
			// Clear labels
			SetupLabels();
			UpdateEffectLabels(); //mxd
			UpdateOverlaySurfaces(); //mxd
			base.OnUndoEnd(); //mxd
		}
		
		// When redo is used
		public override bool OnRedoBegin()
		{
			// Clear ordered selection
			General.Map.Map.ClearAllSelected();

			return base.OnRedoBegin();
		}

		// When redo is performed
		public override void OnRedoEnd()
		{
			// Clear labels
			SetupLabels();
			UpdateEffectLabels(); //mxd
			UpdateOverlaySurfaces(); //mxd
			base.OnRedoEnd(); //mxd
		}

		//mxd
		public override void OnViewSelectionNumbersChanged(bool enabled)
		{
			if(enabled) UpdateSelectedLabels();
		}

		//mxd
		protected override void ToggleHighlight()
		{
			// Update label colors
			PixelColor c = (General.Settings.UseHighlight ? General.Colors.Selection : General.Colors.Highlight);
			foreach(TextLabel[] labelarray in labels.Values)
				foreach(TextLabel l in labelarray) l.Color = c;

			// Update highlighted label color
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				TextLabel[] labelarray = labels[highlighted];
				c = (General.Settings.UseHighlight ? General.Colors.Highlight : General.Colors.Selection);
				foreach(TextLabel l in labelarray) l.Color = c;
			}
			
			// Pass to base
			base.ToggleHighlight();
		}

		//mxd
		private void RenderComment(Sector s)
		{
			if(s.Selected || !s.Fields.ContainsKey("comment")) return;

			int iconindex = 0;
			string comment = s.Fields.GetValue("comment", string.Empty);
			if(comment.Length > 2)
			{
				string type = comment.Substring(0, 3);
				int index = Array.IndexOf(CommentType.Types, type);
				if(index != -1) iconindex = index;
			}

			if(s.Labels.Count > 0)
				foreach(LabelPositionInfo info in s.Labels) RenderComment(s, info.position, iconindex);
			else
				RenderComment(s, new Vector2D(s.BBox.Left + s.BBox.Width / 2, s.BBox.Top + s.BBox.Height / 2), iconindex);
		}

		//mxd
		private void RenderComment(Sector s, Vector2D center, int iconindex)
		{
			RectangleF rect = new RectangleF(center.x - 8 / renderer.Scale, center.y + 8 / renderer.Scale, 16 / renderer.Scale, -16 / renderer.Scale);
			PixelColor c = (s == highlighted ? General.Colors.Highlight : PixelColor.FromColor(Color.White));
			renderer.RenderRectangleFilled(rect, c, true, General.Map.Data.CommentTextures[iconindex]);
		}
		
		#endregion

		#region ================== Actions

		// This copies the properties
		[BeginAction("classiccopyproperties")]
		public void CopyProperties()
		{
			// Determine source sectors
			ICollection<Sector> sel = null;
			if(General.Map.Map.SelectedSectorsCount > 0) sel = General.Map.Map.GetSelectedSectors(true);
			else if(highlighted != null) sel = new List<Sector> { highlighted };
			
			if(sel != null)
			{
				// Copy properties from the first source sector
				BuilderPlug.Me.CopiedSectorProps = new SectorProperties(General.GetByIndex(sel, 0));
				General.Interface.DisplayStatus(StatusType.Action, "Copied sector properties.");
			}
			else
			{
				//mxd
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires highlight or selection!");
			}
		}

		// This pastes the properties
		[BeginAction("classicpasteproperties")]
		public void PasteProperties()
		{
			if(BuilderPlug.Me.CopiedSectorProps != null)
			{
				// Determine target sectors
				ICollection<Sector> sel = null;
				if(General.Map.Map.SelectedSectorsCount > 0) sel = General.Map.Map.GetSelectedSectors(true);
				else if(highlighted != null) sel = new List<Sector> { highlighted };
				
				if(sel != null)
				{
					// Apply properties to selection
					string rest = (sel.Count == 1 ? "a single sector" : sel.Count + " sectors"); //mxd
					General.Map.UndoRedo.CreateUndo("Paste properties to " + rest);
					BuilderPlug.Me.CopiedSectorProps.Apply(sel, false);
					foreach(Sector s in sel)
					{
						s.UpdateCeilingSurface();
						s.UpdateFloorSurface();
					}
					General.Interface.DisplayStatus(StatusType.Action, "Pasted properties to " + rest + ".");
					
					// Update and redraw
					General.Map.IsChanged = true;
					General.Interface.RefreshInfo();
					General.Map.Renderer2D.UpdateExtraFloorFlag(); //mxd
					UpdateEffectLabels(); //mxd
					General.Interface.RedrawDisplay();
				}
				else
				{
					//mxd
					General.Interface.DisplayStatus(StatusType.Warning, "This action requires highlight or selection!");
				}
			}
			else
			{
				//mxd
				General.Interface.DisplayStatus(StatusType.Warning, "Copy sector properties first!");
			}
		}

		//mxd. This pastes the properties with options
		[BeginAction("classicpastepropertieswithoptions")]
		public void PastePropertiesWithOptions()
		{
			if(BuilderPlug.Me.CopiedSectorProps != null)
			{
				// Determine target sectors
				ICollection<Sector> sel = null;
				if(General.Map.Map.SelectedSectorsCount > 0) sel = General.Map.Map.GetSelectedSectors(true);
				else if(highlighted != null) sel = new List<Sector> { highlighted };

				if(sel != null)
				{
					PastePropertiesOptionsForm form = new PastePropertiesOptionsForm();
					if(form.Setup(MapElementType.SECTOR) && form.ShowDialog(General.Interface) == DialogResult.OK)
					{
						// Apply properties to selection
						string rest = (sel.Count == 1 ? "a single sector" : sel.Count + " sectors");
						General.Map.UndoRedo.CreateUndo("Paste properties with options to " + rest);
						BuilderPlug.Me.CopiedSectorProps.Apply(sel, true);
						foreach(Sector s in sel)
						{
							s.UpdateCeilingSurface();
							s.UpdateFloorSurface();
						}
						General.Interface.DisplayStatus(StatusType.Action, "Pasted properties with options to " + rest + ".");

						// Update and redraw
						General.Map.IsChanged = true;
						General.Interface.RefreshInfo();
						General.Map.Renderer2D.UpdateExtraFloorFlag(); //mxd
						UpdateEffectLabels(); //mxd
						General.Interface.RedrawDisplay();
					}
				}
				else
				{
					General.Interface.DisplayStatus(StatusType.Warning, "This action requires highlight or selection!");
				}
			}
			else
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Copy sector properties first!");
			}
		}

		// This creates a new vertex at the mouse position
		[BeginAction("insertitem", BaseAction = true)]
		public void InsertVertexAction()
		{
			// Start drawing mode
			DrawGeometryMode drawmode = new DrawGeometryMode();
			if(mouseinside)
			{
				bool snaptogrid = General.Interface.ShiftState ^ General.Interface.SnapToGrid;
				bool snaptonearest = General.Interface.CtrlState ^ General.Interface.AutoMerge;
				DrawnVertex v = DrawGeometryMode.GetCurrentPosition(mousemappos, snaptonearest, snaptogrid, false, false, renderer, new List<DrawnVertex>());
				drawmode.DrawPointAt(v);
			}
			General.Editing.ChangeMode(drawmode);
		}

		[BeginAction("makedoor")]
		public void MakeDoor()
		{
			// Anything selected?
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			
			//mxd. Should we use highlighted item?
			if(orderedselection.Count == 0 && highlighted != null)
			{
				orderedselection.Add(highlighted);
			}
			
			if(orderedselection.Count > 0)
			{
				string doortex = BuilderPlug.Me.MakeDoor.DoorTexture;
				string tracktex = BuilderPlug.Me.MakeDoor.TrackTexture;
				string ceiltex = BuilderPlug.Me.MakeDoor.CeilingTexture;
				string floortex = null;
				bool resetoffsets = BuilderPlug.Me.MakeDoor.ResetOffsets;
				bool applyactionspecials = BuilderPlug.Me.MakeDoor.ApplyActionSpecials;
				bool applytag = BuilderPlug.Me.MakeDoor.ApplyTag;

				// Find floor texture
				foreach (Sector s in orderedselection)
				{
					if(floortex == null) floortex = s.FloorTexture; else if(floortex != s.FloorTexture) floortex = "";
				}
				
				// Show the dialog
				MakeDoorForm form = new MakeDoorForm();
				if(form.Show(General.Interface, doortex, tracktex, ceiltex, floortex, resetoffsets, applyactionspecials, applytag) == DialogResult.OK)
				{
					doortex = form.DoorTexture;
					tracktex = form.TrackTexture;
					ceiltex = form.CeilingTexture;
					floortex = form.FloorTexture;
					resetoffsets = form.ResetOffsets;
					applyactionspecials = form.ApplyActionSpecials;
					applytag = form.ApplyTag;

					//mxd. Store new settings
					BuilderPlug.Me.MakeDoor = new BuilderPlug.MakeDoorSettings(doortex, tracktex, ceiltex, resetoffsets, applyactionspecials, applytag);
					
					// Create undo
					General.Map.UndoRedo.CreateUndo("Make door (" + doortex + ")");
					General.Interface.DisplayStatus(StatusType.Action, "Created a " + doortex + " door.");
					
					// Go for all selected sectors
					foreach(Sector s in orderedselection)
					{
						// Lower the ceiling down to the floor
						s.CeilHeight = s.FloorHeight;

						// Make a unique tag (not sure if we need it yet, depends on the args)
						int tag = General.Map.Map.GetNewTag();

						// Go for all its sidedefs
						foreach(Sidedef sd in s.Sidedefs)
						{
							// Singlesided?
							if(sd.Other == null)
							{
								// Make this a doortrak
								sd.SetTextureHigh("-");
								if(!string.IsNullOrEmpty(tracktex)) sd.SetTextureMid(tracktex);
								sd.SetTextureLow("-");

								// Set upper/lower unpegged flags
								sd.Line.SetFlag(General.Map.Config.UpperUnpeggedFlag, false);
								sd.Line.SetFlag(General.Map.Config.LowerUnpeggedFlag, true);
							}
							else
							{
								// Set textures
								if(!string.IsNullOrEmpty(floortex)) s.SetFloorTexture(floortex);
								if(!string.IsNullOrEmpty(ceiltex)) s.SetCeilTexture(ceiltex);
								if(!string.IsNullOrEmpty(doortex)) sd.Other.SetTextureHigh(doortex);

								// Set upper/lower unpegged flags
								sd.Line.SetFlag(General.Map.Config.UpperUnpeggedFlag, false);
								sd.Line.SetFlag(General.Map.Config.LowerUnpeggedFlag, false);

								if (applyactionspecials)
								{
									// Get door linedef type from config
									sd.Line.Action = General.Map.Config.MakeDoorAction;

									// Set activation type
									sd.Line.Activate = General.Map.Config.MakeDoorActivate;

									// Set the flags for player repeatable activation
									foreach (var flagpair in General.Map.Config.MakeDoorFlags)
										sd.Line.SetFlag(flagpair.Key, flagpair.Value);
								}

								if (applytag)
								{
									//If tag checkbox is checked, apply our new tag no matter what happens next
									s.Tag = tag;
								}

								// Set the linedef args
								for(int i = 0; i < Linedef.NUM_ARGS; i++)
								{
									// A -1 arg indicates that the arg must be set to the new sector tag
									// and only in this case we set the tag on the sector, because only
									// then we know for sure that we need a tag.
									if(General.Map.Config.MakeDoorArgs[i] == -1)
									{
										sd.Line.Args[i] = tag;
										s.Tag = tag;
									}
									else
									{
										sd.Line.Args[i] = General.Map.Config.MakeDoorArgs[i];
									}
								}

								// Make sure the line is facing outwards
								if(sd.IsFront)
								{
									sd.Line.FlipVertices();
									sd.Line.FlipSidedefs();
								}
							}

							// Reset the texture offsets if required
							if(resetoffsets)
							{
								sd.OffsetX = 0;
								sd.OffsetY = 0;

								if(sd.Other != null)
								{
									sd.Other.OffsetX = 0;
									sd.Other.OffsetY = 0;
								}
							}
						}
					}
					
					// When a single sector was selected, deselect it now
					if(orderedselection.Count == 1) ClearSelection(); //mxd

					General.Map.Data.UpdateUsedTextures(); //mxd
				}
				
				// Done
				form.Dispose();
				General.Interface.RedrawDisplay();
			}
			else //mxd
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a highlight or selection!");
			}
		}
		
		[BeginAction("deleteitem", BaseAction = true)]
		public void DeleteItem()
		{
			//mxd. Make list of selected things
			List<Thing> selectedthings = (BuilderPlug.Me.SyncronizeThingEdit ? new List<Thing>(General.Map.Map.GetSelectedThings(true)) : new List<Thing>());
			
			// Make list of selected sectors
			List<Sector> selectedsectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));
			if((selectedsectors.Count == 0) && (highlighted != null) && !highlighted.IsDisposed)
			{
				selectedsectors.Add(highlighted);

				//mxd. Add things?
				if(BuilderPlug.Me.SyncronizeThingEdit)
				{
					foreach(Thing t in General.Map.Map.Things)
					{
						if(t.Sector == null) t.DetermineSector();
						if(t.Sector == highlighted) selectedthings.Add(t);
					}
				}
			}
			if(selectedsectors.Count == 0 && selectedthings.Count == 0) return; //mxd

			//mxd. Create undo info text
			List<string> info = new List<string>();

			//mxd. Create sectors info text
			if(selectedsectors.Count > 1)
				info.Add(selectedsectors.Count + " sectors");
			else
				info.Add("a sector");

			//mxd. Create things info text
			if(selectedthings.Count > 1)
				info.Add(selectedthings.Count + " things");
			else if(selectedthings.Count == 1)
				info.Add("a thing");

			//mxd. Make undo
			string rest = string.Join(" and ", info.ToArray());
			General.Map.UndoRedo.CreateUndo("Delete " + rest);
			General.Interface.DisplayStatus(StatusType.Action, "Deleted " + rest + ".");

			//mxd. Delete things
			if(selectedthings.Count > 0)
			{
				DeleteThings(selectedthings);
				General.Map.ThingsFilter.Update();
			}

			// Anything to do?
			if(selectedsectors.Count > 0)
			{
				General.Map.Map.BeginAddRemove(); //mxd

				// Dispose selected sectors
				foreach(Sector s in selectedsectors)
				{
					//mxd. Get all the linedefs
					List<Linedef> lines = new List<Linedef>(s.Sidedefs.Count);
					foreach(Sidedef side in s.Sidedefs) lines.Add(side.Line);
					
					// Dispose the sector
					s.Dispose();

					// Check all the lines
					for(int i = lines.Count - 1; i >= 0; i--)
					{
						// If the line has become orphaned, remove it
						if((lines[i].Front == null) && (lines[i].Back == null))
						{
							// Remove line
							lines[i].Dispose();
						}
						else
						{
							// If the line only has a back side left, flip the line and sides
							if((lines[i].Front == null) && (lines[i].Back != null))
							{
								lines[i].FlipVertices();
								lines[i].FlipSidedefs();
							}

							//mxd. Check textures.
							if(lines[i].Front.MiddleRequired() && lines[i].Front.LongMiddleTexture == MapSet.EmptyLongName) 
							{
								if(lines[i].Front.LongHighTexture != MapSet.EmptyLongName) 
								{
									lines[i].Front.SetTextureMid(lines[i].Front.HighTexture);
								} 
								else if(lines[i].Front.LongLowTexture != MapSet.EmptyLongName) 
								{
									lines[i].Front.SetTextureMid(lines[i].Front.LowTexture);
								}
							}

							//mxd. Do we still need high/low textures?
							lines[i].Front.RemoveUnneededTextures(false);
							
							// Update sided flags
							lines[i].ApplySidedFlags();
						}
					}
				}

				General.Map.Map.EndAddRemove(); //mxd
			}

			if(selectedthings.Count > 0 || selectedsectors.Count > 0)
			{
				// Update cache values
				General.Map.IsChanged = true;
				General.Map.Map.Update();
				UpdateOverlaySurfaces(); //mxd
				
				// Make text labels for sectors
				SetupLabels();
				UpdateSelectedLabels();
				
				// Redraw screen
				UpdateSelectionInfo(); //mxd
				General.Map.Renderer2D.UpdateExtraFloorFlag(); //mxd
				General.Interface.RedrawDisplay();
			}
		}

		[BeginAction("dissolveitem", BaseAction = true)] //mxd
		public void DissolveItem() 
		{
			//TODO handle this differently?..
			DeleteItem();
		}
		
		// This joins sectors together and keeps all lines
		[BeginAction("joinsectors")]
		public void JoinSectors()
		{
			// Worth our money?
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true); //mxd
			if(selected.Count > 1)
			{
				// Make undo
				General.Map.UndoRedo.CreateUndo("Join " + selected.Count + " sectors");
				General.Interface.DisplayStatus(StatusType.Action, "Joined " + selected.Count + " sectors.");

				//mxd. Things may require updating
				List<Thing> affectedthings = new List<Thing>();
				foreach(Thing t in General.Map.Map.Things)
					if(t.Sector != null && selected.Contains(t.Sector)) affectedthings.Add(t);
					
				// Merge
				JoinMergeSectors(false);

				//mxd. Things may require updating
				foreach(Thing t in affectedthings) t.DetermineSector();

				// Map was changed
				General.Map.IsChanged = true;

				//mxd. Clear selection info
				General.Interface.DisplayStatus(StatusType.Selection, string.Empty);

				//mxd. Update
				UpdateOverlaySurfaces();
				UpdateEffectLabels();
				General.Map.Renderer2D.UpdateExtraFloorFlag();

				// Redraw display
				General.Interface.RedrawDisplay();
			}
		}

		// This joins sectors together and removes the lines in between
		[BeginAction("mergesectors")]
		public void MergeSectors()
		{
			// Worth our money?
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true); //mxd
			if(selected.Count > 1)
			{
				// Make undo
				General.Map.UndoRedo.CreateUndo("Merge " + selected.Count + " sectors");
				General.Interface.DisplayStatus(StatusType.Action, "Merged " + selected.Count + " sectors.");

				//mxd. Things may require updating
				List<Thing> affectedthings = new List<Thing>();
				foreach(Thing t in General.Map.Map.Things)
					if(t.Sector != null && selected.Contains(t.Sector)) affectedthings.Add(t);

				// Merge
				JoinMergeSectors(true);

				//mxd. Things may require updating
				foreach(Thing t in affectedthings) t.DetermineSector();

				// Map was changed
				General.Map.IsChanged = true;

				//mxd. Clear selection info
				General.Interface.DisplayStatus(StatusType.Selection, string.Empty);
				
				//mxd. Update
				UpdateOverlaySurfaces();
				UpdateEffectLabels();
				General.Map.Renderer2D.UpdateExtraFloorFlag();

				// Redraw display
				General.Interface.RedrawDisplay();
			}
		}

		// Make gradient brightness
		[BeginAction("gradientbrightness")]
		public void MakeGradientBrightness()
		{
			// Need at least 3 selected sectors
			// The first and last are not modified
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			if(orderedselection.Count > 2) 
			{
				General.Interface.DisplayStatus(StatusType.Action, "Created gradient brightness over selected sectors.");
				General.Map.UndoRedo.CreateUndo("Gradient brightness");

				//mxd
				Sector start = General.GetByIndex(orderedselection, 0);
				Sector end = General.GetByIndex(orderedselection, orderedselection.Count - 1);

				//mxd. Use UDMF light?
				string mode = (string)BuilderPlug.Me.MenusForm.GradientModeMenu.SelectedItem;
				InterpolationTools.Mode interpolationmode = (InterpolationTools.Mode) BuilderPlug.Me.MenusForm.GradientInterpolationMenu.SelectedIndex;
				if(General.Map.UDMF && mode != MenusForm.BrightnessGradientModes.Sectors) 
				{
					if(mode == MenusForm.BrightnessGradientModes.Ceilings || mode == MenusForm.BrightnessGradientModes.Floors) 
					{
						string lightKey;
						string lightAbsKey;
						int startbrightness, endbrightness;

						if(mode == MenusForm.BrightnessGradientModes.Ceilings) 
						{
							lightKey = "lightceiling";
							lightAbsKey = "lightceilingabsolute";
						}
						else //should be floors...
						{
							lightKey = "lightfloor";
							lightAbsKey = "lightfloorabsolute";
						}

						//get total brightness of start sector
						if(start.Fields.GetValue(lightAbsKey, false))
							startbrightness = start.Fields.GetValue(lightKey, 0);
						else
							startbrightness = Math.Min(255, Math.Max(0, start.Brightness + start.Fields.GetValue(lightKey, 0)));

						//get total brightness of end sector
						if(end.Fields.GetValue(lightAbsKey, false))
							endbrightness = end.Fields.GetValue(lightKey, 0);
						else
							endbrightness = Math.Min(255, Math.Max(0, end.Brightness + end.Fields.GetValue(lightKey, 0)));

						// Go for all sectors in between first and last
						int index = 0;
						foreach(Sector s in orderedselection) 
						{
							s.Fields.BeforeFieldsChange();
							float u = index / (float) (orderedselection.Count - 1);
							float b = (float)Math.Round(InterpolationTools.Interpolate(startbrightness, endbrightness, u, interpolationmode));

							//absolute flag set?
							if(s.Fields.GetValue(lightAbsKey, false)) 
							{
								if(s.Fields.ContainsKey(lightKey))
									s.Fields[lightKey].Value = (int) b;
								else
									s.Fields.Add(lightKey, new UniValue(UniversalType.Integer, (int) b));
							}
							else 
							{
								UniFields.SetInteger(s.Fields, lightKey, (int) b - s.Brightness, 0);
							}

							s.UpdateNeeded = true;
							index++;
						}
					} 
					else if(mode == MenusForm.BrightnessGradientModes.Fade) 
					{
						ApplyColorGradient(orderedselection, start, end, interpolationmode, "fadecolor", 0);
					} 
					else if(mode == MenusForm.BrightnessGradientModes.Light) 
					{
						ApplyColorGradient(orderedselection, start, end, interpolationmode, "lightcolor", 0xFFFFFF);
					} 
					else if(mode == MenusForm.BrightnessGradientModes.LightAndFade) 
					{
						ApplyColorGradient(orderedselection, start, end, interpolationmode, "fadecolor", 0);
						ApplyColorGradient(orderedselection, start, end, interpolationmode, "lightcolor", 0xFFFFFF);
					}
				}
				else 
				{
					// Go for all sectors in between first and last
					int index = 0;
					foreach(Sector s in orderedselection) 
					{
						float u = index / (float)(orderedselection.Count - 1);
						s.Brightness = (int)Math.Round(InterpolationTools.Interpolate(start.Brightness, end.Brightness, u, interpolationmode)); //mxd
						index++;
					}
				}

				// Update
				General.Map.Map.Update();
				General.Interface.RedrawDisplay();
				General.Interface.RefreshInfo();
				General.Map.IsChanged = true;
			} 
			else 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Select at least 3 sectors first!");
			}
		}

		//mxd
		private static void ApplyColorGradient(ICollection<Sector> orderedselection, Sector start, Sector end, InterpolationTools.Mode interpolationmode, string key, int defaultvalue) 
		{
			if(!start.Fields.ContainsKey(key) && !end.Fields.ContainsKey(key)) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "First or last selected sector must have the \"" + key + "\" property!");
			} 
			else 
			{
				PixelColor startcolor, endcolor;
				if(key == "fadecolor")
				{
					startcolor = Tools.GetSectorFadeColor(start);
					endcolor = Tools.GetSectorFadeColor(end);
				}
				else
				{
					startcolor = PixelColor.FromInt(start.Fields.GetValue(key, defaultvalue));
					endcolor = PixelColor.FromInt(end.Fields.GetValue(key, defaultvalue));
				}

				// Go for all sectors in between first and last
				int index = 0;
				foreach(Sector s in orderedselection) 
				{
					if(index > 0 && index < orderedselection.Count - 1)
					{
						s.Fields.BeforeFieldsChange();
						float u = index / (orderedselection.Count - 1.0f);
						int c = InterpolationTools.InterpolateColor(startcolor, endcolor, u, interpolationmode).WithAlpha(0).ToInt(); // No alpha here!

						UniFields.SetInteger(s.Fields, key, c, defaultvalue);
						s.UpdateNeeded = true;
					}
					index++;
				}
			}
		}

		// Make gradient floors
		[BeginAction("gradientfloors")]
		public void MakeGradientFloors()
		{
			// Need at least 3 selected sectors
			// The first and last are not modified
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			if(orderedselection.Count > 2) 
			{
				General.Interface.DisplayStatus(StatusType.Action, "Created gradient floor heights over selected sectors.");
				General.Map.UndoRedo.CreateUndo("Gradient floor heights");

				int startlevel = General.GetByIndex(orderedselection, 0).FloorHeight;
				int endlevel = General.GetByIndex(orderedselection, orderedselection.Count - 1).FloorHeight;
				InterpolationTools.Mode interpolationmode = (InterpolationTools.Mode)BuilderPlug.Me.MenusForm.GradientInterpolationMenu.SelectedIndex; //mxd

				// Go for all sectors in between first and last
				int index = 0;
				foreach(Sector s in orderedselection) 
				{
					float u = index / (float)(orderedselection.Count - 1);
					s.FloorHeight = (int)Math.Round(InterpolationTools.Interpolate(startlevel, endlevel, u, interpolationmode)); //mxd
					index++;
				}

				// Update
				General.Interface.RefreshInfo();
				General.Map.IsChanged = true;
			} 
			else 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Select at least 3 sectors first!");
			}
		}

		// Make gradient ceilings
		[BeginAction("gradientceilings")]
		public void MakeGradientCeilings()
		{
			// Need at least 3 selected sectors
			// The first and last are not modified
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			if(orderedselection.Count > 2) 
			{
				General.Interface.DisplayStatus(StatusType.Action, "Created gradient ceiling heights over selected sectors.");
				General.Map.UndoRedo.CreateUndo("Gradient ceiling heights");

				int startlevel = General.GetByIndex(orderedselection, 0).CeilHeight;
				int endlevel = General.GetByIndex(orderedselection, orderedselection.Count - 1).CeilHeight;
				InterpolationTools.Mode interpolationmode = (InterpolationTools.Mode)BuilderPlug.Me.MenusForm.GradientInterpolationMenu.SelectedIndex; //mxd

				// Go for all sectors in between first and last
				int index = 0;
				foreach(Sector s in orderedselection) 
				{
					float u = (float)index / (orderedselection.Count - 1);
					s.CeilHeight = (int)Math.Round(InterpolationTools.Interpolate(startlevel, endlevel, u, interpolationmode));
					index++;
				}

				// Update
				General.Interface.RefreshInfo();
				General.Map.IsChanged = true;
			} 
			else 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Select at least 3 sectors first!");
			}
		}

		// Change heights
		[BeginAction("lowerfloor8")]
		public void LowerFloors8() 
		{
			// Get selection
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
			if(selected.Count == 0 && highlighted != null && !highlighted.IsDisposed)
				selected.Add(highlighted);

			if(selected.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			// Make undo
			General.Interface.DisplayStatus(StatusType.Action, "Lowered floor heights by 8mp.");
			General.Map.UndoRedo.CreateUndo("Floor heights change", this, UndoGroup.FloorHeightChange, CreateSelectionCRC(selected));

			// Change heights
			foreach(Sector s in selected) s.FloorHeight -= 8;

			// Update
			General.Interface.RefreshInfo();
			General.Map.IsChanged = true;
		}

		// Change heights
		[BeginAction("raisefloor8")]
		public void RaiseFloors8() 
		{
			// Get selection
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
			if(selected.Count == 0 && highlighted != null && !highlighted.IsDisposed)
				selected.Add(highlighted);

			if(selected.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			// Make undo
			General.Interface.DisplayStatus(StatusType.Action, "Raised floor heights by 8mp.");
			General.Map.UndoRedo.CreateUndo("Floor heights change", this, UndoGroup.FloorHeightChange, CreateSelectionCRC(selected));

			// Change heights
			foreach(Sector s in selected) s.FloorHeight += 8;

			// Update
			General.Interface.RefreshInfo();
			General.Map.IsChanged = true;
		}

		// Change heights
		[BeginAction("lowerceiling8")]
		public void LowerCeilings8() 
		{
			// Get selection
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
			if((selected.Count == 0) && (highlighted != null) && !highlighted.IsDisposed)
				selected.Add(highlighted);

			if(selected.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			// Make undo
			General.Interface.DisplayStatus(StatusType.Action, "Lowered ceiling heights by 8mp.");
			General.Map.UndoRedo.CreateUndo("Ceiling heights change", this, UndoGroup.CeilingHeightChange, CreateSelectionCRC(selected));

			// Change heights
			foreach(Sector s in selected) s.CeilHeight -= 8;

			// Update
			General.Interface.RefreshInfo();
			General.Map.IsChanged = true;
		}

		// Change heights
		[BeginAction("raiseceiling8")]
		public void RaiseCeilings8() 
		{
			// Get selection
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
			if(selected.Count == 0 && highlighted != null && !highlighted.IsDisposed)
				selected.Add(highlighted);

			if(selected.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			// Make undo
			General.Interface.DisplayStatus(StatusType.Action, "Raised ceiling heights by 8mp.");
			General.Map.UndoRedo.CreateUndo("Ceiling heights change", this, UndoGroup.CeilingHeightChange, CreateSelectionCRC(selected));

			// Change heights
			foreach(Sector s in selected) s.CeilHeight += 8;

			// Update
			General.Interface.RefreshInfo();
			General.Map.IsChanged = true;
		}

		//mxd. Raise brightness
		[BeginAction("raisebrightness8")]
		public void RaiseBrightness8() 
		{
			// Get selection
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
			if((selected.Count == 0) && (highlighted != null) && !highlighted.IsDisposed) 
				selected.Add(highlighted);

			if(selected.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			// Make undo
			Sector first = General.GetByIndex(selected, 0); //mxd
			int diff = General.Map.Config.BrightnessLevels.GetNextHigher(first.Brightness) - first.Brightness; //mxd
			General.Interface.DisplayStatus(StatusType.Action, "Raised sector brightness by " + diff + ".");
			General.Map.UndoRedo.CreateUndo("Sector brightness change", this, UndoGroup.SectorBrightnessChange, CreateSelectionCRC(selected));

			// Change heights
			foreach(Sector s in selected) 
			{
				s.Brightness = General.Map.Config.BrightnessLevels.GetNextHigher(s.Brightness);
				s.UpdateNeeded = true;
				s.UpdateCache();
			}

			// Update
			General.Interface.RefreshInfo();
			General.Map.IsChanged = true;

			// Redraw
			General.Interface.RedrawDisplay();
		}

		//mxd. Lower brightness
		[BeginAction("lowerbrightness8")]
		public void LowerBrightness8() 
		{
			// Get selection
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
			if(selected.Count == 0 && highlighted != null && !highlighted.IsDisposed)
				selected.Add(highlighted);

			if(selected.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			// Make undo
			Sector first = General.GetByIndex(selected, 0); //mxd
			int diff = first.Brightness - General.Map.Config.BrightnessLevels.GetNextLower(first.Brightness); //mxd
			General.Interface.DisplayStatus(StatusType.Action, "Lowered sector brightness by " + diff + ".");
			General.Map.UndoRedo.CreateUndo("Sector brightness change", this, UndoGroup.SectorBrightnessChange, CreateSelectionCRC(selected));

			// Change heights
			foreach(Sector s in selected) 
			{
				s.Brightness = General.Map.Config.BrightnessLevels.GetNextLower(s.Brightness);
				s.UpdateNeeded = true;
				s.UpdateCache();
			}

			// Update
			General.Interface.RefreshInfo();
			General.Map.IsChanged = true;

			// Redraw
			General.Interface.RedrawDisplay();
		}

		// This clears the selection
		[BeginAction("clearselection", BaseAction = true)]
		public void ClearSelection()
		{
			// Clear selection
			General.Map.Map.ClearAllSelected();

			//mxd. Clear selection info
			General.Interface.DisplayStatus(StatusType.Selection, string.Empty); 

			// Clear labels
			foreach(TextLabel[] labelarray in labels.Values)
				foreach(TextLabel l in labelarray) l.Text = "";
			UpdateOverlaySurfaces(); //mxd
			UpdateEffectLabels(); //mxd
			
			// Redraw
			General.Interface.RedrawDisplay();
		}

		[BeginAction("placethings")] //mxd
		public void PlaceThings() 
		{
			// Make list of selected sectors
			ICollection<Sector> sectors = General.Map.Map.GetSelectedSectors(true);
			List<Vector2D> positions = new List<Vector2D>();
			
			if(sectors.Count == 0) 
			{
				if(highlighted != null && !highlighted.IsDisposed) 
				{
					sectors.Add(highlighted);
				} 
				else 
				{
					General.Interface.DisplayStatus(StatusType.Warning, "This action requires selection of some description!");
					return;
				}
			}

			// Make list of suitable positions
			foreach(Sector s in sectors) 
			{
				Vector2D pos = (s.Labels.Count > 0 ? s.Labels[0].position : new Vector2D(s.BBox.X + s.BBox.Width / 2, s.BBox.Y + s.BBox.Height / 2));
				if(!positions.Contains(pos)) positions.Add(pos);
			}

			if(positions.Count < 1) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Unable to get vertex positions from selection!");
				return;
			}

			PlaceThingsAtPositions(positions);
		}

		//mxd. rotate clockwise
		[BeginAction("rotateclockwise")]
		public void RotateCW() 
		{
			RotateTextures(5);
		}

		//mxd. rotate counterclockwise
		[BeginAction("rotatecounterclockwise")]
		public void RotateCCW() 
		{
			RotateTextures(-5);
		}

		//mxd
		private void RotateTextures(float increment) 
		{
			if(!General.Map.UDMF) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action works only in UDMF map format!");
				return;
			}

			// Make list of selected sectors
			ICollection<Sector> sectors = General.Map.Map.GetSelectedSectors(true);

			if(sectors.Count == 0 && highlighted != null && !highlighted.IsDisposed)
				sectors.Add(highlighted);

			if(sectors.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			string targets;
			string target;
			switch(General.Map.Renderer2D.ViewMode) 
			{
				case ViewMode.FloorTextures:
					target = " a floor";
					targets = " floors";
					break;

				case ViewMode.CeilingTextures:
					target = " a ceiling";
					targets = " ceilings";
					break;

				default:
					target = " a floor and a ceiling";
					targets = " floors and ceilings";
					break;
			}

			// Make undo
			if(sectors.Count > 1) 
			{
				General.Map.UndoRedo.CreateUndo("Rotate " + sectors.Count + targets, this, UndoGroup.TextureRotationChange, CreateSelectionCRC(sectors));
				General.Interface.DisplayStatus(StatusType.Action, "Rotated " + sectors.Count + targets + ".");
			} 
			else 
			{
				General.Map.UndoRedo.CreateUndo("Rotate" + target, this, UndoGroup.TextureRotationChange, CreateSelectionCRC(sectors));
				General.Interface.DisplayStatus(StatusType.Action, "Rotated" + target + ".");
			}

			//rotate stuff
			foreach(Sector s in sectors) 
			{
				s.Fields.BeforeFieldsChange();

				//floor
				if(General.Map.Renderer2D.ViewMode != ViewMode.CeilingTextures)
				{
					UniFields.SetFloat(s.Fields, "rotationfloor", General.ClampAngle(UniFields.GetFloat(s.Fields, "rotationfloor") + increment));
					s.UpdateNeeded = true;
				}

				//ceiling
				if(General.Map.Renderer2D.ViewMode != ViewMode.FloorTextures) 
				{
					UniFields.SetFloat(s.Fields, "rotationceiling", General.ClampAngle(UniFields.GetFloat(s.Fields, "rotationceiling") + increment));
					s.UpdateNeeded = true;
				}
			}

			// Redraw screen
			General.Map.Map.Update();
			General.Interface.RedrawDisplay();
			General.Interface.RefreshInfo();
		}

		//mxd
		[BeginAction("selectsimilar")]
		public void SelectSimilar() 
		{
			ICollection<Sector> selection = General.Map.Map.GetSelectedSectors(true);

			if(selection.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			var form = new SelectSimilarElementOptionsPanel();
			if(form.Setup(this)) form.ShowDialog(General.Interface);
		}
		
		//mxd
		[BeginAction("fliplinedefs")]
		public void FlipLinedefs() 
		{
			// Get selection
			ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);

			if(selected.Count == 0 && highlighted != null && !highlighted.IsDisposed)
				selected.Add(highlighted);

			if(selected.Count == 0)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			// Make undo
			if(selected.Count > 1)
			{
				General.Map.UndoRedo.CreateUndo("Flip linedefs of " + selected.Count + " sectors");
				General.Interface.DisplayStatus(StatusType.Action, "Flipped linedefs of " + selected.Count + "sectors.");
			}
			else
			{
				General.Map.UndoRedo.CreateUndo("Flip sector linedefs");
				General.Interface.DisplayStatus(StatusType.Action, "Flipped sector linedefs.");
			}

			HashSet<Linedef> selectedlines = new HashSet<Linedef>();
			foreach(Sector s in selected)
			{
				foreach(Sidedef side in s.Sidedefs)
				{
					// Skip single-sided lines with only front side
					if(!selectedlines.Contains(side.Line) && (side.Line.Back != null || side.Line.Front == null))
						selectedlines.Add(side.Line);
				}
			}

			// Flip all selected linedefs
			foreach(Linedef l in selectedlines)
			{
				l.FlipVertices();
				l.FlipSidedefs();
			}

			// Redraw
			General.Map.Map.Update();
			General.Map.IsChanged = true;
			General.Interface.RefreshInfo();
			General.Interface.RedrawDisplay();
		}
		
		//mxd
		[BeginAction("alignlinedefs")]
		public void AlignLinedefs() 
		{
			// Get selection
			ICollection<Sector> selection = General.Map.Map.GetSelectedSectors(true);

			if(selection.Count == 0 && highlighted != null && !highlighted.IsDisposed)
				selection.Add(highlighted);

			if(selection.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!");
				return;
			}

			// Make undo
			if(selection.Count > 1) 
			{
				General.Map.UndoRedo.CreateUndo("Align linedefs of " + selection.Count + " sectors");
				General.Interface.DisplayStatus(StatusType.Action, "Aligned linedefs of " + selection.Count + "sectors.");
			} 
			else 
			{
				General.Map.UndoRedo.CreateUndo("Align sector linedefs");
				General.Interface.DisplayStatus(StatusType.Action, "Aligned sector linedefs.");
			}

			// Flip lines
			Tools.FlipSectorLinedefs(selection, false);
			
			// Redraw
			General.Map.Map.Update();
			General.Map.IsChanged = true;
			General.Interface.RefreshInfo();
			General.Interface.RedrawDisplay();
		}

		#endregion
	}
}
