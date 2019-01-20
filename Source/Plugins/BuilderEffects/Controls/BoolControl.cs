using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderEffects
{
	public partial class BoolControl : UserControl
	{
        public event EventHandler OnValueChanged;
        
        public bool Value { get { return checkBox1.Checked; } set { checkBox1.Checked = value; } }
        public string Label { get { return label1.Text; } set { label1.Text = value; } }
		
		public BoolControl() {
			InitializeComponent();
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e) {
			if(OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
		}
	}
}
