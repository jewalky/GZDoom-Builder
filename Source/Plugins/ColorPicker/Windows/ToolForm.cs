using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.ColorPicker
{
	public partial class ToolsForm : Form
	{
		public ToolsForm() 
		{
			InitializeComponent();
		}

		public void Register() 
		{
			General.Interface.AddModesMenu(cpMenu, "002_modify");
			General.Interface.AddModesButton(cpButton, "002_modify");
		}

		public void Unregister() 
		{
			General.Interface.RemoveMenu(cpMenu);
			General.Interface.RemoveButton(cpButton);
		}

		private void InvokeTaggedAction(object sender, EventArgs e) 
		{
			General.Interface.InvokeTaggedAction(sender, e);
		}
	}
}
