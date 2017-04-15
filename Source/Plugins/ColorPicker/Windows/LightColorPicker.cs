using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.GZBuilder;
using CodeImp.DoomBuilder.VisualModes;
using CodeImp.DoomBuilder.ColorPicker.Controls;

namespace CodeImp.DoomBuilder.ColorPicker.Windows 
{
	public partial class LightColorPicker : DelayedForm, IColorPicker 
	{
		public ColorPickerType Type { get { return ColorPickerType.CP_LIGHT; } }

		private static bool RELATIVE_MODE;
		
		private static readonly int[] LIGHT_USES_ANGLE_VALUE = { 9801, 9802, 9804, 9811, 9812, 9814, 9821, 9822, 9824, 9831, 9832, 9834 };
		
		private List<Thing> selection;
		private List<VisualThing> visualSelection;
		private List<LightProps> fixedValues; //this gets filled with radius / intencity values before RELATIVE_MODE is engaged

		private string editingModeName;
		private bool showAllControls;

		private LightProps lightProps;
 
		//maximum and minimum values for sliders for relative and absolute modes
		//numericUpDown
		private const int NUD_ABS_MIN = 0;
		private const int NUD_ABS_MAX = 16384;

		private const int NUD_REL_MIN = -16384;
		private const int NUD_REL_MAX = 16384;

		//trackBar
		private const int TB_ABS_MIN = 0;
		private const int TB_ABS_MAX = 512;

		private const int TB_REL_MIN = -256;
		private const int TB_REL_MAX = 256;

		//trackBar controlling thing angle
		private const int TB_ANGLE_ABS_MIN = 0;
		private const int TB_ANGLE_ABS_MAX = 359;

		private const int TB_ANGLE_REL_MIN = -180;
		private const int TB_ANGLE_REL_MAX = 180;

		//initialise pannel
		public bool Setup(string editingModeName) 
		{
			this.editingModeName = editingModeName;
			SetupSelection();

			int selCount = selection.Count;
			if(selCount == 0) 
			{
				General.Interface.DisplayStatus(StatusType.Warning, "No lights found in selection!");
				return false;
			}

			lightProps = new LightProps();

			//initialise
			InitializeComponent();

			SetupSliders(selection[0]);
			UpdateLightPropsFromThing(selection[0]);
			SetControlsMode();

			colorPickerControl1.Initialize(GetThingColor(selection[0]));
			colorPickerControl1.OnColorChanged += OnColorPickerControl1OnColorChanged;
			colorPickerControl1.OnOkPressed += colorPickerControl1_OnOkPressed;
			colorPickerControl1.OnCancelPressed += colorPickerControl1_OnCancelPressed;

			cbRelativeMode.Checked = RELATIVE_MODE;
			cbRelativeMode.CheckStateChanged += cbRelativeMode_CheckStateChanged;

			this.AcceptButton = colorPickerControl1.OkButton;
			this.CancelButton = colorPickerControl1.CancelButton;
			this.Text = "Editing " + selCount + " light" + (selCount > 1 ? "s" : "");
			
			//undo
			MakeUndo(this.Text);
			return true;
		}

		//create selection of lights. This is called only once
		private void SetupSelection() 
		{
			selection = new List<Thing>();

			//check things
			if(editingModeName == "BaseVisualMode") 
			{
				visualSelection = new List<VisualThing>();
				List<VisualThing> selectedVisualThings = ((VisualMode)General.Editing.Mode).GetSelectedVisualThings(false);

				foreach(VisualThing t in selectedVisualThings) 
				{
					if(GZGeneral.GetGZLightTypeByThing(t.Thing) != -1) 
					{
						selection.Add(t.Thing);
						visualSelection.Add(t);
					}
				}
			} 
			else 
			{
				ICollection<Thing> list = General.Map.Map.GetSelectedThings(true);
				foreach(Thing t in list) 
				{
					if(GZGeneral.GetGZLightTypeByThing(t) != -1)
						selection.Add(t);
				}
			}
		}

		//set sliders count and labels based on given thing, set lightProps start values
		//this is called only once
		private void SetupSliders(Thing referenceThing) 
		{
			ThingTypeInfo typeInfo = General.Map.Data.GetThingInfoEx(referenceThing.DynamicLightType);
			int firstArg = 3;
			if(referenceThing.DynamicLightType == 1502 || referenceThing.DynamicLightType == 1503)
				firstArg = 0;

			//first slider is always used
			colorPickerSlider1.Label = typeInfo.Args[firstArg].Title + ":";
			colorPickerSlider1.OnValueChanged += OnSliderValueChanged;

			//either both of them or none are used
			if(Array.IndexOf(LIGHT_USES_ANGLE_VALUE, referenceThing.DynamicLightType) != -1) 
			{
				showAllControls = true;
				colorPickerSlider2.Label = typeInfo.Args[4].Title + ":";
				colorPickerSlider2.OnValueChanged += OnSliderValueChanged;
				
				colorPickerSlider3.Label = "Interval:";
				colorPickerSlider3.OnValueChanged += OnSliderValueChanged;
			} 
			else 
			{
				colorPickerSlider2.Visible = false;
				colorPickerSlider3.Visible = false;
			}

			//set window height
			int newHeight;
			if(showAllControls) 
				newHeight = colorPickerSlider3.Location.Y + colorPickerSlider3.Height + 8;
			else
				newHeight = colorPickerSlider1.Location.Y + colorPickerSlider1.Height + 8;

			this.ClientSize = new Size(this.ClientSize.Width, newHeight);
		}

		//this sets lightProps values from given thing
		private void UpdateLightPropsFromThing(Thing referenceThing) 
		{
			//color
			Color c = GetThingColor(referenceThing);
			lightProps.Red = c.R;
			lightProps.Green = c.G;
			lightProps.Blue = c.B;
			
			//size
			int firstArg = 3;
			if(referenceThing.DynamicLightType == 1502 || referenceThing.DynamicLightType == 1503)
				firstArg = 0;

			lightProps.PrimaryRadius = referenceThing.Args[firstArg];

			//either both of them or none are used
			if(showAllControls && Array.IndexOf(LIGHT_USES_ANGLE_VALUE, referenceThing.DynamicLightType) != -1) 
			{
				lightProps.SecondaryRadius = referenceThing.Args[4];
				lightProps.Interval = referenceThing.AngleDoom;
			}
		}

		//this sets lightProps values from sliders
		private void UpdateLightPropsFromSliders() 
		{
			ColorHandler.RGB curColor = colorPickerControl1.CurrentColor;
			bool colorChanged = false; //need this check to allow relative mode to work properly

			if((byte)curColor.Red != lightProps.Red || (byte)curColor.Green != lightProps.Green || (byte)curColor.Blue != lightProps.Blue) 
			{
				lightProps.Red = (byte)curColor.Red;
				lightProps.Green = (byte)curColor.Green;
				lightProps.Blue = (byte)curColor.Blue;
				colorChanged = true;
			}

			lightProps.PrimaryRadius = colorPickerSlider1.Value;
			if(showAllControls) 
			{
				lightProps.SecondaryRadius = colorPickerSlider2.Value;
				lightProps.Interval = colorPickerSlider3.Value;
			}
			UpdateSelection(colorChanged);
		}

		//this sets values from lightProps to things in selection
		private void UpdateSelection(bool colorChanged) 
		{
			for(int i = 0; i < selection.Count; i++) 
			{
				Thing t = selection[i];

				//update color 
				if(colorChanged) //need this check to allow relative mode to work properly
				{ 
					if(t.DynamicLightType == 1503) //Vavoom Light Color
					{ 
						t.Args[1] = lightProps.Red;
						t.Args[2] = lightProps.Green;
						t.Args[3] = lightProps.Blue;
					} 
					else if(t.DynamicLightType != 1502) //vavoom light has no color settings
					{ 
						t.Args[0] = lightProps.Red;
						t.Args[1] = lightProps.Green;
						t.Args[2] = lightProps.Blue;
					}
				}

				int firstArg = 3;
				if(t.DynamicLightType == 1502 || t.DynamicLightType == 1503) firstArg = 0;

				//update radius and intensity
				if(RELATIVE_MODE) 
				{
					LightProps fixedVal = fixedValues[i];

					t.Args[firstArg] = fixedVal.PrimaryRadius + lightProps.PrimaryRadius;
					if(t.Args[firstArg] < 0) t.Args[firstArg] = 0;

					if(showAllControls && Array.IndexOf(LIGHT_USES_ANGLE_VALUE, t.DynamicLightType) != -1) 
					{
						t.Args[4] = fixedVal.SecondaryRadius + lightProps.SecondaryRadius;
						if(t.Args[4] < 0) t.Args[4] = 0;

						t.Rotate(General.ClampAngle(fixedVal.Interval + lightProps.Interval));
					}
				} 
				else 
				{
					if(lightProps.PrimaryRadius != -1)
						t.Args[firstArg] = lightProps.PrimaryRadius;

					if(showAllControls && Array.IndexOf(LIGHT_USES_ANGLE_VALUE, t.DynamicLightType) != -1) 
					{
						t.Args[4] = lightProps.SecondaryRadius;
						t.Rotate(General.ClampAngle(lightProps.Interval));
					}
				}
			}

			//update VisualThings
			if(editingModeName == "BaseVisualMode") 
			{
				foreach(VisualThing t in visualSelection)
					t.UpdateLight();
			}
			else if(editingModeName == "ThingsMode")
			{
				// Hacky way to call ThingsMode.UpdateHelperObjects() without referenceing BuilderModes.dll
				General.Editing.Mode.OnRedoEnd();
				General.Interface.RedrawDisplay();
			}
		}

		//switch between absolute and relative mode
		private void SetControlsMode() 
		{
			if(RELATIVE_MODE) 
			{
				SetFixedValues();
				colorPickerSlider1.SetLimits(TB_REL_MIN, TB_REL_MAX, NUD_REL_MIN, NUD_REL_MAX);
				colorPickerSlider1.Value = 0;

				if(showAllControls) 
				{
					colorPickerSlider2.SetLimits(TB_REL_MIN, TB_REL_MAX, NUD_REL_MIN, NUD_REL_MAX);
					colorPickerSlider2.Value = 0;

					colorPickerSlider3.SetLimits(TB_ANGLE_REL_MIN, TB_ANGLE_REL_MAX, NUD_REL_MIN, NUD_REL_MAX);
					colorPickerSlider3.Value = 0;
				}    
			} 
			else 
			{
				UpdateLightPropsFromThing(selection[0]);
				colorPickerSlider1.SetLimits(TB_ABS_MIN, TB_ABS_MAX, NUD_ABS_MIN, NUD_ABS_MAX);
				colorPickerSlider1.Value = lightProps.PrimaryRadius;

				if(showAllControls) 
				{
					colorPickerSlider2.SetLimits(TB_ABS_MIN, TB_ABS_MAX, NUD_ABS_MIN, NUD_ABS_MAX);
					colorPickerSlider2.Value = lightProps.SecondaryRadius;

					colorPickerSlider3.SetLimits(TB_ANGLE_ABS_MIN, TB_ANGLE_ABS_MAX, NUD_ABS_MIN, NUD_ABS_MAX);
					colorPickerSlider3.Value = lightProps.Interval;
				}
			}
		}

		private void MakeUndo(string description) 
		{
			General.Map.UndoRedo.CreateUndo(description);

			//tricky way to actually store undo information...
			foreach(Thing t in selection) t.Move(t.Position);
		}

		//this is called only once
		private static Color GetThingColor(Thing thing) 
		{
			if(thing.DynamicLightType == 1502) return Color.White; //vavoom light
			if(thing.DynamicLightType == 1503) return Color.FromArgb((byte)thing.Args[1], (byte)thing.Args[2], (byte)thing.Args[3]); //vavoom colored light
			return Color.FromArgb((byte)thing.Args[0], (byte)thing.Args[1], (byte)thing.Args[2]);
		}

		//this sets data to use as a reference for relative mode
		private void SetFixedValues() 
		{
			fixedValues = new List<LightProps>();

			for(int i = 0; i < selection.Count; i++) 
			{
				Thing t = selection[i];
				LightProps lp = new LightProps();
				int firstArg = 3;
				if(t.DynamicLightType == 1502 || t.DynamicLightType == 1503) firstArg = 0;
				lp.PrimaryRadius = t.Args[firstArg];

				//either both of them or none are used
				if(showAllControls &&  Array.IndexOf(LIGHT_USES_ANGLE_VALUE, t.DynamicLightType) != -1) 
				{
					lp.SecondaryRadius = t.Args[4];
					lp.Interval = t.AngleDoom;
				}

				fixedValues.Add(lp);
			}
		}

//events
		private void OnColorPickerControl1OnColorChanged(object sender, ColorChangedEventArgs e) 
		{
			//need this check to allow relative mode to work properly
			if((byte)e.RGB.Red != lightProps.Red || (byte)e.RGB.Green != lightProps.Green || (byte)e.RGB.Blue != lightProps.Blue)
				UpdateLightPropsFromSliders();
		}

		private void colorPickerControl1_OnCancelPressed(object sender, EventArgs e) 
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void colorPickerControl1_OnOkPressed(object sender, EventArgs e) 
		{
			this.DialogResult = DialogResult.OK;
			General.Interface.RefreshInfo();
			Close();
		}

		private void LightColorPicker_FormClosing(object sender, FormClosingEventArgs e) 
		{
			if(this.DialogResult == DialogResult.Cancel) General.Map.UndoRedo.WithdrawUndo();
		}

		private void cbRelativeMode_CheckStateChanged(object sender, EventArgs e) 
		{
			RELATIVE_MODE = ((CheckBox)sender).Checked;
			SetControlsMode();
		}

		private void OnSliderValueChanged(object sender, ColorPickerSliderEventArgs e) 
		{
			UpdateLightPropsFromSliders();
		}

		private void LightColorPicker_HelpRequested(object sender, HelpEventArgs hlpevent) 
		{
			General.ShowHelp("gzdb/features/all_modes/colorpicker.html");
			hlpevent.Handled = true;
		}
	}

	struct LightProps 
	{
		public byte Red;
		public byte Green;
		public byte Blue;

		public int PrimaryRadius;
		public int SecondaryRadius;
		public int Interval;
	}
}