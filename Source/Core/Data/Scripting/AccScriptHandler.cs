#region ================== Namespaces

using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.ZDoom.Scripting;

#endregion

namespace CodeImp.DoomBuilder.Data.Scripting
{
	[ScriptHandler(ScriptType.ACS)]
	internal class AccScriptHandler : ScriptHandler
	{
		#region ================== Methods

		//TODO: Remove ScriptDocumentTab from here
		public override List<CompilerError> UpdateFunctionBarItems(ScriptDocumentTab tab, MemoryStream stream, ComboBox target)
		{
			List<CompilerError> result = new List<CompilerError>();
			if(stream == null) return result;
			target.Items.Clear();

			AcsParserSE parser = new AcsParserSE { AddArgumentsToScriptNames = true, IsMapScriptsLump = tab is ScriptLumpDocumentTab, IgnoreErrors = true };
			DataLocation dl = new DataLocation(DataLocation.RESOURCE_DIRECTORY, Path.GetDirectoryName(string.IsNullOrEmpty(tab.Filename)? tab.Title : tab.Filename), false, false, false);
			TextResourceData data = new TextResourceData(stream, dl, (parser.IsMapScriptsLump ? "?SCRIPTS" : tab.Filename));

			if(parser.Parse(data, false))
			{
				target.Items.AddRange(parser.NamedScripts.ToArray());
				target.Items.AddRange(parser.NumberedScripts.ToArray());
				target.Items.AddRange(parser.Functions.ToArray());
			}

			if(parser.HasError)
				result.Add(new CompilerError(parser.ErrorDescription, parser.ErrorSource, parser.ErrorLine));

			return result;
		}

		#endregion
	}
}
