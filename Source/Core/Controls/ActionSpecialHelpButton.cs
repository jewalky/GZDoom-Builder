using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Controls
{
	public partial class ActionSpecialHelpButton : UserControl
	{
		private int action;
		
		public ActionSpecialHelpButton() 
		{
			InitializeComponent();
		}

		public void UpdateAction(int newaction)
		{
			action = newaction;

			//Can we display help?
			this.Enabled = (action != 0 
				&& !string.IsNullOrEmpty(General.Map.Config.ActionSpecialHelp)
				&& General.Map.Config.LinedefActions.ContainsKey(action)
				&& !string.IsNullOrEmpty(General.Map.Config.LinedefActions[action].Id));
		}

		private void button_Click(object sender, EventArgs e)
		{
			string site = General.Map.Config.ActionSpecialHelp.Replace("%K", General.Map.Config.LinedefActions[action].Id);
			General.OpenWebsite(site);
		}
	}
}
