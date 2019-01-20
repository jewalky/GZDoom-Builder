
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

using System.Globalization;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal partial class VertexInfoPanel : UserControl
	{
		// Constructor
		public VertexInfoPanel()
		{
			// Initialize
			InitializeComponent();
		}

		// This shows the info
		public void ShowInfo(Vertex v)
		{
			// Vertex info
			vertexinfo.Text = " Vertex " + v.Index + " ";
			position.Text = v.Position.x.ToString(CultureInfo.InvariantCulture) + ", " + v.Position.y.ToString(CultureInfo.InvariantCulture);
			
			//mxd. Height offsets
			if(General.Map.UDMF)
			{
				bool haveoffset = !float.IsNaN(v.ZCeiling);
				zceiling.Text = (haveoffset ? v.ZCeiling.ToString(CultureInfo.InvariantCulture) : "--");
				zceiling.Enabled = haveoffset;
				labelzceiling.Enabled = haveoffset;

				haveoffset = !float.IsNaN(v.ZFloor);
				zfloor.Text = (haveoffset ? v.ZFloor.ToString(CultureInfo.InvariantCulture) : "--");
				zfloor.Enabled = haveoffset;
				labelzfloor.Enabled = haveoffset;
			}

			panelOffsets.Visible = General.Map.UDMF;

			// Show the whole thing
			this.Show();
            //this.Update(); // ano - don't think this is needed, and is slow
        }
    }
}
