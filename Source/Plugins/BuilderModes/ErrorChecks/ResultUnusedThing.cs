#region ================== Namespaces

using System;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultUnusedThing : ErrorResult
	{
		#region ================== Variables

		private readonly Thing thing;
		private readonly string details;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 2; } }
		public override string Button1Text { get { return "Delete Thing"; } }
		public override string Button2Text { get { return "Apply default flags"; } }

		#endregion

		#region ================== Constructor / Destructor

		public ResultUnusedThing(Thing t, string details) 
		{
			// Initialize
			this.thing = t;
			this.details = details;
			this.viewobjects.Add(t);
			this.hidden = t.IgnoredErrorChecks.Contains(this.GetType());
			this.description = "This thing won't be shown in any game mode.";
		}

		#endregion

		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide) thing.IgnoredErrorChecks.Add(t);
			else if(thing.IgnoredErrorChecks.Contains(t)) thing.IgnoredErrorChecks.Remove(t);
		}

		// This must return the string that is displayed in the listbox
		public override string ToString() 
		{
			return "Thing " + thing.Index + " (" + General.Map.Data.GetThingInfo(thing.Type).Title + ") is unused. " + details;
		}

		// Rendering
		public override void  RenderOverlaySelection(IRenderer2D renderer) 
		{
			renderer.RenderThing(thing, General.Colors.Selection, General.Settings.ActiveThingsAlpha);
		}
		
		// This removes the thing
		public override bool Button1Click(bool batchMode) 
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Delete thing");
			thing.Dispose();
			General.Map.IsChanged = true;
			General.Map.ThingsFilter.Update();
			return true;
		}

		// This sets default flags
		public override bool Button2Click(bool batchMode) 
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Set default thing flags");
			foreach(string f in General.Map.Config.DefaultThingFlags) thing.SetFlag(f, true);
			General.Map.IsChanged = true;
			General.Map.ThingsFilter.Update();
			return true;
		}

		#endregion
	}
}
