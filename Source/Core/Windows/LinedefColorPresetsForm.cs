#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	public partial class LinedefColorPresetsForm : Form
	{
		#region ================== Structs

		private class PresetItem : IColoredListBoxItem
		{
			private Color color;
			
			public bool ShowWarning { get; set; }
			public Color Color { get { return color; } set { color = value; Preset.Color = PixelColor.FromColor(value); } }
			public readonly LinedefColorPreset Preset;
			public const string DEFAULT_NAME = "Unnamed preset";

			// Constructor for a new preset
			public PresetItem()
			{
				Preset = new LinedefColorPreset(DEFAULT_NAME, General.Colors.BrightColors[General.Random(0, General.Colors.BrightColors.Length - 1)]);
				color = Preset.Color.ToColor();
			}

			// Constructor for existing preset
			public PresetItem(LinedefColorPreset preset)
			{
				Preset = new LinedefColorPreset(preset);
				color = Preset.Color.ToColor();
			}

			public override string ToString()
			{
				return Preset.Name;
			}
		}

		#endregion

		#region ================== Variables

		private bool preventchanges;

		#endregion

		#region ================== Constructor / Setup

		public LinedefColorPresetsForm()
		{
			preventchanges = true;
			
			InitializeComponent();

			// Fill filters list
			foreach(LinedefColorPreset preset in General.Map.ConfigSettings.LinedefColorPresets)
			{
				PresetItem pi = new PresetItem(preset);
				ValidatePreset(pi);
				int index = colorpresets.Items.Add(pi);
				if(preset.Enabled) colorpresets.SetItemChecked(index, true);
			}

			// Setup preset controls
			// Add flags
			foreach(KeyValuePair<string, string> lf in General.Map.Config.LinedefFlags)
			{
				CheckBox box = flags.Add(lf.Value, lf.Key);
				box.ThreeState = true;
				box.CheckStateChanged += flags_OnCheckStateChanged;
			}

			// Fill actions list
			action.GeneralizedCategories = General.Map.Config.GenActionCategories;

			List<LinedefActionInfo> infos = new List<LinedefActionInfo> { new LinedefActionInfo(-1, "Any action", true, false) };
			infos.AddRange(General.Map.Config.SortedLinedefActions);

			action.AddInfo(infos.ToArray());
			action.Value = -1;

			//activations
			udmfactivates.Visible = General.Map.UDMF;
			activation.Visible = !General.Map.UDMF;

			//get activates
			List<LinedefActivateInfo> activations = new List<LinedefActivateInfo>(General.Map.Config.LinedefActivates);

			if(activations.Count > 0)
			{
				activations.Insert(0, new LinedefActivateInfo("-1", "Any activation", true));

				if(General.Map.UDMF)
				{
					foreach(LinedefActivateInfo ai in General.Map.Config.LinedefActivates)
					{
						CheckBox box = udmfactivates.Add(ai.Title, ai.Key);
						box.ThreeState = true;
						box.CheckStateChanged += flags_OnCheckStateChanged;
					}
				}
				else
				{
					activation.Items.AddRange(activations.ToArray());
				}
			}
			else
			{
				presetsettings.TabPages.Remove(tabActivation);
			}

			UpdatePresetsListControls();
			preventchanges = false;

			// Select first preset or disable controls
			if(colorpresets.Items.Count > 0)
				colorpresets.SelectedIndex = 0;
			else
				curpresetgroup.Enabled = false;
		}

		private void ValidatePreset(PresetItem item)
		{
			bool warningstate = item.ShowWarning;
			string warning = string.Empty;

			// Validate
			if(!item.Preset.IsValid())
			{
				item.ShowWarning = true;
				if(General.Map.UDMF)
					warning = "Invalid preset: no flags, action or activation type selected!";
				else
					warning = "Invalid preset: no flags or action selected!";
			}
			else
			{
				item.ShowWarning = false;
				
				// Check for duplicates
				foreach(var otheritem in colorpresets.Items)
				{
					PresetItem other = (PresetItem)otheritem;
					if(other == item) continue;
					if(other.Preset.Action != item.Preset.Action) continue;
					if(other.Preset.Activation != item.Preset.Activation) continue;
					if(other.Preset.Flags.Count != item.Preset.Flags.Count) continue;
					if(other.Preset.RestrictedFlags.Count != item.Preset.RestrictedFlags.Count) continue;

					bool gotmismatch = false;
					foreach(string flag in other.Preset.Flags) 
					{
						if(!item.Preset.Flags.Contains(flag)) 
						{
							gotmismatch = true;
							break;
						}
					}

					foreach(string flag in other.Preset.RestrictedFlags) 
					{
						if(!item.Preset.RestrictedFlags.Contains(flag)) 
						{
							gotmismatch = true;
							break;
						}
					}

					if(gotmismatch) continue;

					//we have a match
					warning = "Preset matches \"" + other.Preset.Name + "\"!";
					item.ShowWarning = true;
					break;
				}
			}

			if(!preventchanges)
			{
				// Redraw presets list?
				if(warningstate != item.ShowWarning) colorpresets.Invalidate();

				// Update error description
				errordescription.Visible = item.ShowWarning;
				erroricon.Visible = item.ShowWarning;
				errordescription.Text = warning;
			}
		}

		private void UpdatePresetsListControls()
		{
			int c = colorpresets.Items.Count;
			curpresetgroup.Enabled = (c > 0);

			if(c < 2)
			{
				movedown.Enabled = false;
				moveup.Enabled = false;
			}
			else
			{
				movedown.Enabled = (colorpresets.SelectedIndex < c - 1);
				moveup.Enabled = (colorpresets.SelectedIndex > 0);
			}
		}

		private static void ApplyFlagsToPreset(CheckboxArrayControl source, LinedefColorPreset preset)
		{
			if(source.Enabled)
			{
				foreach(CheckBox box in source.Checkboxes)
					ApplyFlagToPreset(box.Tag.ToString(), preset, box.CheckState);
			}
			else
			{
				foreach(CheckBox box in source.Checkboxes)
				{
					string flag = box.Tag.ToString();
					if(preset.Flags.Contains(flag)) preset.Flags.Remove(flag);
					if(preset.RestrictedFlags.Contains(flag)) preset.RestrictedFlags.Remove(flag);
				}
			}
		}

		private static void ApplyFlagToPreset(string flag, LinedefColorPreset preset, CheckState state)
		{
			switch(state)
			{
				case CheckState.Checked:
					if(!preset.Flags.Contains(flag)) preset.Flags.Add(flag);
					if(preset.RestrictedFlags.Contains(flag)) preset.RestrictedFlags.Remove(flag);
					break;

				case CheckState.Unchecked:
					if(preset.Flags.Contains(flag)) preset.Flags.Remove(flag);
					if(!preset.RestrictedFlags.Contains(flag)) preset.RestrictedFlags.Add(flag);
					break;

				default:
					if(preset.Flags.Contains(flag)) preset.Flags.Remove(flag);
					if(preset.RestrictedFlags.Contains(flag)) preset.RestrictedFlags.Remove(flag);
					break;
			}
		}

		private static void ApplyFlagsToControl(LinedefColorPreset preset, CheckboxArrayControl target, CheckBox useflags)
		{
			if(preset.Flags.Count == 0 && preset.RestrictedFlags.Count == 0)
			{
				useflags.Checked = false;
				target.Enabled = false;

				foreach(CheckBox box in target.Checkboxes)
					box.CheckState = CheckState.Indeterminate;
			}
			else
			{
				useflags.Checked = true;
				target.Enabled = true;

				foreach(CheckBox box in target.Checkboxes)
				{
					string flag = box.Tag.ToString();
					if(preset.Flags.Contains(flag))
						box.CheckState = CheckState.Checked;
					else if(preset.RestrictedFlags.Contains(flag))
						box.CheckState = CheckState.Unchecked;
					else
						box.CheckState = CheckState.Indeterminate;
				}
			}
		}

		#endregion

		#region ================== Apply / Cancel

		private void apply_Click(object sender, EventArgs e)
		{
			// Replace all presets
			LinedefColorPreset[] newpresets = new LinedefColorPreset[colorpresets.Items.Count];
			for(int i = 0; i < colorpresets.Items.Count; i++)
				newpresets[i] = ((PresetItem)colorpresets.Items[i]).Preset;
			General.Map.ConfigSettings.LinedefColorPresets = newpresets;
			
			// Update stuff
			General.Map.Map.UpdateCustomLinedefColors();
			General.MainWindow.UpdateLinedefColorPresets();
			General.Map.ConfigSettings.Changed = true;
			
			// Close
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			// Close
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void colorpresets_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if(preventchanges || colorpresets.SelectedItem == null) return;
			((PresetItem)colorpresets.SelectedItem).Preset.Enabled = (e.NewValue == CheckState.Checked);
		}

		#endregion

		#region ================== Presets list events

		private void colorpresets_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(preventchanges) return;
			
			// Disable controls
			if(colorpresets.SelectedIndex == -1)
			{
				curpresetgroup.Enabled = false;
				return;
			}

			preventchanges = true;
			curpresetgroup.Enabled = true;

			PresetItem item = (PresetItem) colorpresets.SelectedItem;

			// Update name
			presetname.Text = item.ToString();

			// Update color
			presetcolor.Color = item.Preset.Color;

			// Update flags
			ApplyFlagsToControl(item.Preset, flags, useflags);

			// Update action
			action.Value = item.Preset.Action;
			useaction.Checked = (item.Preset.Action != 0);
			action.Enabled = (item.Preset.Action != 0);

			// Update activation
			if(General.Map.UDMF)
			{
				ApplyFlagsToControl(item.Preset, udmfactivates, useactivation);
			} 
			else if(presetsettings.TabPages.Contains(tabActivation))
			{
				if(item.Preset.Activation == 0)
				{
					activation.SelectedIndex = 1;
					useactivation.Checked = false;
					activation.Enabled = false;
				}
				else
				{
					for(int i = 0; i < activation.Items.Count; i++)
					{
						if(((LinedefActivateInfo)activation.Items[i]).Index == item.Preset.Activation)
						{
							activation.SelectedIndex = i;
							useactivation.Checked = true;
							activation.Enabled = true;
							break;
						}
					}
				}
			}

			preventchanges = false;

			// Update warning description
			ValidatePreset(item);

			// Update controls
			UpdatePresetsListControls();
		}

		private void addpreset_Click(object sender, EventArgs e)
		{
			// Add new item
			PresetItem item = new PresetItem();
			int index = Math.Max(0, colorpresets.SelectedIndex);
			colorpresets.Items.Insert(index, item);
			colorpresets.SetItemChecked(index, true);
			
			// Select it
			colorpresets.SelectedItem = item;

			// Update controls
			UpdatePresetsListControls();
		}

		private void deletepreset_Click(object sender, EventArgs e)
		{
			if(colorpresets.SelectedItem == null) return; //sanity check

			// Remove item
			int index = colorpresets.SelectedIndex;
			colorpresets.Items.RemoveAt(index);
			
			// Select previous item, if possible
			if(colorpresets.Items.Count > 0)
				colorpresets.SelectedIndex = (index >= colorpresets.Items.Count ? colorpresets.Items.Count - 1 : index);

			// Update controls
			UpdatePresetsListControls();
		}

		private void moveup_Click(object sender, EventArgs e)
		{
			if(colorpresets.SelectedItem == null) return;

			PresetItem item = (PresetItem)colorpresets.SelectedItem;
			colorpresets.Items[colorpresets.SelectedIndex] = colorpresets.Items[colorpresets.SelectedIndex - 1];
			colorpresets.Items[colorpresets.SelectedIndex - 1] = item;

			colorpresets.SelectedIndex--;
		}

		private void movedown_Click(object sender, EventArgs e)
		{
			if(colorpresets.SelectedItem == null) return;

			PresetItem item = (PresetItem)colorpresets.SelectedItem;
			colorpresets.Items[colorpresets.SelectedIndex] = colorpresets.Items[colorpresets.SelectedIndex + 1];
			colorpresets.Items[colorpresets.SelectedIndex + 1] = item;

			colorpresets.SelectedIndex++;
		}

		#endregion

		#region ================== Current preset events

		private void presetname_Validating(object sender, CancelEventArgs e)
		{
			if(preventchanges || colorpresets.SelectedItem == null) return;
			if(string.IsNullOrEmpty(presetname.Text)) presetname.Text = PresetItem.DEFAULT_NAME;
			((PresetItem)colorpresets.SelectedItem).Preset.Name = presetname.Text;
			colorpresets.Invalidate();
		}

		private void presetname_Enter(object sender, EventArgs e)
		{
			if(presetname.Text == PresetItem.DEFAULT_NAME) presetname.Text = string.Empty;
		}

		private void presetcolor_ColorChanged(object sender, EventArgs e)
		{
			if(preventchanges || colorpresets.SelectedItem == null) return;
			((PresetItem)colorpresets.SelectedItem).Color = presetcolor.Color.ToColor();
			colorpresets.Invalidate();
		}

		private void flags_OnCheckStateChanged(object sender, EventArgs eventArgs)
		{
			if(preventchanges || colorpresets.SelectedItem == null) return;
			CheckBox cb = (CheckBox)sender;
			PresetItem item = (PresetItem)colorpresets.SelectedItem;
			ApplyFlagToPreset(cb.Tag.ToString(), item.Preset, cb.CheckState);
			ValidatePreset(item);
		}

		private void useflags_CheckedChanged(object sender, EventArgs e)
		{
			if(preventchanges || colorpresets.SelectedItem == null) return;
			flags.Enabled = useflags.Checked;
			PresetItem item = (PresetItem)colorpresets.SelectedItem;
			ApplyFlagsToPreset(flags, item.Preset);
			ValidatePreset(item);
		}

		private void useaction_CheckedChanged(object sender, EventArgs e)
		{
			if(preventchanges) return;
			action.Enabled = useaction.Checked;
			action_ValueChanges(action, EventArgs.Empty);
		}

		private void action_ValueChanges(object sender, EventArgs e)
		{
			if(preventchanges || colorpresets.SelectedItem == null) return;
			PresetItem item = (PresetItem)colorpresets.SelectedItem;
			item.Preset.Action = (action.Enabled ? action.Value : 0);
			ValidatePreset(item);
		}

		private void useactivation_CheckedChanged(object sender, EventArgs e)
		{
			if(preventchanges || colorpresets.SelectedItem == null) return;
			activation.Enabled = useactivation.Checked;
			udmfactivates.Enabled = useactivation.Checked;
			PresetItem item = (PresetItem)colorpresets.SelectedItem;

			if(General.Map.UDMF)
				ApplyFlagsToPreset(udmfactivates, item.Preset);
			else
				item.Preset.Activation = (activation.Enabled ? ((LinedefActivateInfo)activation.SelectedItem).Index : 0);

			ValidatePreset(item);
		}

		private void activation_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(preventchanges || colorpresets.SelectedItem == null || activation.SelectedItem == null) return;
			PresetItem item = (PresetItem)colorpresets.SelectedItem;
			item.Preset.Activation = ((LinedefActivateInfo)activation.SelectedItem).Index;
			ValidatePreset(item);
		}

		#endregion

	}
}