#region ================== Namespaces

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public partial class ColorFieldsControl : UserControl
	{
		#region ================== Events

		public event EventHandler OnValueChanged;

		#endregion

		#region ================== Variables

		private int defaultvalue;
		private string field;
		private bool blockupdate;
		private bool blockevents;

		#endregion

		#region ================== Properties

		public int DefaultValue { get { return defaultvalue; } set { defaultvalue = value; } }
		public string Label { get { return cpColor.Label; } set { cpColor.Label = value; } }
		public string Field { get { return field; } set { field = value; } }
		public PixelColor Color
		{
			get { return cpColor.Color; }
			set
			{
				blockevents = true;
				tbColor.Text = String.Format("{0:X6}", value.ToInt() & 0x00FFFFFF);
				blockevents = false;
			}
		}

		#endregion

		#region ================== Constructor

		public ColorFieldsControl() 
		{
			InitializeComponent();
		}

		#endregion

		#region ================== Methods

		public void SetValueFrom(UniFields fields, bool first)
		{
			blockevents = true;
			
			string colorval = String.Format("{0:X6}", UniFields.GetInteger(fields, field, defaultvalue));
			if(first)
			{
				tbColor.Text = colorval;
			}
			else if(!string.IsNullOrEmpty(tbColor.Text) && colorval != tbColor.Text)
			{
				blockupdate = true;
				tbColor.Text = string.Empty;
				cpColor.Color = PixelColor.FromInt(defaultvalue).WithAlpha(255);
				blockupdate = false;

				CheckColor();
			}

			blockevents = false;
		}

		public void ApplyTo(UniFields fields, int oldvalue)
		{
			int colorval = !string.IsNullOrEmpty(tbColor.Text) ? (cpColor.Color.ToInt() & 0x00FFFFFF) : oldvalue;
			UniFields.SetInteger(fields, field, colorval, defaultvalue);
		}

		private void CheckColor() 
		{
			bool changed = string.IsNullOrEmpty(tbColor.Text) || (cpColor.Color.ToInt() & 0x00FFFFFF) != defaultvalue;
			bReset.Visible = changed;
			tbColor.ForeColor = (changed ? SystemColors.WindowText : SystemColors.GrayText);
		}

		#endregion

		#region ================== Events

		private void bReset_Click(object sender, EventArgs e) 
		{
			cpColor.Focus(); // Otherwise the focus will go to cpColor's textbox, which is not what we want
			cpColor.Color = PixelColor.FromInt(defaultvalue).WithAlpha(255);
			cpColor_ColorChanged(this, EventArgs.Empty);
		}

		private void cpColor_ColorChanged(object sender, EventArgs e) 
		{
			if(blockupdate) return;

			blockupdate = true;
			tbColor.Text = String.Format("{0:X6}", (cpColor.Color.ToInt() & 0x00FFFFFF));
			blockupdate = false;

			CheckColor();
			if(!blockevents && OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
		}

		private void tbColor_TextChanged(object sender, EventArgs e) 
		{
			if(blockupdate) return;
			
			int colorval;
			if(int.TryParse(tbColor.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out colorval))
			{
				colorval = General.Clamp(colorval, 0, 0xFFFFFF);
				cpColor.Color = PixelColor.FromInt(colorval).WithAlpha(255);
			}

			CheckColor();
			if(!blockevents && OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
		}

		#endregion
	}
}
