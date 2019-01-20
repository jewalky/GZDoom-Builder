#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.VisualModes;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderEffects
{
	public partial class JitterSectorsForm : DelayedForm
	{
		#region ================== Variables

		private readonly string editingModeName;
		private readonly List<VisualSector> visualSectors;
		private readonly List<VisualVertexPair> visualVerts;
		private readonly TranslationOffsetVertexData[] vertexData;
		private readonly List<SectorData> sectorData;
		private readonly List<SidedefData> sidedefData;
		private readonly int MaxSafeDistance;
		private readonly int MaxSafeHeightDistance;

		//settings
		private static bool keepExistingSideTextures = true;
		private static bool useFloorVertexHeights;
		private static bool useCeilingVertexHeights;
		private static int storedceiloffsetmode;
		private static int storedflooroffsetmode;

		#endregion

		#region ================== Structs

		private struct TranslationOffsetVertexData
		{
			public Vertex Vertex;
			public Vector2D InitialPosition;
			public int SafeDistance;
			public float JitterAngle;
		}

		private struct HeightOffsetVertexData
		{
			public Vertex Vertex;
			public float InitialFloorHeight;
			public float InitialCeilingHeight;
			public float ZFloor;
			public float ZCeiling;
			public float JitterFloorHeight;
			public float JitterCeilingHeight;
		}

		private struct SectorData
		{
			public Sector Sector;
			public HeightOffsetVertexData[] Verts;
			public int InitialCeilingHeight;
			public int InitialFloorHeight;
			public int SafeDistance;
			public float JitterFloorHeight;
			public float JitterCeilingHeight;
			public bool Triangular;
		}

		private struct SidedefData
		{
			public Sidedef Side;
			public string HighTexture;
			public string OtherLowTexture;
			public string OtherHighTexture;
			public string LowTexture;
			public bool PegTop;
			public bool PegBottom;
			public bool UpdateTextureOnOtherSide;
		}

		#endregion

		#region ================== Constructor / Setup

		public JitterSectorsForm(string editingModeName) 
		{
			this.editingModeName = editingModeName;

			InitializeComponent();

			//get selection
			List<Vertex> verts = new List<Vertex>();
			List<Sector> sectors = new List<Sector>();

			if(editingModeName == "BaseVisualMode") 
			{
				VisualMode vm = (VisualMode)General.Editing.Mode;
				List<VisualGeometry> visualGeometry = vm.GetSelectedSurfaces();
				visualSectors = new List<VisualSector>();

				//get selected visual and regular sectors
				foreach(VisualGeometry vg in visualGeometry) 
				{
					if(vg.GeometryType != VisualGeometryType.CEILING && vg.GeometryType != VisualGeometryType.FLOOR)
						continue;

					if(vg.Sector != null && vg.Sector.Sector != null) 
					{
						foreach(Sidedef sd in vg.Sector.Sector.Sidedefs) 
						{
							if(!verts.Contains(sd.Line.Start))
								verts.Add(sd.Line.Start);
							if(!verts.Contains(sd.Line.End))
								verts.Add(sd.Line.End);
						}

						sectors.Add(vg.Sector.Sector);
						visualSectors.Add(vg.Sector);
					}
				}

				//also get visual sectors around selected ones (because they also may be affected)
				List<Vertex> affectedVerts = new List<Vertex>();

				foreach(Sector s in sectors) 
				{
					foreach(Sidedef sd in s.Sidedefs) 
					{
						if(!affectedVerts.Contains(sd.Line.Start))
							affectedVerts.Add(sd.Line.Start);
						if(!affectedVerts.Contains(sd.Line.End))
							affectedVerts.Add(sd.Line.End);
					}
				}

				List<Sector> affectedSectors = new List<Sector>();
				foreach(Vertex v in affectedVerts) 
				{
					foreach(Linedef l in v.Linedefs) 
					{
						if(l.Front != null && !sectors.Contains(l.Front.Sector) 
							&& !affectedSectors.Contains(l.Front.Sector) 
							&& vm.VisualSectorExists(l.Front.Sector)) 
						{
							visualSectors.Add(vm.GetVisualSector(l.Front.Sector));
							affectedSectors.Add(l.Front.Sector);
						}
						if(l.Back != null && !sectors.Contains(l.Back.Sector) 
							&& !affectedSectors.Contains(l.Back.Sector) 
							&& vm.VisualSectorExists(l.Back.Sector)) 
						{
							visualSectors.Add(vm.GetVisualSector(l.Back.Sector));
							affectedSectors.Add(l.Back.Sector);
						}
					}
				}

				visualVerts = new List<VisualVertexPair>();
				foreach(Vertex vert in affectedVerts)
				{
					if(vm.VisualVertices.ContainsKey(vert)) visualVerts.Add(vm.VisualVertices[vert]);
				}
			} 
			else if(editingModeName == "SectorsMode") 
			{
				ICollection<Sector> list = General.Map.Map.GetSelectedSectors(true);

				foreach(Sector s in list) 
				{
					foreach(Sidedef sd in s.Sidedefs) 
					{
						if(!verts.Contains(sd.Line.Start))
							verts.Add(sd.Line.Start);
						if(!verts.Contains(sd.Line.End))
							verts.Add(sd.Line.End);
					}
					sectors.Add(s);
				}
			}

			if(verts.Count == 0 || sectors.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Unable to get sectors from selection!");
				return;
			}

			//create undo
			General.Map.UndoRedo.ClearAllRedos();
			General.Map.UndoRedo.CreateUndo("Randomize " + sectors.Count + (sectors.Count > 1 ? " sectors" : " sector"));

			//update window header
			this.Text = "Randomize " + sectors.Count + (sectors.Count > 1 ? " sectors" : " sector");

			//store intial properties
//process verts...
			Dictionary<Vertex, TranslationOffsetVertexData> data = new Dictionary<Vertex, TranslationOffsetVertexData>();

			foreach(Vertex v in verts) 
			{
				TranslationOffsetVertexData vd = new TranslationOffsetVertexData();
				vd.Vertex = v;
				vd.InitialPosition = v.Position;
				data.Add(v, vd);
			}

			foreach(Vertex v in verts) 
			{
				if(v.Linedefs == null) continue;

				//get nearest linedef
				Linedef closestLine = null;
				float distance = float.MaxValue;

				// Go for all linedefs in selection
				foreach(Linedef l in General.Map.Map.Linedefs) 
				{
					if(v.Linedefs.Contains(l)) continue;

					// Calculate distance and check if closer than previous find
					float d = l.SafeDistanceToSq(v.Position, true);
					if(d < distance) 
					{
						// This one is closer
						closestLine = l;
						distance = d;
					}
				}

				if(closestLine == null) continue;

				float closestLineDistance = Vector2D.Distance(v.Position, closestLine.NearestOnLine(v.Position));

				//check SafeDistance of closest line
				if(data.ContainsKey(closestLine.Start) 
					&& data[closestLine.Start].SafeDistance > closestLineDistance) 
				{
					TranslationOffsetVertexData vd = data[closestLine.Start];
					vd.SafeDistance = (int)Math.Floor(closestLineDistance);
					data[closestLine.Start] = vd;
				}
				if(data.ContainsKey(closestLine.End) 
					&& data[closestLine.End].SafeDistance > closestLineDistance) 
				{
					TranslationOffsetVertexData vd = data[closestLine.End];
					vd.SafeDistance = (int)Math.Floor(closestLineDistance);
					data[closestLine.End] = vd;
				}

				//save SafeDistance
				int dist = (int)Math.Floor(closestLineDistance);
				if(data[v].SafeDistance == 0 || data[v].SafeDistance > dist) 
				{
					TranslationOffsetVertexData vd = data[v];
					vd.SafeDistance = dist;
					data[v] = vd;
				}
			}

			//store properties
			vertexData = new TranslationOffsetVertexData[data.Values.Count];
			data.Values.CopyTo(vertexData, 0);

			for(int i = 0; i < data.Count; i++) 
			{
				if(vertexData[i].SafeDistance > 0)
					vertexData[i].SafeDistance /= 2;
				if(MaxSafeDistance < vertexData[i].SafeDistance)
					MaxSafeDistance = vertexData[i].SafeDistance;
			}

//process sectors and linedes
			sectorData = new List<SectorData>();
			sidedefData = new List<SidedefData>();

			foreach(Sector s in sectors)
			{
				SectorData sd = new SectorData();

				sd.Sector = s;
				sd.InitialCeilingHeight = s.CeilHeight;
				sd.InitialFloorHeight = s.FloorHeight;
				sd.Triangular = General.Map.UDMF && s.Sidedefs.Count == 3;
				if(sd.Triangular)
				{
					Vertex[] sectorverts = GetSectorVerts(s);
					sd.Verts = new HeightOffsetVertexData[sectorverts.Length];
					for(int i = 0; i < sectorverts.Length; i++)
					{
						HeightOffsetVertexData vd = new HeightOffsetVertexData();
						vd.Vertex = sectorverts[i];
						vd.ZFloor = sectorverts[i].ZFloor;
						vd.ZCeiling = sectorverts[i].ZCeiling;
						vd.InitialFloorHeight = float.IsNaN(vd.ZFloor) ? GetHighestFloor(sectorverts[i]) : sectorverts[i].ZFloor;
						vd.InitialCeilingHeight = float.IsNaN(vd.ZCeiling) ? GetLowestCeiling(sectorverts[i]) : sectorverts[i].ZCeiling;

						sd.Verts[i] = vd;
					}
				}
				sd.SafeDistance = (s.CeilHeight - s.FloorHeight) / 2;
				if(sd.SafeDistance > MaxSafeHeightDistance)	MaxSafeHeightDistance = sd.SafeDistance;
				sectorData.Add(sd);

				foreach(Sidedef side in s.Sidedefs) 
				{
					//store initial sidedef properties
					SidedefData sdd = new SidedefData();

					sdd.Side = side;
					sdd.LowTexture = side.LowTexture;
					sdd.HighTexture = side.HighTexture;
					sdd.PegBottom = side.Line.IsFlagSet(General.Map.Config.LowerUnpeggedFlag);
					sdd.PegTop = side.Line.IsFlagSet(General.Map.Config.UpperUnpeggedFlag);

					if(side.Other != null && !sectors.Contains(side.Other.Sector)) 
					{
						sdd.UpdateTextureOnOtherSide = true;
						sdd.OtherHighTexture = side.Other.HighTexture;
						sdd.OtherLowTexture = side.Other.LowTexture;
					}

					sidedefData.Add(sdd);
				}
			}

			positionJitterAmmount.Maximum = MaxSafeDistance;
			floorHeightAmmount.Maximum = MaxSafeHeightDistance;
			ceilingHeightAmmount.Maximum = MaxSafeHeightDistance;

			//set editing settings
			cbKeepExistingTextures.Checked = keepExistingSideTextures;
			ceiloffsetmode.SelectedIndex = storedceiloffsetmode;
			flooroffsetmode.SelectedIndex = storedflooroffsetmode;

			//vertex heights can not be set in non-UDMF maps
			if(General.Map.UDMF) 
			{
				cbUseFloorVertexHeights.Checked = useFloorVertexHeights;
				cbUseCeilingVertexHeights.Checked = useCeilingVertexHeights;
			} 
			else 
			{
				useFloorVertexHeights = false;
				cbUseFloorVertexHeights.Checked = false;
				cbUseFloorVertexHeights.Enabled = false;

				useCeilingVertexHeights = false;
				cbUseCeilingVertexHeights.Checked = false;
				cbUseCeilingVertexHeights.Enabled = false;
			}

			//texture pickers
			textureLower.Initialize();
			textureUpper.Initialize();

			//We can't use floor/ceiling textures when MixTexturesFlats is disabled
			if(General.Map.Config.MixTexturesFlats)
			{
				textureLower.TextureName = General.Settings.DefaultFloorTexture;
				textureUpper.TextureName = General.Settings.DefaultCeilingTexture;
			}
			else
			{
				textureLower.TextureName = General.Settings.DefaultTexture;
				textureUpper.TextureName = General.Settings.DefaultTexture;
				cbUpperTexStyle.Items[1] = "Use default texture";
				cbLowerTexStyle.Items[1] = "Use default texture";
			}

			cbUpperTexStyle.SelectedIndex = 0;
			cbLowerTexStyle.SelectedIndex = 0;
			UpdateTextureSelectors(); //update interface

			//create random values
			UpdateAngles(); 
			UpdateFloorHeights();
			UpdateCeilingHeights();
		}

		#endregion

		#region ================== Methods

		private static float GetLowestCeiling(Vertex v) 
		{
			if(v.Linedefs.Count == 0) return float.NaN;
			List<Sector> sectors = GetSectors(v);
			if(sectors.Count == 0) return float.NaN;

			float target = sectors[0].CeilHeight;
			for(int i = 1; i < sectors.Count; i++) 
			{
				if(target > sectors[i].CeilHeight && sectors[i].Sidedefs.Count == 3)
					target = sectors[i].CeilHeight;
			}

			return target;
		}

		private static float GetHighestFloor(Vertex v) 
		{
			if(v.Linedefs.Count == 0) return float.NaN;
			List<Sector> sectors = GetSectors(v);
			if(sectors.Count == 0) return float.NaN;

			float target = sectors[0].FloorHeight;
			for(int i = 1; i < sectors.Count; i++) 
			{
				if(target < sectors[i].FloorHeight && sectors[i].Sidedefs.Count == 3)
					target = sectors[i].FloorHeight;
			}

			return target;
		}

		private static List<Sector> GetSectors(Vertex v) 
		{
			List<Sector> result = new List<Sector>();
			foreach(Linedef l in v.Linedefs) 
			{
				if(l.Front != null && l.Front.Sector != null && !result.Contains(l.Front.Sector))
					result.Add(l.Front.Sector);
				if(l.Back != null && l.Back.Sector != null && !result.Contains(l.Back.Sector))
					result.Add(l.Back.Sector);
			}
			return result;
		}

		private static Vertex[] GetSectorVerts(Sector s) 
		{
			List<Vertex> result = new List<Vertex>();
			foreach(Sidedef side in s.Sidedefs) 
			{
				if(side.Line == null) continue;
				if(!result.Contains(side.Line.Start)) result.Add(side.Line.Start);
				if(!result.Contains(side.Line.End)) result.Add(side.Line.End);
			}

			return result.ToArray();
		}

		#endregion

		#region ================== Utility

		private void ApplyTranslationJitter(int ammount) 
		{
			for(int i = 0; i < vertexData.Length; i++)
			{
				int curAmmount = ammount > vertexData[i].SafeDistance ? vertexData[i].SafeDistance : ammount;
				vertexData[i].Vertex.Move(new Vector2D(vertexData[i].InitialPosition.x + (int)(Math.Sin(vertexData[i].JitterAngle) * curAmmount), vertexData[i].InitialPosition.y + (int)(Math.Cos(vertexData[i].JitterAngle) * curAmmount)));
			}

			//update view
			if(editingModeName == "BaseVisualMode") 
			{
				General.Map.Map.Update();
				General.Map.IsChanged = true;
				UpdateVisualGeometry();
			} 
			else 
			{
				General.Interface.RedrawDisplay();
			}
		}

		private void ApplyCeilingHeightJitter(int ammount) 
		{
			for(int i = 0; i < sectorData.Count; i++)
			{
				int curAmmount = ammount > sectorData[i].SafeDistance ? sectorData[i].SafeDistance : ammount;

				if(sectorData[i].Triangular && cbUseCeilingVertexHeights.Checked) 
				{
					foreach(HeightOffsetVertexData vd in sectorData[i].Verts) 
					{
						vd.Vertex.ZCeiling = vd.InitialCeilingHeight - (float)Math.Floor(curAmmount
							* ModifyByOffsetMode(vd.JitterCeilingHeight, ceiloffsetmode.SelectedIndex));
					}
				} 
				else 
				{
					sectorData[i].Sector.CeilHeight = sectorData[i].InitialCeilingHeight - (int)Math.Floor(curAmmount
						* ModifyByOffsetMode(sectorData[i].JitterCeilingHeight, ceiloffsetmode.SelectedIndex));
				}
			}

			//update view
			if(editingModeName == "BaseVisualMode") 
			{
				General.Map.Map.Update();
				General.Map.IsChanged = true;
				UpdateVisualGeometry();
			}

			UpdateUpperTextures(cbUpperTexStyle.SelectedIndex, false);
		}

		private void ApplyFloorHeightJitter(int ammount) 
		{
			for(int i = 0; i < sectorData.Count; i++)
			{
				int curAmmount = ammount > sectorData[i].SafeDistance ? sectorData[i].SafeDistance : ammount;

				if(sectorData[i].Triangular && cbUseFloorVertexHeights.Checked) 
				{
					foreach(HeightOffsetVertexData vd in sectorData[i].Verts) 
					{
						vd.Vertex.ZFloor = vd.InitialFloorHeight + (float)Math.Floor(curAmmount
							* ModifyByOffsetMode(vd.JitterFloorHeight, flooroffsetmode.SelectedIndex));
					}
				} 
				else 
				{
					sectorData[i].Sector.FloorHeight = sectorData[i].InitialFloorHeight + (int)Math.Floor(curAmmount
						* ModifyByOffsetMode(sectorData[i].JitterFloorHeight, flooroffsetmode.SelectedIndex));
				}
			}

			//update view
			if(editingModeName == "BaseVisualMode") 
			{
				General.Map.Map.Update();
				General.Map.IsChanged = true;
				UpdateVisualGeometry();
			}

			UpdateLowerTextures(cbLowerTexStyle.SelectedIndex, false);
		}

		private static float ModifyByOffsetMode(float value, int mode)
		{
			switch(mode)
			{
				case 0: //Raise and lower
					return value;

				case 1: //Raise only
					return Math.Abs(value);

				case 2: //Lower only
					return -Math.Abs(value);

				default:
					throw new NotImplementedException("JitterSectorsForm.ModifyByOffsetMode: got unknown mode (" + mode + ")");
			}
		}

		private void UpdateVisualGeometry() 
		{
			foreach(VisualSector vs in visualSectors) vs.UpdateSectorGeometry(true);
			foreach(VisualSector vs in visualSectors) vs.UpdateSectorData();
			foreach(VisualSector vs in visualSectors) vs.UpdateSectorData();
			foreach(VisualVertexPair pair in visualVerts)
			{
				pair.Changed = true;
				pair.Update();
			}
		}

		private void UpdateTextureSelectors() 
		{
			cbLowerTexStyle.Enabled = floorHeightAmmount.Value > 0;
			cbUpperTexStyle.Enabled = ceilingHeightAmmount.Value > 0;
			gbLowerTexture.Enabled = floorHeightAmmount.Value > 0 && cbLowerTexStyle.SelectedIndex > 0;
			gbUpperTexture.Enabled = ceilingHeightAmmount.Value > 0 && cbUpperTexStyle.SelectedIndex > 0;
			textureLower.Enabled = floorHeightAmmount.Value > 0 && cbLowerTexStyle.SelectedIndex == 2;
			textureUpper.Enabled = ceilingHeightAmmount.Value > 0 && cbUpperTexStyle.SelectedIndex == 2;
		}

		private void UpdateUpperTextures(int index, bool updateGeometry) 
		{
			if(index == -1) return;

			if(index == 0) //revert
			{ 
				foreach(SidedefData sd in sidedefData)
					SetUpperTexture(sd, sd.HighTexture);
			}
			else if(index == 1) //use ceiling or default texture
			{
				if(General.Map.Config.MixTexturesFlats)
				{

					foreach(SidedefData sd in sidedefData)
					{
						if(sd.Side.Sector != null)
						{
							if(sd.UpdateTextureOnOtherSide && sd.Side.Other.Sector != null) SetUpperTexture(sd, sd.Side.Sector.CeilTexture, sd.Side.Other.Sector.CeilTexture);
							else SetUpperTexture(sd, sd.Side.Sector.CeilTexture);
						}
					}
				}
				else
				{
					foreach(SidedefData sd in sidedefData) SetUpperTexture(sd, General.Settings.DefaultTexture);
				}
			}
			else if(index == 2) //use given texture
			{
				foreach(SidedefData sd in sidedefData)
					SetUpperTexture(sd, textureUpper.TextureName);
			}

			General.Map.Data.UpdateUsedTextures();
			if(updateGeometry && editingModeName == "BaseVisualMode") UpdateVisualGeometry();
		}

		private void UpdateLowerTextures(int index, bool updateGeometry) 
		{
			if(index == -1) return;

			if(index == 0) //revert
			{ 
				foreach(SidedefData sd in sidedefData)
					SetLowerTexture(sd, sd.LowTexture);
			}
			else if(index == 1) //use floor or default texture
			{
				if(General.Map.Config.MixTexturesFlats)
				{
					foreach(SidedefData sd in sidedefData)
					{
						if(sd.Side.Sector != null)
						{
							if(sd.UpdateTextureOnOtherSide && sd.Side.Other.Sector != null) SetLowerTexture(sd, sd.Side.Sector.FloorTexture, sd.Side.Other.Sector.FloorTexture);
							else SetLowerTexture(sd, sd.Side.Sector.FloorTexture);
						}
					}
				}
				else
				{
					foreach(SidedefData sd in sidedefData) SetLowerTexture(sd, General.Settings.DefaultTexture);
				}
			}
			else if(index == 2) //use given texture
			{
				foreach(SidedefData sd in sidedefData)
					SetLowerTexture(sd, textureLower.TextureName);
			}

			General.Map.Data.UpdateUsedTextures();
			if(updateGeometry && editingModeName == "BaseVisualMode") UpdateVisualGeometry();
		}

		#endregion

		#region ================== Set Textures

		private void SetUpperTexture(SidedefData sd, string textureName) 
		{
			SetUpperTexture(sd, textureName, textureName);
		}

		private void SetUpperTexture(SidedefData sd, string textureName, string otherTextureName) 
		{
			if(!cbKeepExistingTextures.Checked || string.IsNullOrEmpty(sd.HighTexture) || sd.HighTexture == "-")
				sd.Side.SetTextureHigh(textureName);

			if(sd.UpdateTextureOnOtherSide && sd.Side.Other != null && (!cbKeepExistingTextures.Checked || string.IsNullOrEmpty(sd.OtherHighTexture) || sd.OtherHighTexture == "-"))
				sd.Side.Other.SetTextureHigh(otherTextureName);
		}

		private void SetLowerTexture(SidedefData sd, string textureName) 
		{
			SetLowerTexture(sd, textureName, textureName);
		}

		private void SetLowerTexture(SidedefData sd, string textureName, string otherTextureName) 
		{
			if(!cbKeepExistingTextures.Checked || string.IsNullOrEmpty(sd.LowTexture) || sd.LowTexture == "-")
				sd.Side.SetTextureLow(textureName);

			if(sd.UpdateTextureOnOtherSide && sd.Side.Other != null && (!cbKeepExistingTextures.Checked || string.IsNullOrEmpty(sd.OtherLowTexture) || sd.OtherLowTexture == "-")) 
				sd.Side.Other.SetTextureLow(otherTextureName);
		}

		#endregion

		#region ================== Jitter generation

		private void UpdateAngles() 
		{
			for(int i = 0; i < vertexData.Length; i++) 
			{
				TranslationOffsetVertexData vd = vertexData[i];
				vd.JitterAngle = (float)(General.Random(0, 359) * Math.PI / 180f);
				vertexData[i] = vd;
			}
		}

		private void UpdateFloorHeights() 
		{
			for(int i = 0; i < sectorData.Count; i++) 
			{
				SectorData sd = sectorData[i];

				if(sd.Triangular) 
				{
					for(int c = 0; c < 3; c++) 
					{
						HeightOffsetVertexData vd = sd.Verts[c];
						vd.JitterFloorHeight = General.Random(-100, 100) / 100f;
						sd.Verts[c] = vd;
					}
				} 

				sd.JitterFloorHeight = General.Random(-100, 100) / 100f;
				sectorData[i] = sd;
			}
		}

		private void UpdateCeilingHeights() 
		{
			for(int i = 0; i < sectorData.Count; i++) 
			{
				SectorData sd = sectorData[i];

				if(sd.Triangular) 
				{
					for(int c = 0; c < 3; c++)
					{
						HeightOffsetVertexData vd = sd.Verts[c];
						vd.JitterCeilingHeight = General.Random(-100, 100) / 100f;
						sd.Verts[c] = vd;
					}
				} 

				sd.JitterCeilingHeight = General.Random(-100, 100) / 100f;
				sectorData[i] = sd;
			}
		}

		#endregion

		#region ================== Events

		private void bApply_Click(object sender, EventArgs e) 
		{
			//store settings
			keepExistingSideTextures = cbKeepExistingTextures.Checked;
			useFloorVertexHeights = cbUseFloorVertexHeights.Checked;
			useCeilingVertexHeights = cbUseCeilingVertexHeights.Checked;
			storedceiloffsetmode = ceiloffsetmode.SelectedIndex;
			storedflooroffsetmode = flooroffsetmode.SelectedIndex;
			
			// Clean unused sidedef textures
			foreach(SidedefData sd in sidedefData) 
			{
				sd.Side.RemoveUnneededTextures(false);

				if(sd.UpdateTextureOnOtherSide)
					sd.Side.Other.RemoveUnneededTextures(false);
			}

			// Update cached values
			General.Map.Map.Update();
			General.Map.IsChanged = true;

			// Clear selection
			General.Actions.InvokeAction("builder_clearselection");

			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void bCancel_Click(object sender, EventArgs e) 
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void JitterSectorsForm_FormClosing(object sender, FormClosingEventArgs e) 
		{
			if(this.DialogResult == DialogResult.Cancel)
				General.Map.UndoRedo.WithdrawUndo(); //undo changes
		}

		private void positionJitterAmmount_OnValueChanging(object sender, EventArgs e) 
		{
			ApplyTranslationJitter(positionJitterAmmount.Value);
		}

		private void ceilingHeightAmmount_OnValueChanging(object sender, EventArgs e) 
		{
			ApplyCeilingHeightJitter(ceilingHeightAmmount.Value);
			UpdateTextureSelectors();
		}

		private void floorHeightAmmount_OnValueChanging(object sender, EventArgs e) 
		{
			ApplyFloorHeightJitter(floorHeightAmmount.Value);
			UpdateTextureSelectors();
		}

		private void cbKeepExistingTextures_CheckedChanged(object sender, EventArgs e) 
		{
			//revert possible changes
			if(cbKeepExistingTextures.Checked) 
			{
				foreach(SidedefData sd in sidedefData) 
				{
					if(!string.IsNullOrEmpty(sd.HighTexture))
						sd.Side.SetTextureHigh(sd.HighTexture);

					if(sd.UpdateTextureOnOtherSide && !string.IsNullOrEmpty(sd.OtherHighTexture))
						sd.Side.Other.SetTextureHigh(sd.OtherHighTexture);
				}

				if(editingModeName == "BaseVisualMode")	UpdateVisualGeometry();
			} 
			else 
			{
				UpdateLowerTextures(cbLowerTexStyle.SelectedIndex, false);
				UpdateUpperTextures(cbUpperTexStyle.SelectedIndex, true);
			}
		}

		private void cbUseFloorVertexHeights_CheckedChanged(object sender, EventArgs e) 
		{
			//Reset values?
			if(!cbUseFloorVertexHeights.Checked || floorHeightAmmount.Value == 0)
			{
				foreach(SectorData data in sectorData)
				{
					if(!data.Triangular) continue;
					foreach(HeightOffsetVertexData vd in data.Verts) vd.Vertex.ZFloor = vd.ZFloor;
				}
			}

			//update changes
			ApplyFloorHeightJitter(floorHeightAmmount.Value);
			UpdateTextureSelectors();
		}

		private void cbUseCeilingVertexHeights_CheckedChanged(object sender, EventArgs e) 
		{
			//Reset values?
			if(!cbUseCeilingVertexHeights.Checked || ceilingHeightAmmount.Value == 0) 
			{
				foreach(SectorData data in sectorData) 
				{
					if(!data.Triangular) continue;
					foreach(HeightOffsetVertexData vd in data.Verts) vd.Vertex.ZCeiling = vd.ZCeiling;
				}
			}

			//update changes
			ApplyCeilingHeightJitter(ceilingHeightAmmount.Value);
			UpdateTextureSelectors();
		}

		#region ================== Update Events

		private void bUpdateTranslation_Click(object sender, EventArgs e) 
		{
			UpdateAngles();
			ApplyTranslationJitter(positionJitterAmmount.Value);
		}

		private void bUpdateCeilingHeight_Click(object sender, EventArgs e) 
		{
			UpdateCeilingHeights();
			ApplyCeilingHeightJitter(ceilingHeightAmmount.Value);
		}

		private void bUpdateFloorHeight_Click(object sender, EventArgs e) 
		{
			UpdateFloorHeights();
			ApplyFloorHeightJitter(floorHeightAmmount.Value);
		}

		#endregion

		#region ================== Height Offset Mode Events

		private void ceiloffsetmode_SelectedIndexChanged(object sender, EventArgs e)
		{
			ApplyCeilingHeightJitter(ceilingHeightAmmount.Value);
		}

		private void flooroffsetmode_SelectedIndexChanged(object sender, EventArgs e)
		{
			ApplyFloorHeightJitter(floorHeightAmmount.Value);
		}

		#endregion

		#region ================== Texture Pegging Events

		private void cbPegTop_CheckedChanged(object sender, EventArgs e) 
		{
			if(cbPegTop.Checked) //apply flag
			{ 
				foreach(SidedefData sd in sidedefData)
					sd.Side.Line.SetFlag(General.Map.Config.UpperUnpeggedFlag, true);
			} 
			else //revert to initial setting
			{ 
				foreach(SidedefData sd in sidedefData)
					sd.Side.Line.SetFlag(General.Map.Config.UpperUnpeggedFlag, sd.PegTop);
			}

			if(editingModeName == "BaseVisualMode") 
			{
				General.Map.Data.UpdateUsedTextures();
				UpdateVisualGeometry();
			}
		}

		private void cbPegBottom_CheckedChanged(object sender, EventArgs e) 
		{
			if(cbPegBottom.Checked) //apply flag
			{ 
				foreach(SidedefData sd in sidedefData)
					sd.Side.Line.SetFlag(General.Map.Config.LowerUnpeggedFlag, true);
			} 
			else //revert to initial setting
			{ 
				foreach(SidedefData sd in sidedefData)
					sd.Side.Line.SetFlag(General.Map.Config.LowerUnpeggedFlag, sd.PegBottom);
			}

			if(editingModeName == "BaseVisualMode") 
			{
				General.Map.Data.UpdateUsedTextures();
				UpdateVisualGeometry();
			}
		}

		#endregion

		#region ================== Texture Picker Events

		//texture pickers
		private void textureLower_OnValueChanged(object sender, EventArgs e) 
		{
			UpdateLowerTextures(cbLowerTexStyle.SelectedIndex, true);
		}

		private void textureUpper_OnValueChanged(object sender, EventArgs e) 
		{
			UpdateUpperTextures(cbUpperTexStyle.SelectedIndex, true);
		}

		#endregion

		#region ================== Texture Style Selector Events

		//texture style selectors
		private void cbUpperTexStyle_SelectedIndexChanged(object sender, EventArgs e) 
		{
			UpdateUpperTextures(cbUpperTexStyle.SelectedIndex, true);
			UpdateTextureSelectors();
		}

		private void cbLowerTexStyle_SelectedIndexChanged(object sender, EventArgs e) 
		{
			UpdateLowerTextures(cbLowerTexStyle.SelectedIndex, true);
			UpdateTextureSelectors();
		}

		#endregion

		//HALP!
		private void JitterSectorsForm_HelpRequested(object sender, HelpEventArgs hlpevent) 
		{
			General.ShowHelp("gzdb/features/all_modes/jitter.html");
			hlpevent.Handled = true;
		}

		#endregion
	}
}
