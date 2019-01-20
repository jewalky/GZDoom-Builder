using System;
using System.Windows.Forms;
using InterpolationMode = CodeImp.DoomBuilder.Geometry.InterpolationTools.Mode;
using GridLockMode = CodeImp.DoomBuilder.BuilderModes.DrawGridMode.GridLockMode;

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal partial class DrawGridOptionsPanel : UserControl
	{
		public event EventHandler OnValueChanged;
		public event EventHandler OnGridLockModeChanged;
		public event EventHandler OnContinuousDrawingChanged;
		public event EventHandler OnShowGuidelinesChanged;

		private bool blockevents;

		public bool Triangulate { get { return triangulate.Checked; } set { blockevents = true; triangulate.Checked = value; blockevents = false; } }
		public GridLockMode GridLockMode { get { return (GridLockMode)gridlockmode.SelectedIndex; } set { blockevents = true; gridlockmode.SelectedIndex = (int)value; blockevents = false; } }
		public int HorizontalSlices { get { return (int)slicesH.Value; } set { blockevents = true; slicesH.Value = value; blockevents = false; } }
		public int MaxHorizontalSlices { get { return (int)slicesH.Maximum; } set { slicesH.Maximum = value; } }
		public int VerticalSlices { get { return (int)slicesV.Value; } set { blockevents = true; slicesV.Value = value; blockevents = false; } }
		public int MaxVerticalSlices { get { return (int)slicesV.Maximum; } set { slicesV.Maximum = value; } }
		public bool ContinuousDrawing { get { return continuousdrawing.Checked; } set { continuousdrawing.Checked = value; } }
		public bool ShowGuidelines { get { return showguidelines.Checked; } set { showguidelines.Checked = value; } }

		public InterpolationMode HorizontalInterpolationMode 
		{
			get
			{
				GridLockMode mode = (GridLockMode)gridlockmode.SelectedIndex;
				return (mode == GridLockMode.BOTH || mode == GridLockMode.HORIZONTAL) 
					? InterpolationMode.LINEAR : (InterpolationMode)interphmode.SelectedIndex;
			}
			set { interphmode.SelectedIndex = (int)value; }
		}
		public InterpolationMode VerticalInterpolationMode 
		{
			get
			{
				GridLockMode mode = (GridLockMode)gridlockmode.SelectedIndex;
				return (mode == GridLockMode.BOTH || mode == GridLockMode.VERTICAL) 
					? InterpolationMode.LINEAR : (InterpolationMode)interpvmode.SelectedIndex;
			}
			set { interpvmode.SelectedIndex = (int)value; }
		}

		public DrawGridOptionsPanel() 
		{
			InitializeComponent();

			// Fill them menus
			interphmode.Items.AddRange(new object[] { MenusForm.GradientInterpolationModes.Linear, MenusForm.GradientInterpolationModes.EaseInOutSine, MenusForm.GradientInterpolationModes.EaseInSine, MenusForm.GradientInterpolationModes.EaseOutSine });
			interphmode.SelectedIndex = 0;
			interpvmode.Items.AddRange(new object[] { MenusForm.GradientInterpolationModes.Linear, MenusForm.GradientInterpolationModes.EaseInOutSine, MenusForm.GradientInterpolationModes.EaseInSine, MenusForm.GradientInterpolationModes.EaseOutSine });
			interpvmode.SelectedIndex = 0;
		}

		private void ValueChanged(object sender, EventArgs e) 
		{
			if(!blockevents && OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
		}

		private void gridlockmode_SelectedIndexChanged(object sender, EventArgs e)
		{
			GridLockMode mode = (GridLockMode)gridlockmode.SelectedIndex;
			slicesH.Enabled = (mode == GridLockMode.NONE || mode == GridLockMode.VERTICAL);
			slicesV.Enabled = (mode == GridLockMode.NONE || mode == GridLockMode.HORIZONTAL);
			interphmode.Enabled = slicesH.Enabled;
			interpvmode.Enabled = slicesV.Enabled;
			reset.Enabled = (mode != GridLockMode.BOTH);
			
			if(!blockevents && OnGridLockModeChanged != null) OnGridLockModeChanged(this, EventArgs.Empty);
		}

		private void interpmode_DropDownClosed(object sender, EventArgs e) 
		{
			General.Interface.FocusDisplay();
		}

		private void reset_Click(object sender, EventArgs e)
		{
			GridLockMode mode = (GridLockMode)gridlockmode.SelectedIndex;
			
			blockevents = true;
			if((mode == GridLockMode.NONE || mode == GridLockMode.VERTICAL)) slicesH.Value = 3;
			if(mode == GridLockMode.NONE || mode == GridLockMode.HORIZONTAL) slicesV.Value = 3;
			blockevents = false;

			if(OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
		}

		private void continuousdrawing_CheckedChanged(object sender, EventArgs e)
		{
			if(OnContinuousDrawingChanged != null) OnContinuousDrawingChanged(continuousdrawing.Checked, EventArgs.Empty);
		}

		private void showguidelines_CheckedChanged(object sender, EventArgs e)
		{
			if(OnShowGuidelinesChanged != null) OnShowGuidelinesChanged(showguidelines.Checked, EventArgs.Empty);
		}
	}
}
