#region ================== Namespaces

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

#endregion

namespace CodeImp.DoomBuilder.ColorPicker.Controls 
{
	public partial class ColorPickerControl : UserControl 
	{
		private enum ChangeStyle 
		{
			MouseMove,
			RGB,
			None
		}

		private const string COLOR_INFO_RGB = "RGB";
		private const string COLOR_INFO_HEX = "Hex";
		private const string COLOR_INFO_FLOAT = "Float";
		private readonly object[] COLOR_INFO = new object[] { COLOR_INFO_RGB, COLOR_INFO_HEX, COLOR_INFO_FLOAT };

		private static int colorInfoMode;
		
		private ChangeStyle changeType = ChangeStyle.None;
		private Point selectedPoint;

		private ColorWheel colorWheel;
		private ColorHandler.RGB RGB;

		public ColorHandler.RGB CurrentColor { get { return RGB; } }

		private bool isInUpdate;
		private Color startColor;

		//events
		public event EventHandler<ColorChangedEventArgs> OnColorChanged;
		public event EventHandler OnOkPressed;
		public event EventHandler OnCancelPressed;

		public Button OkButton { get { return btnOK; } }
		public Button CancelButton { get { return btnCancel; } }

		public void Initialize(Color startColor)
		{
			this.startColor = startColor;

			isInUpdate = true;
			InitializeComponent();
			isInUpdate = false;

			cbColorInfo.Items.AddRange(COLOR_INFO);
			cbColorInfo.SelectedIndex = colorInfoMode;
		}

		private void NudValueChanged(object sender, EventArgs e) 
		{
			// If the R, G, or B values change, use this code to update the HSV values and invalidate
			// the color wheel (so it updates the pointers).
			// Check the isInUpdate flag to avoid recursive events when you update the NumericUpdownControls.
			if(!isInUpdate) 
			{
				changeType = ChangeStyle.RGB;
				RGB = new ColorHandler.RGB((int)nudRed.Value, (int)nudGreen.Value, (int)nudBlue.Value);
				UpdateColorInfo(RGB);
				this.Invalidate();
			}
		}

		private void SetRGB(ColorHandler.RGB RGB) 
		{
			// Update the RGB values on the form, but don't trigger the ValueChanged event of the form. The isInUpdate
			// variable ensures that the event procedures exit without doing anything.
			isInUpdate = true;
			UpdateColorInfo(RGB);
			isInUpdate = false;
		}

		private void UpdateColorInfo(ColorHandler.RGB RGB) 
		{
			this.RGB = RGB;
			btnOK.BackColor = ColorHandler.RGBtoColor(RGB);
			btnOK.ForeColor = (RGB.Red < 180 && RGB.Green < 180) ? Color.White : Color.Black;

			//update color info
			switch(cbColorInfo.SelectedItem.ToString()) 
			{
				case COLOR_INFO_RGB:
					RefreshNudValue(nudRed, RGB.Red);
					RefreshNudValue(nudBlue, RGB.Blue);
					RefreshNudValue(nudGreen, RGB.Green);
					break;

				case COLOR_INFO_HEX:
					string r = RGB.Red.ToString("X02");
					string g = RGB.Green.ToString("X02");
					string b = RGB.Blue.ToString("X02");

					isInUpdate = true;
					tbFloatVals.Text = r + g + b;
					isInUpdate = false;
					break;

				case COLOR_INFO_FLOAT:
					string r2 = ((float)Math.Round(RGB.Red / 255f, 2)).ToString("F02", CultureInfo.InvariantCulture);
					string g2 = ((float)Math.Round(RGB.Green / 255f, 2)).ToString("F02", CultureInfo.InvariantCulture);
					string b2 = ((float)Math.Round(RGB.Blue / 255f, 2)).ToString("F02", CultureInfo.InvariantCulture);

					isInUpdate = true;
					tbFloatVals.Text = r2 + " " + g2 + " " + b2;
					isInUpdate = false;
					break;
			}

			//dispatch event further
			EventHandler<ColorChangedEventArgs> handler = OnColorChanged;
			if(handler != null)
				handler(this, new ColorChangedEventArgs(RGB, ColorHandler.RGBtoHSV(RGB)));
		}

		private void UpdateCancelButton(ColorHandler.RGB RGB) 
		{
			btnCancel.BackColor = ColorHandler.RGBtoColor(RGB);
			btnCancel.ForeColor = (RGB.Red < 180 && RGB.Green < 180) ? Color.White : Color.Black;
		}

		private static void RefreshNudValue(NumericUpDown nud, int value) 
		{
			// Update the value of the NumericUpDown control, if the value is different than the current value.
			// Refresh the control, causing an immediate repaint.
			if(nud.Value != value) 
			{
				nud.Value = value;
				nud.Refresh();
			}
		}

		public void SetCurrentColor(Color c) 
		{
			isInUpdate = true;
			changeType = ChangeStyle.RGB;
			RGB = new ColorHandler.RGB(c.R, c.G, c.B);

			UpdateColorInfo(RGB);
			isInUpdate = false;
			this.Invalidate();
		}

		public void SetInitialColor(Color c) 
		{
			UpdateCancelButton(new ColorHandler.RGB(c.R, c.G, c.B));
		}

		//events
		private void ColorPickerControl_Load(object sender, EventArgs e) 
		{
			// Turn on double-buffering, so the form looks better. 
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.DoubleBuffer, true);

			Rectangle BrightnessRectangle = new Rectangle(pnlBrightness.Location, pnlBrightness.Size);
			Rectangle ColorRectangle = new Rectangle(pnlColor.Location, pnlColor.Size);

			// Create the new ColorWheel class, indicating the locations of the color wheel itself, the
			// brightness area, and the position of the selected color.
			colorWheel = new ColorWheel(ColorRectangle, BrightnessRectangle);
			colorWheel.ColorChanged += ColorChanged;

			//set initial colors
			SetCurrentColor(startColor);
			UpdateCancelButton(RGB);
		}

		private void ColorChanged(object sender, ColorChangedEventArgs e) 
		{
			SetRGB(e.RGB);
		}

		private void OnPaint(object sender, PaintEventArgs e) 
		{
			// Depending on the circumstances, force a repaint of the color wheel passing different information.
			switch(changeType) 
			{
				case ChangeStyle.MouseMove:
				case ChangeStyle.None:
					colorWheel.Draw(e.Graphics, selectedPoint);
					break;
				case ChangeStyle.RGB:
					colorWheel.Draw(e.Graphics, RGB);
					break;
			}
		}

		private void ColorPickerControl_MouseDown(object sender, MouseEventArgs e) 
		{
			if(e.Button == MouseButtons.Left) 
			{
				changeType = ChangeStyle.MouseMove;
				selectedPoint = new Point(e.X, e.Y);
				this.Invalidate();
			}
		}

		private void OnMouseMove(object sender, MouseEventArgs e) 
		{
			if(e.Button == MouseButtons.Left) 
			{
				changeType = ChangeStyle.MouseMove;
				selectedPoint = new Point(e.X, e.Y);
				this.Invalidate();
			}
		}

		private void OnMouseUp(object sender, MouseEventArgs e) 
		{
			colorWheel.SetMouseUp();
			changeType = ChangeStyle.None;
		}

		private void OnMouseLeave(object sender, EventArgs e) 
		{
			colorWheel.SetMouseUp();
			changeType = ChangeStyle.None;
			selectedPoint = Point.Empty;
		}

		private void btnOK_Click(object sender, EventArgs e) 
		{
			//dispatch event further
			EventHandler handler = OnOkPressed;
			if(handler != null) handler(this, e);
		}

		private void btnCancel_Click(object sender, EventArgs e) 
		{
			//dispatch event further
			EventHandler handler = OnCancelPressed;
			if(handler != null) handler(this, e);
		}

		private void cbColorInfo_SelectedIndexChanged(object sender, EventArgs e) 
		{
			if(cbColorInfo.SelectedItem.ToString() == COLOR_INFO_RGB) 
			{
				pRGB.Visible = true;
				tbFloatVals.Visible = false;
			} 
			else 
			{
				pRGB.Visible = false;
				tbFloatVals.Visible = true;
			}
			colorInfoMode = cbColorInfo.SelectedIndex;
			UpdateColorInfo(RGB);
		}

		private void tbFloatVals_TextChanged(object sender, EventArgs e) 
		{
			if(isInUpdate) return;
			
			if(COLOR_INFO[colorInfoMode].ToString() == COLOR_INFO_FLOAT) 
			{
				string[] parts = tbFloatVals.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if(parts.Length != 3) return;

				ColorHandler.RGB rgb = new ColorHandler.RGB();

				float c;
				if(!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out c)) return;
				rgb.Red = (int)(General.Clamp(Math.Abs(c), 0.0f, 1.0f) * 255);

				if(!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out c)) return;
				rgb.Green = (int)(General.Clamp(Math.Abs(c), 0.0f, 1.0f) * 255);

				if(!float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out c)) return;
				rgb.Blue = (int)(General.Clamp(Math.Abs(c), 0.0f, 1.0f) * 255);

				changeType = ChangeStyle.RGB;
				UpdateColorInfo(rgb);
				this.Invalidate();
			} 
			else if(COLOR_INFO[colorInfoMode].ToString() == COLOR_INFO_HEX) 
			{
				string hexColor = tbFloatVals.Text.Trim().Replace("-", "");
				if(hexColor.Length != 6) return;

				ColorHandler.RGB rgb = new ColorHandler.RGB();
				int color;

				string colorStr = hexColor.Substring(0, 2);
				if(!int.TryParse(colorStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color)) return;
				rgb.Red = color;

				colorStr = hexColor.Substring(2, 2);
				if(!int.TryParse(colorStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color)) return;
				rgb.Green = color;

				colorStr = hexColor.Substring(4, 2);
				if(!int.TryParse(colorStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color)) return;
				rgb.Blue = color;

				changeType = ChangeStyle.RGB;
				UpdateColorInfo(rgb);
				this.Invalidate();
			}
		}
	}
}