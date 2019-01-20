
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

using System;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public partial class EditSelectionPanel : UserControl
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		// Editing mode
		private EditSelectionMode mode;
		
		// Input
		private bool userinput;
		private bool preventchanges; //mxd
		
		// Values
		private Vector2D orgpos;
		private Vector2D orgsize;
		private Vector2D abspos;
		private Vector2D relpos;
		private Vector2D abssize;
		private Vector2D relsize;
		private float absrotate;
		
		#endregion
		
		#region ================== Constructor
		
		// Constructor
		public EditSelectionPanel(EditSelectionMode mode)
		{
			InitializeComponent();
			this.mode = mode;

			//mxd
			preventchanges = true;
			if(General.Map.UDMF)
			{
				preciseposition.Checked = mode.UsePrecisePosition;
				preciseposition.Enabled = true;
			}
			else
			{
				preciseposition.Checked = false;
				preciseposition.Enabled = false;
			}
			preventchanges = false;

			//mxd. Otherwise the focus will go to one of TextBoxes 
			// and stay there forever preventing tab collapsing when in collapsed mode
			label1.Focus();
		}
		
		#endregion
		
		#region ================== Methods
		
		// This sets the original size
		public void ShowOriginalValues(Vector2D pos, Vector2D size)
		{
			// Set values
			this.orgpos = pos;
			this.orgsize = size;
			
			// Set controls
			orgposx.Text = pos.x.ToString();
			orgposy.Text = pos.y.ToString();
			orgsizex.Text = size.x.ToString();
			orgsizey.Text = size.y.ToString();
		}
		
		// This sets the dynamic values
		public void ShowCurrentValues(Vector2D pos, Vector2D relpos, Vector2D size, Vector2D relsize, float rotation)
		{
			// Set values
			this.abspos = pos;
			this.relpos = relpos;
			this.abssize = size;
			this.relsize = relsize;
			this.absrotate = Angle2D.RadToDeg(rotation);
			
			// Set controls
			absposx.Text = pos.x.ToString("0.#");
			absposy.Text = pos.y.ToString("0.#");
			relposx.Text = relpos.x.ToString("0.#");
			relposy.Text = relpos.y.ToString("0.#");
			abssizex.Text = size.x.ToString("0.#");
			abssizey.Text = size.y.ToString("0.#");
			relsizex.Text = relsize.x.ToString("0.#");
			relsizey.Text = relsize.y.ToString("0.#");
			absrot.Text = this.absrotate.ToString("0.#");
			
			userinput = false;
		}

		//mxd
		internal void SetTextureTransformSettings(bool enable)
		{
			// Disable groups?
			if(!enable)
			{
				ceiltexgroup.Enabled = false;
				floortexgroup.Enabled = false;
				ceiltexall.Enabled = false;
				floortexall.Enabled = false;
				return;
			}

			// Update checkboxes
			preventchanges = true;

			floortexoffset.Checked = mode.TransformFloorOffsets;
			ceiltexoffset.Checked = mode.TransformCeilingOffsets;
			floortexrotation.Checked = mode.RotateFloorOffsets;
			ceiltexrotation.Checked = mode.RotateCeilingOffsets;
			floortexscale.Checked = mode.ScaleFloorOffsets;
			ceiltexscale.Checked = mode.ScaleCeilingOffsets;
			floortexall.Checked = (mode.TransformFloorOffsets && mode.RotateFloorOffsets && mode.ScaleFloorOffsets);
			ceiltexall.Checked = (mode.TransformCeilingOffsets && mode.RotateCeilingOffsets && mode.ScaleCeilingOffsets);

			preventchanges = false;
		}

		//mxd
		internal void SetHeightAdjustMode(EditSelectionMode.HeightAdjustMode adjustmode, bool enable)
		{
			preventchanges = true;
			heightmode.SelectedIndex = (int)adjustmode;
			heightmode.Enabled = enable;
			preventchanges = false;
		}

		//mxd
		private void UpdateAllFloorTransformsCheckbox()
		{
			preventchanges = true;

			int i = 0;
			if(floortexoffset.Checked) i++;
			if(floortexrotation.Checked) i++;
			if(floortexscale.Checked) i++;
			floortexall.Checked = (i == 3);

			preventchanges = false;
		}

		//mxd
		private void UpdateAllCeilingTransformsCheckbox() 
		{
			preventchanges = true;

			int i = 0;
			if(ceiltexoffset.Checked) i++;
			if(ceiltexrotation.Checked) i++;
			if(ceiltexscale.Checked) i++;
			ceiltexall.Checked = (i == 3);

			preventchanges = false;
		}
		
		#endregion
		
		#region ================== Events
		
		// User input given
		private void WhenTextChanged(object sender, EventArgs e)
		{
			userinput = true;
		}
		
		private void absposx_Validated(object sender, EventArgs e)
		{
			if(userinput)
				mode.SetAbsPosX(absposx.GetResultFloat(this.abspos.x));
		}

		private void absposy_Validated(object sender, EventArgs e)
		{
			if(userinput)
				mode.SetAbsPosY(absposy.GetResultFloat(this.abspos.y));
		}

		private void relposx_Validated(object sender, EventArgs e)
		{
			if(userinput)
				mode.SetRelPosX(relposx.GetResultFloat(this.relpos.x));
		}

		private void relposy_Validated(object sender, EventArgs e)
		{
			if(userinput)
				mode.SetRelPosY(relposy.GetResultFloat(this.relpos.y));
		}

		private void abssizex_Validated(object sender, EventArgs e)
		{
			if(userinput)
				mode.SetAbsSizeX(abssizex.GetResultFloat(this.abssize.x));
		}

		private void abssizey_Validated(object sender, EventArgs e)
		{
			if(userinput)
				mode.SetAbsSizeY(abssizey.GetResultFloat(this.abssize.y));
		}

		private void relsizex_Validated(object sender, EventArgs e)
		{
			if(userinput)
				mode.SetRelSizeX(relsizex.GetResultFloat(this.relsize.x));
		}

		private void relsizey_Validated(object sender, EventArgs e)
		{
			if(userinput)
				mode.SetRelSizeY(relsizey.GetResultFloat(this.relsize.y));
		}

		private void absrot_Validated(object sender, EventArgs e)
		{
			if(userinput)
			{
				float rad = Angle2D.DegToRad(absrot.GetResultFloat(this.absrotate));
				mode.SetAbsRotation(rad);
			}
		}

		private void fliph_Click(object sender, EventArgs e)
		{
			General.Actions.InvokeAction("buildermodes_flipselectionh");
			General.Interface.FocusDisplay();
		}

		private void flipv_Click(object sender, EventArgs e)
		{
			General.Actions.InvokeAction("buildermodes_flipselectionv");
			General.Interface.FocusDisplay();
		}

		private void orgposx_Click(object sender, EventArgs e)
		{
			mode.SetAbsPosX(orgpos.x);
			General.Interface.FocusDisplay();
		}

		private void orgposy_Click(object sender, EventArgs e)
		{
			mode.SetAbsPosY(orgpos.y);
			General.Interface.FocusDisplay();
		}

		private void orgsizex_Click(object sender, EventArgs e)
		{
			mode.SetAbsSizeX(orgsize.x);
			General.Interface.FocusDisplay();
		}

		private void orgsizey_Click(object sender, EventArgs e)
		{
			mode.SetAbsSizeY(orgsize.y);
			General.Interface.FocusDisplay();
		}

		//mxd
		private void floortexoffset_CheckedChanged(object sender, EventArgs e)
		{
			if(preventchanges) return;
			mode.TransformFloorOffsets = floortexoffset.Checked;
			UpdateAllFloorTransformsCheckbox();
			General.Interface.FocusDisplay();
		}

		//mxd
		private void ceiltexoffset_CheckedChanged(object sender, EventArgs e) 
		{
			if(preventchanges) return;
			mode.TransformCeilingOffsets = ceiltexoffset.Checked;
			UpdateAllCeilingTransformsCheckbox();
			General.Interface.FocusDisplay();
		}

		//mxd
		private void floortexrotation_CheckedChanged(object sender, EventArgs e) 
		{
			if(preventchanges) return;
			mode.RotateFloorOffsets = floortexrotation.Checked;
			UpdateAllFloorTransformsCheckbox();
			General.Interface.FocusDisplay();
		}

		//mxd
		private void ceiltexrotation_CheckedChanged(object sender, EventArgs e) 
		{
			if(preventchanges) return;
			mode.RotateCeilingOffsets = ceiltexrotation.Checked;
			UpdateAllCeilingTransformsCheckbox();
			General.Interface.FocusDisplay();
		}

		//mxd
		private void floortexscale_CheckedChanged(object sender, EventArgs e)
		{
			if(preventchanges) return;
			mode.ScaleFloorOffsets = floortexscale.Checked;
			UpdateAllFloorTransformsCheckbox();
			General.Interface.FocusDisplay();
		}

		//mxd
		private void ceiltexscale_CheckedChanged(object sender, EventArgs e)
		{
			if(preventchanges) return;
			mode.ScaleCeilingOffsets = ceiltexscale.Checked;
			UpdateAllCeilingTransformsCheckbox();
			General.Interface.FocusDisplay();
		}

		//mxd
		private void floortexall_CheckedChanged(object sender, EventArgs e) 
		{
			if(preventchanges) return;
			preventchanges = true;

			floortexoffset.Checked = floortexall.Checked;
			floortexrotation.Checked = floortexall.Checked;
			floortexscale.Checked = floortexall.Checked;

			mode.TransformFloorOffsets = floortexoffset.Checked;
			mode.RotateFloorOffsets = floortexrotation.Checked;
			mode.ScaleFloorOffsets = floortexscale.Checked;

			preventchanges = false;
		}

		//mxd
		private void ceiltexall_CheckedChanged(object sender, EventArgs e) 
		{
			if(preventchanges) return;
			preventchanges = true;

			ceiltexoffset.Checked = ceiltexall.Checked;
			ceiltexrotation.Checked = ceiltexall.Checked;
			ceiltexscale.Checked = ceiltexall.Checked;

			mode.TransformCeilingOffsets = ceiltexoffset.Checked;
			mode.RotateCeilingOffsets = ceiltexrotation.Checked;
			mode.ScaleCeilingOffsets = ceiltexscale.Checked;

			preventchanges = false;
		}

		//mxd
		private void preciseposition_CheckedChanged(object sender, EventArgs e) 
		{
			if(preventchanges) return;
			mode.UsePrecisePosition = preciseposition.Checked;
			General.Interface.FocusDisplay();
		}

		//mxd
		private void heightmode_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(preventchanges || heightmode.SelectedIndex == -1) return;
			mode.SectorHeightAdjustMode = (EditSelectionMode.HeightAdjustMode)heightmode.SelectedIndex;
		}
		
		#endregion
	}
}
