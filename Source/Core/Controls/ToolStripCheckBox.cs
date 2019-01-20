using System;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace CodeImp.DoomBuilder.Controls
{
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
	public class ToolStripCheckBox : ToolStripControlHost
	{
		public event EventHandler CheckedChanged;

		public bool Checked { get { return cb.Checked; } set { cb.Checked = value; } }
		new public string Text { get { return cb.Text; } set { cb.Text = value; } }
		private CheckBox cb;

		public ToolStripCheckBox() : base(new CheckBox()) { }

		protected override void OnSubscribeControlEvents(Control control) 
		{
			base.OnSubscribeControlEvents(control);
			cb = control as CheckBox;
			cb.CheckedChanged += OnCheckedChanged;
		}

		protected override void OnUnsubscribeControlEvents(Control control) 
		{
			base.OnUnsubscribeControlEvents(control);
			cb.CheckedChanged -= OnCheckedChanged;
		}

		private void OnCheckedChanged(object sender, EventArgs e) 
		{
			if(CheckedChanged != null) CheckedChanged(this, e);
		}
	}
}
