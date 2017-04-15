
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
using System.Globalization;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.VisualModes;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class VisualMiddle3D : BaseVisualGeometrySidedef
	{
		#region ================== Constants

		#endregion
		
		#region ================== Variables

		protected Effect3DFloor extrafloor;
		
		#endregion
		
		#region ================== Properties

		#endregion
		
		#region ================== Constructor / Setup
		
		// Constructor
		public VisualMiddle3D(BaseVisualMode mode, VisualSector vs, Sidedef s) : base(mode, vs, s)
		{
			//mxd
			geometrytype = VisualGeometryType.WALL_MIDDLE_3D;
			partname = "mid";
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		// This builds the geometry. Returns false when no geometry created.
		public override bool Setup() { return this.Setup(this.extrafloor); }
		public bool Setup(Effect3DFloor extrafloor)
		{
			Sidedef sourceside = extrafloor.Linedef.Front;
			this.extrafloor = extrafloor;

			//mxd. Extrafloor may've become invalid during undo/redo...
			if(sourceside == null)
			{
				base.SetVertices(null);
				return false;
			}

			Vector2D vl, vr;

			//mxd. lightfog flag support
			int lightvalue;
			bool lightabsolute;
			GetLightValue(out lightvalue, out lightabsolute);

			Vector2D tscale = new Vector2D(sourceside.Fields.GetValue("scalex_mid", 1.0f),
										   sourceside.Fields.GetValue("scaley_mid", 1.0f));
            Vector2D tscaleAbs = new Vector2D(Math.Abs(tscale.x), Math.Abs(tscale.y));
            Vector2D toffset1 = new Vector2D(Sidedef.Fields.GetValue("offsetx_mid", 0.0f),
											 Sidedef.Fields.GetValue("offsety_mid", 0.0f));
			Vector2D toffset2 = new Vector2D(sourceside.Fields.GetValue("offsetx_mid", 0.0f),
											 sourceside.Fields.GetValue("offsety_mid", 0.0f));
			
			// Left and right vertices for this sidedef
			if(Sidedef.IsFront)
			{
				vl = new Vector2D(Sidedef.Line.Start.Position.x, Sidedef.Line.Start.Position.y);
				vr = new Vector2D(Sidedef.Line.End.Position.x, Sidedef.Line.End.Position.y);
			}
			else
			{
				vl = new Vector2D(Sidedef.Line.End.Position.x, Sidedef.Line.End.Position.y);
				vr = new Vector2D(Sidedef.Line.Start.Position.x, Sidedef.Line.Start.Position.y);
			}
			
			// Load sector data
			SectorData sd = mode.GetSectorData(Sidedef.Sector);
			
			//mxd. which texture we must use?
			long texturelong = 0;
			if((sourceside.Line.Args[2] & (int)Effect3DFloor.Flags.UseUpperTexture) != 0) 
			{
				if(Sidedef.LongHighTexture != MapSet.EmptyLongName)
					texturelong = Sidedef.LongHighTexture;
			} 
			else if((sourceside.Line.Args[2] & (int)Effect3DFloor.Flags.UseLowerTexture) != 0) 
			{
				if(Sidedef.LongLowTexture != MapSet.EmptyLongName)
					texturelong = Sidedef.LongLowTexture;
			} 
			else if(sourceside.LongMiddleTexture != MapSet.EmptyLongName) 
			{
				texturelong = sourceside.LongMiddleTexture;
			}

			// Texture given?
			if(texturelong != 0)
			{
				// Load texture
				base.Texture = General.Map.Data.GetTextureImage(texturelong);
				if(base.Texture == null || base.Texture is UnknownImage)
				{
					base.Texture = General.Map.Data.UnknownTexture3D;
					setuponloadedtexture = texturelong;
				}
				else if(!base.Texture.IsImageLoaded)
				{
					setuponloadedtexture = texturelong;
				}
			}
			else
			{
				// Use missing texture
				base.Texture = General.Map.Data.MissingTexture3D;
				setuponloadedtexture = 0;
			}
			
			// Get texture scaled size
			Vector2D tsz = new Vector2D(base.Texture.ScaledWidth, base.Texture.ScaledHeight);
			tsz = tsz / tscale;
			
			// Get texture offsets
			Vector2D tof = new Vector2D(Sidedef.OffsetX, Sidedef.OffsetY) + new Vector2D(sourceside.OffsetX, sourceside.OffsetY);
			tof = tof + toffset1 + toffset2;
			tof = tof / tscaleAbs;
			if(General.Map.Config.ScaledTextureOffsets && !base.Texture.WorldPanning)
				tof = tof * base.Texture.Scale;
			
			// For Vavoom type 3D floors the ceiling is lower than floor and they are reversed.
			// We choose here.
			float sourcetopheight = extrafloor.VavoomType ? sourceside.Sector.FloorHeight : sourceside.Sector.CeilHeight;
			float sourcebottomheight = extrafloor.VavoomType ? sourceside.Sector.CeilHeight : sourceside.Sector.FloorHeight;
			
			// Determine texture coordinates plane as they would be in normal circumstances.
			// We can then use this plane to find any texture coordinate we need.
			// The logic here is the same as in the original VisualMiddleSingle (except that
			// the values are stored in a TexturePlane)
			// NOTE: I use a small bias for the floor height, because if the difference in
			// height is 0 then the TexturePlane doesn't work!
			TexturePlane tp = new TexturePlane();
			float floorbias = (sourcetopheight == sourcebottomheight) ? 1.0f : 0.0f;

			tp.trb.x = tp.tlt.x + (float)Math.Round(Sidedef.Line.Length); //mxd. (G)ZDoom snaps texture coordinates to integral linedef length
			tp.trb.y = tp.tlt.y + (sourcetopheight - sourcebottomheight) + floorbias;
			
			// Apply texture offset
			tp.tlt += tof;
			tp.trb += tof;
			
			// Transform pixel coordinates to texture coordinates
			tp.tlt /= tsz;
			tp.trb /= tsz;
			
			// Left top and right bottom of the geometry that
			tp.vlt = new Vector3D(vl.x, vl.y, sourcetopheight);
			tp.vrb = new Vector3D(vr.x, vr.y, sourcebottomheight + floorbias);
			
			// Make the right-top coordinates
			tp.trt = new Vector2D(tp.trb.x, tp.tlt.y);
			tp.vrt = new Vector3D(tp.vrb.x, tp.vrb.y, tp.vlt.z);
			
			//mxd. Get ceiling and floor heights. Use our and neighbour sector's data
			SectorData sdo = mode.GetSectorData(Sidedef.Other.Sector); 

			float flo = sdo.Floor.plane.GetZ(vl);
			float fro = sdo.Floor.plane.GetZ(vr);
			float clo = sdo.Ceiling.plane.GetZ(vl);
			float cro = sdo.Ceiling.plane.GetZ(vr);

			float fle = sd.Floor.plane.GetZ(vl);
			float fre = sd.Floor.plane.GetZ(vr);
			float cle = sd.Ceiling.plane.GetZ(vl);
			float cre = sd.Ceiling.plane.GetZ(vr);

			float fl = flo > fle ? flo : fle;
			float fr = fro > fre ? fro : fre;
			float cl = clo < cle ? clo : cle;
			float cr = cro < cre ? cro : cre;
			
			// Anything to see?
			if(((cl - fl) > 0.01f) || ((cr - fr) > 0.01f))
			{
				// Keep top and bottom planes for intersection testing
				top = extrafloor.Floor.plane;
				bottom = extrafloor.Ceiling.plane;
				
				// Create initial polygon, which is just a quad between floor and ceiling
				WallPolygon poly = new WallPolygon();
				poly.Add(new Vector3D(vl.x, vl.y, fl));
				poly.Add(new Vector3D(vl.x, vl.y, cl));
				poly.Add(new Vector3D(vr.x, vr.y, cr));
				poly.Add(new Vector3D(vr.x, vr.y, fr));
				
				// Determine initial color
				int lightlevel = lightabsolute ? lightvalue : sd.Ceiling.brightnessbelow + lightvalue;
				
				//mxd. This calculates light with doom-style wall shading
				PixelColor wallbrightness = PixelColor.FromInt(mode.CalculateBrightness(lightlevel, Sidedef));
				PixelColor wallcolor = PixelColor.Modulate(sd.Ceiling.colorbelow, wallbrightness);
				fogfactor = CalculateFogFactor(lightlevel);
				poly.color = wallcolor.WithAlpha(255).ToInt();

				// Cut off the part above the 3D floor and below the 3D ceiling
				CropPoly(ref poly, extrafloor.Floor.plane, false);
				CropPoly(ref poly, extrafloor.Ceiling.plane, false);

				// Cut out pieces that overlap 3D floors in this sector
				List<WallPolygon> polygons = new List<WallPolygon> { poly };
				bool translucent = (extrafloor.RenderAdditive || extrafloor.Alpha < 255);
				foreach(Effect3DFloor ef in sd.ExtraFloors)
				{
					//mxd. Our poly should be clipped when our ond other extrafloors are both solid or both translucent,
					// or when only our extrafloor is translucent.
					// Our poly should not be clipped when our extrafloor is translucent and the other one isn't and both have renderinside setting.
					bool othertranslucent = (ef.RenderAdditive || ef.Alpha < 255);
					if(translucent && !othertranslucent && !ef.ClipSidedefs) continue;
					if(ef.ClipSidedefs == extrafloor.ClipSidedefs || ef.ClipSidedefs) 
					{
						//TODO: find out why ef can be not updated at this point
						//TODO: [this crashed on me once when performing auto-align on myriad of textures on BoA C1M0]
						if(ef.Floor == null || ef.Ceiling == null) ef.Update();
						
						int num = polygons.Count;
						for(int pi = 0; pi < num; pi++)
						{
							// Split by floor plane of 3D floor
							WallPolygon p = polygons[pi];
							WallPolygon np = SplitPoly(ref p, ef.Ceiling.plane, true);
							
							if(np.Count > 0)
							{
								// Split part below floor by the ceiling plane of 3D floor
								// and keep only the part below the ceiling (front)
								SplitPoly(ref np, ef.Floor.plane, true);

								if(p.Count == 0)
								{
									polygons[pi] = np;
								}
								else
								{
									polygons[pi] = p;
									polygons.Add(np);
								}
							}
							else
							{
								polygons[pi] = p;
							}
						}
					}
				}
				
				// Process the polygon and create vertices
				if(polygons.Count > 0)
				{
					List<WorldVertex> verts = CreatePolygonVertices(polygons, tp, sd, lightvalue, lightabsolute);
					if(verts.Count > 2)
					{
						if (extrafloor.Sloped3dFloor) this.RenderPass = RenderPass.Mask; //mxd
						else if (extrafloor.RenderAdditive) this.RenderPass = RenderPass.Additive; //mxd
						else if ((extrafloor.Alpha < 255) || Texture.IsTranslucent) this.RenderPass = RenderPass.Alpha; // [ZZ] translucent texture should trigger Alpha pass
						else this.RenderPass = RenderPass.Mask;

						if(extrafloor.Alpha < 255)
						{
							// Apply alpha to vertices
							byte alpha = (byte)General.Clamp(extrafloor.Alpha, 0, 255);
							if(alpha < 255)
							{
								for(int i = 0; i < verts.Count; i++)
								{
									WorldVertex v = verts[i];
									v.c = PixelColor.FromInt(v.c).WithAlpha(alpha).ToInt();
									verts[i] = v;
								}
							}
						}

						base.SetVertices(verts);
						return true;
					}
				}
			}
			
			base.SetVertices(null); //mxd
			return false;
		}
		
		#endregion
		
		#region ================== Methods

		// This performs a fast test in object picking (mxd)
		public override bool PickFastReject(Vector3D from, Vector3D to, Vector3D dir) 
		{
			// Top and bottom are swapped in Vavoom-type 3d floors
			if(extrafloor.VavoomType)
				return (pickintersect.z >= top.GetZ(pickintersect)) && (pickintersect.z <= bottom.GetZ(pickintersect));
			return base.PickFastReject(from, to, dir);
		}

		//mxd. Alpha based picking
		public override bool PickAccurate(Vector3D from, Vector3D to, Vector3D dir, ref float u_ray)
		{
			if(!BuilderPlug.Me.AlphaBasedTextureHighlighting || !Texture.IsImageLoaded || (!Texture.IsTranslucent && !Texture.IsMasked)) return base.PickAccurate(from, to, dir, ref u_ray);

			float u;
			Sidedef sourceside = extrafloor.Linedef.Front;
			new Line2D(from, to).GetIntersection(Sidedef.Line.Line, out u);
			if(Sidedef != Sidedef.Line.Front) u = 1.0f - u;

			// Some textures (e.g. HiResImage) may lie about their size, so use bitmap size instead
			Bitmap image = Texture.GetBitmap();

			// Determine texture scale...
			Vector2D imgscale = new Vector2D((float)Texture.Width / image.Width, (float)Texture.Height / image.Height);
			Vector2D texscale = (Texture is HiResImage) ? imgscale * Texture.Scale : Texture.Scale;

			// Get correct offset to texture space...
			float texoffsetx = Sidedef.OffsetX + sourceside.OffsetX + UniFields.GetFloat(Sidedef.Fields, "offsetx_mid") + UniFields.GetFloat(sourceside.Fields, "offsetx_mid");
			int ox = (int)Math.Floor((u * Sidedef.Line.Length * UniFields.GetFloat(sourceside.Fields, "scalex_mid", 1.0f) / texscale.x + (texoffsetx / imgscale.x)) % image.Width);

			float texoffsety = Sidedef.OffsetY + sourceside.OffsetY + UniFields.GetFloat(Sidedef.Fields, "offsety_mid") + UniFields.GetFloat(sourceside.Fields, "offsety_mid");
			int oy = (int)Math.Ceiling(((pickintersect.z - sourceside.Sector.CeilHeight) * UniFields.GetFloat(sourceside.Fields, "scaley_mid", 1.0f) / texscale.y - (texoffsety / imgscale.y)) % image.Height);

			// Make sure offsets are inside of texture dimensions...
			if(ox < 0) ox += image.Width;
			if(oy < 0) oy += image.Height;

			// Check pixel alpha
			Point pixelpos = new Point(General.Clamp(ox, 0, image.Width - 1), General.Clamp(image.Height - oy, 0, image.Height - 1));
			return (image.GetPixel(pixelpos.X, pixelpos.Y).A > 0 && base.PickAccurate(@from, to, dir, ref u_ray));
		}

		// Return texture name
		public override string GetTextureName()
		{
			//mxd
			if((extrafloor.Linedef.Args[2] & (int)Effect3DFloor.Flags.UseUpperTexture) != 0)
				return Sidedef.HighTexture;
			if((extrafloor.Linedef.Args[2] & (int)Effect3DFloor.Flags.UseLowerTexture) != 0)
				return Sidedef.LowTexture;
			return extrafloor.Linedef.Front.MiddleTexture;
		}

		// This changes the texture
		protected override void SetTexture(string texturename)
		{
			//mxd
			if((extrafloor.Linedef.Args[2] & (int)Effect3DFloor.Flags.UseUpperTexture) != 0)
				Sidedef.SetTextureHigh(texturename);
			if((extrafloor.Linedef.Args[2] & (int)Effect3DFloor.Flags.UseLowerTexture) != 0)
				Sidedef.SetTextureLow(texturename);
			else
				extrafloor.Linedef.Front.SetTextureMid(texturename);

			General.Map.Data.UpdateUsedTextures();

			//mxd. Update model sector
			mode.GetVisualSector(extrafloor.Linedef.Front.Sector).UpdateSectorGeometry(false);
		}

		protected override void SetTextureOffsetX(int x)
		{
			Sidedef.Fields.BeforeFieldsChange();
			Sidedef.Fields["offsetx_mid"] = new UniValue(UniversalType.Float, (float)x);
		}

		protected override void SetTextureOffsetY(int y)
		{
			Sidedef.Fields.BeforeFieldsChange();
			Sidedef.Fields["offsety_mid"] = new UniValue(UniversalType.Float, (float)y);
		}

		protected override void MoveTextureOffset(int offsetx, int offsety)
		{
			Sidedef.Fields.BeforeFieldsChange();
			float oldx = Sidedef.Fields.GetValue("offsetx_mid", 0.0f);
			float oldy = Sidedef.Fields.GetValue("offsety_mid", 0.0f);
			float scalex = extrafloor.Linedef.Front.Fields.GetValue("scalex_mid", 1.0f); //mxd
			float scaley = extrafloor.Linedef.Front.Fields.GetValue("scaley_mid", 1.0f); //mxd
			Sidedef.Fields["offsetx_mid"] = new UniValue(UniversalType.Float, GetRoundedTextureOffset(oldx, offsetx, scalex, Texture.Width)); //mxd
			Sidedef.Fields["offsety_mid"] = new UniValue(UniversalType.Float, GetRoundedTextureOffset(oldy, offsety, scaley, Texture.Height)); //mxd
		}

		protected override Point GetTextureOffset()
		{
			float oldx = Sidedef.Fields.GetValue("offsetx_mid", 0.0f);
			float oldy = Sidedef.Fields.GetValue("offsety_mid", 0.0f);
			return new Point((int)oldx, (int)oldy);
		}

		//mxd
		public override Linedef GetControlLinedef()
		{
			return extrafloor.Linedef;
		}

		//mxd
		public override void OnTextureFit(FitTextureOptions options) 
		{
			if(!General.Map.UDMF) return;
			if(string.IsNullOrEmpty(extrafloor.Linedef.Front.MiddleTexture) || extrafloor.Linedef.Front.MiddleTexture == "-" || !Texture.IsImageLoaded) return;
			FitTexture(options);

			// Update the model sector to update all 3d floors
			mode.GetVisualSector(extrafloor.Linedef.Front.Sector).UpdateSectorGeometry(false);
		}

		//mxd. Only control sidedef scale is used by GZDoom
		public override void OnChangeScale(int incrementX, int incrementY)
		{
			if(!General.Map.UDMF || Texture == null || !Texture.IsImageLoaded) return;

			if((General.Map.UndoRedo.NextUndo == null) || (General.Map.UndoRedo.NextUndo.TicketID != undoticket))
				undoticket = mode.CreateUndo("Change wall scale");

			Sidedef target = extrafloor.Linedef.Front;
			if(target == null) return;

			float scaleX = target.Fields.GetValue("scalex_mid", 1.0f);
			float scaleY = target.Fields.GetValue("scaley_mid", 1.0f);

			target.Fields.BeforeFieldsChange();

			if(incrementX != 0)
			{
				float pix = (int)Math.Round(Texture.Width * scaleX) - incrementX;
				float newscaleX = (float)Math.Round(pix / Texture.Width, 3);
				scaleX = (newscaleX == 0 ? scaleX * -1 : newscaleX);
				UniFields.SetFloat(target.Fields, "scalex_mid", scaleX, 1.0f);
			}

			if(incrementY != 0)
			{
				float pix = (int)Math.Round(Texture.Height * scaleY) - incrementY;
				float newscaleY = (float)Math.Round(pix / Texture.Height, 3);
				scaleY = (newscaleY == 0 ? scaleY * -1 : newscaleY);
				UniFields.SetFloat(target.Fields, "scaley_mid", scaleY, 1.0f);
			}
			
			// Update the model sector to update all 3d floors
			mode.GetVisualSector(extrafloor.Linedef.Front.Sector).UpdateSectorGeometry(false);

			// Display result
			mode.SetActionResult("Wall scale changed to " + scaleX.ToString("F03", CultureInfo.InvariantCulture) + ", " + scaleY.ToString("F03", CultureInfo.InvariantCulture) + " (" + (int)Math.Round(Texture.Width / scaleX) + " x " + (int)Math.Round(Texture.Height / scaleY) + ").");
		}

		//mxd
		protected override void ResetTextureScale()
		{
			Sidedef target = extrafloor.Linedef.Front;
			target.Fields.BeforeFieldsChange();
			if(target.Fields.ContainsKey("scalex_mid")) target.Fields.Remove("scalex_mid");
			if(target.Fields.ContainsKey("scaley_mid")) target.Fields.Remove("scaley_mid");
		}

		//mxd
		public override void OnResetTextureOffset()
		{
			base.OnResetTextureOffset();

			// Update the model sector to update all 3d floors
			mode.GetVisualSector(extrafloor.Linedef.Front.Sector).UpdateSectorGeometry(false);
		}

		//mxd
		public override void OnResetLocalTextureOffset()
		{
			if(!General.Map.UDMF || !General.Map.Config.UseLocalSidedefTextureOffsets)
			{
				OnResetTextureOffset();
				return;
			}

			base.OnResetLocalTextureOffset();

			// Update the model sector to update all 3d floors
			mode.GetVisualSector(extrafloor.Linedef.Front.Sector).UpdateSectorGeometry(false);
		}
		
		#endregion
	}
}
