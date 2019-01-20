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
	[ScriptHandler(ScriptType.DECORATE)]
	internal class DecorateScriptHandler : ScriptHandler
	{
		#region ================== Methods

		//TODO: Remove ScriptDocumentTab from here
		public override List<CompilerError> UpdateFunctionBarItems(ScriptDocumentTab tab, MemoryStream stream, ComboBox target)
		{
			List<CompilerError> result = new List<CompilerError>();
			if(stream == null) return result;
			target.Items.Clear();

			DecorateParserSE parser = new DecorateParserSE();
			TextResourceData data = new TextResourceData(stream, new DataLocation(), "DECORATE");

			if(parser.Parse(data, false))
				target.Items.AddRange(parser.Actors.ToArray());

			if(parser.HasError)
				result.Add(new CompilerError(parser.ErrorDescription, parser.ErrorSource, parser.ErrorLine));

			return result;
		}

		#endregion
	}
}
