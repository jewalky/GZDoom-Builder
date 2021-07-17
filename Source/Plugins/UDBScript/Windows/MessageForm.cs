using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

namespace CodeImp.DoomBuilder.UDBScript
{
	public partial class MessageForm : DelayedForm
	{
		public MessageForm(string option1text, string option2text, string message)
		{
			InitializeComponent();

			if (option2text == null)
			{
				btnButton1.Text = option1text;
				btnButton1.DialogResult = DialogResult.OK;
				btnButton2.Visible = false;
			}
			else
			{
				btnButton1.Text = option2text;
				btnButton1.DialogResult = DialogResult.Cancel;
				btnButton2.Text = option1text;
				btnButton2.DialogResult = DialogResult.OK;
			}

			tbMessage.Text = message;
		}

		private void button_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
