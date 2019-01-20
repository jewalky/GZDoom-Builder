#region ================== Namespaces

using System;
using System.Drawing;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultShortLinedef : ErrorResult
	{
		#region ================== Variables
		
		private readonly Linedef line;
		
		#endregion
		
		#region ================== Properties

		public override int Buttons { get { return 0; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ResultShortLinedef(Linedef l)
		{
			// Initialize
			line = l;
			viewobjects.Add(l);
			hidden = l.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			description = "This linedef is shorter than 1 map unit. This can potentially cause nodebuilding errors.";
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
			return "Linedef " + line.Index + " is shorter than 1 mu.";
		}
		
		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotLinedef(line, General.Colors.Selection);
			renderer.PlotVertex(line.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(line.End, ColorCollection.VERTICES);
		}

		// We must zoom in way more than usual...
		public override RectangleF GetZoomArea()
		{
			// Get Area
			RectangleF area = base.GetZoomArea();

			// Remove padding
			area.Inflate(-97f, -97f);

			// Return area
			return area;
		}
		
		#endregion
	}
}