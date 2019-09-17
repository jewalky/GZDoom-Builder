using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Data;
using System;
using System.Windows.Forms;
using System.Reflection;

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
	public partial class QuickTextureForm : DelayedForm
	{
		#region ================== Constructor / Show

		public QuickTextureForm()
		{
			InitializeComponent();
		}

		// This sets the properties and shows the form
		public new DialogResult Show(IWin32Window owner)
		{
			SetupTextureControl(this.slot1, 1);
			SetupTextureControl(this.slot2, 2);
			SetupTextureControl(this.slot3, 3);
			SetupTextureControl(this.slot4, 4);
			SetupTextureControl(this.slot5, 5);
			SetupTextureControl(this.slot6, 6);
			SetupTextureControl(this.slot7, 7);
			SetupTextureControl(this.slot8, 8);
			SetupTextureControl(this.slot9, 9);
			SetupTextureControl(this.slot10, 10);

			return this.ShowDialog(owner);
		}

		#endregion

		private void SetupTextureControl(TextureOrFlatSelectorControl control, int slot)
		{
			TextureOrFlatName texture = General.Map.QuickTextures.GetQuickTexture(slot);
			if (texture == null)
			{
				return;
			}
			control.TextureName = texture.LumpName;
			control.IsFlat = texture.IsFlat;
		}

		private void apply_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();

			UpdateQuickTexture(1, this.slot1);
			UpdateQuickTexture(2, this.slot2);
			UpdateQuickTexture(3, this.slot3);
			UpdateQuickTexture(4, this.slot4);
			UpdateQuickTexture(5, this.slot5);
			UpdateQuickTexture(6, this.slot6);
			UpdateQuickTexture(7, this.slot7);
			UpdateQuickTexture(8, this.slot8);
			UpdateQuickTexture(9, this.slot9);
			UpdateQuickTexture(0, this.slot10);

			General.Interface.Focus();
			General.Editing.AcceptMode();
		}
		
		private void UpdateQuickTexture(int slot, TextureOrFlatSelectorControl control)
		{
			General.Map.QuickTextures.UpdateQuickTexture(slot, new Data.TextureOrFlatName(control.TextureName, control.IsFlat));
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
