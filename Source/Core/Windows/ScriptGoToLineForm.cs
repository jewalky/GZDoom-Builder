using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Windows
{
	public partial class ScriptGoToLineForm : DelayedForm
	{
		public int LineNumber { get { return linenumber.GetResult(-1); } set { linenumber.Text = value.ToString(); } }
		
		public ScriptGoToLineForm()
		{
			InitializeComponent();
		}

		private void accept_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void ScriptGoToLineForm_Shown(object sender, EventArgs e)
		{
			linenumber.Focus();
		}
	}
}
