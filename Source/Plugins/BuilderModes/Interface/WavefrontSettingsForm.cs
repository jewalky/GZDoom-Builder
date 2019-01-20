#region ================== Namespaces

using System;
using System.Windows.Forms;
using System.IO;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
	public partial class WavefrontSettingsForm : DelayedForm
	{
		#region ================== Properties

		public string FilePath { get { return tbExportPath.Text.Trim(); } }
		public bool ExportTextures { get { return cbExportTextures.Checked; } }
		public bool UseGZDoomScale { get { return cbFixScale.Checked; } }
		public float ObjScale { get { return (float)nudScale.Value; } }

		#endregion

		public WavefrontSettingsForm(int sectorsCount) 
		{
			InitializeComponent();

			string name = Path.GetFileNameWithoutExtension(General.Map.FileTitle) + "_" + General.Map.Options.LevelName + ".obj";
			if(string.IsNullOrEmpty(General.Map.FilePathName)) 
			{
				saveFileDialog.FileName = name;
			} 
			else 
			{
				saveFileDialog.InitialDirectory = General.Map.FilePathName;
				saveFileDialog.FileName = Path.GetDirectoryName(General.Map.FilePathName) + Path.DirectorySeparatorChar + name;
				tbExportPath.Text = saveFileDialog.FileName;
			}

			//restore settings
			cbExportTextures.Checked = General.Settings.ReadPluginSetting("objexporttextures", false);
			cbFixScale.Checked = General.Settings.ReadPluginSetting("objgzdoomscale", false);
			nudScale.Value = (decimal)General.Settings.ReadPluginSetting("objscale", 1.0f);

			this.Text = "Export " + (sectorsCount == -1 ? "whole map" : sectorsCount + (sectorsCount > 1 ? "sectors" : "sector")) + " to Wavefront .obj";
		}

		#region ================== Events

		private void browse_Click(object sender, EventArgs e) 
		{
			if(saveFileDialog.ShowDialog() == DialogResult.OK) 
				tbExportPath.Text = saveFileDialog.FileName;
		}

		private void export_Click(object sender, EventArgs e) 
		{
			//check settings
			if(nudScale.Value == 0) 
			{
				MessageBox.Show("Scale should not be zero!");
				return;
			}
			if(!Directory.Exists(Path.GetDirectoryName(tbExportPath.Text))) 
			{
				MessageBox.Show("Selected path does not exist!");
				return;
			}

			//save settings
			General.Settings.WritePluginSetting("objexporttextures", cbExportTextures.Checked);
			General.Settings.WritePluginSetting("objgzdoomscale", cbFixScale.Checked);
			General.Settings.WritePluginSetting("objscale", (float)nudScale.Value);
			
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
