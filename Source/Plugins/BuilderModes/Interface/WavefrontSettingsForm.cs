#region ================== Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Controls;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
	public partial class WavefrontSettingsForm : DelayedForm
	{
		#region ================== Properties

		public string FilePath { get { return tbExportPath.Text.Trim(); } }
		public bool ExportTextures { get { return cbExportTextures.Checked; } }
		public bool UseGZDoomScale { get { return cbExportForGZDoom.Checked; } }
		public float ObjScale { get { return (float)nudScale.Value; } }
		public string BasePath { get { return tbBasePath.Text.Trim(); } }
		public string ActorPath { get { return tbActorPath.Text.Trim(); } }
		public string ModelPath { get { return tbModelPath.Text.Trim(); } }
		public string ActorName { get { return tbActorName.Text.Trim(); } }
		public List<string> SkipTextures { get { return lbSkipTextures.Items.Cast<string>().ToList(); } }
		public bool IgnoreControlSectors { get { return cbIgnoreControlSectors.Checked; } }
		public bool NormalizeLowestVertex { get { return cbNormalizeLowestVertex.Checked; } }
		public bool CenterModel { get { return cbCenterModel.Checked; } }
		public bool NoGravity { get { return cbNoGravity.Checked; } }
		public bool SpawnOnCeiling { get { return cbSpawnOnCeiling.Checked; } }
		public bool Solid { get { return cbSolid.Checked; } }
		public bool ZScript { get { return rbZScript.Checked; } }
		public bool GenerateCode { get { return cbGenerateCode.Checked; } }
		public bool GenerateModeldef { get { return cbGenerateModeldef.Checked; } }
		public string Sprite { get { return tbSprite.Text.Trim().ToUpperInvariant(); } }

		#endregion

		public WavefrontSettingsForm(int sectorsCount) 
		{
			InitializeComponent();

			string name = Path.GetFileNameWithoutExtension(General.Map.FileTitle) + "_" + General.Map.Options.LevelName + ".obj";
			if(string.IsNullOrEmpty(General.Map.FilePathName)) 
			{
				string tmpPath = Path.GetTempPath();
				saveFileDialog.InitialDirectory = tmpPath;
				saveFileDialog.FileName = Path.GetDirectoryName(tmpPath) + Path.DirectorySeparatorChar + name;
				tbExportPath.Text = saveFileDialog.FileName;
			} 
			else 
			{
				saveFileDialog.InitialDirectory = General.Map.FilePathName;
				saveFileDialog.FileName = Path.GetDirectoryName(General.Map.FilePathName) + Path.DirectorySeparatorChar + name;
				tbExportPath.Text = saveFileDialog.FileName;
			}

			//restore settings
			cbExportTextures.Checked = General.Settings.ReadPluginSetting("objexporttextures", false);
			cbExportForGZDoom.Checked = General.Settings.ReadPluginSetting("objgzdoomscale", false);
			nudScale.Value = (decimal)General.Settings.ReadPluginSetting("objscale", 1.0f);

			this.Text = "Export " + (sectorsCount == -1 ? "whole map" : sectorsCount + (sectorsCount > 1 ? "sectors" : "sector")) + " to Wavefront .obj";

			if (General.Map.Config.MixTexturesFlats)
			{
				bAddFlat.Visible = false;
				bAddTexture.Width = bRemoveTexture.Width;
				bAddTexture.Text = "Add texture/flat";
			}

			string mapname = Path.GetFileNameWithoutExtension(General.Map.FileTitle);
			tbActorName.Text = char.ToUpper(mapname[0]) + mapname.Substring(1);

			string initialPath = General.Map.FilePathName == "" ? Path.GetTempPath() : General.Map.FilePathName;
			tbBasePath.Text = General.Settings.ReadPluginSetting("objbasepath", Path.GetDirectoryName(initialPath));
			tbActorPath.Text = General.Settings.ReadPluginSetting("objactorpath", Path.GetDirectoryName(initialPath));
			tbModelPath.Text = General.Settings.ReadPluginSetting("objmodelpath", Path.GetDirectoryName(initialPath));
			tbSprite.Text = General.Settings.ReadPluginSetting("objsprite", "PLAY");

			IDictionary skiptexture = General.Settings.ReadPluginSetting("objskiptextures", new Hashtable());
			foreach (DictionaryEntry de in skiptexture)
			{
				lbSkipTextures.Items.Add(de.Value);
			}

			cbGenerateCode.Checked = General.Settings.ReadPluginSetting("objgeneratecode", true);
			cbGenerateModeldef.Checked = General.Settings.ReadPluginSetting("objgeneratemodeldef", true);

			// Toggle enable/disable manually because cbFixScale is a child of the group box, so disabling
			// the group box would also disable cbFixScale
			//foreach (Control c in gbGZDoom.Controls)
			foreach (Control c in cbExportForGZDoom.Parent.Controls)
			{
				if (c != cbExportForGZDoom)
					c.Enabled = cbExportForGZDoom.Checked;
			}

			if (cbExportForGZDoom.Checked)
			{
				// gbActorFormat.Enabled = gbActorSettings.Enabled = cbGenerateCode.Checked;
				gbActorFormat.Enabled = gbActorSettings.Enabled = tbActorPath.Enabled = bBrowseActorPath.Enabled = cbGenerateCode.Checked;
				tbModelPath.Enabled = bBrowseModelPath.Enabled = cbGenerateModeldef.Checked && cbExportForGZDoom.Checked;
			}


		}

		#region ================== Methods

		private bool PathIsValid(string path)
		{
			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
				path += Path.DirectorySeparatorChar;

			return Directory.Exists(Path.GetDirectoryName(path));
		}

		#endregion

		#region ================== Events

		private void browse_Click(object sender, EventArgs e) 
		{
			if(saveFileDialog.ShowDialog() == DialogResult.OK) 
				tbExportPath.Text = saveFileDialog.FileName;
		}

		private void export_Click(object sender, EventArgs e) 
		{
			// Check settings
			if (cbExportForGZDoom.Checked)
			{
				if (tbActorName.Text.Trim().Length == 0)
				{
					MessageBox.Show("Actor name can not be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (Char.IsDigit(tbActorName.Text.Trim()[0]))
				{
					MessageBox.Show("Actor name can not start with a number!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (tbActorName.Text.Trim().Any(c => Char.IsWhiteSpace(c)))
				{
					MessageBox.Show("Actor name can not contain whitespace!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (!PathIsValid(tbBasePath.Text.Trim()))
				{
					MessageBox.Show("Base path does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (cbGenerateCode.Enabled && !PathIsValid(tbActorPath.Text.Trim()))
				{
					MessageBox.Show("Actor path does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (cbGenerateModeldef.Enabled && !PathIsValid(tbModelPath.Text.Trim()))
				{
					MessageBox.Show("Model path does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (cbExportForGZDoom.Checked && tbSprite.Text.Trim().Length != 4)
				{
					MessageBox.Show("Sprite name must be exactly 4 alphanumeric characters long!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
			else // Not exporting for GZDoom
			{
				if (nudScale.Value == 0)
				{
					MessageBox.Show("Scale should not be zero!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (!Directory.Exists(Path.GetDirectoryName(tbExportPath.Text)))
				{
					MessageBox.Show("Selected path does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}

			//save settings
			General.Settings.WritePluginSetting("objexporttextures", cbExportTextures.Checked);
			General.Settings.WritePluginSetting("objgzdoomscale", cbExportForGZDoom.Checked);
			General.Settings.WritePluginSetting("objscale", (float)nudScale.Value);
			General.Settings.WritePluginSetting("objbasepath", tbBasePath.Text);
			General.Settings.WritePluginSetting("objactorpath", tbActorPath.Text);
			General.Settings.WritePluginSetting("objmodelpath", tbModelPath.Text);
			General.Settings.WritePluginSetting("objsprite", tbSprite.Text.ToUpperInvariant());
			General.Settings.WritePluginSetting("objgeneratecode", cbGenerateCode.Checked);
			General.Settings.WritePluginSetting("objgeneratemodeldef", cbGenerateModeldef.Checked);

			Dictionary <string, string> skiptexture = new Dictionary<string, string>();
			int i = 0;
			foreach(string t in lbSkipTextures.Items)
			{
				skiptexture["texture" + i] = t;
				i++;
			}

			General.Settings.WritePluginSetting("objskiptextures", skiptexture);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e) 
		{
			this.Close();
		}

		#endregion

		private void cbFixScale_CheckedChanged(object sender, EventArgs e)
		{
			// Toggle enable/disable manually because cbFixScale is a child of the group box, so disabling
			// the group box would also disable cbFixScale
			foreach(Control c in cbExportForGZDoom.Parent.Controls)
			{
				if (c != cbExportForGZDoom && c != gbActorSettings && c != gbActorFormat)
					c.Enabled = cbExportForGZDoom.Checked;
			}

			gbActorSettings.Enabled = gbActorFormat.Enabled =  tbActorPath.Enabled = bBrowseActorPath.Enabled = cbGenerateCode.Checked && cbExportForGZDoom.Checked;
			tbModelPath.Enabled = bBrowseModelPath.Enabled = cbGenerateModeldef.Checked && cbExportForGZDoom.Checked;

			tbExportPath.Enabled = browse.Enabled = cbExportTextures.Enabled = nudScale.Enabled = !cbExportForGZDoom.Checked;
		}

		private void bAddTexture_Click(object sender, EventArgs e)
		{
			string name = General.Interface.BrowseTexture(General.Interface, "-");

			foreach(string n in lbSkipTextures.Items)
			{
				if (n == name)
					return;
			}

			lbSkipTextures.Items.Add(name);
		}

		private void bAddFlat_Click(object sender, EventArgs e)
		{
			string name = General.Interface.BrowseFlat(General.Interface, "-");

			foreach (string n in lbSkipTextures.Items)
			{
				if (n == name)
					return;
			}

			lbSkipTextures.Items.Add(name);
		}

		private void bRemoveTexture_Click(object sender, EventArgs e)
		{
			ListBox.SelectedObjectCollection items = new ListBox.SelectedObjectCollection(lbSkipTextures);

			for (int i = items.Count - 1; i >= 0; i--)
			{
				lbSkipTextures.Items.Remove(items[i]);
			}
		}

		private void bBrowseBasePath_Click(object sender, EventArgs e)
		{
			FolderSelectDialog dirdialog = new FolderSelectDialog();
			dirdialog.Title = "Select base folder";
			dirdialog.InitialDirectory = tbBasePath.Text;

			if (dirdialog.ShowDialog(this.Handle))
			{
				tbBasePath.Text = dirdialog.FileName;

				if (string.IsNullOrWhiteSpace(tbActorPath.Text))
					tbActorPath.Text = tbBasePath.Text;

				if (string.IsNullOrWhiteSpace(tbModelPath.Text))
					tbModelPath.Text = tbBasePath.Text;
			}
		}

		private void bBrowseActorPath_Click(object sender, EventArgs e)
		{
			FolderSelectDialog dirdialog = new FolderSelectDialog();
			dirdialog.Title = "Select actor folder";
			dirdialog.InitialDirectory = tbActorPath.Text;

			if (dirdialog.ShowDialog(this.Handle))
			{
				tbActorPath.Text = dirdialog.FileName;
			}
		}

		private void bBrowseModelPath_Click(object sender, EventArgs e)
		{
			FolderSelectDialog dirdialog = new FolderSelectDialog();
			dirdialog.Title = "Select model folder";
			dirdialog.InitialDirectory = tbModelPath.Text;

			if (dirdialog.ShowDialog(this.Handle))
			{
				tbModelPath.Text = dirdialog.FileName;
			}
		}

		private void bResetPaths_Click(object sender, EventArgs e)
		{
			tbBasePath.Text = tbActorPath.Text = tbModelPath.Text = Path.GetDirectoryName(General.Map.FilePathName);
		}

		private void cbSpawnOnCeiling_CheckedChanged(object sender, EventArgs e)
		{
			if (cbSpawnOnCeiling.Checked)
				cbNoGravity.Checked = true;

			cbNoGravity.Enabled = !cbSpawnOnCeiling.Checked;
		}

		private void tbActorName_TextChanged(object sender, EventArgs e)
		{
			string name = tbActorName.Text.Trim();
			bool haserror = false;
			string errortext = "";

			if (name.Length == 0)
			{
				errortext += "Actor name can not be empty";
				haserror = true;
			}
			else
			{
				if (name.Any(c => Char.IsWhiteSpace(c)))
				{
					errortext += "Actor name can not contain whitespace";
					haserror = true;
				}

				if (Char.IsDigit(name[0]))
				{
					if (errortext.Length > 0) errortext += "\n";
					errortext += "Actor name can not start with a digit";
					haserror = true;
				}
			}

			if (haserror)
			{
				tbActorName.BackColor = Color.FromArgb(255, 192, 192);
				toolTip1.SetToolTip(actorNameError, errortext);
				actorNameError.Visible = true;
			}
			else
			{
				tbActorName.BackColor = SystemColors.Window;
				actorNameError.Visible = false;
			}
		}

		private void cbGenerateCode_CheckedChanged(object sender, EventArgs e)
		{
			gbActorFormat.Enabled = gbActorSettings.Enabled = tbActorPath.Enabled = bBrowseActorPath.Enabled = cbGenerateCode.Checked && cbExportForGZDoom.Checked;
		}

		private void cbGenerateModeldef_CheckedChanged(object sender, EventArgs e)
		{
			tbModelPath.Enabled = bBrowseModelPath.Enabled = cbGenerateModeldef.Checked && cbExportForGZDoom.Checked;
		}
	}
}
