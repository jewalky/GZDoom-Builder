#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultUnknownThing : ErrorResult
	{

		#region ================== Variables

		private readonly Thing thing;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 2; } }
		public override string Button1Text { get { return "Edit Thing..."; } }
		public override string Button2Text { get { return "Delete Thing"; } }

		#endregion

		#region ================== Constructor / Destructor

		public ResultUnknownThing(Thing t) 
		{
			// Initialize
			thing = t;
			viewobjects.Add(t);
			hidden = t.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			description = "This thing has unknown type (it's not defined in DECORATE or current game configuration).";
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
			return "Thing " + thing.Index + " has unknown type (" + thing.Type + ").";
		}

		// Rendering
		public override void RenderOverlaySelection(IRenderer2D renderer) 
		{
			renderer.RenderThing(thing, General.Colors.Selection, General.Settings.ActiveThingsAlpha);
		}

		// This edits the thing
		public override bool Button1Click(bool batchMode)
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Edit unknown thing");

			if(General.Interface.ShowEditThings(new List<Thing> { thing }) == DialogResult.OK)
			{
				General.Map.IsChanged = true;
				General.Map.ThingsFilter.Update();
				return true;
			}

			return false;
		}
		
		// This removes the thing
		public override bool Button2Click(bool batchMode) 
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Delete unknown thing");
			thing.Dispose();
			General.Map.IsChanged = true;
			General.Map.ThingsFilter.Update();
			return true;
		}

		#endregion
	}
}
