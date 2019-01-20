
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
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public abstract class FlatAlignMode : BaseClassicMode
	{
		#region ================== Enums and Structs

		private enum ModifyMode
		{
			None,
			Dragging,
			Resizing,
			Rotating
		}

		private enum Grip
		{
			None,
			Main,
			SizeV,
			SizeH,
			RotateRT,
			RotateLB
		}

		protected struct SectorInfo
		{
			public float rotation;
			public Vector2D scale;
			public Vector2D offset;
		}

		#endregion

		#region ================== Constants

		private const float GRIP_SIZE = 9.0f;
		private readonly Cursor[] RESIZE_CURSORS = { Cursors.SizeNS, Cursors.SizeNWSE, Cursors.SizeWE, Cursors.SizeNESW };
		private const byte RECTANGLE_ALPHA = 60;
		private const byte EXTENSION_LINE_ALPHA = 150;
		
		#endregion

		#region ================== Variables

		private ICollection<Sector> selection;
		private Sector editsector;
		protected IList<SectorInfo> sectorinfo;
		private ImageData texture;
		//private Vector2D selectionoffset;
		private ModifyMode mode;
		private bool autopanning;
		private bool modealreadyswitching;
		
		// Modification
		// NOTE: This offset is in world space. ZDoom's offset is done before
		// rotation (not my idea) so we will transform this when applying
		// changes to sectors.
		private float rotation;
		private Vector2D scale = new Vector2D(1.0f, 1.0f);
		private Vector2D offset;

		// Rectangle components
		private readonly Vector2D[] corners = new Vector2D[4]; // lefttop, righttop, rightbottom, leftbottom
		private FlatVertex[] cornerverts = new FlatVertex[6];
		private readonly Vector2D[] extends = new Vector2D[2]; // right, bottom
		private readonly RectangleF[] resizegrips = new RectangleF[2];	// right, bottom
		private readonly RectangleF[] rotategrips = new RectangleF[2];   // righttop, leftbottom
		private Line2D extensionline;
		
		// Aligning
		private RectangleF alignrect;
		private Vector2D alignoffset;
		private bool showalignoffset;
		private Vector2D dragoffset;
		private Vector2D resizevector;
		private Vector2D resizefilter;
		private Line2D resizeaxis;
		private float rotationoffset;
		private Vector2D rotationcenter;

		//mxd. View mode
		private ViewMode prevviewmode;

		// Options
		private bool snaptogrid;		// SHIFT to toggle
		private bool snaptonearest;		// CTRL to enable
		
		#endregion

		#region ================== Properties

		protected abstract string XScaleName { get; }
		protected abstract string YScaleName { get; }
		protected abstract string XOffsetName { get; }
		protected abstract string YOffsetName { get; }
		protected abstract string RotationName { get; }
		protected abstract string UndoDescription { get; }

		#endregion

		#region ================== Methods

		protected abstract ImageData GetTexture(Sector editsector);

		// This checks if a point is in a rect
		private static bool PointInRectF(RectangleF rect, Vector2D point)
		{
			return (point.x >= rect.Left) && (point.x <= rect.Right) && (point.y >= rect.Top) && (point.y <= rect.Bottom);
		}

		// Transforms p from Texture space into World space
		private Vector2D TexToWorld(Vector2D p)
		{
			return TexToWorld(p, sectorinfo[0]);
		}
		
		// Transforms p from Texture space into World space
		protected Vector2D TexToWorld(Vector2D p, SectorInfo s)
		{
			p /= scale * s.scale;
			Vector2D soffset = s.offset.GetRotated(rotation);
			p -= soffset;
			p = p.GetRotated(-(rotation + s.rotation));
			p -= offset;
			return p;
		}

		// Transforms p from World space into Texture space
		private Vector2D WorldToTex(Vector2D p)
		{
			return WorldToTex(p, sectorinfo[0]);
		}
		
		// Transforms p from World space into Texture space
		protected Vector2D WorldToTex(Vector2D p, SectorInfo s)
		{
			p += offset;
			p = p.GetRotated(rotation + s.rotation);
			p += s.offset;
			p *= scale * s.scale;
			return p;
		}

		// This updates all sectors
		private void UpdateSectors()
		{
			int index = 0;
			foreach(Sector s in selection)
			{
				SectorInfo si = sectorinfo[index];
				s.Fields.BeforeFieldsChange();
				Vector2D toffset = offset.GetRotated((rotation + si.rotation));
				Vector2D soffset = si.offset.GetRotated(rotation);
				s.Fields[RotationName] = new UniValue(UniversalType.AngleDegreesFloat, Angle2D.RadToDeg(si.rotation + rotation));
				s.Fields[XScaleName] = new UniValue(UniversalType.Float, si.scale.x * scale.x);
				s.Fields[YScaleName] = new UniValue(UniversalType.Float, si.scale.y * scale.y);
				s.Fields[XOffsetName] = new UniValue(UniversalType.Float, soffset.x + toffset.x);
				s.Fields[YOffsetName] = new UniValue(UniversalType.Float, -(soffset.y + toffset.y));
				s.UpdateNeeded = true;
				s.UpdateCache();
				index++;
			}
		}

		// This restores all sectors to original values
		private void RestoreSectors()
		{
			int index = 0;
			foreach(Sector s in selection)
			{
				SectorInfo si = sectorinfo[index];
				s.Fields.BeforeFieldsChange();
				s.Fields[RotationName] = new UniValue(UniversalType.AngleDegreesFloat, Angle2D.RadToDeg(si.rotation));
				s.Fields[XScaleName] = new UniValue(UniversalType.Float, si.scale.x);
				s.Fields[YScaleName] = new UniValue(UniversalType.Float, si.scale.y);
				s.Fields[XOffsetName] = new UniValue(UniversalType.Float, si.offset.x);
				s.Fields[YOffsetName] = new UniValue(UniversalType.Float, -si.offset.y);
				s.UpdateNeeded = true;
				s.UpdateCache();
				index++;
			}
		}
		
		// This updates the selection
		private void Update()
		{
			// Not in any modifying mode?
			if(mode == ModifyMode.None)
			{
				Vector2D prevdragoffset = alignoffset;
				alignoffset = new Vector2D(float.MinValue, float.MinValue);
				showalignoffset = false;
				
				// Check what grip the mouse is over
				// and change cursor accordingly
				Grip mousegrip = CheckMouseGrip();
				switch(mousegrip)
				{
					case Grip.Main:
						int closestcorner = -1;
						float cornerdist = float.MaxValue;
						for(int i = 0; i < 4; i++)
						{
							Vector2D delta = corners[i] - mousemappos;
							float d = delta.GetLengthSq();
							if(d < cornerdist)
							{
								closestcorner = i;
								cornerdist = d;
							}
						}
						switch(closestcorner)
						{
							// TODO:
							case 0: alignoffset = new Vector2D(0f, 0f); break;
							case 1: alignoffset = new Vector2D(texture.ScaledWidth, 0f); break;
							case 2: alignoffset = new Vector2D(texture.ScaledWidth, -texture.ScaledHeight); break;
							case 3: alignoffset = new Vector2D(0f, -texture.ScaledHeight); break;
						}
						showalignoffset = true;
						General.Interface.SetCursor(Cursors.Hand);
						break;

					case Grip.RotateLB:
					case Grip.RotateRT:
						alignoffset = new Vector2D(0f, 0f);
						showalignoffset = true;
						General.Interface.SetCursor(Cursors.Cross);
						break;

					case Grip.SizeH:
					case Grip.SizeV:
						alignoffset = new Vector2D(0f, 0f);
						showalignoffset = true;
						// Pick the best matching cursor depending on rotation and side
						float resizeangle = -(rotation + sectorinfo[0].rotation);
						if(mousegrip == Grip.SizeH) resizeangle += Angle2D.PIHALF;
						resizeangle = Angle2D.Normalized(resizeangle);
						if(resizeangle > Angle2D.PI) resizeangle -= Angle2D.PI;
						resizeangle = Math.Abs(resizeangle + Angle2D.PI / 8.000001f);
						int cursorindex = (int)Math.Floor((resizeangle / Angle2D.PI) * 4.0f) % 4;
						General.Interface.SetCursor(RESIZE_CURSORS[cursorindex]);
						break;

					default:
						General.Interface.SetCursor(Cursors.Default);
						break;
				}

				if(prevdragoffset != alignoffset)
					General.Interface.RedrawDisplay();
			}
			else
			{
				Vector2D snappedmappos = mousemappos;
				bool dosnaptogrid = snaptogrid;
				
				// Options
				snaptogrid = General.Interface.ShiftState ^ General.Interface.SnapToGrid;
				snaptonearest = General.Interface.CtrlState ^ General.Interface.AutoMerge;

				// Change to crosshair cursor so we can clearly see around the mouse cursor
				General.Interface.SetCursor(Cursors.Cross);
				
				// Check what modifying mode we are in
				switch(mode)
				{
					case ModifyMode.Dragging:
						
						offset = -mousemappos - dragoffset;
						Vector2D transformedpos = TexToWorld(alignoffset);

						// Snap to nearest vertex?
						if(snaptonearest)
						{
							float vrange = BuilderPlug.Me.StitchRange / renderer.Scale;

							// Try the nearest vertex
							Vertex nv = MapSet.NearestVertexSquareRange(General.Map.Map.Vertices, transformedpos, vrange);
							if(nv != null)
							{
								// Change offset to snap to target
								offset -= nv.Position - transformedpos;
								dosnaptogrid = false;
							}
							else
							{
								// Find the nearest line within range
								Linedef nl = MapSet.NearestLinedefRange(General.Map.Map.Linedefs, transformedpos, vrange);
								if(nl != null)
								{
									// Snap to grid?
									if(dosnaptogrid)
									{
										// Get grid intersection coordinates
										List<Vector2D> coords = nl.GetGridIntersections();

										// Find nearest grid intersection
										float found_distance = float.MaxValue;
										Vector2D found_pos = new Vector2D(float.NaN, float.NaN);
										foreach(Vector2D v in coords)
										{
											Vector2D dist = transformedpos - v;
											if(dist.GetLengthSq() < found_distance)
											{
												// Found a better match
												found_distance = dist.GetLengthSq();
												found_pos = v;

												// Do not snap to grid anymore
												dosnaptogrid = false;
											}
										}

										// Found something?
										if(!float.IsNaN(found_pos.x))
										{
											// Change offset to snap to target
											offset -= found_pos - transformedpos;
										}
									}
									else
									{
										// Change offset to snap onto the line
										offset -= nl.NearestOnLine(transformedpos) - transformedpos;
									}
								}
							}
						}

						// Snap to grid?
						if(dosnaptogrid)
						{
							// Change offset to align to grid
							offset -= General.Map.Grid.SnappedToGrid(transformedpos) - transformedpos;
						}
						
						break;

					case ModifyMode.Resizing:
						
						// Snap to nearest vertex?
						if(snaptonearest)
						{
							float vrange = BuilderPlug.Me.StitchRange / renderer.Scale;

							// Try the nearest vertex
							Vertex nv = MapSet.NearestVertexSquareRange(General.Map.Map.Vertices, snappedmappos, vrange);
							if(nv != null)
							{
								snappedmappos = nv.Position;
								dosnaptogrid = false;
							}
						}

						// Snap to grid?
						if(dosnaptogrid)
						{
							// Aligned to grid
							snappedmappos = General.Map.Grid.SnappedToGrid(snappedmappos);
						}

						float newscale = 1f / resizeaxis.GetNearestOnLine(snappedmappos);
						if(float.IsInfinity(newscale) || float.IsNaN(newscale)) newscale = 99999f;
						scale = (newscale * resizefilter) + scale * (1.0f - resizefilter);
						if(float.IsInfinity(scale.x) || float.IsNaN(scale.x)) scale.x = 99999f;
						if(float.IsInfinity(scale.y) || float.IsNaN(scale.y)) scale.y = 99999f;

						// Show the extension line so that the user knows what it is aligning to
						UpdateRectangleComponents();
						Line2D edgeline;
						if(resizefilter.x > resizefilter.y)
							edgeline = new Line2D(corners[1], corners[2]);
						else
							edgeline = new Line2D(corners[3], corners[2]);
						float nearestonedge = edgeline.GetNearestOnLine(snappedmappos);
						if(nearestonedge > 0.5f)
							extensionline = new Line2D(edgeline.v1, snappedmappos);
						else
							extensionline = new Line2D(edgeline.v2, snappedmappos);

						break;

					case ModifyMode.Rotating:

						// Snap to nearest vertex?
						extensionline = new Line2D();
						if(snaptonearest)
						{
							float vrange = BuilderPlug.Me.StitchRange / renderer.Scale;

							// Try the nearest vertex
							Vertex nv = MapSet.NearestVertexSquareRange(General.Map.Map.Vertices, snappedmappos, vrange);
							if(nv != null)
							{
								snappedmappos = nv.Position;
								dosnaptogrid = false;

								// Show the extension line so that the user knows what it is aligning to
								extensionline = new Line2D(corners[0], snappedmappos);
							}
						}

						Vector2D delta = snappedmappos - rotationcenter;
						float deltaangle = -delta.GetAngle();
						
						// Snap to grid?
						if(dosnaptogrid)
						{
							// We make 24 vectors that the rotation can snap to
							float founddistance = float.MaxValue;
							float foundrotation = rotation;
							Vector3D rotvec = Vector2D.FromAngle(deltaangle + rotationoffset);

							for(int i = 0; i < 24; i++)
							{
								// Make the vectors
								float angle = i * Angle2D.PI * 0.08333333333f; //mxd. 15-degree increments
								Vector2D gridvec = Vector2D.FromAngle(angle);

								// Check distance
								float dist = 2.0f - Vector2D.DotProduct(gridvec, rotvec);
								if(dist < founddistance)
								{
									foundrotation = angle;
									founddistance = dist;
								}
							}

							// Keep rotation
							rotation = foundrotation - sectorinfo[0].rotation;
						}
						else
						{
							rotation = deltaangle + rotationoffset - sectorinfo[0].rotation;
						}
						break;
				}
				
				UpdateSectors();
				General.Interface.RedrawDisplay();
			}
		}
		
		// This updates the selection rectangle components
		private void UpdateRectangleComponents()
		{
			float gripsize = GRIP_SIZE / renderer.Scale;
			PixelColor rectcolor = General.Colors.Highlight.WithAlpha(RECTANGLE_ALPHA);

			// Corners in world space
			corners[0] = TexToWorld(new Vector2D(0f, 0f));
			corners[1] = TexToWorld(new Vector2D(texture.ScaledWidth, 0f));
			corners[2] = TexToWorld(new Vector2D(texture.ScaledWidth, -texture.ScaledHeight));
			corners[3] = TexToWorld(new Vector2D(0f, -texture.ScaledHeight));

			// Vertices
			cornerverts = new FlatVertex[6];
			for(int i = 0; i < 6; i++)
			{
				cornerverts[i] = new FlatVertex();
				cornerverts[i].z = 1.0f;
				cornerverts[i].c = rectcolor.ToInt();
			}
			cornerverts[0].x = corners[0].x;
			cornerverts[0].y = corners[0].y;
			cornerverts[1].x = corners[1].x;
			cornerverts[1].y = corners[1].y;
			cornerverts[2].x = corners[2].x;
			cornerverts[2].y = corners[2].y;
			cornerverts[3].x = corners[0].x;
			cornerverts[3].y = corners[0].y;
			cornerverts[4].x = corners[2].x;
			cornerverts[4].y = corners[2].y;
			cornerverts[5].x = corners[3].x;
			cornerverts[5].y = corners[3].y;

			// Extended points for rotation corners
			extends[0] = TexToWorld(new Vector2D(texture.ScaledWidth + (20f * Math.Sign(scale.x * sectorinfo[0].scale.x)) / renderer.Scale * (scale.x * sectorinfo[0].scale.x), 0f));
			extends[1] = TexToWorld(new Vector2D(0f, -texture.ScaledHeight + (-20f * Math.Sign(scale.y * sectorinfo[0].scale.y))  / renderer.Scale * (scale.y * sectorinfo[0].scale.y)));

			// Middle points between corners
			Vector2D middle12 = corners[1] + (corners[2] - corners[1]) * 0.5f;
			Vector2D middle23 = corners[2] + (corners[3] - corners[2]) * 0.5f;
			
			// Resize grips
			resizegrips[0] = new RectangleF(middle12.x - gripsize * 0.5f,
											middle12.y - gripsize * 0.5f,
											gripsize, gripsize);
			resizegrips[1] = new RectangleF(middle23.x - gripsize * 0.5f,
											middle23.y - gripsize * 0.5f,
											gripsize, gripsize);

			// Rotate grips
			rotategrips[0] = new RectangleF(extends[0].x - gripsize * 0.5f,
											extends[0].y - gripsize * 0.5f,
											gripsize, gripsize);
			rotategrips[1] = new RectangleF(extends[1].x - gripsize * 0.5f,
											extends[1].y - gripsize * 0.5f,
											gripsize, gripsize);

			if(showalignoffset)
			{
				Vector2D worldalignoffset = TexToWorld(alignoffset);
				alignrect = new RectangleF(worldalignoffset.x - gripsize * 0.5f,
										   worldalignoffset.y - gripsize * 0.5f,
										   gripsize, gripsize);
			}
		}

		// This checks and returns the grip the mouse pointer is in
		private Grip CheckMouseGrip()
		{
			if(PointInRectF(resizegrips[0], mousemappos)) return Grip.SizeH;
			if(PointInRectF(resizegrips[1], mousemappos)) return Grip.SizeV;
			if(PointInRectF(rotategrips[0], mousemappos)) return Grip.RotateRT;
			if(PointInRectF(rotategrips[1], mousemappos)) return Grip.RotateLB;
			if(Tools.PointInPolygon(corners, mousemappos)) return Grip.Main;
			return Grip.None;
		}
		
		#endregion

		#region ================== Events

		// Mode engages
		public override void OnEngage()
		{
			prevviewmode = General.Map.Renderer2D.ViewMode; //mxd
			base.OnEngage();

			// We don't want to record this for undoing while we move the geometry around.
			// This will be set back to normal when we're done.
			General.Map.UndoRedo.IgnorePropChanges = true;

			// Presentation
			renderer.SetPresentation(Presentation.Standard);

			// Selection
			General.Map.Map.ConvertSelection(SelectionType.Sectors);
			General.Map.Map.SelectionType = SelectionType.Sectors;
			if(General.Map.Map.SelectedSectorsCount == 0)
			{
				// Find the nearest linedef within highlight range
				Linedef l = General.Map.Map.NearestLinedef(mousemappos);
				if(l != null)
				{
					Sector selectsector = null;
					
					// Check on which side of the linedef the mouse is and which sector there is
					float side = l.SideOfLine(mousemappos);
					if((side > 0) && (l.Back != null))
						selectsector = l.Back.Sector;
					else if((side <= 0) && (l.Front != null))
						selectsector = l.Front.Sector;

					// Select the sector!
					if(selectsector != null)
					{
						selectsector.Selected = true;
						foreach(Sidedef sd in selectsector.Sidedefs)
							sd.Line.Selected = true;
					}
				}
			}
			
			// Get sector selection
			selection = General.Map.Map.GetSelectedSectors(true);
			if(selection.Count == 0)
			{
				General.Interface.MessageBeep(MessageBeepType.Default);
				General.Interface.DisplayStatus(StatusType.Action, "A selected sector is required for this action.");
				General.Editing.CancelMode();
				return;
			}
			editsector = General.GetByIndex(selection, 0);

			// Get the texture
			texture = GetTexture(editsector);
			if((texture == null) || (texture == General.Map.Data.WhiteTexture) ||
			   (texture.Width <= 0) || (texture.Height <= 0) || !texture.IsImageLoaded)
			{
				General.Interface.MessageBeep(MessageBeepType.Default);
				General.Interface.DisplayStatus(StatusType.Action, "The selected sector must have a loaded texture to align.");
				General.Editing.CancelMode();
				return;
			}
			
			// Cache the transformation values
			sectorinfo = new List<SectorInfo>(selection.Count);
			foreach(Sector s in selection)
			{
				SectorInfo si;
				si.rotation = Angle2D.DegToRad(s.Fields.GetValue(RotationName, 0.0f));
				si.scale.x = s.Fields.GetValue(XScaleName, 1.0f);
				si.scale.y = s.Fields.GetValue(YScaleName, 1.0f);
				si.offset.x = s.Fields.GetValue(XOffsetName, 0.0f);
				si.offset.y = -s.Fields.GetValue(YOffsetName, 0.0f);
				sectorinfo.Add(si);
			}

			// We want the texture corner nearest to the center of the sector
			Vector2D fp;
			fp.x = (editsector.BBox.Left + editsector.BBox.Right) / 2;
			fp.y = (editsector.BBox.Top + editsector.BBox.Bottom) / 2;

			// Transform the point into texture space
			fp = WorldToTex(fp);
			
			// Snap to the nearest left-top corner
			fp.x = (float)Math.Floor(fp.x / texture.ScaledWidth) * texture.ScaledWidth;
			fp.y = (float)Math.Ceiling(fp.y / texture.ScaledHeight) * texture.ScaledHeight;

			// Now move the offset so that the 0,0 point is at this location
			// We want to work with the 0,0 location because it makes things easier.
			SectorInfo si0 = sectorinfo[0];
			si0.offset -= fp / si0.scale;
			sectorinfo[0] = si0;

			UpdateRectangleComponents();
			UpdateSectors();
		}

		// Mode disengages
		public override void OnDisengage()
		{
			switch(prevviewmode) 
			{
				case ViewMode.Normal: General.Actions.InvokeAction("builder_viewmodenormal"); break;
				case ViewMode.FloorTextures: General.Actions.InvokeAction("builder_viewmodefloors"); break;
				case ViewMode.CeilingTextures: General.Actions.InvokeAction("builder_viewmodeceilings"); break;
				case ViewMode.Brightness: General.Actions.InvokeAction("builder_viewmodebrightness"); break;
			}
			
			base.OnDisengage();

			// When not cancelled manually, we assume it is accepted
			if(!cancelled)
			{
				modealreadyswitching = true;
				General.Editing.AcceptMode();
			}
			
			// Hide highlight info
			General.Interface.SetCursor(Cursors.Default);
			General.Interface.HideInfo();
			General.Interface.RedrawDisplay();
		}

		// When accepted
		public override void OnAccept()
		{
			base.OnAccept();

			if(!modealreadyswitching)
			{
				modealreadyswitching = true;
				
				// Restore original values
				RestoreSectors();
				General.Map.Map.Update();

				// Make undo
				General.Map.UndoRedo.CreateUndo(UndoDescription);

				// Resume normal undo/redo recording
				General.Map.UndoRedo.IgnorePropChanges = false;

				// Apply changes
				UpdateSectors();
				General.Map.Map.Update();

				// Clear selection
				if(selection.Count == 1)
					General.Map.Map.ClearAllSelected();

				// Done
				General.Map.IsChanged = true;
				selection = null;
				sectorinfo = null;
				
				// Return to previous stable mode
				General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
			}
		}

		// Cancel mode
		public override void OnCancel()
		{
			base.OnCancel();
			modealreadyswitching = true;
			
			// Restore original values
			RestoreSectors();
			General.Map.Map.Update();

			// Resume normal undo/redo recording
			General.Map.UndoRedo.IgnorePropChanges = false;

			// Return to previous stable mode
			General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if(panning) return; //mxd. Skip all this jazz while panning
			Update();
		}

		// Mouse leaves the display
		public override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			// Reset cursor
			General.Interface.SetCursor(Cursors.Default);
		}


		// When edit button is pressed
		protected override void OnEditBegin()
		{
			base.OnEditBegin();
			OnSelectBegin();
		}

		// When edit button is released
		protected override void OnEditEnd()
		{
			base.OnEditEnd();
			OnSelectEnd();
		}

		// When select button is pressed
		protected override void OnSelectBegin()
		{
			base.OnSelectBegin();
			if(mode != ModifyMode.None) return;

			// Check what grip the mouse is over
			switch(CheckMouseGrip())
			{
				// Drag main rectangle
				case Grip.Main:
					dragoffset = -mousemappos - offset;
					mode = ModifyMode.Dragging;
					EnableAutoPanning();
					autopanning = true;
					break;

				// Scale
				case Grip.SizeH:
					// The resize vector is a unit vector in the direction of the resize.
					// We multiply this with the sign of the current size, because the
					// corners may be reversed when the selection is flipped.
					resizevector = corners[1] - corners[0];
					resizevector = resizevector.GetNormal() * Math.Sign(scale.x);

					// Make the resize axis. This is a line with the length and direction
					// of basesize used to calculate the resize percentage.
					resizeaxis = new Line2D(corners[0], corners[0] + resizevector * texture.ScaledWidth / Math.Abs(sectorinfo[0].scale.x));

					// Original axis filter
					resizefilter = new Vector2D(1.0f, 0.0f);
					
					mode = ModifyMode.Resizing;
					break;

				// Scale
				case Grip.SizeV:
					// See description above
					resizevector = corners[2] - corners[1];
					resizevector = resizevector.GetNormal() * Math.Sign(scale.y);
					resizeaxis = new Line2D(corners[1], corners[1] + resizevector * texture.ScaledHeight / Math.Abs(sectorinfo[0].scale.y));
					resizefilter = new Vector2D(0.0f, 1.0f);
					mode = ModifyMode.Resizing;
					break;

				// Rotate
				case Grip.RotateRT:
					rotationoffset = Angle2D.PIHALF;
					if(Math.Sign(scale.x * sectorinfo[0].scale.x) < 0)
						rotationoffset += Angle2D.PI;
					rotationcenter = corners[0];
					mode = ModifyMode.Rotating;
					break;

				// Rotate
				case Grip.RotateLB:
					rotationoffset = 0f;
					if(Math.Sign(scale.y * sectorinfo[0].scale.y) < 0)
						rotationoffset += Angle2D.PI;
					rotationcenter = corners[0];
					mode = ModifyMode.Rotating;
					break;
					
				// Outside the selection?
				default:
					// Accept and be done with it
					General.Editing.AcceptMode();
					break;
			}
		}

		// When selected button is released
		protected override void OnSelectEnd()
		{
			base.OnSelectEnd();

			// Remove extension line
			extensionline = new Line2D();

			if(autopanning)
			{
				DisableAutoPanning();
				autopanning = false;
			}

			// No modifying mode
			mode = ModifyMode.None;

			// Redraw
			General.Map.Map.Update();
			General.Interface.RedrawDisplay();
		}

		// This redraws the display
		public override void OnRedrawDisplay()
		{
			if(sectorinfo != null) UpdateRectangleComponents();
			renderer.RedrawSurface();

			// Render lines
			if(renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);
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
			if(sectorinfo != null)
			{
				if(renderer.StartOverlay(true))
				{
					// Rectangle
					PixelColor rectcolor = General.Colors.Highlight.WithAlpha(RECTANGLE_ALPHA);
					renderer.RenderGeometry(cornerverts, null, true);
					if(extensionline.GetLengthSq() > 0.0f)
						renderer.RenderLine(extensionline.v1, extensionline.v2, 1, General.Colors.Indication.WithAlpha(EXTENSION_LINE_ALPHA), true);
					renderer.RenderLine(corners[0], corners[1], 4, rectcolor, true);
					renderer.RenderLine(corners[1], corners[2], 4, rectcolor, true);
					renderer.RenderLine(corners[2], corners[3], 4, rectcolor, true);
					renderer.RenderLine(corners[3], corners[0], 4, rectcolor, true);

					// Lines
					renderer.RenderLine(corners[0], extends[0], 1f, General.Colors.Highlight, true);
					renderer.RenderLine(corners[0], extends[1], 1f, General.Colors.Highlight, true);
					renderer.RenderLine(corners[1], corners[2], 0.5f, General.Colors.Highlight, true);
					renderer.RenderLine(corners[2], corners[3], 0.5f, General.Colors.Highlight, true);

					// Handles
					renderer.RenderRectangleFilled(rotategrips[0], General.Colors.Background, true);
					renderer.RenderRectangleFilled(rotategrips[1], General.Colors.Background, true);
					renderer.RenderRectangle(rotategrips[0], 2f, General.Colors.Indication, true);
					renderer.RenderRectangle(rotategrips[1], 2f, General.Colors.Indication, true);
					renderer.RenderRectangleFilled(resizegrips[0], General.Colors.Background, true);
					renderer.RenderRectangleFilled(resizegrips[1], General.Colors.Background, true);
					renderer.RenderRectangle(resizegrips[0], 2f, General.Colors.Highlight, true);
					renderer.RenderRectangle(resizegrips[1], 2f, General.Colors.Highlight, true);

					// Rotate/align point
					if(showalignoffset)
						renderer.RenderRectangleFilled(alignrect, General.Colors.Selection, true);

					renderer.Finish();
				}
			}
			
			renderer.Present();
		}

		#endregion

		#region ================== Actions

		// This clears the selection
		[BeginAction("clearselection", BaseAction = true)]
		public void ClearSelection()
		{
			// Accept changes
			General.Editing.AcceptMode();
			General.Map.Map.ClearAllSelected();
			General.Interface.RedrawDisplay();
		}

		#endregion
	}
}
