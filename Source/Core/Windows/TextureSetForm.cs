
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
using System.Windows.Forms;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class TextureSetForm : DelayedForm
	{
		// Variables
		private DefinedTextureSet textureset;
		
		// Constructor
		public TextureSetForm()
		{
			InitializeComponent();
            // [ZZ] change default texture size
            matcheslist.ImageSize = 64;
            //
            matcheslist.ApplySettings();
			
			// Show/hide components
			matchesbutton.Visible = (General.Map != null);
			nomatchesbutton.Visible = (General.Map != null);
			matcheslist.Visible = (General.Map != null);
			noresultlabel.Visible = (General.Map == null);
		}
		
		// This initializes the set
		public void Setup(DefinedTextureSet set)
		{
			// Keep reference
			textureset = set;
			
			// Set name
			name.Text = set.Name;
			
			// Fill filters list
			foreach(string s in set.Filters)
				filters.Items.Add(s);
		}

		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			// Apply name
			textureset.Name = name.Text;
			
			// Apply filters
			textureset.Filters.Clear();
			foreach(ListViewItem i in filters.Items) textureset.Filters.Add(i.Text);
			
			// Done
			matcheslist.CleanUp();
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// Be gone.
			matcheslist.CleanUp();
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
		
		// Add texture
		private void addfilter_Click(object sender, EventArgs e)
		{
			ListViewItem i = new ListViewItem("");
			filters.Items.Add(i);
			i.BeginEdit();
		}
		
		// Remove selected items
		private void removefilter_Click(object sender, EventArgs e)
		{
			int index = filters.SelectedIndices[0]; //mxd
			foreach(ListViewItem i in filters.SelectedItems) i.Remove();

			//mxd
			if(filters.Items.Count > 0)
			{
				if(index >= filters.Items.Count) index = filters.Items.Count - 1;
				filters.Items[index].Selected = true;
			}

			filters.Focus(); //mxd
			
			// Run the timer
			filterstimer.Start();
		}
		
		// Items selected/deselected
		private void filters_SelectedIndexChanged(object sender, EventArgs e)
		{
			removefilter.Enabled = (filters.SelectedItems.Count > 0);
		}
		
		// Double clicking an item
		private void filters_DoubleClick(object sender, EventArgs e)
		{
			// Edit item
			if(filters.SelectedItems.Count == 1)
				filters.SelectedItems[0].BeginEdit();
		}
		
		// This removes empty items and makes others uppercase
		private void filterstimer_Tick(object sender, EventArgs e)
		{
			// Stop timer
			filterstimer.Stop();
			
			// Update labels
			for(int i = filters.Items.Count - 1; i >= 0; i--)
			{
				// Empty label?
				if((filters.Items[i].Text == null) ||
				   (filters.Items[i].Text.Trim().Length == 0))
				{
					// Remove it
					filters.Items.RemoveAt(i);
				}
				else
				{
					// Make uppercase
					filters.Items[i].Text = filters.Items[i].Text.ToUpperInvariant();
				}
			}
			
			// Show example results if when we can
			if(General.Map != null)
			{
				Cursor.Current = Cursors.AppStarting;
				
				// Make a set for comparing
				List<string> filterslist = new List<string>(filters.Items.Count);
				foreach(ListViewItem i in filters.Items) filterslist.Add(i.Text);
				MatchingTextureSet set = new MatchingTextureSet(filterslist);
				
				// Determine tooltip text
				string tooltiptext = null;
				if(nomatchesbutton.Checked) tooltiptext = "Double-click to include this texture";
				
				// Start adding
				matcheslist.PreventSelection = matchesbutton.Checked;
				matcheslist.BeginAdding(true);
				
				// Go for all textures
				foreach(ImageData img in General.Map.Data.Textures)
				{
					bool ismatch = set.IsMatch(img);
					if((ismatch && matchesbutton.Checked) || (!ismatch && nomatchesbutton.Checked))
						matcheslist.AddItem(img, tooltiptext);
				}
				
				// If not already mixed, add flats as well
				if(!General.Map.Config.MixTexturesFlats)
				{
					// Go for all flats
					foreach(ImageData img in General.Map.Data.Flats)
					{
						bool ismatch = set.IsMatch(img);
						if((ismatch && matchesbutton.Checked) || (!ismatch && nomatchesbutton.Checked))
							matcheslist.AddItem(img, tooltiptext);
					}
				}
				
				// Done adding
				matcheslist.EndAdding();
				Cursor.Current = Cursors.Default;
			}
		}
		
		// Done editing a filter
		private void filters_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			// Run the timer
			filterstimer.Start();
		}
		
		// When first shown
		private void TextureSetForm_Shown(object sender, EventArgs e)
		{
			// Run the timer
			filterstimer.Start();
		}
		
		// Show (not) matches clicked
		private void matchesbutton_Click(object sender, EventArgs e)
		{
			// Run the timer
			filterstimer.Start();
		}
		
		// Texture doubleclicked
		private void matcheslist_SelectedItemDoubleClicked(ImageBrowserItem item)
		{
			// Add texture name to the list
			if(item != null) filters.Items.Add(item.TextureName);
			
			// Run the timer
			filterstimer.Start();
		}

		// Help
		private void TextureSetForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			General.ShowHelp("w_textureset.html");
			hlpevent.Handled = true;
		}
	}
}

