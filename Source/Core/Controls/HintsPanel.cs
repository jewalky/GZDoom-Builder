using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Controls
{
	internal partial class HintsPanel : UserControl
	{
		public HintsPanel() 
		{
			InitializeComponent();
			hints.Clear();
		}

		//hints should be in rtf markup!
		internal void SetHints(string hintsText) 
		{
			hints.Clear();
			hints.SelectedRtf = hintsText;
		}

		internal void ClearHints() 
		{
			hints.Clear();
		}

		// Fight TextBoxes habit of not releasing the focus by using a carefully placed label
		private void hints_Enter(object sender, EventArgs e) 
		{
			label1.Focus();
		}
	}
}
