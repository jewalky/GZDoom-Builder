using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.VisualModes;

namespace CodeImp.DoomBuilder.ColorPicker.Windows
{
	public partial class SectorColorPicker : DelayedForm, IColorPicker
	{
		public ColorPickerType Type { get { return ColorPickerType.CP_SECTOR; } }

		private const int DEFAULT_LIGHT_COLOR = 0xFFFFFF; //ffffff
		private const int DEFAULT_FADE_COLOR = 0;
		
		private List<Sector> selection;
		private List<VisualSector> visualSelection;
		
		private int curSectorColor;
		private int curFadeColor;
		private int initialSectorColor;
		private int initialFadeColor;

		private static string currentColorTag = "lightcolor"; //lightcolor or fadecolor
		private string mode;

		public bool Setup(string editingModeName) 
		{
			mode = editingModeName;

			if(mode == "SectorsMode") 
			{
				selection = (List<Sector>)(General.Map.Map.GetSelectedSectors(true));
			} 
			else //should be Visual mode
			{ 
				selection = new List<Sector>();
				VisualMode vm = (VisualMode)General.Editing.Mode;
				visualSelection = vm.GetSelectedVisualSectors(false);
				
				if(visualSelection.Count > 0) 
				{
					foreach(VisualSector vs in visualSelection)
						selection.Add(vs.Sector);
				} 
				else //should be some sectors selected in 2d-mode...
				{ 
					visualSelection = new List<VisualSector>();
					selection = (List<Sector>)(General.Map.Map.GetSelectedSectors(true));

					foreach(Sector s in selection) 
					{
						if(vm.VisualSectorExists(s))
							visualSelection.Add(vm.GetVisualSector(s));
					}
				}
			}

			//create undo
			string rest = selection.Count + " sector" + (selection.Count > 1 ? "s" : "");
			General.Map.UndoRedo.CreateUndo("Edit color of " + rest);
			
			foreach(Sector s in selection) s.Fields.BeforeFieldsChange();

			//set colors
			curSectorColor = selection[0].Fields.GetValue("lightcolor", DEFAULT_LIGHT_COLOR);
			curFadeColor = selection[0].Fields.GetValue("fadecolor", DEFAULT_FADE_COLOR);

			//check that all sectors in selection have "lightcolor" and "fadecolor" fields
			for(int i = 0; i < selection.Count; i++) 
			{
				if(!selection[i].Fields.ContainsKey("lightcolor"))
					selection[i].Fields.Add("lightcolor", new UniValue(UniversalType.Color, curSectorColor));
				
				if(!selection[i].Fields.ContainsKey("fadecolor"))
					selection[i].Fields.Add("fadecolor", new UniValue(UniversalType.Color, curFadeColor));
			}

			initialSectorColor = curSectorColor;
			initialFadeColor = curFadeColor;

			InitializeComponent();

			colorPickerControl1.Initialize(Color.FromArgb(currentColorTag == "lightcolor" ? curSectorColor : curFadeColor));
			colorPickerControl1.OnColorChanged += OnColorPickerControl1OnColorChanged;
			colorPickerControl1.OnOkPressed += colorPickerControl1_OnOkPressed;
			colorPickerControl1.OnCancelPressed += colorPickerControl1_OnCancelPressed;

			if(currentColorTag == "lightcolor")
				rbSectorColor.Checked = true;
			else
				rbFadeColor.Checked = true;

			rbSectorColor.CheckedChanged += rbColor_CheckedChanged;
			rbFadeColor.CheckedChanged += rbColor_CheckedChanged;

			Text = "Editing " + rest;

			//cannot fail here :)
			return true;
		}

		private void colorPickerControl1_OnCancelPressed(object sender, EventArgs e) 
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void colorPickerControl1_OnOkPressed(object sender, EventArgs e) 
		{
			//check if values are default
			foreach(Sector s in selection) 
			{
				if((int)s.Fields["lightcolor"].Value == DEFAULT_LIGHT_COLOR)
					s.Fields.Remove("lightcolor");

				if((int)s.Fields["fadecolor"].Value == DEFAULT_FADE_COLOR)
					s.Fields.Remove("fadecolor");
			}

			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void SectorColorPicker_FormClosing(object sender, FormClosingEventArgs e) 
		{
			if(this.DialogResult == DialogResult.Cancel) General.Map.UndoRedo.WithdrawUndo();
		}

		private void OnColorPickerControl1OnColorChanged(object sender, ColorChangedEventArgs e) 
		{
			foreach(Sector s in selection) 
			{
				s.Fields[currentColorTag].Value = e.RGB.Red << 16 | e.RGB.Green << 8 | e.RGB.Blue;
				s.UpdateNeeded = true;
				s.UpdateCache();
			}

			//update display
			if(mode == "SectorsMode") 
			{
				General.Interface.RedrawDisplay();
			} 
			else //should be visual mode
			{ 
				foreach(VisualSector vs in visualSelection) vs.UpdateSectorData();
			}
		}

		private void rbColor_CheckedChanged(object sender, EventArgs e) 
		{
			RadioButton b = (RadioButton)sender;
			if(b.Checked) 
			{
				currentColorTag = (string)b.Tag;

				//update color picker
				if(currentColorTag == "lightcolor")
					colorPickerControl1.SetInitialColor(Color.FromArgb(initialSectorColor));
				else
					colorPickerControl1.SetInitialColor(Color.FromArgb(initialFadeColor));
				colorPickerControl1.SetCurrentColor(Color.FromArgb((int)selection[0].Fields[currentColorTag].Value));
			}
		}

		private void SectorColorPicker_HelpRequested(object sender, HelpEventArgs hlpevent) 
		{
			General.ShowHelp("gzdb/features/all_modes/colorpicker.html");
			hlpevent.Handled = true;
		}
	}
}
