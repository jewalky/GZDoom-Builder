#region ================== Namespaces

using System;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultUnknownSectorEffect : ErrorResult
	{
		#region ================== Variables

		private readonly Sector sector;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 2; } }
		public override string Button1Text { get { return "Remove Effect"; } }
		public override string Button2Text { get { return "Browse Effect..."; } } //mxd

		#endregion

		#region ================== Constructor
		
		// Constructor
		public ResultUnknownSectorEffect(Sector s)
		{
			// Initialize
			sector = s;
			viewobjects.Add(s);
			hidden = s.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			description = "This sector uses unknown effect. This can potentially cause gameplay issues.";
		}
		
		#endregion
		
		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide)
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide) sector.IgnoredErrorChecks.Add(t);
			else if(sector.IgnoredErrorChecks.Contains(t)) sector.IgnoredErrorChecks.Remove(t);
		}

		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return "Sector " + sector.Index + " uses unknown effect " + sector.Effect;
		}

		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotSector(sector, General.Colors.Selection);
		}

		// Fix by removing effect
		public override bool Button1Click(bool batchmode)
		{
			if(!batchmode) General.Map.UndoRedo.CreateUndo("Unknown sector effect removal");
			sector.Effect = 0;
			General.Map.Map.Update();
			return true;
		}

		// Fix by picking effect
		public override bool Button2Click(bool batchmode)
		{
			if(!batchmode) General.Map.UndoRedo.CreateUndo("Unknown sector effect correction");
			sector.Effect = General.Interface.BrowseSectorEffect(BuilderPlug.Me.ErrorCheckForm, sector.Effect);
			General.Map.Map.Update();
			return true;
		}

		#endregion
	}
}
