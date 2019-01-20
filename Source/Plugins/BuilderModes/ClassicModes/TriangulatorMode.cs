
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
using System.Threading;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes.Editing
{
	#if DEBUG
	
	[EditMode(DisplayName = "Triangulator Mode",
			  SwitchAction = "triangulatormode",		// Action name used to switch to this mode
			  ButtonImage = "TriangulatorMode.png",		// Image resource name for the button
			  ButtonOrder = int.MaxValue)]				// Position of the button (lower is more to the left)

	public class TriangulatorMode : ClassicMode
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Highlighted item
		private Sector highlighted;
		
		// Labels
		private ICollection<LabelPositionInfo> labelpos;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public TriangulatorMode()
		{
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up

				// Dispose base
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

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
		}

		// Mode disengages
		public override void OnDisengage()
		{
			base.OnDisengage();

			// Check which mode we are switching to
			if(General.Editing.NewMode is VerticesMode)
			{
				// Convert selection to vertices

				// Clear selected sectors
				General.Map.Map.ClearSelectedSectors();
			}
			else if(General.Editing.NewMode is LinedefsMode)
			{
				// Convert selection to linedefs

				// Clear selected sectors
				General.Map.Map.ClearSelectedSectors();
			}

			// Hide highlight info
			General.Interface.HideInfo();
		}

		// This redraws the display
		public unsafe override void OnRedrawDisplay()
		{
			// Render lines and vertices
			if(renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);
				if((highlighted != null) && !highlighted.IsDisposed)
					renderer.PlotSector(highlighted, General.Colors.Highlight);
				renderer.Finish();
			}
			
			// Render labels
			/*
			if(renderer.StartOverlay(true))
			{
				if(labelpos != null)
				{
					foreach(LabelPositionInfo lb in labelpos)
						renderer.RenderRectangleFilled(new RectangleF(lb.position.x - lb.radius / 2, lb.position.y - lb.radius / 2, lb.radius, lb.radius), General.Colors.Indication, true);
				}

				renderer.Finish();
			}
			*/
			
			// Do not show things
			if(renderer.StartThings(true))
			{
				renderer.Finish();
			}

			renderer.RedrawSurface();
			
			renderer.Present();
		}

		// This highlights a new item
		protected void Highlight(Sector s)
		{
			// Redraw lines
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
			
			// Render labels
			/*
			if(renderer.StartOverlay(true))
			{
				if(labelpos != null)
				{
					foreach(LabelPositionInfo lb in labelpos)
						renderer.RenderRectangleFilled(new RectangleF(lb.position.x - lb.radius / 2, lb.position.y - lb.radius / 2, lb.radius, lb.radius), General.Colors.Indication, true);
				}
				
				renderer.Finish();
			}
			*/
			
			renderer.Present();
			
			// Show highlight info
			if((highlighted != null) && !highlighted.IsDisposed)
				General.Interface.ShowSectorInfo(highlighted);
			else
				General.Interface.HideInfo();
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			// Find the nearest linedef within highlight range
			Linedef l = General.Map.Map.NearestLinedef(mousemappos);

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

		// Mouse leaves
		public override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			// Highlight nothing
			Highlight(null);
		}

		// Mouse button pressed
		public override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			bool front, back;

			// Edit button is used?
			if(General.Actions.CheckActionActive(null, "classicedit"))
			{
				// Item highlighted?
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					// Make update lines selection
					foreach(Sidedef sd in highlighted.Sidedefs)
					{
						if(sd.Line.Front != null) front = sd.Line.Front.Sector.Selected; else front = false;
						if(sd.Line.Back != null) back = sd.Line.Back.Sector.Selected; else back = false;
						sd.Line.Selected = front | back;
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
			}
		}

		// Mouse released
		public override void OnMouseUp(MouseEventArgs e)
		{
			ICollection<Sector> selected;
			PixelColor c;
			
			base.OnMouseUp(e);
			
			// Item highlighted?
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				Sector s = highlighted;
				
				// Remove highlight to avoid confusion
				Highlight(null);
				
				// Get a triangulator and bind events
				Triangulation t = new Triangulation();
				t.OnShowPolygon = ShowPolygon;
				t.OnShowEarClip = ShowEarClip;
				//t.OnShowRemaining = ShowRemaining;
				t.Triangulate(s);
				
				// Start with a clear display
				if(renderer.StartPlotter(true))
				{
					// Render lines and vertices
					renderer.PlotLinedefSet(General.Map.Map.Linedefs);
					renderer.PlotVerticesSet(General.Map.Map.Vertices);
					
					// Go for all triangle vertices to render the inside lines only
					for(int i = 0; i < t.Vertices.Count; i += 3)
					{
						if(t.Sidedefs[i + 0] == null) renderer.PlotLine(t.Vertices[i + 0], t.Vertices[i + 1], PixelColor.FromColor(Color.DeepSkyBlue));
						if(t.Sidedefs[i + 1] == null) renderer.PlotLine(t.Vertices[i + 1], t.Vertices[i + 2], PixelColor.FromColor(Color.DeepSkyBlue));
						if(t.Sidedefs[i + 2] == null) renderer.PlotLine(t.Vertices[i + 2], t.Vertices[i + 0], PixelColor.FromColor(Color.DeepSkyBlue));
					}
					
					// Go for all triangle vertices to renderthe outside lines only
					for(int i = 0; i < t.Vertices.Count; i += 3)
					{
						if(t.Sidedefs[i + 0] != null) renderer.PlotLine(t.Vertices[i + 0], t.Vertices[i + 1], PixelColor.FromColor(Color.Red));
						if(t.Sidedefs[i + 1] != null) renderer.PlotLine(t.Vertices[i + 1], t.Vertices[i + 2], PixelColor.FromColor(Color.Red));
						if(t.Sidedefs[i + 2] != null) renderer.PlotLine(t.Vertices[i + 2], t.Vertices[i + 0], PixelColor.FromColor(Color.Red));
					}
					
					// Done
					renderer.Finish();
					renderer.Present();
					Thread.Sleep(200);
				}
			}
		}

		// This shows a point
		private void ShowPoint(Vector2D v, int c)
		{
			for(int a = 0; a < 6; a++)
			{
				OnRedrawDisplay();
				Thread.Sleep(20);
				Application.DoEvents();
				
				// Start with a clear display
				if(renderer.StartPlotter(true))
				{
					// Render lines and vertices
					renderer.PlotLinedefSet(General.Map.Map.Linedefs);
					renderer.PlotVerticesSet(General.Map.Map.Vertices);

					// Show the point
					renderer.PlotVertexAt(v, c);

					// Done
					renderer.Finish();
					renderer.Present();
				}

				// Wait a bit
				Thread.Sleep(60);
			}
		}

		// This shows a line
		private void ShowLine(Vector2D v1, Vector2D v2, PixelColor c)
		{
			for(int a = 0; a < 3; a++)
			{
				OnRedrawDisplay();
				Thread.Sleep(20);
				Application.DoEvents();
				
				// Start with a clear display
				if(renderer.StartPlotter(true))
				{
					// Render lines and vertices
					renderer.PlotLinedefSet(General.Map.Map.Linedefs);
					renderer.PlotVerticesSet(General.Map.Map.Vertices);

					// Show the line
					renderer.PlotLine(v1, v2, c);

					// Done
					renderer.Finish();
					renderer.Present();
				}

				// Wait a bit
				Thread.Sleep(50);
				Application.DoEvents();
			}
		}

		// This shows a polygon
		private void ShowPolygon(LinkedList<EarClipVertex> p)
		{
			LinkedListNode<EarClipVertex> v = p.First;
			LinkedListNode<EarClipVertex> v2 = p.Last;
			
			// Go for all vertices in the polygon
			while(v != null)
			{
				for(int a = 0; a < 1; a++)
				{

					// Start with a clear display
					if(renderer.StartPlotter(true))
					{
						// Render lines and vertices
						renderer.PlotLinedefSet(General.Map.Map.Linedefs);
						renderer.PlotVerticesSet(General.Map.Map.Vertices);

						// Show line
						renderer.PlotLine(v.Value.Position, v2.Value.Position, PixelColor.FromColor(Color.White));

						// Done
						renderer.Finish();
						renderer.Present();
					}
					
					//Thread.Sleep(10);
					
					// Start with a clear display
					if(renderer.StartPlotter(true))
					{
						// Render lines and vertices
						renderer.PlotLinedefSet(General.Map.Map.Linedefs);
						renderer.PlotVerticesSet(General.Map.Map.Vertices);
						
						// Show line
						renderer.PlotLine(v.Value.Position, v2.Value.Position, PixelColor.FromColor(Color.Red));
						
						// Done
						renderer.Finish();
						renderer.Present();
					}

					// Wait a bit
					//Thread.Sleep(40);
					Application.DoEvents();
				}
				
				v2 = v;
				v = v.Next;
			}
		}

		// This shows a polygon
		private void ShowRemaining(LinkedList<EarClipVertex> remains)
		{
			LinkedListNode<EarClipVertex> v;

			for(int a = 0; a < 100; a++)
			{
				OnRedrawDisplay();
				Thread.Sleep(10);
				Application.DoEvents();
				
				// Start with a clear display
				if(renderer.StartPlotter(true))
				{
					// Render lines and vertices
					renderer.PlotLinedefSet(General.Map.Map.Linedefs);
					renderer.PlotVerticesSet(General.Map.Map.Vertices);

					// Go for all vertices in the polygon
					v = remains.First;
					while(v != null)
					{
						// Show the line
						if(v.Next != null) renderer.PlotLine(v.Value.Position, v.Next.Value.Position, PixelColor.FromColor(Color.Yellow));
						v = v.Next;
					}

					// Show last line as well
					renderer.PlotLine(remains.Last.Value.Position, remains.First.Value.Position, PixelColor.FromColor(Color.Yellow));

					// Done
					renderer.Finish();
					renderer.Present();
				}

				// Wait a bit
				Thread.Sleep(60);
			}
		}

		// This shows a polygon
		private void ShowEarClip(EarClipVertex[] found, LinkedList<EarClipVertex> remains)
		{
			EarClipVertex prev, first;
			
			for(int a = 0; a < 2; a++)
			{
				// Start with a clear display
				if(renderer.StartPlotter(true))
				{
					// Render lines and vertices
					renderer.PlotLinedefSet(General.Map.Map.Linedefs);
					renderer.PlotVerticesSet(General.Map.Map.Vertices);

					// Go for all remaining vertices
					prev = null; first = null;
					foreach(EarClipVertex v in remains)
					{
						// Show the line
						if(prev != null) renderer.PlotLine(v.Position, prev.Position, PixelColor.FromColor(Color.Yellow));
						if(prev == null) first = v;
						prev = v;
						
						if(v.IsReflex)
							renderer.PlotVertexAt(v.Position, ColorCollection.SELECTION);
						else
							renderer.PlotVertexAt(v.Position, ColorCollection.VERTICES);
					}
					if(first != null) renderer.PlotLine(first.Position, prev.Position, PixelColor.FromColor(Color.Yellow));

					if(found != null)
					{
						renderer.PlotLine(found[0].Position, found[1].Position, PixelColor.FromColor(Color.BlueViolet));
						renderer.PlotLine(found[1].Position, found[2].Position, PixelColor.FromColor(Color.BlueViolet));
						renderer.PlotLine(found[2].Position, found[0].Position, PixelColor.FromColor(Color.BlueViolet));
						renderer.PlotVertexAt(found[1].Position, ColorCollection.INDICATION);
					}
					
					// Done
					renderer.Finish();
					renderer.Present();
				}
				Thread.Sleep(10);
				Application.DoEvents();

				// Start with a clear display
				if(renderer.StartPlotter(true))
				{
					// Render lines and vertices
					renderer.PlotLinedefSet(General.Map.Map.Linedefs);
					renderer.PlotVerticesSet(General.Map.Map.Vertices);

					// Go for all remaining vertices
					prev = null; first = null;
					foreach(EarClipVertex v in remains)
					{
						// Show the line
						if(prev != null) renderer.PlotLine(v.Position, prev.Position, PixelColor.FromColor(Color.Yellow));
						if(prev == null) first = v;
						prev = v;

						if(v.IsReflex)
							renderer.PlotVertexAt(v.Position, ColorCollection.SELECTION);
						else
							renderer.PlotVertexAt(v.Position, ColorCollection.VERTICES);
					}
					if(first != null) renderer.PlotLine(first.Position, prev.Position, PixelColor.FromColor(Color.Yellow));

					// Done
					renderer.Finish();
					renderer.Present();
				}
				//Thread.Sleep(10);
			}
		}
		
		#endregion
	}

	#endif
}
