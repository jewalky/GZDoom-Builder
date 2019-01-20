using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
	public partial class ObjExporterSettingsForm : Form
	{
		public bool ExportTextures { get { return exportTextures; } }
		private bool exportTextures;

		public bool FixScale { get { return fixScale; } }
		private bool fixScale;

		public string FilePath { get { return filePath; } }
		private string filePath;
		
		public ObjExporterSettingsForm() {
			InitializeComponent();
			saveFileDialog.InitialDirectory = General.Map.FilePathName;
			saveFileDialog.FileName = General.Map.FileTitle + "_" + General.Map.Options.LevelName;
		}

		private void browse_Click(object sender, EventArgs e) {
			if(saveFileDialog.ShowDialog() == DialogResult.OK) {
				tbExportPath.Text = saveFileDialog.FileName;
			}
		}

		private void export_Click(object sender, EventArgs e) {
			//verify path
			filePath = tbExportPath.Text.Trim();

			if(Directory.Exists(Path.GetDirectoryName(filePath))) {
				exportTextures = cbExportTextures.Checked;
				fixScale = cbFixScale.Checked;
				filePath = tbExportPath.Text;

				this.DialogResult = DialogResult.OK;
				this.Close();
			} else {
				MessageBox.Show("Selected path does not exist!");
			}
		}

		private void cancel_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}
