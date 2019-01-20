#region ================== Namespaces

using System;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultTexturesMisaligned : ErrorResult
	{

		#region ================== Variables
		
		private readonly Sidedef side1;
		private readonly Sidedef side2;
		private readonly string texturename;
		
		#endregion
		
		#region ================== Properties

		public override int Buttons { get { return 0; } }
		
		#endregion
		
		#region ================== Constructor / Destructor

		public ResultTexturesMisaligned(Sidedef side1, Sidedef side2, string texturename)
		{
			// Initialize
			this.side1 = side1;
			this.side2 = side2;
			this.texturename = texturename;
			viewobjects.Add(side1.Line);
			viewobjects.Add(side2.Line);
			hidden = (side1.IgnoredErrorChecks.Contains(this.GetType()) && side2.IgnoredErrorChecks.Contains(this.GetType()));
			description = "Textures are not aligned on given sidedefs. Some players may not like that.";
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
				side1.IgnoredErrorChecks.Add(t);
				side2.IgnoredErrorChecks.Add(t);
			}
			else 
			{
				if(side1.IgnoredErrorChecks.Contains(t)) side1.IgnoredErrorChecks.Remove(t);
				if(side2.IgnoredErrorChecks.Contains(t)) side2.IgnoredErrorChecks.Remove(t);
			}
		}
		
		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return "Texture \"" + texturename + "\" is not aligned on linedefs " + side1.Line.Index + " (" + (side1.IsFront ? "front" : "back") 
				+ ") and " + side2.Line.Index + " (" + (side2.IsFront ? "front" : "back") + ")";
		}
		
		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotLinedef(side1.Line, General.Colors.Selection);
			renderer.PlotLinedef(side2.Line, General.Colors.Selection);
			renderer.PlotVertex(side1.Line.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(side1.Line.End, ColorCollection.VERTICES);
			renderer.PlotVertex(side2.Line.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(side2.Line.End, ColorCollection.VERTICES);
		}
		
		#endregion
	}
}
