
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class AboutForm : DelayedForm
	{
		// Constructor
		public AboutForm()
		{
			// Initialize
			InitializeComponent();

			// Show version
#if DEBUG
			version.Text = Application.ProductName + " [DEVBUILD]";
#else
			version.Text = Application.ProductName + " v" + Application.ProductVersion + " (" + General.CommitHash + ")";
#endif
		}

		// Launch Doom Builder website
		private void builderlink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			General.OpenWebsite("http://" + builderlink.Text);
		}

		// Launch CodeImp website
		private void codeimplink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			General.OpenWebsite("http://" + codeimplink.Text);
		}

		//mxd
		private void zdoomorglink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) 
		{
			General.OpenWebsite("http://forum.zdoom.org/viewtopic.php?f=44&t=54957");
		}

		//mxd
		private void gitlink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			General.OpenWebsite("https://github.com/jewalky/GZDoom-Builder-Bugfix");
		}

		// This copies the version number to clipboard
		private void copyversion_Click(object sender, EventArgs e)
		{
			try //mxd
			{
				Clipboard.SetDataObject(Application.ProductVersion + " (" + General.CommitHash + ")", true, 5, 200);
			}
			catch(ExternalException)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Failed to perform a Clipboard operation...");
			}
		}
	}
}