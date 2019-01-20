#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultInvalidPolyobjectLines : ErrorResult
	{
		#region ================== Variables

		private readonly List<Linedef> lines;
		private readonly string linesinfo;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 0; } }

		#endregion

		#region ================== Constructor / Destructor

		public ResultInvalidPolyobjectLines(List<Linedef> lines, string details) 
		{
			// Initialize
			this.lines = lines;
			this.hidden = true;
			foreach(Linedef l in lines)
			{
				this.viewobjects.Add(l);
				this.hidden &= l.IgnoredErrorChecks.Contains(this.GetType());
			}

			if(lines.Count == 1)
			{
				linesinfo = "Incorrect Polyobject setup for linedef " + lines[0].Index;
			}
			else
			{
				linesinfo = "Incorrect Polyobject setup for linedefs " + lines[0].Index;
				for(int i = 1; i < lines.Count - 1; i++) linesinfo += ", " + lines[i].Index;
				linesinfo += " and " + lines[lines.Count - 1].Index;
			}

			this.description = linesinfo + ": " + details;
		}

		#endregion

		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide)
			{
				foreach(Linedef l in lines)
					l.IgnoredErrorChecks.Add(t);
			}
			else
			{
				foreach(Linedef l in lines)
					if(l.IgnoredErrorChecks.Contains(t)) l.IgnoredErrorChecks.Remove(t);
			}
		}

		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return linesinfo;
		}

		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			foreach(Linedef l in lines)
			{
				renderer.PlotLinedef(l, General.Colors.Selection);
				renderer.PlotVertex(l.Start, ColorCollection.VERTICES);
				renderer.PlotVertex(l.End, ColorCollection.VERTICES);
			}
		}

		#endregion
	}
}
