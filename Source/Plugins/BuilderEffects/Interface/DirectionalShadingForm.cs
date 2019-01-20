#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.VisualModes;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderEffects
{
	public partial class DirectionalShadingForm : DelayedForm
	{
		#region ================== Variables

		private Dictionary<Sector, Vector3D> sectors; // <sector, floor normal>
		private Dictionary<Sidedef, Vector3D> sides; // <side, side normal>
		private Dictionary<Sector, int> sectorbrightness;
		private Dictionary<Sector, int> sectorcolors;
		private Dictionary<Sidedef, int> sidebrightness;
		private IEnumerable<VisualSector> visualsectors;
		private Vector3D sunvector;
		private bool blockupdate;

		#endregion

		#region ================== Constructor

		private DirectionalShadingForm() { }
		public DirectionalShadingForm(IEnumerable<Sector> selectedsectors, IEnumerable<Sidedef> selectedsides, IEnumerable<VisualSector> selectedvisualsectors)
		{
			InitializeComponent();

			// Store sectors, collect floor normals
			visualsectors = selectedvisualsectors;
			sectors = new Dictionary<Sector, Vector3D>();
			sectorbrightness = new Dictionary<Sector, int>();
			sectorcolors = new Dictionary<Sector, int>();
			foreach(Sector s in selectedsectors)
			{
				sectors.Add(s, Sector.GetFloorPlane(s).Normal);
				sectorcolors[s] = UniFields.GetInteger(s.Fields, "lightcolor", PixelColor.INT_WHITE_NO_ALPHA);

				// Store initial brightness
				if(s.Fields.GetValue("lightfloorabsolute", false))
					sectorbrightness[s] = UniFields.GetInteger(s.Fields, "lightfloor");
				else
					sectorbrightness[s] = s.Brightness;
			}

			// Store sidedefs, collect side normals
			sides = new Dictionary<Sidedef, Vector3D>();
			sidebrightness = new Dictionary<Sidedef, int>();
			foreach(Sidedef sd in selectedsides)
			{
				Vector3D normal = Vector3D.FromAngleXY(sd.Line.Angle + Angle2D.PIHALF);
				if(sd == sd.Line.Front) normal *= -1;
				sides.Add(sd, normal);

				// Store initial brightness
				if(sd.Fields.GetValue("lightabsolute", false))
					sidebrightness[sd] = UniFields.GetInteger(sd.Fields, "light");
				else
					sidebrightness[sd] = sd.Sector.Brightness;
			}

			// Create undo
			string sectorscount = (sectors.Count > 0 ? (sectors.Count > 1 ? " sectors" : " sector") : "");
			string sidescount = (!string.IsNullOrEmpty(sectorscount) ? " and " : "");
			sidescount += (sides.Count > 0 ? (sides.Count > 1 ? " sidedefs" : " sidedef") : "");
			General.Map.UndoRedo.ClearAllRedos();
			General.Map.UndoRedo.CreateUndo("Apply directional shading to " + sectorscount + sidescount);

			// Prepare your fields!
			foreach(Sector s in sectors.Keys) s.Fields.BeforeFieldsChange();
			foreach(Sidedef sd in sides.Keys) sd.Fields.BeforeFieldsChange();

			// Load settings
			blockupdate = true;

			int angle = General.Settings.ReadPluginSetting("directionalshading.sunangle", 45);
			sunangle.Angle = angle;
			sunangletb.Text = angle.ToString();
			lightamount.Value = General.Settings.ReadPluginSetting("directionalshading.lightamount", 64);
			lightcolor.Color = PixelColor.FromInt(General.Settings.ReadPluginSetting("directionalshading.lightcolor", 0xFDEBD7));
			shadeamount.Value = General.Settings.ReadPluginSetting("directionalshading.shadeamount", 16);
			shadecolor.Color = PixelColor.FromInt(General.Settings.ReadPluginSetting("directionalshading.shadecolor", 0xABC8EB));

			blockupdate = false;

			OnSunAngleChanged();
		}

		#endregion

		#region ================== Methods

		private void UpdateShading()
		{
			// Update sector shading
			foreach(KeyValuePair<Sector, Vector3D> group in sectors)
			{
				// Calculate light amount
				float anglediff = Vector3D.DotProduct(group.Value, sunvector);
				int targetlight;
				PixelColor targetcolor;

				// Calculate light and light color when surface normal is rotated towards the sun vector
				if(anglediff >= 0.5f)
				{
					float lightmul = (anglediff - 0.5f) * 2.0f;
					targetlight = (int)Math.Round(lightamount.Value * lightmul);
					targetcolor = InterpolationTools.InterpolateColor(shadecolor.Color, lightcolor.Color, anglediff);
				}
				// Otherwise calculate shade and shade color
				else
				{
					float lightmul = (0.5f - anglediff) * -2.0f;
					targetlight = (int)Math.Round(shadeamount.Value * lightmul);
					targetcolor = InterpolationTools.InterpolateColor(shadecolor.Color, lightcolor.Color, anglediff);
				}

				// Apply settings
				if(group.Key.Fields.GetValue("lightfloorabsolute", false))
					UniFields.SetInteger(group.Key.Fields, "lightfloor", General.Clamp(targetlight + sectorbrightness[group.Key], 0, 255), 0);
				else
					UniFields.SetInteger(group.Key.Fields, "lightfloor", General.Clamp(targetlight, -255, 255), 0);

				// Apply sector color
				int c = (targetcolor.ToInt() & 0x00FFFFFF);

				// Restore initial color?
				if(c == PixelColor.INT_WHITE_NO_ALPHA) c = sectorcolors[group.Key];

				// Apply color
				UniFields.SetInteger(group.Key.Fields, "lightcolor", c, PixelColor.INT_WHITE_NO_ALPHA);

				// Mark for update
				group.Key.UpdateNeeded = true;
			}

			// Update sidedef shading
			foreach(KeyValuePair<Sidedef, Vector3D> group in sides)
			{
				// Calculate light amount
				float anglediff = Vector3D.DotProduct(group.Value, sunvector);
				int targetlight;

				// Calculate light and light color when surface normal is rotated towards the sun vector
				if(anglediff >= 0.5f)
				{
					float lightmul = (anglediff - 0.5f) * 2.0f;
					targetlight = (int)Math.Round(lightamount.Value * lightmul);
				}
				// Otherwise calculate shade and shade color
				else
				{
					float lightmul = (0.5f - anglediff) * -2.0f;
					targetlight = (int)Math.Round(shadeamount.Value * lightmul);
				}

				// Apply settings
				if(group.Key.Fields.GetValue("lightabsolute", false))
					UniFields.SetInteger(group.Key.Fields, "light", General.Clamp(targetlight + sidebrightness[group.Key], 0, 255), 0);
				else
					UniFields.SetInteger(group.Key.Fields, "light", General.Clamp(targetlight, -255, 255), 0);

				// Mark for update
				group.Key.Sector.UpdateNeeded = true;
			}

			// Update map
			General.Map.Map.Update();
			General.Map.IsChanged = true;

			// Update view
			if(visualsectors != null)
			{
				foreach(var vs in visualsectors) vs.UpdateSectorData();
			}
			else if(sectors.Count > 0)
			{
				General.Interface.RedrawDisplay();
			}
		}

		private void OnSunAngleChanged()
		{
			// TODO: Altitude?
			sunvector = Vector3D.FromAngleXYZ(Angle2D.DegToRad(sunangle.Angle + 90), Angle2D.DegToRad(45));
			UpdateShading();
		}

		#endregion

		#region ================== Events

		private void sunangle_AngleChanged(object sender, EventArgs e)
		{
			if(blockupdate) return;

			blockupdate = true;
			sunangletb.Text = sunangle.Angle.ToString();
			blockupdate = false;

			OnSunAngleChanged();
		}

		private void sunangletb_WhenTextChanged(object sender, EventArgs e)
		{
			if(blockupdate) return;

			blockupdate = true;
			sunangle.Angle = sunangletb.GetResult(sunangle.Angle);
			blockupdate = false;

			OnSunAngleChanged();
		}

		private void OnShadingChanged(object sender, EventArgs e)
		{
			if(!blockupdate) UpdateShading();
		}

		private void apply_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void DirectionalShadingForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(this.DialogResult == DialogResult.OK)
			{
				// Save settings
				General.Settings.WritePluginSetting("directionalshading.sunangle", sunangle.Angle);
				General.Settings.WritePluginSetting("directionalshading.lightamount", lightamount.Value);
				General.Settings.WritePluginSetting("directionalshading.lightcolor", lightcolor.Color.ToInt());
				General.Settings.WritePluginSetting("directionalshading.shadeamount", shadeamount.Value);
				General.Settings.WritePluginSetting("directionalshading.shadecolor", shadecolor.Color.ToInt());
			}
			else
			{
				// Undo changes
				General.Map.UndoRedo.WithdrawUndo(); 
			}
		}

		#endregion

	}
}
