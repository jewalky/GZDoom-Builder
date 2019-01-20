using System;
using System.Windows.Forms;
using CodeImp.DoomBuilder.BuilderModes.ClassicModes;
using CodeImp.DoomBuilder.Windows;

namespace CodeImp.DoomBuilder.BuilderModes.Interface 
{
	internal struct BridgeInterpolationMode 
	{
		public const string BRIGHTNESS_HIGHEST = "Use highest";
		public const string BRIGHTNESS_LOWEST = "Use lowest";

		public const string HIGHEST = "Highest ceiling";
		public const string LOWEST = "Lowest floor";
		public const string LINEAR = "Linear interpolation";
		public const string IN_SINE = "EaseInSine interpolation";
		public const string OUT_SINE = "EaseOutSine interpolation";
		public const string IN_OUT_SINE = "EaseInOutSine interpolation";

		public static readonly string[] CEILING_INTERPOLATION_MODES = { LINEAR, HIGHEST, IN_SINE, OUT_SINE, IN_OUT_SINE/*, IN_OUT_CUBIC, OUT_IN_CUBIC*/ };
		public static readonly string[] FLOOR_INTERPOLATION_MODES = { LINEAR, LOWEST, IN_SINE, OUT_SINE, IN_OUT_SINE/*, IN_OUT_CUBIC, OUT_IN_CUBIC*/ };
		public static readonly string[] BRIGHTNESS_INTERPOLATION_MODES = { LINEAR, BRIGHTNESS_HIGHEST, BRIGHTNESS_LOWEST };
	}
	
	public partial class BridgeModeForm : DelayedForm 
	{
		internal int Subdivisions { get { return (int)nudSubdivisions.Value; } set { nudSubdivisions.Value = value; } }
		internal string FloorAlignMode { get { return (string)cbFloorAlign.SelectedItem; } }
		internal string CeilingAlignMode { get { return (string)cbCeilingAlign.SelectedItem; } }
		internal string BrightnessMode { get { return (string)cbBrightness.SelectedItem; } }

		internal bool MirrorMode { get { return cbMirror.Checked; } }
		internal bool CopyMode { get { return cbCopy.Checked; } }

		internal event EventHandler OnSubdivisionChanged;
		internal event EventHandler OnOkClick;
		internal event EventHandler OnCancelClick;
		internal event EventHandler OnFlipClick;
		
		public BridgeModeForm() 
		{
			InitializeComponent();

			cbBrightness.Items.AddRange(BridgeInterpolationMode.BRIGHTNESS_INTERPOLATION_MODES);
			cbCeilingAlign.Items.AddRange(BridgeInterpolationMode.CEILING_INTERPOLATION_MODES);
			cbFloorAlign.Items.AddRange(BridgeInterpolationMode.FLOOR_INTERPOLATION_MODES);

			cbBrightness.SelectedIndex = 0;
			cbCeilingAlign.SelectedIndex = 0;
			cbFloorAlign.SelectedIndex = 0;

			nudSubdivisions.Minimum = BridgeMode.MIN_SUBDIVISIONS;
			nudSubdivisions.Maximum = BridgeMode.MAX_SUBDIVISIONS;
		}

//events
		private void BezierPathForm_FormClosed(object sender, FormClosedEventArgs e) 
		{
			if(OnCancelClick != null) OnCancelClick(this, EventArgs.Empty);
		}

		private void buttonCancel_Click(object sender, EventArgs e) 
		{
			if(OnCancelClick != null) OnCancelClick(this, EventArgs.Empty);
		}

		private void buttonOK_Click(object sender, EventArgs e) 
		{
			if(OnOkClick != null) OnOkClick(this, EventArgs.Empty);
		}

		private void nudSubdivisions_ValueChanged(object sender, EventArgs e) 
		{
			if(OnSubdivisionChanged != null) OnSubdivisionChanged(this, EventArgs.Empty);
		}

		private void BezierPathForm_MouseLeave(object sender, EventArgs e) 
		{
			General.Interface.FocusDisplay();
		}

		private void buttonFlip_Click(object sender, EventArgs e) 
		{
			if(OnFlipClick != null) OnFlipClick(this, EventArgs.Empty);
		}

		private void cbCopy_CheckedChanged(object sender, EventArgs e) 
		{
			if(cbMirror.Checked && cbCopy.Checked) cbMirror.Checked = false;
		}

		private void cbMirror_CheckStateChanged(object sender, EventArgs e) 
		{
			if(cbMirror.Checked && cbCopy.Checked) cbCopy.Checked = false;
		}
	}
}
