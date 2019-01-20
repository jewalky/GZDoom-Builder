
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
using System.Globalization;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class EffectBrowserForm : DelayedForm
	{
		// Constants
		private const int MAX_OPTIONS = 8;
		
		// Variables
		private int selectedeffect;
		private readonly ComboBox[] options;
		private readonly ListViewItem[] allitems; //mxd
		private readonly bool addanyeffect;
		
		// Properties
		public int SelectedEffect { get { return selectedeffect; } }
		
		// Constructor
		public EffectBrowserForm(int effect, bool addanyeffect)
		{
			// Initialize
			InitializeComponent();

			//mxd. Show "Any action" item?
			this.addanyeffect = addanyeffect;

			// Make array references for controls
			options = new[] { option0, option1, option2, option3, option4, option5, option6, option7 };
			Label[] optionlbls = { option0label, option1label, option2label, option3label, 
								   option4label, option5label, option6label, option7label };
			
			// Using generalized effects?
			int nongeneralizedeffect = effect; //mxd
			if(General.Map.Config.GeneralizedEffects)
			{
				// Go for all options, bigger steps first (mxd)
				for(int i = MAX_OPTIONS - 1; i > -1; i--)
				{
					// Option used in selected category?
					if(i < General.Map.Config.GenEffectOptions.Count)
					{
						GeneralizedOption o = General.Map.Config.GenEffectOptions[i];
						
						// Setup controls
						optionlbls[i].Text = o.Name + ":";
						options[i].Items.Clear();
						options[i].Items.AddRange(o.Bits.ToArray());
						
						// Show option
						options[i].Visible = true;
						optionlbls[i].Visible = true;

						if(effects.SelectedItems.Count == 0)
						{
							// Go for all bits
							foreach(GeneralizedBit ab in o.Bits)
							{
								// Select this setting if matches
								if((effect & ab.Index) == ab.Index)
								{
									options[i].SelectedItem = ab;
									nongeneralizedeffect -= ab.Index; //mxd
									if(ab.Index > 0) break; //mxd
								}
							}
						}
					}
					else
					{
						// Hide option
						options[i].Visible = false;
						optionlbls[i].Visible = false;
					}
				}
			}
			else
			{
				// Remove generalized tab
				tabs.TabPages.Remove(tabgeneralized);
			}

			//mxd. Go for all predefined effects
			bool selected = CreateEffects(nongeneralizedeffect) && General.Map.Config.GeneralizedEffects; //mxd
			allitems = new ListViewItem[effects.Items.Count]; //mxd
			effects.Items.CopyTo(allitems, 0); //mxd

			// Open the generalized tab when given effect is generalized
			if(!selected && General.Map.Config.GeneralizedEffects) tabs.SelectedTab = tabgeneralized;
		}
		
		// This browses for an effect
		// Returns the new effect or the same effect when cancelled
		public static int BrowseEffect(IWin32Window owner, int effect) { return BrowseEffect(owner, effect, false); } //mxd
		public static int BrowseEffect(IWin32Window owner, int effect, bool addanyeffect)
		{
			EffectBrowserForm f = new EffectBrowserForm(effect, addanyeffect);
			if(f.ShowDialog(owner) == DialogResult.OK) effect = f.SelectedEffect;
			f.Dispose();
			return effect;
		}

		//mxd
		private bool CreateEffects(int effect) 
		{
			bool selected = false;

			if(addanyeffect)
			{
				ListViewItem n = effects.Items.Add("-1");
				n.SubItems.Add("Any effect");
				n.Tag = new SectorEffectInfo(-1, "Any effect", false, false);
				if(effect == -1)
				{
					selected = true;
					n.Selected = true;
				}
			}

			foreach(SectorEffectInfo si in General.Map.Config.SortedSectorEffects) 
			{
				// Create effect
				ListViewItem n = effects.Items.Add(si.Index.ToString());
				n.SubItems.Add(si.Title);
				n.Tag = si;
				if(si.Index == effect) 
				{
					selected = true;
					n.Selected = true;
				}
			}
			return selected;
		}

		//mxd
		private void FilterEffects(string text) 
		{
			List<ListViewItem> filtereditems = new List<ListViewItem>();
			HashSet<string> added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			// First add nodes, which titles start with given text
			foreach(ListViewItem i in allitems) 
			{
				SectorEffectInfo si = i.Tag as SectorEffectInfo;
				if(si != null && si.Title.ToUpperInvariant().StartsWith(text))
				{
					filtereditems.Add(i);
					added.Add(si.Title);
				}
			}

			// Then add nodes, which titles contain given text
			foreach(ListViewItem i in allitems)
			{
				SectorEffectInfo si = i.Tag as SectorEffectInfo;
				if(si != null && !added.Contains(si.Title) && si.Title.ToUpperInvariant().Contains(text))
				{
					filtereditems.Add(i);
					added.Add(si.Title);
				}
			}

			effects.BeginUpdate();
			effects.Items.Clear();
			effects.Items.AddRange(filtereditems.ToArray());
			effects.EndUpdate();
		}
		
		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			// Presume no result
			selectedeffect = 0;

			//mxd. Add predefined effect?
			if((effects.SelectedItems.Count > 0) && (effects.SelectedItems[0].Tag is SectorEffectInfo))
			{
				// Our result
				selectedeffect = ((SectorEffectInfo)effects.SelectedItems[0].Tag).Index;
			}

			//mxd. Add generalized effects? (Don't add when "Any effect" is selected)
			if(selectedeffect != -1 && General.Map.Config.GeneralizedEffects)
			{
				// Go for all options
				for(int i = 0; i < MAX_OPTIONS; i++)
				{
					// Option used?
					if(i < General.Map.Config.GenEffectOptions.Count)
					{
						// Add selected bits
						if(options[i].SelectedIndex > -1)
							selectedeffect += (options[i].SelectedItem as GeneralizedBit).Index;
					}
					else
					{
						break; //mxd
					}
				}
			}
			
			// Done
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// Leave
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
		
		// Double-clicked on item
		private void effects_DoubleClick(object sender, EventArgs e)
		{
			// Effect selected?
			if((effects.SelectedItems.Count > 0) && (effects.SelectedItems[0].Tag is SectorEffectInfo))
			{
				if(apply.Enabled) apply_Click(this, EventArgs.Empty);
			}
		}

		//mxd
		private void tbFilter_TextChanged(object sender, EventArgs e) 
		{
			if(!string.IsNullOrEmpty(tbFilter.Text.Trim()))
			{
				FilterEffects(tbFilter.Text.ToUpperInvariant());
			} 
			else
			{
				effects.Items.Clear();
				CreateEffects(effects.SelectedItems.Count > 0 ? ((SectorEffectInfo)effects.SelectedItems[0].Tag).Index : 0);
			}
		}

		//mxd. Switch focus to effects list?
		private void tbFilter_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Down && effects.Items.Count > 0)
			{
				effects.Items[0].Selected = true;
				effects.Focus();
			}
		}

		//mxd
		private void btnClearFilter_Click(object sender, EventArgs e) 
		{
			tbFilter.Clear();
		}

		//mxd
		private void EffectBrowserForm_Shown(object sender, EventArgs e)
		{
			if(tabs.SelectedTab == tabeffects) tbFilter.Focus();
		}

		//mxd
		private void effects_MouseEnter(object sender, EventArgs e)
		{
			effects.Focus();
		}

		//mxd. Transfer focus to Filter textbox
		private void effects_KeyPress(object sender, KeyPressEventArgs e)
		{
			tbFilter.Focus();
			if(e.KeyChar == '\b') // Any better way to check for Backspace?..
			{
				if(!string.IsNullOrEmpty(tbFilter.Text) && tbFilter.SelectionStart > 0 && tbFilter.SelectionLength == 0)
				{
					int s = tbFilter.SelectionStart - 1;
					tbFilter.Text = tbFilter.Text.Remove(s, 1);
					tbFilter.SelectionStart = s;
				}
			}
			else
			{
				tbFilter.AppendText(e.KeyChar.ToString(CultureInfo.InvariantCulture));
			}
		}

	}
}