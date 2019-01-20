
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Windows.Forms;
using System.IO;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class ResourceOptionsForm : DelayedForm
	{
		// Variables
		private DataLocation res;
        private string startPath;
        private Controls.FolderSelectDialog dirdialog;

        // Properties
        public DataLocation ResourceLocation { get { return res; } }
		
		// Constructor
		public ResourceOptionsForm(DataLocation settings, string caption, string startPath) //mxd. added startPath
		{
			// Initialize
			InitializeComponent();

			// Set caption
			this.Text = caption;
			
			// Apply settings from ResourceLocation
			this.res = settings;
			switch(res.type)
			{
				// Setup for WAD File
				case DataLocation.RESOURCE_WAD:
					wadlocation.Text = res.location;
					strictpatches.Checked = res.option1;
					break;

				// Setup for Directory
				case DataLocation.RESOURCE_DIRECTORY:
					dirlocation.Text = res.location;
					dir_textures.Checked = res.option1;
					dir_flats.Checked = res.option2;
					break;
					
				// Setup for PK3 File
				case DataLocation.RESOURCE_PK3:
					pk3location.Text = res.location;
					break;
			}
			
			// Select appropriate tab
			tabs.SelectedIndex = res.type;
			
			// Checkbox
			notfortesting.Checked = res.notfortesting;

            this.startPath = startPath;
		}
		
		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			// Apply settings to ResourceLocation
			switch(tabs.SelectedIndex)
			{
				// Setup WAD File
				case DataLocation.RESOURCE_WAD:

					// Check if file is specified
					if((wadlocation.Text.Length == 0) ||
					   (!File.Exists(wadlocation.Text)))
					{
						// No valid wad file specified
						MessageBox.Show(this, "Please select a valid WAD File resource.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					else
					{
						// Apply settings
						res.type = DataLocation.RESOURCE_WAD;
						res.location = wadlocation.Text;
						res.option1 = strictpatches.Checked;
						res.option2 = false;
						res.notfortesting = notfortesting.Checked;

						// Done
						this.DialogResult = DialogResult.OK;
						this.Close();
					}
					break;

				// Setup Directory
				case DataLocation.RESOURCE_DIRECTORY:

					// Check if directory is specified
					if((dirlocation.Text.Length == 0) ||
					   (!Directory.Exists(dirlocation.Text)))
					{
						// No valid directory specified
						MessageBox.Show(this, "Please select a valid directory resource.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					else
					{
						// Apply settings
						res.type = DataLocation.RESOURCE_DIRECTORY;
						res.location = dirlocation.Text;
						res.option1 = dir_textures.Checked;
						res.option2 = dir_flats.Checked;
						res.notfortesting = notfortesting.Checked;

						// Done
						this.DialogResult = DialogResult.OK;
						this.Close();
					}
					break;
					
				// Setup PK3 File
				case DataLocation.RESOURCE_PK3:

					// Check if file is specified
					if((pk3location.Text.Length == 0) ||
					   (!File.Exists(pk3location.Text)))
					{
						// No valid pk3 file specified
						MessageBox.Show(this, "Please select a valid PK3 or PK7 File resource.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					else
					{
						// Apply settings
						res.type = DataLocation.RESOURCE_PK3;
						res.location = pk3location.Text;
						res.option1 = false;
						res.option2 = false;
						res.notfortesting = notfortesting.Checked;

						// Done
						this.DialogResult = DialogResult.OK;
						this.Close();
					}
					break;
			}
		}
		
		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// Just hide
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// Browse WAD File clicked
		private void browsewad_Click(object sender, EventArgs e)
		{
			// Browse for WAD File
			if(wadfiledialog.ShowDialog(this) == DialogResult.OK)
			{
				// Use this file
				wadlocation.Text = wadfiledialog.FileName;
			}
		}

		// Browse Directory clicked
		private void browsedir_Click(object sender, EventArgs e)
		{
            // Browse for Directory
            dirdialog = new Controls.FolderSelectDialog();
            dirdialog.Title = "Select Resource Folder";

            if (string.IsNullOrEmpty(dirlocation.Text) || !Directory.Exists(dirlocation.Text))
            {
                //mxd
                if (!string.IsNullOrEmpty(startPath))
                {
                    string startDir = Path.GetDirectoryName(startPath);
                    if (Directory.Exists(startDir)) dirdialog.InitialDirectory = startDir + '\\';
                }
            }
            else
            {
                dirdialog.InitialDirectory = dirlocation.Text;
            }

            if (dirdialog.ShowDialog(this.Handle))
			{
                // Use this directory
                dirlocation.Text = dirdialog.FileName;
                dirdialog = null;
			}
		}

		// Browse PK3 File clicked
		private void browsepk3_Click(object sender, EventArgs e)
		{
			// Browse for PK3 File
			if(pk3filedialog.ShowDialog(this) == DialogResult.OK)
			{
				// Use this file
				pk3location.Text = pk3filedialog.FileName;
			}
		}

		// Link clicked
		private void link_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			General.OpenWebsite("http://www.zdoom.org/wiki/Using_ZIPs_as_WAD_replacement");
		}

		// Help
		private void ResourceOptionsForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			General.ShowHelp("w_resourceoptions.html");
			hlpevent.Handled = true;
		}
	}
}