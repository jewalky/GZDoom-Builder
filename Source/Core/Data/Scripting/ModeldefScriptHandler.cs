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
	[ScriptHandler(ScriptType.MODELDEF)]
	internal class ModeldefScriptHandler : ScriptHandler
	{
		#region ================== Methods

		//TODO: Remove ScriptDocumentTab from here
		public override List<CompilerError> UpdateFunctionBarItems(ScriptDocumentTab tab, MemoryStream stream, ComboBox target)
		{
			List<CompilerError> result = new List<CompilerError>();
			if(stream == null) return result;
			target.Items.Clear();

			ModeldefParserSE parser = new ModeldefParserSE();
			TextResourceData data = new TextResourceData(stream, new DataLocation(), "MODELDEF");

			if(parser.Parse(data, false))
				target.Items.AddRange(parser.Models.ToArray());

			if(parser.HasError)
				result.Add(new CompilerError(parser.ErrorDescription, parser.ErrorSource, parser.ErrorLine));

			return result;
		}

		#endregion
	}
}
