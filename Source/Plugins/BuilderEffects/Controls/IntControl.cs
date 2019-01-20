#region ================== Namespaces

using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.BuilderEffects
{
	[DesignerCategory("code")]
	internal partial class IntControl : UserControl
	{
		public event EventHandler OnValueChanging;
		public event EventHandler OnValueChanged;

		private int previousValue;
		public int Value 
		{
			get { return (int)numericUpDown1.Value; }
			set 
			{
				blockEvents = true;
				previousValue = General.Clamp(value, (int)numericUpDown1.Minimum, (int)numericUpDown1.Maximum);
				numericUpDown1.Value = previousValue;
				trackBar1.Value = previousValue;
				valueChanged = false;
				blockEvents = false;
			}
		}

		internal int Delta { get { return delta; } }
		private int delta;

		public string Label { get { return label1.Text; } set { label1.Text = value; } }

		public bool ExtendedLimits 
		{
			get { return extendedLimits; }
			set {
				extendedLimits = value;
				UpdateLimits();
			}
		}
		private bool extendedLimits;

		public bool AllowNegative 
		{
			get { return allowNegative; }
			set {
				allowNegative = value;

				if(!allowNegative)
				{
					if(minimum < 0 && maximum < 0) 
					{
						int diff = Math.Abs(maximum - minimum);
						minimum = 0;
						maximum = diff;
					} 
					else 
					{
						if(minimum < 0) minimum = 0;
						if(maximum < 0) maximum = 0;
					}
				}

				UpdateLimits();
			}
		}

		private bool allowNegative;

		public int Minimum 
		{
			get { return minimum; }
			set 
			{
				minimum = value;
				UpdateLimits();
			}
		}
		private int minimum;
		
		public int Maximum 
		{
			get { return maximum; }
			set 
			{
				maximum = value;
				UpdateLimits();
			}
		}
		private int maximum;

		private bool blockEvents;
		private bool valueChanged;

		internal IntControl() 
		{
			InitializeComponent();
			numericUpDown1.MouseLeave += numericUpDown1_MouseLeave;
		}

		private void UpdateLimits() 
		{
			blockEvents = true;

			trackBar1.Value = General.Clamp(trackBar1.Value, minimum, maximum);
			trackBar1.Minimum = minimum;
			trackBar1.Maximum = maximum;
			labelMaximum.Text = maximum.ToString();

			numericUpDown1.Value = General.Clamp((int)numericUpDown1.Value, minimum, maximum);

			if(extendedLimits) 
			{
				numericUpDown1.Minimum = minimum * 32;
				numericUpDown1.Maximum = maximum * 32;
			} 
			else 
			{
				numericUpDown1.Minimum = minimum;
				numericUpDown1.Maximum = maximum;
			}

			blockEvents = false;
		}

		//events
		private void trackBar1_ValueChanged(object sender, EventArgs e) 
		{
			if(!blockEvents) numericUpDown1.Value = ((TrackBar)sender).Value;
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e) 
		{
			int value = (int)((NumericUpDown)sender).Value;
			if(value == previousValue) return;

			valueChanged = true;
			delta = value - previousValue;
			previousValue = value;

			if(!blockEvents && OnValueChanging != null)
				OnValueChanging(this, EventArgs.Empty);

			blockEvents = true;
			trackBar1.Value = General.Clamp(value, trackBar1.Minimum, trackBar1.Maximum);
			blockEvents = false;
		}

		private void trackBar1_MouseLeave(object sender, EventArgs e) 
		{
			if(valueChanged && OnValueChanged != null) 
			{
				OnValueChanged(this, EventArgs.Empty);
				valueChanged = false;
			}
		}

		private void numericUpDown1_MouseLeave(object sender, EventArgs e) 
		{
			if(valueChanged && OnValueChanged != null) 
			{
				OnValueChanged(this, EventArgs.Empty);
				valueChanged = false;
			}
		}

	}
}
