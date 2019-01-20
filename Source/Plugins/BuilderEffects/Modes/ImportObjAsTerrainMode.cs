#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Editing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;
using System.IO;
using System.Globalization;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderEffects
{
	[EditMode(DisplayName = "Terrain Importer",
			  SwitchAction = "importobjasterrain",
			  Volatile = true,
			  UseByDefault = true,
			  AllowCopyPaste = false)]
	public class ImportObjAsTerrainMode : ClassicMode
	{
		#region ================== Constants

		private readonly static char[] space = { ' ' };
		private const string slash = "/";
		internal const int VERTEX_HEIGHT_THING_TYPE = 1504;

		#endregion

		#region ================== Variables

		private struct Face
		{
			public readonly Vector3D V1;
			public readonly Vector3D V2;
			public readonly Vector3D V3;

			public Face(Vector3D v1, Vector3D v2, Vector3D v3) 
			{
				V1 = v1;
				V2 = v2;
				V3 = v3;
			}
		}

		private readonly ObjImportSettingsForm form;

		#endregion

		#region ================== Properties

		internal enum UpAxis
		{
			Y,
			Z,
			X
		}

		#endregion

		#region ================== Constructor

		public ImportObjAsTerrainMode() 
		{
			form = new ObjImportSettingsForm();
		}

		#endregion

		#region ================== Methods

		public override void OnEngage() 
		{
			base.OnEngage();
			General.Map.Map.ClearAllSelected();

			//show interface
			if(form.ShowDialog() == DialogResult.OK && File.Exists(form.FilePath)) 
			{
				OnAccept();
			} 
			else 
			{
				OnCancel();
			}
		}

		public override void OnAccept() 
		{
			Cursor.Current = Cursors.AppStarting;
			General.Interface.DisplayStatus(StatusType.Busy, "Creating geometry...");

			// Collections! Everyone loves them!
			List<Vector3D> verts = new List<Vector3D>(12);
			List<Face> faces = new List<Face>(4);
			int minZ = int.MaxValue;
			int maxZ = int.MinValue;

			// Read .obj, create and select sectors 
			if(!ReadGeometry(form.FilePath, form.ObjScale, form.UpAxis, verts, faces, ref minZ, ref maxZ) || !CreateGeometry(verts, faces, maxZ + (maxZ - minZ)/2)) 
			{
				// Fial!
				Cursor.Current = Cursors.Default;

				// Return to base mode
				General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
				return;
			}
			
			// Update caches
			General.Map.Map.SnapAllToAccuracy();
			General.Map.Map.UpdateConfiguration();
			General.Map.Map.Update();
			General.Map.IsChanged = true;

			// Update things filter so that it includes added things
			if(form.UseVertexHeights && !General.Map.UDMF) General.Map.ThingsFilter.Update();

			// Done
			Cursor.Current = Cursors.Default;

			// Switch to Edit Selection mode
			General.Editing.ChangeMode("EditSelectionMode", true);
		}

		public override void OnCancel() 
		{
			// Cancel base class
			base.OnCancel();

			// Return to base mode
			General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
		}

		#endregion

		#region ================== Geometry creation

		private bool CreateGeometry(List<Vector3D> verts, List<Face> faces, int maxZ) 
		{
			if(verts.Count < 3 || faces.Count == 0)
			{
				MessageBox.Show("Cannot import the model: failed to find any suitable polygons!",
					"Terrain Importer", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			
			MapSet map = General.Map.Map;
			
			// Capacity checks
			int totalverts = map.Vertices.Count + verts.Count;
			int totalsides = map.Sidedefs.Count + faces.Count * 6;
			int totallines = map.Linedefs.Count + faces.Count * 3;
			int totalsectors = map.Sectors.Count + faces.Count;
			int totalthings = 0;
			if(form.UseVertexHeights && !General.Map.UDMF) 
			{
				// We'll use vertex height things in non-udmf maps, if such things are defined in Game Configuration
				totalthings = map.Things.Count + verts.Count;
			}

			if(totalverts > General.Map.FormatInterface.MaxVertices) 
			{
				MessageBox.Show("Cannot import the model: resulting vertex count (" + totalverts 
					+ ") is larger than map format's maximum (" + General.Map.FormatInterface.MaxVertices + ")!", 
					"Terrain Importer", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			if(totalsides > General.Map.FormatInterface.MaxSidedefs) 
			{
				MessageBox.Show("Cannot import the model: resulting sidedefs count (" + totalsides 
					+ ") is larger than map format's maximum (" + General.Map.FormatInterface.MaxSidedefs + ")!", 
					"Terrain Importer", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			if(totallines > General.Map.FormatInterface.MaxLinedefs) 
			{
				MessageBox.Show("Cannot import the model: resulting sidedefs count (" + totallines 
					+ ") is larger than map format's maximum (" + General.Map.FormatInterface.MaxLinedefs + ")!", 
					"Terrain Importer", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			if(totalsectors > General.Map.FormatInterface.MaxSectors) 
			{
				MessageBox.Show("Cannot import the model: resulting sidedefs count (" + totalsectors 
					+ ") is larger than map format's maximum (" + General.Map.FormatInterface.MaxSectors + ")!", 
					"Terrain Importer", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			if(totalthings > General.Map.FormatInterface.MaxThings) 
			{
				MessageBox.Show("Cannot import the model: resulting things count (" + totalthings
					+ ") is larger than map format's maximum (" + General.Map.FormatInterface.MaxThings + ")!",
					"Terrain Importer", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			
			//make undo
			General.Map.UndoRedo.CreateUndo("Import Terrain");

			//prepare mapset
			List<Linedef> newlines = new List<Linedef>();
			map.BeginAddRemove();
			map.SetCapacity(totalverts, totallines, totalsides, totalsectors, totalthings);

			//terrain has many faces... let's create them
			Dictionary<Vector3D, Vertex> newverts = new Dictionary<Vector3D, Vertex>();
			foreach(Face face in faces)
			{
				// Create sector
				Sector s = map.CreateSector();
				s.Selected = true;
				s.FloorHeight = (int)Math.Round((face.V1.z + face.V2.z + face.V3.z) / 3);
				s.CeilHeight = maxZ;
				s.Brightness = General.Settings.DefaultBrightness; //todo: allow user to change this
				s.SetCeilTexture(General.Map.Config.SkyFlatName);
				s.SetFloorTexture(General.Map.Options.DefaultFloorTexture); //todo: allow user to change this

				// And linedefs
				Linedef newline = GetLine(newverts, s, face.V1, face.V2);
				if(newline != null) newlines.Add(newline);

				newline = GetLine(newverts, s, face.V2, face.V3);
				if(newline != null) newlines.Add(newline);

				newline = GetLine(newverts, s, face.V3, face.V1);
				if(newline != null) newlines.Add(newline);

				s.UpdateCache();
			}

			// Add slope things
			if(form.UseVertexHeights && !General.Map.UDMF) 
			{
				foreach(Vector3D pos in newverts.Keys) 
				{
					Thing t = map.CreateThing();
					General.Settings.ApplyDefaultThingSettings(t);
					t.Type = VERTEX_HEIGHT_THING_TYPE;
					t.Move(pos);
					t.Selected = true;
				}
			}

			//update new lines
			foreach(Linedef l in newlines) l.ApplySidedFlags();

			map.EndAddRemove();
			return true;
		}

		private Linedef GetLine(Dictionary<Vector3D, Vertex> verts, Sector sector, Vector3D v1, Vector3D v2) 
		{
			Linedef line = null;

			//get start and end verts
			Vertex start = GetVertex(verts, v1);
			Vertex end = GetVertex(verts, v2);

			//check if the line is already created
			foreach(Linedef l in start.Linedefs)
			{
				if(l.End == end || l.Start == end) 
				{
					line = l;
					break;
				}
			}

			//create a new line?
			Sidedef side;
			if(line == null) 
			{
				line = General.Map.Map.CreateLinedef(start, end);

				//create front sidedef and attach sector to it
				side = General.Map.Map.CreateSidedef(line, true, sector);
			} 
			else 
			{
				//create back sidedef and attach sector to it
				side = General.Map.Map.CreateSidedef(line, false, sector);
			}

			if(!form.UseVertexHeights) side.SetTextureLow(General.Map.Options.DefaultWallTexture);
			line.Selected = true;
			return line;
		}

		private Vertex GetVertex(Dictionary<Vector3D, Vertex> verts, Vector3D pos) 
		{
			//already there?
			if(verts.ContainsKey(pos)) return verts[pos];
			
			//make a new one
			Vertex v = General.Map.Map.CreateVertex(pos);
			if(form.UseVertexHeights) v.ZFloor = pos.z;
			verts.Add(pos, v);
			return v;
		}

		#endregion

		#region ================== .obj import

		private static bool ReadGeometry(string path, float scale, UpAxis axis, List<Vector3D> verts, List<Face> faces, ref int minZ, ref int maxZ) 
		{
			using(StreamReader reader = File.OpenText(path)) 
			{
				string line;
				int counter = 0;
				float picoarse = (float)Math.Round(Angle2D.PI, 3);

				while((line = reader.ReadLine()) != null) 
				{
					counter++;

					if(line.StartsWith("v ")) 
					{
						string[] parts = line.Split(space, StringSplitOptions.RemoveEmptyEntries);

						float x, y, z;
						if(parts.Length != 4 || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x) ||
							!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out y) ||
							!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out z)) 
						{
							MessageBox.Show("Failed to parse vertex definition at line " + counter + "!", "Terrain Importer", MessageBoxButtons.OK, MessageBoxIcon.Error);
							return false;
						}

						//apply up axis
						int px, py, pz;
						switch(axis) 
						{
							case UpAxis.Y:
								px = (int)Math.Round(x * scale);
								py = (int)Math.Round(-z * scale);
								pz = (int)Math.Round(y * scale);
								break;

							case UpAxis.Z:
								px = (int)Math.Round(-x * scale);
								py = (int)Math.Round(-y * scale);
								pz = (int)Math.Round(z * scale);
								break;

							case UpAxis.X:
								px = (int)Math.Round(-y * scale);
								py = (int)Math.Round(-z * scale);
								pz = (int)Math.Round(x * scale);
								break;

							default: //same as UpAxis.Y
								px = (int)Math.Round(x * scale);
								py = (int)Math.Round(-z * scale);
								pz = (int)Math.Round(y * scale);
								break;
						}

						if(maxZ < pz) maxZ = pz;
						if(minZ > pz) minZ = pz;

						verts.Add(new Vector3D(px, py, pz));
					} 
					else if(line.StartsWith("f ")) 
					{
						string[] parts = line.Split(space, StringSplitOptions.RemoveEmptyEntries);

						if(parts.Length != 4) 
						{
							MessageBox.Show("Failed to parse face definition at line " + counter + ": only triangular faces are supported!", "Terrain Importer", MessageBoxButtons.OK, MessageBoxIcon.Error);
							return false;
						}

						//.obj vertex indices are 1-based
						int v1 = ReadVertexIndex(parts[1]) - 1;
						int v2 = ReadVertexIndex(parts[2]) - 1;
						int v3 = ReadVertexIndex(parts[3]) - 1;

						// Skip face if vertex indexes match
						if(verts[v1] == verts[v2] || verts[v1] == verts[v3] || verts[v2] == verts[v3]) continue;

						// Skip face if it's vertical
						Plane p = new Plane(verts[v1], verts[v2], verts[v3], true);
						if((float)Math.Round(p.Normal.GetAngleZ(), 3) == picoarse) continue;

						faces.Add(new Face(verts[v3], verts[v2], verts[v1]));
					}
				}
			}

			return true;
		}

		private static int ReadVertexIndex(string def) 
		{
			int slashpos = def.IndexOf(slash);
			if(slashpos != -1) def = def.Substring(0, slashpos);
			return int.Parse(def);
		}

		#endregion
	}
}
