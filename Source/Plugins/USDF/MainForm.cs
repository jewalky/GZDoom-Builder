#region === Copyright (c) 2010 Pascal van der Heiden ===

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.USDF
{
	public partial class MainForm : DelayedForm
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		// Position/size
		private Point lastposition;
		private Size lastsize;
		
		#endregion
		
		#region ================== Properties
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public MainForm()
		{
			InitializeComponent();
			
			// Load data from DIALOGUE lump
			MemoryStream s = General.Map.GetLumpData("DIALOGUE");
			if(s != null)
			{
				
			}
		}
		
		#endregion
		
		#region ================== Methods
		
		// Called before the map is saved so we can save our data
		public void SaveData()
		{
		}
		
		#endregion
		
		#region ================== Events
		
		// Form loaded
		private void MainForm_Load(object sender, EventArgs e)
		{
			this.SuspendLayout();
			this.Location = new Point(General.Settings.ReadPluginSetting("mainwindow.positionx", this.Location.X),
									  General.Settings.ReadPluginSetting("mainwindow.positiony", this.Location.Y));
			this.Size = new Size(General.Settings.ReadPluginSetting("mainwindow.sizewidth", this.Size.Width),
								 General.Settings.ReadPluginSetting("mainwindow.sizeheight", this.Size.Height));
			this.WindowState = (FormWindowState)General.Settings.ReadPluginSetting("mainwindow.windowstate", (int)FormWindowState.Normal);
			this.ResumeLayout(true);
			
			// Normal windowstate?
			if(this.WindowState == FormWindowState.Normal)
			{
				// Keep last position and size
				lastposition = this.Location;
				lastsize = this.Size;
			}
		}
		
		// Form is being closed
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			int windowstate;
			
			// Determine window state to save
			if(this.WindowState != FormWindowState.Minimized)
				windowstate = (int)this.WindowState;
			else
				windowstate = (int)FormWindowState.Normal;
			
			// Save window settings
			General.Settings.WritePluginSetting("mainwindow.positionx", lastposition.X);
			General.Settings.WritePluginSetting("mainwindow.positiony", lastposition.Y);
			General.Settings.WritePluginSetting("mainwindow.sizewidth", lastsize.Width);
			General.Settings.WritePluginSetting("mainwindow.sizeheight", lastsize.Height);
			General.Settings.WritePluginSetting("mainwindow.windowstate", windowstate);
			
			// Save dialog data
			SaveData();
		}
		
		// Form resized
		private void MainForm_ResizeEnd(object sender, EventArgs e)
		{
			// Normal windowstate?
			if(this.WindowState == FormWindowState.Normal)
			{
				// Keep last position and size
				lastposition = this.Location;
				lastsize = this.Size;
			}
		}
		
		// Form moved
		private void MainForm_Move(object sender, EventArgs e)
		{
			// Normal windowstate?
			if(this.WindowState == FormWindowState.Normal)
			{
				// Keep last position and size
				lastposition = this.Location;
				lastsize = this.Size;
			}
		}
		
		#endregion
	}
}