
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public class TransparentPanel : Panel
	{
		#region ================== Constructor / Disposer

		// Constructor
		public TransparentPanel()
		{
		}

		#endregion

		#region ================== Methods
		
		// Override this property
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x20;
				return cp;
			}
		}

		// Disable background drawing by overriding this
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if(BackColor != Color.Transparent)
				e.Graphics.Clear(BackColor);
		}
		
		#endregion
	}
}
