using CodeImp.DoomBuilder.Windows;
using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
	public partial class QuickTextureForm : DelayedForm
	{
		#region ================== Properties

		public string Slot1Texture { get { return slot1.TextureName; } }
		public string Slot2Texture { get { return slot2.TextureName; } }
		public string Slot3Texture { get { return slot3.TextureName; } }
		public string Slot4Texture { get { return slot4.TextureName; } }
		public string Slot5Texture { get { return slot5.TextureName; } }
		public string Slot6Texture { get { return slot6.TextureName; } }
		public string Slot7Texture { get { return slot7.TextureName; } }
		public string Slot8Texture { get { return slot8.TextureName; } }
		public string Slot9Texture { get { return slot9.TextureName; } }
		public string Slot10Texture { get { return slot10.TextureName; } }

		#endregion

		#region ================== Constructor / Show

		public QuickTextureForm()
		{
			InitializeComponent();
		}

		// This sets the properties and shows the form
		public new DialogResult Show(IWin32Window owner)
		{
			string[] currentQuickTextures = new string[10];
			for (int slot = 0; slot < 10; slot++)
			{
				currentQuickTextures[slot] = "";
				if (General.Map.Options.QuickTextures.ContainsKey(slot))
				{
					currentQuickTextures[slot] = General.Map.Options.QuickTextures[slot];
				}
			}
			this.slot10.TextureName = currentQuickTextures[0];
			this.slot1.TextureName = currentQuickTextures[1];
			this.slot2.TextureName = currentQuickTextures[2];
			this.slot3.TextureName = currentQuickTextures[3];
			this.slot4.TextureName = currentQuickTextures[4];
			this.slot5.TextureName = currentQuickTextures[5];
			this.slot6.TextureName = currentQuickTextures[6];
			this.slot7.TextureName = currentQuickTextures[7];
			this.slot8.TextureName = currentQuickTextures[8];
			this.slot9.TextureName = currentQuickTextures[9];
			return this.ShowDialog(owner);
		}

		#endregion

		private void apply_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
			General.Map.Options.QuickTextures[0] = this.Slot10Texture;
			General.Map.Options.QuickTextures[1] = this.Slot1Texture;
			General.Map.Options.QuickTextures[2] = this.Slot2Texture;
			General.Map.Options.QuickTextures[3] = this.Slot3Texture;
			General.Map.Options.QuickTextures[4] = this.Slot4Texture;
			General.Map.Options.QuickTextures[5] = this.Slot5Texture;
			General.Map.Options.QuickTextures[6] = this.Slot6Texture;
			General.Map.Options.QuickTextures[7] = this.Slot7Texture;
			General.Map.Options.QuickTextures[8] = this.Slot8Texture;
			General.Map.Options.QuickTextures[9] = this.Slot9Texture;

			General.Interface.Focus();
			General.Editing.AcceptMode();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
