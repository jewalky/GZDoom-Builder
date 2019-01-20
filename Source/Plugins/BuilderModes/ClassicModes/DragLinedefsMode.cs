
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
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	// No action or button for this mode, it is automatic.
	// The EditMode attribute does not have to be specified unless the
	// mode must be activated by class name rather than direct instance.
	// In that case, just specifying the attribute like this is enough:
	// [EditMode]

	[EditMode(DisplayName = "Linedefs",
			  AllowCopyPaste = false,
			  Volatile = true)]
	
	public sealed class DragLinedefsMode : DragGeometryMode
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private ICollection<Linedef> selectedlines;
		private ICollection<Linedef> unselectedlines;

		#endregion

		#region ================== Properties
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor to start dragging immediately
		public DragLinedefsMode(Vector2D dragstartmappos)
		{
			// Mark what we are dragging
			General.Map.Map.ClearAllMarks(false);
			General.Map.Map.MarkSelectedLinedefs(true, true);
			ICollection<Vertex> verts = General.Map.Map.GetVerticesFromLinesMarks(true);
			foreach(Vertex v in verts) v.Marked = true;
			
			// Get line collections
			selectedlines = General.Map.Map.GetSelectedLinedefs(true);
			unselectedlines = General.Map.Map.GetSelectedLinedefs(false);

			// Initialize
			base.StartDrag(dragstartmappos);
			undodescription = (selectedlines.Count == 1 ? "Drag linedef" : "Drag " + selectedlines.Count + " linedefs"); //mxd
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up

				// Done
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods
		
		// Disenagaging
		public override void OnDisengage()
		{
			// Select vertices from lines selection
			General.Map.Map.ClearSelectedVertices();
			ICollection<Vertex> verts = General.Map.Map.GetVerticesFromLinesMarks(true);
			foreach(Vertex v in verts) v.Selected = true;

			// Perform normal disengage
			base.OnDisengage();

			// Clear vertex selection
			General.Map.Map.ClearSelectedVertices();
			
			// When not cancelled
			if(!cancelled)
			{
				// If only a single linedef was selected, deselect it now
				if(selectedlines.Count == 1) General.Map.Map.ClearSelectedLinedefs();
			}
		}

		// This redraws the display
		public override void OnRedrawDisplay()
		{
			renderer.RedrawSurface();

			UpdateRedraw();

			if(CheckViewChanged())
			{
				// Start rendering things
				if(renderer.StartThings(true))
				{
					renderer.RenderThingSet(General.Map.Map.Things, General.Settings.ActiveThingsAlpha);
					renderer.Finish();
				}
			}

			renderer.Present();
		}

		// This redraws only the required things
		protected override void UpdateRedraw()
		{
			// Start rendering structure
			if(renderer.StartPlotter(true))
			{
				// Render lines and vertices
				renderer.PlotLinedefSet(unselectedlines);
				renderer.PlotLinedefSet(selectedlines);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);

				// Draw the dragged item highlighted
				// This is important to know, because this item is used
				// for snapping to the grid and snapping to nearest items
				renderer.PlotVertex(dragitem, ColorCollection.HIGHLIGHT);

				// Done
				renderer.Finish();
			}

			//mxd. Render things
			if(renderer.StartThings(true)) 
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, General.Settings.HiddenThingsAlpha);
				renderer.RenderThingSet(unselectedthings, General.Settings.ActiveThingsAlpha);
				renderer.RenderThingSet(selectedthings, General.Settings.ActiveThingsAlpha);
				renderer.Finish();
			}

			// Redraw overlay
			if(renderer.StartOverlay(true))
			{
				renderer.RenderText(labels);
				renderer.Finish();
			}
		}
		
		#endregion
	}
}
