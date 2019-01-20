using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Controls
{
	public class TransparentListView : ListView
	{
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT

				return cp;
			}
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			// Don't paint background
		}
	}
}
