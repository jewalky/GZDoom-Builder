
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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Editing;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class GridSetupForm : DelayedForm
	{
		// Variables
		private string backgroundname;
		private int backgroundsource;
		
		// Constructor
		public GridSetupForm()
		{
			// Initialize
			InitializeComponent();

			// Show grid size
			gridsize.Text = General.Map.Grid.GridSizeF.ToString();
			
			// Background image?
			if((General.Map.Grid.Background != null) &&
			   !(General.Map.Grid.Background is UnknownImage))
			{
				// Show background image
				showbackground.Checked = true;
				backgroundname = General.Map.Grid.BackgroundName;
				backgroundsource = General.Map.Grid.BackgroundSource;
				General.DisplayZoomedImage(backgroundimage, General.Map.Grid.Background.GetBitmap());
			}
			else
			{
				// No background image
				showbackground.Checked = false;
			}

			// Show background offset
			backoffsetx.Text = General.Map.Grid.BackgroundX.ToString();
			backoffsety.Text = General.Map.Grid.BackgroundY.ToString();
			int scalex = (int)(General.Map.Grid.BackgroundScaleX * 100.0f);
			int scaley = (int)(General.Map.Grid.BackgroundScaleY * 100.0f);
			backscalex.Text = General.Clamp(scalex, 1, 10000).ToString();
			backscaley.Text = General.Clamp(scaley, 1, 10000).ToString();
		}

		// Show Background changed
		private void showbackground_CheckedChanged(object sender, EventArgs e)
		{
			// Enable/disable controls
			selecttexture.Enabled = showbackground.Checked;
			selectflat.Enabled = showbackground.Checked;
			selectfile.Enabled = showbackground.Checked;
			backoffset.Enabled = showbackground.Checked;
			backscale.Enabled = showbackground.Checked;
			backoffsetx.Enabled = showbackground.Checked;
			backoffsety.Enabled = showbackground.Checked;
			backscalex.Enabled = showbackground.Checked;
			backscaley.Enabled = showbackground.Checked;
		}

		// Browse texture
		private void selecttexture_Click(object sender, EventArgs e)
		{
			// Browse for texture
			string result = TextureBrowserForm.Browse(this, backgroundname, false);
			if(result != null)
			{
				// Set this texture as background
				backgroundname = result;
				backgroundsource = GridSetup.SOURCE_TEXTURES;
				ImageData img = General.Map.Data.GetTextureImage(result);
				img.LoadImage();
				General.DisplayZoomedImage(backgroundimage, img.GetBitmap());
			}
		}

		// Browse flat
		private void selectflat_Click(object sender, EventArgs e)
		{
			// Browse for flat
			string result = TextureBrowserForm.Browse(this, backgroundname, true);
			if(result != null)
			{
				// Set this flat as background
				backgroundname = result;
				backgroundsource = GridSetup.SOURCE_FLATS;
				ImageData img = General.Map.Data.GetFlatImage(result);
				img.LoadImage();
				General.DisplayZoomedImage(backgroundimage, img.GetBitmap());
			}
		}

		// Browse file
		private void selectfile_Click(object sender, EventArgs e)
		{
			// Browse for file
			if(browsefile.ShowDialog(this) == DialogResult.OK)
			{
				// Set this file as background
				backgroundname = browsefile.FileName;
				backgroundsource = GridSetup.SOURCE_FILE;
				ImageData img = new FileImage(Path.GetFileNameWithoutExtension(backgroundname), backgroundname, false, 1.0f, 1.0f);
				img.LoadImage();
				General.DisplayZoomedImage(backgroundimage, new Bitmap(img.GetBitmap()));
				img.Dispose();
			}
		}
		
		// Cancelled
		private void cancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// Apply
		private void apply_Click(object sender, EventArgs e)
		{
			//mxd. Apply
			float newgridsize = gridsize.GetResultFloat(General.Map.Grid.GridSizeF);
			if(newgridsize != General.Map.Grid.GridSizeF)
			{
				//Disable automatic grid resizing
				General.MainWindow.DisableDynamicGridResize();

				//Apply grid size
				General.Map.Grid.SetGridSize(newgridsize);
			}
			
			// Apply
			General.Map.Grid.SetBackgroundView(backoffsetx.GetResult(General.Map.Grid.BackgroundX),
											   backoffsety.GetResult(General.Map.Grid.BackgroundY),
											   backscalex.GetResult((int)(General.Map.Grid.BackgroundScaleX * 100.0f)) / 100.0f,
											   backscaley.GetResult((int)(General.Map.Grid.BackgroundScaleY * 100.0f)) / 100.0f);
			
			// Background image?
			if(showbackground.Checked)
			{
				// Set background image
				General.Map.Grid.SetBackground(backgroundname, backgroundsource);
			}
			else
			{
				// No background image
				General.Map.Grid.SetBackground(null, 0);
			}

			// Done
			DialogResult = DialogResult.OK;
			this.Close();
		}

		// Help
		private void GridSetupForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			General.ShowHelp("w_gridsetup.html");
			hlpevent.Handled = true;
		}
	}
}