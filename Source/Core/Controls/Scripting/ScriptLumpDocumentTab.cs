
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

using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal sealed class ScriptLumpDocumentTab : ScriptDocumentTab
	{
		#region ================== Constants
		
		#endregion

		#region ================== Variables

		private readonly string lumpname;
		private readonly bool ismapheader;
		
		#endregion
		
		#region ================== Properties
		
		public override bool ExplicitSave { get { return false; } }
		public override bool IsSaveAsRequired { get { return false; } }
		public override bool IsClosable { get { return false; } }
		public override bool IsReconfigurable { get { return false; } }
		public override string Filename { get { return lumpname; } } //mxd
		
		#endregion
		
		#region ================== Constructor / Disposer
		
		// Constructor
		public ScriptLumpDocumentTab(ScriptEditorPanel panel, string lumpname, ScriptConfiguration config) : base(panel, config)
		{
			// Initialize
			tabtype = ScriptDocumentTabType.LUMP; //mxd
			if(lumpname == MapManager.CONFIG_MAP_HEADER)
			{
				this.lumpname = MapManager.TEMP_MAP_HEADER;
				this.ismapheader = true;
			}
			else
			{
				this.lumpname = lumpname;
				this.ismapheader = false;
			}
			
			// Load the lump data
			MemoryStream stream = General.Map.GetLumpData(this.lumpname);
			if(stream != null)
			{
				editor.SetText(stream.ToArray()); //mxd
				editor.ClearUndoRedo();
				editor.SetSavePoint();
			}

			// Set title
			SetTitle(ismapheader ? General.Map.Options.CurrentName : this.lumpname.ToUpper());
		}
		
		#endregion
		
		#region ================== Methods
		
		// Compile script
		public override void Compile()
		{
			//mxd. Boilerplate
			if(!General.Map.Config.MapLumps.ContainsKey(lumpname))
			{
				General.ShowErrorMessage("Unable to compile lump \"" + lumpname + "\". This lump is not defined in the current game configuration.", MessageBoxButtons.OK);
				return;
			}

			// Compile
			List<CompilerError> errors = new List<CompilerError>();
			if(General.Map.TemporaryMapFile.CompileLump((ismapheader ? MapManager.CONFIG_MAP_HEADER : lumpname), config, errors))
			{
				//mxd. Update script navigator
				errors.AddRange(UpdateNavigator());
			}

			// Feed errors to panel
			panel.ShowErrors(errors, false);
		}
		
		// Implicit save
		public override bool Save()
		{
			if (!editor.IsChanged) return false;

            // [ZZ] remove trailing whitespace
            RemoveTrailingWhitespace();

			// Store the lump data
			MemoryStream stream = new MemoryStream(editor.GetText());
			General.Map.SetLumpData(lumpname, stream);
			editor.SetSavePoint(); //mxd
			UpdateTitle(); //mxd
			return true;
		}

		// This checks if a script error applies to this script
		public override bool VerifyErrorForScript(CompilerError e)
		{
			return (string.Compare(e.filename, "?" + lumpname, true) == 0);
		}
		
		#endregion
		
		#region ================== Events

		#endregion
	}
}
