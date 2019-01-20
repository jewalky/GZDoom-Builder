using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Windows
{
	public partial class UpdateForm : DelayedForm
	{
		public bool IgnoreThisUpdate { get { return ignorethisupdate.Checked; } }
		private int remoterev;

		public UpdateForm(int remoterev, string changelog)
		{
			InitializeComponent();
			Setup(remoterev, changelog);
		}

		private void Setup(int remoterev, string changelog)
		{
			this.Text = this.Text.Replace("[rev]", remoterev.ToString());
			this.label.Text = label.Text.Replace("[rev]", remoterev.ToString());
			this.changelog.SelectedRtf = changelog;
			this.remoterev = remoterev;
		}

		private void UpdateForm_Shown(object sender, EventArgs e)
		{
			this.changelog.Focus();
		}

		private void downloadupdate_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;

			// Working directory must be set
			Process.Start(new ProcessStartInfo { WorkingDirectory = General.AppPath, FileName = "Updater.exe", Arguments = "-rev " + remoterev });

			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
