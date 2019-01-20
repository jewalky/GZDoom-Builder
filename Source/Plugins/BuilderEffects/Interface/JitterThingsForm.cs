#region Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.VisualModes;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderEffects
{
	public partial class JitterThingsForm : DelayedForm
	{
		#region Variables

		private readonly string editingModeName;
		private readonly List<Thing> selection;
		private readonly List<VisualThing> visualSelection;
		private readonly List<ThingData> thingData;
		private readonly int MaxSafeDistance;
		private readonly int MaxSafeHeightDistance;

		private static bool relativePitch;
		private static bool relativeRoll;
		private static bool allowNegativePitch;
		private static bool allowNegativeRoll;
		private static bool relativeScale;
		private static bool allowNegativeScaleX;
		private static bool allowNegativeScaleY;
		private static bool uniformScale;

		private struct ThingData 
		{
			public Vector3D Position;
			public int Angle;
			public int Pitch;
			public int Roll;
			public float ScaleX;
			public float ScaleY;
			public int SectorHeight;
			public int ZOffset;
			public int SafeDistance;
			public int OffsetAngle; //position jitter angle, not Thing angle!
			public float JitterRotation;
			public float JitterPitch;
			public float JitterRoll;
			public float JitterScaleX;
			public float JitterScaleY;
			public float JitterHeight;
		}

		#endregion

		#region Constructor

		public JitterThingsForm(string editingModeName) 
		{
			this.editingModeName = editingModeName;
			this.HelpRequested += JitterThingsForm_HelpRequested;

			InitializeComponent();

			//have thing height?
			heightJitterAmmount.Enabled = General.Map.FormatInterface.HasThingHeight;
			bUpdateHeight.Enabled = General.Map.FormatInterface.HasThingHeight;

			//disable pitch/roll/scale?
			if(!General.Map.UDMF) 
			{
				pitchAmmount.Enabled = false;
				rollAmmount.Enabled = false;
				bUpdatePitch.Enabled = false;
				bUpdateRoll.Enabled = false;
				scalegroup.Enabled = false;
				cbRelativePitch.Enabled = false;
				cbRelativeRoll.Enabled = false;
				cbNegativePitch.Enabled = false;
				cbNegativeRoll.Enabled = false;
			}

			//get selection
			selection = new List<Thing>();

			if(editingModeName == "BaseVisualMode") 
			{
				visualSelection = ((VisualMode)General.Editing.Mode).GetSelectedVisualThings(false);
				foreach(VisualThing t in visualSelection) selection.Add(t.Thing);
			} 
			else 
			{
				ICollection<Thing> list = General.Map.Map.GetSelectedThings(true);
				foreach(Thing t in list) selection.Add(t);
			}

			//update window header
			this.Text = "Randomize " + selection.Count + (selection.Count > 1 ? " things" : " thing");

			//store intial properties
			thingData = new List<ThingData>();

			foreach(Thing t in selection)
			{
				ThingData d = new ThingData();

				Thing closest = MapSet.NearestThing(General.Map.Map.Things, t);

				if(closest != null)
				{
					d.SafeDistance = (int)Math.Round(Vector2D.Distance(t.Position, closest.Position));
				}
				else
				{
					d.SafeDistance = 512;
				}

				if(d.SafeDistance > 0) d.SafeDistance /= 2;
				if(MaxSafeDistance < d.SafeDistance) MaxSafeDistance = d.SafeDistance;
				d.Position = t.Position;
				d.Angle = t.AngleDoom;
				d.Pitch = t.Pitch;
				d.Roll = t.Roll;
				d.ScaleX = t.ScaleX;
				d.ScaleY = t.ScaleY;

				if(General.Map.FormatInterface.HasThingHeight) 
				{
					if(t.Sector == null) t.DetermineSector();
					if(t.Sector != null)
					{
						d.SectorHeight = Math.Max(0, t.Sector.CeilHeight - (int)t.Height - t.Sector.FloorHeight);
						if(MaxSafeHeightDistance < d.SectorHeight) MaxSafeHeightDistance = d.SectorHeight;
						d.ZOffset = (int)t.Position.z;
					}
				}

				thingData.Add(d);
			}

			positionJitterAmmount.Maximum = MaxSafeDistance;
			heightJitterAmmount.Maximum = MaxSafeHeightDistance;

			//create undo
			General.Map.UndoRedo.ClearAllRedos();
			General.Map.UndoRedo.CreateUndo("Randomize " + selection.Count + (selection.Count > 1 ? " things" : " thing"));

			//update controls
			UpdateOffsetAngles();
			UpdateHeights();
			UpdateRotationAngles();
			UpdatePitchAngles();
			UpdateRollAngles();
			UpdateScaleX();
			UpdateScaleY();

			//apply settings
			cbRelativeScale.Checked = relativeScale;
			cbUniformScale.Checked = uniformScale;
			cbNegativeScaleX.Checked = allowNegativeScaleX;
			cbNegativeScaleY.Checked = allowNegativeScaleY;
			cbRelativePitch.Checked = relativePitch;
			cbRelativeRoll.Checked = relativeRoll;
			cbNegativePitch.Checked = allowNegativePitch;
			cbNegativeRoll.Checked = allowNegativeRoll;
			

			//add event listeners
			cbRelativeScale.CheckedChanged += cbRelativeScale_CheckedChanged;
			cbUniformScale.CheckedChanged += cbUniformScale_CheckedChanged;
			cbNegativeScaleX.CheckedChanged += cbNegativeScaleX_CheckedChanged;
			cbNegativeScaleY.CheckedChanged += cbNegativeScaleY_CheckedChanged;
			cbRelativePitch.CheckedChanged += cbRelativePitch_CheckedChanged;
			cbRelativeRoll.CheckedChanged += cbRelativeRoll_CheckedChanged;
			cbNegativePitch.CheckedChanged += cbNegativePitch_CheckedChanged;
			cbNegativeRoll.CheckedChanged += cbNegativeRoll_CheckedChanged;

			//disable controls if necessary
			if(uniformScale) cbUniformScale_CheckedChanged(cbUniformScale, EventArgs.Empty);

			//tricky way to actually store undo information...
			foreach(Thing t in selection) t.Move(t.Position);
		}

		#endregion

		#region Apply logic

		private void ApplyTranslation(int ammount) 
		{
			for(int i = 0; i < selection.Count; i++) 
			{
				int curAmmount = ammount > thingData[i].SafeDistance ? thingData[i].SafeDistance : ammount;
				selection[i].Move(new Vector2D(thingData[i].Position.x + (int)(Math.Sin(thingData[i].OffsetAngle) * curAmmount), thingData[i].Position.y + (int)(Math.Cos(thingData[i].OffsetAngle) * curAmmount)));
				selection[i].DetermineSector();
			}

			UpdateGeometry();
		}

		private void ApplyRotation(int ammount) 
		{
			for(int i = 0; i < selection.Count; i++)
			{
				int newangle = (int)Math.Round(thingData[i].Angle + ammount * thingData[i].JitterRotation);
				if(General.Map.Config.DoomThingRotationAngles) newangle = newangle / 45 * 45;
				selection[i].Rotate(newangle % 360);
			}

			// Update view
			if(editingModeName == "ThingsMode") General.Interface.RedrawDisplay();
		}

		private void ApplyPitch(int ammount) 
		{
			for(int i = 0; i < selection.Count; i++) 
			{
				int p;
				if(cbRelativePitch.Checked) 
				{
					p = (int)((thingData[i].Pitch + ammount * thingData[i].JitterPitch) % 360);
				} 
				else 
				{
					p = (int)((ammount * thingData[i].JitterPitch) % 360);
				}
				
				selection[i].SetPitch(p);
			}

			//update view
			if(editingModeName == "ThingsMode") General.Interface.RedrawDisplay();
		}

		private void ApplyRoll(int ammount) 
		{
			for(int i = 0; i < selection.Count; i++) 
			{
				int r;
				if(cbRelativeRoll.Checked) 
				{
					r = (int)((thingData[i].Roll + ammount * thingData[i].JitterRoll) % 360);
				} 
				else 
				{
					r = (int)((ammount * thingData[i].JitterRoll) % 360);
				}

				selection[i].SetRoll(r);
			}

			//update view
			if(editingModeName == "ThingsMode") General.Interface.RedrawDisplay();
		}

		private void ApplyHeight(int ammount) 
		{
			for(int i = 0; i < selection.Count; i++) 
			{
				if(thingData[i].SectorHeight == 0) continue;
				int curAmmount = Math.Min(thingData[i].SectorHeight, Math.Max(0, thingData[i].ZOffset + ammount));
				selection[i].Move(selection[i].Position.x, selection[i].Position.y, curAmmount * thingData[i].JitterHeight);
			}

			UpdateGeometry();
		}

		private void ApplyScale() 
		{
			ApplyScale((float)minScaleX.Value, (float)maxScaleX.Value, (float)minScaleY.Value, (float)maxScaleY.Value);

			//update view
			if(editingModeName == "ThingsMode") General.Interface.RedrawDisplay();
		}

		private void ApplyScale(float minX, float maxX, float minY, float maxY) 
		{
			if(cbUniformScale.Checked)
			{
				minY = minX;
				maxY = maxX;
			}
			
			if(minX > maxX) General.Swap(ref minX, ref maxX);
			if(minY > maxY) General.Swap(ref minY, ref maxY);

			float diffX = maxX - minX;
			float diffY = maxY - minY;

			for(int i = 0; i < selection.Count; i++) 
			{
				float jitterX = thingData[i].JitterScaleX;
				float jitterY = (cbUniformScale.Checked ? jitterX : thingData[i].JitterScaleY);
				float sx, sy;

				if(cbRelativeScale.Checked) 
				{
					sx = thingData[i].ScaleX + minX + diffX * jitterX;
					sy = thingData[i].ScaleY + minY + diffY * jitterY;
				} 
				else 
				{
					sx = minX + diffX * jitterX;
					sy = minY + diffY * jitterY;
				}

				selection[i].SetScale(sx, sy);
			}
		}

		#endregion

		#region Update logic

		private void UpdateGeometry() 
		{
			// Update what must be updated
			if(editingModeName == "BaseVisualMode") 
			{
				VisualMode vm = ((VisualMode)General.Editing.Mode);

				for(int i = 0; i < selection.Count; i++)
				{
					visualSelection[i].SetPosition(new Vector3D(selection[i].Position.x, selection[i].Position.y, selection[i].Sector.FloorHeight + selection[i].Position.z));
					visualSelection[i].Update();

					if(vm.VisualSectorExists(visualSelection[i].Thing.Sector))
						vm.GetVisualSector(visualSelection[i].Thing.Sector).UpdateSectorGeometry(true);
				}
			} 
			else 
			{
				//update view
				General.Interface.RedrawDisplay();
			}
		}

		private void UpdateOffsetAngles() 
		{
			for(int i = 0; i < thingData.Count; i++) 
			{
				ThingData td = thingData[i];
				td.OffsetAngle = General.Random(0, 359);
				thingData[i] = td;
			}
		}

		private void UpdateHeights() 
		{
			for(int i = 0; i < thingData.Count; i++) 
			{
				ThingData td = thingData[i];
				td.JitterHeight = (General.Random(0, 100) / 100f);
				thingData[i] = td;
			}
		}

		private void UpdateRotationAngles() 
		{
			for(int i = 0; i < thingData.Count; i++) 
			{
				ThingData td = thingData[i];
				td.JitterRotation = (General.Random(-100, 100) / 100f);
				thingData[i] = td;
			}
		}

		private void UpdatePitchAngles() 
		{
			int min = (cbNegativePitch.Checked ? -100 : 0);
			for(int i = 0; i < thingData.Count; i++) 
			{
				ThingData td = thingData[i];
				td.JitterPitch = (General.Random(min, 100) / 100f);
				thingData[i] = td;
			}
		}

		private void UpdateRollAngles() 
		{
			int min = (cbNegativeRoll.Checked ? -100 : 0);
			for(int i = 0; i < thingData.Count; i++) 
			{
				ThingData td = thingData[i];
				td.JitterRoll = (General.Random(min, 100) / 100f);
				thingData[i] = td;
			}
		}

		private void UpdateScaleX() 
		{
			int min = (cbNegativeScaleX.Checked ? -100 : 0);
			for(int i = 0; i < thingData.Count; i++) 
			{
				ThingData td = thingData[i];
				td.JitterScaleX = (General.Random(min, 100) / 100f);
				thingData[i] = td;
			}
		}

		private void UpdateScaleY() 
		{
			int min = (cbNegativeScaleY.Checked ? -100 : 0);
			for(int i = 0; i < thingData.Count; i++) 
			{
				ThingData td = thingData[i];
				td.JitterScaleY = (General.Random(min, 100) / 100f);
				thingData[i] = td;
			}
		}

		#endregion

		#region Events

		private void bApply_Click(object sender, EventArgs e) 
		{
			// Store settings
			relativePitch = cbRelativePitch.Checked;
			relativeRoll = cbRelativeRoll.Checked;
			relativeScale = cbRelativeScale.Checked;
			allowNegativeScaleX = cbNegativeScaleX.Checked;
			allowNegativeScaleY = cbNegativeScaleY.Checked;
			uniformScale = cbUniformScale.Checked;
			allowNegativePitch = cbNegativePitch.Checked;
			allowNegativeRoll = cbNegativeRoll.Checked;

			// Update
			foreach(Thing t in selection) t.DetermineSector();

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

		private void JitterThingsForm_FormClosing(object sender, FormClosingEventArgs e) 
		{
			if(this.DialogResult == DialogResult.Cancel)
				General.Map.UndoRedo.WithdrawUndo(); //undo changes
		}

		private void positionJitterAmmount_OnValueChanged(object sender, EventArgs e) 
		{
			ApplyTranslation(positionJitterAmmount.Value);
		}

		private void rotationJitterAmmount_OnValueChanged(object sender, EventArgs e) 
		{
			ApplyRotation(rotationJitterAmmount.Value);
		}

		private void heightJitterAmmount_OnValueChanging(object sender, EventArgs e) 
		{
			ApplyHeight(heightJitterAmmount.Value);
		}

		private void pitchAmmount_OnValueChanging(object sender, EventArgs e) 
		{
			ApplyPitch(pitchAmmount.Value);
		}

		private void rollAmmount_OnValueChanging(object sender, EventArgs e) 
		{
			ApplyRoll(rollAmmount.Value);
		}

		private void minScaleX_ValueChanged(object sender, EventArgs e) 
		{
			ApplyScale();
		}

		private void minScaleY_ValueChanged(object sender, EventArgs e) 
		{
			ApplyScale();
		}

		#endregion

		#region Buttons & checkboxes events

		private void bUpdateTranslation_Click(object sender, EventArgs e) 
		{
			UpdateOffsetAngles();
			ApplyTranslation(positionJitterAmmount.Value);
		}

		private void bUpdateHeight_Click(object sender, EventArgs e) 
		{
			UpdateHeights();
			ApplyHeight(heightJitterAmmount.Value);
		}

		private void bUpdateAngle_Click(object sender, EventArgs e) 
		{
			UpdateRotationAngles();
			ApplyRotation(rotationJitterAmmount.Value);
		}

		private void bUpdatePitch_Click(object sender, EventArgs e) 
		{
			UpdatePitchAngles();
			ApplyPitch(pitchAmmount.Value);
		}

		private void bUpdateRoll_Click(object sender, EventArgs e) 
		{
			UpdateRollAngles();
			ApplyRoll(rollAmmount.Value);
		}

		private void bUpdateScaleX_Click(object sender, EventArgs e) 
		{
			UpdateScaleX();
			ApplyScale();
		}

		private void bUpdateScaleY_Click(object sender, EventArgs e) 
		{
			UpdateScaleY();
			ApplyScale();
		}

		private void cbRelativePitch_CheckedChanged(object sender, EventArgs e) 
		{
			UpdatePitchAngles();
			ApplyPitch(pitchAmmount.Value);
		}

		private void cbRelativeRoll_CheckedChanged(object sender, EventArgs e) 
		{
			UpdateRollAngles();
			ApplyRoll(rollAmmount.Value);
		}

		private void cbNegativePitch_CheckedChanged(object sender, EventArgs e) 
		{
			UpdatePitchAngles();
			ApplyPitch(pitchAmmount.Value);
		}

		private void cbNegativeRoll_CheckedChanged(object sender, EventArgs e) 
		{
			UpdateRollAngles();
			ApplyRoll(rollAmmount.Value);
		}

		private void cbRelativeScale_CheckedChanged(object sender, EventArgs e) 
		{
			ApplyScale();
		}

		private void cbUniformScale_CheckedChanged(object sender, EventArgs e) 
		{
			bUpdateScaleY.Enabled = !cbUniformScale.Checked;
			minScaleY.Enabled = !cbUniformScale.Checked;
			maxScaleY.Enabled = !cbUniformScale.Checked;
			minScaleYLabel.Enabled = !cbUniformScale.Checked;
			maxScaleYLabel.Enabled = !cbUniformScale.Checked;
			ApplyScale();
		}

		private void cbNegativeScaleX_CheckedChanged(object sender, EventArgs e) 
		{
			UpdateScaleX();
			ApplyScale();
		}

		private void cbNegativeScaleY_CheckedChanged(object sender, EventArgs e) 
		{
			UpdateScaleY();
			ApplyScale();
		}

		#endregion

		//HALP!
		private void JitterThingsForm_HelpRequested(object sender, HelpEventArgs hlpevent) 
		{
			General.ShowHelp("gzdb/features/all_modes/jitter.html");
			hlpevent.Handled = true;
		}
	}
}
