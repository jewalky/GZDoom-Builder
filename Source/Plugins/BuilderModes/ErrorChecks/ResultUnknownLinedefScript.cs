#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultUnknownLinedefScript : ErrorResult
	{
		#region ================== Variables

		private readonly Linedef line;
		private readonly bool namedscript;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 1; } }
		public override string Button1Text { get { return "Edit Linedef..."; } }

		#endregion

		#region ================== Constructor / Destructor

		public ResultUnknownLinedefScript(Linedef l, bool isnamedscript) 
		{
			// Initialize
			line = l;
			namedscript = isnamedscript;
			viewobjects.Add(l);
			hidden = l.IgnoredErrorChecks.Contains(this.GetType());
			description = "This linedef references unknown ACS script " + (namedscript ? "name" : "number") + ".";
		}

		#endregion

		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide) line.IgnoredErrorChecks.Add(t);
			else if(line.IgnoredErrorChecks.Contains(t)) line.IgnoredErrorChecks.Remove(t);
		}

		// This must return the string that is displayed in the listbox
		public override string ToString() 
		{
			if(namedscript)
				return "Linedef references unknown ACS script name \"" + line.Fields.GetValue("arg0str", string.Empty) + "\".";

			return "Linedef references unknown ACS script number \"" + line.Args[0] + "\".";
		}

		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotLinedef(line, General.Colors.Selection);
			renderer.PlotVertex(line.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(line.End, ColorCollection.VERTICES);
		}
		
		// This edits the linedef
		public override bool Button1Click(bool batchMode)
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Edit linedef");

			if(General.Interface.ShowEditLinedefs(new List<Linedef> { line }) == DialogResult.OK)
			{
				General.Map.Map.Update();
				return true;
			}

			return false;
		}

		#endregion
	}
}
