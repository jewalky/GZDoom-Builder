#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultVertexOverlappingLine : ErrorResult
	{
		#region ================== Variables

		private readonly Linedef line;
		private readonly Vertex vertex;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 1; } }
		public override string Button1Text { get { return "Split Linedef"; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ResultVertexOverlappingLine(Vertex v, Linedef l)
		{
			// Initialize
			line = l;
			vertex = v;
			viewobjects.Add(l);
			viewobjects.Add(v);
			hidden = (l.IgnoredErrorChecks.Contains(this.GetType()) && v.IgnoredErrorChecks.Contains(this.GetType())); //mxd
			description = "This vertex overlaps this linedef without splitting it.";
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
				vertex.IgnoredErrorChecks.Add(t);
				line.IgnoredErrorChecks.Add(t);
			} 
			else 
			{
				if(vertex.IgnoredErrorChecks.Contains(t)) vertex.IgnoredErrorChecks.Remove(t);
				if(line.IgnoredErrorChecks.Contains(t)) line.IgnoredErrorChecks.Remove(t);
			}
		}

		// This must return the string that is displayed in the listbox
		public override string ToString() 
		{
			return "Vertex " + vertex.Index + " overlaps line " + line.Index + " without splitting it";
		}

		// Rendering
		public override void PlotSelection(IRenderer2D renderer) 
		{
			renderer.PlotLinedef(line, General.Colors.Selection);
			renderer.PlotVertex(line.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(line.End, ColorCollection.VERTICES);

			renderer.PlotVertex(vertex, ColorCollection.SELECTION);
		}

		// Fix by splitting the line
		public override bool Button1Click(bool batchMode) 
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Split Linedef");
			line.Split(vertex);

			//check that we don't have duplicate lines
			List<Linedef> lines = new List<Linedef>(vertex.Linedefs);

			for(int i = 0; i < lines.Count - 1; i++) 
			{
				for(int c = i + 1; c < lines.Count; c++) 
				{
					if( (lines[i].Start == lines[c].Start && lines[i].End == lines[c].End) || 
						(lines[i].Start == lines[c].End && lines[i].End == lines[c].Start)) 
					{
						lines[c].Join(lines[i]);

						//mxd. Textures may've become unused
						if(lines[c].Front != null) lines[c].Front.RemoveUnneededTextures(lines[c].Back != null, false, true);
						if(lines[c].Back != null) lines[c].Back.RemoveUnneededTextures(lines[c].Front != null, false, true);
					}
				}
			}

			General.Map.Map.Update();
			return true;
		}
		
		#endregion
	}
}
