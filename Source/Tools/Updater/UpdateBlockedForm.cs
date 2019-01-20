using System;
using System.Windows.Forms;

namespace mxd.GZDBUpdater
{
	public partial class UpdateBlockedForm : Form
	{
		private bool formaccepted;

		public UpdateBlockedForm()
		{
			InitializeComponent();
			this.Icon = MainForm.AppIcon;
		}

		private void UpdateBlockedForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.DialogResult = (formaccepted && proceed.Checked ? DialogResult.OK : DialogResult.Cancel);
		}

		private void accept_Click(object sender, EventArgs e)
		{
			formaccepted = true;
			this.Close();
		}
	}
}
