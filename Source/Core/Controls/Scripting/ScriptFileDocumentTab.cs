
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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal sealed class ScriptFileDocumentTab : ScriptDocumentTab
	{
		#region ================== Constants

		#endregion
		
		#region ================== Variables
		
		private string filepathname;
		
		#endregion
		
		#region ================== Properties
		
		public override bool IsSaveAsRequired { get { return (filepathname.Length == 0); } }
		public override string Filename { get { return filepathname; } }
		
		#endregion
		
		#region ================== Constructor / Disposer
		
		// Constructor
		public ScriptFileDocumentTab(ScriptEditorPanel panel, ScriptConfiguration config) : base(panel, config)
		{
			string ext = "";
			
			// Initialize
			this.filepathname = "";
			tabtype = ScriptDocumentTabType.FILE; //mxd
			if(config.Extensions.Length > 0) ext = "." + config.Extensions[0];
			SetTitle("Untitled" + ext);
			editor.ClearUndoRedo();
		}

		//mxd. Replace constructor
		internal ScriptFileDocumentTab(ScriptResourceDocumentTab sourcetab)
			: base(sourcetab.Panel, sourcetab.Config)
		{
			// Set text and view settings
			tabtype = ScriptDocumentTabType.FILE;
			editor.Scintilla.Text = sourcetab.Editor.Scintilla.Text;
			SetViewSettings(sourcetab.GetViewSettings());

			// Set title
			SetTitle(sourcetab.Filename);
		}
		
		#endregion
		
		#region ================== Methods
		
		// This compiles the script file
		public override void Compile()
		{
			//mxd. Compile
			List<CompilerError> errors = new List<CompilerError>();
			if(DirectoryReader.CompileScriptLump(filepathname, config, errors))
			{
				//mxd. Update script navigator
				errors.AddRange(UpdateNavigator());
			}

			// Feed errors to panel
			panel.ShowErrors(errors, false);
		}

		// This checks if a script error applies to this script
		public override bool VerifyErrorForScript(CompilerError e)
		{
			return (string.Compare(e.filename, filepathname, true) == 0);
		}
		
		// This saves the document (used for both explicit and implicit)
		// Return true when successfully saved
		public override bool Save()
		{
            // [ZZ] remove trailing whitespace
            RemoveTrailingWhitespace();

            try
			{
				// Write the file
				File.WriteAllBytes(filepathname, editor.GetText());
			}
			catch(Exception e)
			{
				// Failed
				General.ErrorLogger.Add(ErrorType.Error, "Cannot open file \"" + filepathname + "\" for writing. Make sure the path exists and that the file is not in use by another application.");
				General.WriteLogLine(e.GetType().Name + ": " + e.Message);
				General.ShowErrorMessage("Unable to open file \"" + filepathname + "\" for writing. Make sure the path exists and that the file is not in use by another application.", MessageBoxButtons.OK);
				return false;
			}
			
			// Done
			editor.SetSavePoint(); //mxd
			UpdateTitle(); //mxd
			return true;
		}
		
		// This saves the document to a new file
		// Return true when successfully saved
		public override bool SaveAs(string filename)
		{
			string oldfilename = filepathname;
			filepathname = filename;
			if(this.Save())
			{
				SetTitle(Path.GetFileName(filepathname));
				return true;
			}
			else
			{
				this.filepathname = oldfilename;
				return false;
			}
		}
		
		// This opens a file and returns true when successful
		public bool Open(string filepathname)
		{
			try
			{
				// Read the file
				editor.Text = File.ReadAllText(filepathname); //mxd
			}
			catch(Exception e)
			{
				// Failed
				General.ErrorLogger.Add(ErrorType.Error, "Cannot open file \"" + filepathname + "\" for reading. Make sure the path exists and that the file is not in use by another application.");
				General.WriteLogLine(e.GetType().Name + ": " + e.Message);
				General.ShowErrorMessage("Unable to open file \"" + filepathname + "\" for reading. Make sure the path exists and that the file is not in use by another application.", MessageBoxButtons.OK);
				return false;
			}
			
			// Setup
			this.filepathname = filepathname;
			editor.ClearUndoRedo();
			SetTitle(Path.GetFileName(filepathname));
			this.ToolTipText = filepathname; //mxd
			panel.ShowErrors(UpdateNavigator(), true); //mxd

			return true;
		}

		// This changes the script configurations
		public override void ChangeScriptConfig(ScriptConfiguration newconfig)
		{
			if(filepathname.Length == 0)
			{
				string ext = (newconfig.Extensions.Length > 0 ? "." + newconfig.Extensions[0] : "");
				SetTitle("Untitled" + ext);
			}
			else
			{
				UpdateTitle(); //mxd
			}

			//mxd
			base.ChangeScriptConfig(newconfig);
		}
		
		#endregion
		
		#region ================== Events
		
		#endregion
	}
}
