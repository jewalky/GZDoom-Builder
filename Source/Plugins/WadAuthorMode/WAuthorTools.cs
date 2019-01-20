using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.Editing
{
	public partial class WAuthorTools : Form
	{
		// Tools
		public ContextMenuStrip LinedefPopup { get { return linedefpopup; } }
		
		// Constructor
		public WAuthorTools()
		{
			InitializeComponent();
		}
	}
}