#region === Copyright (c) 2010 Pascal van der Heiden ===

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.USDF
{
	public partial class ToolsForm : Form
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ToolsForm()
		{
			InitializeComponent();

			General.Interface.AddButton(dialogbutton, ToolbarSection.Script);
			General.Interface.AddMenu(dialogitem, MenuSection.ViewScriptEdit);
		}

		// Disposer
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			
			General.Interface.RemoveButton(dialogbutton);
			General.Interface.RemoveMenu(dialogitem);
			
			base.Dispose(disposing);
		}
		
		#endregion

		#region ================== Methods

		// This invokes an action from control event
		private void InvokeTaggedAction(object sender, EventArgs e)
		{
			General.Interface.InvokeTaggedAction(sender, e);
		}
		
		#endregion

		#region ================== Events

		#endregion
	}
}
