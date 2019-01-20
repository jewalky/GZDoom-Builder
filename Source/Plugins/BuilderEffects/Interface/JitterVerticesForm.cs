#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.VisualModes;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.BuilderEffects
{
	public partial class JitterVerticesForm : DelayedForm
	{
		#region ================== Variables

		private readonly string editingModeName;
		private readonly List<Vertex> selection;
		private readonly List<VisualSector> visualSectors;
		private readonly List<VisualVertexPair> visualVerts;
		private readonly VertexData[] vertexData;
		private readonly int MaxSafeDistance;

		#endregion

		#region ================== Structs

		private struct VertexData
		{
			public Vector2D Position;
			public int SafeDistance;
			public float JitterAngle;
		}

		#endregion

		#region ================== Constructor / Setup

		public JitterVerticesForm(string editingModeName) 
		{
			this.editingModeName = editingModeName;
			this.HelpRequested += JitterVerticesForm_HelpRequested;

			InitializeComponent();

			//get selection
			selection = new List<Vertex>();

			if(editingModeName == "BaseVisualMode") 
			{
				VisualMode vm = (VisualMode)General.Editing.Mode;
				List<VisualGeometry> visualSelection = vm.GetSelectedSurfaces();
				visualSectors = new List<VisualSector>();
				int linesCount = 0;

				foreach(VisualGeometry vg in visualSelection) 
				{
					if(vg.Sidedef != null && vm.VisualSectorExists(vg.Sidedef.Sector)) 
					{
						if(!selection.Contains(vg.Sidedef.Line.Start))
							selection.Add(vg.Sidedef.Line.Start);
						if(!selection.Contains(vg.Sidedef.Line.End))
							selection.Add(vg.Sidedef.Line.End);
						linesCount++;

						visualSectors.Add(vm.GetVisualSector(vg.Sidedef.Sector));

						if(vg.Sidedef.Other != null && vg.Sidedef.Other.Sector != null && vm.VisualSectorExists(vg.Sidedef.Other.Sector))
							visualSectors.Add(vm.GetVisualSector(vg.Sidedef.Other.Sector));
					}
				}

				visualVerts = new List<VisualVertexPair>();
				foreach(Vertex vert in selection) 
				{
					if(vm.VisualVertices.ContainsKey(vert)) visualVerts.Add(vm.VisualVertices[vert]);
				}

				//update window header
				this.Text = "Randomize " + linesCount + (linesCount > 1 ? " linedefs" : " linedef");

			} 
			else if(editingModeName == "LinedefsMode") 
			{
				ICollection<Linedef> list = General.Map.Map.GetSelectedLinedefs(true);
				int linesCount = 0;

				foreach(Linedef l in list) 
				{
					if(!selection.Contains(l.Start)) selection.Add(l.Start);
					if(!selection.Contains(l.End)) selection.Add(l.End);
					linesCount++;
				}

				//update window header
				this.Text = "Randomize " + linesCount + (linesCount > 1 ? " linedefs" : " linedef");
			} 
			else 
			{ 
				ICollection<Vertex> list = General.Map.Map.GetSelectedVertices(true);
				foreach(Vertex v in list) selection.Add(v);

				//update window header
				this.Text = "Randomize " + selection.Count + (selection.Count > 1 ? " vertices" : " vertex");
			}

			if(selection.Count == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Unable to get vertices from selection!");
				return;
			}

			//create undo
			General.Map.UndoRedo.ClearAllRedos();
			General.Map.UndoRedo.CreateUndo("Randomize " + selection.Count + (selection.Count > 1 ? " vertices" : " vertex"));

			Dictionary<Vertex, VertexData> data = new Dictionary<Vertex, VertexData>();

			foreach(Vertex v in selection) 
			{
				VertexData vd = new VertexData {Position = v.Position};
				data.Add(v, vd);
			}

			foreach(Vertex v in selection)
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
					VertexData vd = data[closestLine.Start];
					vd.SafeDistance = (int)Math.Floor(closestLineDistance);
					data[closestLine.Start] = vd;
				}
				if(data.ContainsKey(closestLine.End) 
					&& data[closestLine.End].SafeDistance > closestLineDistance) 
				{
					VertexData vd = data[closestLine.End];
					vd.SafeDistance = (int)Math.Floor(closestLineDistance);
					data[closestLine.End] = vd;
				}

				//save SafeDistance
				int dist = (int)Math.Floor(closestLineDistance);
				if(data[v].SafeDistance == 0 || data[v].SafeDistance > dist) 
				{
					VertexData vd = data[v];
					vd.SafeDistance = dist;
					data[v] = vd;
				}
			}

			//store properties
			vertexData = new VertexData[data.Values.Count];
			data.Values.CopyTo(vertexData, 0);

			for(int i = 0; i < vertexData.Length; i++) 
			{
				if(vertexData[i].SafeDistance > 0) 
					vertexData[i].SafeDistance /= 2;
				if(MaxSafeDistance < vertexData[i].SafeDistance) 
					MaxSafeDistance = vertexData[i].SafeDistance;
			}

			positionJitterAmmount.Maximum = MaxSafeDistance;

			UpdateAngles();
		}

		#endregion

		#region ================== Utility

		private void ApplyTranslationJitter(int ammount) 
		{
			for(int i = 0; i < selection.Count; i++)
			{
				int curAmmount = ammount > vertexData[i].SafeDistance ? vertexData[i].SafeDistance : ammount;
				selection[i].Move(new Vector2D(vertexData[i].Position.x + (int)(Math.Sin(vertexData[i].JitterAngle) * curAmmount), vertexData[i].Position.y + (int)(Math.Cos(vertexData[i].JitterAngle) * curAmmount)));
			}

			//update view
			if(editingModeName == "BaseVisualMode") 
			{
				General.Map.Map.Update();
				General.Map.IsChanged = true;

				foreach(VisualSector vs in visualSectors) vs.UpdateSectorGeometry(true);
				foreach(VisualSector vs in visualSectors) vs.UpdateSectorData();
				foreach(VisualVertexPair pair in visualVerts)
				{
					pair.Changed = true;
					pair.Update();
				}
			} 
			else 
			{
				General.Interface.RedrawDisplay();
			}
		}

		private void UpdateAngles() 
		{
			for(int i = 0; i < vertexData.Length; i++) 
			{
				VertexData vd = vertexData[i];
				vd.JitterAngle = (float)(General.Random(0, 359) * Math.PI / 180f);
				vertexData[i] = vd;
			}
		}

#endregion

		#region ================== Events

		private void bApply_Click(object sender, EventArgs e) 
		{
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

		private void JitterVerticesForm_FormClosing(object sender, FormClosingEventArgs e) 
		{
			if(this.DialogResult == DialogResult.Cancel)
				General.Map.UndoRedo.WithdrawUndo(); //undo changes
		}

		private void positionJitterAmmount_OnValueChanging(object sender, EventArgs e) 
		{
			ApplyTranslationJitter(positionJitterAmmount.Value);
		}

		private void bUpdateTranslation_Click(object sender, EventArgs e) 
		{
			UpdateAngles();
			ApplyTranslationJitter(positionJitterAmmount.Value);
		}

		private void JitterVerticesForm_HelpRequested(object sender, HelpEventArgs hlpevent) 
		{
			General.ShowHelp("gzdb/features/all_modes/jitter.html");
			hlpevent.Handled = true;
		}

		#endregion
	}
}
