#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes.IO
{
	#region ================== Structs

	internal struct WavefrontExportSettings
	{
		public string Obj;
		public readonly string ObjName;
		public readonly string ObjPath;
		public readonly float Scale;
		public readonly bool FixScale;
		public readonly bool ExportTextures;
		public bool Valid;
		public string[] Textures;
		public string[] Flats;

		public WavefrontExportSettings(string name, string path, float scale, bool fixScale, bool exportTextures) 
		{
			ObjName = name;
			ObjPath = path;
			Scale = scale;
			FixScale = fixScale;
			ExportTextures = exportTextures;

			Valid = false;
			Obj = string.Empty;
			Textures = null;
			Flats = null;
		}
	}

	#endregion

	internal class WavefrontExporter
	{

		#region ================== Variables and structs

		private const string DEFAULT = "Default";

		private struct VertexIndices
		{
			public int PositionIndex;
			public int UVIndex;
			public int NormalIndex;
		}

		#endregion

		#region ================== Export

		public void Export(ICollection<Sector> sectors, WavefrontExportSettings settings) 
		{
			CreateObjFromSelection(sectors, ref settings);

			if(!settings.Valid) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "OBJ creation failed. Check 'Errors and Warnings' window for details.");
				return;
			}

			if(settings.ExportTextures) 
			{
				//save all used textures
				if(settings.Textures != null) 
				{
					foreach(string s in settings.Textures) 
					{
						if(s == DEFAULT) continue;
						if(General.Map.Data.GetTextureExists(s)) 
						{
							ImageData id = General.Map.Data.GetTextureImage(s);
							if(id.Width == 0 || id.Height == 0) 
							{
								General.ErrorLogger.Add(ErrorType.Warning, "OBJ Exporter: texture \"" + s + "\" has invalid size (" + id.Width + "x" + id.Height + ")!");
								continue;
							}

							if(!id.IsImageLoaded) id.LoadImage();
							Bitmap bmp = id.GetBitmap();
                            lock (bmp)
                            {
                                bmp.Save(Path.Combine(settings.ObjPath, Path.GetFileNameWithoutExtension(s) + ".PNG"), ImageFormat.Png);
                            }
						} 
						else 
						{
							General.ErrorLogger.Add(ErrorType.Warning, "OBJ Exporter: texture \"" + s + "\" does not exist!");
						}
					}
				}

				if(settings.Flats != null) 
				{
					foreach(string s in settings.Flats) 
					{
						if(s == DEFAULT) continue;
						if(General.Map.Data.GetFlatExists(s)) 
						{
							ImageData id = General.Map.Data.GetFlatImage(s);
							if(id.Width == 0 || id.Height == 0) 
							{
								General.ErrorLogger.Add(ErrorType.Warning, "OBJ Exporter: flat \"" + s + "\" has invalid size (" + id.Width + "x" + id.Height + ")!");
								continue;
							}

							if(!id.IsImageLoaded) id.LoadImage();
							Bitmap bmp = id.GetBitmap();

							// Handle duplicate names
							string flatname = s;
							if(settings.Textures != null && Array.IndexOf(settings.Textures, s) != -1)
								flatname += "_FLAT";

                            lock (bmp)
                            {
                                bmp.Save(Path.Combine(settings.ObjPath, Path.GetFileNameWithoutExtension(flatname) + ".PNG"), ImageFormat.Png);
                            }

						} 
						else 
						{
							General.ErrorLogger.Add(ErrorType.Warning, "OBJ Exporter: flat \"" + s + "\" does not exist!");
						}
					}
				}
			}

			//write obj
			string savePath = Path.Combine(settings.ObjPath, settings.ObjName);
			using(StreamWriter sw = new StreamWriter(savePath + ".obj", false))
				sw.Write(settings.Obj);

			//create mtl
			StringBuilder mtl = new StringBuilder();
			mtl.Append("# MTL for " + General.Map.FileTitle + ", map " + General.Map.Options.LevelName + Environment.NewLine);
			mtl.Append("# Created by GZDoom Builder " + Application.ProductVersion + Environment.NewLine + Environment.NewLine);

			if(settings.Textures != null) 
			{
				foreach(string s in settings.Textures) 
				{
					if(s == DEFAULT) continue;
					mtl.Append("newmtl " + s.ToUpperInvariant() + Environment.NewLine);
					mtl.Append("Kd 1.0 1.0 1.0" + Environment.NewLine);
					if(settings.ExportTextures) mtl.Append("map_Kd " + Path.Combine(settings.ObjPath, s.ToUpperInvariant() + ".PNG") + Environment.NewLine);
					mtl.Append(Environment.NewLine);
				}
			}

			if(settings.Flats != null) 
			{
				foreach(string s in settings.Flats) 
				{
					if(s == DEFAULT) continue;
					mtl.Append("newmtl " + s.ToUpperInvariant() + Environment.NewLine);
					mtl.Append("Kd 1.0 1.0 1.0" + Environment.NewLine);
					if(settings.ExportTextures) 
					{
						// Handle duplicate names
						string flatname = s;
						if(settings.Textures != null && Array.IndexOf(settings.Textures, s) != -1)
							flatname += "_FLAT";

						mtl.Append("map_Kd " + Path.Combine(settings.ObjPath, flatname.ToUpperInvariant() + ".PNG") + Environment.NewLine);
					}
					mtl.Append(Environment.NewLine);
				}
			}

			//write mtl
			using(StreamWriter sw = new StreamWriter(savePath + ".mtl", false))
				sw.Write(mtl.ToString());

			//done
			General.Interface.DisplayStatus(StatusType.Warning, "Geometry exported to \"" + savePath + ".obj\"");
		}

		#endregion

		#region ================== Utility

		private static void CreateObjFromSelection(ICollection<Sector> sectors, ref WavefrontExportSettings data) 
		{
			BaseVisualMode mode = new BaseVisualMode();
			bool renderingEffectsDisabled = false;

			if(!General.Settings.EnhancedRenderingEffects) 
			{
				renderingEffectsDisabled = true;
				mode.ToggleEnhancedRendering();
			}

			mode.RebuildElementData();

			List<BaseVisualSector> visualSectors = new List<BaseVisualSector>();

			//create visual geometry
			foreach(Sector s in sectors) 
			{
				BaseVisualSector bvs = mode.CreateBaseVisualSector(s);
				if(bvs != null) visualSectors.Add(bvs);
			}

			if(visualSectors.Count == 0) 
			{
				General.ErrorLogger.Add(ErrorType.Error, "OBJ Exporter: no visual sectors to export!");
				return;
			}

			//sort geometry
			List<Dictionary<string, List<WorldVertex[]>>> geometryByTexture = SortGeometry(visualSectors);

			//restore vm settings
			if(renderingEffectsDisabled) mode.ToggleEnhancedRendering();
			mode.Dispose();

			//create obj
			StringBuilder obj = CreateObjGeometry(geometryByTexture, data);

			if(obj.Length == 0) 
			{
				General.ErrorLogger.Add(ErrorType.Error, "OBJ Exporter: failed to create geometry!");
				return;
			}

			//add header
			obj.Insert(0, "o " + General.Map.Options.LevelName + Environment.NewLine); //name
			obj.Insert(0, "# Created by GZDoom Builder " + Application.ProductVersion + Environment.NewLine + Environment.NewLine);
			obj.Insert(0, "# " + General.Map.FileTitle + ", map " + General.Map.Options.LevelName + Environment.NewLine);
			data.Obj = obj.ToString();

			string[] textures = new string[geometryByTexture[0].Keys.Count];
			geometryByTexture[0].Keys.CopyTo(textures, 0);
			Array.Sort(textures);
			data.Textures = textures;

			string[] flats = new string[geometryByTexture[1].Keys.Count];
			geometryByTexture[1].Keys.CopyTo(flats, 0);
			Array.Sort(flats);
			data.Flats = flats;

			data.Valid = true;
		}

		private static List<Dictionary<string, List<WorldVertex[]>>> SortGeometry(List<BaseVisualSector> visualSectors) 
		{
			var texturegeo = new Dictionary<string, List<WorldVertex[]>>(StringComparer.Ordinal);
			texturegeo.Add(DEFAULT, new List<WorldVertex[]>());
			var flatgeo = new Dictionary<string, List<WorldVertex[]>>(StringComparer.Ordinal);
			flatgeo.Add(DEFAULT, new List<WorldVertex[]>());

			foreach(BaseVisualSector vs in visualSectors) 
			{
				//floor
				string texture;
				if(vs.Floor != null) 
				{
					texture = vs.Sector.FloorTexture;
					CheckTextureName(ref flatgeo, ref texture);
					flatgeo[texture].AddRange(OptimizeGeometry(vs.Floor.Vertices, vs.Floor.GeometryType, vs.Floor.Sector.Sector.Labels.Count > 1));
				}

				//ceiling
				if(vs.Ceiling != null) 
				{
					texture = vs.Sector.CeilTexture;
					CheckTextureName(ref flatgeo, ref texture);
					flatgeo[texture].AddRange(OptimizeGeometry(vs.Ceiling.Vertices, vs.Ceiling.GeometryType, vs.Ceiling.Sector.Sector.Labels.Count > 1));
				}

				//walls
				if(vs.Sides != null) 
				{
					foreach(VisualSidedefParts part in vs.Sides.Values) 
					{
						//upper
						if(part.upper != null && part.upper.Vertices != null) 
						{
							texture = part.upper.Sidedef.HighTexture;
							CheckTextureName(ref texturegeo, ref texture);
							texturegeo[texture].AddRange(OptimizeGeometry(part.upper.Vertices, part.upper.GeometryType));
						}

						//middle single
						if(part.middlesingle != null && part.middlesingle.Vertices != null) 
						{
							texture = part.middlesingle.Sidedef.MiddleTexture;
							CheckTextureName(ref texturegeo, ref texture);
							texturegeo[texture].AddRange(OptimizeGeometry(part.middlesingle.Vertices, part.middlesingle.GeometryType));
						}

						//middle double
						if(part.middledouble != null && part.middledouble.Vertices != null) 
						{
							texture = part.middledouble.Sidedef.MiddleTexture;
							CheckTextureName(ref texturegeo, ref texture);
							texturegeo[texture].AddRange(OptimizeGeometry(part.middledouble.Vertices, part.middledouble.GeometryType));
						}

						//middle3d
						if(part.middle3d != null && part.middle3d.Count > 0) 
						{
							foreach(VisualMiddle3D m3d in part.middle3d) 
							{
								if(m3d.Vertices == null) continue;
								texture = m3d.GetControlLinedef().Front.MiddleTexture;
								CheckTextureName(ref texturegeo, ref texture);
								texturegeo[texture].AddRange(OptimizeGeometry(m3d.Vertices, m3d.GeometryType));
							}
						}

						//backsides(?)

						//lower
						if(part.lower != null && part.lower.Vertices != null) 
						{
							texture = part.lower.Sidedef.LowTexture;
							CheckTextureName(ref texturegeo, ref texture);
							texturegeo[texture].AddRange(OptimizeGeometry(part.lower.Vertices, part.lower.GeometryType));
						}
					}
				}

				//3d ceilings
				foreach(VisualCeiling vc in vs.ExtraCeilings) 
				{
					texture = vc.GetControlSector().CeilTexture;
					CheckTextureName(ref flatgeo, ref texture);
					flatgeo[texture].AddRange(OptimizeGeometry(vc.Vertices, (vc.ExtraFloor.VavoomType ? vc.GeometryType : VisualModes.VisualGeometryType.FLOOR)));
				}

				//3d floors
				foreach(VisualFloor vf in vs.ExtraFloors) 
				{
					texture = vf.GetControlSector().FloorTexture;
					CheckTextureName(ref flatgeo, ref texture);
					flatgeo[texture].AddRange(OptimizeGeometry(vf.Vertices, (vf.ExtraFloor.VavoomType ? vf.GeometryType : VisualModes.VisualGeometryType.CEILING)));
				}

				//backsides(?)
			}

			return new List<Dictionary<string, List<WorldVertex[]>>> { texturegeo, flatgeo };
		}

		private static void CheckTextureName(ref Dictionary<string, List<WorldVertex[]>> geo, ref string texture) 
		{
			if(!string.IsNullOrEmpty(texture) && texture != "-") 
			{
				if(!geo.ContainsKey(texture)) geo.Add(texture, new List<WorldVertex[]>());
			} 
			else 
			{
				texture = DEFAULT;
			}
		}

		#endregion

		#region ================== Surface optimization

		private static List<WorldVertex[]> OptimizeGeometry(WorldVertex[] verts, VisualModes.VisualGeometryType geotype)
		{
			return OptimizeGeometry(verts, geotype, false);
		}

		private static List<WorldVertex[]> OptimizeGeometry(WorldVertex[] verts, VisualModes.VisualGeometryType geotype, bool skiprectoptimization) 
		{
			List<WorldVertex[]> groups = new List<WorldVertex[]>();

			if(!skiprectoptimization && verts.Length == 6) //rectangular surface
			{
				if(geotype == VisualModes.VisualGeometryType.CEILING) 
				{
					verts = new[] { verts[2], verts[5], verts[1], verts[0] };
				} 
				else 
				{
					verts = new[] { verts[5], verts[2], verts[1], verts[0] };
				}
				groups.Add(verts);
			} 
			else 
			{
				for(int i = 0; i < verts.Length; i += 3) 
				{
					groups.Add(new[] { verts[i + 2], verts[i + 1], verts[i] });
				}
			}

			return groups;
		}

		#endregion

		#region ================== OBJ Creation

		private static StringBuilder CreateObjGeometry(List<Dictionary<string, List<WorldVertex[]>>> geometryByTexture, WavefrontExportSettings data) 
		{
			StringBuilder obj = new StringBuilder();
			const string vertexFormatter = "{0} {2} {1}\n";

			Dictionary<Vector3D, int> uniqueVerts = new Dictionary<Vector3D, int>();
			Dictionary<Vector3D, int> uniqueNormals = new Dictionary<Vector3D, int>();
			Dictionary<PointF, int> uniqueUVs = new Dictionary<PointF, int>();

			var vertexDataByTexture = new Dictionary<string, Dictionary<WorldVertex, VertexIndices>>(StringComparer.Ordinal);
			int pc = 0;
			int nc = 0;
			int uvc = 0;

			//optimize geometry
			foreach(Dictionary<string, List<WorldVertex[]>> dictionary in geometryByTexture) 
			{
				foreach(KeyValuePair<string, List<WorldVertex[]>> group in dictionary) 
				{
					Dictionary<WorldVertex, VertexIndices> vertsData = new Dictionary<WorldVertex, VertexIndices>();
					foreach(WorldVertex[] verts in group.Value) 
					{
						//vertex normals
						Vector3D n = new Vector3D(verts[0].nx, verts[0].ny, verts[0].nz).GetNormal();
						int ni;
						if(uniqueNormals.ContainsKey(n)) 
						{
							ni = uniqueNormals[n];
						} 
						else 
						{
							uniqueNormals.Add(n, ++nc);
							ni = nc;
						}

						foreach(WorldVertex v in verts) 
						{
							if(vertsData.ContainsKey(v)) continue;
							VertexIndices indices = new VertexIndices();
							indices.NormalIndex = ni;

							//vertex coords
							Vector3D vc = new Vector3D(v.x, v.y, v.z);
							if(uniqueVerts.ContainsKey(vc)) 
							{
								indices.PositionIndex = uniqueVerts[vc];
							} 
							else 
							{
								uniqueVerts.Add(vc, ++pc);
								indices.PositionIndex = pc;
							}

							//uv
							PointF uv = new PointF(v.u, v.v);
							if(uniqueUVs.ContainsKey(uv)) 
							{
								indices.UVIndex = uniqueUVs[uv];
							} 
							else 
							{
								uniqueUVs.Add(uv, ++uvc);
								indices.UVIndex = uvc;
							}

							vertsData.Add(v, indices);
						}
					}

					if(vertsData.Count > 0) 
					{
						if(vertexDataByTexture.ContainsKey(group.Key)) 
						{
							foreach(KeyValuePair<WorldVertex, VertexIndices> g in vertsData) 
								vertexDataByTexture[group.Key].Add(g.Key, g.Value);
						} 
						else 
						{
							vertexDataByTexture.Add(group.Key, vertsData);
						}
					}
				}
			}

			//write geometry
			//write vertices
			if(data.FixScale) 
			{
				foreach(KeyValuePair<Vector3D, int> group in uniqueVerts)
					obj.Append(string.Format(CultureInfo.InvariantCulture, "v " + vertexFormatter, -group.Key.x * data.Scale, group.Key.y * data.Scale, group.Key.z * data.Scale * 1.2f));
			} 
			else 
			{
				foreach(KeyValuePair<Vector3D, int> group in uniqueVerts)
					obj.Append(string.Format(CultureInfo.InvariantCulture, "v " + vertexFormatter, -group.Key.x * data.Scale, group.Key.y * data.Scale, group.Key.z * data.Scale));
			}

			//write normals
			foreach(KeyValuePair<Vector3D, int> group in uniqueNormals)
				obj.Append(string.Format(CultureInfo.InvariantCulture, "vn " + vertexFormatter, group.Key.x, group.Key.y, group.Key.z));

			//write UV coords
			foreach(KeyValuePair<PointF, int> group in uniqueUVs)
				obj.Append(string.Format(CultureInfo.InvariantCulture, "vt {0} {1}\n", group.Key.X, -group.Key.Y));

			//write material library
			obj.Append("mtllib ").Append(data.ObjName + ".mtl").Append("\n");

			//write materials and surface indices
			foreach(Dictionary<string, List<WorldVertex[]>> dictionary in geometryByTexture) 
			{
				foreach(KeyValuePair<string, List<WorldVertex[]>> group in dictionary) 
				{
					//material
					obj.Append("usemtl ").Append(group.Key).Append("\n");

					foreach(WorldVertex[] verts in group.Value) 
					{
						//surface indices
						obj.Append("f");
						foreach(WorldVertex v in verts) 
						{
							VertexIndices vi = vertexDataByTexture[group.Key][v];
							obj.Append(" " + vi.PositionIndex + "/" + vi.UVIndex + "/" + vi.NormalIndex);
						}
						obj.Append("\n");
					}
				}
			}

			return obj;
		}

		#endregion
	}
}
