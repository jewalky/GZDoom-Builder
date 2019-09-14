#region ================== Namespaces

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization; // biwa
using System.Text;
using System.Collections.Generic;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.GZBuilder.Data;
using SlimDX;
using SlimDX.Direct3D9;
using CodeImp.DoomBuilder.Geometry;

#endregion

//mxd. Original version taken from here: http://colladadotnet.codeplex.com/SourceControl/changeset/view/40680
namespace CodeImp.DoomBuilder.GZBuilder.MD3
{
	internal static class ModelReader
	{
		#region ================== Variables

		internal class MD3LoadResult
		{
			public List<string> Skins;
			public List<Mesh> Meshes;
			public string Errors;

			public MD3LoadResult()
			{
				Skins = new List<string>();
				Meshes = new List<Mesh>();
			}
		}

		private static readonly VertexElement[] vertexElements = new[]
		{
			new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
			new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
			new VertexElement(0, 16, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
			new VertexElement(0, 24, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
			VertexElement.VertexDeclarationEnd
		};

		#endregion

		#region ================== Load

		public static void Load(ModelData mde, List<DataReader> containers, Device device)
		{
			if(mde.IsVoxel) LoadKVX(mde, containers, device);
			else LoadModel(mde, containers, device);
		}

		private static void LoadKVX(ModelData mde, List<DataReader> containers, Device device)
		{
			mde.Model = new GZModel();
			string unused = string.Empty;
			foreach(string name in mde.ModelNames)
			{
				//find the model
				foreach(DataReader dr in containers)
				{
					Stream ms = dr.GetVoxelData(name, ref unused);
					if(ms == null) continue;

					//load kvx
					ReadKVX(mde, ms, device);

					//done
					ms.Close();
					break;
				}
			}

			//clear unneeded data
			mde.SkinNames = null;
			mde.ModelNames = null;

			if(mde.Model.Meshes == null || mde.Model.Meshes.Count == 0)
			{
				mde.Model = null;
			}
		}

		private static void LoadModel(ModelData mde, List<DataReader> containers, Device device)
		{
			mde.Model = new GZModel();
			BoundingBoxSizes bbs = new BoundingBoxSizes();
			MD3LoadResult result = new MD3LoadResult();

			//load models and textures
			for(int i = 0; i < mde.ModelNames.Count; i++)
			{
				// Use model skins?
				// INFO: Skin MODELDEF property overrides both embedded surface names and ones set using SurfaceSkin MODELDEF property
				Dictionary<int, string> skins = null;
				if(string.IsNullOrEmpty(mde.SkinNames[i]))
				{
					skins = (mde.SurfaceSkinNames[i].Count > 0 ? mde.SurfaceSkinNames[i] : new Dictionary<int, string>());
				}

				// Load mesh
				MemoryStream ms = LoadFile(containers, mde.ModelNames[i], true);
				if(ms == null)
				{
					General.ErrorLogger.Add(ErrorType.Error, "Error while loading \"" + mde.ModelNames[i] + "\": unable to find file.");
					continue;
				}

				string ext = Path.GetExtension(mde.ModelNames[i]);
				switch(ext)
				{
					case ".md3":
						if(!string.IsNullOrEmpty(mde.FrameNames[i]))
						{
							General.ErrorLogger.Add(ErrorType.Error, "Error while loading \"" + mde.ModelNames[i] + "\": frame names are not supported for MD3 models!");
							continue;
						}
						result = ReadMD3Model(ref bbs, skins, ms, device, mde.FrameIndices[i]);
						break;
					case ".md2":
						result = ReadMD2Model(ref bbs, ms, device, mde.FrameIndices[i], mde.FrameNames[i]);
						break;
                    case ".3d":
                        result = Read3DModel(ref bbs, skins, ms, device, mde.FrameIndices[i], mde.ModelNames[i], containers);
                        break;
					case ".obj":
						// OBJ doesn't support frames, so print out an error
						if (mde.FrameIndices[i] > 0)
						{
							General.ErrorLogger.Add(ErrorType.Error, "Trying to load frame " + mde.FrameIndices[i] + " of model \"" + mde.ModelNames[i] + "\", but OBJ doesn't support frames!");
							continue;
						}
						result = ReadOBJModel(ref bbs, skins, ms, device, mde.ModelNames[i]);
						break;
					default:
						result.Errors = "model format is not supported";
						break;
				}

				ms.Close();
                if (result == null)
                    continue;

                //got errors?
                if (!String.IsNullOrEmpty(result.Errors))
				{
					General.ErrorLogger.Add(ErrorType.Error, "Error while loading \"" + mde.ModelNames[i] + "\": " + result.Errors);
				}
				else
				{
					//add loaded data to ModeldefEntry
					mde.Model.Meshes.AddRange(result.Meshes);

					//prepare UnknownTexture3D... just in case :)
					if(General.Map.Data.UnknownTexture3D.Texture == null || General.Map.Data.UnknownTexture3D.Texture.Disposed)
						General.Map.Data.UnknownTexture3D.CreateTexture();

					//load texture
					List<string> errors = new List<string>();

					// Texture not defined in MODELDEF?
					if(skins != null)
					{
						//try to use model's own skins
						for(int m = 0; m < result.Meshes.Count; m++)
						{
							// biwa. Makes sure to add a dummy texture if the MODELDEF skin definition is erroneous
							if(m >= result.Skins.Count)
							{
								errors.Add("no skin defined for mesh " + m + ".");
								mde.Model.Textures.Add(General.Map.Data.UnknownTexture3D.Texture);
								continue;
							}

							if(string.IsNullOrEmpty(result.Skins[m]))
							{
								mde.Model.Textures.Add(General.Map.Data.UnknownTexture3D.Texture);
								errors.Add("texture not found in MODELDEF or model skin.");
								continue;
							}

							string path = result.Skins[m].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

							if(!String.IsNullOrEmpty(mde.Path))
								path = Path.Combine(mde.Path, path);

							Texture t = GetTexture(containers, path, device);

							if(t != null)
							{
								mde.Model.Textures.Add(t);
								continue;
							}

							// That didn't work, let's try to load the texture without the additional path
							path = result.Skins[m].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
							t = GetTexture(containers, path, device);

							if (t == null)
							{
								mde.Model.Textures.Add(General.Map.Data.UnknownTexture3D.Texture);
								errors.Add("unable to load skin \"" + path + "\"");
								continue;
							}

							mde.Model.Textures.Add(t);
						}
					}
					//Try to use texture loaded from MODELDEFS
					else
					{
						Texture t = GetTexture(containers, mde.SkinNames[i], device);

						if(t == null)
						{
							mde.Model.Textures.Add(General.Map.Data.UnknownTexture3D.Texture);
							errors.Add("unable to load skin \"" + mde.SkinNames[i] + "\"");
						}
						else
						{
							mde.Model.Textures.Add(t);
						}
					}

					//report errors
					if(errors.Count > 0)
					{
						foreach(string e in errors)
							General.ErrorLogger.Add(ErrorType.Error, "Error while loading \"" + mde.ModelNames[i] + "\": " + e);
					}
				}
			}

			//clear unneeded data
			mde.SkinNames = null;
			mde.ModelNames = null;

			if(mde.Model.Meshes == null || mde.Model.Meshes.Count == 0)
			{
				mde.Model = null;
				return;
			}

			//scale bbs
			bbs.MaxX = (int)(bbs.MaxX * mde.Scale.X);
			bbs.MinX = (int)(bbs.MinX * mde.Scale.X);
			bbs.MaxY = (int)(bbs.MaxY * mde.Scale.Y);
			bbs.MinY = (int)(bbs.MinY * mde.Scale.Y);

			//calculate model radius
			mde.Model.Radius = Math.Max(Math.Max(Math.Abs(bbs.MinY), Math.Abs(bbs.MaxY)), Math.Max(Math.Abs(bbs.MinX), Math.Abs(bbs.MaxX)));
		}

		private static Texture GetTexture(List<DataReader> containers, string texturename, Device device)
		{
			Texture t = null;
			string[] extensions = new string[ModelData.SUPPORTED_TEXTURE_EXTENSIONS.Length + 1];

			Array.Copy(ModelData.SUPPORTED_TEXTURE_EXTENSIONS, 0, extensions, 1, ModelData.SUPPORTED_TEXTURE_EXTENSIONS.Length);
			extensions[0] = "";

			// Try to load the texture as defined by its path. GZDoom doesn't care about extensions
			if (t == null)
			{
				foreach (string extension in extensions)
				{
					string name = Path.ChangeExtension(texturename, null) + extension;
					name = name.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

					t = LoadTexture(containers, name, device);

					if (t != null)
						break;
				}
			}

			// Try to use an already defined texture. Again, just try out all extensions
			foreach (string extension in extensions)
			{
				string name = Path.ChangeExtension(texturename, null) + extension;

				if (General.Map.Data.GetTextureExists(name))
				{
					ImageData image = General.Map.Data.GetTextureImage(name);

					if (!image.IsImageLoaded)
						image.LoadImage();

					if (image.Texture == null)
						image.CreateTexture();

					t = image.Texture;

					break;
				}
			}

			// GZDoom can also ignore the path completely (because why not), so let's see if there's a texture with
			// just the skin name
			if (t == null)
			{
				string name = Path.ChangeExtension(Path.GetFileName(texturename), null);

				if (General.Map.Data.GetTextureExists(name))
				{
					ImageData image = General.Map.Data.GetTextureImage(name);

					if (!image.IsImageLoaded)
						image.LoadImage();

					if (image.Texture == null)
						image.CreateTexture();

					t = image.Texture;
				}
			}

			// Or maybe it's a sprite
			if(t == null)
			{
				string name = Path.ChangeExtension(texturename, null);

				if (General.Map.Data.GetSpriteExists(name))
				{
					ImageData image = General.Map.Data.GetSpriteImage(name);

					if (!image.IsImageLoaded)
						image.LoadImage();

					if (image.Texture == null)
						image.CreateTexture();

					t = image.Texture;
				}
			}

			return t;
		}

        #endregion

        #region ================== 3D (unreal)

        // there is probably better way to emulate 16-bit cast, but this was easiest for me at 3am
        private static int PadInt16(int n)
        {
            if (n > 32767)
                return -(65536 - n);
            return n;
        }

        private static float UnpackUVertex(int n, int c)
        {
            switch (c)
            {
                case 0:
                    return PadInt16((n & 0x7ff) << 5) / 128f;
                case 1:
                    return PadInt16((((int)n >> 11) & 0x7ff) << 5) / 128f;
                case 2:
                    return PadInt16((((int)n >> 22) & 0x3ff) << 6) / 128f;
                default:
                    return 0f;
            }
        }

        private struct UE1Poly
        {
            public int[] V;
            public float[] S;
            public float[] T;
            public int TexNum, Type;
            public Vector3D Normal;
        }

        internal static MD3LoadResult Read3DModel(ref BoundingBoxSizes bbs, Dictionary<int, string> skins, Stream s, Device device, int frame, string filename, List<DataReader> containers)
        {
            Stream stream_d;
            Stream stream_a;

            if (filename.IndexOf("_d.3d") == filename.Length-5)
            {
                string filename_a = filename.Replace("_d.3d", "_a.3d");
                stream_d = s;
                stream_a = LoadFile(containers, filename_a, true);
                if (stream_a == null)
                {
                    General.ErrorLogger.Add(ErrorType.Error, "Error while loading \"" + filename + "\": unable to find corresponding \"_a.3d\" file.");
                    return null;
                }
            }
            else
            {
                string filename_d = filename.Replace("_a.3d", "_d.3d");
                stream_a = s;
                stream_d = LoadFile(containers, filename_d, true);
                if (stream_d == null)
                {
                    General.ErrorLogger.Add(ErrorType.Error, "Error while loading \"" + filename + "\": unable to find corresponding \"_d.3d\" file.");
                    return null;
                }
            }

            MD3LoadResult result = new MD3LoadResult();
            BinaryReader br_d = new BinaryReader(stream_d);
            BinaryReader br_a = new BinaryReader(stream_a);

            // read d3d header
            uint d3d_numpolys = br_d.ReadUInt16();
            uint d3d_numverts = br_d.ReadUInt16();
            stream_d.Position += 44; // bogusrot, bogusframe, bogusnorm[3], fixscale, unused[3], padding[12]

            long start_d = stream_d.Position;

            // read a3d header
            uint a3d_numframes = br_a.ReadUInt16();
            uint a3d_framesize = br_a.ReadUInt16();

            long start_a = stream_a.Position;

            // Sanity check
            if (frame < 0 || frame >= a3d_numframes)
            {
                result.Errors = "frame " + frame + " is outside of model's frame range [0.." + (a3d_numframes - 1) + "]";
                return result;
            }

            // check for deus ex format
            bool isdeusex = false;
            if ( (a3d_framesize/d3d_numverts) == 8 ) isdeusex = true;

            // read vertices
            WorldVertex[] vertices = new WorldVertex[d3d_numverts];
            for (uint i = 0; i < d3d_numverts; i++)
            {
                WorldVertex Vert = new WorldVertex();
                if ( isdeusex )
                {
                    stream_a.Position = start_a + (i + frame * d3d_numverts) * 8;
                    int vx = br_a.ReadInt16();
                    int vy = br_a.ReadInt16();
                    int vz = br_a.ReadInt16();
                    Vert.y = -vx;
                    Vert.z = vz;
                    Vert.x = -vy;
                }
                else
                {
                    stream_a.Position = start_a + (i + frame * d3d_numverts) * 4;
                    int v_uint = br_a.ReadInt32();
                    Vert.y = -UnpackUVertex(v_uint, 0);
                    Vert.z = UnpackUVertex(v_uint, 2);
                    Vert.x = -UnpackUVertex(v_uint, 1);
                }
                vertices[i] = Vert;
            }

            // read polygons
            //int minverthack = 0;
            //int minvert = 2147483647;
            UE1Poly[] polys = new UE1Poly[d3d_numpolys];
            int[] polyindexlist = new int[d3d_numpolys*3];
            for (uint i = 0; i < d3d_numpolys; i++)
            {
                //
                stream_d.Position = start_d + 16 * i;
                polys[i].V = new int[3];
                polys[i].S = new float[3];
                polys[i].T = new float[3];
                bool brokenpoly = false;
                for (int j = 0; j < 3; j++)
                {
                    polyindexlist[i * 3 + j] = polys[i].V[j] = br_d.ReadInt16();
                    if (polys[i].V[j] >= vertices.Length || polys[i].V[j] < 0)
                        brokenpoly = true;
                }

                // Resolves polygons that reference out-of-bounds vertices by simply making them null size.
                // This is easier than changing array to dynamically sized list.
                if (brokenpoly)
                {
                    polys[i].V[0] = 0;
                    polys[i].V[1] = 0;
                    polys[i].V[2] = 0;
                }

                polys[i].Type = br_d.ReadByte();
                stream_d.Position += 1; // color
                for (int j = 0; j < 3; j++)
                {
                    byte u = br_d.ReadByte();
                    byte v = br_d.ReadByte();
                    polys[i].S[j] = u / 255f;
                    polys[i].T[j] = v / 255f;
                }
                polys[i].TexNum = br_d.ReadByte();
            }

            // calculate poly normals
            for (uint i = 0; i < d3d_numpolys; i++)
            {
                Vector3D[] dir = new Vector3D[2];
                Vector3D norm;
                dir[0].x = vertices[polys[i].V[1]].x-vertices[polys[i].V[0]].x;
                dir[0].y = vertices[polys[i].V[1]].y-vertices[polys[i].V[0]].y;
                dir[0].z = vertices[polys[i].V[1]].z-vertices[polys[i].V[0]].z;
                dir[1].x = vertices[polys[i].V[2]].x-vertices[polys[i].V[0]].x;
                dir[1].y = vertices[polys[i].V[2]].y-vertices[polys[i].V[0]].y;
                dir[1].z = vertices[polys[i].V[2]].z-vertices[polys[i].V[0]].z;
                norm.x = dir[0].y * dir[1].z - dir[0].z * dir[1].y;
                norm.y = dir[0].z * dir[1].x - dir[0].x * dir[1].z;
                norm.z = dir[0].x * dir[1].y - dir[0].y * dir[1].x;
                polys[i].Normal = norm.GetNormal();
            }

            // calculate vertex normals
            for (uint i = 0; i < d3d_numverts; i++)
            {
                Vector3D nsum = new Vector3D(0, 0, 0);
                int total = 0;
                for (uint j = 0; j < d3d_numpolys; j++)
                {
                    if ((polys[j].V[0] != i) && (polys[j].V[1] != i) && (polys[j].V[2] != i)) continue;
                    nsum.x += polys[j].Normal.x;
                    nsum.y += polys[j].Normal.y;
                    nsum.z += polys[j].Normal.z;
                    total++;
                }
                vertices[i].nx = -nsum.x / total;
                vertices[i].ny = -nsum.y / total;
                vertices[i].nz = -nsum.z / total;
            }

            List<int> exGroups = new List<int>();
            Dictionary<int, int> textureGroupRemap = new Dictionary<int, int>();
            for (int i = 0; i < polys.Length; i++)
            {
                if (exGroups.Contains(polys[i].TexNum))
                    continue;
                if (exGroups.Count == 0 ||
                    polys[i].TexNum <= exGroups[0])
                    exGroups.Insert(0, polys[i].TexNum);
                else if (exGroups.Count == 0 ||
                         polys[i].TexNum >= exGroups[exGroups.Count - 1])
                    exGroups.Add(polys[i].TexNum);
            }

            for (int i = 0; i < exGroups.Count; i++)
                textureGroupRemap[exGroups[i]] = i;

            if (skins == null)
            {
                List<WorldVertex> out_verts = new List<WorldVertex>();
                List<int> out_polys = new List<int>();

                for (int i = 0; i < polys.Length; i++)
                {
                    if ( (polys[i].Type&0x08) != 0 )
                        continue;
                    for (int j = 0; j < 3; j++)
                    {
                        WorldVertex vx = vertices[polys[i].V[j]];
                        vx.u = polys[i].S[j];
                        vx.v = polys[i].T[j];
                        if ( (polys[i].Type&0x20) != 0 )
                        {
                            vx.nx = polys[i].Normal.x;
                            vx.ny = polys[i].Normal.y;
                            vx.nz = polys[i].Normal.z;
                        }
                        out_polys.Add(out_verts.Count);
                        out_verts.Add(vx);
                    }
                }

                CreateMesh(device, ref result, out_verts, out_polys);
                result.Skins.Add("");
            }
            else
            {
                for (int k = 0; k < exGroups.Count; k++)
                {
                    List<WorldVertex> out_verts = new List<WorldVertex>();
                    List<int> out_polys = new List<int>();

                    for (int i = 0; i < polys.Length; i++)
                    {
                        if ( (polys[i].Type&0x08) != 0 )
                            continue;

                        if (textureGroupRemap[polys[i].TexNum] != k)
                            continue;

                        for (int j = 0; j < 3; j++)
                        {
                            WorldVertex vx = vertices[polys[i].V[j]];
                            vx.u = polys[i].S[j];
                            vx.v = polys[i].T[j];
                            if ( (polys[i].Type&0x20) != 0 )
                            {
                                vx.nx = polys[i].Normal.x;
                                vx.ny = polys[i].Normal.y;
                                vx.nz = polys[i].Normal.z;
                            }
                            out_polys.Add(out_verts.Count);
                            out_verts.Add(vx);
                        }
                    }

                    CreateMesh(device, ref result, out_verts, out_polys);
                    result.Skins.Add(skins.ContainsKey(k)?skins[k].ToLowerInvariant():string.Empty);
                }
            }

            return result;
        }

        #endregion

        #region ================== MD3

        internal static MD3LoadResult ReadMD3Model(ref BoundingBoxSizes bbs, Dictionary<int, string> skins, Stream s, Device device, int frame)
		{
			long start = s.Position;
			MD3LoadResult result = new MD3LoadResult();

			using(var br = new BinaryReader(s, Encoding.ASCII))
			{
				string magic = ReadString(br, 4);
				if(magic != "IDP3")
				{
					result.Errors = "unknown header: expected \"IDP3\", but got \"" + magic + "\"";
					return result;
				}

				int modelVersion = br.ReadInt32();
				if(modelVersion != 15) //MD3 version. Must be equal to 15
				{
					result.Errors = "expected MD3 version 15, but got " + modelVersion;
					return result;
				}

				s.Position += 76;
				int numSurfaces = br.ReadInt32();
				s.Position += 12;
				int ofsSurfaces = br.ReadInt32();

				s.Position = ofsSurfaces + start;

				List<int> polyIndecesList = new List<int>();
				List<WorldVertex> vertList = new List<WorldVertex>();

				Dictionary<string, List<List<int>>> polyIndecesListsPerTexture = new Dictionary<string, List<List<int>>>(StringComparer.Ordinal);
				Dictionary<string, List<WorldVertex>> vertListsPerTexture = new Dictionary<string, List<WorldVertex>>(StringComparer.Ordinal);
				Dictionary<string, List<int>> vertexOffsets = new Dictionary<string, List<int>>(StringComparer.Ordinal);
				bool useskins = false;

				for(int c = 0; c < numSurfaces; c++)
				{
					string skin = "";
					string error = ReadSurface(ref bbs, ref skin, br, polyIndecesList, vertList, frame);

					if(!string.IsNullOrEmpty(error))
					{
						result.Errors = error;
						return result;
					}

					// Pick a skin to use
					if(skins == null)
					{
						// skins is null when Skin MODELDEF property is set
						skin = string.Empty;
					}
					else if(skins.ContainsKey(c))
					{
						// Overrtide surface skin with SurfaceSkin MODELDEF property
						skin = skins[c];
					}

					if(!string.IsNullOrEmpty(skin))
					{
						useskins = true;

						if(polyIndecesListsPerTexture.ContainsKey(skin))
						{
							polyIndecesListsPerTexture[skin].Add(polyIndecesList);
							vertListsPerTexture[skin].AddRange(vertList.ToArray());
							vertexOffsets[skin].Add(vertList.Count);
						}
						else
						{
							polyIndecesListsPerTexture.Add(skin, new List<List<int>> { polyIndecesList } );
							vertListsPerTexture.Add(skin, vertList);
							vertexOffsets.Add(skin, new List<int> { vertList.Count });
						}

						//reset lists
						polyIndecesList = new List<int>();
						vertList = new List<WorldVertex>();
					}
				}

				if(!useskins)
				{
					//create mesh
					CreateMesh(device, ref result, vertList, polyIndecesList);
					result.Skins.Add("");
				}
				else
				{
					//create a mesh for each surface texture
					foreach(KeyValuePair<string, List<List<int>>> group in polyIndecesListsPerTexture)
					{
						polyIndecesList = new List<int>();
						int offset = 0;

						//collect indices, fix vertex offsets
						for(int i = 0; i < group.Value.Count; i++)
						{
							if(i > 0)
							{
								//TODO: Damn I need to rewrite all of this stuff from scratch...
								offset += vertexOffsets[group.Key][i - 1];
								for(int c = 0; c < group.Value[i].Count; c++)
									group.Value[i][c] += offset;
							}
							polyIndecesList.AddRange(group.Value[i].ToArray());
						}

						CreateMesh(device, ref result, vertListsPerTexture[group.Key], polyIndecesList);
						result.Skins.Add(group.Key.ToLowerInvariant());
					}
				}
			}

			return result;
		}

		private static string ReadSurface(ref BoundingBoxSizes bbs, ref string skin, BinaryReader br, List<int> polyIndecesList, List<WorldVertex> vertList, int frame)
		{
			int vertexOffset = vertList.Count;
			long start = br.BaseStream.Position;

			string magic = ReadString(br, 4);
			if(magic != "IDP3") return "error while reading surface. Unknown header: expected \"IDP3\", but got \"" + magic + "\"";

			string name = ReadString(br, 64);
			int flags = br.ReadInt32();
			int numFrames = br.ReadInt32(); //Number of animation frames. This should match NUM_FRAMES in the MD3 header.
			int numShaders = br.ReadInt32(); //Number of Shader objects defined in this Surface, with a limit of MD3_MAX_SHADERS. Current value of MD3_MAX_SHADERS is 256.
			int numVerts = br.ReadInt32(); //Number of Vertex objects defined in this Surface, up to MD3_MAX_VERTS. Current value of MD3_MAX_VERTS is 4096.
			int numTriangles = br.ReadInt32(); //Number of Triangle objects defined in this Surface, maximum of MD3_MAX_TRIANGLES. Current value of MD3_MAX_TRIANGLES is 8192.
			int ofsTriangles = br.ReadInt32(); //Relative offset from SURFACE_START where the list of Triangle objects starts.
			int ofsShaders = br.ReadInt32();
			int ofsST = br.ReadInt32(); //Relative offset from SURFACE_START where the list of ST objects (s-t texture coordinates) starts.
			int ofsNormal = br.ReadInt32(); //Relative offset from SURFACE_START where the list of Vertex objects (X-Y-Z-N vertices) starts.
			int ofsEnd = br.ReadInt32(); //Relative offset from SURFACE_START to where the Surface object ends.

			// Sanity check
			if(frame < 0 || frame >= numFrames)
			{
				return "frame " + frame + " is outside of model's frame range [0.." + (numFrames - 1) + "]";
			}

			// Polygons
			if(start + ofsTriangles != br.BaseStream.Position)
				br.BaseStream.Position = start + ofsTriangles;

			for(int i = 0; i < numTriangles * 3; i++)
				polyIndecesList.Add(vertexOffset + br.ReadInt32());

			// Shaders
			if(start + ofsShaders != br.BaseStream.Position)
				br.BaseStream.Position = start + ofsShaders;

			skin = ReadString(br, 64); //we are interested only in the first one

			// Vertices
			if(start + ofsST != br.BaseStream.Position)
				br.BaseStream.Position = start + ofsST;

			for(int i = 0; i < numVerts; i++)
			{
				WorldVertex v = new WorldVertex();
				v.c = -1; //white
				v.u = br.ReadSingle();
				v.v = br.ReadSingle();

				vertList.Add(v);
			}

			// Positions and normals
			long vertoffset = start + ofsNormal + numVerts * 8 * frame; // The length of Vertex struct is 8 bytes
			if(br.BaseStream.Position != vertoffset) br.BaseStream.Position = vertoffset;

			for(int i = vertexOffset; i < vertexOffset + numVerts; i++)
			{
				WorldVertex v = vertList[i];

				//read vertex
				v.y = -(float)br.ReadInt16() / 64;
				v.x = (float)br.ReadInt16() / 64;
				v.z = (float)br.ReadInt16() / 64;

				//bounding box
				BoundingBoxTools.UpdateBoundingBoxSizes(ref bbs, v);

				var lat = br.ReadByte() * (2 * Math.PI) / 255.0;
				var lng = br.ReadByte() * (2 * Math.PI) / 255.0;

				v.nx = (float)(Math.Sin(lng) * Math.Sin(lat));
				v.ny = -(float)(Math.Cos(lng) * Math.Sin(lat));
				v.nz = (float)(Math.Cos(lat));

				vertList[i] = v;
			}

			if(start + ofsEnd != br.BaseStream.Position)
				br.BaseStream.Position = start + ofsEnd;
			return "";
		}

		#endregion

		#region ================== MD2

		private static MD3LoadResult ReadMD2Model(ref BoundingBoxSizes bbs, Stream s, Device device, int frame, string framename)
		{
			long start = s.Position;
			MD3LoadResult result = new MD3LoadResult();

			using(var br = new BinaryReader(s, Encoding.ASCII))
			{
				string magic = ReadString(br, 4);
				if(magic != "IDP2") //magic number: "IDP2"
				{
					result.Errors = "unknown header: expected \"IDP2\", but got \"" + magic + "\"";
					return result;
				}

				int modelVersion = br.ReadInt32();
				if(modelVersion != 8) //MD2 version. Must be equal to 8
				{
					result.Errors = "expected MD3 version 15, but got " + modelVersion;
					return result;
				}

				int texWidth = br.ReadInt32();
				int texHeight = br.ReadInt32();
				int framesize = br.ReadInt32(); // Size of one frame in bytes
				s.Position += 4; //Number of textures
				int num_verts = br.ReadInt32(); //Number of vertices
				int num_uv = br.ReadInt32(); //The number of UV coordinates in the model
				int num_tris = br.ReadInt32(); //Number of triangles
				s.Position += 4; //Number of OpenGL commands
				int num_frames = br.ReadInt32(); //Total number of frames

				// Sanity checks
				if(frame < 0 || frame >= num_frames)
				{
					result.Errors = "frame " + frame + " is outside of model's frame range [0.." + (num_frames - 1) + "]";
					return result;
				}

				s.Position += 4; //Offset to skin names (each skin name is an unsigned char[64] and are null terminated)
				int ofs_uv = br.ReadInt32();//Offset to s-t texture coordinates
				int ofs_tris = br.ReadInt32(); //Offset to triangles
				int ofs_animFrame = br.ReadInt32(); //An offset to the first animation frame

				List<int> polyIndecesList = new List<int>();
				List<int> uvIndecesList = new List<int>();
				List<Vector2> uvCoordsList = new List<Vector2>();
				List<WorldVertex> vertList = new List<WorldVertex>();

				// Polygons
				s.Position = ofs_tris + start;

				for(int i = 0; i < num_tris; i++)
				{
					polyIndecesList.Add(br.ReadUInt16());
					polyIndecesList.Add(br.ReadUInt16());
					polyIndecesList.Add(br.ReadUInt16());

					uvIndecesList.Add(br.ReadUInt16());
					uvIndecesList.Add(br.ReadUInt16());
					uvIndecesList.Add(br.ReadUInt16());
				}

				// UV coords
				s.Position = ofs_uv + start;

				for(int i = 0; i < num_uv; i++)
					uvCoordsList.Add(new Vector2((float)br.ReadInt16() / texWidth, (float)br.ReadInt16() / texHeight));

				// Frames
				// Find correct frame
				if(!string.IsNullOrEmpty(framename))
				{
					// Skip frames untill frame name matches
					bool framefound = false;
					for(int i = 0; i < num_frames; i++)
					{
						s.Position = ofs_animFrame + start + i * framesize;
						s.Position += 24; // Skip scale and translate
						string curframename = ReadString(br, 16).ToLowerInvariant();

						if(curframename == framename)
						{
							// Step back so scale and translate can be read
							s.Position -= 40;
							framefound = true;
							break;
						}
					}

					// No dice? Bail out!
					if(!framefound)
					{
						result.Errors = "unable to find frame \"" + framename + "\"!";
						return result;
					}
				}
				else
				{
					// If we have frame number, we can go directly to target frame
					s.Position = ofs_animFrame + start + frame * framesize;
				}

				Vector3 scale = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				Vector3 translate = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

				s.Position += 16; // Skip frame name

				// Prepare to fix rotation angle
				float angleOfsetCos = (float)Math.Cos(-Angle2D.PIHALF);
				float angleOfsetSin = (float)Math.Sin(-Angle2D.PIHALF);

				//verts
				for(int i = 0; i < num_verts; i++)
				{
					WorldVertex v = new WorldVertex();

					v.x = (br.ReadByte() * scale.X + translate.X);
					v.y = (br.ReadByte() * scale.Y + translate.Y);
					v.z = (br.ReadByte() * scale.Z + translate.Z);

					// Fix rotation angle
					float rx = angleOfsetCos * v.x - angleOfsetSin * v.y;
					float ry = angleOfsetSin * v.x + angleOfsetCos * v.y;
					v.y = ry;
					v.x = rx;

					vertList.Add(v);
					s.Position += 1; //vertex normal
				}

				for(int i = 0; i < polyIndecesList.Count; i++)
				{
					WorldVertex v = vertList[polyIndecesList[i]];

					//bounding box
					BoundingBoxTools.UpdateBoundingBoxSizes(ref bbs, new WorldVertex(v.y, v.x, v.z));

					//uv
					float tu = uvCoordsList[uvIndecesList[i]].X;
					float tv = uvCoordsList[uvIndecesList[i]].Y;

					//uv-coordinates already set?
					if(v.c == -1 && (v.u != tu || v.v != tv))
					{
						//add a new vertex
						vertList.Add(new WorldVertex(v.x, v.y, v.z, -1, tu, tv));
						polyIndecesList[i] = vertList.Count - 1;
					}
					else
					{
						v.u = tu;
						v.v = tv;
						v.c = -1; //set color to white

						//return to proper place
						vertList[polyIndecesList[i]] = v;
					}
				}

				//mesh
				Mesh mesh = new Mesh(device, polyIndecesList.Count / 3, vertList.Count, MeshFlags.Use32Bit | MeshFlags.IndexBufferManaged | MeshFlags.VertexBufferManaged, vertexElements);

				using(DataStream stream = mesh.LockVertexBuffer(LockFlags.None))
				{
					stream.WriteRange(vertList.ToArray());
				}
				mesh.UnlockVertexBuffer();

				using(DataStream stream = mesh.LockIndexBuffer(LockFlags.None))
				{
					stream.WriteRange(polyIndecesList.ToArray());
				}
				mesh.UnlockIndexBuffer();

				mesh.OptimizeInPlace(MeshOptimizeFlags.AttributeSort);

				//store in result
				result.Meshes.Add(mesh);
				result.Skins.Add(""); //no skin support for MD2
			}

			return result;
		}

		#endregion

		#region ================== KVX

		private static void ReadKVX(ModelData mde, Stream stream, Device device)
		{
			PixelColor[] palette = new PixelColor[256];
			List<WorldVertex> verts = new List<WorldVertex>();
			List<int> indices = new List<int>();
			Dictionary<long, int> verthashes = new Dictionary<long, int>();
			int xsize, ysize, zsize;
			int facescount = 0;
			Vector3D pivot;

			using(BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
			{
				reader.ReadInt32(); //numbytes, we don't use that
				xsize = reader.ReadInt32();
				ysize = reader.ReadInt32();
				zsize = reader.ReadInt32();

				pivot = new Vector3D();
				pivot.x = reader.ReadInt32() / 256f;
				pivot.y = reader.ReadInt32() / 256f;
				pivot.z = reader.ReadInt32() / 256f;

				//read offsets
				int[] xoffset = new int[xsize + 1]; //why is it xsize + 1, not xsize?..
				short[,] xyoffset = new short[xsize, ysize + 1]; //why is it ysize + 1, not ysize?..

				for(int i = 0; i < xoffset.Length; i++)
				{
					xoffset[i] = reader.ReadInt32();
				}

				for(int x = 0; x < xsize; x++)
				{
					for(int y = 0; y < ysize + 1; y++)
					{
						xyoffset[x, y] = reader.ReadInt16();
					}
				}

				//read slabs
				List<int> offsets = new List<int>(xsize * ysize);
				for(int x = 0; x < xsize; x++)
				{
					for(int y = 0; y < ysize; y++)
					{
						offsets.Add(xoffset[x] + xyoffset[x, y] + 28); //for some reason offsets are counted from start of xoffset[]...
					}
				}

				int counter = 0;
				int slabsEnd = (int)(reader.BaseStream.Length - 768);

				//read palette
				if(!mde.OverridePalette)
				{
					reader.BaseStream.Position = slabsEnd;
					for(int i = 0; i < 256; i++)
					{
						byte r = (byte)(reader.ReadByte() * 4);
						byte g = (byte)(reader.ReadByte() * 4);
						byte b = (byte)(reader.ReadByte() * 4);
						palette[i] = new PixelColor(255, r, g, b);
					}
				}
				else
				{
					for(int i = 0; i < 256; i++ )
					{
						palette[i] = General.Map.Data.Palette[i];
					}
				}

				for(int x = 0; x < xsize; x++)
				{
					for(int y = 0; y < ysize; y++)
					{
						reader.BaseStream.Position = offsets[counter];
						int next = (counter < offsets.Count - 1 ? offsets[counter + 1] : slabsEnd);

						//read slab
						while(reader.BaseStream.Position < next)
						{
							int ztop = reader.ReadByte();
							int zleng = reader.ReadByte();
							if(ztop + zleng > zsize) break;
							int flags = reader.ReadByte();

							if(zleng > 0)
							{
								List<int> colorIndices = new List<int>(zleng);
								for(int i = 0; i < zleng; i++)
								{
									colorIndices.Add(reader.ReadByte());
								}

								if((flags & 16) != 0)
								{
									AddFace(verts, indices, verthashes, new Vector3D(x, y, ztop), new Vector3D(x + 1, y, ztop), new Vector3D(x, y + 1, ztop), new Vector3D(x + 1, y + 1, ztop), pivot, colorIndices[0]);
									facescount += 2;
								}

								int z = ztop;
								int cstart = 0;
								while(z < ztop + zleng)
								{
									int c = 0;
									while(z + c < ztop + zleng && colorIndices[cstart + c] == colorIndices[cstart]) c++;

									if((flags & 1) != 0)
									{
										AddFace(verts, indices, verthashes, new Vector3D(x, y, z), new Vector3D(x, y + 1, z), new Vector3D(x, y, z + c), new Vector3D(x, y + 1, z + c), pivot, colorIndices[cstart]);
										facescount += 2;
									}
									if((flags & 2) != 0)
									{
										AddFace(verts, indices, verthashes, new Vector3D(x + 1, y + 1, z), new Vector3D(x + 1, y, z), new Vector3D(x + 1, y + 1, z + c), new Vector3D(x + 1, y, z + c), pivot, colorIndices[cstart]);
										facescount += 2;
									}
									if((flags & 4) != 0)
									{
										AddFace(verts, indices, verthashes, new Vector3D(x + 1, y, z), new Vector3D(x, y, z), new Vector3D(x + 1, y, z + c), new Vector3D(x, y, z + c), pivot, colorIndices[cstart]);
										facescount += 2;
									}
									if((flags & 8) != 0)
									{
										AddFace(verts, indices, verthashes, new Vector3D(x, y + 1, z), new Vector3D(x + 1, y + 1, z), new Vector3D(x, y + 1, z + c), new Vector3D(x + 1, y + 1, z + c), pivot, colorIndices[cstart]);
										facescount += 2;
									}

									if(c == 0) c++;
									z += c;
									cstart += c;
								}

								if((flags & 32) != 0)
								{
									z = ztop + zleng - 1;
									AddFace(verts, indices, verthashes, new Vector3D(x + 1, y, z + 1), new Vector3D(x, y, z + 1), new Vector3D(x + 1, y + 1, z + 1), new Vector3D(x, y + 1, z + 1), pivot, colorIndices[zleng - 1]);
									facescount += 2;
								}
							}
						}

						counter++;
					}
				}
			}

			// get model extents
			int minX = (int)((xsize / 2f - pivot.x) * mde.Scale.X);
			int maxX = (int)((xsize / 2f + pivot.x) * mde.Scale.X);
			int minY = (int)((ysize / 2f - pivot.y) * mde.Scale.Y);
			int maxY = (int)((ysize / 2f + pivot.y) * mde.Scale.Y);

			// Calculate model radius
			mde.Model.Radius = Math.Max(Math.Max(Math.Abs(minY), Math.Abs(maxY)), Math.Max(Math.Abs(minX), Math.Abs(maxX)));

			// Create texture
			MemoryStream memstream = new MemoryStream((4096 * 4) + 4096);
			using(Bitmap bmp = CreateVoxelTexture(palette)) bmp.Save(memstream, ImageFormat.Bmp);
			memstream.Seek(0, SeekOrigin.Begin);

			Texture texture = Texture.FromStream(device, memstream, (int)memstream.Length, 64, 64, 0, Usage.None, Format.Unknown, Pool.Managed, Filter.Point, Filter.Box, 0);
			memstream.Dispose();

			// Add texture
			mde.Model.Textures.Add(texture);

			// Create mesh
			MeshFlags meshflags = MeshFlags.Managed;
			if(indices.Count > ushort.MaxValue - 1) meshflags |= MeshFlags.Use32Bit;

			Mesh mesh = new Mesh(device, facescount, verts.Count, meshflags, vertexElements);

			DataStream mstream = mesh.VertexBuffer.Lock(0, 0, LockFlags.None);
			mstream.WriteRange(verts.ToArray());
			mesh.VertexBuffer.Unlock();

			mstream = mesh.IndexBuffer.Lock(0, 0, LockFlags.None);

			if(indices.Count > ushort.MaxValue - 1)
				mstream.WriteRange(indices.ToArray());
			else
				foreach(int index in indices) mstream.Write((ushort)index);

			mesh.IndexBuffer.Unlock();

			mesh.OptimizeInPlace(MeshOptimizeFlags.AttributeSort);

			// Add mesh
			mde.Model.Meshes.Add(mesh);
		}

		// Shameless GZDoom rip-off
		private static void AddFace(List<WorldVertex> verts, List<int> indices, Dictionary<long, int> hashes, Vector3D v1, Vector3D v2, Vector3D v3, Vector3D v4, Vector3D pivot, int colorIndex)
		{
			float pu0 = (colorIndex % 16) / 16f;
			float pu1 = pu0 + 0.001f;
			float pv0 = (colorIndex / 16) / 16f;
			float pv1 = pv0 + 0.001f;

			WorldVertex wv1 = new WorldVertex
			{
				x = v1.x - pivot.x,
				y = -v1.y + pivot.y,
				z = -v1.z + pivot.z,
				c = -1,
				u = pu0,
				v = pv0
			};
			int i1 = AddVertex(wv1, verts, indices, hashes);

			WorldVertex wv2 = new WorldVertex
			{
				x = v2.x - pivot.x,
				y = -v2.y + pivot.y,
				z = -v2.z + pivot.z,
				c = -1,
				u = pu1,
				v = pv1
			};
			AddVertex(wv2, verts, indices, hashes);

			WorldVertex wv4 = new WorldVertex
			{
				x = v4.x - pivot.x,
				y = -v4.y + pivot.y,
				z = -v4.z + pivot.z,
				c = -1,
				u = pu0,
				v = pv0
			};
			int i4 = AddVertex(wv4, verts, indices, hashes);

			WorldVertex wv3 = new WorldVertex
			{
				x = v3.x - pivot.x,
				y = -v3.y + pivot.y,
				z = -v3.z + pivot.z,
				c = -1,
				u = pu1,
				v = pv1
			};
			AddVertex(wv3, verts, indices, hashes);

			indices.Add(i1);
			indices.Add(i4);
		}

		// Returns index of added vert
		private static int AddVertex(WorldVertex v, List<WorldVertex> verts, List<int> indices, Dictionary<long, int> hashes)
		{
			long hash;
			unchecked // Overflow is fine, just wrap
			{
				hash = 2166136261;
				hash = (hash * 16777619) ^ v.x.GetHashCode();
				hash = (hash * 16777619) ^ v.y.GetHashCode();
				hash = (hash * 16777619) ^ v.z.GetHashCode();
				hash = (hash * 16777619) ^ v.u.GetHashCode();
				hash = (hash * 16777619) ^ v.v.GetHashCode();
			}

			if(hashes.ContainsKey(hash))
			{
				indices.Add(hashes[hash]);
				return hashes[hash];
			}
			else
			{
				verts.Add(v);
				hashes.Add(hash, verts.Count - 1);
				indices.Add(verts.Count - 1);
				return verts.Count - 1;
			}
		}

		private unsafe static Bitmap CreateVoxelTexture(PixelColor[] palette)
		{
			Bitmap bmp = new Bitmap(16, 16);
			BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, 16, 16), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			if(bmpdata != null)
			{
				PixelColor* pixels = (PixelColor*)(bmpdata.Scan0.ToPointer());
				const int numpixels = 256;
				int i = 255;

				for(PixelColor* cp = pixels + numpixels - 1; cp >= pixels; cp--, i--)
				{
					cp->r = palette[i].r;
					cp->g = palette[i].g;
					cp->b = palette[i].b;
					cp->a = palette[i].a;
				}
				bmp.UnlockBits(bmpdata);
			}

			//scale bitmap, so colors stay (almost) the same when bilinear filtering is enabled
			Bitmap scaled = new Bitmap(64, 64);
			using(Graphics gs = Graphics.FromImage(scaled))
			{
				gs.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				gs.DrawImage(bmp, new Rectangle(0, 0, 64, 64), new Rectangle(0, 0, 16, 16), GraphicsUnit.Pixel);
			}
			bmp.Dispose();

			return scaled;
		}

		#endregion

		#region ================== OBJ

		private static MD3LoadResult ReadOBJModel(ref BoundingBoxSizes bbs, Dictionary<int, string> skins, Stream s, Device device, string name)
		{
			MD3LoadResult result = new MD3LoadResult();

			using (var reader = new StreamReader(s, Encoding.ASCII))
			{
				string line;
				int linenum = 1;
				string message;
				int surfaceskinid = 0;
				List<Vector3D> vertices = new List<Vector3D>();
				List<int> faces = new List<int>();
				List<Vector3D> normals = new List<Vector3D>();
				List<Vector2D> texcoords = new List<Vector2D>();
				List<WorldVertex> worldvertices = new List<WorldVertex>();
				List<int> polyindiceslist = new List<int>();

				while ((line = reader.ReadLine()) != null) {
					string[] fields = line.Trim().Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);

					if (fields.Length == 0) continue; // Empty line
					if (fields[0].Trim() == "#") continue; // Comment

					string keyword = fields[0].Trim();
					string payload = null;

					if (fields.Length == 2)
						 payload = fields[1].Trim();

					switch(keyword)
					{
						case "v":
							Vector3D v = new Vector3D(0, 0, 0);

							if (OBJParseVertex(payload, ref v, out message))
								vertices.Add(v);
							else
							{
								result.Errors = String.Format("Error in line {0}: {1}", linenum, message);
								return result;
							}

							break;
						case "vt":
							Vector2D t = new Vector2D(0, 0);

							if (OBJParseTextureCoords(payload, ref t, out message))
								texcoords.Add(t);
							else
							{
								result.Errors = String.Format("Error in line {0}: {1}", linenum, message);
								return result;
							}

							break;
						case "vn":
							Vector3D n = new Vector3D(0, 0, 0);

							if (OBJParseNormal(payload, ref n, out message))
								normals.Add(n);
							else
							{
								result.Errors = String.Format("Error in line {0}: {1}", linenum, message);
								return result;
							}

							break;
						case "f":
							List<int> fv = new List<int>();
							List<int> vt = new List<int>();
							List<int> vn = new List<int>();

							if (OBJParseFace(payload, ref fv, ref vt, ref vn, out message))
							{
								// Sanity check for vertices
								for (int i=0; i < fv.Count; i++)
									if(fv[i] != -1 && fv[i] > vertices.Count)
									{
										result.Errors = String.Format("Error in line {0}: vertex {1} does not exist", linenum, i + 1);
										return result;
									}

								// Sanity check for texture coordinates
								for (int i=0; i < vt.Count; i++)
									if(vt[i] != -1 && vt[i] > texcoords.Count)
									{
										result.Errors = String.Format("Error in line {0}: texture coordinate {1} does not exist", linenum, i + 1);
										return result;
									}

								// Sanity check for normals
								for (int i = 0; i < vn.Count; i++)
									if (vn[i] != -1 && vn[i] > normals.Count)
									{
										result.Errors = String.Format("Error in line {0}: vertex {1} does not exist", linenum, i + 1);
										return result;
									}

								int[] seq;

								// If the face is a quad split it into two triangles
								if (fv.Count == 3)
									seq = new int[] { 0, 1, 2 };
								else
									seq = new int[] { 0, 1, 2, 0, 2, 3 };

								for (int i = 0; i < seq.Length; i++) {
									WorldVertex wc = new WorldVertex(vertices[fv[seq[i]]]);

									if(vt[seq[i]] != -1)
									{
										wc.u = texcoords[vt[seq[i]]].x;
										wc.v = texcoords[vt[seq[i]]].y;
									}

									if (vn[seq[i]] != -1)
									{
										wc.nx = normals[vn[seq[i]]].x;
										wc.ny = normals[vn[seq[i]]].y;
										wc.nz = normals[vn[seq[i]]].z;
									}

									BoundingBoxTools.UpdateBoundingBoxSizes(ref bbs, wc);

									worldvertices.Add(wc);
									polyindiceslist.Add(polyindiceslist.Count);
								}
							}
							else
							{
								result.Errors = String.Format("Error in line {0}: {1}", linenum, message);
								return result;
							}

							break;
						case "usemtl":
							// If there's a new texture defined create a mesh from the current faces and
							// start a gather new faces for the next mesh
							if(worldvertices.Count > 0)
							{
								CreateMesh(device, ref result, worldvertices, polyindiceslist);
								worldvertices.Clear();
								polyindiceslist.Clear();
							}
							if (fields.Length >= 2)
								result.Skins.Add(fields[1]);
							
							surfaceskinid++;
							break;
						case "": // Empty line
						case "#": // Line is a comment
						case "s": // Smooth
						case "g": // Group
						case "o": // Object
						default:
							break;
					}

					linenum++;
				}

				CreateMesh(device, ref result, worldvertices, polyindiceslist);

				// Overwrite internal textures with SurfaceSkin definitions if necessary
				if (skins != null)
				{
					foreach (KeyValuePair<int, string> group in skins)
					{
						// Add dummy skins if necessary
						while (result.Skins.Count <= group.Key)
							result.Skins.Add(String.Empty);

						result.Skins[group.Key] = group.Value;
					}
				}
			}

			return result;
		}

		private static bool OBJParseVertex(string payload, ref Vector3D v, out string message)
		{
			if(String.IsNullOrEmpty(payload))
			{
				message = "no arguments given";
				return false;
			}

			string[] fields = payload.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			if(fields.Length < 3)
			{
				message = "too few arguments";
				return false;
			}

			try
			{
				v.x = float.Parse(fields[0], CultureInfo.InvariantCulture);
				v.z = float.Parse(fields[1], CultureInfo.InvariantCulture);
				v.y = -float.Parse(fields[2], CultureInfo.InvariantCulture);


				// Prepare to fix rotation angle
				float angleOfsetCos = (float)Math.Cos(-Angle2D.PIHALF);
				float angleOfsetSin = (float)Math.Sin(-Angle2D.PIHALF);
				
				// Fix rotation angle
				float rx = angleOfsetCos * v.x - angleOfsetSin * v.y;
				float ry = angleOfsetSin * v.x + angleOfsetCos * v.y;
				v.x = rx;
				v.y = ry;
			}
			catch (FormatException)
			{
				message = "field is not a float";
				return false;
			}

			message = "";
			return true;
		}

		private static bool OBJParseTextureCoords(string payload, ref Vector2D t, out string message)
		{
			if (String.IsNullOrEmpty(payload))
			{
				message = "no arguments given";
				return false;
			}

			string[] fields = payload.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			if (fields.Length < 2)
			{
				message = "too few arguments";
				return false;
			}

			try
			{
				t.x = float.Parse(fields[0], CultureInfo.InvariantCulture);

				if (fields.Length == 2)
					t.y = float.Parse(fields[1], CultureInfo.InvariantCulture);
				else
					t.y = 0.0f;
			}
			catch (FormatException)
			{
				message = "field is not a float";
				return false;
			}

			message = "";
			return true;
		}

		private static bool OBJParseNormal(string payload, ref Vector3D normal, out string message)
		{
			if (String.IsNullOrEmpty(payload))
			{
				message = "no arguments given";
				return false;
			}

			string[] fields = payload.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			if (fields.Length < 3)
			{
				message = "too few arguments";
				return false;
			}

			try
			{
				normal.x = float.Parse(fields[0], CultureInfo.InvariantCulture);
				normal.y = float.Parse(fields[1], CultureInfo.InvariantCulture);
				normal.z = float.Parse(fields[2], CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				message = "field is not a float";
				return false;
			}

			message = "";
			return true;
		}

		private static bool OBJParseFace(string payload, ref List<int> face, ref List<int> texcoords, ref List<int> normals, out string message)
		{
			if (String.IsNullOrEmpty(payload))
			{
				message = "no arguments given";
				return false;
			}

			string[] fields = payload.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			if (fields.Length < 3)
			{
				message = "too few arguments";
				return false;
			}

			if(fields.Length > 4)
			{
				message = "faces with more than 4 sides are not supported";
				return false;
			}

			try
			{
				for (int i = 0; i < fields.Length; i++)
				{
					string[] vertexdata = fields[i].Split('/');

					face.Add(int.Parse(vertexdata[0], CultureInfo.InvariantCulture) - 1);

					if (vertexdata.Length > 1 && vertexdata[1] != "")
						texcoords.Add(int.Parse(vertexdata[1], CultureInfo.InvariantCulture) - 1);
					else
						texcoords.Add(-1);

					if (vertexdata.Length > 2 && vertexdata[2] != "")
						normals.Add(int.Parse(vertexdata[2], CultureInfo.InvariantCulture) - 1);
					else
						normals.Add(-1);
				}
			}
			catch(FormatException)
			{
				message = "field is not an integer";
				return false;
			}

			message = "";
			return true;
		}

		#endregion

		#region ================== Utility

		private static MemoryStream LoadFile(List<DataReader> containers, string path, bool isModel)
		{
			foreach(DataReader dr in containers)
			{
				if(isModel && dr is WADReader) continue;  //models cannot be stored in WADs

				//load file
				if(dr.FileExists(path)) return dr.LoadFile(path);
			}
			return null;
		}

		private static Texture LoadTexture(List<DataReader> containers, string path, Device device)
		{
			if(string.IsNullOrEmpty(path)) return null;

			MemoryStream ms = LoadFile(containers, path, true);
			if(ms == null) return null;

			Texture texture = null;

			//create texture
			if(Path.GetExtension(path) == ".pcx") //pcx format requires special handling...
			{
				FileImageReader fir = new FileImageReader();
				Bitmap bitmap = fir.ReadAsBitmap(ms);

				ms.Close();

				if(bitmap != null)
				{
					BitmapData bmlock = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
					texture = new Texture(device, bitmap.Width, bitmap.Height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

					DataRectangle textureLock = texture.LockRectangle(0, LockFlags.None);
					textureLock.Data.WriteRange(bmlock.Scan0, bmlock.Height * bmlock.Stride);

					bitmap.UnlockBits(bmlock);
					texture.UnlockRectangle(0);
				}
			}
			else
			{
				texture = Texture.FromStream(device, ms);

				ms.Close();
			}

			return texture;
		}

		private static void CreateMesh(Device device, ref MD3LoadResult result, List<WorldVertex> verts, List<int> indices)
		{
			//create mesh
			Mesh mesh = new Mesh(device, indices.Count / 3, verts.Count, MeshFlags.Use32Bit | MeshFlags.IndexBufferManaged | MeshFlags.VertexBufferManaged, vertexElements);

			TextureAddress ta = (TextureAddress)device.GetSamplerState(0, SamplerState.AddressU);

			using (DataStream stream = mesh.LockVertexBuffer(LockFlags.None))
			{
				stream.WriteRange(verts.ToArray());
			}
			mesh.UnlockVertexBuffer();

			using(DataStream stream = mesh.LockIndexBuffer(LockFlags.None))
			{
				stream.WriteRange(indices.ToArray());
			}
			mesh.UnlockIndexBuffer();

			mesh.OptimizeInPlace(MeshOptimizeFlags.AttributeSort);

			//store in result
			result.Meshes.Add(mesh);
		}

		private static string ReadString(BinaryReader br, int len)
		{
			string result = string.Empty;
			int i;

			for(i = 0; i < len; ++i)
			{
				var c = br.ReadChar();
				if(c == '\0')
				{
					++i;
					break;
				}
				result += c;
			}

			for(; i < len; ++i) br.ReadChar();
			return result;
		}

		#endregion
	}
}
