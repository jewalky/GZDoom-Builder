using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.Windows
{
	public partial class ChangeMapForm : DelayedForm
	{
		private MapOptions options;
		private Configuration mapsettings;
		private readonly string filepathname;

		public MapOptions Options { get { return options; } }

		public ChangeMapForm(string filepathname, MapOptions options) 
		{
			InitializeComponent();
			this.options = options;
			this.filepathname = filepathname;
		}

		private void LoadSettings() 
		{
			// Check if the file exists
			if(!File.Exists(filepathname))
			{
				// WAD file does not exist
				MessageBox.Show(this, "Could not open the WAD file. The file does not exist.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.Cancel;
				this.Close();
				return;
			}

			// Busy
			Cursor.Current = Cursors.WaitCursor;

			WAD wadfile;
			try
			{
				// Open the WAD file
				wadfile = new WAD(filepathname, true);
			}
			catch(Exception)
			{
				// Unable to open WAD file (or its config)
				MessageBox.Show(this, "Could not open the WAD file for reading. Please make sure the file you selected is valid and is not in use by any other application.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.Cancel;
				this.Close();
				return;
			}

			// Make an array for the map names
			List<ListViewItem> mapnames = new List<ListViewItem>();

			// Open the Map Settings configuration
			string dbsfile = filepathname.Substring(0, filepathname.Length - 4) + ".dbs";
			if(File.Exists(dbsfile))
			{
				try
				{
					mapsettings = new Configuration(dbsfile, true);
				}
				catch(Exception)
				{
					mapsettings = new Configuration(true);
				}
			}
			else
			{
				mapsettings = new Configuration(true);
			}

			//mxd. Get Proper configuration
			ConfigurationInfo ci = General.GetConfigurationInfo(options.ConfigFile);

			// Get the map lump names
			IDictionary maplumpnames = ci.Configuration.ReadSetting("maplumpnames", new Hashtable());

			// Count how many required lumps we have to find
			int lumpsrequired = 0;
			foreach(DictionaryEntry ml in maplumpnames) 
			{
				// Ignore the map header (it will not be found because the name is different)
				if(ml.Key.ToString() != MapManager.CONFIG_MAP_HEADER) 
				{
					// Read lump setting and count it
					if(ci.Configuration.ReadSetting("maplumpnames." + ml.Key + ".required", false))
						lumpsrequired++;
				}
			}

			// Go for all the lumps in the wad
			for(int scanindex = 0; scanindex < (wadfile.Lumps.Count - 1); scanindex++) 
			{
				// Make sure this lump is not part of the map
				if(!maplumpnames.Contains(wadfile.Lumps[scanindex].Name)) 
				{
					// Reset check
					int lumpsfound = 0;
					int checkoffset = 1;

					// Continue while still within bounds and lumps are still recognized
					while(((scanindex + checkoffset) < wadfile.Lumps.Count) &&
						  maplumpnames.Contains(wadfile.Lumps[scanindex + checkoffset].Name)) 
					{
						// Count the lump when it is marked as required
						string lumpname = wadfile.Lumps[scanindex + checkoffset].Name;
						if(ci.Configuration.ReadSetting("maplumpnames." + lumpname + ".required", false))
							lumpsfound++;

						// Check the next lump
						checkoffset++;
					}

					// Map found? Then add it to the list
					if(lumpsfound >= lumpsrequired)
						mapnames.Add(new ListViewItem(wadfile.Lumps[scanindex].Name));
				}
			}

			wadfile.Dispose();

			// Clear the list and add the new map names
			mapslist.BeginUpdate();
			mapslist.Items.Clear();
			mapslist.Items.AddRange(mapnames.ToArray());
			mapslist.Sort();

			//select current map
			foreach(ListViewItem item in mapslist.Items) 
			{
				// Was this item previously selected?
				if(item.Text == options.LevelName) 
				{
					// Select it again
					item.Selected = true;
					item.EnsureVisible();
					break;
				}
			}

			mapslist.EndUpdate();

			// Do some focus managing
			if(mapslist.SelectedItems.Count > 0)
			{
				mapslist.FocusedItem = mapslist.SelectedItems[0];
			}
			
			// Done
			Cursor.Current = Cursors.Default;
		}

		private void ChangeMapForm_Shown(object sender, EventArgs e)
		{
			LoadSettings();
		}

		private void mapslist_DoubleClick(object sender, EventArgs e) 
		{
			// Click OK
			if(mapslist.SelectedItems.Count > 0) apply.PerformClick();
		}

		private void apply_Click(object sender, EventArgs e) 
		{
			// No map selected?
			if(mapslist.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "Please select a map to load for editing.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				mapslist.Focus();
				return;
			}

			// Current map is already loaded
			if(mapslist.SelectedItems[0].Text == options.LevelName)
			{
				MessageBox.Show(this, "Map \"" + options.LevelName + "\" is already loaded.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				mapslist.Focus();
				return;
			}

			// Just NO...
			if(!General.Map.ConfigSettings.ValidateMapName(mapslist.SelectedItems[0].Text.ToUpperInvariant())) 
			{
				MessageBox.Show(this, "Selected map name conflicts with a lump name defined for current map format.\nPlease rename the map and try again.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				mapslist.Focus();
				return;
			}

			// Create new map options, pass settings which should stay unchanged
			//TODO: are there other settings which should stay unchanged?..
			MapOptions newoptions = new MapOptions(mapsettings, mapslist.SelectedItems[0].Text, options.UseLongTextureNames);
			newoptions.ConfigFile = options.ConfigFile;
			newoptions.ScriptCompiler = options.ScriptCompiler;
			newoptions.CopyResources(options.Resources);
			options = newoptions;
			
			// Hide window
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e) 
		{
			// Just hide the window
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
