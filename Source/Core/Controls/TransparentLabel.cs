using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Controls
{
	//mxd. Label, which ignores mouse events
	internal class TransparentLabel : Label
	{
		private const int WM_NCHITTEST = 0x0084;
		private const int WM_MOUSEHOVER = 0x02A1;
		private const int HTTRANSPARENT = -1;
		
		protected override void WndProc(ref Message m)
		{
			switch(m.Msg)
			{
				case WM_NCHITTEST:
				case WM_MOUSEHOVER:
					m.Result = (IntPtr)HTTRANSPARENT;
					break;

				default:
					base.WndProc(ref m);
					break;
			}
		}
	}
}
