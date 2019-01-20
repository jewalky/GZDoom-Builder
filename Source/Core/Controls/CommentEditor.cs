#region ================== Copyright

/*
 * Made by MaxED, mkay?
 */

#endregion

#region ================== Namespaces

using System;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public partial class CommentEditor : UserControl
	{
		#region ================== Variables

		private const string DEFAULT_TEXT = "Click to enter a comment...";
		private bool mixedvalues;
		private int commenttype;

		#endregion

		#region ================== Constructor / Disposer

		public CommentEditor()
		{
			InitializeComponent();
		}

		#endregion

		#region ================== Methods

		public void SetValues(UniFields fields, bool first)
		{
			if(mixedvalues) return;
			string c = fields.GetValue("comment", string.Empty);
			if(first)
			{
				textbox.Text = c;
			}
			else if(textbox.Text != c)
			{
				textbox.Clear();
				mixedvalues = true;
			}
		}

		public void FinishSetup()
		{
			if(mixedvalues) return;

			if(string.IsNullOrEmpty(textbox.Text))
			{
				textbox.Text = DEFAULT_TEXT;
				textbox.ForeColor = SystemColors.InactiveCaptionText;
				radioButton1.Checked = true;
			}
			else if(textbox.Text.Length > 2)
			{
				string type = textbox.Text.Substring(0, 3);
				int index = Array.IndexOf(CommentType.Types, type);
				commenttype = (index > 0 ? index : 0);
				if(commenttype > 0) textbox.Text = textbox.Text.TrimStart(type.ToCharArray());

				// Isn't there a better way to do this?..
				switch(commenttype)
				{
					case 0: radioButton1.Checked = true; break;
					case 1: radioButton2.Checked = true; break;
					case 2: radioButton3.Checked = true; break;
					case 3: radioButton4.Checked = true; break;
					case 4: radioButton5.Checked = true; break;
				}
			}
		}

		public void Apply(UniFields fields)
		{
			if(mixedvalues) return;
			string text = (!string.IsNullOrEmpty(textbox.Text) && textbox.Text != DEFAULT_TEXT ? CommentType.Types[commenttype] + textbox.Text : String.Empty);
			UniFields.SetString(fields, "comment", text, string.Empty);
		}

		#endregion

		#region ================== Events

		private void clear_Click(object sender, EventArgs e)
		{
			textbox.Text = DEFAULT_TEXT;
			textbox.ForeColor = SystemColors.InactiveCaptionText;
			mixedvalues = false;
		}

		private void textbox_Enter(object sender, EventArgs e)
		{
			if(textbox.Text == DEFAULT_TEXT)
			{
				textbox.Clear();
				textbox.ForeColor = SystemColors.WindowText;
			}
		}

		private void textbox_Leave(object sender, EventArgs e)
		{
			if(string.IsNullOrEmpty(textbox.Text) && !mixedvalues)
			{
				textbox.Text = DEFAULT_TEXT;
				textbox.ForeColor = SystemColors.InactiveCaptionText;
			}
		}

		private void textbox_TextChanged(object sender, EventArgs e)
		{
			mixedvalues = false;
		}

		private void radiobutton_CheckedChanged(object sender, EventArgs e)
		{
			commenttype = (int)((Control)sender).Tag;
		}

		// We don't want to close the parent form when user presses Enter while typing the text
		private void textbox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if(e.KeyCode == Keys.Enter) e.IsInputKey = true;
		}

		//mxd. Because anchor-based alignment fails when using high-Dpi settings...
		private void CommentEditor_Resize(object sender, EventArgs e)
		{
			clear.Left = this.Width - clear.Margin.Right - clear.Width;
			textbox.Width = this.Width - textbox.Left - textbox.Margin.Right;
			textbox.Height = this.Height - textbox.Top - textbox.Margin.Bottom;
		}

		#endregion
	}
}
