#region ================== Namespaces

using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Data.Scripting;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	//mxd. Document tab bound to a resource entry. Script type can't be changed. Can be readonly.
	//Must be replaced with ScriptFileDocumentTab when unable to locate target resource entry to save to.
	internal sealed class ScriptResourceDocumentTab : ScriptDocumentTab
	{
		#region ================== Variables

		private ScriptResource source;
		private string hash;
		private string filepathname;

		#endregion

		#region ================== Properties

		public override bool IsReconfigurable { get { return false; } }
		public override bool IsSaveAsRequired { get { return false; } }
		public override string Filename { get { return filepathname; } }
		internal ScriptResource Resource { get { return source; } }

		#endregion

		#region ================== Constructor

		internal ScriptResourceDocumentTab(ScriptEditorPanel panel, ScriptResource resource, ScriptConfiguration config) : base(panel, config)
		{
			// Store resource
			source = resource;

			// Load the data
			MemoryStream stream = source.Resource.LoadFile(source.Filename, source.LumpIndex);
			if(stream != null)
			{
				hash = MD5Hash.Get(stream);
				editor.SetText(stream.ToArray());
				editor.Scintilla.ReadOnly = source.IsReadOnly;
				editor.ClearUndoRedo();
				editor.SetSavePoint();
			}
			else
			{
				General.ErrorLogger.Add(ErrorType.Warning, "Failed to load " + source.ScriptType + " resource \"" + source.Filename + "\" from \"" + source.Resource.Location.GetDisplayName() + "\".");
			}

			// Set title and tooltip
			tabtype = ScriptDocumentTabType.RESOURCE;
			filepathname = source.FilePathName;
			SetTitle(source.ToString());
			this.ToolTipText = filepathname;

			// Update navigator
			panel.ShowErrors(UpdateNavigator(), true);
		}

		#endregion

		#region ================== Methods

		public override void Compile()
		{
			List<CompilerError> errors = new List<CompilerError>();
			DataReader reader = source.Resource;
			if(reader != null && reader.CompileLump(source.Filename, source.LumpIndex, config, errors))
			{
				// Update script navigator
				errors.AddRange(UpdateNavigator());
			}

			// Feed errors to panel
			panel.ShowErrors(errors, false);
		}

		// This checks if a script error applies to this script
		public override bool VerifyErrorForScript(CompilerError e)
		{
			return (string.Compare(e.filename, source.Filename, true) == 0);
		}

		// This saves the document (used for both explicit and implicit)
		// Return true when successfully saved
		public override bool Save()
		{
			if(source.IsReadOnly || !editor.IsChanged) return false;

            // [ZZ] remove trailing whitespace
            RemoveTrailingWhitespace();

            // Find lump, check it's hash
            bool dosave = true;
			DataReader reader = source.Resource;
            // reload the reader
            if (reader.FileExists(source.Filename, source.LumpIndex))
            {
                using (MemoryStream ms = reader.LoadFile(source.Filename, source.LumpIndex))
                {
                    if (MD5Hash.Get(ms) != hash
                        && MessageBox.Show("Target lump was modified by another application. Do you still want to replace it?", "Warning", MessageBoxButtons.OKCancel)
                        == DialogResult.Cancel)
                    {
                        dosave = false;
                    }
                }
            }

            if (dosave)
            {
                // Store the lump data
                using (MemoryStream stream = new MemoryStream(editor.GetText()))
                {
                    if (reader.SaveFile(stream, source.Filename, source.LumpIndex))
                    {
                        // Update what must be updated
                        hash = MD5Hash.Get(stream);
                        editor.SetSavePoint();
                        UpdateTitle();
                    }
                }
            }

			return dosave;
		}

		internal override ScriptDocumentSettings GetViewSettings()
		{
			// Store resource location
			var settings = base.GetViewSettings();
			DataReader reader = source.Resource;
			if(reader != null)
			{
				settings.ResourceLocation = reader.Location.location;
				settings.Filename = Path.Combine(reader.Location.location, filepathname); // Make unique location
			}
			return settings;
		}

		//mxd. Check if resource still exists
		internal void OnReloadResources()
		{
			DataReader reader = source.Resource;
			if(reader == null)
			{
				// Ask script editor to replace us with ScriptFileDocumentTab
				panel.OnScriptResourceLost(this);
			}
			else
			{
				// Some paths may need updating...
				filepathname = source.FilePathName;
				this.ToolTipText = filepathname;
			}
		}

		#endregion
	}
}
