#region ================== Namespaces

using System;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultUnknownLinedefAction : ErrorResult
	{
		#region ================== Variables
		
		private readonly Linedef line;
		
		#endregion
		
		#region ================== Properties

		public override int Buttons { get { return 2; } }
		public override string Button1Text { get { return "Remove Action"; } }
		public override string Button2Text { get { return "Browse Action..."; } } //mxd
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ResultUnknownLinedefAction(Linedef l)
		{
			// Initialize
			line = l;
			viewobjects.Add(l);
			hidden = l.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			description = "This linedef uses unknown action. This can potentially cause gameplay issues.";
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
			return "Linedef " + line.Index + " uses unknown action " + line.Action;
		}
		
		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotLinedef(line, General.Colors.Selection);
			renderer.PlotVertex(line.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(line.End, ColorCollection.VERTICES);
		}

		// Fix by removing action
		public override bool Button1Click(bool batchmode)
		{
			if(!batchmode) General.Map.UndoRedo.CreateUndo("Unknown linedef action removal");
			line.Action = 0;
			General.Map.Map.Update();
			return true;
		}

		// Fix by picking action
		public override bool Button2Click(bool batchmode)
		{
			if(!batchmode) General.Map.UndoRedo.CreateUndo("Unknown linedef action correction");
			line.Action = General.Interface.BrowseLinedefActions(BuilderPlug.Me.ErrorCheckForm, line.Action);
			General.Map.Map.Update();
			return true;
		}
		
		#endregion
	}
}
