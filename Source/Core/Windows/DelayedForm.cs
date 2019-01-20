
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Actions;
using Action = CodeImp.DoomBuilder.Actions.Action;

#endregion

// This Form is a workaround for the slow drawing of the .NET Forms.
// By showing the Form at 0% Opacity it allows the .NET framework to complete
// drawing the Form first, then we set it to 100% Opacity to actually show it.
// To use this class properly, set the initial Opacity of your Form to 0.

namespace CodeImp.DoomBuilder.Windows
{
	public class DelayedForm : Form
	{
		// Variables
		protected readonly string configname; //mxd

		//mxd. Stored window size and location. Tracks location and size of FormWindowState.Normal window 
		private Size windowsize = Size.Empty;
		private Point windowlocation = Point.Empty;
		
		// Constructor
		protected DelayedForm()
		{
			//mxd. And now, let's hope this doesn't horribly break anything...
			if(!(this is MainForm))
			{
				this.KeyPreview = true;
				this.KeyUp += OnKeyUp;
			}

			//mxd. Only when running (this.DesignMode doesn't seem to cut it here,
			// probably because not this, but a child class is in design mode...)
			if(LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
				configname = this.GetType().Name.ToLowerInvariant();
				General.Actions.BindMethods(this);
			}
		}

		//mxd
		protected override void OnLoad(EventArgs e)
		{
			// Let the base class know
			base.OnLoad(e);

			if(this.DesignMode) return;
			
			// Restore location and size
			this.SuspendLayout();

			// Restore windowstate
			if(this.MaximizeBox)
			{
				this.WindowState = (FormWindowState)General.Settings.ReadSetting("windows." + configname + ".windowstate", (int)FormWindowState.Normal);
			}

			// Form size matters?
			if(this.FormBorderStyle == FormBorderStyle.Sizable || this.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				this.Size = new Size(General.Settings.ReadSetting("windows." + configname + ".sizewidth", this.Size.Width),
									 General.Settings.ReadSetting("windows." + configname + ".sizeheight", this.Size.Height));
			}

			// Restore location
			Point validlocation = Point.Empty;
			Point location = new Point(General.Settings.ReadSetting("windows." + configname + ".positionx", int.MaxValue),
									   General.Settings.ReadSetting("windows." + configname + ".positiony", int.MaxValue));

			if(location.X < int.MaxValue && location.Y < int.MaxValue)
			{
				// Location withing screen bounds?
				Rectangle bounds = new Rectangle(location, this.Size);
				bounds.Inflate(16, 16); // Add some safety padding
				if(SystemInformation.VirtualScreen.IntersectsWith(bounds)) validlocation = location;
			}

			if(validlocation == Point.Empty && !(this is MainForm))
			{
				// Do the manual CenterParent...
				validlocation = new Point(General.MainWindow.Location.X + General.MainWindow.Width / 2 - this.Width / 2,
										  General.MainWindow.Location.Y + General.MainWindow.Height / 2 - this.Height / 2);
			}

			// Apply location
			if(validlocation == Point.Empty)
			{
				this.StartPosition = FormStartPosition.CenterParent;
			}
			else
			{
				this.StartPosition = FormStartPosition.Manual;
				this.Location = validlocation;
			}

			// Show the form if needed
			if(this.Opacity < 1.0) this.Opacity = 1.0;

			this.ResumeLayout();
		}

		//mxd. When form is closing
		protected override void OnClosing(CancelEventArgs e)
		{
			if(e.Cancel) return;
			
			// Let the base class know
			base.OnClosing(e);

			// Determine window state to save
			if(this.MaximizeBox)
			{
				int windowstate;
				if(this.WindowState != FormWindowState.Minimized)
					windowstate = (int)this.WindowState;
				else
					windowstate = (int)FormWindowState.Normal;

				General.Settings.WriteSetting("windows." + configname + ".windowstate", windowstate);
			}

			// Form size matters?
			if(this.FormBorderStyle == FormBorderStyle.Sizable || this.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				Size size = ((windowsize.IsEmpty && this.WindowState == FormWindowState.Normal) ? this.Size : windowsize); // Prefer stored size if it was set
				if(!size.IsEmpty)
				{
					General.Settings.WriteSetting("windows." + configname + ".sizewidth", size.Width);
					General.Settings.WriteSetting("windows." + configname + ".sizeheight", size.Height);
				}
			}

			// Save location
			Point location = ((windowlocation.IsEmpty && this.WindowState == FormWindowState.Normal) ? this.Location : windowlocation); // Prefer stored location if it was set
			if(!location.IsEmpty)
			{
				General.Settings.WriteSetting("windows." + configname + ".positionx", location.X);
				General.Settings.WriteSetting("windows." + configname + ".positiony", location.Y);
			}
		}

		//mxd. Also triggered when the window is dragged.
		protected override void OnResizeEnd(EventArgs e)
		{
			// Store location and size when window is not minimized or maximized
			if(this.WindowState == FormWindowState.Normal)
			{
				// Form size matters?
				if(this.FormBorderStyle == FormBorderStyle.Sizable || this.FormBorderStyle == FormBorderStyle.SizableToolWindow)
					windowsize = this.Size;
				windowlocation = this.Location;
			}

			base.OnResizeEnd(e);
		}

		//mxd. Special handling to call "save screenshot" actions from any form, 
		//which inherits form DelayedForm and either doesn't override OnKeyUp, or calls base.OnKeyUp
		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			e.Handled = (Form.ActiveForm == this && ProcessSaveScreenshotAction((int)e.KeyData));
			if(e.Handled) e.SuppressKeyPress = true;
		}

		// mxd. Handle screenshot saving from any form
		private static bool ProcessSaveScreenshotAction(int key)
		{
			Action[] actions = General.Actions.GetActionsByKey(key);
			foreach(Action action in actions) 
			{
				if(action.ShortName == "savescreenshot" || action.ShortName == "saveeditareascreenshot")
				{
					General.Actions.InvokeAction(action.Name);
					return true;
				}
			}

			return false;
		}

		//mxd
		[EndAction("savescreenshot", BaseAction = true)]
		internal void SaveScreenshot() 
		{
			if(Form.ActiveForm == this) General.MainWindow.SaveScreenshot(false);
		}

		//mxd
		[EndAction("saveeditareascreenshot", BaseAction = true)]
		internal void SaveEditAreaScreenshot() 
		{
			if(Form.ActiveForm == this) General.MainWindow.SaveScreenshot(true);
		}
	}
}
