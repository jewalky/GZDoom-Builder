#region ================== Namespaces

using System;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderEffects
{
	public partial class ObjImportSettingsForm : DelayedForm
	{
		#region ================== Variables

		private ImportObjAsTerrainMode.UpAxis axis;
		private bool slopessupported;

		#endregion

		#region ================== Properties

		internal string FilePath { get { return tbImportPath.Text.Trim(); } }
		internal ImportObjAsTerrainMode.UpAxis UpAxis { get { return axis; } }
		internal float ObjScale { get { return (float)nudScale.Value; } }
		internal bool UseVertexHeights { get { return slopessupported && cbusevertexheight.Checked; } }

		//todo: floor/ceiling textures? height offsets? ceiling extra height?

		#endregion

		public ObjImportSettingsForm() 
		{
			InitializeComponent();

			//restore settings
			axis = (ImportObjAsTerrainMode.UpAxis)General.Settings.ReadPluginSetting("objexportupaxis", 0);
			nudScale.Value = (decimal)General.Settings.ReadPluginSetting("objexportscale", 1.0f);
			slopessupported = (General.Map.UDMF || General.Map.Data.GetThingInfoEx(ImportObjAsTerrainMode.VERTEX_HEIGHT_THING_TYPE) != null);
			if(slopessupported) 
				cbusevertexheight.Checked = General.Settings.ReadPluginSetting("objusevertexheights", true);
			else 
				cbusevertexheight.Enabled = false;

			switch(axis) 
			{
				case ImportObjAsTerrainMode.UpAxis.X: axisx.Checked = true; break;
				case ImportObjAsTerrainMode.UpAxis.Y: axisy.Checked = true; break;
				case ImportObjAsTerrainMode.UpAxis.Z: axisz.Checked = true; break;
				default: axisy.Checked = true; break;
			}
		}

		#region ================== Events

		private void browse_Click(object sender, EventArgs e) 
		{
			if(openFileDialog.ShowDialog() == DialogResult.OK) tbImportPath.Text = openFileDialog.FileName;
		}

		private void import_Click(object sender, EventArgs e) 
		{
			if(nudScale.Value == 0) 
			{
				MessageBox.Show("Scale should not be zero!");
				return;
			}
			if(!File.Exists(tbImportPath.Text)) 
			{
				MessageBox.Show("Selected path does not exist!");
				return;
			}

			axis = (axisy.Checked ? ImportObjAsTerrainMode.UpAxis.Y : (axisz.Checked ? ImportObjAsTerrainMode.UpAxis.Z : ImportObjAsTerrainMode.UpAxis.X));

			//save settings
			General.Settings.WritePluginSetting("objexportupaxis", (int)axis);
			General.Settings.WritePluginSetting("objexportscale", (float)nudScale.Value);
			if(slopessupported) General.Settings.WritePluginSetting("objusevertexheights", cbusevertexheight.Checked);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e) 
		{
			this.Close();
		}

		#endregion
	}
}
