#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultUnknownThingScript : ErrorResult
	{
		#region ================== Variables

		private readonly Thing thing;
		private readonly bool namedscript;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 2; } }
		public override string Button1Text { get { return "Edit Thing..."; } }
		public override string Button2Text { get { return "Delete Thing"; } }

		#endregion

		#region ================== Constructor / Destructor

		public ResultUnknownThingScript(Thing t, bool isnamedscript) 
		{
			// Initialize
			thing = t;
			namedscript = isnamedscript;
			viewobjects.Add(t);
			hidden = t.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			description = "This thing references unknown ACS script " + (namedscript ? "name" : "number") + ".";
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
			if(namedscript)
				return "Thing references unknown ACS script name \"" + thing.Fields.GetValue("arg0str", string.Empty) + "\".";

			return "Thing references unknown ACS script number \"" + thing.Args[0] + "\".";
		}

		// Rendering
		public override void RenderOverlaySelection(IRenderer2D renderer) 
		{
			renderer.RenderThing(thing, General.Colors.Selection, General.Settings.ActiveThingsAlpha);
		}

		// This edits the thing
		public override bool Button1Click(bool batchMode)
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Edit thing");

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
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Delete thing");
			thing.Dispose();
			General.Map.IsChanged = true;
			General.Map.ThingsFilter.Update();
			return true;
		}

		#endregion
	}
}
