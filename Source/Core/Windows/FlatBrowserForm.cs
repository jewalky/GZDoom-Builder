
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.IO;
using Microsoft.Win32;
using System.Diagnostics;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Controls;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class FlatBrowserForm : Form
	{
		// Constants
		private const int COLUMN_WIDTH_COUNT = 52;
		
		// Variables
		private string selectedname;
		private Point lastposition;
		private Size lastsize;
		private ListViewGroup usedgroup;
		private ListViewGroup availgroup;
		private ListViewItem selectedset;
		private string selecttextureonfill;
		
		// Properties
		public string SelectedName { get { return selectedname; } }
		
		// Constructor
		public FlatBrowserForm(string selecttexture)
		{
			Cursor.Current = Cursors.WaitCursor;
			ListViewItem item;
			bool foundselecttexture = false;
			long longname = Lump.MakeLongName(selecttexture ?? "");
			
			// Initialize
			InitializeComponent();
			browser.ApplySettings();
			
			// Update the used textures
			General.Map.Data.UpdateUsedTextures();

			// Resize columns to maximize available width
			countcolumn.Width = COLUMN_WIDTH_COUNT;
			namecolumn.Width = texturesets.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth - countcolumn.Width - 2;

			// Fill texture sets list with normal texture sets
			foreach(IFilledTextureSet ts in General.Map.Data.TextureSets)
			{
				item = texturesets.Items.Add(ts.Name);
				item.Tag = ts;
				item.ImageIndex = 0;
				item.UseItemStyleForSubItems = false;
				item.SubItems.Add(ts.Textures.Count.ToString(), item.ForeColor,
						item.BackColor, new Font(item.Font, FontStyle.Regular));
			}
			
			// Add container-specific texture sets
			foreach(ResourceTextureSet ts in General.Map.Data.ResourceTextureSets)
			{
				item = texturesets.Items.Add(ts.Name);
				item.Tag = ts;
				item.ImageIndex = 2 + ts.Location.type;
				item.UseItemStyleForSubItems = false;
				item.SubItems.Add(ts.Textures.Count.ToString(), item.ForeColor,
						item.BackColor, new Font(item.Font, FontStyle.Regular));
			}

			// Add All textures set
			item = texturesets.Items.Add(General.Map.Data.AllTextureSet.Name);
			item.Tag = General.Map.Data.AllTextureSet;
			item.ImageIndex = 1;
			item.UseItemStyleForSubItems = false;
			item.SubItems.Add(General.Map.Data.AllTextureSet.Textures.Count.ToString(),
				item.ForeColor, item.BackColor, new Font(item.Font, FontStyle.Regular));

			// Select the last one that was selected
			string selectname = General.Settings.ReadSetting("browserwindow.textureset", "");
			foreach(ListViewItem i in texturesets.Items)
			{
				if(i.Text == selectname)
				{
					IFilledTextureSet set = (i.Tag as IFilledTextureSet);
					foreach(ImageData img in set.Textures)
					{
						if(img.LongName == longname)
						{
							i.Selected = true;
							foundselecttexture = true;
							break;
						}
					}
					break;
				}
			}

			// If the selected texture was not found in the last-selected set, try finding it in the other sets
			if(!foundselecttexture)
			{
				foreach(ListViewItem i in texturesets.Items)
				{
					IFilledTextureSet set = (i.Tag as IFilledTextureSet);
					foreach(ImageData img in set.Textures)
					{
						if(img.LongName == longname)
						{
							i.Selected = true;
							foundselecttexture = true;
							break;
						}
					}
				}
			}

			// Texture still now found? Then just select the last used set
			if(!foundselecttexture)
			{
				foreach(ListViewItem i in texturesets.Items)
				{
					if(i.Text == selectname)
					{
						i.Selected = true;
						foundselecttexture = true;
						break;
					}
				}
			}

			// WARNING: Some strange behavior of the listview here!
			// When you leave this line out, the list becomes very slow.
			// Also, this does not change the item selected previously.
			texturesets.Items[0].Selected = true;

			// Texture to select when list is filled
			selecttextureonfill = selecttexture;

			// Make groups
			usedgroup = browser.AddGroup("Used Textures");
			availgroup = browser.AddGroup("Available Textures");
			
			// Keep last position and size
			lastposition = this.Location;
			lastsize = this.Size;
			
			// Position window from configuration settings
			this.SuspendLayout();
			/*
			this.Location = new Point(General.Settings.ReadSetting("browserwindow.positionx", this.Location.X),
									  General.Settings.ReadSetting("browserwindow.positiony", this.Location.Y));
			*/
			this.Size = new Size(General.Settings.ReadSetting("browserwindow.sizewidth", this.Size.Width),
								 General.Settings.ReadSetting("browserwindow.sizeheight", this.Size.Height));
			this.WindowState = (FormWindowState)General.Settings.ReadSetting("browserwindow.windowstate", (int)FormWindowState.Normal);
			if(this.WindowState == FormWindowState.Normal) this.StartPosition = FormStartPosition.CenterParent;
			this.ResumeLayout(true);
		}
		
		// Selection changed
		private void browser_SelectedItemChanged()
		{
			apply.Enabled = (browser.SelectedItem != null);
		}

		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			// Set selected name and close
			if(browser.SelectedItem != null)
			{
				selectedname = browser.SelectedItem.Text;
				DialogResult = DialogResult.OK;
			}
			else
			{
				selectedname = "";
				DialogResult = DialogResult.Cancel;
			}
			this.Close();
		}

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// No selection, close
			selectedname = "";
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// Activated
		private void FlatBrowserForm_Activated(object sender, EventArgs e)
		{
			// Focus the textbox
			browser.FocusTextbox();
			Cursor.Current = Cursors.Default;
		}

		// Loading
		private void FlatBrowserForm_Load(object sender, EventArgs e)
		{
			// Normal windowstate?
			if(this.WindowState == FormWindowState.Normal)
			{
				// Keep last position and size
				lastposition = this.Location;
				lastsize = this.Size;
			}
		}

		// Resized
		private void FlatBrowserForm_ResizeEnd(object sender, EventArgs e)
		{
			// Normal windowstate?
			if(this.WindowState == FormWindowState.Normal)
			{
				// Keep last position and size
				lastposition = this.Location;
				lastsize = this.Size;
			}
		}

		// Moved
		private void FlatBrowserForm_Move(object sender, EventArgs e)
		{
			// Normal windowstate?
			if(this.WindowState == FormWindowState.Normal)
			{
				// Keep last position and size
				lastposition = this.Location;
				lastsize = this.Size;
			}
		}

		// Closing
		private void FlatBrowserForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			int windowstate;

			// Determine window state to save
			if(this.WindowState != FormWindowState.Minimized)
				windowstate = (int)this.WindowState;
			else
				windowstate = (int)FormWindowState.Normal;

			// Save window settings
			General.Settings.WriteSetting("browserwindow.positionx", lastposition.X);
			General.Settings.WriteSetting("browserwindow.positiony", lastposition.Y);
			General.Settings.WriteSetting("browserwindow.sizewidth", lastsize.Width);
			General.Settings.WriteSetting("browserwindow.sizeheight", lastsize.Height);
			General.Settings.WriteSetting("browserwindow.windowstate", windowstate);
			
			// Save last selected texture set
			if(texturesets.SelectedItems.Count > 0)
				General.Settings.WriteSetting("browserwindow.textureset", texturesets.SelectedItems[0].Text);
			
			// Clean up
			browser.CleanUp();
		}

		// Static method to browse for flats
		// Returns null when cancelled.
		public static string Browse(IWin32Window parent, string select)
		{
			FlatBrowserForm browser = new FlatBrowserForm(select);
			if(browser.ShowDialog(parent) == DialogResult.OK)
			{
				// Return result
				return browser.SelectedName;
			}
			else
			{
				// Cancelled
				return select;
			}
		}
		
		// Texture set selected
		private void texturesets_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Anything slected?
			if(texturesets.SelectedItems.Count > 0)
			{
				selectedset = texturesets.SelectedItems[0];
				FillImagesList(null);
			}
		}

		// Item double clicked
		private void browser_SelectedItemDoubleClicked()
		{
			if(apply.Enabled) apply_Click(this, EventArgs.Empty);
		}

		// This fills the list of textures, depending on the selected texture set
		private void FillImagesList(string selectflat)
		{
			// Get the selected texture set
			IFilledTextureSet set = (selectedset.Tag as IFilledTextureSet);

			// Start adding
			browser.BeginAdding(false);

			// Add all used flats
			foreach(ImageData img in set.Flats)
				if(img.UsedInMap) browser.Add(img.Name, img, img, usedgroup);

			// Add all available flats
			foreach(ImageData img in set.Flats)
				browser.Add(img.Name, img, img, availgroup);

			// Done adding
			browser.EndAdding();
		}

		// Help requested
		private void FlatBrowserForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			General.ShowHelp("w_imagesbrowser.html");
			hlpevent.Handled = true;
		}

		private void FlatBrowserForm_Shown(object sender, EventArgs e)
		{
			// Select texture
			if(!string.IsNullOrEmpty(selecttextureonfill))
			{
				browser.SelectItem(selecttextureonfill, usedgroup);
				selecttextureonfill = null;
			}
		}
	}
}