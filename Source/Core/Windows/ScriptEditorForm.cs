
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
using CodeImp.DoomBuilder.Controls;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class ScriptEditorForm : DelayedForm
	{
		#region ================== Variables

		// Closing?
		private bool appclose;
		
		#endregion
		
		#region ================== Properties
		
		public ScriptEditorPanel Editor { get { return editor; } }
		
		#endregion
		
		#region ================== Constructor
		
		// Constructor
		public ScriptEditorForm()
		{
			InitializeComponent();
			editor.Initialize(this);
		}
		
		#endregion
		
		#region ================== Methods
		
		// This asks to save files and returns the result
		// Also does implicit saves
		// Returns false when cancelled by the user
		public bool AskSaveAll()
		{
			// Implicit-save the script lumps
			editor.ImplicitSave();
			
			// Save other scripts
			return editor.AskSaveAll();
		}
		
		// Close the window
		new public void Close()
		{
			appclose = true;
			base.Close();
		}

		//mxd
		internal void OnReloadResources()
		{
			editor.OnReloadResources();
		}

		//mxd
		internal void DisplayError(TextResourceErrorItem error)
		{
			editor.ShowError(error);
		}

		//mxd
		/*internal void DisplayError(TextFileErrorItem error)
		{
			editor.ShowError(error);
		}*/

		#endregion
		
		#region ================== Events

		// Window is loaded
		private void ScriptEditorForm_Load(object sender, EventArgs e)
		{
			// Apply panel settings
			editor.ApplySettings();
		}
		
		// Window is shown
		private void ScriptEditorForm_Shown(object sender, EventArgs e)
		{
			// Focus to script editor
			editor.ForceFocus();
		}
		
		// Window is closing
		private void ScriptEditorForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			editor.SaveSettings();
			
			// Only when closed by the user
			if(!appclose && (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.FormOwnerClosing))
			{
				// Remember if scipts are changed
				General.Map.ApplyScriptChanged();
				
				// Ask to save scripts
				if(AskSaveAll())
				{
					// Let the general call close the editor
					General.Map.CloseScriptEditor(true);
				}
				else
				{
					// Cancel
					e.Cancel = true;
				}
			}

			// Not cancelling?
			if(!e.Cancel) editor.OnClose();
		}

		// Help
		private void ScriptEditorForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			if(!editor.LaunchKeywordHelp())	General.ShowHelp("w_scripteditor.html"); //mxd
			hlpevent.Handled = true;
		}
		
		#endregion
	}
}